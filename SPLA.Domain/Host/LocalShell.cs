using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Host;

/// <summary>
/// Passthrough shell: runs commands through PowerShell on the host, exactly as the shell tool did
/// before the seam existed. On a server this is replaced by an OS-isolated shell — or by
/// <c>null</c> in <see cref="ISandbox.Shell"/> to forbid execution entirely.
/// </summary>
public sealed class LocalShell : IShell
{
    public async Task<ShellResult> RunAsync(ShellCommand command, CancellationToken ct = default)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding(command.CodePage);
        var encodedScript = Convert.ToBase64String(
            Encoding.Unicode.GetBytes(BuildPowerShellScript(command.Command, command.CodePage)));

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encodedScript}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = encoding,
            StandardErrorEncoding = encoding,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = string.IsNullOrEmpty(command.WorkingDirectory)
                ? Directory.GetCurrentDirectory()
                : command.WorkingDirectory
        };
        psi.Environment["PYTHONIOENCODING"] = encoding.WebName;
        psi.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Could not start process.");

        var outputTask = process.StandardOutput.ReadToEndAsync(ct);
        var errorTask = process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        return new ShellResult(process.ExitCode, await outputTask, await errorTask);
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
