using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Llm;
using SPLA.Domain.Llm.Middleware;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Covers the gate that keeps requests apart: that it queues callers on one connection, that it keeps
/// separate connections independent, and that switched off — its default — it is not merely fast but
/// entirely absent from the path.
/// </summary>
public class RequestPacingTests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Second = TimeSpan.FromSeconds(1);

    [Fact]
    public void The_first_caller_on_an_idle_connection_waits_for_nothing()
    {
        Assert.Equal(TimeSpan.Zero, new RequestPacer().Claim("openrouter", Second, T0));
    }

    /// <summary>The slot is claimed at once rather than after the wait, so callers arriving together
    /// line up instead of all being told to wait the same single interval.</summary>
    [Fact]
    public void Callers_arriving_together_queue_one_behind_the_other()
    {
        var pacer = new RequestPacer();

        var waits = Enumerable.Range(0, 3).Select(_ => pacer.Claim("openrouter", Second, T0)).ToList();

        Assert.Equal([TimeSpan.Zero, Second, TimeSpan.FromSeconds(2)], waits);
    }

    /// <summary>A key that has been idle longer than the interval owes nothing — the gate spaces
    /// requests, it does not ration them.</summary>
    [Fact]
    public void An_idle_stretch_is_not_carried_forward()
    {
        var pacer = new RequestPacer();
        pacer.Claim("openrouter", Second, T0);

        Assert.Equal(TimeSpan.Zero, pacer.Claim("openrouter", Second, T0.AddMinutes(5)));
    }

    /// <summary>Different credentials have different budgets, so they never wait on each other.</summary>
    [Fact]
    public void Separate_connections_do_not_delay_each_other()
    {
        var pacer = new RequestPacer();
        pacer.Claim("openrouter", Second, T0);

        Assert.Equal(TimeSpan.Zero, pacer.Claim("lmstudio", Second, T0));
    }

    [Fact]
    public void Zero_interval_never_makes_anyone_wait()
    {
        var pacer = new RequestPacer();

        Assert.Equal(TimeSpan.Zero, pacer.Claim("openrouter", TimeSpan.Zero, T0));
        Assert.Equal(TimeSpan.Zero, pacer.Claim("openrouter", TimeSpan.Zero, T0));
    }

    private static LlmTurnContext Context(double interval, string? connectionId = "openrouter") => new()
    {
        Messages = new List<ChatMessage> { new() { Role = ChatRole.User, Content = "go" } },
        Settings = new LLMSettings { ConnectionId = connectionId, MinRequestInterval = interval }
    };

    private static LlmTurnResult Answer() => new()
    {
        Message = new ChatMessage { Role = ChatRole.Assistant, Content = "ok" }
    };

    [Fact]
    public async Task The_default_of_zero_leaves_the_turn_untouched()
    {
        var elapsed = Stopwatch.StartNew();
        var middleware = new RequestPacingMiddleware(new RequestPacer());

        await middleware.InvokeAsync(Context(0), (_, _) => Task.FromResult(Answer()), CancellationToken.None);
        await middleware.InvokeAsync(Context(0), (_, _) => Task.FromResult(Answer()), CancellationToken.None);

        Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(1), "an unpaced turn must not sleep at all");
    }

    /// <summary>A turn with no configured account has no budget to protect and must not be held up by
    /// one shared "" queue.</summary>
    [Fact]
    public async Task A_turn_without_a_connection_is_never_paced()
    {
        var elapsed = Stopwatch.StartNew();
        var middleware = new RequestPacingMiddleware(new RequestPacer());

        await middleware.InvokeAsync(Context(30, connectionId: null), (_, _) => Task.FromResult(Answer()), CancellationToken.None);
        await middleware.InvokeAsync(Context(30, connectionId: null), (_, _) => Task.FromResult(Answer()), CancellationToken.None);

        Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Stop_ends_the_wait_instead_of_serving_the_turn_late()
    {
        var pacer = new RequestPacer();
        var middleware = new RequestPacingMiddleware(pacer);
        var calls = 0;

        // Burn the first slot so the next caller owes a long wait.
        await middleware.InvokeAsync(Context(60), (_, _) => { calls++; return Task.FromResult(Answer()); }, CancellationToken.None);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var elapsed = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            middleware.InvokeAsync(Context(60), (_, _) => { calls++; return Task.FromResult(Answer()); }, cts.Token));

        Assert.Equal(1, calls);
        Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(5), "the pause must observe the turn's token");
    }
}
