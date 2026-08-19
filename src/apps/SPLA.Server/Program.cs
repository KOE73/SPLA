using SPLA.Runtime;
using System.Text.Json;
using SPLA.Domain.Identity;
using SPLA.Domain.Settings;
using SPLA.Observability;
using SPLA.Service;
using SPLA.Service.Auth;
using Microsoft.Extensions.Logging;

// The dedicated server host. The actual WebSocket/HTTP service lives in SPLA.Service
// (SplaServiceHost) — this exe is the DEPLOYMENT home for it: the place where domain concerns
// (Negotiate auth, per-identity connections, running as a service) belong, kept out of the CLI.
// NOTE: this project has NO platform reference. The identity provider (Windows, Linux, …) is a DLL
// named in server.json and loaded by reflection — swap the DLL, not this host, to change platform.
Console.WriteLine("=== SPLA Server ===");

SplaTelemetry.ConfigureGlobalLogs();
using var loggerFactory = LoggerFactory.Create(b =>
{
    b.ClearProviders();
    b.AddProvider(SplaTelemetry.CreateFileLoggerProvider());
    b.SetMinimumLevel(LogLevel.Information);
});
var log = loggerFactory.CreateLogger("SPLA.Server");

// Load the identity provider named in server.json (next to the exe). No config → the neutral
// built-in provider. This is the only place platform choice enters — as data, not a reference.
var identityProvider = LoadIdentityProvider();
var hostIdentity = identityProvider.Current();
Console.WriteLine($"Server identity: {hostIdentity.DisplayName}  ({hostIdentity.UserKey}, {hostIdentity.Groups.Count} groups)");

// args: [--port N] [--bind addr] [--token X] [--auth negotiate|local|none] [--no-auth]
//       [--no-register] [--root DIR] [--https] [--cert file] [--cert-password pw] [--hub]
// Binds all interfaces by default — this is the remote server host, meant to be reached from other
// machines. Domain deployments gate access by Negotiate; home/workgroup deployments use --auth local
// (username/password + admin panel). --no-auth is the same as --auth none.
int port = 5050;
string bind = "0.0.0.0";
string? token = null;
bool requireAuth = true;
string? authArg = null;
bool allowRegister = true;
string? rootOverride = null;
bool useHttps = false;
string? certPath = null;
string certPassword = "spla";
string? otlpEndpoint = null;
bool runHub = false;
for (int i = 0; i < args.Length; i++)
{
    switch (args[i].ToLowerInvariant())
    {
        case "--port":     if (i + 1 < args.Length && int.TryParse(args[++i], out var p)) port = p; break;
        case "--bind":     if (i + 1 < args.Length) bind = args[++i]; break;
        case "--token":    if (i + 1 < args.Length) token = args[++i]; break;
        case "--auth":     if (i + 1 < args.Length) authArg = args[++i]; break;
        case "--no-auth":  requireAuth = false; break;
        case "--no-register": allowRegister = false; break;
        case "--root":     if (i + 1 < args.Length) rootOverride = args[++i]; break;
        case "--https":    useHttps = true; break;
        case "--cert":     if (i + 1 < args.Length) certPath = args[++i]; break;
        case "--cert-password": if (i + 1 < args.Length) certPassword = args[++i]; break;
        case "--otlp-endpoint": if (i + 1 < args.Length) otlpEndpoint = args[++i]; break;
        case "--hub":      runHub = true; break;
    }
}

// Resolve the auth mode: an explicit --auth wins; otherwise --no-auth → none, default → negotiate.
var authMode = authArg?.ToLowerInvariant() switch
{
    "local"     => AuthMode.Local,
    "negotiate" => AuthMode.Negotiate,
    "none"      => AuthMode.None,
    null        => requireAuth ? AuthMode.Negotiate : AuthMode.None,
    _           => requireAuth ? AuthMode.Negotiate : AuthMode.None
};

// The server's storage root: {root}/users/{sid}/ holds each user's own area. --root wins over
// server.json "serverRoot". This is the setting the user asked for — one folder where everything for
// every user lives (file-storage variant).
var serverRootPath = rootOverride ?? LoadServerRoot();
SPLA.Domain.Project.ServerProjectRoot? serverRoot = null;
if (!string.IsNullOrWhiteSpace(serverRootPath))
{
    serverRoot = new SPLA.Domain.Project.ServerProjectRoot(serverRootPath);
    Console.WriteLine($"Server root: {serverRootPath}  (each user gets {serverRootPath}\\users\\<sid>\\)");

    // A project inside somebody's area means THEIR skill branches, THEIR trust grants and THEIR
    // drafts folder — not the service account's. Registered before the first LoadAndResolve, and
    // deliberately keyed off the workspace rather than the connection: a runtime is per project, and
    // on a server a project already lives in exactly one person's area.
    ConfigLoader.PersonalDirResolver = workspace => serverRoot.UserAreaFor(workspace);
}
else
{
    Console.WriteLine("Server root: NOT SET — all clients share one project. Set server.json \"serverRoot\" or pass --root for per-user areas.");
}

// The server process itself has no ambient project: users route into their own areas. Its own
// settings are the minimal default, used only for the host's logger/plugin manager, never shown to a user.
var settings = ConfigLoader.LoadAndResolve();
SplaTelemetry.ConfigureProjectLogs(settings.Project.GetBucket("logs").MapToHostDirectory());

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    log.LogCritical(e.ExceptionObject as Exception, "Unhandled exception");
    Console.Error.WriteLine($"\n[FATAL] {e.ExceptionObject}");
};

