using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Llm;
using SPLA.Domain.Llm.Middleware;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Covers the layer that waits out a rate limit: that it retries the one refusal it is meant to,
/// that a stated <c>Retry-After</c> overrides the schedule instead of being clamped by it, and — the
/// three that protect existing behaviour — that it never retries once text has reached the screen,
/// never treats the user pressing stop as a failure, and never retries a refusal that waiting cannot
/// fix.
/// </summary>
public class RateLimitRetryMiddlewareTests
{
    /// <summary>Pauses small enough that a test which does wait finishes in milliseconds.</summary>
    private static LlmTurnContext Context(SplaRetrySection? policy = null) => new()
    {
        Messages = new List<ChatMessage> { new() { Role = ChatRole.User, Content = "go" } },
        Settings = new LLMSettings
        {
            Retry = policy ?? new SplaRetrySection
            {
                Attempts = 4, MinDelay = 0.01, Step = 2, MaxDelay = 0.05, Total = 10
            }
        }
    };

    private static LlmRequestException RateLimited(double? retryAfterSeconds = null) =>
        new(LlmErrorKind.RateLimited, "slow down", System.Net.HttpStatusCode.TooManyRequests)
        {
            Signals = retryAfterSeconds is { } s
                ?
                [
                    new ProviderFact
                    {
                        Key = "ratelimit.retry_after", Label = "Retry after",
                        Value = s.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        Unit = "s", Kind = ProviderFactKind.Duration, ObservedAt = DateTimeOffset.UtcNow
                    }
                ]
                : []
        };

    private static LlmTurnResult Answer(string text) => new()
    {
        Message = new ChatMessage { Role = ChatRole.Assistant, Content = text }
    };

    [Fact]
    public async Task Retries_a_rate_limit_and_returns_the_next_attempts_answer()
    {
        var calls = 0;
        var result = await new RateLimitRetryMiddleware().InvokeAsync(
            Context(),
            (_, _) => ++calls == 1 ? throw RateLimited() : Task.FromResult(Answer("hello")),
            CancellationToken.None);

        Assert.Equal(2, calls);
        Assert.Equal("hello", result.Message.Content);
    }

    [Fact]
    public async Task Gives_up_after_the_configured_attempts_and_rethrows_the_last_failure()
    {
        var calls = 0;
        var ex = await Assert.ThrowsAsync<LlmRequestException>(() =>
            new RateLimitRetryMiddleware().InvokeAsync(
                Context(),
                (_, _) => { calls++; throw RateLimited(); },
                CancellationToken.None));

        Assert.Equal(4, calls);
        Assert.Equal(LlmErrorKind.RateLimited, ex.Kind);
    }

    [Fact]
    public async Task A_refusal_that_waiting_cannot_fix_is_not_retried()
    {
        var calls = 0;
        await Assert.ThrowsAsync<LlmRequestException>(() =>
            new RateLimitRetryMiddleware().InvokeAsync(
                Context(),
                (_, _) => { calls++; throw new LlmRequestException(LlmErrorKind.AuthFailed, "bad key"); },
                CancellationToken.None));

        Assert.Equal(1, calls);
    }

    /// <summary>The provider's own figure is a fact, not a guess, so the ceilings that bound our
    /// guesses must not touch it — here MaxDelay is far below what the provider asked for.</summary>
    [Fact]
    public async Task A_stated_retry_after_overrides_the_schedule_and_ignores_the_ceilings()
    {
        var notes = new List<string?>();
        var ctx = Context(new SplaRetrySection
        {
            Attempts = 2, MinDelay = 0.01, Step = 2, MaxDelay = 0.02, Total = 0.02
        });
        ctx.OnAttempt = a => notes.Add(a.Note);

        var calls = 0;
        await new RateLimitRetryMiddleware().InvokeAsync(
            ctx,
            (_, _) => ++calls == 1 ? throw RateLimited(retryAfterSeconds: 0.2) : Task.FromResult(Answer("ok")),
            CancellationToken.None);

        Assert.Equal(2, calls);
        Assert.Contains("the provider asked for 0.2s", Assert.Single(notes));
    }

