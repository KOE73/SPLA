using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// <c>blob_grep</c> — finds lines in a stored text blob and shows them with their neighbours.
///
/// <para>
/// The half of the data channel that was missing. <see cref="BlobPeekTool"/> can show a slice at an
/// offset, which answers "what IS this" and nothing else: to find a line in a half-megabyte log
/// through a 2 KB window, a reader would have to guess where it is, two hundred and fifty times.
/// A store that can only be read blind is a grave, and a blob nobody can search is a polite way of
/// throwing data away.
/// </para>
/// <para>
/// That became urgent when <c>ResultBudgetStage</c> started spilling over-long tool results into the
/// blob store <i>without being asked</i>, telling the model "the rest is at blob:…". That sentence
/// is only true if the rest can actually be reached.
/// </para>
/// <para>
/// Bounded on purpose, like its neighbour: a match cap, a context cap, and a hard character budget
/// on the whole answer. A search tool that can return the entire blob is the flood the data channel
/// exists to prevent, arriving by a different door.
/// </para>
/// </summary>
public sealed class BlobGrepTool : IMcpTool
{
    private const int DefaultMaxMatches = 40;
    private const int MaxMaxMatches = 200;
    private const int MaxContextLines = 20;

    /// <summary>The whole answer's ceiling. Reached before the match cap when lines are long — a
    /// blob of minified JSON has few lines and each is enormous.</summary>
    private const int MaxAnswerChars = 20_000;

    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(2);

