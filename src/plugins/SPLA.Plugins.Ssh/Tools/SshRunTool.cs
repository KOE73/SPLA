using System.Text;
using System.Text.Json;
using Renci.SshNet;
using SPLA.Domain.Models;
using SPLA.Domain.Secrets;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>ssh_run</c> — connects to a configured host and runs a single <b>read-only</b> command,
/// returning its stdout/stderr/exit code. Every command is screened by <see cref="ReadOnlyGuard"/>
/// before a connection is even opened; anything that could mutate the remote system is refused. This
/// is the first-cut SSH tool: one-shot commands, no interactive session, no sudo.
/// </summary>
public sealed class SshRunTool : IMcpTool
{
    private readonly SshSettings _settings;
    private readonly ISecretResolver _resolver;

    public SshRunTool(SshSettings settings, ISecretResolver resolver)
    {
        _settings = settings;
        _resolver = resolver;
    }

    public string Name => "ssh_run";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Runs a single READ-ONLY command over SSH on a configured host and returns its output. " +
                "Only read-only commands are permitted (ls, cat, grep, ps, df, uname, ip, systemctl status, etc.); " +
                "anything that modifies the system, redirects output, or uses sudo is refused. " +
                "No interactive session — one command per call.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    command = new { type = "string", description = "The read-only shell command to run (e.g. 'uname -a', 'df -h', 'ls -la /var/log')." },
                    host = new { type = "string", description = "Configured host name to target. Omit to use the default host." }
                },
                required = new[] { "command" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string command;
        string? hostName;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            command = ToolJson.GetStringTrimmed(doc.RootElement, "command") ?? "";
            hostName = ToolJson.GetStringTrimmed(doc.RootElement, "host");
        }
        catch (JsonException)
        {
            return "Error: invalid JSON arguments.";
        }

        if (string.IsNullOrWhiteSpace(command))
            return "Error: 'command' is required.";

        // Read-only enforcement happens BEFORE connecting — a rejected command never reaches the host.
        var rejection = ReadOnlyGuard.Reject(command);
        if (rejection != null)
            return $"Refused (read-only mode): {rejection}.";

        var (name, cfg) = ResolveHost(hostName);
        if (cfg == null)
            return hostName == null
                ? "Error: no default host configured. Set plugins.ssh.settings.default_host or pass 'host'."
                : $"Error: unknown host '{hostName}'. Use ssh_list_hosts to see configured hosts.";

        SshClient? client = null;
        try
        {
            client = await SshConnectionFactory.ConnectAsync(cfg, _settings.TimeoutSeconds, _resolver, cancellationToken);

            using var cmd = client.CreateCommand(command);
            cmd.CommandTimeout = TimeSpan.FromSeconds(Math.Clamp(_settings.TimeoutSeconds, 5, 120));
            var stdout = await Task.Run(() => cmd.Execute(), cancellationToken);

            var sb = new StringBuilder();
            sb.AppendLine($"[{name}] {cfg.User}@{cfg.Host}  $ {command}");
            sb.AppendLine($"exit: {cmd.ExitStatus}");
            if (!string.IsNullOrEmpty(stdout))
                sb.AppendLine(stdout.TrimEnd());
            if (!string.IsNullOrEmpty(cmd.Error))
                sb.AppendLine($"[stderr]\n{cmd.Error.TrimEnd()}");
            return sb.ToString().TrimEnd();
        }
        catch (OperationCanceledException)
        {
            return "Error: command cancelled or timed out.";
        }
        catch (Exception ex)
        {
            // Never echo the arguments/credential; report only the failure reason.
            return $"Error connecting/running on '{name}': {ex.Message}";
        }
        finally
        {
            if (client != null)
            {
                if (client.IsConnected) client.Disconnect();
                client.Dispose();
            }
        }
    }

    private (string name, SshHostConfig? cfg) ResolveHost(string? requested)
    {
        var name = requested ?? _settings.DefaultHost
            ?? (_settings.Hosts.Count == 1 ? _settings.Hosts.Keys.First() : null);
        if (name != null && _settings.Hosts.TryGetValue(name, out var cfg))
            return (name, cfg);
        return (name ?? "(none)", null);
    }
}
