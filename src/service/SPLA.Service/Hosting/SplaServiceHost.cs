using SPLA.Runtime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SPLA.Domain.Identity;
using SPLA.Service.Auth;
using SPLA.Service.Observability;

namespace SPLA.Service;

/// <summary>Bind/port/token for the service endpoint.</summary>
public sealed record ServiceOptions
{
    /// <summary>Address to bind. Loopback by default — no auth needed; the OS gates access.</summary>
    public string Bind { get; init; } = "127.0.0.1";

    /// <summary>0 = ephemeral: let the OS pick a free loopback port. Two independent <c>spla serve</c>
    /// invocations (different projects) used to both default to a fixed port and the second would fail
    /// to bind; nothing needs a well-known port any more since the actual bound address is published to
    /// the project's instance lock file (see <c>ProjectInstance.Publish</c>). Pass a nonzero value for
    /// an explicit, fixed address.</summary>
    public int Port { get; init; } = 0;

    /// <summary>Connect secret required for non-loopback clients. Null = no token (loopback use).</summary>
    public string? Token { get; init; }

    /// <summary>How long the instance must have no clients AND nothing running before it shuts itself
    /// down. Zero (the default) disables it: nobody expects a daemon they started by hand to leave on
    /// its own. A service spawned by a window passes a few minutes, so closing the window eventually
    /// reclaims the process without ever killing work in flight. See <c>InstanceLease</c>.</summary>
    public TimeSpan IdleTimeout { get; init; } = TimeSpan.Zero;

    /// <summary>Silence after which a registered turn counts as stopped halfway rather than working.
    /// Both states forbid eviction; they differ in what a person is shown.</summary>
    public TimeSpan StallAfter { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>Optional one-shot startup message. The first WebSocket client that completes the
    /// handshake gets a newly-created chat and sends this message through the normal interactive
    /// turn path, so streaming, permission prompts, and clarification prompts stay in the UI.</summary>
    public string? InitialChatMessage { get; init; }

    /// <summary>When true, the host enforces Negotiate (NTLM/Kerberos) auth: every request must carry a
    /// domain principal, and each connection's <see cref="ClientConnection"/> gets that user's identity.
    /// The server deployment (SPLA.Server) sets this; loopback/embedded leaves it false and stays
    /// single-user, so existing tests and the embedded WebView are unaffected.
    /// <para>Back-compat shim: this is the boolean face of <see cref="Auth"/>. When set with
    /// <see cref="Auth"/> left at its default, it means <see cref="AuthMode.Negotiate"/>.</para></summary>
    public bool RequireAuthentication { get; init; }

    /// <summary>The authentication mode. When left at <see cref="AuthMode.None"/> but
    /// <see cref="RequireAuthentication"/> is true, the effective mode is
    /// <see cref="AuthMode.Negotiate"/> (so existing callers keep working). <see cref="AuthMode.Local"/>
    /// enables username/password auth with an admin panel — the home/workgroup deployment.</summary>
    public AuthMode Auth { get; init; } = AuthMode.None;

    /// <summary>Local-auth only: whether the public <c>/register</c> page is offered.</summary>
    public bool AllowSelfRegistration { get; init; } = true;

    /// <summary>The resolved auth mode, folding the legacy <see cref="RequireAuthentication"/> flag in.</summary>
    public AuthMode EffectiveAuthMode =>
        Auth != AuthMode.None ? Auth : (RequireAuthentication ? AuthMode.Negotiate : AuthMode.None);

    /// <summary>True when any authentication is enforced (cookie pipeline is wired, /ws needs identity).</summary>
    public bool AuthEnabled => EffectiveAuthMode != AuthMode.None;

    public bool IsLoopback => Bind is "127.0.0.1" or "localhost" or "::1";

    /// <summary>
    /// When true, the host binds HTTPS instead of HTTP and generates (or loads) a self-signed
    /// certificate. Required for <c>crypto.randomUUID</c> and other Secure Context browser APIs
    /// to work on non-localhost origins. The cert is stored as a PFX next to the exe and reused
    /// across restarts; clients must add it to their trusted store once.
    /// </summary>
    public bool UseHttps { get; init; }

    /// <summary>Path to an existing PFX certificate file. When null and <see cref="UseHttps"/> is
    /// true, a self-signed cert is generated and saved to <c>spla-cert.pfx</c> next to the exe.</summary>
    public string? CertPath { get; init; }

    /// <summary>Password for the PFX. Empty string if none.</summary>
    public string CertPassword { get; init; } = "spla";

    /// <summary>Whether the batteries-included local stats collector + <c>/stats</c> dashboard run.
    /// Cheap (in-process listeners on the existing meter/traces); on by default.</summary>
    public bool EnableLocalStats { get; init; } = true;

    /// <summary>Optional JSON file the local stats collector persists its per-period buckets to, so the
    /// "over time" view survives a restart. Null = in-memory only (resets on restart).</summary>
    public string? StatsPath { get; init; }

    /// <summary>Optional OTLP endpoint (e.g. <c>http://localhost:4317</c>) to export traces + metrics to
    /// an external observability backend. Null = no export (local stats only). A control-plane / egress
    /// setting — it determines where telemetry leaves the host.</summary>
    public string? OtlpEndpoint { get; init; }
    /// <summary>How long an unused project runtime is kept before it is dropped. Zero (the default)
    /// keeps them forever, which is what a local single-project process wants — there the instance
    /// lease already decides when the whole process may go. A server never exits and holds N users
    /// times M projects, so there this is the condition for surviving the day.</summary>
    public TimeSpan EvictIdleProjectsAfter { get; init; } = TimeSpan.Zero;

    /// <summary>Whether <c>POST /mcp</c> is mapped at all. Off by default — see
    /// <see cref="SplaMcpSection"/>/<see cref="SPLA.Domain.Settings.ResolvedSettings.McpEnabled"/> for
    /// the project-file switch a person actually turns on.</summary>
    public bool McpEnabled { get; init; } = false;

    /// <summary>When set, this host also serves the registry routes for the given hub. Lets one
    /// process be both a service and the hub instances register with — what a small deployment wants,
    /// and what the server does — without a second implementation of the routes for that case.</summary>
    public SPLA.Instances.RegistryHub? RegistryHub { get; init; }

}

/// <summary>
/// The ASP.NET host exposing the agent over a WebSocket at <c>/ws</c>. Backed by an
/// <see cref="AgentRuntimeRegistry"/> rather than a single fixed <see cref="AgentRuntime"/>: each
/// connection resolves the project it needs per message (see <see cref="ClientConnection.Resolve"/>),
/// so the same host can serve one project (today's only real deployment) or several side by side
/// without a shape change. Built so the CLI can run it alongside the console REPL, and so the
/// embedded Avalonia client can later self-host an instance bound to loopback and talk to it on the
/// same footing as a remote one.
/// </summary>
public sealed class SplaServiceHost
{
    /// <summary>Claim that carries the authenticated user's stable key (SID) in the auth cookie.</summary>
    private const string UserKeyClaim = AuthClaims.UserKey;

