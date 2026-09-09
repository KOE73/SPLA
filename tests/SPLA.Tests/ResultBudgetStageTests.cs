using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.Domain.Host;
using SPLA.MCP.Core.Pipeline;
using SPLA.MCP.Core.Pipeline.Stages;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The turn that died: a ten-minute <c>ssh_session_wait</c> returned 460 000 characters, the request
/// came to 396 067 tokens against a 98 304 window, and the endpoint refused it. These tests pin the
/// four answers the Post link may give — and, just as importantly, that it gives the first two
/// (pass through) far more often than the last two.
/// </summary>
public sealed class ResultBudgetStageTests
{
    private static readonly ToolCallInvocation Call = new(AgentMode.Agent, "ssh_session_wait", "{}");

    private static Task<ToolResult> Run(ToolResult result, IAgentSession? session)
    {
        var stage = new ResultBudgetStage();
        if (session is null) return stage.InvokeAsync(Call, (_, _) => Task.FromResult(result), default);
        using var scope = AgentSessionScope.Begin(session);
        return stage.InvokeAsync(Call, (_, _) => Task.FromResult(result), default);
    }

    [Fact]
    public async Task Bulk_is_cut_even_when_nothing_is_known_about_the_model()
    {
        // The standing ceiling, not a calculation. "Unknown" is the normal state in a CLI run, a
        // spawned agent, and the first turn of every chat — it cannot be the case that passes.
        var session = new FakeSession(budget: null);

        var result = await Run(ToolResult.Text(new string('x', 500_000)), session);

        Assert.True(result.TextContent.Length < 100_000);
        Assert.Single(session.Blobs.List());
    }

    [Fact]
    public async Task A_large_window_does_not_buy_a_larger_result()
    {
        // The budget may only LOWER the ceiling. A roomy window is not a reason to hand the model
        // more text than it can use.
        var roomy = new FakeSession(new ContextBudget(1_000_000, 1_000));
        var blind = new FakeSession(budget: null);

        var withRoom = await Run(ToolResult.Text(new string('x', 500_000)), roomy);
        var withNothing = await Run(ToolResult.Text(new string('x', 500_000)), blind);

        Assert.Equal(withNothing.TextContent.Length, withRoom.TextContent.Length);
    }

    [Fact]
    public async Task A_tight_window_cuts_harder_than_the_standing_ceiling()
    {
        var roomy = new FakeSession(new ContextBudget(1_000_000, 1_000));
        var tight = new FakeSession(new ContextBudget(8_192, 6_000));

        var loose = await Run(ToolResult.Text(new string('x', 500_000)), roomy);
        var strict = await Run(ToolResult.Text(new string('x', 500_000)), tight);

        Assert.True(strict.TextContent.Length < loose.TextContent.Length);
    }

    [Fact]
    public async Task No_session_at_all_still_cuts_but_says_the_rest_is_gone()
    {
        var result = await Run(ToolResult.Text(new string('x', 500_000)), session: null);

        Assert.True(result.TextContent.Length < 100_000);
        Assert.Empty(result.Content.OfType<ToolResource>());
        Assert.Contains("could not be stored", result.TextContent);
    }

    [Fact]
    public async Task A_result_that_fits_is_returned_untouched()
    {
        var original = ToolResult.Text("exit: 0\nall done");

        var result = await Run(original, new FakeSession(new ContextBudget(200_000, 10_000)));

        Assert.Same(original, result);
    }

    [Fact]
    public async Task An_ordinary_sized_result_is_untouched_with_no_budget_either()
    {
        // The common case must stay free: a link that rewrites every small result will eventually
        // rewrite the wrong one.
        var original = ToolResult.Text(new string('x', 20_000));

        var result = await Run(original, new FakeSession(budget: null));

        Assert.Same(original, result);
    }

    [Fact]
    public async Task A_result_that_cannot_fit_keeps_its_tail_and_spills_the_rest_to_a_blob()
    {
        var lines = Enumerable.Range(0, 60_000).Select(i => $"48dd761b6bfe: Downloading {i}MB/113.2MB");
        var text = string.Join("\n", lines) + "\nexit: 0";
        var session = new FakeSession(new ContextBudget(98_304, 90_000));

        var result = await Run(ToolResult.Text(text), session);

        // The end of the output is where the answer is.
        Assert.Contains("exit: 0", result.TextContent);
        Assert.Contains("lines cut", result.TextContent);
        Assert.True(result.TextContent.Length < text.Length / 4);

        // Stored WITHOUT having been asked: by the time anyone knows the output was too big, the
        // call that could have carried an output='blob' flag has already happened.
        var stored = Assert.Single(session.Blobs.List());
        Assert.Equal(text, session.Blobs.Get(stored.Handle)!.Text);

        // And the handle is a resource on the result, not just a sentence in the prose.
        var resource = Assert.Single(result.Content.OfType<ToolResource>());
        Assert.Equal(stored.Handle, resource.Uri);
        Assert.Contains(stored.Handle, result.TextContent);

        // The note must name the way back in. "The rest is at blob:…" is only true if the rest can
        // actually be reached, and blob_peek's 2 KB window at a guessed offset cannot reach it.
        Assert.Contains("blob_grep", result.TextContent);
    }

    [Fact]
    public async Task Trimming_does_not_turn_a_call_into_a_failure()
    {
        var text = new string('x', 500_000);
        var failed = ToolResult.Fail(text, "session error");

        var result = await Run(failed, new FakeSession(new ContextBudget(98_304, 90_000)));

        Assert.Equal(ToolOutcome.Failed, result.Outcome);
        Assert.Equal("session error", result.Reason);
    }

    [Fact]
    public async Task A_tool_that_already_routed_to_the_data_channel_is_left_alone()
    {
        // The tool made its own arrangement; trimming here would trim a summary.
        var routed = new ToolResult
        {
            Content =
            [
                new ToolText(new string('x', 500_000)),
                new ToolResource("blob:abc", "text/plain", "the full thing")
            ]
        };

        var result = await Run(routed, new FakeSession(new ContextBudget(98_304, 90_000)));

        Assert.Same(routed, result);
    }

    [Fact]
    public async Task Non_text_content_survives_the_trim()
    {
        var withImage = new ToolResult
        {
            Content = [new ToolText(new string('x', 500_000)), new ToolImage("AAAA", "image/png")]
        };

        var result = await Run(withImage, new FakeSession(new ContextBudget(98_304, 90_000)));

        var image = Assert.Single(result.Content.OfType<ToolImage>());
        Assert.Equal("AAAA", image.Data);
    }

    [Fact]
    public void The_link_sits_on_Post()
        => Assert.Equal(ToolPipelineStage.Post, new ResultBudgetStage().Stage);

    private sealed class FakeSession(ContextBudget? budget) : IAgentSession, IContextBudgetHost
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
        public IContextBudgetHost? ContextBudget => this;
        public string? ChatId => "chat-1";
        public ContextBudget? Budget { get; } = budget;
    }
}
