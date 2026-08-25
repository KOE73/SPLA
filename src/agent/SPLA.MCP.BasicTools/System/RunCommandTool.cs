using SPLA.Domain.Agent;
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

public class RunCommandTool : IMcpTool
{
    public string Name => "system_run_shell";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Executes a shell command on the host system. " +
                          "Set output='blob' to capture large stdout without flooding context.",
            Details = "The command may come back before it has finished. If the result shows "
                      + "'Status: waiting_for_input', the command is alive and asking you something — "
                      + "its question is in Output. Answer it with system_resume_shell using the "
                      + "Session id shown, or end the command with system_kill_shell. "
                      + "'Status: running' means it is merely quiet, not asking. "
                      + "Never re-issue a command that is still running: start it once, then drive the session.",
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.High,
            StrictSchema = true,
            // Long builds, installs and scans are exactly the "runs longer than a turn should wait"
            // case ADR §2's criterion names — and the tool already has its own way to say "I need to
            // ask something" (waiting_for_input), which a background run answers with an automatic
            // refusal rather than a hang. See PLAN_20260824-2 step 1.7.
            SupportsBackground = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    command = new
                    {
                        type = "string",
                        description = "The command to execute"
                    },
                    cwd = new
                    {
                        type = new[] { "string", "null" },
                        description = "Current working directory for the command. Null = current directory."
                    },
                    code_page = new
                    {
                        type = new[] { "integer", "null" },
                        description = "Windows console code page for native command output. Null = 65001 (UTF-8)."
                    },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "command", "cwd", "code_page", "output", "output_name" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var cmd      = ToolJson.GetString(doc.RootElement, "command");
            var cwd      = ToolJson.GetString(doc.RootElement, "cwd");
            var codePage = ToolJson.GetInt32(doc.RootElement, "code_page", 65001);

            if (string.IsNullOrEmpty(cmd)) return ToolResult.Fail("Error: Missing 'command' parameter.", "missing command");

            var sandbox = HostServices.Sandbox;
            if (!sandbox.Gate.CanExecute() || sandbox.Shell is not { } shell)
                return ToolResult.Refuse("Error: Shell execution is disabled in this environment.", "shell disabled");

            var run = await shell.RunAsync(
                new ShellCommand(cmd, string.IsNullOrEmpty(cwd) ? null : cwd, codePage),
                cancellationToken);

            var result = ShellResultText.Render(run, codePage);

            // An unfinished run goes to context whatever was asked: the session id and the question
            // are the whole point of this reply, and stashing them in a blob hides both.
            var target = run.Status == ShellStatus.Exited
                ? DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"))
                : OutputTarget.Context;
            if (target == OutputTarget.Context)
                return ToolResult.Text(result);
            var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
            var routed = DataChannel.Route(target, BlobPayload.OfText(result), $"system_run_shell: exit={run.ExitCode}, {run.StandardOutput.Length} chars output", blobName);
            return ToolResult.Text(routed);
        }
        catch (JsonException)
        {
            return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error executing command: {ex.Message}", "execution failed");
        }
    }
}
