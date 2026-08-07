using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
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
/// </summary>
public sealed class McpStdioServer
{
    private const string DefaultProtocolVersion = "2025-06-18";

    private readonly IToolHost _host;
    private readonly Func<IEnumerable<ToolDefinition>> _listTools;
    private readonly AgentMode _mode;
    private readonly ToolCallContext? _context;
    private readonly TextWriter _log;

    /// <param name="host">Executes the calls. Given, not constructed — see the class remarks.</param>
    /// <param name="listTools">What this caller may be offered. The exposure decision belongs to
    /// whoever hosts this server, not to the protocol.</param>
    /// <param name="mode">The ceiling on what a call may do, fixed for the connection.</param>
    /// <param name="context">Whose calls these are. Null lets the host read the ambient scopes,
    /// which is right for a stdio child that is the only thing running.</param>
    /// <param name="log">Diagnostics sink. Must not be stdout.</param>
    public McpStdioServer(
        IToolHost host,
        Func<IEnumerable<ToolDefinition>> listTools,
        AgentMode mode = AgentMode.Agent,
        ToolCallContext? context = null,
        TextWriter? log = null)
    {
        _host = host;
        _listTools = listTools;
        _mode = mode;
        _context = context;
        _log = log ?? Console.Error;
    }

    public async Task RunAsync(TextReader input, TextWriter output, CancellationToken ct = default)
    {
        _log.WriteLine("[spla-mcp] ready");

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

            var response = await HandleAsync(request, ct);

            // A notification has no id and must get no reply — answering one is a protocol error,
            // not merely noise.
            if (response is null) continue;

            await output.WriteLineAsync(response.ToJsonString());
            await output.FlushAsync(ct);
        }

        _log.WriteLine("[spla-mcp] stopped");
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
                case "notifications/cancelled":
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
        catch (Exception ex)
        {
            _log.WriteLine($"[spla-mcp] {method} failed: {ex}");
            return Error(id, -32603, ex.Message);
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

        var result = await _host.ExecuteToolAsync(_mode, name, args, ct, _context);

        return new JsonObject
        {
            ["content"] = Project(result),
            // MCP carries a flag where SPLA carries three outcomes. Refused and Failed both fold
            // into it: the distinction matters to an audit log, not to the model reading the answer.
            ["isError"] = result.IsError
        };
    }

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
