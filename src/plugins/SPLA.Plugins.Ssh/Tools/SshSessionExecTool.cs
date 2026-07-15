using System.Text;
using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>ssh_session_exec</c> — runs a command inside a live, ADDRESSABLE shell session
/// (<c>host#N</c>) shared with the human's terminals: the human literally watches the agent's
/// command and its output appear in the attached terminal panel, and can type into the same shell.
/// Targeting: <c>session</c> picks an exact open session; else <c>host</c> reuses that host's most
/// recent session or opens a new one; else the default host applies.
///
/// <para>Read-only guard applies per host: hosts with <c>allow_write: true</c> accept any command
/// (apt, systemctl, …); others refuse mutating commands before they reach the shell. A command that
/// doesn't finish within the timeout (e.g. <c>top</c>) returns its partial output as a snapshot and
/// the shell is recovered with Ctrl+C.</para>
/// </summary>
internal sealed class SshSessionExecTool : IMcpTool
{
    private readonly SshSessionHub _hub;
    private readonly SessionToolContext _ctx;

    public SshSessionExecTool(SshSessionHub hub, SessionToolContext ctx)
    {
        _hub = hub;
        _ctx = ctx;
    }

    public string Name => "ssh_session_exec";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Runs a command in a PERSISTENT SSH shell session that the human watches live in a terminal " +
                "panel (they see every command and its output, and can type into the same shell). Sessions are " +
                "addressable as host#N — pass 'session' to target one exactly; several parallel sessions to one " +
                "host are fine (e.g. watch top in ioBroker#1 while working in ioBroker#2). Shell state (cd, env) " +
                "persists per session. On hosts without allow_write only read-only commands are accepted. " +
                "A command still running at the timeout is NOT killed: you get the output so far with status " +
                "'running' — call ssh_session_wait to keep waiting (cheap, nothing is lost between calls), " +
                "ssh_session_send to answer a prompt, or pass interrupt_on_timeout=true to Ctrl+C instead " +
                "(useful to snapshot interactive programs like top). A dropped connection (e.g. after 'reboot') " +
                "returns immediately with status 'disconnected' — reconnect later by opening a new session. " +
                "sudo password prompts are answered automatically with the host's stored credential — just run " +
                "'sudo ...' normally; never ask the user for a password and never type one yourself. " +
                "Use ssh_sessions to list open sessions, ssh_session_close to end one.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    command = new { type = "string", description = "The shell command to run (e.g. 'cd /var/log', 'tail -n 50 syslog', 'top')." },
                    session = new { type = "string", description = "Exact session id (host#N) to run in. Omit to use/open a session on 'host'." },
                    host = new { type = "string", description = "Configured host name. Omit to use the default host / the already-open session." },
                    timeout_seconds = new { type = "integer", description = "Max seconds to wait for completion (5–300). On expiry the command keeps running — continue with ssh_session_wait." },
                    interrupt_on_timeout = new { type = "boolean", description = "Send Ctrl+C when the timeout expires instead of leaving the command running (default false). Use for top-like programs you only want to sample." }
                },
                required = new[] { "command" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string command;
        string? sessionId, hostName;
        int? timeoutOverride = null;
        var interruptOnTimeout = false;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            command = ToolJson.GetStringTrimmed(doc.RootElement, "command") ?? "";
            sessionId = ToolJson.GetStringTrimmed(doc.RootElement, "session");
            hostName = ToolJson.GetStringTrimmed(doc.RootElement, "host");
            if (doc.RootElement.TryGetProperty("timeout_seconds", out var t) && t.TryGetInt32(out var tv))
                timeoutOverride = tv;
            if (doc.RootElement.TryGetProperty("interrupt_on_timeout", out var i)
                && i.ValueKind is JsonValueKind.True or JsonValueKind.False)
                interruptOnTimeout = i.GetBoolean();
        }
        catch (JsonException)
        {
            return "Error: invalid JSON arguments.";
        }

        if (string.IsNullOrWhiteSpace(command))
            return "Error: 'command' is required.";

        var (session, cfg, error) = await _ctx.ResolveAsync(_hub, sessionId, hostName, "agent", cancellationToken);
        if (session == null || cfg == null)
            return error ?? "Error: could not resolve an SSH session.";

        // Read-only enforcement per host policy — screened before the shell sees anything.
        if (!cfg.AllowWrite)
        {
            var rejection = ReadOnlyGuard.Reject(command);
            if (rejection != null)
                return $"Refused (read-only host — set allow_write in the host settings to lift): {rejection}.";
        }

        try
        {
            var tail = new StringBuilder();
            void OnChunk(string chunk)
            {
                if (string.IsNullOrEmpty(chunk)) return;
                tail.Append(chunk);
                var lastLine = LastNonEmptyLine(tail.ToString());
                ProgressScope.Report(new ToolProgress
                {
                    Message = $"[{session.Id}] {command}",
                    Details = lastLine is null ? null : new[] { new ToolProgressDetail("out", Trunc(lastLine, 200)) }
                });
            }

            var timeout = TimeSpan.FromSeconds(Math.Clamp(timeoutOverride ?? _ctx.Settings().TimeoutSeconds, 5, 300));
            var res = await session.ExecAsync(command, timeout, OnChunk, interruptOnTimeout, cancellationToken);

            var sb = new StringBuilder();
            sb.AppendLine($"[{session.Id}] $ {command}");
            sb.AppendLine(res.Status switch
            {
                "done" => $"exit: {res.ExitCode?.ToString() ?? "?"}",
                "running" => $"STILL RUNNING after {timeout.TotalSeconds:0}s — output so far below. " +
                             "Continue with ssh_session_wait (or ssh_session_send to answer a prompt / ctrl_c to stop).",
                "interrupted" => "(no exit within timeout — Ctrl+C sent, output below is a snapshot)",
                "disconnected" => "CONNECTION CLOSED BY REMOTE (reboot or network drop) — the session is gone. " +
                                  "Open a new session to reconnect; if you just rebooted, retry with pauses until the host is back.",
                _ => res.Status
            });
            var body = StripEchoedCommand(res.NewOutput, command);
            if (!string.IsNullOrWhiteSpace(body)) sb.AppendLine(body.TrimEnd());
            return sb.ToString().TrimEnd();
        }
        catch (OperationCanceledException)
        {
            return "Error: command cancelled.";
        }
        catch (Exception ex)
        {
            return $"Error in session '{session.Id}': {ex.Message}";
        }
    }

    private static string? LastNonEmptyLine(string s)
    {
        var lines = s.Split('\n');
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            var t = lines[i].Trim('\r', ' ', '\t');
            if (t.Length > 0) return t;
        }
        return null;
    }

    // The pty echoes the typed command back on the first line; drop it from the returned body so the
    // agent sees output, not its own input.
    private static string StripEchoedCommand(string output, string command)
    {
        var lines = output.Split('\n').ToList();
        for (int i = 0; i < lines.Count && i < 2; i++)
        {
            if (lines[i].Contains(command, StringComparison.Ordinal))
            {
                lines.RemoveRange(0, i + 1);
                break;
            }
        }
        return string.Join('\n', lines);
    }

    private static string Trunc(string s, int n) => s.Length <= n ? s : s[..n] + "…";
}