    private readonly WebApplication _app;
    private readonly SPLA.Observability.Collection.TelemetryCollector? _collector;
    private readonly IDisposable? _gaugeTimer;
    private readonly IDisposable? _evictionTimer;
    private readonly string _scheme;
    private readonly string _bind;
    private string? _url;

    /// <summary>The address the service is actually listening on. Only meaningful after
    /// <see cref="StartAsync"/> has completed: with an ephemeral (<c>Port == 0</c>) binding the real
    /// port is not known until Kestrel has bound the socket, so this throws rather than returning a
    /// guess for a caller that reads it too early.</summary>
    public string Url => _url
        ?? throw new InvalidOperationException($"{nameof(SplaServiceHost)}.{nameof(Url)} is only available after {nameof(StartAsync)}() completes.");

    private readonly InstanceLease _lease;
    private readonly ConnectionHub _hub;

    /// <summary>Raised when the instance has had no clients and nothing running for the configured
    /// grace period. The host does not stop itself: whoever owns the process decides what else has to
    /// wind down first. Never raised when <see cref="ServiceOptions.IdleTimeout"/> is zero.</summary>
    public event Action? LeaseExpired
    {
        add => _lease.Expired += value;
        remove => _lease.Expired -= value;
    }

    /// <summary>Counts a holder that is not a socket — a console REPL running beside the service, a
    /// test. Without it an instance whose only user types at its own terminal looks unattended.</summary>
    public IDisposable HoldLease() => _lease.Hold();

    /// <summary>How many clients are connected right now. Exposed for the registry channel, which
    /// reports it upward: a hub showing "3 windows on this project" is the difference between an
    /// instance somebody is using and one that is merely still alive.</summary>
    public int ClientCount => _hub.Count;

    private SplaServiceHost(
        WebApplication app, string scheme, string bind, InstanceLease lease, ConnectionHub hub,
        SPLA.Observability.Collection.TelemetryCollector? collector = null, IDisposable? gaugeTimer = null,
        IDisposable? evictionTimer = null)
    {
        _app = app;
        _hub = hub;
        _scheme = scheme;
        _bind = bind;
        _lease = lease;
        _collector = collector;
        _gaugeTimer = gaugeTimer;
        _evictionTimer = evictionTimer;
    }

    public static SplaServiceHost Build(
        AgentRuntimeRegistry registry, ServiceOptions options, IIdentityProvider? identityProvider = null,
        SPLA.Domain.Project.ServerProjectRoot? serverRoot = null, LocalAccountService? accounts = null)
    {
        // The host never references a platform: the provider is passed in (loaded from config by the
        // deployment) or defaults to the neutral claims provider. Windows is a DLL, not a dependency.
        var idp = identityProvider ?? new ClaimsIdentityProvider();

        var builder = WebApplication.CreateBuilder();

        var hub = new ConnectionHub();
        var auth = new AuthGate(options.Token);
        var initialChat = InitialChatRequest.Create(options.InitialChatMessage);

        ConfigureAuthentication(builder, options);
        OtlpExport.MaybeWire(builder, options);
        WireRuntimeEvents(registry, hub);

        // Build (and wire, via the event above) the connection's default project eagerly, the same
        // moment today's single-runtime host used to construct its one AgentRuntime.
        var defaultEntry = registry.Open(registry.DefaultProjectId);
        var loggerFactory = defaultEntry.Runtime.LoggerFactory;

        // The agent already logs through SplaTelemetry; keep ASP.NET quiet on the console.
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new ForwardingLoggerProvider(loggerFactory));

        string scheme;
        if (options.UseHttps)
        {
            var cert = LoadOrCreateCertificate(options);
            var bindIp = options.Bind == "0.0.0.0" ? IPAddress.Any : IPAddress.Parse(options.Bind);
            builder.WebHost.ConfigureKestrel(k =>
            {
                k.Listen(bindIp, options.Port, lo =>
                {
                    lo.UseHttps(httpsOpts => { httpsOpts.ServerCertificate = cert; });
                });
            });
            scheme = "https";
        }
        else
        {
            scheme = "http";
        }
        var app = builder.Build();
        // Port 0 here (the default) means "ephemeral" — ASP.NET/Kestrel understands a :0 port in the
        // URL the same way it understands one passed to ConfigureKestrel above, and binds a free port.
        // The actual bound port isn't known until StartAsync() runs; see ResolveUrl().
        if (!options.UseHttps) app.Urls.Add($"{scheme}://{options.Bind}:{options.Port}");
        app.UseWebSockets();

