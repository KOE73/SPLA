using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.MCP.Core;
using SPLA.MCP.Core.ToolSets;
using SPLA.Mcp.Client;

namespace SPLA.Runtime;

/// <summary>
/// What one connected (or attempted) MCP server looks like from the outside — the seam step 6's
/// wire payload is built from. Deliberately flat and deliberately not the wire type itself: this is
/// runtime state, and the protocol payload (out of scope here) may want to shape it differently
/// (e.g. localized strings, a subset of fields for a list view vs. a detail view).
/// </summary>
/// <param name="Id">The server's configured id — also the id of its <see cref="ToolSetDescriptor"/>.</param>
/// <param name="Name">Display name (falls back to <paramref name="Id"/> — never persisted as a
/// fallback, same convention as the rest of the settings model).</param>
/// <param name="Transport">stdio or HTTP.</param>
/// <param name="State">Where the session stands right now.</param>
/// <param name="LastError">What the last connect/reconnect attempt failed with, or null.</param>
/// <param name="ToolCount">How many of the server's tools actually made it into <see cref="McpHost"/>
/// — after naming refusals and collisions, not the server's raw count.</param>
/// <param name="ToolSetId">The id of the <see cref="ToolSetDescriptor"/> this server's tools are
/// grouped under (always equal to <paramref name="Id"/> today — carried separately because "the set
/// this server's tools belong to" and "the server's own id" are different questions that happen to
/// have the same answer, and a payload built from this record should not have to know that).</param>
public sealed record McpServerStatus(
    string Id,
    string Name,
    McpTransportKind Transport,
    McpSessionState State,
    string? LastError,
    int ToolCount,
    string ToolSetId);

/// <summary>
/// Owns the set of connected foreign MCP servers for one project: reads
/// <see cref="ResolvedSettings.McpServers"/>, connects each one, and keeps <see cref="McpHost"/> and
/// <see cref="ToolSetRegistry"/> in sync with what each session reports for as long as this runtime
/// lives. See <c>docs/adr/ADR_20260826_service_mcp-client.md</c> and
/// <c>docs/plans/PLAN_20260826_service_mcp-client.md</c> step 5.
///
/// <para><b>A server is a basket.</b> One connected server contributes exactly one
/// <see cref="ToolSetDescriptor"/> (id = server id) holding every tool name it currently offers under
/// its <c>&lt;server_id&gt;_</c> prefix. The descriptor is rebuilt (removed, then re-added) whenever
/// the tool list changes, and removed outright when the connection drops — a disconnected server's
/// tools must not linger, the same "disabled means does not exist" rule the ADR applies to a settings
/// toggle applied here to a connection state.</para>
/// </summary>
public sealed class McpClientManager : IAsyncDisposable
{
    private readonly ResolvedSettings _settings;
    private readonly McpHost _host;
    private readonly ToolSetRegistry _toolSets;
    private readonly ServiceEvents _events;
    private readonly ILogger<McpClientManager> _logger;
    private readonly Func<McpServerSpec, McpServerSession> _sessionFactory;

    private readonly ConcurrentDictionary<string, ServerEntry> _servers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Everything this manager tracks about one server beyond what the session itself knows —
    /// display name, whether the operator vouched for its output, and which tool names it currently
    /// holds in <see cref="McpHost"/> (so the next <c>ToolsChanged</c> knows what to take back).</summary>
    private sealed class ServerEntry
    {
        public required McpServerSession Session { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required bool NamedOrigin { get; init; }
        public required McpTransportKind Transport { get; init; }

        /// <summary>Guards <see cref="RegisteredToolNames"/> against the session's own event thread
        /// firing <c>ToolsChanged</c> and <c>StateChanged</c> concurrently for the same server.</summary>
        public readonly object Gate = new();
        public List<string> RegisteredToolNames = [];
    }

    /// <param name="sessionFactory">How to build the session for a resolved spec. Defaults to a plain
    /// <c>new McpServerSession(spec, logger: ...)</c>, which lets the session pick its own transport
    /// from <c>spec.Transport</c> exactly as production does. The only reason this is a parameter at
    /// all is testability — a test hands back a session wired to <c>McpTestDuplex</c>'s in-memory pipes
    /// instead of a real process or socket; production code never passes this argument.</param>
    public McpClientManager(
        ResolvedSettings settings,
        McpHost host,
        ToolSetRegistry toolSets,
        ServiceEvents events,
        ILogger<McpClientManager> logger,
        Func<McpServerSpec, McpServerSession>? sessionFactory = null)
    {
        _settings = settings;
        _host = host;
        _toolSets = toolSets;
        _events = events;
        _logger = logger;
        _sessionFactory = sessionFactory ?? (spec => new McpServerSession(spec, logger: _logger));
    }

