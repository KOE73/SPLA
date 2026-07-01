using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.SystemTools;

public class DotnetTestTool : IMcpTool
{
    public string Name => "dotnet_test_project";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Executes 'dotnet test' on the host system to run unit tests in a .NET project or solution.",
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    project_path = new
                    {
                        type = new[] { "string", "null" },
                        description = "Path to the project or solution file. Null = test in current directory."
                    },
                    configuration = new
                    {
                        type = new[] { "string", "null" },
                        description = "Test configuration: 'Debug' or 'Release'. Null = Debug."
                    },
                    no_build = new
                    {
                        type = new[] { "boolean", "null" },
                        description = "True = skip build before running tests. Null = false."
                    },
                    filter = new
                    {
                        type = new[] { "string", "null" },
                        description = "Filter expression to select specific tests. Null = run all tests."
                    },
                    cwd = new
                    {
                        type = new[] { "string", "null" },
                        description = "Working directory for the command. Null = current directory."
                    },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "project_path", "configuration", "no_build", "filter", "cwd", "output", "output_name" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var doc = JsonDocument.Parse(argumentsJson);
            
            var projectPath = ToolJson.GetString(doc.RootElement, "project_path");
            var configuration = ToolJson.GetString(doc.RootElement, "configuration") ?? "Debug";
            var noBuild       = ToolJson.GetBoolean(doc.RootElement, "no_build", false);
            var filter        = ToolJson.GetString(doc.RootElement, "filter");
            var cwd           = ToolJson.GetString(doc.RootElement, "cwd");

            var arguments = new StringBuilder("dotnet test");

            if (!string.IsNullOrWhiteSpace(projectPath))
            {
                arguments.Append($" \"{projectPath}\"");
            }

            if (!string.IsNullOrWhiteSpace(configuration))
            {
                arguments.Append($" -c {configuration}");
            }

            if (noBuild)
            {
                arguments.Append(" --no-build");
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                arguments.Append($" --filter \"{filter}\"");
            }

            var sandbox = HostServices.Sandbox;
            if (!sandbox.Gate.CanExecute() || sandbox.Shell is not { } shell)
                return "Error: Shell execution is disabled in this environment.";

            var run = await shell.RunAsync(
                new ShellCommand(arguments.ToString(), string.IsNullOrEmpty(cwd) ? null : cwd),
                cancellationToken);

            var result = $"ExitCode: {run.ExitCode}\nOutput:\n{run.StandardOutput}\nError:\n{run.StandardError}";
            var target = DataChannel.ParseTarget(ToolJson.GetString(doc.RootElement, "output"));
            if (target == OutputTarget.Context) return result;
            var blobName = ToolJson.GetString(doc.RootElement, "output_name");
            return DataChannel.Route(target, BlobPayload.OfText(result), $"dotnet_test: exit={run.ExitCode}", blobName);
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
