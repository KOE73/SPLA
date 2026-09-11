using System.Collections.Concurrent;

namespace SPLA.Runtime;

/// <summary>
/// The default <see cref="IChatFeed"/>: a thread-safe set of subscribers, delivered to synchronously
/// and in order (ADR_20260910-2 wave 0 — the non-blocking queue is wave 4's job, not this class's).
/// <para>
/// Publishing with zero subscribers is a single empty-collection check — no allocation, no lock beyond
/// a lock-free read of a <see cref="ConcurrentDictionary{TKey,TValue}"/> — which is what keeps a
/// windowless <c>chat run</c> with nested spawns from paying for a stream nobody is watching (§4.7).
/// </para>
/// </summary>
public sealed class ChatFeed : IChatFeed
{
    // Keyed by a token object rather than the delegate itself: two subscriptions with the same handler
    // instance (unusual, but not forbidden — a spawn parent subscribing its own tree-label callback to
    // more than one child at once could do it) must unsubscribe independently.
    private readonly ConcurrentDictionary<object, Action<ChatEvent>> _subscribers = new();

    /// <summary>
    /// Guards <see cref="_sequence"/> and every subscriber-set change — wave 1's "снимок плюс поток"
    /// (ADR_20260910-2 §4.4). Held for the whole of <see cref="Publish"/>, sequence assignment
    /// included, which is what makes a snapshot taken under the same lock (<see cref="SubscribeWithSnapshot{T}"/>)
    /// unable to race a publish: either the snapshot's caller runs first and the new subscriber then
    /// sees this event delivered normally, or this publish runs first and the snapshot (captured after
    /// it releases the lock) already reflects whatever state this event caused. Delivery itself stays
    /// synchronous and in order (wave 0's promise, still true here) — the non-blocking queue is wave
    /// 4's job, not this class's.
    /// </summary>
    private readonly Lock _gate = new();
    private long _sequence;

    /// <summary>The sequence number of the most recently published event, 0 before the first one.</summary>
    public long Sequence { get { lock (_gate) return _sequence; } }

    public IDisposable Subscribe(Action<ChatEvent> handler)
    {
        lock (_gate)
        {
            var token = new object();
            _subscribers[token] = handler;
            return new Subscription(_subscribers, token);
        }
    }

    /// <summary>
    /// Structural events (assistant message, tool started/result, turn started/completed, ask
    /// raised/resolved, user message, task changed, generation attempt, llm-turn-started/finished,
    /// notice — see <see cref="QueuedSubscription.CoalesceKeyFor"/> for the exact split) are never
    /// dropped by a queued subscription (ADR_20260910-2 §4.3). A subscriber that cannot keep up piles
    /// them up instead of losing any; past this many UNDELIVERED structural events the subscription is
    /// more useful resynced from a fresh snapshot than fed a backlog nobody asked to watch replayed
    /// minutes late. 512 is generous for a UI socket — a normal turn produces a few dozen structural
    /// events, so only a truly stalled connection (closed laptop lid, dead network) ever reaches this —
    /// and small enough that the worst case (a few hundred small event records held by one stuck
    /// subscriber) is not worth budgeting memory for.
    /// </summary>
    public const int StructuralOverflowThreshold = 512;

    /// <summary>
    /// Registers <paramref name="handler"/> on a bounded, per-subscriber queue instead of calling it
    /// synchronously from inside <see cref="Publish"/> — wave 4 (ADR_20260910-2 §4.3): "публикация не
    /// ждёт подписчиков". <see cref="Publish"/> only ever touches this subscription's
    /// <see cref="QueuedSubscription.Enqueue"/>, which does in-memory list/dictionary bookkeeping and
    /// never awaits — the actual work <paramref name="handler"/> does (e.g. a WebSocket send) runs on a
    /// dedicated consumer task, so a subscriber slow to drain never slows the chat that is publishing.
    /// <para>
    /// Consecutive text deltas and reasoning chunks for the same bubble are coalesced (concatenated);
    /// tool-call and progress-tree ticks for the same call/node keep only the latest; every other event
    /// is structural and counts toward <see cref="StructuralOverflowThreshold"/>. Crossing it detaches
    /// the subscription (unsubscribes, stops its consumer) and calls <paramref name="onDetached"/> so
    /// the caller can resync — a fresh subscription plus a fresh snapshot — instead of ever replaying a
    /// stale backlog. The synchronous <see cref="Subscribe"/> above is unaffected and stays the right
    /// choice for a cheap in-process subscriber (CLI console, <c>chat run</c>, tests) that wants to see
    /// every event before the turn's own <c>SendAsync</c> returns.
    /// </para>
    /// </summary>
    public IDisposable SubscribeQueued(Func<ChatEvent, Task> handler, Action onDetached)
    {
        lock (_gate)
        {
            var token = new object();
            var sub = new QueuedSubscription(_subscribers, token, handler, onDetached);
            _subscribers[token] = sub.Enqueue;
            return sub;
        }
    }

