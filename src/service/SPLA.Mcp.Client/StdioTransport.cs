using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace SPLA.Mcp.Client;

/// <summary>
/// A child process speaking JSON-RPC on its stdin/stdout — what nearly every published MCP server
/// is, started through <c>npx</c> or <c>uvx</c>.
///
/// <para>The line protocol itself is <see cref="StreamTransport"/>'s; this class owns only what a
/// process brings with it, and every one of those is a way a connection dies quietly:</para>
/// <list type="bullet">
///   <item><b>stderr must be drained.</b> Not politeness — a redirected pipe nobody reads fills its
///   buffer and blocks the child on its next write, which presents as a server that mysteriously
///   stopped answering. It is also the only place a server explains why it failed to start.</item>
///   <item><b>UTF-8 without a BOM, explicitly.</b> A BOM on the first line is not valid JSON, and the
///   console default on a non-English Windows box is not UTF-8 at all — either turns every non-ASCII
///   tool description into a parse error or mojibake.</item>
///   <item><b>Kill the tree, not the process.</b> <c>npx</c> is a launcher: killing it leaves the
///   node process it spawned alive, holding whatever it held, until the machine reboots.</item>
/// </list>
/// </summary>
public sealed class StdioTransport(McpServerSpec spec, ILogger? logger = null) : IMcpTransport
{
    private readonly CancellationTokenSource _stopping = new();

    private Process? _process;
    private StreamTransport? _frames;
    private Task? _errorLoop;

    public event Action<JsonNode>? FrameReceived;
    public event Action<Exception?>? Closed;

    public string Describe() => $"{spec.Id} (stdio: {spec.Command} {string.Join(' ', spec.Args)})";

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(spec.Command))
            throw new InvalidOperationException($"MCP server '{spec.Id}' is stdio but declares no command.");

        var startInfo = new ProcessStartInfo
        {
            FileName = spec.Command,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = new UTF8Encoding(false)
        };

        foreach (var arg in spec.Args) startInfo.ArgumentList.Add(arg);
        if (!string.IsNullOrWhiteSpace(spec.WorkingDirectory))
            startInfo.WorkingDirectory = spec.WorkingDirectory;
        foreach (var (key, value) in spec.Env) startInfo.Environment[key] = value;

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.Exited += (_, _) => OnProcessExited(process);

        try
        {
            if (!process.Start())
                throw new IOException($"Could not start MCP server '{spec.Id}': {spec.Command}");
        }
        catch (Exception ex) when (ex is not IOException)
        {
            process.Dispose();
            throw new IOException($"Could not start MCP server '{spec.Id}': {spec.Command}", ex);
        }

        // The child's stdin is left as the process gave it: a StreamWriter with its own encoding,
        // which .NET already set from StartInfo. Wrapping it again would double-encode.
        var frames = new StreamTransport(
            process.StandardOutput, process.StandardInput, Describe(), logger);
        frames.FrameReceived += frame => FrameReceived?.Invoke(frame);
        frames.Closed += OnFramesClosed;

        _process = process;
        _frames = frames;
        _errorLoop = Task.Run(() => DrainStderrAsync(process), CancellationToken.None);

        await frames.StartAsync(ct);

        logger?.LogInformation("MCP server started. Server={ServerId} Command={Command}", spec.Id, spec.Command);
    }

    public Task SendAsync(JsonNode frame, CancellationToken ct = default) =>
        (_frames ?? throw new InvalidOperationException("Transport is not started.")).SendAsync(frame, ct);

    private void OnProcessExited(Process process)
    {
        if (_stopping.IsCancellationRequested) return;   // we are the ones who killed it

        // Reported through the framing transport rather than raised here, so that "the process died"
        // and "stdout ended" collapse into one Closed event instead of two. Whichever noticed first
        // is the one that speaks; the other is the same fact seen from a different side.
        _frames?.RaiseClosed(
            new IOException($"MCP server '{spec.Id}' exited with code {ExitCodeOf(process)}."));
    }

    private void OnFramesClosed(Exception? cause)
    {
        if (cause is not null)
            logger?.LogWarning(cause, "MCP connection closed. Server={ServerId}", spec.Id);
        Closed?.Invoke(cause);
    }

    private async Task DrainStderrAsync(Process process)
    {
        try
        {
            while (!_stopping.IsCancellationRequested)
            {
                var line = await process.StandardError.ReadLineAsync(_stopping.Token);
                if (line is null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;
                logger?.LogDebug("[{ServerId}] {Line}", spec.Id, line.Length <= 400 ? line : line[..400] + "…");
            }
        }
        catch (OperationCanceledException) { /* shutting down */ }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "Stopped reading stderr. Server={ServerId}", spec.Id);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _stopping.CancelAsync();

        if (_frames is not null) await _frames.DisposeAsync();

        var process = _process;
        if (process is not null)
        {
            try
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Could not kill MCP server process. Server={ServerId}", spec.Id);
            }
        }

        if (_errorLoop is not null)
            try { await _errorLoop.WaitAsync(TimeSpan.FromSeconds(2)); }
            catch { /* a server wedged unkillable must not hold up everything else's shutdown */ }

        process?.Dispose();
        _stopping.Dispose();
    }

    private static int? ExitCodeOf(Process process)
    {
        try { return process.ExitCode; }
        catch { return null; }
    }
}
