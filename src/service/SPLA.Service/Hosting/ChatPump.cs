using Microsoft.Extensions.Logging;
using SPLA.Domain.Tools;
using SPLA.Runtime;

namespace SPLA.Service;

/// <summary>
/// Wakes a chat's own turn when a background task's result arrives with nobody there to send the
/// next message — the piece PLAN_20260825 wave B exists for. One instance per chat, wired up from
/// <see cref="ChatRegistry.RuntimeOpened"/> right beside <c>SplaServiceHost.WireChatProgress</c> and
/// disposed on the new <see cref="ChatRegistry.RuntimeClosed"/>, so it lives and dies with the
/// <see cref="ChatRuntime"/> it watches.
///
/// <para>
/// Subscribes to <see cref="ChatInbox.Enqueued"/>. A <see cref="InboxItemKind.TaskResult"/> signal
/// arms a 500ms debounce timer rather than waking immediately — three tasks finishing within a
/// second must produce ONE turn carrying all three results, not three (ADR §2.1, plan step B.3).
/// <see cref="InboxItemKind.Human"/> is ignored here: nothing produces it yet (that is wave D), and
/// when it does, it will wake immediately rather than through this debounce. <see cref="InboxItemKind.Notice"/>
/// never arrives here at all — see <see cref="BroadcastNotice"/> below.
/// </para>
///
/// <para>
/// The wake decision itself (<see cref="DecideWake"/>) is a pure function of chat state, deliberately
/// kept free of <c>ConnectionHub</c>/timer/socket so it can be driven directly from a unit test. The
/// timer, the debounce, and the actual turn call live in <see cref="OnTimerElapsedAsync"/> around it.
/// </para>
/// </summary>
internal sealed class ChatPump : IDisposable
{
    /// <summary>Consecutive auto-wakes (turns started with no human message since the last one)
    /// allowed before the pump refuses to wake itself again — ADR §2.6 / plan step B.5. A woken turn
    /// can launch a background task whose result wakes the next turn; without a cap that loop never
    /// stops on its own.</summary>
    internal const int SelfFeedingCap = 3;

    private static readonly TimeSpan DebounceWindow = TimeSpan.FromMilliseconds(500);

    /// <summary>What the pure decision method concluded, for the impure driver above it to act on.</summary>
    internal enum WakeDecision
    {
        /// <summary>Trap B.6: a turn that was already running drained the inbox itself before this
        /// signal was even handled. Waking now would start an empty turn.</summary>
        NothingPending,
        /// <summary>A turn is already running (or queued on the gate) — <c>ChatRuntime._turnGate</c>
        /// would serialise a second one anyway, but starting it now would just make it wait; re-arm
        /// and check again once the running turn is done.</summary>
        TurnAlreadyRunning,
        /// <summary>Default policy (ADR §4.1): nobody is watching, so nobody would see an auto-turn's
        /// tokens spent. Items stay queued for the next human turn.</summary>
        NoWatchers,
        /// <summary>The self-feeding guard has tripped — see <see cref="SelfFeedingCap"/>.</summary>
        SelfFeedingCapReached,
        /// <summary>Go — start a turn with no human text; its content is whatever DrainInbox picks up.</summary>
        Wake
    }

    /// <summary>
    /// The wake decision, isolated from the pump's own timer/subscription plumbing so it can be
    /// exercised directly by a test with plain booleans and ints — no <c>ConnectionHub</c>, no
    /// <c>ChatRuntime</c>, no clock. Order matches plan step B.2's numbered policy exactly.
    /// </summary>
    internal static WakeDecision DecideWake(
        bool hasPending, bool isTurnRunning, bool hasWatchers, int consecutiveAutoWakes, int cap)
    {
        if (!hasPending) return WakeDecision.NothingPending;
        if (isTurnRunning) return WakeDecision.TurnAlreadyRunning;
        if (!hasWatchers) return WakeDecision.NoWatchers;
        if (consecutiveAutoWakes >= cap) return WakeDecision.SelfFeedingCapReached;
        return WakeDecision.Wake;
    }

    private readonly ChatInbox _inbox;
    private readonly Func<bool> _hasWatchers;
    private readonly Func<bool> _isTurnRunning;
    private readonly Func<int> _humanTurnCount;
    private readonly Func<CancellationToken, Task> _runTurn;
    private readonly Action<string> _broadcastNotice;
    /// <summary>Optional so a test can build a pump without one; the wiring in SplaServiceHost passes
    /// the chat's real logger, which is the only place a swallowed wake failure could surface.</summary>
    private readonly ILogger? _log;

    private readonly Timer _timer;
    private readonly CancellationTokenSource _lifetime = new();
    private int _disposed;

    /// <summary>Guards <see cref="OnTimerElapsedAsync"/> against overlapping runs: the timer can fire
    /// again (a fresh signal re-arms it) while a previous firing is still mid-turn.</summary>
    private int _handling;

    private int _consecutiveAutoWakes;
    private int _lastSeenHumanTurnCount;
    private bool _capNoticeSent;

