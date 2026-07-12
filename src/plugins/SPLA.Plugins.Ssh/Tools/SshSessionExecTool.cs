using System.Text;
using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>ssh_session_exec</c> — runs a READ-ONLY command inside a <b>persistent</b> shell session (opened
/// on first use, reused after), and <b>streams the output to the human live</b> via
/// <see cref="ProgressScope"/> as it arrives. Unlike <c>ssh_run</c>, shell state carries over between
/// calls (<c>cd</c>, env), so this behaves like a real console the human watches in real time.
///
/// <para>Read-only enforcement is unchanged: the agent's command is screened by
/// <see cref="ReadOnlyGuard"/> before it is written to the shell. (When the browser terminal lands,
/// the human typing directly into the pty is out of scope for this guard — their own supervised session.)</para>
/// </summary>
internal sealed class SshSessionExecTool : IMcpTool
{
    private readonly SshSessionRegistry _registry;
    private readonly int _timeoutSeconds;

    public SshSessionExecTool(SshSessionRegistry registry, int timeoutSeconds)
    {
        _registry = registry;
        _timeoutSeconds = timeoutSeconds;
    }

    public string Name => "ssh_session_exec";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Runs a READ-ONLY command in a PERSISTENT SSH shell session (a live console the human " +
                "watches in real time). Shell state persists between calls: cd, exported vars, etc. carry " +
                "over. The session opens automatically on first use and is reused. Only read-only commands " +
                "are allowed (same guard as ssh_run); mutation/redirection/sudo are refused before running. " +
                "Use ssh_session_close to end the session.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    command = new { type = "string", description = "The read-only command to run in the session (e.g. 'cd /var/log', 'tail -n 50 syslog')." },
                    host = new { type = "string", description = "Configured host name. Omit to use the default host / the already-open session." }
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

        // Read-only enforcement — agent command screened before it reaches the shell.
        var rejection = ReadOnlyGuard.Reject(command);
        if (rejection != null)
            return $"Refused (read-only mode): {rejection}.";

        var (name, cfg) = _registry.ResolveHost(hostName);
        if (cfg == null)
            return hostName == null
                ? "Error: no default host configured. Set plugins.ssh.settings.default_host or pass 'host'."
                : $"Error: unknown host '{hostName}'. Use ssh_list_hosts to see configured hosts.";

        try
        {
            var session = await _registry.GetOrOpenAsync(name!, cfg, cancellationToken);

            // Stream each output chunk to the human live. The status bar / CLI shows the latest line;
            // the full transcript is the tool's returned value (what the agent reads).
            var tail = new StringBuilder();
            void OnChunk(string chunk)
            {
                if (string.IsNullOrEmpty(chunk)) return;
                tail.Append(chunk);
                var lastLine = LastNonEmptyLine(tail.ToString());
                ProgressScope.Report(new ToolProgress
                {
                    Message = $"[{name}] {command}",
                    Details = lastLine is null ? null : new[] { new ToolProgressDetail("out", Trunc(lastLine, 200)) }
                });
            }

            var (output, exit) = await session.RunAsync(
                command, OnChunk, TimeSpan.FromSeconds(Math.Clamp(_timeoutSeconds, 5, 120)), cancellationToken);

            var sb = new StringBuilder();
            sb.AppendLine($"[{name}] $ {command}");
            if (exit is int code) sb.AppendLine($"exit: {code}");
            var body = StripEchoedCommand(output, command);
            if (!string.IsNullOrWhiteSpace(body)) sb.AppendLine(body.TrimEnd());
            return sb.ToString().TrimEnd();
        }
        catch (OperationCanceledException)
        {
            return "Error: command cancelled or timed out.";
        }
        catch (Exception ex)
        {
            return $"Error in session on '{name}': {ex.Message}";
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
