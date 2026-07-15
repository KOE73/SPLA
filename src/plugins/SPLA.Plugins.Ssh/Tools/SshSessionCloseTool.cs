using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>ssh_session_close</c> — closes one live SSH session by id (<c>host#N</c>). Omit <c>session</c>
/// to close the only open one. Any terminals attached to it show "closed". Idempotent.
/// </summary>
internal sealed class SshSessionCloseTool : IMcpTool
{
    private readonly SshSessionHub _hub;
    public SshSessionCloseTool(SshSessionHub hub) => _hub = hub;

    public string Name => "ssh_session_close";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Closes an open SSH session (from ssh_sessions). Pass 'session' (host#N); omit to close the sole open one.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    session = new { type = "string", description = "Session id (host#N). Omit to close the only open session." }
                }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? sessionId = null;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            sessionId = ToolJson.GetStringTrimmed(doc.RootElement, "session")
                     ?? ToolJson.GetStringTrimmed(doc.RootElement, "host"); // tolerate the old shape
        }
        catch (JsonException) { /* no args is fine */ }

        var open = _hub.List();
        // Old callers pass a bare host name — accept it when it maps to exactly one session.
        if (sessionId != null && _hub.Get(sessionId) == null)
        {
            var byHost = _hub.ForHost(sessionId);
            if (byHost.Count == 1) sessionId = byHost[0].Id;
        }
        sessionId ??= open.Count == 1 ? open[0].Id : null;

        if (sessionId == null)
            return Task.FromResult(open.Count == 0
                ? "No open SSH sessions."
                : "Multiple sessions open — specify 'session'. Open: " + string.Join(", ", open.Select(s => s.Id)));

        var closed = _hub.Close(sessionId);
        return Task.FromResult(closed ? $"Closed session '{sessionId}'." : $"No open session '{sessionId}'.");
    }
}
