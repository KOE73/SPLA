namespace SPLA.Instances;

/// <summary>What came of asking for an instance to be started.</summary>
/// <param name="Started">True when a new process was launched. False with no <paramref name="Error"/>
/// means one was already there — which is a success for the caller ("the project is up"), just not a
/// new process.</param>
/// <param name="Error">Why it could not be started, or null.</param>
/// <param name="AlreadyRunning">True when the project already had an agent, so nothing was launched.</param>
public sealed record SpawnResult(bool Started, string? Error = null, bool AlreadyRunning = false);

/// <summary>
/// How a hub starts an agent on a project.
///
/// <para><b>Why this is an interface and not a method on the hub.</b> The hub proper is a plain index
/// with no idea that processes exist — that is what lets the desktop shell reference it without
/// acquiring ASP.NET or a process model. Starting things is a capability handed to whoever is hosting
/// the hub, so a deployment that must not spawn (a shared server, say) simply passes nothing and the
/// route answers "not supported" instead of holding a power nobody granted it.</para>
///
/// <para><b>Why the hub may start at all.</b> Stopping is relayed because only the instance knows
/// whether it may go; starting has no such owner to ask, and the alternative — that a project can only
/// be brought up by a person at a desktop — makes the machine unmanageable from a console or a script.
/// See ADR_20260820_apps_project-hub §4: this is a deliberate reversal, not an extension.
/// That is the whole reason this exists: the same management has to be possible with no UI on the box.</para>
/// </summary>
public interface IInstanceSpawner
{
    /// <summary>Starts an agent on <paramref name="projectId"/>, or reports why not. Must be safe to
    /// call for a project that already has one: the honest answer there is "already running", never a
    /// second writer.</summary>
    Task<SpawnResult> StartAsync(string projectId, CancellationToken ct = default);
}
