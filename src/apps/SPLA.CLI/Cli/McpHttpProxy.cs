using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace SPLA.CLI;

/// <summary>
/// Bridges stdio ⟷ the service's <c>POST /mcp</c> endpoint (see
/// <c>SplaServiceHost.HandleMcpAsync</c>), so <c>spla mcp</c> can stay an ordinary stdio MCP server to
/// whoever launched it while the tools it offers actually run on a runtime it does not own — the join
/// half of join-or-start (see <see cref="McpCommand"/>).
///
/// <para><b>The shape of one exchange.</b> <c>/mcp</c> takes exactly one JSON-RPC line per request and
/// answers with either a single JSON line (no progress asked for, or the request was a notification —
/// then there is nothing to answer with, and the endpoint returns 204), or, when the request carries
/// both an <c>id</c> and a <c>_meta.progressToken</c>, a <c>text/event-stream</c> response whose
/// <c>data:</c> frames are progress notifications followed by the final reply, in order. Both shapes
/// are already exactly one JSON-RPC object each — the only work this proxy does is choosing which shape
/// to ask for and copying whatever comes back onto stdout unchanged, one line per frame.</para>
///
/// <para><b>Why requests are not serialised against each other.</b> <c>McpStdioServer</c> (the in-
/// process server this proxy stands in for) runs every <c>tools/call</c> on its own task specifically so
/// a long call does not block <c>tools/list</c> or a keepalive <c>ping</c> arriving behind it on the same
/// pipe. Over HTTP the same property falls out for free — each request is an independent POST — so this
/// proxy fires every line at the endpoint as soon as it is read and only serialises the one thing that
/// still needs it: writes to the single shared stdout.</para>
///
/// <para><b>Known gap: <c>notifications/cancelled</c> does not actually cancel anything over this
/// transport.</b> <c>HandleMcpAsync</c> builds a fresh <c>McpStdioServer</c> (and therefore a fresh,
/// empty in-flight-call table) for every single POST, because each request is independent and the
/// endpoint keeps no session state between them. A cancellation notification forwarded by this proxy
/// arrives as its own unrelated POST and finds nothing to cancel — the call it names is a different
/// request already running its own <c>McpStdioServer</c> instance server-side. This is a property of
/// <c>/mcp</c> itself, not something introduced here, and fixing it is out of scope for this layer (see
/// <see cref="McpCommand"/>'s remarks on the separately-owned gap around the endpoint not holding the
/// writer lease between calls either).</para>
/// </summary>
public sealed class McpHttpProxy
{
    private readonly string _mcpUrl;
    private readonly HttpClient _http;

    public McpHttpProxy(string baseUrl)
    {
        _mcpUrl = baseUrl.TrimEnd('/') + "/mcp";
        // No client-side timeout: a tools/call can legitimately run for minutes (a sub-agent, a long
        // shell command), and the HTTP request for it is held open by the server for exactly that long
        // — see HandleMcpAsync, which awaits the call to completion before answering. Cancellation is
        // stdin closing, handled by the caller's CancellationToken instead.
        _http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
    }

