using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SPLA.Mcp;

/// <summary>
/// Serves SPLA's tools to a foreign head over MCP on stdin/stdout.
/// <para>
/// <b>It is handed a host; it never builds one.</b> That single constraint is what keeps both
/// deployments open: the same server either sits on a runtime the launching process created (its own
/// body — a stdio child, headless, no port) or on one already serving a UI (a shared body reached
/// through a pipe). Whether the body is its own is a question about who called the constructor, not
/// about this class.
/// </para>
/// <para>
/// <b>stdout is the protocol.</b> One JSON object per line and nothing else, ever — a stray
/// <c>Console.WriteLine</c> anywhere in the process corrupts the stream and the connection dies.
/// Diagnostics go to stderr, which the client ignores.
/// </para>
/// <para>
/// <b>A call does not hold the reader.</b> <c>tools/call</c> runs on its own task; everything else is
/// answered inline, being instant. That is not about throughput — a foreign head issues one call at a
/// time. It is that the two things a client may send *during* a long call, a cancellation and a
/// keepalive ping, are unreachable if the loop is sitting inside the call they are about.
/// </para>
/// </summary>
public sealed class McpStdioServer
{
    private const string DefaultProtocolVersion = "2025-06-18";

    private readonly IToolHost _host;
    private readonly Func<IEnumerable<ToolDefinition>> _listTools;
    private readonly AgentMode _mode;
    private readonly ToolCallContext? _context;
    private readonly TextWriter _log;

    /// <summary>Serialises the pipe. Progress notifications are written from tool threads while a call
    /// is still running, so "one JSON object per line" stops being free the moment anything but the
    /// read loop can write.</summary>
    private readonly SemaphoreSlim _writeGate = new(1, 1);

    /// <summary>Calls still running, by request id, so <c>notifications/cancelled</c> has something to
    /// cancel. Keyed on the id's JSON text because JSON-RPC ids may be a string or a number and the
    /// client is entitled to either.</summary>
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _inFlight = new();

    /// <summary>The pipe, for the duration of <see cref="RunAsync"/>.</summary>
    private TextWriter? _output;

    /// <param name="host">Executes the calls. Given, not constructed — see the class remarks.</param>
    /// <param name="listTools">What this caller may be offered. The exposure decision belongs to
    /// whoever hosts this server, not to the protocol.</param>
    /// <param name="mode">The ceiling on what a call may do, fixed for the connection.</param>
    /// <param name="context">Whose calls these are. Null lets the host read the ambient scopes
    /// (Session, Identity) — right for a stdio child sitting on a runtime that already has a chat
    /// open. Either way, <paramref name="source"/> is stamped over whatever <see cref="ToolCallContext.Source"/>
    /// it carries, so an audit log always sees where this transport's calls actually came from.</param>
    /// <param name="log">Diagnostics sink. Must not be stdout.</param>
    /// <param name="source">The label an audit log sees for every call this instance makes — e.g.
    /// the HTTP <c>/mcp</c> endpoint passes <c>"mcp-http &lt;ip&gt;"</c>. Defaults to
    /// <c>"mcp-stdio"</c>, since that is what this class actually is.</param>
    public McpStdioServer(
        IToolHost host,
        Func<IEnumerable<ToolDefinition>> listTools,
        AgentMode mode = AgentMode.Agent,
        ToolCallContext? context = null,
        TextWriter? log = null,
        string source = "mcp-stdio")
    {
        _host = host;
        _listTools = listTools;
        _mode = mode;
        _context = (context ?? ToolCallContext.FromAmbient()) with { Source = source };
        _log = log ?? Console.Error;
    }

    public async Task RunAsync(TextReader input, TextWriter output, CancellationToken ct = default)
    {
        _output = output;
        _log.WriteLine("[spla-mcp] ready");

        // What we still owe the client. Awaited on the way out so a call that was in flight when the
        // pipe closed gets to finish writing rather than being torn off mid-frame.
        var running = new ConcurrentDictionary<Task, byte>();

        while (!ct.IsCancellationRequested)
        {
            var line = await input.ReadLineAsync(ct);
            if (line is null) break;                 // client closed the pipe: we are done
            if (string.IsNullOrWhiteSpace(line)) continue;

            JsonNode? request;
            try
            {
                request = JsonNode.Parse(line);
            }
            catch (JsonException ex)
            {
                _log.WriteLine($"[spla-mcp] unparsable line: {ex.Message}");
                continue;                            // no id to answer with; saying nothing is correct
            }

            if (request is null) continue;

            // The one method that may take minutes goes off the loop; everything else is a lookup and
            // is cheaper to answer here than to schedule.
            if (request["method"]?.GetValue<string>() == "tools/call")
            {
                // Registered here rather than inside the task: a client is entitled to send the
                // cancellation on the very next line, and a call that is not yet findable when it
                // arrives would run to completion having been told to stop.
                var key = request["id"]?.ToJsonString();
                var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                if (key is not null) _inFlight[key] = cts;

                Task? call = null;
                call = Task.Run(async () =>
                {
                    try { await DispatchCallAsync(request, key, cts); }
                    finally { if (call is not null) running.TryRemove(call, out _); }
                }, CancellationToken.None);
                running[call] = 0;
                continue;
            }

            var response = await HandleAsync(request, ct);

            // A notification has no id and must get no reply — answering one is a protocol error,
            // not merely noise.
            if (response is null) continue;

            await SendAsync(response, ct);
        }

        // Cancellation first, then the wait: a call still inside the model or a sub-agent would
        // otherwise hold the shutdown for as long as it felt like.
        foreach (var cts in _inFlight.Values) { try { cts.Cancel(); } catch (ObjectDisposedException) { } }
        try { await Task.WhenAll(running.Keys); } catch { /* each task already reported for itself */ }

        _log.WriteLine("[spla-mcp] stopped");
    }

