using System;
using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;

namespace SPLA.Domain.Project;

/// <summary>
/// Local project over <see cref="LocalProjectBackend"/>. Brokers the stores the app already had
/// (project KV, secrets, workspace) without owning any data itself. Identity comes from the
/// backend, display name from the resolved manifest.
/// </summary>
public sealed class LocalProject : IProject
{
    private readonly ResolvedSettings _settings;
    private readonly IProjectBackend _backend;
    private readonly Lazy<ProjectKvStore> _kv;
    private readonly IWorkspace _workspace = new LocalWorkspace();

    public LocalProject(ResolvedSettings settings, IProjectBackend backend)
    {
        _settings = settings;
        _backend = backend;
        _kv = new(() => new ProjectKvStore(
            backend.GetBucket(IProjectBackend.RootBucket).MapToHostDirectory()
            ?? throw new InvalidOperationException(
                "Project KV over a virtual backend needs a KV-capable backend (later phase).")));
    }

    public static LocalProject For(ResolvedSettings settings)
        => new(settings, LocalProjectBackend.For(settings));

    public string Id => _backend.ProjectId;
    public string? Name => _settings.ProjectName;

    public IWorkspace GetWorkspace() => _workspace;
    public IKeyValueStore GetKV() => _kv.Value.Store;
    public ISecretStore GetSecrets() => _settings.Secrets;
    public IBucket GetBucket(string name) => _backend.GetBucket(name);
}
