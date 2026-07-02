using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Project;
using SPLA.Domain.Settings;

namespace SPLA.Service;

/// <summary>One project's live agent stack: the process-wide runtime plus its chat registry.</summary>
public sealed record RuntimeEntry(AgentRuntime Runtime, ChatRegistry Chats);

/// <summary>
/// Multi-project home for <see cref="AgentRuntime"/>: today's service builds exactly one runtime for
/// the process's single project (see <c>Program.cs</c>/CLI); this registry is the seam that lets a
/// service host many runtimes side by side, each keyed by <see cref="ProjectDescriptor.Id"/> (a
/// manifest path locally). Runtimes are built lazily on first touch and cached for the process
/// lifetime — reopening the same project returns the SAME <see cref="AgentRuntime"/>, so its chats,
/// KV, and connection-health cache stay one consistent state regardless of how many clients look at
/// it (mirrors the existing per-chat sharing in <see cref="ChatRegistry"/>, one level up).
/// <para>
/// Listing/recency delegates to <see cref="IProjectProvider"/> (backend-agnostic); building the
/// actual <see cref="ResolvedSettings"/> still goes through <see cref="ConfigLoader"/> because
/// <see cref="AgentRuntime"/> is inherently local-desktop-shaped today — a server backend swaps this
/// resolution step, not the registry's shape.
/// </para>
/// </summary>
public sealed class AgentRuntimeRegistry : IDisposable
{
    /// <summary>Sentinel key for the no-manifest scenario (cwd/~/.spla fallback) — the historical
    /// single-project-per-process default, kept reachable by its own stable id.</summary>
    public const string NoProjectId = "(no-project)";

    private readonly ILoggerFactory _loggerFactory;
    private readonly IProjectProvider _provider;
    private readonly ConcurrentDictionary<string, RuntimeEntry> _entries = new();

    public AgentRuntimeRegistry(ILoggerFactory loggerFactory, IProjectProvider? provider = null)
    {
        _loggerFactory = loggerFactory;
        _provider = provider ?? new LocalProjectProvider();
    }

    public IReadOnlyList<ProjectDescriptor> List() => _provider.List();
    public IReadOnlyList<ProjectDescriptor> Recent() => _provider.Recent();

    /// <summary>Opens (building on first touch) the runtime for a known project id, or the sentinel
    /// no-project runtime when <paramref name="projectId"/> is null/empty/<see cref="NoProjectId"/>.</summary>
    public RuntimeEntry Open(string? projectId)
    {
        var key = string.IsNullOrEmpty(projectId) ? NoProjectId : projectId;
        return _entries.GetOrAdd(key, id =>
        {
            ResolvedSettings settings;
            if (id == NoProjectId)
            {
                settings = ConfigLoader.LoadAndResolve();
            }
            else
            {
                // Validates existence and bumps recency in the shared registry; the resolved
                // ResolvedSettings themselves come from ConfigLoader (AgentRuntime's actual shape).
                _provider.Open(id);
                settings = ConfigLoader.LoadAndResolve(id);
            }
            return Build(settings);
        });
    }

    /// <summary>Creates a brand-new project via the provider, then opens (and caches) its runtime.</summary>
    public RuntimeEntry Create(ProjectDescriptor descriptor)
    {
        _provider.Create(descriptor);
        var id = descriptor.ManifestPath ?? descriptor.Id;
        return _entries.GetOrAdd(id, _ => Build(ConfigLoader.LoadAndResolve(id)));
    }

    private RuntimeEntry Build(ResolvedSettings settings)
    {
        var runtime = new AgentRuntime(settings, _loggerFactory);
        return new RuntimeEntry(runtime, new ChatRegistry(runtime));
    }

    public void Dispose()
    {
        foreach (var entry in _entries.Values) entry.Runtime.Dispose();
    }
}
