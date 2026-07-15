using System.Text;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>ssh_sessions</c> — lists the open SSH sessions with their addressable ids (<c>host#N</c>),
/// who opened each (agent/human) and how many terminals are watching. Read-only, no network.
/// </summary>
internal sealed class SshSessionsListTool : IMcpTool
{
    private readonly SshSessionHub _hub;
    public SshSessionsListTool(SshSessionHub hub) => _hub = hub;

    public string Name => "ssh_sessions";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Lists open SSH sessions: id (host#N — pass it to ssh_session_exec's 'session'), host, who opened it, viewer count.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new { type = "object", properties = new { } }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var sessions = _hub.List();
        if (sessions.Count == 0)
            return Task.FromResult("No open SSH sessions. ssh_session_exec opens one automatically on first use.");

        var sb = new StringBuilder($"Open SSH sessions: {sessions.Count}\n");
        foreach (var s in sessions)
            sb.AppendLine($"  {s.Id}  host={s.HostName}  opened_by={s.OpenedBy}  viewers={s.ViewerCount}  since={s.OpenedAt:HH:mm:ss}"
                + (s.PendingCommand is string cmd ? $"  running: {cmd}" : ""));
        return Task.FromResult(sb.ToString().TrimEnd());
    }
}
