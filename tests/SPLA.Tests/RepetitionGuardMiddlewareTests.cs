using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Agent.Guards;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Covers the guard's behaviour as a pipeline stage: that it abandons a looping generation, that it
/// allows exactly one retry, that it watches BOTH output channels (the old guard watched only the
/// visible answer, which is why loops in reasoning ran to the token ceiling unnoticed), and — the
/// subtle one — that it never mistakes the user pressing stop for its own abort.
/// </summary>
public class RepetitionGuardMiddlewareTests
{
    /// <summary>A unit with no internal repetition, so its only valid period is its own length.</summary>
    private const string Unit = "abcdefghij0123456789";

    /// <summary>Long enough to clear MinTailChars (300) and MinRepeats (6) several times over.</summary>
    private static string Looping => string.Concat(Enumerable.Repeat(Unit, 20));

    private static LlmTurnContext Context() => new()
    {
        Messages = new List<ChatMessage> { new() { Role = ChatRole.User, Content = "go" } },
        Settings = new LLMSettings()
    };

    /// <summary>A stand-in provider: streams its scripted text in small pieces through the context's
    /// sinks, checking the token between them exactly as a real socket read loop does.</summary>
    private static async Task<LlmTurnResult> StreamAsync(
        LlmTurnContext ctx, CancellationToken ct, string? content, string? reasoning = null)
    {
        foreach (var piece in Pieces(reasoning))
        {
            ct.ThrowIfCancellationRequested();
            if (ctx.OnReasoning != null) await ctx.OnReasoning(piece);
        }
        foreach (var piece in Pieces(content))
        {
            ct.ThrowIfCancellationRequested();
            if (ctx.OnDelta != null) await ctx.OnDelta(piece);
        }
        ct.ThrowIfCancellationRequested();

        return new LlmTurnResult
        {
            Message = new ChatMessage { Role = ChatRole.Assistant, Content = content, Reasoning = reasoning }
        };
    }

