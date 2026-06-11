using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
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
    public string Name => "dotnet_build";

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
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    projectPath = new
                    {
                        type = "string",
                        description = "The path to the project or solution file to build. If omitted, builds the project in the current directory."
                    },
                    configuration = new
                    {
                        type = "string",
                        description = "The configuration to use for building (e.g., 'Debug' or 'Release'). Defaults to 'Debug'."
                    },
                    noRestore = new
                    {
                        type = "boolean",
                        description = "If true, does not execute an implicit restore during build."
                    },
                    cwd = new
                    {
                        type = "string",
                        description = "Current working directory for the command (optional)"
                    }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var doc = JsonDocument.Parse(argumentsJson);
            
            var projectPath = doc.RootElement.TryGetProperty("projectPath", out var ppElement) ? ppElement.GetString() : null;
            var configuration = doc.RootElement.TryGetProperty("configuration", out var confElement) ? confElement.GetString() : "Debug";
            var noRestore = doc.RootElement.TryGetProperty("noRestore", out var nrElement) && nrElement.GetBoolean();
            var cwd = doc.RootElement.TryGetProperty("cwd", out var cwdElement) ? cwdElement.GetString() : null;

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