    /// <param name="inbox">This chat's inbox — the pump's only trigger.</param>
    /// <param name="hasWatchers">True when somebody has the chat open. Injected rather than a direct
    /// <c>ConnectionHub</c> reference so the decision path stays testable without one.</param>
    /// <param name="isTurnRunning">True while a turn is running or queued on the gate.</param>
    /// <param name="humanTurnCount">Rising count of turns started by an actual human message — see
    /// <see cref="ChatRuntime.HumanTurnCount"/>. Used only to detect "did a person speak since my last
    /// wake", never compared across chats or persisted.</param>
    /// <param name="runTurn">Starts one turn with no human text (<c>text: null</c>) and returns when
    /// it completes. Wired to <c>ChatTurnDriver.RunTurnAsync</c> by the caller.</param>
    /// <param name="broadcastNotice">Delivers one <c>Notice</c> to the chat's watchers when the
    /// self-feeding guard trips. Deliberately NOT routed through <see cref="ChatInbox.Enqueue"/>: a
    /// notice enqueued there would itself raise <see cref="ChatInbox.Enqueued"/> and re-trigger the
    /// pump, defeating the very guard that just fired.</param>
    public ChatPump(
        ChatInbox inbox,
        Func<bool> hasWatchers,
        Func<bool> isTurnRunning,
        Func<int> humanTurnCount,
        Func<CancellationToken, Task> runTurn,
        Action<string> broadcastNotice,
        ILogger? log = null)
    {
        _inbox = inbox;
        _hasWatchers = hasWatchers;
        _isTurnRunning = isTurnRunning;
        _humanTurnCount = humanTurnCount;
        _runTurn = runTurn;
        _broadcastNotice = broadcastNotice;
        _log = log;
        _lastSeenHumanTurnCount = humanTurnCount();

        // Created idle (Timeout.Infinite): nothing arms it until the first TaskResult signal.
        // The firing is fire-and-forget by nature — a timer has nobody to hand a Task back to — so the
        // continuation below is the only thing standing between a throwing wake and total silence.
        // ChatTurnDriver already swallows and logs a turn's own failures, which leaves only the rare
        // ones around it; those must not vanish, or a pump that stopped waking would look like a pump
        // with nothing to do.
        _timer = new Timer(_ => _ = OnTimerElapsedAsync().ContinueWith(
                t => _log?.LogError(t.Exception, "Chat pump wake failed."),
                CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default),
            null, Timeout.Infinite, Timeout.Infinite);
        _inbox.Enqueued += OnEnqueued;
    }

    private void OnEnqueued(InboxItemKind kind)
    {
        // Human is wave D's trigger (immediate wake, no debounce) — nothing wires it yet, so treating
        // it as a no-op here is correct today and stays obviously incomplete rather than pretending to
        // handle a case it does not. Notice never arrives: see the constructor's broadcastNotice comment.
        if (kind != InboxItemKind.TaskResult) return;
        if (Volatile.Read(ref _disposed) != 0) return;

        try { _timer.Change(DebounceWindow, Timeout.InfiniteTimeSpan); }
        catch (ObjectDisposedException) { /* raced with Dispose — nothing left to wake */ }
    }

    private async Task OnTimerElapsedAsync()
    {
        if (Volatile.Read(ref _disposed) != 0) return;
        if (Interlocked.Exchange(ref _handling, 1) == 1) return; // already mid-decision on another firing
        try
        {
            // Looped rather than one-shot: after a turn we just ran completes, more may have arrived
            // while it was running (ADR trap B.6's mirror image) — re-check immediately instead of
            // waiting for another debounce window.
            while (true)
            {
                if (HumanSpokeSinceLastCheck())
                {
                    _consecutiveAutoWakes = 0;
                    _capNoticeSent = false;
                }

                var decision = DecideWake(
                    _inbox.HasPending, _isTurnRunning(), _hasWatchers(), _consecutiveAutoWakes, SelfFeedingCap);

                switch (decision)
                {
                    case WakeDecision.NothingPending:
                    case WakeDecision.NoWatchers:
                        return;

                    case WakeDecision.TurnAlreadyRunning:
                        // The running turn's own DrainInbox will pick up what is queued; re-arm so this
                        // check repeats once the gate frees up, instead of busy-looping here.
                        try { _timer.Change(DebounceWindow, Timeout.InfiniteTimeSpan); }
                        catch (ObjectDisposedException) { }
                        return;

                    case WakeDecision.SelfFeedingCapReached:
                        // Once per trip of the guard, not once per still-arriving TaskResult: more
                        // results can keep landing while the pump sits paused, and repeating the same
                        // notice for each would be exactly the noise the guard exists to prevent.
                        if (!_capNoticeSent)
                        {
                            _capNoticeSent = true;
                            _broadcastNotice(
                                $"Auto-wake paused after {SelfFeedingCap} consecutive background wakes with " +
                                "no reply from you — send a message to resume.");
                        }
                        return;

                    case WakeDecision.Wake:
                        _consecutiveAutoWakes++;
                        await _runTurn(_lifetime.Token);
                        continue; // re-check: the turn may have left more behind, or a human may have spoken
                }
            }
        }
        finally { Volatile.Write(ref _handling, 0); }
    }

    /// <summary>True (and resets the watermark) the first time this is called after
    /// <see cref="ChatRuntime.HumanTurnCount"/> has risen — the pump's only view of "a person spoke",
    /// per ADR §2.6.</summary>
    private bool HumanSpokeSinceLastCheck()
    {
        var current = _humanTurnCount();
        if (current == _lastSeenHumanTurnCount) return false;
        _lastSeenHumanTurnCount = current;
        return true;
    }

    /// <summary>Stops the pump for good — called on <see cref="ChatRegistry.RuntimeClosed"/>. Cancels
    /// any turn this pump itself started (via the token handed to <c>runTurn</c>); a turn a human
    /// started is unaffected, since it never runs on this token.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _inbox.Enqueued -= OnEnqueued;
        _timer.Dispose();
        _lifetime.Cancel();
        _lifetime.Dispose();
    }
}
