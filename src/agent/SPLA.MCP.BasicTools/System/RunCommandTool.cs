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
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.High,
            StrictSchema = true,
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

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var cmd      = ToolJson.GetString(doc.RootElement, "command");
            var cwd      = ToolJson.GetString(doc.RootElement, "cwd");
            var codePage = ToolJson.GetInt32(doc.RootElement, "code_page", 65001);

            if (string.IsNullOrEmpty(cmd)) return "Error: Missing 'command' parameter.";

            var sandbox = HostServices.Sandbox;
            if (!sandbox.Gate.CanExecute() || sandbox.Shell is not { } shell)
                return "Error: Shell execution is disabled in this environment.";

            var run = await shell.RunAsync(
                new ShellCommand(cmd, string.IsNullOrEmpty(cwd) ? null : cwd, codePage),
                cancellationToken);

            var result = $"ExitCode: {run.ExitCode}\nCodePage: {codePage}\nOutput:\n{run.StandardOutput}\nError:\n{run.StandardError}";
            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
            if (target == OutputTarget.Context) return result;
            var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
            return DataChannel.Route(target, BlobPayload.OfText(result), $"system_run_shell: exit={run.ExitCode}, {run.StandardOutput.Length} chars output", blobName);
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error executing command: {ex.Message}";
        }
    }
}
