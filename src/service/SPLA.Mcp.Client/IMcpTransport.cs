using System.Text.Json.Nodes;

namespace SPLA.Mcp.Client;

/// <summary>
/// A full-duplex channel of JSON-RPC frames to one server.
///
/// <para><b>Not request/response, on purpose.</b> A server may send a notification (progress, "my
/// tool list changed") at any moment, and may send a <i>request</i> of its own — which is exactly
/// what <c>elicitation</c> and <c>sampling</c> are. A transport shaped as "send a request, await its
/// reply" has nowhere to put any of that, and the shape would have to be broken open again the day
/// the second wave arrives. So the transport carries frames in both directions and matches nothing;
/// correlating a reply with its request is <see cref="McpServerSession"/>'s job.</para>
/// </summary>
public interface IMcpTransport : IAsyncDisposable
{
    /// <summary>Opens the channel — starts the child process, or prepares the HTTP client. Throws if
    /// the server cannot be reached at all; everything after that is reported through
    /// <see cref="Closed"/>.</summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>Writes one frame. May be called from any thread; implementations serialise.</summary>
    Task SendAsync(JsonNode frame, CancellationToken ct = default);

    /// <summary>Every frame that arrived from the server: replies, notifications and the server's own
    /// requests alike, undifferentiated. Raised on a background thread.</summary>
    event Action<JsonNode>? FrameReceived;

    /// <summary>The channel is gone — the child exited, the stream ended, the endpoint stopped
    /// answering. The argument is the cause where there is one, null for an orderly end. Anything
    /// still waiting for a reply has to be failed by whoever is waiting; the transport does not know
    /// about pending calls.</summary>
    event Action<Exception?>? Closed;

    /// <summary>What to call this connection in a log line.</summary>
    string Describe();
}
