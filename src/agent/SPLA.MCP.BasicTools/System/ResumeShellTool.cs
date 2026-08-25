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

/// <summary>
/// Answers — or simply keeps waiting on — a shell session that <c>system_run_shell</c> left alive.
/// </summary>
public class ResumeShellTool : IMcpTool
{
    public string Name => "system_resume_shell";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Continues a shell session that has not finished: sends an answer to a "
                          + "command waiting for input, or just waits longer.",
            Details = "Use the Session id from a system_run_shell result that showed "
                      + "'Status: waiting_for_input' or 'Status: running'. "
                      + "Set input to the answer the command is asking for (a line break is added for you) — "
                      + "for a [y/N] question that is \"y\". "
                      + "Leave input null to keep waiting without sending anything; that is not the same as "
                      + "sending an empty line. "
                      + "Output contains only what the command printed since you last looked, not the whole "
                      + "transcript. The session ends when the result shows an ExitCode.",
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.High,
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
                    },
                    input = new
                    {
                        type = new[] { "string", "null" },
                        description = "Text to send to the command's stdin, without a trailing line break. "
                                      + "Null = send nothing and keep waiting."
                    },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "session", "input", "output", "output_name" }
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

            // Deliberately not GetStringTrimmed: "keep waiting" (null) and "send this text" are
            // different acts, and trimming would quietly turn a whitespace answer into the former.
            var input = ToolJson.GetString(doc.RootElement, "input");

            var sandbox = HostServices.Sandbox;
            if (!sandbox.Gate.CanExecute() || sandbox.Shell is not { } shell)
                return ToolResult.Refuse("Error: Shell execution is disabled in this environment.", "shell disabled");

            var run = await shell.ResumeAsync(session, input, cancellationToken);
            var result = ShellResultText.Render(run, 65001);

            var target = run.Status == ShellStatus.Exited
                ? DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"))
                : OutputTarget.Context;
            if (target == OutputTarget.Context)
                return ToolResult.Text(result);

            var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
            var routed = DataChannel.Route(target, BlobPayload.OfText(result),
                $"system_resume_shell: exit={run.ExitCode}, {run.StandardOutput.Length} chars output", blobName);
            return ToolResult.Text(routed);
        }
        catch (JsonException)
        {
            return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json");
        }
        catch (InvalidOperationException ex)
        {
            // Unknown or already-finished session — a plain fact for the model, not a crash.
            return ToolResult.Fail($"Error: {ex.Message}", "no such session");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error resuming session: {ex.Message}", "resume failed");
        }
    }
}