    /// <summary>
    /// <see cref="SubscribeQueued"/> plus an atomically-captured snapshot — closes the gap wave 4 left
    /// open (ADR_20260910-2 plan, wave 4 "Открыто"): an overflow-detach used to call
    /// <see cref="SubscribeQueued"/> to get a fresh queue, then build a resync snapshot separately and
    /// later (a fresh <c>chat.opened</c>, off the gate, sometimes on a different connection's own
    /// query). Between those two moments the feed could publish events that land BOTH in the new queue
    /// (already subscribed) AND in the snapshot (captured after them) — a client resynced this way saw
    /// them twice. Registering the subscription and calling <paramref name="captureSnapshot"/> inside
    /// the same <see cref="_gate"/> acquisition makes the two moments one: the returned snapshot
    /// reflects exactly the state as of the sequence number the new queue starts delivering after, the
    /// same guarantee <see cref="SubscribeWithSnapshot{T}"/> already gives a normal (non-queued)
    /// subscriber.
    /// </summary>
    public (T Snapshot, IDisposable Subscription) SubscribeQueuedWithSnapshot<T>(
        Func<ChatEvent, Task> handler, Action onDetached, Func<T> captureSnapshot)
    {
        lock (_gate)
        {
            var token = new object();
            var sub = new QueuedSubscription(_subscribers, token, handler, onDetached);
            _subscribers[token] = sub.Enqueue;
            var snapshot = captureSnapshot();
            return (snapshot, sub);
        }
    }

    /// <summary>
    /// Atomically subscribes and captures a caller-supplied snapshot of whatever state this feed's
    /// events describe — the tmux-style "снимок плюс поток" a client reconnecting mid-turn needs
    /// (ADR_20260910-2 §4.4). <paramref name="captureSnapshot"/> runs under the same lock
    /// <see cref="Publish"/> holds for the whole of an event's delivery, so it can never observe a
    /// half-published event, and the returned <see cref="Sequence"/> is exactly the last one already
    /// reflected in the snapshot — every event after it, and only those, reach <paramref name="handler"/>.
    /// </summary>
    public (T Snapshot, long Sequence, IDisposable Subscription) SubscribeWithSnapshot<T>(
        Func<T> captureSnapshot, Action<ChatEvent> handler)
    {
        lock (_gate)
        {
            var snapshot = captureSnapshot();
            var token = new object();
            _subscribers[token] = handler;
            return (snapshot, _sequence, new Subscription(_subscribers, token));
        }
    }

    /// <summary>
    /// Runs <paramref name="attach"/> (typically: mark one more connection a watcher) and
    /// <paramref name="captureSnapshot"/> under the same lock <see cref="Publish"/> uses, without
    /// registering a new <see cref="IChatFeed"/> subscriber. For a chat with a single, permanently
    /// registered wire subscriber that fans events out to whichever connections are currently marked
    /// as watching it (<c>ChatFeedWireSubscriber</c>) — <paramref name="attach"/> is that marking, and
    /// doing it under this lock is what makes "which events this connection sees" agree exactly with
    /// "what the snapshot already reflects": an event published before <paramref name="attach"/> runs
    /// is in the snapshot and was never sent to this not-yet-watching connection; one published after
    /// is not in the snapshot and reaches the permanent subscriber, which by then already sees this
    /// connection as a watcher.
    /// </summary>
    public T SnapshotUnderGate<T>(Func<T> captureSnapshot, Action? attach = null)
    {
        lock (_gate)
        {
            attach?.Invoke();
            return captureSnapshot();
        }
    }

