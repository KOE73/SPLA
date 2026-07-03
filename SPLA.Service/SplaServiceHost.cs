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
    /// single-user, so existing tests and the embedded WebView are unaffected.</summary>
    public bool RequireAuthentication { get; init; }

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
    private const string UserKeyClaim = "spla.userkey";

    private readonly WebApplication _app;
    public string Url { get; }

    private SplaServiceHost(WebApplication app, string url)
    {
        _app = app;
        Url = url;
    }

    public static SplaServiceHost Build(
        AgentRuntimeRegistry registry, ServiceOptions options, IIdentityProvider? identityProvider = null,
        SPLA.Domain.Project.ServerProjectRoot? serverRoot = null)
    {
        // The host never references a platform: the provider is passed in (loaded from config by the
        // deployment) or defaults to the neutral claims provider. Windows is a DLL, not a dependency.
        var idp = identityProvider ?? new ClaimsIdentityProvider();

        var builder = WebApplication.CreateBuilder();

        var hub = new ConnectionHub();
        var auth = new AuthGate(options.Token);

        // Domain auth for the server deployment. Negotiate (NTLM/Kerberos) authenticates the PAGE once
        // via /login; that issues a cookie, and every later request — crucially the WebSocket upgrade,
        // which a browser cannot drive through a Negotiate challenge/response — rides that cookie. So
        // the default scheme is the cookie; the challenge redirects to /login where Negotiate runs.
        if (options.RequireAuthentication)
        {
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(o =>
                {
                    o.Cookie.Name = "spla.auth";
                    o.Cookie.HttpOnly = true;
                    o.ExpireTimeSpan = TimeSpan.FromHours(10);
                    o.SlidingExpiration = true;
                    o.LoginPath = "/login";
                })
                .AddNegotiate();
            builder.Services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        }

        // Wires a newly-built runtime's domain events and initial connection-health check into the
        // hub, scoped to its own project id — fires for the eagerly-opened default project below AND
        // for any project a client opens/creates later via ProjectOps, so live updates are never
        // limited to whichever project happened to exist at process startup.
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

        if (options.RequireAuthentication)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.MapGet("/health", () => Results.Text("SPLA service running. Connect a client to /ws.", "text/plain"))
            .AllowAnonymous();

        if (options.RequireAuthentication)
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

        app.MapGet("/", () => ServeAsset("/index.html"));
        app.MapGet("/{**path}", (string path) => ServeAsset("/" + path));

        app.Map("/ws", async (HttpContext context) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            // The connection's user comes straight from the auth cookie (set at /login) — the user key
            // and display name, no server-side lookup, so it survives restarts. Reaching here already
            // means the cookie authenticated (fallback policy on /ws); the key is just read back.
            IIdentity identity;
            if (options.RequireAuthentication)
            {
                var userKey = context.User.FindFirst(UserKeyClaim)?.Value;
                if (string.IsNullOrEmpty(userKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                identity = new ClaimIdentity(userKey, context.User.Identity?.Name ?? userKey, Array.Empty<string>());
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
        });

        return new SplaServiceHost(app, url);
    }

    public Task StartAsync(CancellationToken ct = default) => _app.StartAsync(ct);
    public Task StopAsync(CancellationToken ct = default) => _app.StopAsync(ct);

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