    /// <summary>Read-only status of every server this manager has attempted to connect — including one
    /// that failed and holds no tools, so the UI (step 6) can still show it red rather than absent.</summary>
    public IReadOnlyList<McpServerStatus> Servers =>
        _servers.Values.Select(ToStatus).ToList();

    private static McpServerStatus ToStatus(ServerEntry entry)
    {
        int toolCount;
        lock (entry.Gate) toolCount = entry.RegisteredToolNames.Count;

        return new McpServerStatus(
            entry.Session.Id,
            entry.Name,
            entry.Transport,
            entry.Session.State,
            entry.Session.LastError,
            toolCount,
            entry.Session.Id);
    }

    /// <summary>
    /// Connects every enabled, well-configured server in <see cref="ResolvedSettings.McpServers"/>,
    /// concurrently. A misconfigured entry (missing id, unknown transport, or missing the field its
    /// transport needs) is logged and skipped; a server whose handshake throws is logged and left in
    /// <see cref="McpSessionState.Failed"/>, tracked with no tools. Neither stops any other server in
    /// this call — one bad entry must not take the rest down with it.
    /// </summary>
    public async Task ConnectAllAsync(CancellationToken ct = default)
    {
        var tasks = _settings.McpServers
            .Where(s => s.Enabled != false)
            .Select(s => ConnectOneAsync(s, ct));

        await Task.WhenAll(tasks);
    }

    private async Task ConnectOneAsync(SplaMcpServerSection section, CancellationToken ct)
    {
        var id = section.Id;
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("MCP server entry has no id; skipped.");
            return;
        }

        McpTransportKind transport;
        switch ((section.Transport ?? "stdio").Trim().ToLowerInvariant())
        {
            case "stdio": transport = McpTransportKind.Stdio; break;
            case "http": transport = McpTransportKind.Http; break;
            default:
                _logger.LogWarning(
                    "MCP server has an unknown transport; skipped. Server={ServerId} Transport={Transport}",
                    id, section.Transport);
                return;
        }

        if (transport == McpTransportKind.Stdio && string.IsNullOrWhiteSpace(section.Command))
        {
            _logger.LogWarning("MCP stdio server has no command; skipped. Server={ServerId}", id);
            return;
        }
        if (transport == McpTransportKind.Http && string.IsNullOrWhiteSpace(section.Url))
        {
            _logger.LogWarning("MCP http server has no url; skipped. Server={ServerId}", id);
            return;
        }

        // The `level:` convenience field seeds `toolsets:` only when the project said nothing at all
        // for this server's id — an explicit `toolsets:` entry (set before this ever runs, or set by
        // a later live edit) must still win, and it already does by construction: this only fills a
        // gap, never overwrites.
        if (!string.IsNullOrWhiteSpace(section.Level) && !_settings.ToolSets.ContainsKey(id))
            _settings.ToolSets[id] = section.Level!;

        try
        {
            // Secrets are resolved here, once, at connect time, into the spec handed to the session —
            // never written back onto `section`/`ResolvedSettings`. See agents/secrets.md §1
            // invariant 4 ("materialized at the point of use and dropped") and §4 ("resolve as late
            // as possible, as narrowly as possible").
            var env = await ResolveAsync(section.Env, ct);
            var headers = await ResolveAsync(section.Headers, ct);

            var spec = new McpServerSpec(id, transport)
            {
                Command = section.Command,
                Args = section.Args ?? [],
                WorkingDirectory = section.Cwd,
                Env = env,
                Url = section.Url,
                Headers = headers
            };

            var session = _sessionFactory(spec);

            var entry = new ServerEntry
            {
                Session = session,
                Name = string.IsNullOrWhiteSpace(section.Name) ? id : section.Name!,
                Description = section.Description ?? string.Empty,
                Transport = transport,
                // Unnamed (the default) is the strict choice: the operator named the pipe, not what
                // flows through it. See ADR_20260826_service_mcp-client §2, "named origin" row.
                NamedOrigin = string.Equals(section.Origin, "named", StringComparison.OrdinalIgnoreCase)
            };
            _servers[id] = entry;

            session.ToolsChanged += _ => OnToolsChanged(id, entry);
            session.StateChanged += _ => OnStateChanged(id, entry);

            await session.ConnectAsync(ct);
        }
        catch (Exception ex)
        {
            // The session itself already logged (McpServerSession.ConnectAsync) and left its state as
            // Failed — this catch exists so one server's handshake exception cannot fault
            // Task.WhenAll and take the others down with it. The server stays tracked (Servers still
            // reports it, Failed, with zero tools) rather than vanishing.
            _logger.LogWarning(ex, "Could not connect MCP server. Server={ServerId}", id);
        }
    }

