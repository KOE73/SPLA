using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SPLA.Instances;

namespace SPLA.Service;

/// <summary>
/// A process that is only a hub: it holds no project, runs no agent, and answers nothing but the
/// registry routes.
///
/// <para>Separate from <see cref="SplaServiceHost"/> rather than a flag on it, because that host
/// exists to serve one project's agent and builds a runtime before it opens a socket. A hub that had
/// to open a project first would be a hub with a project — which is exactly the coupling the
/// instance design is trying to remove. A <c>serve</c> that also wants to be a hub maps the same
/// routes onto its own app instead; there is one implementation either way.</para>
/// </summary>
public sealed class RegistryHubHost
{
    private readonly WebApplication _app;

    private RegistryHubHost(WebApplication app, RegistryHub hub)
    {
        _app = app;
        Hub = hub;
    }

    public RegistryHub Hub { get; }

    /// <summary>Where it ended up listening. Only meaningful after <see cref="StartAsync"/>, because
    /// port 0 — the sensible default for something started beside a window — is not resolved until
    /// Kestrel binds.</summary>
    public string Url { get; private set; } = "";

    public static RegistryHubHost Build(string bind, int port, string? token, ILoggerFactory loggers)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new SplaServiceHost.ForwardingLoggerProvider(loggers));

        var app = builder.Build();
        app.Urls.Add($"http://{bind}:{port}");
        app.UseWebSockets();

        var hub = new RegistryHub();
        app.MapGet("/health", () => Results.Text("SPLA registry hub running.", "text/plain"));
        app.MapRegistry(hub, token);

        return new RegistryHubHost(app, hub);
    }

    public async Task StartAsync()
    {
        await _app.StartAsync();
        Url = _app.Urls.FirstOrDefault()?.Replace("0.0.0.0", "127.0.0.1") ?? "";
    }

    public Task StopAsync() => _app.StopAsync();

    public Task WaitForShutdownAsync() => _app.WaitForShutdownAsync();
}
