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

public class RunCommandTool : IMcpTool
{
    public string Name => "sys.shell";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Executes a shell command on the host system.",
            Scope = ToolScope.Shell,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.High,
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
                        type = "string",
                        description = "Current working directory for the command (optional)"
                    },
                    codepage = new
                    {
                        type = "integer",
                        description = "Windows console code page for native command output. Defaults to 65001 (UTF-8)."
                    }
                },
                required = new[] { "command" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var doc = JsonDocument.Parse(argumentsJson);
            if (doc.RootElement.TryGetProperty("command", out var cmdElement))
            {
                var cmd = cmdElement.GetString();
                var cwd = doc.RootElement.TryGetProperty("cwd", out var cwdElement) ? cwdElement.GetString() : null;
                var codePage = doc.RootElement.TryGetProperty("codepage", out var codePageElement) && codePageElement.TryGetInt32(out var parsedCodePage)
                    ? parsedCodePage
                    : 65001;

                if (string.IsNullOrEmpty(cmd)) return "Error: command is empty.";
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

                return $"ExitCode: {process.ExitCode}\nCodePage: {codePage}\nOutput:\n{output}\nError:\n{error}";
            }
            return "Error: Missing 'command' parameter.";
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
