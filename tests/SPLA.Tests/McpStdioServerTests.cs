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

        /// <summary>Stands in for the pipeline's ProgressNodeStage plus a tool that reports. The real
        /// host opens the node; a fake that did not could never show that the tree reaches the wire.</summary>
        public Func<CancellationToken, Task>? Work { get; init; }

        public required IEnumerable<ToolDefinition> Tools { get; init; }

        private readonly TaskCompletionSource _idle = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Completes when a call has run its course, however it ended. The tests use it to hold
        /// the pipe open until then, because closing it means something — see the shutdown test.</summary>
        public Task Idle => _idle.Task;

        public IEnumerable<ToolDefinition> GetToolDefinitions() => Tools;

        public async Task<ToolResult> ExecuteToolAsync(
            AgentMode mode, string name, string argumentsJson,
            CancellationToken cancellationToken = default, ToolCallContext? context = null)
        {
            Executed.Add(name);
            try
            {
                if (Work is not null)
                {
                    using var node = ProgressScope.BeginNode(name);
                    await Work(cancellationToken);
                }

                return Behaviour?.Invoke(name) ?? ToolResult.Text($"ran {name}");
            }
            finally
            {
                _idle.TrySetResult();
            }
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

    /// <summary>
    /// Feeds lines one at a time, holding the last one back until <paramref name="gate"/> completes.
    /// A <see cref="StringReader"/> hands over the whole script before anything has run, which is fine
    /// for request/response but cannot express "this arrives while that one is still running" — the
    /// only shape in which a cancellation is worth testing.
    /// </summary>
    private sealed class ScriptedReader(IReadOnlyList<string> lines, Task? lastLineGate, Task? eofGate)
        : TextReader
    {
        private int _next;

        public override async ValueTask<string?> ReadLineAsync(CancellationToken ct)
        {
            if (_next >= lines.Count)
            {
                if (eofGate is not null) await Hold(eofGate, ct);
                return null;                                 // the client hangs up
            }

            if (lastLineGate is not null && _next == lines.Count - 1) await Hold(lastLineGate, ct);

            return lines[_next++];
        }

        /// <summary>Waits, but never forever: a gate that never opens is a bug in the test, and it
        /// should surface as a failed assertion rather than a hung run.</summary>
        private static async Task Hold(Task gate, CancellationToken ct)
        {
            try { await gate.WaitAsync(TimeSpan.FromSeconds(10), ct); }
            catch (TimeoutException) { }
        }

        public override string? ReadLine() => _next < lines.Count ? lines[_next++] : null;
    }

    /// <summary>
    /// Runs the given lines through a server and returns everything it wrote — answers and
    /// notifications alike, in the order they left. The pipe stays open until any call has finished,
    /// because closing it is not neutral: EOF means the client is gone and outstanding work is stopped.
    /// </summary>
    private static Task<List<JsonNode>> Exchange(FakeHost host, params string[] lines)
        => Exchange(host, null, EofGate(host, lines), lines);

    /// <summary>As <see cref="Exchange(FakeHost, string[])"/>, but the final line is withheld until
    /// <paramref name="gate"/> completes — the only way to express "this arrives while that one is
    /// still running".</summary>
    private static Task<List<JsonNode>> ExchangeGated(FakeHost host, Task gate, params string[] lines)
        => Exchange(host, gate, EofGate(host, lines), lines);

    /// <summary>Something to wait for at EOF only when the host has slow work to do. A call that
    /// resolves at once — or is refused before the host is ever reached — has nothing outstanding, and
    /// waiting on a signal that will never come is just a stalled test.</summary>
    private static Task? EofGate(FakeHost host, string[] lines)
        => host.Work is not null ? host.Idle : null;

    /// <summary>Closes the pipe the moment the script runs out, whatever is still running.</summary>
    private static Task<List<JsonNode>> ExchangeAndHangUp(FakeHost host, Task gate, params string[] lines)
        => Exchange(host, gate, null, lines);

    private static async Task<List<JsonNode>> Exchange(
        FakeHost host, Task? lastLineGate, Task? eofGate, string[] lines)
    {
        // What the caller may see — the same decision the CLI makes with ToolExposure.
        var offered = host.Tools.Where(d => !d.Function.ConversationBound).ToList();

        var server = new McpStdioServer(host, () => offered, log: TextWriter.Null);

        var output = new StringWriter();
        await server.RunAsync(new ScriptedReader(lines, lastLineGate, eofGate), output);

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

    // ── Progress ────────────────────────────────────────────────────────────────────────────────
    //
    // Nothing below tests a tool. The point of the mechanism is that it is tool-agnostic: every tool
    // in the project already reports into ProgressScope and the pipeline already opens a node per
    // call, so what these check is only that a tree is opened over an MCP call and reaches the wire.

    /// <summary>Ticks separated by more than the reporter's floor, so the throttle is not what is
    /// under test here.</summary>
    private static async Task Ticks(int count, CancellationToken ct)
    {
        for (var i = 1; i <= count; i++)
        {
            ProgressScope.Report(i, count, "working");
            await Task.Delay(160, ct);
        }
    }

    [Fact]
    public async Task A_call_that_asked_for_progress_is_told_how_it_is_going()
    {
        var host = new FakeHost { Tools = [Tool("port_scan")], Work = ct => Ticks(2, ct) };

        var frames = await Exchange(host, Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"port_scan","arguments":{},"_meta":{"progressToken":"tok-7"}}}""");

        var progress = frames
            .Where(f => f["method"]?.GetValue<string>() == "notifications/progress")
            .ToList();

        Assert.True(progress.Count >= 2, $"expected ticks on the wire, saw {progress.Count}");

        // The token is the client's and is echoed verbatim; the counter is ours and must only ever go up.
        Assert.All(progress, f => Assert.Equal("tok-7", f["params"]!["progressToken"]!.GetValue<string>()));
        var counters = progress.Select(f => f["params"]!["progress"]!.GetValue<long>()).ToList();
        Assert.Equal(counters.OrderBy(v => v).ToList(), counters);
        Assert.Equal(counters.Distinct().Count(), counters.Count);

        // A notification carries no id and is not an answer: the result still arrives, once.
        var result = Assert.Single(frames, f => f["id"]?.GetValue<int>() == 2);
        Assert.Equal("ran port_scan", result["result"]!["content"]![0]!["text"]!.GetValue<string>());
    }

    /// <summary>The message is the payload — MCP has one scalar per tick where a tree has many, so
    /// what a person reads is the path to whatever moved and the counter it reported.</summary>
    [Fact]
    public async Task The_tick_names_the_tool_and_what_it_last_said()
    {
        var host = new FakeHost { Tools = [Tool("port_scan")], Work = ct => Ticks(1, ct) };

        var frames = await Exchange(host, Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"port_scan","arguments":{},"_meta":{"progressToken":1}}}""");

        var messages = Messages(frames);

        Assert.Contains(messages, m => m.Contains("port_scan") && m.Contains("working") && m.Contains("(1/1)"));
    }

    /// <summary>
    /// The rate limit holds a tick back; it must not swallow it. This is the case that matters and the
    /// one a discarding limiter gets wrong: a tool says what it is about to do and then goes quiet for
    /// a long time. That sentence lands inside the floor — the frame announcing the tool started is
    /// the same instant — so discarding it would leave the reader watching "lan_scan …" for the whole
    /// scan, holding the less informative of the two only because it happened first.
    /// <para>(A tick overtaken by a fresher one inside the same window is a different matter: there the
    /// newest is the true answer to "what is happening now", and coalescing is meant to prefer it.)</para>
    /// </summary>
    [Fact]
    public async Task A_tick_the_rate_limit_held_back_still_arrives()
    {
        var host = new FakeHost
        {
            Tools = [Tool("lan_scan")],
            Work = async ct =>
            {
                ProgressScope.Report(0, 254, "sweeping 10.0.0.0/24");
                await Task.Delay(500, ct);          // …and nothing to say for a long while
            }
        };

        var frames = await Exchange(host, Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"lan_scan","arguments":{},"_meta":{"progressToken":"t"}}}""");

        Assert.Contains(Messages(frames), m => m.Contains("sweeping 10.0.0.0/24"));
    }

    private static List<string> Messages(IEnumerable<JsonNode> frames) => frames
        .Where(f => f["method"]?.GetValue<string>() == "notifications/progress")
        .Select(f => f["params"]!["message"]!.GetValue<string>())
        .ToList();

    /// <summary>Progress is opt-in per call. A client that never asked would not know what to do with
    /// the frames, and sending them anyway is noise on a pipe where noise is fatal.</summary>
    [Fact]
    public async Task Without_a_token_nothing_is_reported()
    {
        var host = new FakeHost { Tools = [Tool("port_scan")], Work = ct => Ticks(2, ct) };

        var frames = await Exchange(host, Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"port_scan","arguments":{}}}""");

        Assert.DoesNotContain(frames, f => f["method"] is not null);
        Assert.Single(frames, f => f["id"]?.GetValue<int>() == 2);
    }

    /// <summary>
    /// The reason a call does not hold the read loop. A cancellation sent while the tool is running is
    /// unreachable if the loop is sitting inside the call it is about — which is exactly the case that
    /// matters, since the calls worth withdrawing are the long ones.
    /// </summary>
    [Fact]
    public async Task A_call_can_be_withdrawn_while_it_runs()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var host = new FakeHost
        {
            Tools = [Tool("agent_spawn")],
            Work = async ct =>
            {
                started.TrySetResult();
                await Task.Delay(Timeout.Infinite, ct);
            }
        };

        var frames = await ExchangeGated(host, started.Task,
            Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"agent_spawn","arguments":{}}}""",
            """{"jsonrpc":"2.0","method":"notifications/cancelled","params":{"requestId":2,"reason":"user"}}""");

        // Per the spec a withdrawn request must not be answered — and it was in fact reached, which is
        // the half a hung call would fail.
        Assert.Equal("agent_spawn", Assert.Single(host.Executed));
        Assert.DoesNotContain(frames, f => f["id"]?.GetValue<int>() == 2);
    }

    /// <summary>
    /// EOF on stdin is the client hanging up, and it is not neutral: there is nobody left to answer, so
    /// a sub-agent that would have run for another four minutes is stopped rather than left holding a
    /// process whose only reason to exist has gone.
    /// </summary>
    [Fact]
    public async Task Closing_the_pipe_stops_what_was_still_running()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stopped = false;
        var host = new FakeHost
        {
            Tools = [Tool("agent_spawn")],
            Work = async ct =>
            {
                started.TrySetResult();
                try { await Task.Delay(Timeout.Infinite, ct); }
                catch (OperationCanceledException) { stopped = true; throw; }
            }
        };

        // The last line is withheld until the tool is running, and EOF follows it immediately.
        var frames = await ExchangeAndHangUp(host, started.Task,
            Init,
            """{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"agent_spawn","arguments":{}}}""",
            """{"jsonrpc":"2.0","id":3,"method":"ping"}""");

        Assert.True(stopped, "the running call should have been cancelled when the pipe closed");
        Assert.DoesNotContain(frames, f => f["id"]?.GetValue<int>() == 2);
    }
}