using var registry = new AgentRuntimeRegistry(loggerFactory)
{
    DefaultProjectId = AgentRuntimeRegistry.NoProjectId,
    InstanceMode = "server"
};

// Local-credentials mode: a JSON user store lives in the server root (so it sits with the per-user
// areas) or next to the exe when no root is configured. The account service seeds a first admin when
// the store is empty (random password printed to the console on that first run).
LocalAccountService? accounts = null;
if (authMode == AuthMode.Local)
{
    var usersPath = !string.IsNullOrWhiteSpace(serverRootPath)
        ? Path.Combine(serverRootPath, "users.json")
        : Path.Combine(AppContext.BaseDirectory, "users.json");
    accounts = new LocalAccountService(
        new JsonUserStore(usersPath), allowRegister, loggerFactory.CreateLogger("LocalAuth"));
}

// Persist the local stats "over time" buckets in the server root (survives restart) when a root is set.
var statsPath = string.IsNullOrWhiteSpace(serverRootPath)
    ? null
    : Path.Combine(serverRootPath, "stats.json");

var options = new ServiceOptions
{
    Port = port, Bind = bind, Token = token,
    Auth = authMode, RequireAuthentication = authMode == AuthMode.Negotiate,
    AllowSelfRegistration = allowRegister,
    UseHttps = useHttps, CertPath = certPath, CertPassword = certPassword,
    StatsPath = statsPath, OtlpEndpoint = otlpEndpoint,
    // A server that is also the hub its machines' instances register with. Opt-in rather than
    // automatic: mapping registration routes onto a production deployment is a decision, and the
    // routes are the same ones `spla hub` runs alone — one implementation for both scenarios.
    RegistryHub = runHub ? new SPLA.Instances.RegistryHub() : null,
    // A server never exits, so nothing else would ever let go of a runtime built for somebody who
    // logged off this morning. Twenty minutes is long enough that walking away from a project and
    // coming back costs nothing visible, short enough that a day's worth of users does not stack up.
    EvictIdleProjectsAfter = TimeSpan.FromMinutes(20)
};
var host = SplaServiceHost.Build(registry, options, identityProvider, serverRoot, accounts);
await host.StartAsync();

var wsUrl = host.Url.Replace("http://", "ws://") + "/ws";
Console.WriteLine($"\nSPLA server listening on {host.Url}  (WebSocket: {wsUrl})");
switch (authMode)
{
    case AuthMode.Negotiate:
        Console.WriteLine("Auth: Negotiate (NTLM/Kerberos) — each client authenticates as its domain user; identity flows to the connection.");
        break;
    case AuthMode.Local:
        Console.WriteLine($"Auth: local username/password. Sign in at {host.Url}/login  ·  admin panel at {host.Url}/admin");
        Console.WriteLine($"      Self-registration: {(allowRegister ? "enabled (/register)" : "disabled — admin creates accounts")}.");
        if (string.IsNullOrWhiteSpace(serverRootPath))
            Console.WriteLine("      NOTE: no server root set — all users share one project. Set --root for per-user areas.");
        break;
    case AuthMode.None:
        if (token == null && !options.IsLoopback)
            Console.WriteLine("WARNING: --no-auth on a non-loopback bind without --token — anyone who can reach this port controls the agent.");
        break;
}
if (runHub)
    Console.WriteLine(
        $"Registry hub: instances register at {host.Url}{SPLA.Instances.RegistryRoutes.Channel}, " +
        $"observers read {SPLA.Instances.RegistryRoutes.Instances}");
Console.WriteLine($"Stats: local dashboard at {host.Url}/stats"
    + (string.IsNullOrWhiteSpace(otlpEndpoint) ? "  (OTLP export off — pass --otlp-endpoint to ship to Grafana/Jaeger/…)" : $"  ·  OTLP → {otlpEndpoint}"));
Console.WriteLine("Press Ctrl+C to stop.");

var stop = new TaskCompletionSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.TrySetResult(); };
await stop.Task;

// Reads the "serverRoot" string from server.json (next to the exe), or null when unset/unreadable.
static string? LoadServerRoot()
{
    var configPath = Path.Combine(AppContext.BaseDirectory, "server.json");
    if (!File.Exists(configPath)) return null;
    try
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
        return doc.RootElement.TryGetProperty("serverRoot", out var r) ? r.GetString() : null;
    }
    catch { return null; }
}

// Reads server.json (next to the exe) for the identity provider DLL to load. Missing/blank config
// falls back to the neutral built-in provider — the host runs on any platform with no config at all.
static IIdentityProvider LoadIdentityProvider()
{
    var baseDir = AppContext.BaseDirectory;
    var configPath = Path.Combine(baseDir, "server.json");
    IdentityProviderConfig? idConfig = null;
    if (File.Exists(configPath))
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
            if (doc.RootElement.TryGetProperty("identity", out var id))
                idConfig = id.Deserialize<IdentityProviderConfig>(
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: could not read server.json identity config: {ex.Message} — using built-in provider.");
        }
    }

    try
    {
        var provider = IdentityProviderLoader.Load(idConfig, baseDir);
        Console.WriteLine(idConfig?.Assembly is { Length: > 0 } a
            ? $"Identity provider: {idConfig.Type} (from {a})"
            : "Identity provider: built-in (neutral claims) — no platform DLL configured.");
        return provider;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"WARNING: failed to load configured identity provider ({idConfig?.Assembly}): {ex.Message} — using built-in provider.");
        return new ClaimsIdentityProvider();
    }
}
