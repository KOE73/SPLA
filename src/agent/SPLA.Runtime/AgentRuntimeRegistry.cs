using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Project;
using SPLA.Domain.Settings;

namespace SPLA.Runtime;

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

    /// <summary>The project a connection means when it omits ProjectId on a message — the one the
    /// service was anchored to at startup (a real manifest path, or <see cref="NoProjectId"/> when
    /// none). Single-project clients never learn about project ids at all; this is what keeps them
    /// working unchanged once the service can hold more than one project.</summary>
    public string DefaultProjectId { get; set; } = NoProjectId;

    /// <summary>Fires once, synchronously, right after a runtime is built — the host wires each new
    /// runtime's events/health into the live connection fan-out here (keyed by project id, since
    /// broadcasts must be scoped per project), so a project opened lazily via <see cref="Open"/>/
    /// <see cref="Create"/> gets the same live wiring the startup-time default did.</summary>
    public event Action<string, RuntimeEntry>? RuntimeCreated;

    /// <summary>
    /// Every runtime built so far, so a subscriber that attached late can catch up on what it missed.
    /// <see cref="RuntimeCreated"/> alone is not enough: a caller may legitimately open a project
    /// before handing the registry to whoever wires it — <c>ServeCommand</c> opens the default project
    /// fifteen lines before <c>SplaServiceHost.Build</c> — and that entry's event fired into an empty
    /// handler list. Wiring "future ones, plus the ones already here" is what makes the wiring true
    /// regardless of construction order, instead of making every caller get the order right.
    /// </summary>
    public IReadOnlyCollection<KeyValuePair<string, RuntimeEntry>> Existing => _entries.ToArray();

    public AgentRuntimeRegistry(ILoggerFactory loggerFactory, IProjectProvider? provider = null)
    {
        _loggerFactory = loggerFactory;
        _provider = provider ?? new LocalProjectProvider();
    }

    public IReadOnlyList<ProjectDescriptor> List() => _provider.List();
    public IReadOnlyList<ProjectDescriptor> Recent() => _provider.Recent();

    /// <summary>Opens (building on first touch) the runtime for a known project id, or
    /// <see cref="DefaultProjectId"/> when <paramref name="projectId"/> is null/empty.</summary>
    public RuntimeEntry Open(string? projectId)
    {
        var key = string.IsNullOrEmpty(projectId) ? DefaultProjectId : projectId;
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
            return Build(id, settings);
        });
    }

    /// <summary>Creates a brand-new project via the provider, then opens (and caches) its runtime.</summary>
    public RuntimeEntry Create(ProjectDescriptor descriptor)
    {
        _provider.Create(descriptor);
        var id = descriptor.ManifestPath ?? descriptor.Id;
        return _entries.GetOrAdd(id, _ => Build(id, ConfigLoader.LoadAndResolve(id)));
    }

    /// <summary>
    /// Drops every project runtime that is idle and that nobody is looking at, and returns how many
    /// went. Never touches one that is working, waiting on a person, or stalled.
    ///
    /// <para><b>Why this exists only on a server.</b> Locally one process holds one project, and the
    /// instance lease already decides when the whole process may go — there is nothing to evict
    /// inside it. A server holds N users times M projects in a single process and never exits, so a
    /// runtime built for somebody who logged off in the morning would still be holding its plugins,
    /// its skill fond and its connection cache at midnight. There the same rule stops being a
    /// convenience and becomes the condition for the process surviving the day.</para>
    ///
    /// <para><paramref name="isInUse"/> is asked, not assumed: only the host knows whether a client
    /// is still bound to a project, and a registry that guessed would eventually guess wrong on the
    /// one that mattered.</para>
    /// </summary>
    /// <param name="isInUse">True when some connection is still bound to that project id.</param>
    /// <param name="stallAfter">Silence after which a registered turn counts as stopped halfway —
    /// and therefore as something that must survive, not something idle.</param>
    public int EvictIdle(Func<string, bool> isInUse, TimeSpan stallAfter)
    {
        var evicted = 0;
        foreach (var (id, entry) in _entries.ToArray())
        {
            if (isInUse(id)) continue;
            if (!SPLA.Domain.Project.InstanceStates.MayEvict(entry.Runtime.State(stallAfter))) continue;

            // Removed before disposing: a client arriving in the gap gets a freshly built runtime
            // rather than a half-disposed one. Rebuilding is exactly what makes eviction safe —
            // nothing unique lives in a runtime, only warmth.
            if (!_entries.TryRemove(id, out var removed)) continue;
            removed.Runtime.Dispose();
            evicted++;
        }
        return evicted;
    }

    /// <summary>What the process holding these runtimes is — recorded in each project's instance lock
    /// so a refused second writer is told what has it. Set once by the host at startup.</summary>
    public string InstanceMode { get; set; } = "app";

    /// <summary>
    /// Raised when something decides this whole process should end — today, only a forced or accepted
    /// <c>instance.stop</c>. Lives here rather than on <c>SplaServiceHost</c> because this registry is
    /// the one process-wide object every message handler can already reach (through
    /// <c>IClientSession.Registry</c>); the handler that decides to stop must not be given a reference
    /// to the web host just to raise one event, and the host that ties this to <c>Environment.Exit</c>
    /// (or, in <c>ServeCommand</c>, the same stop signal <c>LeaseExpired</c> already uses) is free to
    /// subscribe without the registry knowing anything about how it is hosted.
    /// </summary>
    public event Action? ShutdownRequested;

    /// <summary>Raises <see cref="ShutdownRequested"/>. A method rather than a public event-raise
    /// pattern quirk — handlers reach this through the registry, never by invoking the event field
    /// directly (C# forbids that outside the declaring type anyway).</summary>
    public void RequestShutdown() => ShutdownRequested?.Invoke();

    /// <summary>
    /// The busiest state across every project this process holds. A process is only free to go away
    /// when none of its projects is doing anything — one busy project speaks for the whole process,
    /// which is why the aggregate is the maximum and not an average.
    /// </summary>
    public SPLA.Domain.Project.InstanceState State(TimeSpan stallAfter)
    {
        var worst = SPLA.Domain.Project.InstanceState.Idle;
        foreach (var entry in _entries.Values)
        {
            var state = entry.Runtime.State(stallAfter);
            if (Rank(state) > Rank(worst)) worst = state;
        }
        return worst;
    }

    /// <summary>Ordering by "how strongly this forbids going away", not by anything the enum implies.</summary>
    private static int Rank(SPLA.Domain.Project.InstanceState state) => state switch
    {
        SPLA.Domain.Project.InstanceState.Idle => 0,
        SPLA.Domain.Project.InstanceState.Working => 1,
        SPLA.Domain.Project.InstanceState.Stalled => 2,
        SPLA.Domain.Project.InstanceState.Waiting => 3,
        _ => 0
    };

    private RuntimeEntry Build(string id, ResolvedSettings settings)
    {
        var runtime = new AgentRuntime(settings, _loggerFactory, instanceMode: InstanceMode);
        var entry = new RuntimeEntry(runtime, new ChatRegistry(runtime));
        RuntimeCreated?.Invoke(id, entry);
        return entry;
    }

    public void Dispose()
    {
        foreach (var entry in _entries.Values) entry.Runtime.Dispose();
    }
}