    public string Name => "blob_grep";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Searches a stored TEXT blob (by blob: handle) and returns the matching lines with their line " +
                "numbers and optional surrounding lines — the way grep does. This is how you read a large result " +
                "that was spilled to a blob: search it for what you are actually looking for (an error, a name, " +
                "an exit code) instead of paging through it. The pattern is a regular expression by default; pass " +
                "fixedString=true to search for it literally. Bounded: capped number of matches and a ceiling on the " +
                "answer, so it never floods context. Binary blobs cannot be searched — use blob_peek for a hex dump.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    handle = new { type = "string", description = "The blob: handle to search." },
                    pattern = new { type = "string", description = "What to look for. A .NET regular expression unless 'fixedString' is true." },
                    fixedString = new { type = new[] { "boolean", "null" }, description = "Treat 'pattern' as literal text rather than a regex. Default false." },
                    ignoreCase = new { type = new[] { "boolean", "null" }, description = "Case-insensitive search. Default true." },
                    context = new { type = new[] { "integer", "null" }, description = $"Lines of context to show on each side of a match (0–{MaxContextLines}). Default 0." },
                    maxMatches = new { type = new[] { "integer", "null" }, description = $"Stop after this many matches. Default {DefaultMaxMatches}, capped at {MaxMaxMatches}." },
                    invert = new { type = new[] { "boolean", "null" }, description = "Return the lines that do NOT match. Default false." }
                },
                required = new[] { "handle", "pattern" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? handle, pattern;
        bool literal, ignoreCase, invert;
        int context, maxMatches;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            handle = ToolJson.GetStringTrimmed(doc.RootElement, "handle");
            pattern = ToolJson.GetString(doc.RootElement, "pattern");
            literal = ToolJson.GetBoolean(doc.RootElement, "fixedString", false);
            ignoreCase = ToolJson.GetBoolean(doc.RootElement, "ignoreCase", true);
            invert = ToolJson.GetBoolean(doc.RootElement, "invert", false);
            context = Math.Clamp(ToolJson.GetInt32(doc.RootElement, "context", 0), 0, MaxContextLines);
            maxMatches = Math.Clamp(ToolJson.GetInt32(doc.RootElement, "maxMatches", DefaultMaxMatches), 1, MaxMaxMatches);
        }
        catch (JsonException) { return Task.FromResult(ToolResult.Fail("error: invalid_json", "invalid json")); }

        if (string.IsNullOrEmpty(handle)) return Task.FromResult(ToolResult.Fail("error: 'handle' is required", "missing handle"));
        if (string.IsNullOrEmpty(pattern)) return Task.FromResult(ToolResult.Fail("error: 'pattern' is required", "missing pattern"));

        var session = AgentSessionScope.Current;
        if (session is null) return Task.FromResult(ToolResult.Refuse("error: no active chat session", "no chat session"));

        var payload = session.Blobs.Get(handle);
        if (payload is null) return Task.FromResult(ToolResult.Fail($"error: no blob found for handle '{handle}'", "unknown handle"));
        if (payload.Kind != BlobKind.Text)
            return Task.FromResult(ToolResult.Fail(
                $"error: blob '{handle}' is binary ({payload.ContentType}) — there are no lines to search. Use blob_peek for a hex dump.",
                "binary blob"));

        Regex rx;
        try
        {
            var options = RegexOptions.CultureInvariant | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            rx = new Regex(literal ? Regex.Escape(pattern) : pattern, options, MatchTimeout);
        }
        catch (ArgumentException ex)
        {
            return Task.FromResult(ToolResult.Fail(
                $"error: invalid regex: {ex.Message}. Pass fixedString=true to search for it as literal text.",
                "invalid regex"));
        }

        try
        {
            return Task.FromResult(Search(handle, payload.Text ?? "", rx, context, maxMatches, invert));
        }
        catch (RegexMatchTimeoutException)
        {
            // Said plainly rather than reported as a generic failure: the pattern is the thing to
            // change, and only the caller can change it.
            return Task.FromResult(ToolResult.Fail(
                "error: the pattern took too long against this blob — it is probably backtracking. Simplify it, " +
                "or pass fixedString=true.",
                "regex timeout"));
        }
    }

    private static ToolResult Search(string handle, string text, Regex rx, int context, int maxMatches, bool invert)
    {
        var lines = text.Split('\n');
        var hits = new List<int>();
        for (var i = 0; i < lines.Length && hits.Count < maxMatches; i++)
            if (rx.IsMatch(lines[i]) != invert) hits.Add(i);

        // Whether anything was skipped is the difference between "not there" and "stopped looking",
        // and a reader that cannot tell them apart will conclude the wrong one.
        var scannedAll = hits.Count < maxMatches;
        var header = $"{handle} — {lines.Length} lines, {(hits.Count == 0 ? "no matches" : $"{hits.Count} match(es)")}" +
                     (scannedAll ? "" : $" (stopped at maxMatches={maxMatches}; there may be more)");

        if (hits.Count == 0)
            return ToolResult.Text(header + "\nNothing matched. The blob is intact — try a looser pattern, or blob_peek to see what it looks like.");

        var sb = new StringBuilder(header).Append('\n');
        var lastPrinted = -1;
        var truncated = false;

        foreach (var hit in hits)
        {
            var from = Math.Max(0, hit - context);
            var to = Math.Min(lines.Length - 1, hit + context);
            if (from > lastPrinted + 1 && lastPrinted >= 0) sb.Append("--\n");
            for (var i = Math.Max(from, lastPrinted + 1); i <= to; i++)
            {
                var line = lines[i].TrimEnd('\r');
                // Marked with ':' for a match and '-' for context, exactly as grep does — the reader
                // already knows how to read that, and inventing a second convention teaches nothing.
                var rendered = $"{i + 1}{(i == hit ? ':' : '-')}{line}\n";
                if (sb.Length + rendered.Length > MaxAnswerChars) { truncated = true; break; }
                sb.Append(rendered);
                lastPrinted = i;
            }
            if (truncated) break;
        }

        if (truncated)
            sb.Append("…[answer truncated at ").Append(MaxAnswerChars)
              .Append(" characters — narrow the pattern, lower 'context', or lower 'maxMatches']…\n");

        return ToolResult.Text(sb.ToString().TrimEnd());
    }
}
