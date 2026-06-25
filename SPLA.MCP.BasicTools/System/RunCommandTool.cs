using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var doc = JsonDocument.Parse(argumentsJson);
            var cmd      = ToolJson.GetString(doc.RootElement, "command");
            var cwd      = ToolJson.GetString(doc.RootElement, "cwd");
            var codePage = ToolJson.GetInt32(doc.RootElement, "code_page", 65001);

            if (string.IsNullOrEmpty(cmd)) return "Error: Missing 'command' parameter.";
            {
                var encoding = Encoding.GetEncoding(codePage);
                var encodedScript = Convert.ToBase64String(Encoding.Unicode.GetBytes(BuildPowerShellScript(cmd, codePage)));

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encodedScript}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = encoding,
                    StandardErrorEncoding = encoding,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = string.IsNullOrEmpty(cwd) ? Directory.GetCurrentDirectory() : cwd
                };
                processStartInfo.Environment["PYTHONIOENCODING"] = encoding.WebName;
                processStartInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";

                using var process = Process.Start(processStartInfo);
                if (process == null) return "Error: Could not start process.";

                var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken);

                var output = await outputTask;
                var error = await errorTask;

                var result = $"ExitCode: {process.ExitCode}\nCodePage: {codePage}\nOutput:\n{output}\nError:\n{error}";
                var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
                if (target == OutputTarget.Context) return result;
                var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
                return DataChannel.Route(target, BlobPayload.OfText(result), $"system_run_shell: exit={process.ExitCode}, {output.Length} chars output", blobName);
            }
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

    private static string BuildPowerShellScript(string command, int codePage)
    {
        var escapedCommand = command.Replace("'", "''");
        var script = new StringBuilder();
        script.AppendLine("try {");
        script.AppendLine($"    [Console]::InputEncoding = [System.Text.Encoding]::GetEncoding({codePage})");
        script.AppendLine($"    [Console]::OutputEncoding = [System.Text.Encoding]::GetEncoding({codePage})");
        script.AppendLine($"    $OutputEncoding = [System.Text.Encoding]::GetEncoding({codePage})");
        script.AppendLine("    if ($IsWindows -or $env:OS -eq 'Windows_NT') {");
        script.AppendLine($"        chcp.com {codePage} > $null");
        script.AppendLine("    }");
        script.AppendLine($"    Invoke-Expression '{escapedCommand}'");
        script.AppendLine("    exit $LASTEXITCODE");
        script.AppendLine("} catch {");
        script.AppendLine("    [Console]::Error.WriteLine($_.Exception.Message)");
        script.AppendLine("    exit 1");
        script.AppendLine("}");
        return script.ToString();
    }
}
