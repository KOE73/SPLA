using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>ssh_session_close</c> — closes a persistent shell session opened by <c>ssh_session_exec</c>.
/// Omit <c>host</c> to close the only open session. Idempotent: closing a non-open session is not an error.
/// </summary>
internal sealed class SshSessionCloseTool : IMcpTool
{
    private readonly SshSessionRegistry _registry;
    public SshSessionCloseTool(SshSessionRegistry registry) => _registry = registry;

    public string Name => "ssh_session_close";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Closes a persistent SSH shell session (from ssh_session_exec). Omit 'host' to close the only open one.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Configured host name whose session to close. Omit to close the sole open session." }
                }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? hostName = null;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            hostName = ToolJson.GetStringTrimmed(doc.RootElement, "host");
        }
        catch (JsonException) { /* no args is fine */ }

        var open = _registry.OpenSessions();
        var name = hostName ?? (open.Count == 1 ? open[0] : null);
        if (name == null)
            return Task.FromResult(open.Count == 0
                ? "No open SSH sessions."
                : "Multiple sessions open — specify 'host'. Open: " + string.Join(", ", open));

        var closed = _registry.Close(name);
        return Task.FromResult(closed ? $"Closed session on '{name}'." : $"No open session on '{name}'.");
    }
}
