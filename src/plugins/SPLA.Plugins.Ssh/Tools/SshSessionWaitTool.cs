using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>ssh_session_wait</c> — continues reading a live session from where the agent last looked.
/// The companion of <c>ssh_session_exec</c>'s "still running" result: output keeps accumulating in
/// the session buffer between calls, so nothing is lost; each wait returns only the NEW output.
/// Ends when the pending command completes, an <c>until</c> regex matches, the connection drops,
/// or the timeout passes (call again). Works without a pending command too — a passive watch.
/// </summary>
internal sealed class SshSessionWaitTool : IMcpTool
{
    private readonly SshSessionHub _hub;
    private readonly SessionToolContext _ctx;

    public SshSessionWaitTool(SshSessionHub hub, SessionToolContext ctx)
    {
        _hub = hub;
        _ctx = ctx;
    }

    public string Name => "ssh_session_wait";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Keeps waiting on a live SSH session after ssh_session_exec reported 'still running' (or just " +
                "watches for new output). Returns only the NEW output since your last exec/wait — nothing is " +
                "lost between calls. Ends when the running command completes (status done + exit code), when " +
                "'until' (a regex) matches the output, when the connection drops (status disconnected — e.g. " +
                "the reboot you asked for), or when timeout_seconds passes (status running — call again; for " +
                "long jobs like apt upgrade just keep calling with generous timeouts). If output stalls and the " +
                "command seems to wait for input, answer with ssh_session_send.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    session = new { type = "string", description = "Session id (host#N). Omit to use the sole open session." },
                    timeout_seconds = new { type = "integer", description = "Max seconds to wait this call (5–600, default 60)." },
                    until = new { type = "string", description = "Optional regex — return as soon as it matches the accumulated output (e.g. 'login:' after a reboot)." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? sessionId, untilPattern;
        var timeoutSeconds = 60;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            sessionId = ToolJson.GetStringTrimmed(doc.RootElement, "session");
            untilPattern = ToolJson.GetStringTrimmed(doc.RootElement, "until");
            if (doc.RootElement.TryGetProperty("timeout_seconds", out var t) && t.TryGetInt32(out var tv))
                timeoutSeconds = tv;
        }
        catch (JsonException)
        {
            return "Error: invalid JSON arguments.";
        }

        var open = _hub.List();
        sessionId ??= open.Count == 1 ? open[0].Id : null;
        if (sessionId == null)
            return open.Count == 0
                ? "Error: no open SSH sessions."
                : "Error: multiple sessions open — specify 'session'. Open: " + string.Join(", ", open.Select(s => s.Id));

        var (session, cfg, error) = await _ctx.ResolveAsync(_hub, sessionId, null, "agent", cancellationToken);
        if (session == null || cfg == null) return error ?? "Error: session not found.";

        Regex? until = null;
        if (!string.IsNullOrEmpty(untilPattern))
        {
            try { until = new Regex(untilPattern, RegexOptions.None, TimeSpan.FromMilliseconds(200)); }
            catch (ArgumentException ex) { return $"Error: invalid 'until' regex: {ex.Message}"; }
        }

        var pending = session.PendingCommand;
        void OnChunk(string chunk)
        {
            if (string.IsNullOrEmpty(chunk)) return;
            var lastLine = chunk.Split('\n').LastOrDefault(l => l.Trim('\r', ' ', '\t').Length > 0)?.Trim();
            ProgressScope.Report(new ToolProgress
            {
                Message = $"[{session.Id}] waiting" + (pending is null ? "" : $": {pending}"),
                Details = lastLine is null ? null : new[] { new ToolProgressDetail("out", Trunc(lastLine, 200)) }
            });
        }

        try
        {
            var timeout = TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 5, 600));
            var res = await session.WaitAsync(timeout, until, OnChunk, cancellationToken);

            var sb = new StringBuilder();
            sb.AppendLine($"[{session.Id}]" + (pending is null ? " (watch)" : $" $ {pending}"));
            sb.AppendLine(res.Status switch
            {
                "done" => $"exit: {res.ExitCode?.ToString() ?? "?"}",
                "matched" => "'until' pattern matched — new output below." +
                             (pending is null ? "" : " The command is still running."),
                "running" => $"STILL RUNNING after {timeout.TotalSeconds:0}s more — new output below (empty means no " +
                             "output at all: the command may be waiting for input — check with ssh_session_send / the terminal).",
                "disconnected" => "CONNECTION CLOSED BY REMOTE (reboot or network drop) — the session is gone. " +
                                  "Open a new session to reconnect; after a reboot retry with pauses until the host is back.",
                _ => res.Status
            });
            if (!string.IsNullOrWhiteSpace(res.NewOutput)) sb.AppendLine(res.NewOutput.TrimEnd());
            return sb.ToString().TrimEnd();
        }
        catch (OperationCanceledException)
        {
            return "Error: wait cancelled.";
        }
        catch (Exception ex)
        {
            return $"Error in session '{session.Id}': {ex.Message}";
        }
    }

    private static string Trunc(string s, int n) => s.Length <= n ? s : s[..n] + "…";
}
