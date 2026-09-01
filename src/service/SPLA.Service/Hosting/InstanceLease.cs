using Microsoft.Extensions.Logging;
using SPLA.Domain.Project;
using SPLA.Runtime;

namespace SPLA.Service;

/// <summary>
/// Decides when a service instance has nothing left to stay alive for.
///
/// <para><b>Lease, not ownership.</b> The obvious rule — "whoever started the child kills it" — breaks
/// on the ordinary case: open project A, open project B, go back to A, close the window, then
/// remember B. Under ownership, closing the window kills B mid-turn. So nothing owns an instance.
/// It lives while somebody is connected, or while there is work in flight, and goes away only after
/// a grace period in which neither is true.</para>
///
/// <para><b>Why leaving is cheap.</b> An instance holds nothing unique — chats, KV and the usage
/// tally are on disk — so shutting an idle one down costs the next warm-up, not any work. That is
/// what makes the grace period a performance knob rather than a safety one, and why it can be
/// generous without anybody having to think hard about the number.</para>
///
/// <para><b>What is never evicted.</b> Anything but <see cref="InstanceState.Idle"/>. A turn in
/// flight is obvious; a question waiting on a person and a model that stopped halfway are the two
/// that matter, because those are exactly the states somebody walks back to their desk to deal with.
/// See <see cref="InstanceStates.MayEvict"/>.</para>
/// </summary>
internal sealed class InstanceLease : IDisposable
{
    private readonly ConnectionHub _hub;
    private readonly AgentRuntimeRegistry _registry;
    private readonly TimeSpan _grace;
    private readonly TimeSpan _stallAfter;
    private readonly ILogger _log;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;

    private int _holders;
    private DateTime? _freeSince;

    /// <param name="grace">How long "nobody here, nothing running" must hold before the instance
    /// goes away. <see cref="TimeSpan.Zero"/> or less disables eviction entirely — what a
    /// hand-started daemon wants, since nobody expects a service they launched to leave on its own.</param>
    /// <param name="stallAfter">Silence after which a running turn counts as stopped halfway.</param>
    public InstanceLease(
        ConnectionHub hub, AgentRuntimeRegistry registry, TimeSpan grace, TimeSpan stallAfter, ILogger log)
    {
        _hub = hub;
        _registry = registry;
        _grace = grace;
        _stallAfter = stallAfter;
        _log = log;
        _loop = grace > TimeSpan.Zero ? Task.Run(() => WatchAsync(_cts.Token)) : Task.CompletedTask;
    }

    /// <summary>Raised once, when the lease has expired. The host stops itself; the lease never
    /// terminates a process on its own, so a caller with something else to wind down still can.</summary>
    public event Action? Expired;

    /// <summary>
    /// Registers a holder that is not a socket — the parallel console REPL, a test harness. Without
    /// this, an instance whose only user is typing at its own terminal looks unattended.
    /// </summary>
    public IDisposable Hold()
    {
        Interlocked.Increment(ref _holders);
        return new Holder(this);
    }

    private void Release() => Interlocked.Decrement(ref _holders);

    private async Task WatchAsync(CancellationToken ct)
    {
        // A minute's resolution on a grace measured in minutes. Polling rather than reacting to
        // connect/disconnect because the other half of the condition — state — changes on its own
        // schedule anyway, and one clock is easier to reason about than two edges.
        var tick = TimeSpan.FromSeconds(Math.Clamp(_grace.TotalSeconds / 10, 5, 60));

        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(tick, ct); }
            catch (OperationCanceledException) { return; }

            var state = _registry.State(_stallAfter);
            var free = _hub.Count == 0 && Volatile.Read(ref _holders) == 0 && InstanceStates.MayEvict(state);

            if (!free)
            {
                _freeSince = null;
                continue;
            }

            _freeSince ??= DateTime.UtcNow;
            if (DateTime.UtcNow - _freeSince < _grace) continue;

            _log.LogInformation(
                "Shutting down: idle lease expired — no clients and nothing running for {Grace}.", _grace);
            Expired?.Invoke();
            return;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _loop.Wait(TimeSpan.FromSeconds(2)); } catch { /* shutting down anyway */ }
        _cts.Dispose();
    }

    private sealed class Holder(InstanceLease lease) : IDisposable
    {
        private int _released;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0) lease.Release();
        }
    }
}
