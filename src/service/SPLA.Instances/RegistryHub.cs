using System.Collections.Concurrent;
using SPLA.Domain.Project;

namespace SPLA.Instances;

/// <summary>
/// The set of instances currently registered, and the live state each one last reported.
///
/// <para><b>Liveness is the channel.</b> An instance is present exactly while its registration
/// connection is open. There is no heartbeat to tune, no pid to check, and nothing to prune on a
/// timer — the transport already answers the question, and it answers it the same way for an
/// instance on this machine and one three hops away. A hub that had to decide "is it still there?"
/// from timestamps would be inventing an answer the socket already has.</para>
///
/// <para><b>No storage.</b> Everything here dies with the process, deliberately. The hub is an index,
/// not a source of truth: an instance that outlives a hub restart simply registers again, and losing
/// the hub loses nothing but the view. This is the same rule that keeps the machine-level project
/// registry a convenience — delete it and no project disappears.</para>
///
/// <para>Transport lives outside this class. It is handed a way to talk back to each instance and
/// never learns what that is, so the same hub serves a WebSocket endpoint in the CLI and one inside
/// the server host.</para>
/// </summary>
public sealed class RegistryHub
{
    private sealed record Entry(
        string InstanceId,
        string ProjectId,
        string? ProjectName,
        InstanceInfo Info,
        Func<string, StopFrame, Task> Control)
    {
        public InstanceState State { get; set; } = InstanceState.Idle;
        public int Clients { get; set; }
        public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Raised whenever the set or any member's state changes — one signal for observers to
    /// re-read. Coarse on purpose: the list is small, and a diff an observer has to reconcile is a
    /// list an observer can get wrong.</summary>
    public event Action? Changed;

    /// <summary>
    /// Takes an instance's registration and returns the handle that keeps it present. Disposing the
    /// handle — which the transport does when the connection ends, however it ends — removes it.
    /// </summary>
    /// <param name="control">How to send a control frame back to this instance. The hub relays a stop
    /// rather than performing one: only the instance knows whether it may go.</param>
    public IDisposable Register(RegisterFrame frame, Func<string, StopFrame, Task> control)
    {
        var id = string.IsNullOrWhiteSpace(frame.Info.InstanceId)
            ? Guid.NewGuid().ToString("N")
            : frame.Info.InstanceId;

        var entry = new Entry(id, frame.ProjectId, frame.ProjectName, frame.Info, control);

        // Last registration wins. A reconnecting instance keeps its id, so the natural outcome of a
        // dropped-and-restored channel is one entry replaced, never two claiming the same project.
        _entries[id] = entry;
        Changed?.Invoke();
        return new Registration(this, id);
    }

    /// <summary>Records what an instance says it is doing. Unknown ids are ignored: a status that
    /// arrives after the channel closed is late news, not an error worth failing a socket over.</summary>
    public void Report(string instanceId, StatusFrame status)
    {
        if (!_entries.TryGetValue(instanceId, out var entry)) return;

        InstanceStates.TryParse(status.State, out var state);
        entry.State = state;
        entry.Clients = status.Clients;
        entry.LastSeen = DateTimeOffset.UtcNow;
        Changed?.Invoke();
    }

    /// <summary>Everything registered, newest first.</summary>
    public IReadOnlyList<RegisteredInstanceDto> List()
        => _entries.Values
            .OrderByDescending(e => e.Info.StartedAt)
            .Select(e => new RegisteredInstanceDto
            {
                ProjectId = e.ProjectId,
                ProjectName = e.ProjectName,
                Info = e.Info,
                State = InstanceStates.Name(e.State),
                Clients = e.Clients,
                LastSeen = e.LastSeen
            })
            .ToList();

    /// <summary>
    /// Asks one instance to stop, through its own channel. False = no such instance here.
    ///
    /// <para>The hub never decides whether the stop happens: an instance mid-turn refuses, and it is
    /// the only party that knows that. Relaying keeps the hub an index rather than an authority over
    /// processes it does not own.</para>
    /// </summary>
    public async Task<bool> RequestStopAsync(string instanceId, bool force)
    {
        if (!_entries.TryGetValue(instanceId, out var entry)) return false;
        await entry.Control(RegistryFrames.Stop, new StopFrame { Force = force });
        return true;
    }

    private void Remove(string instanceId)
    {
        if (_entries.TryRemove(instanceId, out _)) Changed?.Invoke();
    }

    private sealed class Registration(RegistryHub hub, string instanceId) : IDisposable
    {
        private int _disposed;

        /// <summary>The instance id the hub assigned, so the transport can attribute later frames.</summary>
        public string InstanceId => instanceId;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0) hub.Remove(instanceId);
        }
    }

    /// <summary>The id a registration handle carries — the transport needs it to attribute status
    /// frames, and asking for it here keeps <see cref="Registration"/> private.</summary>
    public static string IdOf(IDisposable registration)
        => registration is Registration r ? r.InstanceId : "";
}