        if (options.AuthEnabled)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        // Carries the instance id of the default project's claim, so a caller that found this address
        // in a lock file can prove the address still belongs to the instance that wrote it. A port
        // number alone proves nothing: the OS hands it to whoever asks next.
        // Also a hub, when the deployment asked for it. The same routes `spla hub` runs alone —
        // mapped, not reimplemented, because "with a server" and "without one" are two scenarios and
        // only one implementation may exist for both.
        if (options.RegistryHub is { } registryHub) app.MapRegistry(registryHub, options.Token);

        app.MapGet("/health", () => Results.Text(
                $"SPLA service running. Connect a client to /ws.\ninstance: {defaultEntry.Runtime.Instance?.Info.InstanceId ?? "none"}",
                "text/plain"))
            .AllowAnonymous();

        // MCP over HTTP: a foreign head that wants tools without taking the writer lease `spla mcp`
        // takes (that command builds its own AgentRuntime — see McpCommand's remarks). This endpoint
        // dispatches against the runtime this host already has open, so any number of MCP clients can
        // share it the same way any number of browser windows already share /ws. One request, one
        // JSON-RPC line in, one line out — McpStdioServer needs nothing more than a reader that hits
        // EOF after that line, which a request body naturally does.
        if (options.McpEnabled)
            app.MapPost("/mcp", (Func<HttpContext, Task<IResult>>)(ctx => HandleMcpAsync(ctx, registry)));

        if (options.EffectiveAuthMode == AuthMode.Negotiate)
        {
            // The one place Negotiate runs: reaching here means the browser authenticated (NTLM/
            // Kerberos). Sign that principal into the cookie, then bounce back to the app — every
            // request after this (including the /ws upgrade) authenticates by cookie, not Negotiate.
            app.MapGet("/login", async (HttpContext ctx) =>
            {
                // Put the user KEY + display name straight into the cookie — small (two claims, no
                // group SIDs), so no size/chunking problem and, crucially, no server-side session
                // store to go stale across restarts. Groups aren't carried here; they're only needed
                // for sharing (future) and will be re-resolved from the key then.
                var id = idp.FromPrincipal(ctx.User);
                var cookiePrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(UserKeyClaim, id.UserKey),
                        new Claim(ClaimTypes.Name, id.DisplayName)
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme));
                await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cookiePrincipal);

