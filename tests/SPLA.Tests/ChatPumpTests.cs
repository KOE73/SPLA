using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.Service;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// <see cref="ChatPump"/>. Two layers, matching the class itself: <see cref="ChatPump.DecideWake"/>
/// is exercised directly as a pure function (no inbox, no timer, no chat); the coalescing/timing
/// behaviour is exercised through a real <see cref="ChatPump"/> wired to a real <see cref="ChatInbox"/>
/// with fake environment delegates, polling for the async effect rather than sleeping a fixed amount
/// (the 500ms debounce plus scheduler jitter makes a fixed sleep either flaky or slow).
/// </summary>
public class ChatPumpTests
{
    // ---- pure decision method -------------------------------------------------------------------

    [Fact]
    public void DecideWake_nothing_pending_does_not_wake()
    {
        var d = ChatPump.DecideWake(hasPending: false, isTurnRunning: false, hasWatchers: true,
            consecutiveAutoWakes: 0, cap: 3);
        Assert.Equal(ChatPump.WakeDecision.NothingPending, d);
    }

    [Fact]
    public void DecideWake_turn_already_running_does_not_start_a_second_one()
    {
        var d = ChatPump.DecideWake(hasPending: true, isTurnRunning: true, hasWatchers: true,
            consecutiveAutoWakes: 0, cap: 3);
        Assert.Equal(ChatPump.WakeDecision.TurnAlreadyRunning, d);
    }

    [Fact]
    public void DecideWake_no_watchers_does_not_wake_by_default_policy()
    {
        var d = ChatPump.DecideWake(hasPending: true, isTurnRunning: false, hasWatchers: false,
            consecutiveAutoWakes: 0, cap: 3);
        Assert.Equal(ChatPump.WakeDecision.NoWatchers, d);
    }

    [Fact]
    public void DecideWake_below_cap_wakes()
    {
        var d = ChatPump.DecideWake(hasPending: true, isTurnRunning: false, hasWatchers: true,
            consecutiveAutoWakes: 2, cap: 3);
        Assert.Equal(ChatPump.WakeDecision.Wake, d);
    }

    [Fact]
    public void DecideWake_at_cap_refuses_and_reports_self_feeding()
    {
        var d = ChatPump.DecideWake(hasPending: true, isTurnRunning: false, hasWatchers: true,
            consecutiveAutoWakes: 3, cap: 3);
        Assert.Equal(ChatPump.WakeDecision.SelfFeedingCapReached, d);
    }

    [Fact]
    public void DecideWake_suppressed_wins_over_a_running_turn()
    {
        // Order matters (plan step C.2): Suppressed must be checked BEFORE TurnAlreadyRunning, because
        // that branch re-arms the debounce timer — a suppressed pump must go quiet instead, or Stop
        // degrades into a half-second pause rather than a stop.
        var d = ChatPump.DecideWake(hasPending: true, isTurnRunning: true, hasWatchers: true,
            consecutiveAutoWakes: 0, cap: 3, autoWakeSuppressed: true);
        Assert.Equal(ChatPump.WakeDecision.Suppressed, d);
    }

    [Fact]
    public void DecideWake_suppressed_with_nothing_pending_still_reports_nothing_pending()
    {
        // NothingPending is checked first regardless — an empty inbox is an empty inbox whether or
        // not the pump happens to be suppressed.
        var d = ChatPump.DecideWake(hasPending: false, isTurnRunning: false, hasWatchers: true,
            consecutiveAutoWakes: 0, cap: 3, autoWakeSuppressed: true);
        Assert.Equal(ChatPump.WakeDecision.NothingPending, d);
    }

    // ---- end-to-end coalescing / policy via a real ChatPump --------------------------------------

    private static ChatMessage TaskResultMessage(string text) => new() { Role = ChatRole.User, Content = text };

    private sealed class Env
    {
        public bool Watchers = true;
        public bool TurnRunning;
        public bool AutoWakeSuppressed;
        public int HumanTurnCount;
        public int RunTurnCalls;
        public readonly List<string> Notices = new();
        public readonly SemaphoreSlim RanOnce = new(0);

