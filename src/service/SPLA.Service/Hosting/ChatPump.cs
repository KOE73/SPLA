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
/// <see cref="InboxItemKind.Human"/> (PLAN_20260825 wave D — <c>ChatHandlers.Send</c> enqueues it)
/// wakes immediately instead, skipping the debounce entirely. <see cref="InboxItemKind.Notice"/>
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
    /// <summary>Back-compat default for consecutive auto-wakes (turns started with no human message
    /// since the last one) allowed before the pump refuses to wake itself again — ADR §2.6 / plan step
    /// B.5. A woken turn can launch a background task whose result wakes the next turn; without a cap
    /// that loop never stops on its own. Used only when the constructor's own <c>selfFeedingCap</c>
    /// parameter is <c>null</c> (every pre-existing caller, including every test); the real app now
    /// wires <see cref="SPLA.Domain.Settings.SplaAgentSection.SelfFeedingCap"/> instead, which defaults
    /// to disabled — see that setting's own doc for why the guard stopped being unconditional.</summary>
    internal const int SelfFeedingCap = 3;

    private static readonly TimeSpan DebounceWindow = TimeSpan.FromMilliseconds(500);

    /// <summary>What the pure decision method concluded, for the impure driver above it to act on.</summary>
    internal enum WakeDecision
    {
        /// <summary>Trap B.6: a turn that was already running drained the inbox itself before this
        /// signal was even handled. Waking now would start an empty turn.</summary>
        NothingPending,
        /// <summary>Stop disarmed the pump (wave C, ADR §2.4) and no human message has arrived since.
        /// Ordered before <see cref="TurnAlreadyRunning"/> deliberately: that branch re-arms the timer
        /// to check again later, and a suppressed pump must go quiet instead — re-arming forever would
        /// turn "stop" into a half-second pause rather than a stop.</summary>
        Suppressed,
        /// <summary>A turn is already running (or queued on the gate) — <c>ChatRuntime._turnGate</c>
        /// would serialise a second one anyway, but starting it now would just make it wait; re-arm
        /// and check again once the running turn is done.</summary>
        TurnAlreadyRunning,
        /// <summary>Wave 6's emergency stop (ADR §2.4) — <see cref="InboxItemKind.Peer"/> depth reached
        /// <c>agent.peer_hard_cap</c> with nothing else pending to justify waking anyway. Should never
        /// fire: the debounce and depth ceiling below exist precisely to keep depth from ever getting
        /// here. Firing is a defect in the regulator, not normal operation — the driver announces it
        /// and refuses the reply exactly once per trip, mirroring <see cref="SelfFeedingCapReached"/>.</summary>
        PeerHardCapReached,
        /// <summary>Wave 6 (ADR §2.4) — a <see cref="InboxItemKind.Peer"/> item is the only reason to
        /// wake and its correspondence's depth has passed <c>agent.peer_depth_ceiling</c>. The reply is
        /// NOT discarded (trap 6's mirror for correspondence: <c>ChatInbox</c> is never cleared here) —
        /// it waits for a turn that starts for some other reason, which is also what resets the depth.</summary>
        PeerDepthCeilingReached,
        /// <summary>Default policy (ADR §4.1): nobody is watching, so nobody would see an auto-turn's
        /// tokens spent. Items stay queued for the next human turn. Does NOT apply when a
        /// <see cref="InboxItemKind.Peer"/> item is pending (wave 6, ADR §2.4: correspondence is
        /// internal circulation between chats that may have no watcher at all — <c>spla serve</c> with
        /// no window, a nightly run — and is meant to proceed anyway; the regulator below is what
        /// bounds the unattended spend this creates).</summary>
        NoWatchers,
        /// <summary>The self-feeding guard has tripped — see <see cref="SelfFeedingCap"/>.</summary>
        SelfFeedingCapReached,
        /// <summary>Go — start a turn with no human text; its content is whatever DrainInbox picks up.</summary>
        Wake
    }

    /// <summary>
    /// The wake decision, isolated from the pump's own timer/subscription plumbing so it can be
    /// exercised directly by a test with plain booleans and ints — no <c>ConnectionHub</c>, no
    /// <c>ChatRuntime</c>, no clock. Order matches plan step B.2's numbered policy, extended by wave 6
    /// (ADR §2.4) for <paramref name="hasPeerPending"/>/<paramref name="peerDepth"/>.
    /// <para>
    /// The four peer parameters default to values that reproduce the pre-wave-6 decision exactly
    /// (<paramref name="hasPeerPending"/><c> = false</c> keeps <see cref="WakeDecision.NoWatchers"/>
    /// exactly as it was, and a depth of 0 against a ceiling/cap of <see cref="int.MaxValue"/> never
    /// trips) — every pre-existing call site, including every pre-wave-6 test, keeps compiling and
    /// keeps its original meaning unchanged.
    /// </para>
    /// </summary>
    internal static WakeDecision DecideWake(
        bool hasPending, bool isTurnRunning, bool hasWatchers, int consecutiveAutoWakes, int cap,
        bool autoWakeSuppressed = false,
        bool hasPeerPending = false, int peerDepth = 0,
        int peerDepthCeiling = int.MaxValue, int peerHardCap = int.MaxValue)
    {
        if (!hasPending) return WakeDecision.NothingPending;
        if (autoWakeSuppressed) return WakeDecision.Suppressed;
        if (isTurnRunning) return WakeDecision.TurnAlreadyRunning;

        // Depth is only ever a reason to refuse — never a reason to wake on its own — so it is checked
        // unconditionally here, ahead of the watcher policy right below: two correspondents with a
        // person watching both chats must decay exactly the same as two with nobody watching at all,
        // or the regulator would be pointless the moment somebody opens a window.
        if (hasPeerPending && peerDepth >= peerHardCap) return WakeDecision.PeerHardCapReached;
        if (hasPeerPending && peerDepth > peerDepthCeiling) return WakeDecision.PeerDepthCeilingReached;

        // Peer bypasses the watcher gate (ADR §2.4); anything else pending still needs one.
        if (!hasWatchers && !hasPeerPending) return WakeDecision.NoWatchers;
        if (consecutiveAutoWakes >= cap) return WakeDecision.SelfFeedingCapReached;
        return WakeDecision.Wake;
    }

    /// <summary>
    /// Wave 6's decay regulator (ADR §2.4) — three pure, stateless computations next to
    /// <see cref="DecideWake"/> so they can be driven from a test with plain numbers: no clock, no
    /// timer, no <c>ConnectionHub</c>. <see cref="ChatPump"/> is the only thing that carries the
    /// mutable "current depth" this reads and writes (<c>_peerDepth</c>) — these methods only ever
    /// transform whatever depth they are handed.
    /// </summary>
    internal static class PeerWakePolicy
    {
        /// <summary>
        /// <c>base · 2^depth</c>, floored at <paramref name="base"/> and capped at
        /// <paramref name="max"/>. Depth 0 (the first reply after external energy) always waits exactly
        /// <paramref name="base"/> — the same coalescing window every other item already gets — and the
        /// wait only grows once a correspondence starts circulating on its own.
        /// </summary>
        internal static TimeSpan Debounce(int depth, TimeSpan @base, TimeSpan max)
        {
            if (depth < 0) depth = 0;
            if (@base <= TimeSpan.Zero) return TimeSpan.Zero;
            // depth is capped before the power so a very deep, mis-tracked count cannot overflow into
            // Infinity/NaN on the way to a TimeSpan — 32 is already many times past any sane ceiling.
            var factor = Math.Pow(2, Math.Min(depth, 32));
            var ms = @base.TotalMilliseconds * factor;
            if (double.IsInfinity(ms) || ms > max.TotalMilliseconds) return max;
            return TimeSpan.FromMilliseconds(ms);
        }
    }

    private readonly ChatInbox _inbox;
    private readonly Func<bool> _hasWatchers;
    private readonly Func<bool> _isTurnRunning;
    private readonly Func<bool> _autoWakeSuppressed;
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

    /// <summary>Wave 6's decay counter (ADR §2.4): consecutive <see cref="InboxItemKind.Peer"/> items
    /// enqueued since the last <see cref="InboxItemKind.Human"/> or <see cref="InboxItemKind.TaskResult"/>
    /// — external energy resets it to zero right in <see cref="OnEnqueued"/>, the same place that
    /// observes the kind in the first place, rather than waiting for <see cref="HumanSpokeSinceLastCheck"/>
    /// to notice a turn actually started. Read by <see cref="PeerWakePolicy.Debounce"/> to grow the
    /// wait and by <see cref="DecideWake"/>'s peer-depth checks.</summary>
    private int _peerDepth;
    private bool _peerHardCapNoticeSent;

    private readonly TimeSpan _peerDebounceBase;
    private readonly TimeSpan _peerDebounceMax;
    private readonly int _peerDepthCeiling;
    private readonly int _peerHardCap;
    private readonly int _selfFeedingCap;

    /// <param name="inbox">This chat's inbox — the pump's only trigger.</param>
    /// <param name="hasWatchers">True when somebody has the chat open. Injected rather than a direct
    /// <c>ConnectionHub</c> reference so the decision path stays testable without one.</param>
    /// <param name="isTurnRunning">True while a turn is running or queued on the gate.</param>
    /// <param name="autoWakeSuppressed">Required rather than optional, unlike <paramref name="log"/>:
    /// a missing logger costs a diagnostic, whereas a pump silently defaulting to "never suppressed"
    /// would quietly break Stop's promise that it stops everything. True after Stop has disarmed this
    /// chat's pump — see
    /// <see cref="ChatRuntime.AutoWakeSuppressed"/>. Injected the same way as <paramref name="hasWatchers"/>
    /// so the decision path stays testable without a real <c>ChatRuntime</c>.</param>
    /// <param name="humanTurnCount">Rising count of turns started by an actual human message — see
    /// <see cref="ChatRuntime.HumanTurnCount"/>. Used only to detect "did a person speak since my last
    /// wake", never compared across chats or persisted.</param>
    /// <param name="runTurn">Starts one turn with no human text (<c>text: null</c>) and returns when
    /// it completes. Wired to <c>ChatTurnDriver.RunTurnAsync</c> by the caller.</param>
    /// <param name="broadcastNotice">Delivers one <c>Notice</c> to the chat's watchers when the
    /// self-feeding guard trips. Deliberately NOT routed through <see cref="ChatInbox.Enqueue"/>: a
    /// notice enqueued there would itself raise <see cref="ChatInbox.Enqueued"/> and re-trigger the
    /// pump, defeating the very guard that just fired.</param>
    /// <param name="peerDebounceBase">Floor of the <see cref="InboxItemKind.Peer"/> debounce — see
    /// <see cref="SPLA.Domain.Settings.SplaAgentSection.PeerDebounceBaseSeconds"/>. Defaults to the
    /// plan's own default (2s) so a test or caller that does not care about wave 6 need not pass it.</param>
    /// <param name="peerDebounceMax">Ceiling of the same debounce (default 5 minutes) — see
    /// <see cref="SPLA.Domain.Settings.SplaAgentSection.PeerDebounceMaxSeconds"/>.</param>
    /// <param name="peerDepthCeiling">See <see cref="SPLA.Domain.Settings.SplaAgentSection.PeerDepthCeiling"/>
    /// (default 6).</param>
    /// <param name="peerHardCap">See <see cref="SPLA.Domain.Settings.SplaAgentSection.PeerHardCap"/>
    /// (default 24).</param>
    /// <param name="selfFeedingCap">See <see cref="SPLA.Domain.Settings.SplaAgentSection.SelfFeedingCap"/>.
    /// Defaults to this class's own back-compat constant (<see cref="SelfFeedingCap"/>, 3) when a caller
    /// omits it — every pre-existing caller, including every test, keeps its original behavior
    /// unchanged. Zero or a negative value means "disabled" (never trips); <c>SplaServiceHost</c> passes
    /// that explicitly whenever <c>agent.self_feeding_cap</c> is unset, per ADR §2.6's revision — the
    /// demo this guard used to strangle is meant to run unbounded unless someone opts into a limit.
    /// </param>
    public ChatPump(
        ChatInbox inbox,
        Func<bool> hasWatchers,
        Func<bool> isTurnRunning,
        Func<int> humanTurnCount,
        Func<CancellationToken, Task> runTurn,
        Action<string> broadcastNotice,
        Func<bool> autoWakeSuppressed,
        ILogger? log = null,
        TimeSpan? peerDebounceBase = null,
        TimeSpan? peerDebounceMax = null,
        int peerDepthCeiling = 6,
        int peerHardCap = 24,
        int selfFeedingCap = SelfFeedingCap)
    {
        _inbox = inbox;
        _hasWatchers = hasWatchers;
        _isTurnRunning = isTurnRunning;
        _autoWakeSuppressed = autoWakeSuppressed;
        _humanTurnCount = humanTurnCount;
        _runTurn = runTurn;
        _broadcastNotice = broadcastNotice;
        _log = log;
        _lastSeenHumanTurnCount = humanTurnCount();
        _peerDebounceBase = peerDebounceBase ?? TimeSpan.FromSeconds(2);
        _peerDebounceMax = peerDebounceMax ?? TimeSpan.FromMinutes(5);
        _peerDepthCeiling = peerDepthCeiling;
        _peerHardCap = peerHardCap;
        _selfFeedingCap = selfFeedingCap > 0 ? selfFeedingCap : int.MaxValue;

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
        // Notice never arrives here at all — see the constructor's broadcastNotice comment.
        if (kind == InboxItemKind.Notice) return;
        if (Volatile.Read(ref _disposed) != 0) return;

        // Wave 6 (ADR §2.4): Human and TaskResult are external energy and reset the decay counter to
        // zero right here — the moment the kind is known — rather than waiting for the next timer
        // firing to notice. A Peer item deepens the same counter by one, read back below for both the
        // debounce and (in DecideWake) the ceiling/hard-cap checks.
        switch (kind)
        {
            case InboxItemKind.Peer:
                Interlocked.Increment(ref _peerDepth);
                break;
            case InboxItemKind.Human:
            case InboxItemKind.TaskResult:
                Volatile.Write(ref _peerDepth, 0);
                _peerHardCapNoticeSent = false;
                break;
        }

        // A person's own words (PLAN_20260825 wave D) skip the debounce entirely — TimeSpan.Zero fires
        // on the next scheduler tick instead of waiting out the 500ms window three background results
        // coalesce over. ChatHandlers.Send already cleared AutoWakeSuppressed and bumped HumanTurnCount
        // before this enqueue (NoteHumanMessage), and the sender is by definition watching (it just
        // came from a live connection), so DecideWake's ordinary checks fall through to Wake on their
        // own — nothing here needs to bypass them, only the wait. Peer's own debounce grows with depth
        // (wave 6, ADR §2.4) instead of using the fixed 500ms window TaskResult coalesces over.
        var due = kind switch
        {
            InboxItemKind.Human => TimeSpan.Zero,
            InboxItemKind.Peer => PeerWakePolicy.Debounce(Volatile.Read(ref _peerDepth), _peerDebounceBase, _peerDebounceMax),
            _ => DebounceWindow
        };
        try { _timer.Change(due, Timeout.InfiniteTimeSpan); }
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
                    _inbox.HasPending, _isTurnRunning(), _hasWatchers(), _consecutiveAutoWakes, _selfFeedingCap,
                    _autoWakeSuppressed(),
                    hasPeerPending: _inbox.HasPendingOfKind(InboxItemKind.Peer),
                    peerDepth: Volatile.Read(ref _peerDepth),
                    peerDepthCeiling: _peerDepthCeiling,
                    peerHardCap: _peerHardCap);

                switch (decision)
                {
                    case WakeDecision.NothingPending:
                    case WakeDecision.NoWatchers:
                        return;

                    case WakeDecision.Suppressed:
                        // Go quiet, not re-arm: Stop's whole point is that the pump does not keep
                        // trying. The pending item stays queued (ADR §2.4, the inbox is never cleared)
                        // and rides the next human turn, which also clears the suppression.
                        return;

                    case WakeDecision.TurnAlreadyRunning:
                        // The running turn's own DrainInbox will pick up what is queued; re-arm so this
                        // check repeats once the gate frees up, instead of busy-looping here.
                        try { _timer.Change(DebounceWindow, Timeout.InfiniteTimeSpan); }
                        catch (ObjectDisposedException) { }
                        return;

                    case WakeDecision.PeerDepthCeilingReached:
                        // No re-arm: nothing here will change until either the correspondent sends
                        // another Peer item (re-arms on its own via OnEnqueued) or a Human/TaskResult
                        // resets the depth and wakes on its own — this is "waits for a turn that
                        // happens for some other reason" (ADR §2.4), not a retry loop.
                        return;

                    case WakeDecision.PeerHardCapReached:
                        // Wave 6's emergency stop. Should never fire — see the enum member's own doc —
                        // so once per trip is still plenty; more Peer items can keep landing while
                        // parked here and must not each repeat the notice.
                        if (!_peerHardCapNoticeSent)
                        {
                            _peerHardCapNoticeSent = true;
                            _broadcastNotice(
                                $"Correspondence depth hit the emergency limit ({_peerHardCap}) — this is a " +
                                "regulator defect, not normal decay. The reply is refused; send a message to resume.");
                        }
                        return;

                    case WakeDecision.SelfFeedingCapReached:
                        // Once per trip of the guard, not once per still-arriving TaskResult: more
                        // results can keep landing while the pump sits paused, and repeating the same
                        // notice for each would be exactly the noise the guard exists to prevent.
                        if (!_capNoticeSent)
                        {
                            _capNoticeSent = true;
                            _broadcastNotice(
                                $"Auto-wake paused after {_selfFeedingCap} consecutive background wakes with " +
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
