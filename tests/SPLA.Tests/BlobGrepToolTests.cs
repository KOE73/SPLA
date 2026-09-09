using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Tools;
using System.Text.Json;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The half of the data channel that was missing. A blob that can only be read blind — a 2 KB window
/// at an offset the reader has to guess — is a grave, not a store; and the moment over-long tool
/// results started spilling into blobs automatically, "the rest is at blob:…" became a promise that
/// had to be keepable.
/// </summary>
public sealed class BlobGrepToolTests
{
    private static string Args(object o) => JsonSerializer.Serialize(o);

    private static async Task<(ToolResult Result, FakeSession Session)> Grep(string text, object args)
    {
        var session = new FakeSession();
        var handle = session.Blobs.Put(BlobPayload.OfText(text, "text/plain"));
        using var scope = AgentSessionScope.Begin(session);

        var json = JsonSerializer.Serialize(args);
        var merged = json.Insert(1, $"\"handle\":\"{handle}\",");
        var result = await new BlobGrepTool().ExecuteAsync(merged);
        return (result, session);
    }

    private static string Log =>
        string.Join("\n", Enumerable.Range(1, 5000).Select(i =>
            i == 4200 ? "ERROR: failed to pull mariadb:11" : $"48dd761b6bfe: Downloading {i}MB/113.2MB"));

    [Fact]
    public async Task Finds_the_one_line_that_matters_in_a_large_log()
    {
        var (result, _) = await Grep(Log, new { pattern = "ERROR" });

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        Assert.Contains("failed to pull mariadb:11", result.TextContent);
        Assert.Contains("4200:", result.TextContent); // line number, grep-style
    }

    [Fact]
    public async Task Context_lines_are_marked_apart_from_the_match()
    {
        var (result, _) = await Grep(Log, new { pattern = "ERROR", context = 2 });

        Assert.Contains("4200:ERROR", result.TextContent);
        Assert.Contains("4199-", result.TextContent);
        Assert.Contains("4201-", result.TextContent);
    }

    [Fact]
    public async Task A_regex_that_is_meant_literally_can_say_so()
    {
        var (asRegex, _) = await Grep("a.c\nabc", new { pattern = "a.c" });
        Assert.Contains("abc", asRegex.TextContent);

        var (asText, _) = await Grep("a.c\nabc", new { pattern = "a.c", fixedString = true });
        Assert.Contains("a.c", asText.TextContent);
        Assert.DoesNotContain("2:abc", asText.TextContent);
    }

    [Fact]
    public async Task Nothing_matched_is_an_answer_not_a_failure()
    {
        var (result, _) = await Grep(Log, new { pattern = "no such thing anywhere" });

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        Assert.Contains("Nothing matched", result.TextContent);
    }

    [Fact]
    public async Task Stopping_early_is_stated_so_it_is_not_read_as_completeness()
    {
        // "Not there" and "stopped looking" are different facts, and a reader that cannot tell them
        // apart will conclude the wrong one.
        var (result, _) = await Grep(Log, new { pattern = "Downloading", maxMatches = 5 });

        Assert.Contains("stopped at maxMatches=5", result.TextContent);
    }

    [Fact]
    public async Task The_answer_is_bounded_however_many_lines_match()
    {
        // A search tool that can return the whole blob is the flood the data channel exists to
        // prevent, arriving by a different door. The character ceiling — not the match cap — is what
        // holds here: 200 long lines are past it long before the 200th match.
        var wide = string.Join("\n", Enumerable.Range(1, 1000).Select(i => $"row {i}: " + new string('y', 300)));

        var (result, _) = await Grep(wide, new { pattern = "row", maxMatches = 200 });

        Assert.True(result.TextContent.Length <= 21_000, $"answer was {result.TextContent.Length} chars");
        Assert.Contains("truncated", result.TextContent);
    }

    [Fact]
    public async Task Invert_returns_the_lines_that_did_not_match()
    {
        var (result, _) = await Grep("keep\ndrop\nkeep", new { pattern = "drop", invert = true });

        Assert.Contains("1:keep", result.TextContent);
        Assert.DoesNotContain("2:drop", result.TextContent);
    }

    [Fact]
    public async Task A_broken_pattern_says_what_to_do_instead()
    {
        var (result, _) = await Grep("anything", new { pattern = "([unclosed" });

        Assert.Equal(ToolOutcome.Failed, result.Outcome);
        Assert.Contains("fixedString=true", result.TextContent);
    }

    [Fact]
    public async Task A_binary_blob_is_refused_with_the_tool_that_does_work_on_it()
    {
        var session = new FakeSession();
        var handle = session.Blobs.Put(BlobPayload.OfBytes([1, 2, 3], "application/octet-stream"));
        using var scope = AgentSessionScope.Begin(session);

        var result = await new BlobGrepTool().ExecuteAsync(Args(new { handle, pattern = "x" }));

        Assert.Equal(ToolOutcome.Failed, result.Outcome);
        Assert.Contains("blob_peek", result.TextContent);
    }

    [Fact]
    public async Task An_unknown_handle_fails_rather_than_returning_nothing_found()
    {
        var session = new FakeSession();
        using var scope = AgentSessionScope.Begin(session);

        var result = await new BlobGrepTool().ExecuteAsync(Args(new { handle = "blob:nope", pattern = "x" }));

        Assert.Equal(ToolOutcome.Failed, result.Outcome);
        Assert.Contains("no blob found", result.TextContent);
    }

    internal sealed class FakeSession : IAgentSession
    {
        public IKeyValueStore SessionKv => throw new NotSupportedException();
        public IBlobStore Blobs { get; } = new BlobStore();
        public MarkManager Checkpoint => throw new NotSupportedException();
        public ISkillSession Skills => throw new NotSupportedException();
        public IToolSetSession ToolSets => throw new NotSupportedException();
        public ISandbox Sandbox => throw new NotSupportedException();
        public SPLA.Domain.Security.ChatDoubt Doubt { get; } = new();
        public IBackgroundTaskHost? Background => null;
        public ICorrespondenceHost? Correspondence => null;
        public IContextBudgetHost? ContextBudget => null;
        public string? ChatId => "chat-1";
    }
}
