using SPLA.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// ADR_20260910-2 §4.3, wave 4: "публикация не ждёт подписчиков". <see cref="ChatFeed.SubscribeQueued"/>
/// is the mechanism — <see cref="ChatFeed.Publish"/> only ever touches the subscriber's fast in-memory
/// queue, a dedicated consumer task does the (possibly slow) actual delivery.
/// </summary>
public class ChatFeedQueuedSubscriptionTests
{
    private static ChatDelta Delta(string chatId, int msgIndex, string text) => new(msgIndex, text) { ChatId = chatId };
    private static ChatToolStarted Started(string chatId, string callId) =>
        new(new SPLA.Domain.Models.ToolCall
        {
            Id = callId,
            Function = new SPLA.Domain.Models.FunctionCall { Name = "t", Arguments = "{}" }
        })
        { ChatId = chatId };

    /// <summary>
    /// The core promise: publishing to a subscriber whose handler is arbitrarily slow costs the
    /// publisher nothing. Proven by timing <see cref="ChatFeed.Publish"/> itself, not the subscriber's
    /// own delivery — a synchronous <see cref="ChatFeed.Subscribe"/> subscriber with the same slow
    /// handler would make every single publish take the delay; the queued one must not.
    /// </summary>
    [Fact]
    public async Task Publish_is_not_slowed_by_a_slow_queued_subscriber()
    {
        var feed = new ChatFeed();
        var release = new TaskCompletionSource();
        var delivered = new List<ChatEvent>();

        using var subscription = feed.SubscribeQueued(async e =>
        {
            await release.Task; // the "slow window" — nothing unblocks this until the test says so
            delivered.Add(e);
        }, onDetached: () => { });

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < 50; i++)
            feed.Publish(Delta("c1", 0, "x"));
        sw.Stop();

        // 50 publishes into a subscriber blocked forever must still return near-instantly.
        Assert.True(sw.ElapsedMilliseconds < 500, $"Publish took {sw.ElapsedMilliseconds}ms — should not wait on the subscriber");
        Assert.Empty(delivered); // nothing delivered yet — the consumer is still blocked on release.Task

