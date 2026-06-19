using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.SystemTools;

public class DotnetBuildTool : IMcpTool
{
    public string Name => "dotnet_build_project";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Executes 'dotnet build' on the host system to compile a .NET project or solution.",
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
                        description = "Path to the project or solution file. Null = build in current directory."
                    },
                    configuration = new
                    {
                        type = new[] { "string", "null" },
                        description = "Build configuration: 'Debug' or 'Release'. Null = Debug."
                    },
                    no_restore = new
                    {
                        type = new[] { "boolean", "null" },
                        description = "True = skip implicit restore. Null = false."
                    },
                    cwd = new
                    {
                        type = new[] { "string", "null" },
                        description = "Working directory for the command. Null = current directory."
                    }
                },
                required = new[] { "project_path", "configuration", "no_restore", "cwd" }
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
            var noRestore     = ToolJson.GetBoolean(doc.RootElement, "no_restore", false);
            var cwd           = ToolJson.GetString(doc.RootElement, "cwd");

            var arguments = new StringBuilder("build");
            
            if (!string.IsNullOrWhiteSpace(projectPath))
            {
                arguments.Append($" \"{projectPath}\"");
            }
            
            if (!string.IsNullOrWhiteSpace(configuration))
            {
                arguments.Append($" -c {configuration}");
            }
            
            if (noRestore)
            {
                arguments.Append(" --no-restore");
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = string.IsNullOrEmpty(cwd) ? Directory.GetCurrentDirectory() : cwd
            };
            
            processStartInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";

            using var process = Process.Start(processStartInfo);
            if (process == null) return "Error: Could not start process.";

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var output = await outputTask;
            var error = await errorTask;

            return $"ExitCode: {process.ExitCode}\nOutput:\n{output}\nError:\n{error}";
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
