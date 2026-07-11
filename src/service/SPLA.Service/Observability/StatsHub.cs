using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using SPLA.Observability.Collection;

namespace SPLA.Service.Observability;

/// <summary>One connected stats viewer: its socket plus the identity that decides its scope (an admin
/// sees the whole server, an ordinary user only their own activity).</summary>
internal sealed class StatsSubscriber
{
    public required WebSocket Socket { get; init; }
    public required string? UserKey { get; init; }
    public required bool IsAdmin { get; init; }
    public readonly SemaphoreSlim SendLock = new(1, 1);
}

/// <summary>
/// The real-time stats firehose: the set of connected <c>/stats/live</c> viewers. The collector raises
/// an event per completed activity, which this hub pushes instantly to the admins and to the acting
/// user (per-user slice) — WebSocket push, not client polling. A periodic snapshot push keeps the KPI
/// tiles / gauges current without the page ever polling.
/// </summary>
internal sealed class StatsHub
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly ConcurrentDictionary<StatsSubscriber, byte> _subscribers = new();

    public void Add(StatsSubscriber subscriber) => _subscribers[subscriber] = 0;
    public void Remove(StatsSubscriber subscriber) => _subscribers.TryRemove(subscriber, out _);

    /// <summary>Fan one completed activity out live — to every admin, and to the user who caused it.</summary>
    public Task PushEventAsync(RecentEvent evt)
        => FanOutAsync(
            s => s.IsAdmin || (evt.UserKey != null && s.UserKey == evt.UserKey),
            new { type = "event", @event = evt });

    /// <summary>Push each subscriber their own scoped snapshot (KPIs + gauges).</summary>
    public async Task PushSnapshotsAsync(TelemetryCollector collector)
    {
        foreach (var subscriber in _subscribers.Keys)
            await SendAsync(subscriber, new { type = "snapshot", snapshot = collector.SnapshotFor(subscriber.UserKey, subscriber.IsAdmin) });
    }

    /// <summary>Send the initial snapshot to a subscriber that just connected.</summary>
    public Task SendInitialAsync(StatsSubscriber subscriber, TelemetryCollector collector)
        => SendAsync(subscriber, new { type = "snapshot", snapshot = collector.SnapshotFor(subscriber.UserKey, subscriber.IsAdmin) });

    private async Task FanOutAsync(Func<StatsSubscriber, bool> filter, object payload)
    {
        foreach (var subscriber in _subscribers.Keys)
            if (filter(subscriber))
                await SendAsync(subscriber, payload);
    }

    private static async Task SendAsync(StatsSubscriber subscriber, object payload)
    {
        if (subscriber.Socket.State != WebSocketState.Open) return;
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, Json);
        await subscriber.SendLock.WaitAsync();
        try
        {
            if (subscriber.Socket.State == WebSocketState.Open)
                await subscriber.Socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
        }
        catch { /* a dead viewer must not break the fan-out */ }
        finally { subscriber.SendLock.Release(); }
    }
}
