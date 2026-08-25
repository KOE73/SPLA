using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.SystemTools;

/// <summary>
/// Ends a shell session and everything it spawned. The counterpart to leaving a session open:
/// an abandoned session is a live process on the host, not just a forgotten id.
/// </summary>
public class KillShellTool : IMcpTool
{
    public string Name => "system_kill_shell";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Ends an unfinished shell session, killing the command and any process it started.",
            Details = "Use this on a session you no longer need — a command stuck on a question you cannot "
                      + "answer, or one that is taking too long. A session you never end keeps running on "
                      + "the host until SPLA exits. "
                      + "'Status: running' on its own is not a reason to kill: that just means the command "
                      + "is quiet. Killing loses whatever work the command had not yet finished.",
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    session = new
                    {
                        type = "string",
                        description = "Session id from an unfinished system_run_shell result (e.g. 'sh_1')."
                    }
                },
                required = new[] { "session" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var session = ToolJson.GetStringTrimmed(doc.RootElement, "session");
            if (session is null) return ToolResult.Fail("Error: Missing 'session' parameter.", "missing session");

            var sandbox = HostServices.Sandbox;
            if (!sandbox.Gate.CanExecute() || sandbox.Shell is not { } shell)
                return ToolResult.Refuse("Error: Shell execution is disabled in this environment.", "shell disabled");

            var run = await shell.KillAsync(session, cancellationToken);
            return ToolResult.Text($"Killed session {session}.\n{ShellResultText.Render(run, 65001)}");
        }
        catch (JsonException)
        {
            return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json");
        }
        catch (InvalidOperationException ex)
        {
            return ToolResult.Fail($"Error: {ex.Message}", "no such session");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error killing session: {ex.Message}", "kill failed");
        }
    }
}