    /// <summary>The wait travels structured, not only inside the English note, because the surface that
    /// shows it has to word it in the reader's language — and has to say whose figure it is.</summary>
    [Fact]
    public async Task Reports_the_wait_and_whose_figure_it_is()
    {
        var reported = new List<GenerationAttempt>();
        var ctx = Context();
        ctx.OnAttempt = reported.Add;

        var calls = 0;
        await new RateLimitRetryMiddleware().InvokeAsync(
            ctx,
            (_, _) => ++calls switch
            {
                1 => throw RateLimited(retryAfterSeconds: 0.05),
                2 => throw RateLimited(),
                _ => Task.FromResult(Answer("ok"))
            },
            CancellationToken.None);

        Assert.Equal(2, reported.Count);
        Assert.All(reported, a => Assert.Equal(AttemptOutcome.RateLimited, a.Outcome));

        Assert.Equal(TimeSpan.FromSeconds(0.05), reported[0].Wait);
        Assert.True(reported[0].WaitStated);

        // Second refusal named nothing, so the schedule answers — and says so.
        Assert.False(reported[1].WaitStated);
        Assert.NotNull(reported[1].Wait);
    }

    /// <summary>Without a stated figure the budget binds, and it binds BEFORE sleeping: a pause that
    /// would not fit is not taken at all.</summary>
    [Fact]
    public async Task The_total_budget_stops_the_retrying()
    {
        var calls = 0;
        var elapsed = Stopwatch.StartNew();

        await Assert.ThrowsAsync<LlmRequestException>(() =>
            new RateLimitRetryMiddleware().InvokeAsync(
                Context(new SplaRetrySection
                {
                    Attempts = 4, MinDelay = 5, Step = 2, MaxDelay = 10, Total = 0.5
                }),
                (_, _) => { calls++; throw RateLimited(); },
                CancellationToken.None));

        Assert.Equal(1, calls);
        Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(2), "the over-budget pause must not be slept");
    }

    /// <summary>The data path bypasses the pipeline, so a chunk already delivered cannot be taken back
    /// and a second attempt would append a whole answer under the first one.</summary>
    [Fact]
    public async Task Never_retries_once_a_chunk_has_reached_the_caller()
    {
        var seen = new List<string>();
        var ctx = Context();
        ctx.OnDelta = chunk => { seen.Add(chunk); return Task.CompletedTask; };

        var calls = 0;
        await Assert.ThrowsAsync<LlmRequestException>(() =>
            new RateLimitRetryMiddleware().InvokeAsync(
                ctx,
                async (c, _) =>
                {
                    calls++;
                    await c.OnDelta!("half an answer");
                    throw RateLimited();
                },
                CancellationToken.None));

        Assert.Equal(1, calls);
        Assert.Equal(["half an answer"], seen);
    }

    [Fact]
    public async Task Stop_ends_the_wait_at_once_instead_of_after_it()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var calls = 0;
        var elapsed = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new RateLimitRetryMiddleware().InvokeAsync(
                Context(new SplaRetrySection
                {
                    Attempts = 4, MinDelay = 30, Step = 2, MaxDelay = 60, Total = 600
                }),
                (_, _) => { calls++; throw RateLimited(); },
                cts.Token));

        Assert.Equal(1, calls);
        Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(5), "the pause must observe the turn's token");
    }

    /// <summary>The context belongs to the caller: the sinks it arrived with are the sinks it leaves
    /// with, whether the turn succeeded or threw.</summary>
    [Fact]
    public async Task Leaves_the_callers_sinks_as_it_found_them()
    {
        var ctx = Context();
        Func<string, Task> original = _ => Task.CompletedTask;
        ctx.OnDelta = original;

        await new RateLimitRetryMiddleware().InvokeAsync(
            ctx, (_, _) => Task.FromResult(Answer("ok")), CancellationToken.None);

        Assert.Same(original, ctx.OnDelta);
    }
}
