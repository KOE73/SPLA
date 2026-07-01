using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SPLA.Service;

/// <summary>Bind/port/token for the service endpoint.</summary>
public sealed record ServiceOptions
{
    /// <summary>Address to bind. Loopback by default — no auth needed; the OS gates access.</summary>
    public string Bind { get; init; } = "127.0.0.1";

    public int Port { get; init; } = 5050;

    /// <summary>Connect secret required for non-loopback clients. Null = no token (loopback use).</summary>
    public string? Token { get; init; }

    public bool IsLoopback => Bind is "127.0.0.1" or "localhost" or "::1";
}

/// <summary>
/// The ASP.NET host exposing the agent over a WebSocket at <c>/ws</c>. One <see cref="AgentRuntime"/>
/// (the agent on the project) is shared by every connection. Built so the CLI can run it alongside
/// the console REPL, and so the embedded Avalonia client can later self-host an instance bound to
/// loopback and talk to it on the same footing as a remote one.
/// </summary>
public sealed class SplaServiceHost
{
    private readonly WebApplication _app;
    public string Url { get; }

    private SplaServiceHost(WebApplication app, string url)
    {
        _app = app;
        Url = url;
    }

    /// <param name="chats">
    /// The shared chat registry. Passed in (not created here) so a parallel console REPL can drive
    /// the very same <see cref="ChatRuntime"/> instances as socket clients — one agent, consistent
    /// state, regardless of which входной канал sent the message.
    /// </param>
    public static SplaServiceHost Build(AgentRuntime runtime, ServiceOptions options, ChatRegistry chats)
    {
        var builder = WebApplication.CreateBuilder();

        // The agent already logs through SplaTelemetry; keep ASP.NET quiet on the console.
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new ForwardingLoggerProvider(runtime.LoggerFactory));

        var url = $"http://{options.Bind}:{options.Port}";

        var app = builder.Build();
        app.Urls.Add(url);
        app.UseWebSockets();

        var hub = new ConnectionHub();
        var auth = new AuthGate(options.Token);
        var inspector = new LiveAgentInspector(runtime);

        // The single place that maps domain events to wire broadcasts. Mutators publish to
        // runtime.Events knowing nothing of sockets; here each event becomes a fan-out message.
        runtime.Events.Subscribe(evt =>
        {
            switch (evt)
            {
                case AppearanceChanged a:
                    _ = hub.BroadcastAsync(Contracts.MessageTypes.AppearanceChanged,
                        new Contracts.AppearanceChangedPayload { Theme = a.Theme, Density = a.Density });
                    break;
            }
        });

        app.MapGet("/health", () => Results.Text("SPLA service running. Connect a client to /ws.", "text/plain"));

        // Warm the health cache in the background right after the service starts so the first
        // client to open settings sees real results (or the cached "not yet checked" state) instantly.
        _ = Task.Run(async () =>
        {
            try
            {
                var health = await ConnectionDiagOps.PingAllAsync(
                    runtime.Settings.Connections, runtime.ConnectionHealth);
                await hub.BroadcastAsync(Contracts.MessageTypes.ConnectionsHealth, health);
            }
            catch { }
        });

        // The browser client — served from embedded static assets. Any client drives the same agent /ws.
        static IResult ServeAsset(string path)
        {
            var asset = WebAssets.Get(path);
            return asset is { } a ? Results.Bytes(a.Bytes, a.ContentType) : Results.NotFound();
        }
        // Persisted chat image attachments (sidecar files). Served read-only with a path-traversal guard.
        app.MapGet("/chat-image/{chatId}/{file}", (string chatId, string file) =>
        {
            var path = ChatImages.Resolve(runtime.Settings.WorkspacePath, chatId, file);
            return path != null
                ? Results.File(path, ChatImages.ContentType(file))
                : Results.NotFound();
        });

        // Plugin-contributed web settings UI — served straight from each plugin's own directory
        // (see PluginMeta.WebSettingsEntry). The host never builds or knows the content of these
        // files; it only resolves pluginId → directory and streams whatever is there.
        app.MapGet("/plugin-assets/{pluginId}/{**path}", (string pluginId, string path) =>
        {
            var dir = runtime.PluginManager.GetPluginDirectory(pluginId);
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

            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            var log = runtime.LoggerFactory.CreateLogger<ClientConnection>();
            var conn = new ClientConnection(socket, runtime, chats, hub, auth, inspector, log);
            await conn.RunAsync(context.RequestAborted);
        });

        return new SplaServiceHost(app, url);
    }

    public Task StartAsync(CancellationToken ct = default) => _app.StartAsync(ct);
    public Task StopAsync(CancellationToken ct = default) => _app.StopAsync(ct);

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
