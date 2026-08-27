using System.Collections.Concurrent;
using SPLA.Domain.Project;

namespace SPLA.Instances;

/// <summary>
/// The set of participants currently registered, and the live state each one last reported.
///
/// <para><b>Participants, not only agents.</b> Anything attached to a project registers here and
/// carries a <see cref="ParticipantRoles">role</see> — the agent holding it, a window looking at it,
/// the machine's tray shell. That is what makes the hub the one place able to address them: "raise
/// the window for this project" and "close everything on this project" are questions an index of
/// agents alone cannot answer, and every orphaned window came from trying.</para>
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
        string Role,
        Func<string, object, Task> Control)
    {
        public InstanceState State { get; set; } = InstanceState.Idle;
        public int Clients { get; set; }
        public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Every project id (manifest path) this hub has ever accepted a registration for, with the name
    /// that registration gave it. Kept separately from <see cref="_entries"/> — and, unlike it,
    /// <b>never</b> pruned when an instance disconnects — because the whole reason a caller looks a
    /// project up by name instead of by path is that the path is exactly what they do not want to
    /// carry around; if this forgot a name the moment its instance went idle and stopped, name lookup
    /// would only ever work for projects that happen to be running at that exact moment, which is not
    /// a lookup a client could build anything on top of.
    ///
    /// <para><b>Still not storage.</b> This dies with the hub process precisely like <see
    /// cref="_entries"/> does — it is process memory, not a file — so a project this hub has never once
    /// seen register (a fresh checkout nobody has run <c>spla serve</c> in yet, or one only ever started
    /// against a different hub) cannot be found by name here. Only by its manifest path, which needs no
    /// lookup at all. That gap is a deliberate non-feature: scanning the disk for manifests to fill it
    /// would turn a cheap index into a filesystem crawler, for a case a path already answers.</para>
    /// </summary>
    private readonly ConcurrentDictionary<string, string?> _knownNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Raised whenever the set or any member's state changes — one signal for observers to
    /// re-read. Coarse on purpose: the list is small, and a diff an observer has to reconcile is a
    /// list an observer can get wrong.</summary>
    public event Action? Changed;

    /// <summary>
    /// Takes an instance's registration and returns the handle that keeps it present. Disposing the
    /// handle — which the transport does when the connection ends, however it ends — removes it.
    /// </summary>
    /// <param name="control">How to send a control frame back to this participant. The hub relays a
    /// request rather than performing one: only the participant knows whether it may act on it.</param>
    public IDisposable Register(RegisterFrame frame, Func<string, object, Task> control)
    {
        var id = string.IsNullOrWhiteSpace(frame.Info.InstanceId)
            ? Guid.NewGuid().ToString("N")
            : frame.Info.InstanceId;

        var role = string.IsNullOrWhiteSpace(frame.Role) ? ParticipantRoles.Agent : frame.Role;
        var entry = new Entry(id, frame.ProjectId, frame.ProjectName, frame.Info, role, control);

        // Last registration wins. A reconnecting instance keeps its id, so the natural outcome of a
        // dropped-and-restored channel is one entry replaced, never two claiming the same project.
        _entries[id] = entry;

        // Written on every registration, never removed on disconnect — see the field's own remarks on
        // why this outlives _entries. Only agents hold a project worth finding this way; a window
        // registering under the same project id would otherwise overwrite a real name with null the
        // moment it opened, which is a worse answer than simply not recording it.
        if (role == ParticipantRoles.Agent)
            _knownNames[frame.ProjectId] = frame.ProjectName;

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
                Role = e.Role,
                LastSeen = e.LastSeen
            })
            .ToList();

    /// <summary>
    /// Asks one instance to stop, through its own channel. False = no such instance here.
    ///
    /// <para>The hub never decides whether the stop happens: an instance mid-turn refuses, and it is
    /// the only party that knows that.</para>
    ///
    /// <para><b>Stopping is relayed; starting is not.</b> This class stayed an index — it holds no
    /// process handles and cannot end anything — but the hub as a deployment has since gained the
    /// power to <i>start</i> agents (<see cref="IInstanceSpawner"/>, mapped by the host, off by
    /// default). The asymmetry is the point: a stop has an owner who may refuse, while a start has
    /// nobody to ask, and refusing to start would leave a machine with no desktop unable to bring a
    /// project up at all. Recorded in ADR_20260820_apps_project-hub §4, which deliberately reverses
    /// the earlier "index, never an authority" rule rather than quietly widening it.</para>
    /// </summary>
    public async Task<bool> RequestStopAsync(string instanceId, bool force)
    {
        if (!_entries.TryGetValue(instanceId, out var entry)) return false;
        await entry.Control(RegistryFrames.Stop, new StopFrame { Force = force });
        return true;
    }

    /// <summary>
    /// Asks everything registered against one project to stop — the agent holding it and every window
    /// looking at it.
    ///
    /// <para>This is what "close the project" means, and it is the reason roles exist. Stopping the
    /// agent alone is what used to leave a window pointed at a service that would never answer again:
    /// the window was never addressed because the hub did not know it was there.</para>
    ///
    /// <para>Returns how many participants were asked, not how many complied — each one still decides,
    /// and an agent mid-turn still refuses. A caller that needs to know the outcome watches the list.</para>
    /// </summary>
    public async Task<int> RequestStopProjectAsync(string projectId, bool force)
    {
        var targets = _entries.Values
            .Where(e => string.Equals(e.ProjectId, projectId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Agents last: a window told to close after its agent has already gone would spend its final
        // moments reconnecting to nothing, which is the very flicker this is meant to remove.
        foreach (var entry in targets.OrderBy(e => e.Role == ParticipantRoles.Agent))
            await entry.Control(RegistryFrames.Stop, new StopFrame { Force = force });

        return targets.Count;
    }

    /// <summary>
    /// Asks one participant to come to the front. False = no such participant here.
    ///
    /// <para>Only a window can act on it. Sending it to an agent is harmless and deliberately not an
    /// error: the hub addresses participants, and making it police which frames suit which role would
    /// put knowledge of every role's capabilities into the index.</para>
    /// </summary>
    public async Task<bool> RequestFocusAsync(string instanceId)
    {
        if (!_entries.TryGetValue(instanceId, out var entry)) return false;
        await entry.Control(RegistryFrames.Focus, new { });
        return true;
    }

    /// <summary>A window already looking at this project, or null when none is. What "Open" consults
    /// before starting anything: raising the window somebody already has beats opening a second one.</summary>
    public RegisteredInstanceDto? FindWindow(string projectId)
        => List().FirstOrDefault(e =>
            e.Role == ParticipantRoles.Window &&
            string.Equals(e.ProjectId, projectId, StringComparison.OrdinalIgnoreCase));

    /// <summary>The live agent for one project id (manifest path), or null when nothing is currently
    /// registered for it — which covers both "never started" and "started, then evicted or stopped".
    /// Callers that need to start it on a miss go through <see cref="IInstanceSpawner"/> themselves;
    /// this only answers what is up right now, the same promise every other read on this class
    /// makes.</summary>
    public RegisteredInstanceDto? FindAgent(string projectId)
        => List().FirstOrDefault(e =>
            e.Role == ParticipantRoles.Agent &&
            string.Equals(e.ProjectId, projectId, StringComparison.OrdinalIgnoreCase));

    /// <summary>One candidate a name search turned up: the project id a caller would need in order to
    /// address it directly, paired with the name that matched.</summary>
    public readonly record struct ProjectMatch(string ProjectId, string? ProjectName);

    /// <summary>
    /// Every project id this hub has ever seen register under <paramref name="name"/> — see
    /// <see cref="_knownNames"/> for what "seen" does and does not mean. Ordinarily this returns at
    /// most one match; more than one means two different manifests happen to share a display name, and
    /// it is deliberately left to the caller to refuse the guess rather than pick one (an HTTP 409
    /// listing both is the point of returning a list here instead of a single nullable result).
    /// </summary>
    public IReadOnlyList<ProjectMatch> FindByName(string name)
        => _knownNames
            .Where(kv => string.Equals(kv.Value, name, StringComparison.OrdinalIgnoreCase))
            .Select(kv => new ProjectMatch(kv.Key, kv.Value))
            .ToList();

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
