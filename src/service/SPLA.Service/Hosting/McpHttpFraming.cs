namespace SPLA.Service;

/// <summary>
/// The plumbing that lets <c>McpStdioServer</c> — built for a line-in/line-out pipe — answer a single
/// HTTP POST instead. Shared by every route that relays one JSON-RPC line and waits for the matching
/// reply: an instance's own <c>POST /mcp</c> (<see cref="SplaServiceHost"/>) and the hub's
/// <c>POST /hub/mcp</c> (<see cref="RegistryEndpoints"/>) alike, so the framing is written once instead
/// of drifting between the two.
/// </summary>
internal static class McpHttpFraming
{
    /// <summary>True when <paramref name="jsonLine"/> is the reply to the request that carried
    /// <paramref name="requestId"/> — as opposed to a <c>notifications/progress</c> frame, which has no
    /// <c>id</c> at all.</summary>
    public static bool IsFinalResponse(string? jsonLine, System.Text.Json.Nodes.JsonNode? requestId)
    {
        if (string.IsNullOrEmpty(jsonLine)) return false;
        try
        {
            var id = System.Text.Json.Nodes.JsonNode.Parse(jsonLine)?["id"];
            return requestId is null
                ? id is null
                : System.Text.Json.Nodes.JsonNode.DeepEquals(id, requestId);
        }
        catch (System.Text.Json.JsonException) { return false; }
    }

    /// <summary>Yields one line, then blocks — never hands <c>McpStdioServer.RunAsync</c> an EOF until
    /// <see cref="SignalEof"/> is called, so a call still writing progress frames is not torn off.</summary>
    public sealed class PendingLineReader : TextReader
    {
        private readonly string _line;
        private bool _sent;
        private readonly TaskCompletionSource _eof = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public PendingLineReader(string line) => _line = line;

        public void SignalEof() => _eof.TrySetResult();

        public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            if (!_sent) { _sent = true; return _line; }
            await _eof.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }
    }

    /// <summary>Captures the reply matching the request's own id and nothing else — a progress
    /// notification arriving on this path is silently dropped rather than mistaken for the answer.
    /// <see cref="ResponseWritten"/> completes the moment the real reply is written, which is the
    /// signal the caller waits on before it lets the reader see EOF.</summary>
    public sealed class CapturingWriter : TextWriter
    {
        private readonly System.Text.Json.Nodes.JsonNode? _requestId;
        private readonly TaskCompletionSource<string?> _written =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CapturingWriter(System.Text.Json.Nodes.JsonNode? requestId) => _requestId = requestId;

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public Task<string?> ResponseWritten => _written.Task;

        public override Task WriteLineAsync(string? value)
        {
            if (IsFinalResponse(value, _requestId)) _written.TrySetResult(value);
            return Task.CompletedTask;
        }

        public override Task FlushAsync() => Task.CompletedTask;
    }
}
