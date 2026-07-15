using System.Text.Json;
using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// SSH plugin: read-only remote command execution over SSH. Hosts and their credential references
/// live in the project config (<c>plugins.ssh.settings</c>); the actual passwords live in the global
/// secret store and are resolved only at connect time. Runs in the host (not a sandbox) — an SSH
/// client is a self-contained outbound-TCP tool; isolation buys little here, and the credential must
/// stay host-side regardless.
/// </summary>
public sealed class SshPlugin : ISplaPlugin, ISplaPluginAction
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // Captured at Initialize so settings-UI actions (testHost) can resolve credential references
    // the same way tools do. Actions never receive or return secret values.
    private ISecretResolver? _resolver;
    // The project-scoped session hub (shared with the service's human terminals via
    // ResolvedSettings.SharedServices) — kept so plugin actions can answer session queries.
    private SshSessionHub? _hub;

    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        _resolver = settings.SecretResolver;

        // LIVE settings: re-read the plugins.ssh blob on every use instead of snapshotting at load.
        // plugins.save mutates settings.Plugins in place, so hosts added in the Settings UI become
        // visible to the agent immediately — no service restart.
        SshSettings Current() =>
            SshSettings.FromBlob(settings.Plugins.GetValueOrDefault("ssh")?.Settings);

        // The PROJECT-scoped session hub: the same instance the service's human terminals use
        // (ResolvedSettings.SharedServices), so a session the agent opens is the session a human
        // terminal attaches to — one shell, many viewers.
        var hub = SshSessionHub.For(settings);
        _hub = hub;
        var ctx = new SessionToolContext { Settings = Current, Resolver = settings.SecretResolver };

        return new IMcpTool[]
        {
            new SshListHostsTool(Current),
            new SshRunTool(Current, settings.SecretResolver),
            new SshSessionsListTool(hub),
            new SshSessionExecTool(hub, ctx),
            new SshSessionWaitTool(hub, ctx),
            new SshSessionSendTool(hub, ctx),
            new SshSessionCloseTool(hub),
        };
    }

    /// <summary>"Test connection" from the web settings UI: connect with the given host config
    /// (credential resolved server-side) and report success — never the credential itself.</summary>
    public async Task<object?> InvokeActionAsync(string action, string? valueJson, CancellationToken ct = default)
    {
        // Open sessions (ids + hosts + opener) — feeds session pickers. Names only, no secrets.
        if (action == "listSessions")
            return _hub?.List().Select(s => new { id = s.Id, host = s.HostName, openedBy = s.OpenedBy }).ToArray()
                ?? (object)Array.Empty<object>();

        if (action != "testHost")
            throw new InvalidOperationException($"Unknown ssh plugin action: {action}");
        if (_resolver is null)
            throw new InvalidOperationException("plugin is not initialized");

        var cfg = string.IsNullOrWhiteSpace(valueJson)
            ? throw new InvalidOperationException("testHost requires a host config")
            : JsonSerializer.Deserialize<TestHostRequest>(valueJson, JsonOpts)
              ?? throw new InvalidOperationException("invalid host config");

        var host = new SshHostConfig
        {
            Host = cfg.Host, Port = cfg.Port is > 0 and < 65536 ? cfg.Port : 22,
            User = cfg.User, Credential = cfg.Credential,
            Password = cfg.Password, KeyFile = cfg.KeyFile
        };

        try
        {
            using var client = await SshConnectionFactory.ConnectAsync(host, timeoutSeconds: 15, _resolver, ct);
            var who = client.ConnectionInfo.Username;
            client.Disconnect();
            return new { ok = true, message = $"Connected as {who}@{host.Host}:{host.Port}." };
        }
        catch (Exception ex)
        {
            return new { ok = false, message = $"Failed: {ex.Message}" };
        }
    }

    private sealed record TestHostRequest(
        string? Host, int Port, string? User, string? Credential, string? Password, string? KeyFile);
}
