using System.Diagnostics;
using System.Globalization;

namespace SPLA.Plugins.Android.Adb;

internal sealed class AdbRunner(string adbExe, int serverPort)
{
    internal static ProcessStartInfo BuildStartInfo(string executable, int port, string? serial, IReadOnlyList<string> args)
    {
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true };
        if (serial is not null) { start.ArgumentList.Add("-s"); start.ArgumentList.Add(serial); }
        foreach (var arg in args) start.ArgumentList.Add(arg);
        if (port > 0) start.Environment["ANDROID_ADB_SERVER_PORT"] = port.ToString(CultureInfo.InvariantCulture);
        return start;
    }

    public async Task<AdbBinaryResult> RunBinaryAsync(string? serial, IReadOnlyList<string> args, TimeSpan timeout, CancellationToken ct,
        int maximumBytes = 64 * 1024 * 1024, bool allowTruncation = false)
    {
        using var process = new Process { StartInfo = BuildStartInfo(adbExe, serverPort, serial, args) };
        process.Start();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(timeout);
        using var output = new MemoryStream();
        var stderr = ReadTextAsync(process.StandardError, 20000, deadline.Token);
        var stdout = CopyBoundedAsync(process.StandardOutput.BaseStream, output, maximumBytes, deadline.Token);
        try
        {
            await Task.WhenAll(stdout, stderr, process.WaitForExitAsync(deadline.Token));
            var truncated = await stdout;
            if (truncated && !allowTruncation) throw new IOException($"adb binary output exceeds {maximumBytes} bytes.");
            return new(process.ExitCode, output.ToArray(), await stderr, truncated);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            await process.WaitForExitAsync(CancellationToken.None);
            if (ct.IsCancellationRequested) throw;
            throw new TimeoutException($"adb timed out after {timeout.TotalSeconds:0} seconds.");
        }
    }

    public async Task<AdbResult> RunAsync(string? serial, IReadOnlyList<string> args, TimeSpan timeout, CancellationToken ct)
    {
        var result = await RunBinaryAsync(serial, args, timeout, ct, 80000, allowTruncation: true);
        return new(result.ExitCode, System.Text.Encoding.UTF8.GetString(result.StdOut) + (result.Truncated ? "\n[stdout truncated]" : ""), result.StdErr);
    }

    public Process StartLongRunning(string? serial, IReadOnlyList<string> args, Action<string> onOutputLine)
    {
        var process = new Process { StartInfo = BuildStartInfo(adbExe, serverPort, serial, args) };
        process.OutputDataReceived += (_, e) => { if (e.Data is { } line) onOutputLine(line); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is { } line) onOutputLine(line); };
        process.Start(); process.BeginOutputReadLine(); process.BeginErrorReadLine();
        return process;
    }

    public async Task<string> CheckedAsync(string? serial, IReadOnlyList<string> args, CancellationToken ct, int timeoutMs = 30000)
    {
        var result = await RunAsync(serial, args, TimeSpan.FromMilliseconds(timeoutMs), ct);
        if (result.ExitCode != 0) throw new IOException($"adb exit {result.ExitCode}: {result.StdErr} {result.StdOut}");
        return result.StdOut;
    }

    internal static string Quote(string value) => "'" + value.Replace("'", "'\\''") + "'";

    private static async Task<bool> CopyBoundedAsync(Stream input, Stream output, int maximumBytes, CancellationToken ct)
    {
        var buffer = new byte[81920]; var captured = 0; var truncated = false;
        int count;
        while ((count = await input.ReadAsync(buffer, ct)) != 0)
        {
            var keep = Math.Min(count, maximumBytes - captured);
            if (keep > 0) await output.WriteAsync(buffer.AsMemory(0, keep), ct);
            captured += keep; truncated |= keep < count;
        }
        return truncated;
    }
    private static async Task<string> ReadTextAsync(StreamReader reader, int maximumCharacters, CancellationToken ct)
    {
        var text = new System.Text.StringBuilder(); var buffer = new char[4096]; var truncated = false;
        int count;
        while ((count = await reader.ReadAsync(buffer, ct)) != 0)
        {
            var keep = Math.Min(count, maximumCharacters - text.Length);
            text.Append(buffer, 0, keep); truncated |= keep < count;
        }
        return text.ToString() + (truncated ? "\n[stderr truncated]" : "");
    }
}

internal sealed record AdbResult(int ExitCode, string StdOut, string StdErr);
internal sealed record AdbBinaryResult(int ExitCode, byte[] StdOut, string StdErr, bool Truncated = false);