        release.SetResult();
        await WaitUntil(() => delivered.Count > 0);
    }

    /// <summary>Consecutive text deltas for the same bubble concatenate into one delivered event instead
    /// of arriving one at a time — ADR §4.3's first lag rule.</summary>
    [Fact]
    public async Task Consecutive_deltas_for_the_same_bubble_coalesce()
    {
        var feed = new ChatFeed();
        var release = new TaskCompletionSource();
        var delivered = new List<ChatEvent>();

        using var subscription = feed.SubscribeQueued(async e =>
        {
            await release.Task; // hold the consumer so every publish below lands before the first dequeue
            delivered.Add(e);
        }, onDetached: () => { });

        feed.Publish(Delta("c1", 0, "he"));
        feed.Publish(Delta("c1", 0, "ll"));
        feed.Publish(Delta("c1", 0, "o"));
        release.SetResult();

        await WaitUntil(() => delivered.Count > 0);
        await Task.Delay(20); // let the consumer drain fully — coalescing means at most one item here

        // The consumer may already hold the first chunk in flight before the rest are published — an
        // in-flight item cannot be merged into — so at most two deliveries, and never a lost chunk.
        var deltas = delivered.Cast<ChatDelta>().ToList();
        Assert.InRange(deltas.Count, 1, 2);
        Assert.Equal("hello", string.Concat(deltas.Select(d => d.Text)));
    }

    /// <summary>Order is preserved for what actually gets delivered — a structural event enqueued
    /// between two coalescable deltas must not be jumped by either delta's later merge.</summary>
    [Fact]
    public async Task Order_is_preserved_across_coalesced_and_structural_events()
    {
        var feed = new ChatFeed();
        var delivered = new List<ChatEvent>();
        var gate = new object();

        using var subscription = feed.SubscribeQueued(e =>
        {
            lock (gate) delivered.Add(e);
            return Task.CompletedTask;
        }, onDetached: () => { });

        feed.Publish(Delta("c1", 0, "a"));
        feed.Publish(Started("c1", "call-1"));
        feed.Publish(Delta("c1", 0, "b"));

        await WaitUntil(() =>
        {
            lock (gate) return delivered.Count >= 3;
        });

        List<ChatEvent> snapshot;
        lock (gate) snapshot = delivered.ToList();

        Assert.Equal(3, snapshot.Count);
        Assert.IsType<ChatDelta>(snapshot[0]);
        Assert.Equal("a", ((ChatDelta)snapshot[0]).Text);
        Assert.IsType<ChatToolStarted>(snapshot[1]);
        Assert.IsType<ChatDelta>(snapshot[2]);
        Assert.Equal("b", ((ChatDelta)snapshot[2]).Text);
    }

    /// <summary>A structural event is a barrier for coalescing even when the consumer lags: the delta
    /// after it must not merge back into the delta before it (an attempt clearing the bubble between
    /// them would otherwise erase the real answer on the client).</summary>
    [Fact]
    public async Task A_structural_event_stops_coalescing_across_it_under_lag()
    {
        var feed = new ChatFeed();
        var release = new TaskCompletionSource();
        var delivered = new List<ChatEvent>();
        var gate = new object();

        using var subscription = feed.SubscribeQueued(async e =>
        {
            await release.Task;
            lock (gate) delivered.Add(e);
        }, onDetached: () => { });

        feed.Publish(Delta("c1", 0, "a"));
        feed.Publish(Started("c1", "call-1"));
        feed.Publish(Delta("c1", 0, "b"));
        feed.Publish(Delta("c1", 0, "c"));
        release.SetResult();

        await WaitUntil(() => { lock (gate) return delivered.Count >= 3; });
        await Task.Delay(20);

        List<ChatEvent> got;
        lock (gate) got = delivered.ToList();
        Assert.Equal(3, got.Count);
        Assert.Equal("a", Assert.IsType<ChatDelta>(got[0]).Text);
        Assert.IsType<ChatToolStarted>(got[1]);
        Assert.Equal("bc", Assert.IsType<ChatDelta>(got[2]).Text);
    }

    /// <summary>Overflowing the structural queue bound detaches the subscription (stops delivery, calls
    /// <c>onDetached</c>) instead of ever growing without bound or replaying a stale backlog — ADR §4.3:
    /// "отцепляется и подключается заново через снимок". A fresh <see cref="ChatFeed.SubscribeQueued"/>
    /// after detach (the resync a real caller performs alongside a fresh snapshot) sees only events
    /// published from that point on — a consistent continuation, not a gap-ridden replay.</summary>
    [Fact]
    public async Task Overflow_detaches_and_a_fresh_subscription_after_it_is_consistent()
    {
        var feed = new ChatFeed();
        var release = new TaskCompletionSource();
        var detached = new TaskCompletionSource();

        var subscription = feed.SubscribeQueued(async e =>
        {
            await release.Task; // never let the consumer drain — forces the structural backlog to grow
        }, onDetached: () => detached.TrySetResult());

        // One more structural event than the threshold allows.
        for (var i = 0; i <= ChatFeed.StructuralOverflowThreshold; i++)
            feed.Publish(Started("c1", $"call-{i}"));

        // Proves the overflow was noticed without ever unblocking the slow handler — bounded so a
        // regression here fails fast instead of hanging the run.
        Assert.True(await Task.WhenAny(detached.Task, Task.Delay(5000)) == detached.Task, "subscription never detached");
        release.SetResult();
        subscription.Dispose();

        // Resync: a fresh queued subscription must only ever see events published after it exists.
        var afterResync = new List<ChatEvent>();
        using var fresh = feed.SubscribeQueued(e =>
        {
            afterResync.Add(e);
            return Task.CompletedTask;
        }, onDetached: () => { });

        feed.Publish(Started("c1", "post-resync"));
        await WaitUntil(() => afterResync.Count > 0);

        var only = Assert.Single(afterResync.Cast<ChatToolStarted>());
        Assert.Equal("post-resync", only.Call.Id);
    }

    /// <summary>Parallel publishers (e.g. several tool calls running concurrently, each publishing its
    /// own progress) still leave one subscriber with a single, sequence-consistent total order: nothing
    /// about queuing per subscriber reorders what <see cref="ChatFeed.Publish"/> already serializes
    /// under its own gate.</summary>
    [Fact]
    public async Task Parallel_publishers_keep_one_subscribers_order_consistent_with_publish_sequence()
    {
        var feed = new ChatFeed();
        var delivered = new List<(ChatEvent Event, long Sequence)>();
        var gate = new object();

        using var subscription = feed.SubscribeQueued(e =>
        {
            lock (gate) delivered.Add((e, feed.Sequence));
            return Task.CompletedTask;
        }, onDetached: () => { });

        // 4 × 100 stays under StructuralOverflowThreshold: a subscriber lagging behind four producer
        // threads is allowed to detach, and that is a different test.
        const int perPublisher = 100;
        var publishers = Enumerable.Range(0, 4).Select(p => Task.Run(() =>
        {
            for (var i = 0; i < perPublisher; i++)
                feed.Publish(Started("c1", $"p{p}-call-{i}"));
        })).ToArray();
        await Task.WhenAll(publishers);

        await WaitUntil(() =>
        {
            lock (gate) return delivered.Count == 4 * perPublisher;
        });

        // Every structural event from every publisher arrived (never dropped, ADR §4.3), and the
        // sequence numbers observed at delivery time are non-decreasing — this subscriber's own
        // delivery order agrees with the feed's single total publish order, however interleaved the
        // four producer threads were.
        List<(ChatEvent Event, long Sequence)> snapshot;
        lock (gate) snapshot = delivered.ToList();

        Assert.Equal(4 * perPublisher, snapshot.Select(d => ((ChatToolStarted)d.Event).Call.Id).Distinct().Count());
        for (var i = 1; i < snapshot.Count; i++)
            Assert.True(snapshot[i].Sequence >= snapshot[i - 1].Sequence);
    }

    /// <summary>
    /// PLAN_20260910-2 wave 4's "Открыто", closed by wave 5: resubscribing after an overflow-detach and
    /// capturing the resync snapshot used to be two separate steps — the new queue went live first, the
    /// snapshot (built later, off the feed's gate) second — so an event published in between showed up
    /// twice: once already reflected in the snapshot, once replayed by the new queue.
    /// <see cref="ChatFeed.SubscribeQueuedWithSnapshot{T}"/> does both under the same gate acquisition,
    /// so a concurrent publisher racing the resubscribe can never land an event on both sides.
    /// </summary>
    [Fact]
    public async Task SubscribeQueuedWithSnapshot_never_double_delivers_against_a_concurrent_publisher()
    {
        var feed = new ChatFeed();
        var state = new List<string>(); // mirrors the chat state a real snapshot builder would read

        // Seed some events before the resync, mutating "state" the same way ChatRuntime.Emit(event,
        // mutate) folds a state change into Publish's own critical section.
        for (var i = 0; i < 20; i++)
        {
            var id = $"seed-{i}";
            feed.Publish(Started("c1", id), mutateUnderGate: () => state.Add(id));
        }

        var delivered = new List<string>();
        var gate = new object();
        var stop = 0;

        // Keeps publishing (and mutating "state") while the resync below runs — the exact race the
        // wave-4 note describes.
        var publisher = Task.Run(() =>
        {
            var i = 0;
            while (Volatile.Read(ref stop) == 0)
            {
                var id = $"concurrent-{i++}";
                feed.Publish(Started("c1", id), mutateUnderGate: () => state.Add(id));
            }
        });

        await Task.Delay(20); // let the publisher get going before the resync races it

        var (snapshot, subscription) = feed.SubscribeQueuedWithSnapshot(
            e => { lock (gate) delivered.Add(((ChatToolStarted)e).Call.Id); return Task.CompletedTask; },
            onDetached: () => { },
            captureSnapshot: () => state.ToList());

        await Task.Delay(50);
        Volatile.Write(ref stop, 1);
        await publisher;
        await WaitUntil(() => { lock (gate) return delivered.Count > 0; });
        await Task.Delay(20); // let the consumer drain whatever is left in its queue
        subscription.Dispose();

        List<string> got;
        lock (gate) got = delivered.ToList();

        // No event the snapshot already reflects was also replayed by the new subscription...
        Assert.Empty(snapshot.Intersect(got));
        // ...and nothing published by the time the publisher stopped fell in the gap between the two.
        Assert.Empty(state.Except(snapshot).Except(got));
    }

    private static async Task WaitUntil(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("condition never became true");
            await Task.Delay(5);
        }
    }
}