    /// <summary>
    /// Runs one <c>tools/call</c> to completion and writes its answer. Owns the request's cancellation
    /// source for exactly as long as the call lives, which is what makes
    /// <c>notifications/cancelled</c> mean something.
    /// </summary>
    private async Task DispatchCallAsync(JsonNode request, string? key, CancellationTokenSource cts)
    {
        using var lifetime = cts;
        try
        {
            var response = await HandleAsync(request, cts.Token);
            if (response is not null) await SendAsync(response, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // The client asked for this and, per the spec, must not be answered for a request it has
            // withdrawn. Recorded on stderr so a cancelled call is not indistinguishable from a lost one.
            _log.WriteLine($"[spla-mcp] tools/call {key ?? "(no id)"} cancelled");
        }
        finally
        {
            if (key is not null) _inFlight.TryRemove(key, out _);
        }
    }

    private async Task<JsonNode?> HandleAsync(JsonNode request, CancellationToken ct)
    {
        var method = request["method"]?.GetValue<string>();
        var id = request["id"];

        if (method is null) return null;

        try
        {
            switch (method)
            {
                case "initialize":
                    return Ok(id, Initialize(request));

                case "notifications/initialized":
                    return null;

                case "notifications/cancelled":
                    Cancel(request);
                    return null;

                case "ping":
                    return Ok(id, new JsonObject());

                case "tools/list":
                    return Ok(id, ListTools());

                case "tools/call":
                    return Ok(id, await CallToolAsync(request, ct));

                default:
                    // -32601 is JSON-RPC's "method not found". Answered rather than ignored so a
                    // client probing for an optional capability learns it is absent instead of
                    // waiting for a reply that never comes.
                    return Error(id, -32601, $"Method '{method}' is not supported.");
            }
        }
        catch (OperationCanceledException)
        {
            throw;                                   // withdrawn, not failed — see DispatchCallAsync
        }
        catch (Exception ex)
        {
            _log.WriteLine($"[spla-mcp] {method} failed: {ex}");
            return Error(id, -32603, ex.Message);
        }
    }

    /// <summary>Withdraws a running call. Silent when the id names nothing: a cancellation that lost the
    /// race against completion is normal traffic, not an error worth answering.</summary>
    private void Cancel(JsonNode request)
    {
        var key = request["params"]?["requestId"]?.ToJsonString();
        if (key is null || !_inFlight.TryGetValue(key, out var cts)) return;

        try { cts.Cancel(); } catch (ObjectDisposedException) { }
    }

    /// <summary>One frame out. The only writer to the pipe — see <see cref="_writeGate"/>.</summary>
    private async Task SendAsync(JsonNode frame, CancellationToken ct)
    {
        var output = _output;
        if (output is null) return;

        await _writeGate.WaitAsync(CancellationToken.None);
        try
        {
            await output.WriteLineAsync(frame.ToJsonString());
            await output.FlushAsync(ct);
        }
        catch (Exception ex)
        {
            // A dead pipe is the normal end of a session; it must not surface as a tool failure on a
            // thread that has nothing to do with the client going away.
            _log.WriteLine($"[spla-mcp] write failed: {ex.Message}");
        }
        finally
        {
            _writeGate.Release();
        }
    }

    /// <summary>
    /// The handshake. The client's protocol version is echoed back when it sent one: this subset —
    /// listing tools and calling them — has not changed across the revisions in circulation, so
    /// insisting on our own number would refuse a client we can in fact serve.
    /// </summary>
    private JsonObject Initialize(JsonNode request)
    {
        var asked = request["params"]?["protocolVersion"]?.GetValue<string>();
        var client = request["params"]?["clientInfo"]?["name"]?.GetValue<string>() ?? "unknown";
        _log.WriteLine($"[spla-mcp] initialize from {client} (protocol {asked ?? "unstated"})");

        return new JsonObject
        {
            ["protocolVersion"] = asked ?? DefaultProtocolVersion,
            ["capabilities"] = new JsonObject { ["tools"] = new JsonObject() },
            ["serverInfo"] = new JsonObject
            {
                ["name"] = "spla",
                ["version"] = typeof(McpStdioServer).Assembly.GetName().Version?.ToString() ?? "0.0.0"
            }
        };
    }

    private JsonObject ListTools()
    {
        var tools = new JsonArray();

        foreach (var def in _listTools())
        {
            var fn = def.Function;
            tools.Add(new JsonObject
            {
                ["name"] = fn.Name,
                ["description"] = fn.Description,
                // The schema travels verbatim: it is already JSON-shaped for the model providers,
                // and MCP wants the same thing under a different name.
                ["inputSchema"] = fn.Parameters is null
                    ? new JsonObject { ["type"] = "object" }
                    : JsonNode.Parse(JsonSerializer.Serialize(fn.Parameters))
            });
        }

        return new JsonObject { ["tools"] = tools };
    }

    private async Task<JsonObject> CallToolAsync(JsonNode request, CancellationToken ct)
    {
        var name = request["params"]?["name"]?.GetValue<string>()
                   ?? throw new ArgumentException("tools/call requires 'name'.");

        var args = request["params"]?["arguments"]?.ToJsonString() ?? "{}";

        // Callable is exactly listable, checked here and not only when the catalogue is built.
        // Filtering the listing alone is decoration: a caller that names a tool it was never offered
        // would still reach it, and a foreign head has no trouble naming one — the names are in its
        // own history. Answered as "not found" rather than "not for you", because to this caller
        // that is the truth: nothing it can address is behind the refusal.
        if (!_listTools().Any(d => string.Equals(d.Function.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            _log.WriteLine($"[spla-mcp] refused unlisted tool: {name}");
            return new JsonObject
            {
                ["content"] = new JsonArray
                {
                    new JsonObject { ["type"] = "text", ["text"] = $"Error: Tool '{name}' not found." }
                },
                ["isError"] = true
            };
        }

        // Progress is opt-in, per call, by the client: a token in _meta means "tell me how this is
        // going". Without one nothing is opened and the tool's ticks fall on the floor exactly as
        // before — which is the correct answer to a client that never asked and would not know what
        // to do with the frames.
        //
        // Nothing below this line is tool-specific, and that is the point. Every tool in the project
        // already reports through ProgressScope, and every stage of the pipeline already opens a node
        // per call at any depth; the only thing missing over MCP was a tree for them to land in,
        // because the tree is normally opened by the agent loop and there is no agent loop here.
        var token = request["params"]?["_meta"]?["progressToken"];

        if (token is null)
        {
            var plain = await _host.ExecuteToolAsync(_mode, name, args, ct, _context);
            return Answer(plain);
        }

        var tree = new ProgressTree();

        // CancellationToken.None deliberately: a withdrawn call still gets its frames written rather
        // than torn off half-way, and the pipe is shared — a partial line ends the session for
        // everything, not just for this call.
        var reporter = new McpProgressReporter(tree, token, frame => SendAsync(frame, CancellationToken.None));
        try
        {
            using var treeScope = ProgressScope.BeginTree(tree);
            var result = await _host.ExecuteToolAsync(_mode, name, args, ct, _context);
            return Answer(result);
        }
        finally
        {
            // Before the answer, always: progress that arrived after the result it was about would be
            // a puzzle for the client and a lie about the order things happened in.
            await reporter.FinishAsync();
        }
    }

    /// <summary>The result frame. MCP carries a flag where SPLA carries three outcomes: refused and
    /// failed both fold into it, because the distinction matters to an audit log, not to the model
    /// reading the answer.</summary>
    private static JsonObject Answer(ToolResult result) => new()
    {
        ["content"] = Project(result),
        ["isError"] = result.IsError
    };

    /// <summary>Content blocks, one for one. The list already exists on our side — this only renames
    /// the fields.</summary>
    private static JsonArray Project(ToolResult result)
    {
        var content = new JsonArray();

        foreach (var block in result.Content)
        {
            switch (block)
            {
                case ToolText text:
                    content.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    break;

                case ToolImage image:
                    content.Add(new JsonObject
                    {
                        ["type"] = "image",
                        ["data"] = image.Data,
                        ["mimeType"] = image.MimeType
                    });
                    break;

                case ToolResource resource:
                    content.Add(new JsonObject
                    {
                        ["type"] = "resource",
                        ["resource"] = new JsonObject
                        {
                            ["uri"] = resource.Uri,
                            ["mimeType"] = resource.MimeType,
                            ["text"] = resource.Description
                        }
                    });
                    break;
            }
        }

        // A result with nothing to say still needs a block: clients treat an empty content array as
        // a malformed answer rather than as silence.
        if (content.Count == 0)
            content.Add(new JsonObject { ["type"] = "text", ["text"] = "" });

        return content;
    }

    private static JsonObject Ok(JsonNode? id, JsonNode payload) => new()
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id?.DeepClone(),
        ["result"] = payload
    };

    private static JsonObject Error(JsonNode? id, int code, string message) => new()
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id?.DeepClone(),
        ["error"] = new JsonObject { ["code"] = code, ["message"] = message }
    };
}