    private static IEnumerable<string> Pieces(string? text, int size = 5)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        for (var i = 0; i < text.Length; i += size)
            yield return text.Substring(i, Math.Min(size, text.Length - i));
    }

    [Fact]
    public async Task A_looping_generation_is_abandoned_and_retried_once()
    {
        var scripts = new[] { Looping, "a perfectly ordinary answer" };
        var calls = 0;

        var result = await new RepetitionGuardMiddleware().InvokeAsync(
            Context(),
            (c, t) => StreamAsync(c, t, scripts[calls++]),
            CancellationToken.None);

        Assert.Equal(2, calls);
        Assert.Equal(LlmTurnStatus.Ok, result.Status);
        Assert.Equal("a perfectly ordinary answer", result.Message.Content);
    }

    [Fact]
    public async Task Looping_on_every_attempt_ends_the_call_as_degenerate()
    {
        var calls = 0;

        var result = await new RepetitionGuardMiddleware().InvokeAsync(
            Context(),
            (c, t) => { calls++; return StreamAsync(c, t, Looping); },
            CancellationToken.None);

        // Exactly MaxAttempts, not "until it works": two loops at the same point mean the model is
        // stuck on the task, and every further attempt is paid for.
        Assert.Equal(2, calls);
        Assert.Equal(LlmTurnStatus.Degenerate, result.Status);
        // The abandoned text survives on the message — the provider's own buffers died with the
        // cancelled read, so the observer's copy is the only one left.
        Assert.Contains(Unit, result.Message.Content);
    }

    [Fact]
    public async Task A_loop_in_reasoning_is_caught_too()
    {
        // The case that motivated the whole guard: the model loops while thinking, and the answer
        // channel stays empty, so a guard watching only the visible text sees nothing at all.
        var calls = 0;

        var result = await new RepetitionGuardMiddleware().InvokeAsync(
            Context(),
            (c, t) => { calls++; return StreamAsync(c, t, content: null, reasoning: Looping); },
            CancellationToken.None);

        Assert.Equal(2, calls);
        Assert.Equal(LlmTurnStatus.Degenerate, result.Status);
        Assert.Contains(Unit, result.Message.Reasoning);
    }

    [Fact]
    public async Task It_watches_even_when_nobody_is_listening()
    {
        // A headless run (CLI, a spawned sub-agent, the librarian) supplies no sinks. The guard must
        // still attach its own, or protection would depend on whether a UI happened to be watching.
        var ctx = Context();
        ctx.OnDelta = null;
        ctx.OnReasoning = null;
        var calls = 0;

        var result = await new RepetitionGuardMiddleware().InvokeAsync(
            ctx, (c, t) => { calls++; return StreamAsync(c, t, Looping); }, CancellationToken.None);

        Assert.Equal(2, calls);
        Assert.Equal(LlmTurnStatus.Degenerate, result.Status);
    }

    [Fact]
    public async Task The_caller_still_receives_every_chunk()
    {
        var seen = new List<string>();
        var ctx = Context();
        ctx.OnDelta = chunk => { seen.Add(chunk); return Task.CompletedTask; };

        await new RepetitionGuardMiddleware().InvokeAsync(
            ctx, (c, t) => StreamAsync(c, t, "hello there"), CancellationToken.None);

        Assert.Equal("hello there", string.Concat(seen));
        // And the context is handed back exactly as it came: the guard's wrapper does not outlive it.
        Assert.NotNull(ctx.OnDelta);
        seen.Clear();
        await ctx.OnDelta!("x");
        Assert.Equal(new[] { "x" }, seen);
    }

    [Fact]
    public async Task One_abandoned_generation_reports_exactly_one_attempt()
    {
        var scripts = new[] { Looping, "a perfectly ordinary answer" };
        var calls = 0;
        var reported = new List<GenerationAttempt>();
        var ctx = Context();
        ctx.OnAttempt = a => reported.Add(a);

        await new RepetitionGuardMiddleware().InvokeAsync(
            ctx, (c, t) => StreamAsync(c, t, scripts[calls++]), CancellationToken.None);

        var attempt = Assert.Single(reported);
        Assert.Equal(1, attempt.Index);
        Assert.Equal(AttemptOutcome.Repetition, attempt.Outcome);
        Assert.False(string.IsNullOrEmpty(attempt.Note));
    }

    [Fact]
    public async Task A_call_that_loops_every_time_reports_one_attempt_per_try()
    {
        var reported = new List<GenerationAttempt>();
        var ctx = Context();
        ctx.OnAttempt = a => reported.Add(a);

        await new RepetitionGuardMiddleware().InvokeAsync(
            ctx, (c, t) => StreamAsync(c, t, Looping), CancellationToken.None);

        Assert.Equal(2, reported.Count); // MaxAttempts
        Assert.Equal(new[] { 1, 2 }, reported.Select(a => a.Index));
        Assert.All(reported, a => Assert.False(string.IsNullOrEmpty(a.Note)));
    }

    [Fact]
    public async Task A_clean_generation_reports_no_attempts()
    {
        var reported = new List<GenerationAttempt>();
        var ctx = Context();
        ctx.OnAttempt = a => reported.Add(a);

        var result = await new RepetitionGuardMiddleware().InvokeAsync(
            ctx, (c, t) => StreamAsync(c, t, "hello there"), CancellationToken.None);

        Assert.Equal(LlmTurnStatus.Ok, result.Status);
        Assert.Empty(reported);
    }

    [Fact]
    public async Task The_user_pressing_stop_is_not_a_reason_to_try_again()
    {
        // Both cancellations surface as the same exception type. Confusing them would mean a guard
        // that restarts a generation the human just killed — the failure this discrimination exists
        // to prevent. Here the caller's own sink cancels, mid-stream, before any loop could form.
        using var user = new CancellationTokenSource();
        var ctx = Context();
        ctx.OnDelta = _ => { user.Cancel(); return Task.CompletedTask; };
        var calls = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new RepetitionGuardMiddleware().InvokeAsync(
                ctx, (c, t) => { calls++; return StreamAsync(c, t, Looping); }, user.Token));

        Assert.Equal(1, calls);
    }
}
