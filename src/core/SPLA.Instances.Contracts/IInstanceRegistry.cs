using SPLA.Domain.Project;

namespace SPLA.Instances;

/// <summary>
/// One live instance, as an observer sees it: what the lock file says plus whatever the observer
/// managed to learn by asking. Distinct from <see cref="InstanceInfo"/> on purpose — that is what the
/// instance published about itself and cannot be wrong; everything here beyond <see cref="Info"/> is
/// an observation that may be stale by the time it is read.
/// </summary>
/// <param name="ProjectId">The manifest path, the same id the project registry uses.</param>
/// <param name="ProjectName">Display name, from the manifest.</param>
/// <param name="Info">What the instance published about itself in its lock file.</param>
/// <param name="State">What it said it was doing, or <see cref="InstanceState.Unreachable"/> when
/// nobody answered. Never invented: an instance with no endpoint is not asked at all.</param>
/// <param name="Clients">How many clients it reported, or null when it was not asked.</param>
/// <param name="Role">What this participant is; one of <see cref="ParticipantRoles"/>. Defaults to
/// <see cref="ParticipantRoles.Agent"/> because the file backend can only ever see agents — a lock
/// file is written by whoever holds the project, and a window holds nothing.</param>
public sealed record InstanceRecord(
    string ProjectId,
    string? ProjectName,
    InstanceInfo Info,
    InstanceState State,
    int? Clients,
    string Role = ParticipantRoles.Agent)
{
    /// <summary>True when the instance offers an address to connect to. A REPL or an <c>mcp</c>
    /// session holds a project without serving, and there is nothing to ask it.</summary>
    public bool IsServing => Info.Endpoint is { Length: > 0 };
}

/// <summary>
/// Where the answer to "what is running, and where" comes from.
///
/// <para><b>Two backends, one interface.</b> Locally the filesystem is the registry: known projects
/// come from the project list and liveness comes from each project's own lock file, which the OS
/// releases on any death. Remotely an instance cannot be observed that way at all — there is no
/// shared filesystem and a pid means nothing across machines — so it registers itself with a hub
/// whose address it was given at launch, and liveness becomes the registration channel. The file
/// backend is the degenerate case of the same shape, not a different mechanism.</para>
///
/// <para><b>Discovery only.</b> Nothing here reports what an instance is <i>doing</i> as a live feed:
/// files cannot push, and a badge that updates on a poll is not a badge. State on a record is a
/// snapshot taken when the list was built. Live state travels over the instance's own channel.</para>
/// </summary>
public interface IInstanceRegistry
{
    /// <summary>Every instance this observer can see, newest first. Never throws for one unreachable
    /// instance: it comes back as <see cref="InstanceState.Unreachable"/>, because "I could not
    /// confirm it" and "it is not there" are different answers and only one of them is true.</summary>
    Task<IReadOnlyList<InstanceRecord>> ListAsync(CancellationToken ct = default);

    /// <summary>The instance holding one project, or null when nothing holds it.</summary>
    Task<InstanceRecord?> FindAsync(string projectId, CancellationToken ct = default);
}

/// <summary>Asks an instance what it is doing. Separated from <see cref="IInstanceRegistry"/> so the
/// registry can be built without dragging a protocol client into every consumer — a caller that only
/// wants to know what exists passes nothing and gets lock-file facts.</summary>
public interface IInstanceProbe
{
    /// <summary>Asks the instance at <paramref name="endpoint"/> for its state and client count.
    /// Returns null when it could not be reached or answered something unusable.</summary>
    Task<(InstanceState State, int Clients)?> ProbeAsync(string endpoint, CancellationToken ct = default);
}