        public ChatPump MakePump(ChatInbox inbox) => new(
            inbox,
            hasWatchers: () => Watchers,
            isTurnRunning: () => TurnRunning,
            humanTurnCount: () => HumanTurnCount,
            runTurn: ct =>
            {
                Interlocked.Increment(ref RunTurnCalls);
                inbox.DrainAll(); // stand-in for what the real turn's DrainInbox would do
                RanOnce.Release();
                return Task.CompletedTask;
            },
            broadcastNotice: text => { lock (Notices) Notices.Add(text); },
            autoWakeSuppressed: () => AutoWakeSuppressed);
    }

    /// <summary>Polls a condition instead of a fixed sleep, since the thing under test is itself a
    /// timer with real elapsed time (500ms debounce) — a fixed wait would be either flaky under load
    /// or needlessly slow. Fails loudly rather than hanging if the condition never turns true.</summary>
    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("condition never became true");
            await Task.Delay(25);
        }
    }

    [Fact]
    public async Task Three_results_within_the_debounce_window_produce_one_turn()
    {
        var inbox = new ChatInbox();
        var env = new Env();
        using var pump = env.MakePump(inbox);

        inbox.Enqueue(TaskResultMessage("one"), InboxItemKind.TaskResult);
        inbox.Enqueue(TaskResultMessage("two"), InboxItemKind.TaskResult);
        inbox.Enqueue(TaskResultMessage("three"), InboxItemKind.TaskResult);

        // Give it comfortably longer than the 500ms debounce, then confirm exactly one turn ran and
        // stays at one (no further firing sneaks a second one in).
        await WaitUntilAsync(() => env.RunTurnCalls >= 1, timeoutMs: 3000);
        await Task.Delay(300); // settle: nothing else should fire after the one turn
        Assert.Equal(1, env.RunTurnCalls);
    }

    [Fact]
    public async Task No_wake_when_nothing_pending()
    {
        var inbox = new ChatInbox();
        var env = new Env();
        using var pump = env.MakePump(inbox);

        // Nothing enqueued — the timer is never even armed. Confirm no turn fires within a window
        // comfortably longer than the debounce.
        await Task.Delay(700);
        Assert.Equal(0, env.RunTurnCalls);
    }

    [Fact]
    public async Task No_wake_while_a_turn_is_already_running()
    {
        var inbox = new ChatInbox();
        var env = new Env { TurnRunning = true };
        using var pump = env.MakePump(inbox);

        inbox.Enqueue(TaskResultMessage("x"), InboxItemKind.TaskResult);

        await Task.Delay(700);
        Assert.Equal(0, env.RunTurnCalls);
        Assert.True(inbox.HasPending); // still there — the running turn's own drain will take it
    }

    [Fact]
    public async Task No_wake_without_watchers()
    {
        var inbox = new ChatInbox();
        var env = new Env { Watchers = false };
        using var pump = env.MakePump(inbox);

        inbox.Enqueue(TaskResultMessage("x"), InboxItemKind.TaskResult);

        await Task.Delay(700);
        Assert.Equal(0, env.RunTurnCalls);
        Assert.True(inbox.HasPending); // queued for the next human turn, per ADR §4.1
    }

    [Fact]
    public async Task Self_feeding_cap_stops_auto_wakes_and_notices_once()
    {
        var inbox = new ChatInbox();
        var env = new Env();
        using var pump = env.MakePump(inbox);

        // Each of the pump's own turns re-enqueues a fresh TaskResult, simulating a woken turn that
        // itself kicks off a background task whose completion wakes the next one.
        for (var i = 0; i < ChatPump.SelfFeedingCap + 2; i++)
        {
            inbox.Enqueue(TaskResultMessage($"round-{i}"), InboxItemKind.TaskResult);
            // Longer than the 500ms debounce so each round fires (and drains) on its own instead of
            // coalescing into one turn with the next round's item.
            await Task.Delay(700);
        }

        await WaitUntilAsync(() => env.RunTurnCalls >= ChatPump.SelfFeedingCap, timeoutMs: 5000);
        await Task.Delay(700); // let any further (wrongly-permitted) firing show up
        Assert.Equal(ChatPump.SelfFeedingCap, env.RunTurnCalls);
        Assert.Single(env.Notices);
    }

    [Fact]
    public async Task A_human_turn_resets_the_self_feeding_counter()
    {
        var inbox = new ChatInbox();
        var env = new Env();
        using var pump = env.MakePump(inbox);

        // One extra round beyond the cap: the cap-th wake only sets the counter to the cap value, and
        // the guard is only *evaluated* against a fresh pending item — this extra round is what makes
        // the pump actually see, and refuse, the capped state (and so emit the notice).
        for (var i = 0; i < ChatPump.SelfFeedingCap + 1; i++)
        {
            inbox.Enqueue(TaskResultMessage($"round-{i}"), InboxItemKind.TaskResult);
            await Task.Delay(700);
        }
        await WaitUntilAsync(() => env.RunTurnCalls >= ChatPump.SelfFeedingCap, timeoutMs: 5000);
        await Task.Delay(700);
        Assert.Equal(ChatPump.SelfFeedingCap, env.RunTurnCalls);
        Assert.Single(env.Notices);

        // A person speaks: HumanTurnCount rises, same as ChatRuntime.SendAsync(text: "...") would do.
        env.HumanTurnCount++;

        inbox.Enqueue(TaskResultMessage("after-human"), InboxItemKind.TaskResult);
        await WaitUntilAsync(() => env.RunTurnCalls > ChatPump.SelfFeedingCap, timeoutMs: 3000);
        Assert.Equal(ChatPump.SelfFeedingCap + 1, env.RunTurnCalls);
    }

    // ---- wave C: Stop disarms the pump --------------------------------------------------------

    [Fact]
    public async Task Suppressed_pump_does_not_wake_even_with_pending_items_and_watchers()
    {
        var inbox = new ChatInbox();
        var env = new Env { AutoWakeSuppressed = true };
        using var pump = env.MakePump(inbox);

        inbox.Enqueue(TaskResultMessage("x"), InboxItemKind.TaskResult);

        await Task.Delay(700); // comfortably longer than the 500ms debounce
        Assert.Equal(0, env.RunTurnCalls);
        Assert.True(inbox.HasPending); // Stop never clears the inbox — ADR §2.4
    }

    [Fact]
    public async Task Suppression_survives_a_task_cancellation_delivery_landing_in_the_inbox()
    {
        // A cancelled background task still delivers its cancellation result into the inbox
        // (CorrelationHandlers.Cancel/BackgroundTaskRegistry.CancelAll leave delivery to whatever
        // already handles a finished task). That delivery itself raises ChatInbox.Enqueued — exactly
        // the signal that would normally arm the debounce timer — and suppression must survive it.
        var inbox = new ChatInbox();
        var env = new Env { AutoWakeSuppressed = true };
        using var pump = env.MakePump(inbox);

        inbox.Enqueue(TaskResultMessage("cancelled: bg_1 (system_run_shell)"), InboxItemKind.TaskResult);

        await Task.Delay(700);
        Assert.Equal(0, env.RunTurnCalls);
    }

    [Fact]
    public async Task A_human_turn_clears_suppression_and_the_pump_wakes_again()
    {
        var inbox = new ChatInbox();
        var env = new Env { AutoWakeSuppressed = true };
        using var pump = env.MakePump(inbox);

        inbox.Enqueue(TaskResultMessage("during-stop"), InboxItemKind.TaskResult);
        await Task.Delay(700);
        Assert.Equal(0, env.RunTurnCalls);

        // A person speaks: in the real system ChatRuntime.SendAsync clears AutoWakeSuppressed the
        // same place it bumps HumanTurnCount — mirrored here on the fake env.
        env.AutoWakeSuppressed = false;
        env.HumanTurnCount++;

        inbox.Enqueue(TaskResultMessage("after-human"), InboxItemKind.TaskResult);
        await WaitUntilAsync(() => env.RunTurnCalls >= 1, timeoutMs: 3000);
        Assert.Equal(1, env.RunTurnCalls);
    }
}
