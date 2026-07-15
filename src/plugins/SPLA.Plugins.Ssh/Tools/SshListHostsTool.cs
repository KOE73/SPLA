using System.Text;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>ssh_list_hosts</c> — lists the configured SSH hosts (name, address, user, description). Never
/// discloses credentials: only the fact that a host uses a password reference or a key file, not the
/// value. Read-only, no network access.
/// </summary>
public sealed class SshListHostsTool : IMcpTool
{
    // Live settings provider — re-read per call so hosts saved from the Settings UI show up
    // without a restart.
    private readonly Func<SshSettings> _settings;
    public SshListHostsTool(Func<SshSettings> settings) => _settings = settings;

    public string Name => "ssh_list_hosts";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Lists configured SSH hosts (name, host, port, user, auth type, description). Does not reveal passwords.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new { type = "object", properties = new { } }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var settings = _settings();
        if (settings.Hosts.Count == 0)
            return Task.FromResult("No SSH hosts are configured. Add them under plugins.ssh.settings.hosts in the project config.");

        var sb = new StringBuilder();
        sb.AppendLine($"Configured SSH hosts: {settings.Hosts.Count}" +
                      (settings.DefaultHost is { } d ? $" (default: {d})" : ""));
        foreach (var (name, h) in settings.Hosts)
        {
            // Which auth path ConnectAsync will take. A credential entry decides by its own fields
            // (private_key vs password) — we only name the entry here, never look inside it.
            var auth = !string.IsNullOrWhiteSpace(h.Credential) ? $"credential '{h.Credential}'"
                : !string.IsNullOrWhiteSpace(h.KeyFile) ? "key" : "password";
            var user = h.User ?? (h.Credential is null ? "?" : "<from credential>");
            sb.AppendLine($"  {name}: {user}@{h.Host}:{h.Port}  [auth: {auth}]" +
                          (string.IsNullOrWhiteSpace(h.Description) ? "" : $"  — {h.Description}"));
        }
        return Task.FromResult(sb.ToString().TrimEnd());
    }
}