    /// <summary>Delivers <paramref name="chatEvent"/> to every current subscriber, in registration
    /// order isn't guaranteed by <see cref="ConcurrentDictionary{TKey,TValue}"/> — no consumer needs
    /// cross-subscriber ordering, only per-subscriber in-order delivery, which a synchronous foreach
    /// already gives each one individually.
    /// <para>A subscriber that throws is swallowed rather than breaking the rest, and never breaks the
    /// chat's own turn: this is diagnostic, not authoritative, output, and the chat that raised the
    /// event must not fail because someone watching it misbehaved.</para>
    /// <para><paramref name="mutateUnderGate"/> — wave 1's addition — lets the caller fold the state
    /// change this event announces (a live-text append, a message added to the conversation, clearing
    /// the live partial) into the same critical section as the sequence bump and delivery. Without it,
    /// a snapshot taken between "chat mutates its state" and "chat calls Publish" would either miss the
    /// mutation (if it ran first) and then also miss the event (already delivered to a not-yet-current
    /// subscriber list) or double-count it (state already reflected, event delivered again). Run before
    /// the sequence is assigned: the number identifies the state change, not just the announcement of
    /// it.</para>
    /// </summary>
    public long Publish(ChatEvent chatEvent, Action? mutateUnderGate = null)
    {
        lock (_gate)
        {
            mutateUnderGate?.Invoke();
            _sequence++;

            foreach (var handler in _subscribers.Values)
            {
                try { handler(chatEvent); }
                catch { /* a misbehaving subscriber must not break the chat that is publishing */ }
            }

            return _sequence;
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly ConcurrentDictionary<object, Action<ChatEvent>> _subscribers;
        private object? _token;

        public Subscription(ConcurrentDictionary<object, Action<ChatEvent>> subscribers, object token)
        {
            _subscribers = subscribers;
            _token = token;
        }

        public void Dispose()
        {
            var token = Interlocked.Exchange(ref _token, null);
            if (token is not null) _subscribers.TryRemove(token, out _);
        }
    }

    /// <summary>
    /// The queue backing <see cref="SubscribeQueued"/>: a lag-aware mailbox plus one consumer task.
    /// <see cref="Enqueue"/> runs inside <see cref="Publish"/>'s lock and must stay allocation-light and
    /// synchronous — it is the only thing standing between a slow subscriber and the chat loop.
    /// Delivery order for events actually forwarded is preserved (single consumer, FIFO queue); events
    /// merged by <see cref="CoalesceKeyFor"/> keep the position of their first still-undelivered entry,
    /// so a coalesced delta does not jump ahead of a structural event enqueued in between.
    /// </summary>
    private sealed class QueuedSubscription : IDisposable
    {
        private sealed class QueuedItem
        {
            public required ChatEvent Event { get; set; }
            public string? CoalesceKey { get; init; }
        }

        private readonly ConcurrentDictionary<object, Action<ChatEvent>> _subscribers;
        private readonly object _token;
        private readonly Func<ChatEvent, Task> _handler;
        private readonly Action _onDetached;

        private readonly Lock _queueLock = new();
        private readonly LinkedList<QueuedItem> _queue = new();
        private readonly Dictionary<string, LinkedListNode<QueuedItem>> _coalesced = new();
        private int _structuralCount;
        private readonly SemaphoreSlim _signal = new(0);
        private int _stoppedFlag;
        private readonly Task _consumer;

        public QueuedSubscription(
            ConcurrentDictionary<object, Action<ChatEvent>> subscribers, object token,
            Func<ChatEvent, Task> handler, Action onDetached)
        {
            _subscribers = subscribers;
            _token = token;
            _handler = handler;
            _onDetached = onDetached;
            _consumer = Task.Run(ConsumeAsync);
        }

        /// <summary>The delegate registered into the feed's subscriber dictionary — synchronous,
        /// non-blocking, safe to call from inside <see cref="ChatFeed.Publish"/>'s lock.</summary>
        public void Enqueue(ChatEvent e)
        {
            if (Volatile.Read(ref _stoppedFlag) != 0) return;

            bool overflowed;
            lock (_queueLock)
            {
                var key = CoalesceKeyFor(e);
                if (key is not null && _coalesced.TryGetValue(key, out var node))
                {
                    node.Value.Event = Merge(node.Value.Event, e);
                    overflowed = false;
                }
                else
                {
                    var newNode = _queue.AddLast(new QueuedItem { Event = e, CoalesceKey = key });
                    if (key is not null) _coalesced[key] = newNode;
                    else
                    {
                        _structuralCount++;
                        // A structural event is a barrier: nothing after it may merge into an entry
                        // before it. Otherwise "delta A, attempt (clears the bubble), delta B" under lag
                        // would deliver "AB" ahead of the attempt, and the client's clear would then
                        // erase the real answer.
                        _coalesced.Clear();
                    }
                    overflowed = _structuralCount > StructuralOverflowThreshold;
                }
            }
            _signal.Release();
            if (overflowed) Detach();
        }

        private async Task ConsumeAsync()
        {
            while (Volatile.Read(ref _stoppedFlag) == 0)
            {
                await _signal.WaitAsync();
                if (Volatile.Read(ref _stoppedFlag) != 0) break;

                QueuedItem item;
                lock (_queueLock)
                {
                    var node = _queue.First;
                    if (node is null) continue; // a detach's own Release can wake this with nothing queued
                    _queue.RemoveFirst();
                    // Only if it still points here — a barrier may have cleared it, and a later entry
                    // under the same key must not be unlinked by this one leaving.
                    if (node.Value.CoalesceKey is { } key &&
                        _coalesced.TryGetValue(key, out var current) && current == node)
                        _coalesced.Remove(key);
                    item = node.Value;
                }

                try { await _handler(item.Event); }
                catch { /* a misbehaving subscriber must not break the chat that is publishing */ }

                // Counted as undelivered until the handler has actually finished with it: an event a
                // stuck connection is sitting on is exactly as undelivered as one still queued.
                if (item.CoalesceKey is null)
                    lock (_queueLock) _structuralCount--;
            }
        }

        /// <summary>Stops delivery and unregisters from the feed exactly once, whichever of
        /// <see cref="Detach"/> (overflow) or <see cref="Dispose"/> (normal unsubscribe) gets there
        /// first.</summary>
        private bool TryStop()
        {
            if (Interlocked.CompareExchange(ref _stoppedFlag, 1, 0) != 0) return false;
            _subscribers.TryRemove(_token, out _);
            _signal.Release(); // wake the consumer so it observes the stop flag and exits
            return true;
        }

        /// <summary>Called when the structural backlog crosses <see cref="StructuralOverflowThreshold"/>
        /// — unsubscribes and tells the caller to resync (ADR_20260910-2 §4.3: "отцепляется и
        /// подключается заново через снимок"), unlike a plain <see cref="Dispose"/>, which is a normal
        /// unsubscribe (e.g. <c>RuntimeClosed</c>) and must not trigger a resync nobody asked for.</summary>
        private void Detach()
        {
            if (TryStop()) _onDetached();
        }

        public void Dispose() => TryStop();

        /// <summary>Which events coalesce, and by what key — ADR_20260910-2 §4.3's three lag rules.
        /// Text deltas and reasoning chunks concatenate per bubble (<see cref="ChatDelta.MsgIndex"/> /
        /// <see cref="ChatReasoning.MsgIndex"/>); a tool call's flat progress bar and one progress-tree
        /// node each keep only the latest tick per call/node id. Everything else returns null — never
        /// coalesced, always counted as structural.</summary>
        internal static string? CoalesceKeyFor(ChatEvent e) => e switch
        {
            ChatDelta d => $"delta:{d.MsgIndex}",
            ChatReasoning r => $"reasoning:{r.MsgIndex}",
            ChatToolProgress p => $"toolprogress:{p.Call.Id}",
            ChatProgressNode n => $"progressnode:{n.TreeId}:{n.Node.Id}",
            _ => null
        };

        private static ChatEvent Merge(ChatEvent existing, ChatEvent incoming) => (existing, incoming) switch
        {
            (ChatDelta a, ChatDelta b) => a with { Text = a.Text + b.Text },
            (ChatReasoning a, ChatReasoning b) => a with { Text = a.Text + b.Text },
            _ => incoming // tool/progress-tree ticks: latest wins
        };
    }
}