    /// <summary>
    /// Checks whether the instance at <paramref name="baseUrl"/> actually maps <c>POST /mcp</c> before
    /// committing to it.
    ///
    /// <para><b>Why this has to be asked, not assumed.</b> <c>McpEnabled</c> is a project setting
    /// (<c>mcp.enabled</c>), off by default, and it travels with whichever <c>spla serve</c> published
    /// the lock file we might join — not with us. An instance somebody else started for an ordinary
    /// desktop session, or with an older config, may simply not have the route mapped, and the failure
    /// mode for POSTing to an unmapped route is a plain ASP.NET 404. An empty body is used as the probe
    /// because it is the cheapest request the endpoint can receive: a mapped route answers it with 400
    /// (<c>HandleMcpAsync</c>'s own <c>string.IsNullOrWhiteSpace(line) → BadRequest</c>), which this
    /// probe treats identically to any other non-404 — the only distinction that matters is "mapped or
    /// not".</para>
    /// </summary>
    public static async Task<bool> SupportsMcpAsync(string baseUrl, CancellationToken ct)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            using var response = await http.PostAsync(
                baseUrl.TrimEnd('/') + "/mcp", new StringContent(string.Empty), ct);
            return response.StatusCode != HttpStatusCode.NotFound;
        }
        catch
        {
            // Unreachable is not "unsupported" — the caller's own subsequent request will fail loudly
            // and specifically, instead of this probe manufacturing a misleading "not supported".
            return true;
        }
    }

    public async Task RunAsync(TextReader input, TextWriter output, CancellationToken ct)
    {
        var writeGate = new SemaphoreSlim(1, 1);

        // Mirrors McpStdioServer's own `running` bookkeeping: what we still owe the client, awaited on
        // the way out so a call in flight when stdin closes gets to finish writing rather than being
        // torn off mid-frame.
        var running = new ConcurrentDictionary<Task, byte>();

        while (!ct.IsCancellationRequested)
        {
            var line = await input.ReadLineAsync(ct);
            if (line is null) break;                 // client closed the pipe: we are done
            if (string.IsNullOrWhiteSpace(line)) continue;

            Task? call = null;
            call = Task.Run(async () =>
            {
                try { await ForwardAsync(line, output, writeGate, ct); }
                finally { if (call is not null) running.TryRemove(call, out _); }
            }, CancellationToken.None);
            running[call] = 0;
        }

        try { await Task.WhenAll(running.Keys); } catch { /* each request already reported for itself */ }
    }

    private async Task ForwardAsync(string line, TextWriter output, SemaphoreSlim writeGate, CancellationToken ct)
    {
        try
        {
            var wantsSse = HasProgressToken(line);

            using var request = new HttpRequestMessage(HttpMethod.Post, _mcpUrl)
            {
                Content = new StringContent(line, Encoding.UTF8, "application/json")
            };
            if (wantsSse) request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            var isSse = response.Content.Headers.ContentType?.MediaType == "text/event-stream";
            if (isSse)
            {
                await RelaySseAsync(response, output, writeGate, ct);
                return;
            }

            // 204: the request was a notification (no id) — /mcp answers it with no content, and a
            // notification gets no reply on stdio either, so there is nothing to write.
            if (response.StatusCode == HttpStatusCode.NoContent) return;

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!string.IsNullOrWhiteSpace(body)) await WriteLineAsync(output, writeGate, body, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Shutting down — not a call failure worth reporting.
        }
        catch (Exception ex)
        {
            // A dead connection or a fault on the server side must not corrupt stdout with anything
            // that is not a JSON-RPC frame — the same rule McpStdioServer follows for its own pipe.
            // Diagnostics go to stderr; the caller on the other end of stdin simply never sees a reply
            // to this one request, exactly as if the call itself had thrown.
            Console.Error.WriteLine($"[spla-mcp] proxied request failed: {ex.Message}");
        }
    }

    /// <summary>Relays an SSE response frame by frame. Every <c>data:</c> line is already one complete
    /// JSON-RPC object — progress notifications and the final reply alike, in the order
    /// <c>McpStdioServer</c> produced them server-side — so this needs no knowledge of which one is "the"
    /// answer, unlike the server's own <c>SseWriter</c> (which has to know when to stop writing headers).
    /// Here the stream simply ends when the HTTP response body ends.</summary>
    private static async Task RelaySseAsync(
        HttpResponseMessage response, TextWriter output, SemaphoreSlim writeGate, CancellationToken ct)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (true)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line is null) break;
            // Blank lines separate SSE events; anything that is not a `data:` field (an `event:` line,
            // a comment) carries nothing this protocol uses.
            if (!line.StartsWith("data:", StringComparison.Ordinal)) continue;

            var payload = line.Length > 5 ? line[5..].TrimStart() : string.Empty;
            if (payload.Length == 0) continue;

            await WriteLineAsync(output, writeGate, payload, ct);
        }
    }

    private static async Task WriteLineAsync(TextWriter output, SemaphoreSlim gate, string line, CancellationToken ct)
    {
        await gate.WaitAsync(ct);
        try
        {
            await output.WriteLineAsync(line);
            await output.FlushAsync(ct);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>Whether this request should ask for the SSE shape — mirrors the same test
    /// <c>HandleMcpAsync</c> applies server-side (an id, and <c>_meta.progressToken</c>), so a request
    /// that would get progress frames over stdio gets them here too.</summary>
    private static bool HasProgressToken(string line)
    {
        try
        {
            var node = JsonNode.Parse(line);
            return node?["id"] is not null && node["params"]?["_meta"]?["progressToken"] is not null;
        }
        catch
        {
            return false;
        }
    }
}
