using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace SPLA.Mcp.Client;

/// <summary>
/// MCP's "streamable HTTP": one POST per outgoing frame, and a reply that is either a single JSON
/// body or an SSE stream of frames. The mirror of what <c>SplaServiceHost</c> already serves at
/// <c>POST /mcp</c> — which makes SPLA↔SPLA the one end-to-end bench that needs nothing installed.
///
/// <para><b>What this transport cannot see, and it is worth naming.</b> A server may only reach us
/// while we are holding a response open — i.e. during our own call. The spec also allows a standalone
/// <c>GET</c> that leaves an SSE stream open for server-initiated traffic; wave one does not open
/// one. The practical loss is <c>notifications/tools/list_changed</c> arriving between calls rather
/// than never: over stdio we would hear it immediately, here we hear it on the next call or not at
/// all. Acceptable while nothing depends on the difference, and the place to fix it is here.</para>
/// </summary>
public sealed class HttpTransport(McpServerSpec spec, ILogger? logger = null, HttpClient? client = null)
    : IMcpTransport
{
    private readonly HttpClient _http = client ?? new HttpClient();
    private readonly bool _ownsClient = client is null;
    private readonly CancellationTokenSource _stopping = new();

    /// <summary>The session id a server hands out on <c>initialize</c> and then expects back on every
    /// later request. Absent for servers that do not use one, which is why it is never required.</summary>
    private string? _sessionId;

    /// <summary>The version the server said it would speak, echoed on later requests as the spec
    /// requires. Set once, from the handshake reply.</summary>
    private string? _negotiatedVersion;

    private int _closedRaised;

    public event Action<JsonNode>? FrameReceived;
    public event Action<Exception?>? Closed;

    public string Describe() => $"{spec.Id} (http: {spec.Url})";

    public Task StartAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(spec.Url))
            throw new InvalidOperationException($"MCP server '{spec.Id}' is http but declares no url.");
        if (!Uri.TryCreate(spec.Url, UriKind.Absolute, out _))
            throw new InvalidOperationException($"MCP server '{spec.Id}' has an unusable url: {spec.Url}");

        // Nothing to open: HTTP has no connection to hold. Reachability is discovered by the
        // handshake, which is the first thing the session sends — reporting it here as well would
        // mean two different errors for one condition.
        return Task.CompletedTask;
    }

    public async Task SendAsync(JsonNode frame, CancellationToken ct = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _stopping.Token);
        var token = linked.Token;

        using var request = new HttpRequestMessage(HttpMethod.Post, spec.Url)
        {
            Content = new StringContent(frame.ToJsonString(), Encoding.UTF8, "application/json")
        };

        // Both are offered because either is a valid reply and the server picks: a fast tool answers
        // with a JSON body, a slow one streams progress. A client that offered only one would either
        // lose progress or fail against servers that never stream.
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        if (_sessionId is not null) request.Headers.TryAddWithoutValidation("Mcp-Session-Id", _sessionId);
        if (_negotiatedVersion is not null)
            request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", _negotiatedVersion);
        foreach (var (key, value) in spec.Headers) request.Headers.TryAddWithoutValidation(key, value);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            RaiseClosed(ex);
            throw;
        }

        using (response)
        {
            if (response.Headers.TryGetValues("Mcp-Session-Id", out var ids))
                _sessionId ??= ids.FirstOrDefault();

            if (!response.IsSuccessStatusCode)
            {
                var body = await SafeReadAsync(response, token);
                throw new IOException(
                    $"MCP server '{spec.Id}' answered {(int)response.StatusCode} {response.ReasonPhrase}. {body}");
            }

            // A notification gets 202 with no body, and that is the whole exchange — there is nothing
            // to correlate and nothing to hand upward.
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted) return;

            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (string.Equals(mediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase))
                await ReadEventStreamAsync(response, token);
            else
                await ReadSingleFrameAsync(response, token);
        }
    }

    private async Task ReadSingleFrameAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body)) return;
        Deliver(body);
    }

    /// <summary>Reads <c>data:</c> events as they arrive and hands each frame up immediately. Not
    /// buffered to the end of the stream on purpose: buffering would turn live progress back into one
    /// lump at the finish, which is the entire thing SSE is here for.</summary>
    private async Task ReadEventStreamAsync(HttpResponseMessage response, CancellationToken ct)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var data = new StringBuilder();

        while (!ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is null) break;                       // stream ended

            if (line.Length == 0)
            {
                // A blank line terminates an event. Multi-line data fields are joined with newlines,
                // per the SSE spec — a JSON frame pretty-printed across lines is legal and arrives
                // this way.
                if (data.Length > 0)
                {
                    Deliver(data.ToString());
                    data.Clear();
                }
                continue;
            }

            if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                if (data.Length > 0) data.Append('\n');
                data.Append(line.AsSpan(5).TrimStart());
            }
            // Every other field (event:, id:, retry:, comments) carries nothing we act on.
        }

        if (data.Length > 0) Deliver(data.ToString());
    }

    private void Deliver(string json)
    {
        JsonNode? frame;
        try
        {
            frame = JsonNode.Parse(json);
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger?.LogDebug(ex, "MCP server sent an unparsable frame. Server={ServerId}", spec.Id);
            return;
        }

        if (frame is null) return;

        try { FrameReceived?.Invoke(frame); }
        catch (Exception ex) { logger?.LogError(ex, "MCP frame handler threw. Server={ServerId}", spec.Id); }
    }

    /// <summary>Records the version the server chose, so later requests can carry it.</summary>
    internal void RememberNegotiatedVersion(string? version) => _negotiatedVersion ??= version;

    private void RaiseClosed(Exception? cause)
    {
        if (Interlocked.Exchange(ref _closedRaised, 1) != 0) return;
        if (cause is not null)
            logger?.LogWarning(cause, "MCP connection failed. Server={ServerId}", spec.Id);
        try { Closed?.Invoke(cause); }
        catch (Exception ex) { logger?.LogError(ex, "MCP Closed handler threw. Server={ServerId}", spec.Id); }
    }

    private static async Task<string> SafeReadAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            return body.Length <= 400 ? body : body[..400] + "…";
        }
        catch { return string.Empty; }
    }

    public async ValueTask DisposeAsync()
    {
        await _stopping.CancelAsync();
        if (_ownsClient) _http.Dispose();
        _stopping.Dispose();
    }
}
