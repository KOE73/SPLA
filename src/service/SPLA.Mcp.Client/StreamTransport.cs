using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace SPLA.Mcp.Client;

/// <summary>
/// The framing half of a line-oriented MCP connection: one JSON object per line, read from a
/// <see cref="TextReader"/> and written to a <see cref="TextWriter"/>. It owns neither.
///
/// <para>Split out from <see cref="StdioTransport"/> because a child process and a line protocol are
/// two different concerns, and only one of them needs an operating system to test. Over a pair of
/// in-memory pipes this same class puts our client and our own <c>McpStdioServer</c> at opposite ends
/// of a conversation with nothing spawned and no port opened — which is the cheapest honest
/// end-to-end test this feature can have.</para>
/// </summary>
public sealed class StreamTransport(
    TextReader input,
    TextWriter output,
    string description = "stream",
    ILogger? logger = null) : IMcpTransport
{
    /// <summary>Serialises writes. Callers send from any thread, and "one JSON object per line" stops
    /// being free the moment two of them interleave.</summary>
    private readonly SemaphoreSlim _writeGate = new(1, 1);

    private readonly CancellationTokenSource _stopping = new();
    private Task? _readLoop;
    private int _closedRaised;

    public event Action<JsonNode>? FrameReceived;
    public event Action<Exception?>? Closed;

    public string Describe() => description;

    public Task StartAsync(CancellationToken ct = default)
    {
        _readLoop = Task.Run(ReadLoopAsync, CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task SendAsync(JsonNode frame, CancellationToken ct = default)
    {
        await _writeGate.WaitAsync(ct);
        try
        {
            await output.WriteLineAsync(frame.ToJsonString().AsMemory(), ct);
            await output.FlushAsync(ct);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private async Task ReadLoopAsync()
    {
        try
        {
            while (!_stopping.IsCancellationRequested)
            {
                var line = await input.ReadLineAsync(_stopping.Token);
                if (line is null) break;                      // the other end closed
                if (string.IsNullOrWhiteSpace(line)) continue;

                JsonNode? frame;
                try
                {
                    frame = JsonNode.Parse(line);
                }
                catch (System.Text.Json.JsonException)
                {
                    // A banner, a warning, a progress bar written to the wrong stream. Common enough
                    // among real servers that refusing to continue would rule out working ones, and
                    // there is no id to answer with in any case.
                    logger?.LogDebug("Non-JSON line on an MCP connection. Where={Where} Line={Line}",
                        description, line.Length <= 400 ? line : line[..400] + "…");
                    continue;
                }

                if (frame is null) continue;

                try { FrameReceived?.Invoke(frame); }
                catch (Exception ex)
                {
                    // A handler that throws must not take the reader down with it: the connection
                    // would die on one bad frame and every pending call would hang.
                    logger?.LogError(ex, "MCP frame handler threw. Where={Where}", description);
                }
            }
        }
        catch (OperationCanceledException) { /* shutting down */ }
        catch (Exception ex)
        {
            RaiseClosed(ex);
            return;
        }

        RaiseClosed(null);
    }

    internal void RaiseClosed(Exception? cause)
    {
        if (Interlocked.Exchange(ref _closedRaised, 1) != 0) return;
        try { Closed?.Invoke(cause); }
        catch (Exception ex) { logger?.LogError(ex, "MCP Closed handler threw. Where={Where}", description); }
    }

    public async ValueTask DisposeAsync()
    {
        await _stopping.CancelAsync();

        if (_readLoop is not null)
            try { await _readLoop.WaitAsync(TimeSpan.FromSeconds(2)); }
            catch { /* a reader blocked on a stream nobody will close is not worth waiting on */ }

        _stopping.Dispose();
        _writeGate.Dispose();
    }
}
