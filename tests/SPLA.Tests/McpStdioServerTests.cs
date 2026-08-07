using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.Mcp;
using System.Text.Json.Nodes;

namespace SPLA.Tests;

/// <summary>
/// The projection outward: SPLA's tools spoken as MCP over stdio.
/// <para>
/// These drive the server the way a client does — lines of JSON-RPC in, lines out — because the
/// failures that matter here are protocol failures, and a test that called the methods directly
/// would miss every one of them.
/// </para>
/// </summary>
public class McpStdioServerTests
{
    private sealed class FakeHost : IToolHost
    {
        public List<string> Executed { get; } = [];
        public Func<string, ToolResult>? Behaviour { get; init; }
        public required IEnumerable<ToolDefinition> Tools { get; init; }

        public IEnumerable<ToolDefinition> GetToolDefinitions() => Tools;

        public Task<ToolResult> ExecuteToolAsync(
            AgentMode mode, string name, string argumentsJson,
            CancellationToken cancellationToken = default, ToolCallContext? context = null)
        {
            Executed.Add(name);
            return Task.FromResult(Behaviour?.Invoke(name) ?? ToolResult.Text($"ran {name}"));
        }
    }

    private static ToolDefinition Tool(string name, bool bound = false) => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name = name,
            Description = $"{name} description",
            ConversationBound = bound,
            Parameters = new { type = "object", properties = new { } }
        }
    };

    /// <summary>Runs the given lines through a server and returns the responses it wrote.</summary>
    private static async Task<List<JsonNode>> Exchange(FakeHost host, params string[] lines)
    {
        // What the caller may see — the same decision the CLI makes with ToolExposure.
        var offered = host.Tools.Where(d => !d.Function.ConversationBound).ToList();

        var server = new McpStdioServer(host, () => offered, log: TextWriter.Null);

        var output = new StringWriter();
        await server.RunAsync(new StringReader(string.Join("\n", lines)), output);

        return output.ToString()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => JsonNode.Parse(l)!)
            .ToList();
    }

    private const string Init =
        """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","clientInfo":{"name":"t"},"capabilities":{}}}""";

    [Fact]
    public async Task Initialize_answers_and_a_notification_gets_no_reply()
    {
        var host = new FakeHost { Tools = [Tool("sql_query")] };

        var replies = await Exchange(host, Init, """{"jsonrpc":"2.0","method":"notifications/initialized"}""");

        // Exactly one: answering a notification is a protocol error, not merely noise.
        var reply = Assert.Single(replies);
        Assert.Equal("2.0", reply["jsonrpc"]!.GetValue<string>());
        Assert.NotNull(reply["result"]!["capabilities"]!["tools"]);
    }

    /// <summary>The subset served here has not changed across the revisions in circulation, so a
    /// client asking for one we did not name is answered in its own terms rather than refused.</summary>
    [Fact]
    public async Task The_clients_protocol_version_is_echoed_back()
    {
        var host = new FakeHost { Tools = [Tool("sql_query")] };

        var replies = await Exchange(host,
            """{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2099-01-01","capabilities":{}}}""");

        Assert.Equal("2099-01-01", replies[0]["result"]!["protocolVersion"]!.GetValue<string>());
    }

    [Fact]
    public async Task Tools_list_carries_names_descriptions_and_schemas()
    {
        var host = new FakeHost { Tools = [Tool("sql_query"), Tool("ssh_run")] };

        var replies = await Exchange(host, Init, """{"jsonrpc":"2.0","id":2,"method":"tools/list"}""");

        var tools = replies[1]["result"]!["tools"]!.AsArray();
        Assert.Equal(2, tools.Count);
        Assert.Equal("sql_query", tools[0]!["name"]!.GetValue<string>());
        Assert.Equal("sql_query description", tools[0]!["description"]!.GetValue<string>());
        Assert.Equal("object", tools[0]!["inputSchema"]!["type"]!.GetValue<string>());
    }

    [Fact]
    public async Task A_call_returns_content_blocks_and_no_error_flag()
    {
        var host = new FakeHost { Tools = [Tool("sql_query")] };

        var replies = await Exchange(host, Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"sql_query","arguments":{"sql":"select 1"}}}""");

        var result = replies[1]["result"]!;
        Assert.False(result["isError"]!.GetValue<bool>());
        Assert.Equal("text", result["content"]![0]!["type"]!.GetValue<string>());
        Assert.Equal("ran sql_query", result["content"]![0]!["text"]!.GetValue<string>());
    }

    /// <summary>MCP carries a flag where SPLA carries three outcomes; refused and failed both fold
    /// into it. Before the outcome was separate this could not be reported at all — the text simply
    /// began with the word "error" and no client could tell.</summary>
    [Theory]
    [InlineData(ToolOutcome.Failed)]
    [InlineData(ToolOutcome.Refused)]
    public async Task Both_kinds_of_bad_news_set_the_error_flag(ToolOutcome outcome)
    {
        var host = new FakeHost
        {
            Tools = [Tool("sql_query")],
            Behaviour = _ => outcome == ToolOutcome.Failed
                ? ToolResult.Fail("nope", "reason")
                : ToolResult.Refuse("nope", "reason")
        };

        var replies = await Exchange(host, Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"sql_query","arguments":{}}}""");

        Assert.True(replies[1]["result"]!["isError"]!.GetValue<bool>());
    }

    /// <summary>
    /// The one that live testing caught. Filtering only the catalogue is decoration: a foreign head
    /// can name a tool it was never offered — the names are in its own history — and would reach it.
    /// Callable must be exactly listable, and the check belongs on the call.
    /// </summary>
    [Fact]
    public async Task A_tool_that_was_not_offered_cannot_be_called_by_naming_it()
    {
        var host = new FakeHost { Tools = [Tool("sql_query"), Tool("mark_set", bound: true)] };

        var replies = await Exchange(host, Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"mark_set","arguments":{"name":"x"}}}""");

        Assert.True(replies[1]["result"]!["isError"]!.GetValue<bool>());
        // And, decisively, the host was never asked to run it.
        Assert.Empty(host.Executed);
    }

    [Fact]
    public async Task An_image_block_survives_the_projection()
    {
        var host = new FakeHost
        {
            Tools = [Tool("browser_screenshot")],
            Behaviour = _ => ToolResult.From(
                new ToolText("shot taken"),
                new ToolImage("QUJD", "image/png"))
        };

        var replies = await Exchange(host, Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"browser_screenshot","arguments":{}}}""");

        var content = replies[1]["result"]!["content"]!.AsArray();
        Assert.Equal(2, content.Count);
        Assert.Equal("image", content[1]!["type"]!.GetValue<string>());
        Assert.Equal("QUJD", content[1]!["data"]!.GetValue<string>());
        Assert.Equal("image/png", content[1]!["mimeType"]!.GetValue<string>());
    }

    [Fact]
    public async Task An_unknown_method_is_answered_rather_than_ignored()
    {
        var host = new FakeHost { Tools = [] };

        var replies = await Exchange(host, Init, """{"jsonrpc":"2.0","id":9,"method":"resources/list"}""");

        // Silence would leave a client probing for an optional capability waiting forever.
        Assert.Equal(-32601, replies[1]["error"]!["code"]!.GetValue<int>());
    }

    [Fact]
    public async Task A_malformed_line_does_not_kill_the_session()
    {
        var host = new FakeHost { Tools = [Tool("sql_query")] };

        var replies = await Exchange(host, Init, "{ this is not json",
            """{"jsonrpc":"2.0","id":3,"method":"tools/list"}""");

        Assert.Equal(2, replies.Count);   // the garbage produced nothing, the next request still worked
        Assert.NotNull(replies[1]["result"]!["tools"]);
    }
}