    private async Task<Dictionary<string, string>> ResolveAsync(
        Dictionary<string, string>? source, CancellationToken ct)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (source is null) return result;

        foreach (var (key, reference) in source)
            result[key] = await _settings.SecretResolver.ResolveAsync(reference, ct) ?? string.Empty;

        return result;
    }

    /// <summary>
    /// A session announced a fresh tool list — after connect, or after the server said its list
    /// changed. Drops everything this server previously held in <see cref="McpHost"/> and
    /// <see cref="ToolSetRegistry"/>, then re-registers from scratch: simplest correct answer, since
    /// tool sets are cheap to rebuild and "diff the two lists" buys nothing a full replace does not.
    /// </summary>
    private void OnToolsChanged(string serverId, ServerEntry entry)
    {
        lock (entry.Gate)
        {
            foreach (var name in entry.RegisteredToolNames)
                _host.UnregisterTool(name);

            var registered = new List<string>();

            foreach (var info in entry.Session.Tools)
            {
                var prefixed = McpToolNaming.Prefixed(serverId, info.Name, out var refusal);
                if (prefixed is null)
                {
                    _logger.LogWarning(
                        "MCP tool could not be named and was skipped. Server={ServerId} Tool={ToolName} Reason={Reason}",
                        serverId, info.Name, refusal);
                    continue;
                }

                // The naming half (McpToolNaming.Prefixed) cannot check this itself — it lives in
                // SPLA.Mcp.Client, which knows nothing of McpHost's live registry (ADR step 3 note).
                // This is that check, deferred to the one place that actually holds the registry.
                // GetPermittedToolNames() (not GetToolDefinitions()) is the authoritative surface here
                // — it is what "already registered by something else" actually means, independent of
                // disclosure/activation state that GetToolDefinitions additionally filters by.
                if (_host.GetPermittedToolNames().Contains(prefixed, StringComparer.OrdinalIgnoreCase))
                {
                    // Best-effort context for the log line only — a definition may be absent here
                    // (e.g. disclosure-gated) even though the name is permitted; that does not change
                    // the refusal, only how much the message can say about who holds the name.
                    var existingDescription = _host.GetToolDefinitions()
                        .FirstOrDefault(d => string.Equals(d.Function.Name, prefixed, StringComparison.OrdinalIgnoreCase))
                        ?.Function.Description ?? "(already registered)";
                    _logger.LogWarning(
                        "MCP tool name collision; not registered. Server={ServerId} Tool={ToolName} " +
                        "AlreadyHeldBy={ExistingDescription}",
                        serverId, prefixed, existingDescription);
                    continue;
                }

                _host.RegisterTool(new McpProxyTool(entry.Session, info, serverId, entry.NamedOrigin));
                registered.Add(prefixed);
            }

            entry.RegisteredToolNames = registered;

            _toolSets.RemoveDynamic(serverId);
            if (registered.Count > 0)
                _toolSets.AddDynamic(new ToolSetDescriptor
                {
                    Id = serverId,
                    Origin = ToolSetOrigin.Mcp,
                    OriginId = serverId,
                    Description = entry.Description,
                    ToolNames = registered
                });
        }

        _events.Publish(new McpServersChanged());
    }

    /// <summary>
    /// A session's connection state moved. Anything other than <see cref="McpSessionState.Ready"/>
    /// means this server's tools must not linger — a connection drop is "disabled" happening to the
    /// server rather than to the settings, and the ADR treats both the same way: gone, not merely
    /// hidden.
    /// </summary>
    private void OnStateChanged(string serverId, ServerEntry entry)
    {
        if (entry.Session.State != McpSessionState.Ready)
        {
            lock (entry.Gate)
            {
                foreach (var name in entry.RegisteredToolNames)
                    _host.UnregisterTool(name);
                entry.RegisteredToolNames = [];
                _toolSets.RemoveDynamic(serverId);
            }
        }

        _events.Publish(new McpServersChanged());
    }

    /// <summary>
    /// Tears down every tracked session — genuinely async, because it kills child processes. See
    /// <see cref="AgentRuntime.Dispose"/> for how a synchronous caller waits on this with a bound.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        var sessions = _servers.Values.Select(e => e.Session).ToList();

        foreach (var session in sessions)
        {
            try { await session.DisposeAsync(); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MCP session did not dispose cleanly. Server={ServerId}", session.Id);
            }
        }
    }
}