                var ret = ctx.Request.Query["returnUrl"].FirstOrDefault();
                return Results.Redirect(string.IsNullOrEmpty(ret) ? "/" : ret);
            }).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme });
        }
        else if (options.EffectiveAuthMode == AuthMode.Local && accounts != null)
        {
            // Local username/password auth: the login/register/account pages and the admin panel.
            // All of them issue the same cookie the Negotiate path does, so /ws and per-user areas
            // are identical downstream.
            AuthEndpoints.Map(app, accounts);
            AdminEndpoints.Map(app, accounts);
        }

        // The browser client — served from embedded static assets. Any client drives the same agent /ws.
        static IResult ServeAsset(string path)
        {
            var asset = WebAssets.Get(path);
            return asset is { } a ? Results.Bytes(a.Bytes, a.ContentType) : Results.NotFound();
        }

        // Persisted chat image attachments (sidecar files). Served read-only with a path-traversal
        // guard. An optional ?project= resolves a non-default project's images; omitted defaults to
        // this connection's default project (today's only real case).
        app.MapGet("/chat-image/{chatId}/{file}", (string chatId, string file, string? project) =>
        {
            var runtime = registry.Open(project).Runtime;
            var path = ChatImages.Resolve(runtime.Settings.Project, chatId, file);
            return path != null
                ? Results.File(path, ChatImages.ContentType(file))
                : Results.NotFound();
        });

        // Plugin-contributed web settings UI — served straight from each plugin's own directory
        // (see PluginMeta.WebSettingsEntry). The host never builds or knows the content of these
        // files; it only resolves pluginId → directory and streams whatever is there. Plugins are
        // process-wide (loaded once, not per-project), so this always resolves against the default
        // project's PluginManager regardless of ?project=.
        app.MapGet("/plugin-assets/{pluginId}/{**path}", (string pluginId, string path) =>
        {
            var dir = defaultEntry.Runtime.PluginManager.GetPluginDirectory(pluginId);
            if (dir == null) return Results.NotFound();
            var asset = WebAssets.GetFromDirectory(dir, path);
            return asset is { } a ? Results.Bytes(a.Bytes, a.ContentType) : Results.NotFound();
        });

        // Observability plane: the batteries-included local stats collector taps the existing meter and
        // traces in-process (nothing added to the hot path) and serves the /stats dashboard. Admin-gated
        // under local auth; open on a loopback/no-auth box. The connection gauge is refreshed on a timer.
        SPLA.Observability.Collection.TelemetryCollector? collector = null;
        System.Threading.Timer? gaugeTimer = null;
        if (options.EnableLocalStats)
        {
            collector = new SPLA.Observability.Collection.TelemetryCollector(persistPath: options.StatsPath);
            collector.SetGauge("connections.active", hub.Count);

            var statsHub = new StatsHub();
            // Each completed activity is pushed live to the firehose (admins + the acting user).
            var liveCollector = collector;
            collector.EventRecorded += evt => _ = statsHub.PushEventAsync(evt);
            // One timer refreshes the connection gauge AND pushes a fresh scoped snapshot to every
            // viewer — so the KPIs/gauges update by server push, with no client-side polling.
            gaugeTimer = new System.Threading.Timer(_ =>
            {
                liveCollector.SetGauge("connections.active", hub.Count);
                _ = statsHub.PushSnapshotsAsync(liveCollector);
            }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3));

            StatsEndpoints.Map(app, statsHub, authEnabled: options.AuthEnabled);
        }

        app.MapGet("/", () => ServeAsset("/index.html"));
        app.MapGet("/{**path}", (string path) => ServeAsset("/" + path));

        app.Map("/ws", (HttpContext context) =>
            HandleWebSocketAsync(context, registry, options, serverRoot, hub, auth, initialChat, loggerFactory));

        var lease = new InstanceLease(
            hub, registry, options.IdleTimeout, options.StallAfter,
            loggerFactory.CreateLogger<InstanceLease>());

        // Unused project runtimes go on a slow sweep. Idle only, and only when nobody is bound to
        // them — a runtime holds nothing unique (chats, KV and the tally are on disk), so dropping
        // one costs the next warm-up and never any work. Off unless a deployment asks.
        System.Threading.Timer? evictionTimer = null;
        if (options.EvictIdleProjectsAfter > TimeSpan.Zero)
        {
            var sweep = options.EvictIdleProjectsAfter;
            evictionTimer = new System.Threading.Timer(_ =>
            {
                try { registry.EvictIdle(id => hub.CountForProject(id) > 0, StallAfter); }
                catch { /* a sweep that throws must not take the host down */ }
            }, null, sweep, sweep);
        }

        return new SplaServiceHost(app, scheme, options.Bind, lease, hub, collector, gaugeTimer, evictionTimer);
    }

    /// <summary>Configures the auth pipeline for the effective mode. Both server modes issue the same
    /// <c>spla.auth</c> cookie so everything downstream (the /ws upgrade, per-user areas) is identical:
    /// <list type="bullet">
    /// <item><b>Negotiate</b> — NTLM/Kerberos authenticates the page once via <c>/login</c>, which
    /// issues the cookie every later request rides.</item>
    /// <item><b>Local</b> — cookie only (no Negotiate); credentials are validated by
    /// <see cref="LocalAccountService"/> and an <c>spla.admin</c> policy gates the admin panel.</item>
    /// </list></summary>
    private static void ConfigureAuthentication(WebApplicationBuilder builder, ServiceOptions options)
    {
        var mode = options.EffectiveAuthMode;
        if (mode == AuthMode.None) return;

        var authentication = builder.Services.AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(o =>
            {
                o.Cookie.Name = "spla.auth";
                o.Cookie.HttpOnly = true;
                o.ExpireTimeSpan = TimeSpan.FromHours(10);
                o.SlidingExpiration = true;
                o.LoginPath = "/login";
                o.AccessDeniedPath = "/Account/AccessDenied";

                // API paths (the admin panel's fetch calls) must see real status codes, not a 302 to
                // an HTML page: unauthenticated → 401, authenticated-but-forbidden → 403. Browser
                // navigations still get the usual redirect to /login or the access-denied page.
                o.Events.OnRedirectToLogin = ctx =>
                {
                    if (IsApiPath(ctx.Request)) { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; }
                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
                o.Events.OnRedirectToAccessDenied = ctx =>
                {
                    if (IsApiPath(ctx.Request)) { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; }
                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
            });

        if (mode == AuthMode.Negotiate)
            authentication.AddNegotiate();

        var authorization = builder.Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        // The admin panel is role-gated (local mode only — Negotiate has no local roles).
        if (mode == AuthMode.Local)
            authorization.AddPolicy("spla.admin", p => p.RequireAuthenticatedUser().RequireRole("admin"));
    }

    /// <summary>True for endpoints a programmatic client (the admin panel's fetch) calls, where an auth
    /// failure should be a status code rather than a redirect to an HTML page.</summary>
    private static bool IsApiPath(HttpRequest request)
        => request.Path.StartsWithSegments("/admin/api") || request.Path.StartsWithSegments("/stats/api");

    /// <summary>Wires each newly-built runtime's domain events and initial connection-health warm-up
    /// into the hub, scoped to its own project id — fires for the eagerly-opened default project and
    /// for any project a client opens/creates later, so live updates are never limited to whichever
    /// project happened to exist at process startup.</summary>
    /// <summary>Silence after which a registered turn counts as stopped halfway. The same judgement
    /// the instance handlers and the chat projections make, and deliberately not a knob: it is a
    /// statement about how long a model may go quiet before a person would call it stuck.</summary>
    private static readonly TimeSpan StallAfter = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Forwards every node change in a chat's whole progress forest — the turn's own tree and any
    /// background task's, present or future — to every watcher of that chat. One subscription for the
    /// chat's entire life (wired once, from <see cref="ChatRegistry.RuntimeOpened"/>), which is what
    /// makes a background task's live ticks visible at all: they have no turn of their own for a
    /// per-turn subscription to attach to.
    /// <para>
    /// Throttling state (last-sent-at per node, 120ms) lives here rather than per-turn for the same
    /// reason — a node born under one tree and a node born under another must not share a clock, but
    /// both must survive across however many turns and tasks this chat runs.
    /// </para>
    /// </summary>
    private static void WireChatProgress(
        SPLA.Runtime.ChatRuntime chat, AgentRuntime runtime, ConnectionHub hub)
    {
        var lastSent = new System.Collections.Concurrent.ConcurrentDictionary<string, DateTime>();

        chat.Progress.NodeChanged += (treeId, node) =>
        {
            runtime.Turns.Touch(chat.ChatId);

            // Each ProgressTree numbers its own nodes from "n1" — fine while exactly one tree was
            // ever live per chat (a turn's own). Now a background task's tree can be live alongside
            // the current turn's, and their local ids collide on the wire with no tree of their own
            // to disambiguate them in the flat `progress.node` stream. Namespacing by the hub's tree
            // id ("t2:n1") is the fix, done here rather than in ProgressNodePayload/ProgressTree
            // themselves — those stay tree-local (correct for every other consumer, MCP included),
            // and only the point that merges several trees into one stream needs to know they exist.
            var wireId = $"{treeId}:{node.Id}";
            var wireParentId = node.ParentId is null ? null : $"{treeId}:{node.ParentId}";

            var now = DateTime.UtcNow;
            var known = lastSent.TryGetValue(wireId, out var last);

            // Structural frames — a node's first appearance and its finish — are never throttled:
            // they are what a client builds the tree's shape out of, and one dropped frame is a
            // branch that never appears or one that spins forever. Only the ticks between are
            // throttled, and per node, so one host's scan cannot silence what started under it.
            if (known && node.State == SPLA.Domain.Models.ProgressState.Running
                      && (now - last).TotalMilliseconds < 120) return;

            lastSent[wireId] = now;

            var latest = node.Latest;
            _ = hub.BroadcastToWatchersAsync(chat.ChatId, Contracts.MessageTypes.ProgressNode, new Contracts.ProgressNodePayload
            {
                NodeId = wireId,
                ParentId = wireParentId,
                Label = node.Label,
                State = node.State.ToString().ToLowerInvariant(),
                Current = latest?.Current,
                Total = latest?.Total,
                Fraction = latest?.Fraction,
                Message = latest?.Message,
                Details = latest?.Details?
                    .Select(d => new Contracts.ToolProgressDetailDto { Label = d.Label, Value = d.Value })
                    .ToList()
            });
        };
    }

    private static void WireRuntimeEvents(AgentRuntimeRegistry registry, ConnectionHub hub)
    {
        registry.RuntimeCreated += (projectId, entry) =>
        {
            entry.Runtime.Events.Subscribe(evt =>
            {
                switch (evt)
                {
                    case AppearanceChanged a:
                        _ = hub.BroadcastToProjectAsync(projectId, Contracts.MessageTypes.AppearanceChanged,
                            new Contracts.AppearanceChangedPayload { Theme = a.Theme, Density = a.Density });
                        break;

                    // The whole list, not a delta: it is small, and a panel that reconciles deltas
                    // is a panel that can drift from what the fond actually holds.
                    case SkillsChanged:
                        _ = hub.BroadcastToProjectAsync(projectId, Contracts.MessageTypes.SkillsResult,
                            SettingsOps.GetSkills(entry.Runtime));
                        break;
                }
            });

            // A question a running turn is waiting on. Raised by the project's runtime rather than by
            // the connection that started the turn, so it reaches every window watching the chat and
            // any of them may answer — including one that opened after the question was asked.
            entry.Runtime.Asks.Asked += ask =>
            {
                _ = hub.BroadcastToWatchersAsync(
                    ask.ChatId, ProtocolMapper.MessageTypeFor(ask), ProtocolMapper.PayloadFor(ask), ask.RequestId);
                // The sidebar too, not just the open chat: "somebody is being waited for" is the one
                // state a person needs to see from a chat they are not currently looking at.
                _ = BroadcastChatsAsync(hub, projectId, entry);
            };

            // ...and its counterpart: whoever closed it, every other window drops the dialog instead
            // of leaving a button that answers nothing.
            entry.Runtime.Asks.Resolved += (ask, reason) =>
            {
                _ = hub.BroadcastToWatchersAsync(
                    ask.ChatId, Contracts.MessageTypes.AskResolved,
                    new Contracts.AskResolvedPayload { Reason = ProtocolMapper.ReasonName(reason) }, ask.RequestId);
                _ = BroadcastChatsAsync(hub, projectId, entry);
            };

            // Chat-level progress: one subscription for the chat's whole life, not one per turn and
            // not one per connection. Replaces the old per-turn subscription in
            // ClientConnection.BuildCallbacks, which only ever saw the turn's own tree — a background
            // task's ticks (plan step 0.4, closed properly here rather than deferred again) had no
            // subscription to ride at all, only its final result via the inbox. ChatRuntime.Progress
            // already collects every root, turn and background task alike (built in wave 0's
            // ProgressHub), so wiring it once here reaches both automatically.
            entry.Chats.RuntimeOpened += chat => WireChatProgress(chat, entry.Runtime, hub);


            // Live SSH sessions: create the project's hub eagerly and fan its open/close events out
            // as ssh.sessions.changed, so pickers refresh and terminals auto-attach the moment the
            // AGENT opens a session — the human sees it happen instead of discovering it later.
            var sshHub = SPLA.Plugins.Ssh.SshSessionHub.For(entry.Runtime.Settings);
            sshHub.Changed += () =>
                _ = hub.BroadcastToProjectAsync(projectId, Contracts.MessageTypes.SshSessionsChanged, new { });

            // Warm the health cache in the background right after the runtime starts so the first
            // client to open settings sees real results (or the cached "not yet checked" state) instantly.
            _ = Task.Run(async () =>
            {
                try
                {
                    var health = await ConnectionDiagOps.PingAllAsync(
                        entry.Runtime.Settings.Connections, entry.Runtime.ConnectionHealth,
                        entry.Runtime.Settings.SecretResolver);
                    await hub.BroadcastToProjectAsync(projectId, Contracts.MessageTypes.ConnectionsHealth, health);
                }
                catch { }
            });
        };
    }

    /// <summary>The chat list, to everyone watching this project. Sent whenever a chat's state
    /// changes rather than only at turn boundaries — the state is what the sidebar badges are made
    /// of, and a badge that appears a turn late is not a badge.</summary>
    private static Task BroadcastChatsAsync(ConnectionHub hub, string projectId, RuntimeEntry entry)
        => hub.BroadcastToProjectAsync(projectId, Contracts.MessageTypes.ChatListResult,
            new Contracts.ChatListResultPayload { Chats = entry.Chats.List() });

    /// <summary>Handles a <c>/ws</c> upgrade: the Origin gate (CSWSH defence in cookie deployments),
    /// resolving the connection's identity from the auth cookie (or the local sentinel), scoping it to
    /// the user's own server area, then running the <see cref="ClientConnection"/> for the socket's life.</summary>
    private static async Task HandleWebSocketAsync(
        HttpContext context, AgentRuntimeRegistry registry, ServiceOptions options,
        SPLA.Domain.Project.ServerProjectRoot? serverRoot, ConnectionHub hub, AuthGate auth,
        InitialChatRequest? initialChat, ILoggerFactory loggerFactory)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // Origin gate (cookie/Negotiate deployments only). The /ws upgrade authenticates by the
        // ambient auth cookie, so a page on any other site the user has open could open a socket
        // here and drive the agent with the victim's cookie (cross-site WebSocket hijacking). A
        // browser always sends Origin on a WS handshake; require it to match this server's own
        // host. Non-browser clients (CLI/embedded) send no Origin and are unaffected; the check
        // is skipped entirely when auth is off (loopback/embedded).
        if (options.AuthEnabled)
        {
            var origin = context.Request.Headers.Origin.ToString();
            if (!string.IsNullOrEmpty(origin) && !IsSameHostOrigin(origin, context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        // The connection's user comes straight from the auth cookie (set at /login) — the user key,
        // display name, and any group claims, no server-side lookup, so it survives restarts. Reaching
        // here already means the cookie authenticated (fallback policy on /ws); the claims are read back.
        IIdentity identity;
        if (options.AuthEnabled)
        {
            var userKey = context.User.FindFirst(UserKeyClaim)?.Value;
            if (string.IsNullOrEmpty(userKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            var groups = context.User.FindAll(AuthClaims.Group).Select(c => c.Value).ToArray();
            identity = new ClaimIdentity(userKey, context.User.Identity?.Name ?? userKey, groups);
        }
        else
        {
            identity = LocalIdentity.Single;
        }

        // Server mode: scope the connection to the user's own area — their project list and the
        // default project they land in come from {serverRoot}/users/{sid}/, auto-provisioned on
        // first connect. Local/embedded (serverRoot == null) keeps the shared registry scope.
        var userProvider = serverRoot?.ProviderFor(identity);
        var userDefault = serverRoot?.EnsureDefaultProject(identity);
        var userArea = serverRoot?.EnsureUserArea(identity.UserKey);

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var log = loggerFactory.CreateLogger<ClientConnection>();
        var conn = new ClientConnection(
            socket, registry, hub, auth, log, identity, userProvider, userDefault, userArea, initialChat);
        await conn.RunAsync(context.RequestAborted);
    }

    /// <summary>One JSON-RPC request over HTTP, in one of two shapes depending on what the client
    /// asked for:
    /// <list type="bullet">
    /// <item><b>Plain</b> (no <c>Accept: text/event-stream</c>) — one request, one response, same as
    /// before: a call that opts into progress (<c>_meta.progressToken</c>) still runs to completion,
    /// its progress frames simply have nowhere to go and are dropped.</item>
    /// <item><b>SSE</b> (<c>Accept: text/event-stream</c>, MCP's "streamable HTTP" transport) — the
    /// connection stays open and every frame <c>McpStdioServer</c> writes, progress notifications
    /// included, is pushed as its own <c>data:</c> event the moment it is produced. This is the network
    /// equivalent of stdio: the same live ticks a stdio client gets down the pipe while a slow call
    /// (<c>ssh_run</c>, a long <c>agent_spawn</c>) is still running.</item>
    /// </list>
    /// <para><c>?project=</c> picks the runtime the same way <c>/chat-image</c> does; omitted defaults
    /// to this host's default project.</para>
    /// <para><b>Why not a plain <see cref="StringReader"/>.</b> The first cut fed the request line
    /// through one and let it hit EOF on the very next read — which is exactly the signal
    /// <c>McpStdioServer.RunAsync</c> treats as "the pipe closed", so it cancelled every call still
    /// in flight before returning. That is invisible for an instant call like <c>tools/list</c> but
    /// silently killed anything doing real I/O — <c>ssh_run</c> connecting to a host, mid-TCP-handshake,
    /// came back as an empty response because the cancellation raced its own completion and won.
    /// <see cref="PendingLineReader"/> holds EOF back until the call is actually done (signalled by
    /// either writer below), so the call's own cancellation token is never touched before it is done
    /// with it.</para></summary>
    private static async Task<IResult> HandleMcpAsync(HttpContext ctx, AgentRuntimeRegistry registry)
    {
        string line;
        using (var bodyReader = new StreamReader(ctx.Request.Body))
        {
            line = await bodyReader.ReadToEndAsync(ctx.RequestAborted);
        }
        if (string.IsNullOrWhiteSpace(line)) return Results.BadRequest();

        System.Text.Json.Nodes.JsonNode? request;
        try { request = System.Text.Json.Nodes.JsonNode.Parse(line); }
        catch (System.Text.Json.JsonException) { return Results.BadRequest(); }

        // A notification (e.g. notifications/initialized) has no id and gets no reply per JSON-RPC —
        // waiting on a response line for one would hang until the client gives up.
        var requestId = request?["id"];
        var expectsReply = requestId is not null;

        var project = ctx.Request.Query["project"].FirstOrDefault();
        var runtime = registry.Open(project).Runtime;
        var exposure = SPLA.MCP.Core.ToolExposure.Default;

        var reader = new PendingLineReader(line);
        var server = new SPLA.Mcp.McpStdioServer(
            runtime.McpHost,
            () => runtime.McpHost.GetToolDefinitionsFor(exposure),
            log: TextWriter.Null,
            source: $"mcp-http {ctx.Connection.RemoteIpAddress}");

        var wantsSse = expectsReply &&
            ctx.Request.Headers.Accept.Any(a => a?.Contains("text/event-stream", StringComparison.OrdinalIgnoreCase) == true);

        if (wantsSse)
        {
            // Headers first, then every frame as its own event as soon as it exists — nothing here
            // buffers, which is the entire point over the plain-JSON path.
            ctx.Response.ContentType = "text/event-stream";
            ctx.Response.Headers.CacheControl = "no-cache";

            var sse = new SseWriter(ctx.Response.Body, requestId);
            var runningSse = server.RunAsync(reader, sse, ctx.RequestAborted);

            var finishedSse = await Task.WhenAny(sse.FinalResponseWritten, runningSse);
            if (finishedSse != runningSse) await sse.FinalResponseWritten;

            reader.SignalEof();
            await runningSse;
            return Results.Empty; // the response was already streamed directly to ctx.Response.Body
        }

        var writer = new CapturingWriter(requestId);
        var running = server.RunAsync(reader, writer, ctx.RequestAborted);

        string? responseLine = null;
        if (expectsReply)
        {
            var finished = await Task.WhenAny(writer.ResponseWritten, running);
            if (finished == running)
            {
                // The server's loop ended (client aborted, or an unexpected fault) before it ever
                // wrote a reply — there is nothing to wait for any more.
                await running; // surfaces the fault, if any, instead of swallowing it
            }
            else
            {
                responseLine = await writer.ResponseWritten;
            }
        }

        reader.SignalEof();
        await running;

        return responseLine is null ? Results.NoContent() : Results.Text(responseLine, "application/json");
    }

    /// <summary>True when <paramref name="jsonLine"/> is the reply to the request that carried
    /// <paramref name="requestId"/> — as opposed to a <c>notifications/progress</c> frame, which has no
    /// <c>id</c> at all. Shared by both writers below so "which line is the actual answer" is decided
    /// once, the same way, regardless of transport.</summary>
    private static bool IsFinalResponse(string? jsonLine, System.Text.Json.Nodes.JsonNode? requestId)
    {
        if (string.IsNullOrEmpty(jsonLine)) return false;
        try
        {
            var id = System.Text.Json.Nodes.JsonNode.Parse(jsonLine)?["id"];
            return requestId is null
                ? id is null
                : System.Text.Json.Nodes.JsonNode.DeepEquals(id, requestId);
        }
        catch (System.Text.Json.JsonException) { return false; }
    }

    /// <summary>Yields one line, then blocks — never hands <c>McpStdioServer.RunAsync</c> an EOF until
    /// <see cref="SignalEof"/> is called. See <see cref="HandleMcpAsync"/> for why that matters.</summary>
    private sealed class PendingLineReader : TextReader
    {
        private readonly string _line;
        private bool _sent;
        private readonly TaskCompletionSource _eof = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public PendingLineReader(string line) => _line = line;

        public void SignalEof() => _eof.TrySetResult();

        public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            if (!_sent) { _sent = true; return _line; }
            await _eof.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }
    }

    /// <summary>Captures the reply matching the request's own id and nothing else — a progress
    /// notification arriving on this path (a client that sent <c>_meta.progressToken</c> without
    /// asking for SSE) is silently dropped rather than mistaken for the answer.
    /// <see cref="ResponseWritten"/> completes the moment the real reply is written, which is the
    /// signal <see cref="HandleMcpAsync"/> waits on before it lets the reader see EOF.</summary>
    private sealed class CapturingWriter : TextWriter
    {
        private readonly System.Text.Json.Nodes.JsonNode? _requestId;
        private readonly TaskCompletionSource<string?> _written =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CapturingWriter(System.Text.Json.Nodes.JsonNode? requestId) => _requestId = requestId;

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public Task<string?> ResponseWritten => _written.Task;

        public override Task WriteLineAsync(string? value)
        {
            if (IsFinalResponse(value, _requestId)) _written.TrySetResult(value);
            return Task.CompletedTask;
        }

        public override Task FlushAsync() => Task.CompletedTask;
    }

    /// <summary>Streams every frame straight to the response body as an SSE <c>data:</c> event —
    /// progress notifications and the final reply alike, in the order <c>McpStdioServer</c> produces
    /// them. This is the piece that makes <c>/mcp</c> a real network analogue of stdio: a client that
    /// asked for progress actually sees it arrive while the call is still running, not bundled into one
    /// response at the end.</summary>
    private sealed class SseWriter : TextWriter
    {
        private readonly Stream _body;
        private readonly System.Text.Json.Nodes.JsonNode? _requestId;
        private readonly TaskCompletionSource _finalResponseWritten =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly SemaphoreSlim _writeGate = new(1, 1);

        public SseWriter(Stream body, System.Text.Json.Nodes.JsonNode? requestId)
        {
            _body = body;
            _requestId = requestId;
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        /// <summary>Completes once the frame answering the request (not a progress notification) has
        /// been written and flushed.</summary>
        public Task FinalResponseWritten => _finalResponseWritten.Task;

        public override async Task WriteLineAsync(string? value)
        {
            if (string.IsNullOrEmpty(value)) return;

            var bytes = System.Text.Encoding.UTF8.GetBytes($"data: {value}\n\n");
            await _writeGate.WaitAsync().ConfigureAwait(false);
            try
            {
                await _body.WriteAsync(bytes).ConfigureAwait(false);
                await _body.FlushAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // The client disconnected mid-stream — normal end of an SSE session, not a fault for
                // the tool call that was still writing to it. HandleMcpAsync's own CancellationToken
                // (ctx.RequestAborted) is what actually stops the call; this just stops pretending the
                // write succeeded.
            }
            finally { _writeGate.Release(); }

            if (IsFinalResponse(value, _requestId)) _finalResponseWritten.TrySetResult();
        }

        public override Task FlushAsync() => Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        await _app.StartAsync(ct);
        _url = ResolveUrl();
    }

    /// <summary>Reads back whatever Kestrel actually bound (via <see cref="IServerAddressesFeature"/>),
    /// which is the only place the real port lives when <see cref="ServiceOptions.Port"/> was 0. The
    /// feature's address uses Kestrel's own formatting (e.g. "*" for a wildcard host), so only the port
    /// is taken from it; scheme and host are the ones this host was actually configured with.</summary>
    private string ResolveUrl()
    {
        var feature = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
        var bound = feature?.Addresses.FirstOrDefault();
        if (bound != null && Uri.TryCreate(bound, UriKind.Absolute, out var uri))
            return $"{_scheme}://{_bind}:{uri.Port}";

        // Should not happen — Kestrel always populates this feature once started — but fail soft
        // rather than throw out of StartAsync for a cosmetic URL string.
        return $"{_scheme}://{_bind}";
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        _lease.Dispose();
        _gaugeTimer?.Dispose();
        _evictionTimer?.Dispose();
        _collector?.Dispose();   // flushes persisted stats
        await _app.StopAsync(ct);
    }

    /// <summary>
    /// Loads the PFX from <see cref="ServiceOptions.CertPath"/> when specified, or generates a
    /// self-signed RSA-2048 certificate valid for 10 years and saves it as <c>spla-cert.pfx</c>
    /// next to the exe. Subsequent starts reuse the same file so clients only need to trust it once.
    /// </summary>
    private static X509Certificate2 LoadOrCreateCertificate(ServiceOptions options)
    {
        var path = options.CertPath
            ?? Path.Combine(AppContext.BaseDirectory, "spla-cert.pfx");
        var password = options.CertPassword;

        if (File.Exists(path))
        {
            Console.WriteLine($"[HTTPS] Loading certificate from {path}");
            return X509CertificateLoader.LoadPkcs12FromFile(path, password);
        }

        Console.WriteLine($"[HTTPS] Generating self-signed certificate → {path}");
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=SPLA Server,O=SPLA,C=RU",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        req.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
        req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
            new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false)); // TLS server

        // SAN: DNS name + all common LAN patterns so the cert works with hostname or IP
        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName("localhost");
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
        // Add the machine hostname so \\PC-UOIT140\... style access works too
        sanBuilder.AddDnsName(Environment.MachineName);
        sanBuilder.AddDnsName(Environment.MachineName.ToLowerInvariant());
        req.CertificateExtensions.Add(sanBuilder.Build());

        var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
        var notAfter  = notBefore.AddYears(10);
        using var cert = req.CreateSelfSigned(notBefore, notAfter);

        // Export with private key so Kestrel can use it
        var pfxBytes = cert.Export(X509ContentType.Pfx, password);
        File.WriteAllBytes(path, pfxBytes);
        Console.WriteLine($"[HTTPS] Certificate saved. Add it to Trusted Root CAs on every client machine.");
        Console.WriteLine($"[HTTPS]   File: {path}");
        Console.WriteLine($"[HTTPS]   Thumbprint: {cert.Thumbprint}");

        return X509CertificateLoader.LoadPkcs12(pfxBytes, password);
    }

    /// <summary>True when <paramref name="origin"/> names the same host this request arrived on (scheme
    /// and port ignored — TLS termination or a reverse proxy can rewrite those; the host is what a
    /// cross-site attacker cannot forge). Malformed Origin values are treated as a mismatch.</summary>
    private static bool IsSameHostOrigin(string origin, HttpRequest request)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
            return false;
        return string.Equals(originUri.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Starts and blocks until the host shuts down.</summary>
    public Task RunAsync() => _app.RunAsync();

    /// <summary>Bridges ASP.NET's logging into the agent's existing <see cref="ILoggerFactory"/>.</summary>
    /// <summary>Internal rather than private: the registry-only host reuses it for the same reason
    /// this one has it — the agent already logs through SplaTelemetry, and ASP.NET must not also
    /// write to the console.</summary>
    internal sealed class ForwardingLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerFactory _factory;
        public ForwardingLoggerProvider(ILoggerFactory factory) => _factory = factory;
        public ILogger CreateLogger(string categoryName) => _factory.CreateLogger(categoryName);
        public void Dispose() { }
    }
}
