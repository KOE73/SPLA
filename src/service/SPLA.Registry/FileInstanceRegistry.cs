using SPLA.Domain.Project;

namespace SPLA.Registry;

/// <summary>
/// The zero-configuration registry: the filesystem itself.
///
/// <para>Known projects come from <see cref="IProjectProvider"/>, and liveness comes from each
/// project's own <c>.spla/instance.json</c>, which its holder keeps open with write denied for as
/// long as it lives. That is why there is no directory of instances to prune anywhere: a process
/// that died — cleanly, killed, or crashed — has its handle closed by the OS, and the lock reads
/// back as nothing. No pid check, no heartbeat, nothing to garbage-collect.</para>
///
/// <para>The cost is that it only sees this machine's projects, which is exactly the case it is for:
/// somebody ran <c>spla serve</c> in a folder and wants a window to find it. Anything spread across
/// machines registers itself with a hub instead — the same interface, a different backend.</para>
/// </summary>
public sealed class FileInstanceRegistry : IInstanceRegistry
{
    private readonly IProjectProvider _projects;
    private readonly IInstanceProbe? _probe;

    /// <param name="probe">Asks each serving instance what it is doing. Optional: without it the
    /// listing still names what exists and where, it just cannot say more than the lock does.</param>
    public FileInstanceRegistry(IProjectProvider? projects = null, IInstanceProbe? probe = null)
    {
        _projects = projects ?? new LocalProjectProvider();
        _probe = probe;
    }

    public async Task<IReadOnlyList<InstanceRecord>> ListAsync(CancellationToken ct = default)
    {
        var found = new List<InstanceRecord>();

        foreach (var project in _projects.List())
        {
            ct.ThrowIfCancellationRequested();
            if (Read(project) is not { } record) continue;
            found.Add(await ResolveStateAsync(record, ct));
        }

        return found.OrderByDescending(r => r.Info.StartedAt).ToList();
    }

    public async Task<InstanceRecord?> FindAsync(string projectId, CancellationToken ct = default)
    {
        var project = _projects.List().FirstOrDefault(p =>
            string.Equals(p.Id, projectId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.ManifestPath, projectId, StringComparison.OrdinalIgnoreCase));

        if (project is null || Read(project) is not { } record) return null;
        return await ResolveStateAsync(record, ct);
    }

    /// <summary>Reads one project's lock, with everything the lock alone can tell us filled in.
    /// State starts at <see cref="InstanceState.Unreachable"/> — not because anything is wrong, but
    /// because a file cannot say what a process is doing, and pretending otherwise is how a stale
    /// snapshot ends up looking authoritative.</summary>
    private static InstanceRecord? Read(ProjectDescriptor project)
    {
        if (project.ManifestPath is not { } manifest) return null;
        if (Path.GetDirectoryName(manifest) is not { } dir) return null;

        var info = InstanceLock.Read(Path.Combine(dir, ".spla"));
        if (info is null) return null;

        return new InstanceRecord(
            ProjectId: manifest,
            ProjectName: project.Name ?? Path.GetFileNameWithoutExtension(manifest),
            Info: info,
            State: InstanceState.Unreachable,
            Clients: null);
    }

    /// <summary>Asks the instance what it is doing, when there is somewhere to ask and somebody to
    /// ask with. An instance that holds a project without serving keeps
    /// <see cref="InstanceState.Unreachable"/>: there is genuinely no way to find out, and inventing
    /// <c>idle</c> for it would be a guess dressed as a fact.</summary>
    private async Task<InstanceRecord> ResolveStateAsync(InstanceRecord record, CancellationToken ct)
    {
        if (_probe is null || !record.IsServing) return record;

        var answer = await _probe.ProbeAsync(record.Info.Endpoint!, ct);
        return answer is { } a ? record with { State = a.State, Clients = a.Clients } : record;
    }
}
