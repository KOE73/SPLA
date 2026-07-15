using SPLA.Runtime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

    public int Port { get; init; } = 5050;

    /// <summary>Connect secret required for non-loopback clients. Null = no token (loopback use).</summary>
    public string? Token { get; init; }

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
    public string Url { get; }

    private SplaServiceHost(
        WebApplication app, string url,
        SPLA.Observability.Collection.TelemetryCollector? collector = null, IDisposable? gaugeTimer = null)
    {
        _app = app;
        _collector = collector;
        _gaugeTimer = gaugeTimer;
        Url = url;
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
        var url = $"{scheme}://{options.Bind}:{options.Port}";

        var app = builder.Build();
        if (!options.UseHttps) app.Urls.Add(url);
        app.UseWebSockets();

        if (options.AuthEnabled)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.MapGet("/health", () => Results.Text("SPLA service running. Connect a client to /ws.", "text/plain"))
            .AllowAnonymous();

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
            HandleWebSocketAsync(context, registry, options, serverRoot, hub, auth, loggerFactory));

        return new SplaServiceHost(app, url, collector, gaugeTimer);
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
                }
            });

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
                        entry.Runtime.Settings.Connections, entry.Runtime.ConnectionHealth);
                    await hub.BroadcastToProjectAsync(projectId, Contracts.MessageTypes.ConnectionsHealth, health);
                }
                catch { }
            });
        };
    }

    /// <summary>Handles a <c>/ws</c> upgrade: the Origin gate (CSWSH defence in cookie deployments),
    /// resolving the connection's identity from the auth cookie (or the local sentinel), scoping it to
    /// the user's own server area, then running the <see cref="ClientConnection"/> for the socket's life.</summary>
    private static async Task HandleWebSocketAsync(
        HttpContext context, AgentRuntimeRegistry registry, ServiceOptions options,
        SPLA.Domain.Project.ServerProjectRoot? serverRoot, ConnectionHub hub, AuthGate auth, ILoggerFactory loggerFactory)
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
        var conn = new ClientConnection(socket, registry, hub, auth, log, identity, userProvider, userDefault, userArea);
        await conn.RunAsync(context.RequestAborted);
    }

    public Task StartAsync(CancellationToken ct = default) => _app.StartAsync(ct);

    public async Task StopAsync(CancellationToken ct = default)
    {
        _gaugeTimer?.Dispose();
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
    private sealed class ForwardingLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerFactory _factory;
        public ForwardingLoggerProvider(ILoggerFactory factory) => _factory = factory;
        public ILogger CreateLogger(string categoryName) => _factory.CreateLogger(categoryName);
        public void Dispose() { }
    }
}
