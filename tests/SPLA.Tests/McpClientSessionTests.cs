using System.Text.Json.Nodes;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.Mcp;
using SPLA.Mcp.Client;
using static SPLA.Tests.McpTestDuplex;

namespace SPLA.Tests;

/// <summary>
/// The client half: consuming somebody else's tools (Шаг 2 of
/// ../../docs/plans/PLAN_20260826_service_mcp-client.md).
///
/// <para>Half of these put our client at one end of <see cref="McpTestDuplex"/> and our own
/// <see cref="McpStdioServer"/> at the other — nothing spawned, no port opened, yet both sides are
/// the real ones: the cheapest honest end-to-end this feature can have, and the reason
/// <see cref="StreamTransport"/> exists apart from <see cref="StdioTransport"/>.</para>
///
/// <para>The other half talks to a <see cref="McpTestDuplex.ScriptedServer"/> instead, because the
/// three things that matter most cannot be provoked from our own server: it never sends us a
/// request, never reports progress we did not ask for, and never dies mid-call.</para>
/// </summary>
public class McpClientSessionTests
{
    private sealed class FakeHost : IToolHost
    {
        public required IEnumerable<ToolDefinition> Tools { get; init; }
        public Func<string, string, ToolResult>? Behaviour { get; init; }
        public List<string> Executed { get; } = [];

        public IEnumerable<ToolDefinition> GetToolDefinitions() => Tools;

        public Task<ToolResult> ExecuteToolAsync(
            AgentMode mode, string name, string argumentsJson,
            CancellationToken cancellationToken = default, ToolCallContext? context = null)
        {
            Executed.Add(name);
            return Task.FromResult(
                Behaviour?.Invoke(name, argumentsJson) ?? ToolResult.Text($"ran {name}"));
        }
    }

    private static ToolDefinition Tool(string name) => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name = name,
            Description = $"does {name}",
            Parameters = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject { ["q"] = new JsonObject { ["type"] = "string" } }
            }
        }
    };

    /// <summary>Runs our real server against the far ends of the pipes, for the duration of the test.</summary>
    private static Task ServeAsync(FakeHost host, LinePipe toServer, LinePipe toClient, CancellationToken ct)
    {
        var server = new McpStdioServer(host, host.GetToolDefinitions, log: TextWriter.Null);
        return Task.Run(() => server.RunAsync(toServer.Reader, toClient.Writer, ct), CancellationToken.None);
    }

    [Fact]
    public async Task Connecting_to_our_own_server_brings_back_its_tool_list()
    {
        var host = new FakeHost { Tools = [Tool("alpha"), Tool("beta")] };
        var (session, toServer, toClient) = Session();
        using var stop = new CancellationTokenSource();
        var serving = ServeAsync(host, toServer, toClient, stop.Token);

        await session.ConnectAsync();

        Assert.Equal(McpSessionState.Ready, session.State);
        Assert.Equal(["alpha", "beta"], session.Tools.Select(t => t.Name).OrderBy(n => n));
        Assert.Equal("does alpha", session.Tools.Single(t => t.Name == "alpha").Description);
        Assert.NotNull(session.Tools.Single(t => t.Name == "alpha").InputSchema);

        await session.DisposeAsync();
        await stop.CancelAsync();
    }

    [Fact]
    public async Task Calling_a_tool_returns_what_the_server_said()
    {
        var host = new FakeHost
        {
            Tools = [Tool("alpha")],
            Behaviour = (_, args) => ToolResult.Text($"got {args}")
        };
        var (session, toServer, toClient) = Session();
        using var stop = new CancellationTokenSource();
        var serving = ServeAsync(host, toServer, toClient, stop.Token);

        await session.ConnectAsync();
        var result = await session.CallToolAsync("alpha", """{"q":"hi"}""");

        Assert.Equal("alpha", host.Executed.Single());
        var text = result["content"]?[0]?["text"]?.GetValue<string>();
        Assert.Contains("\"q\":\"hi\"", text);

        await session.DisposeAsync();
        await stop.CancelAsync();
    }

    [Fact]
    public async Task A_tool_that_failed_comes_back_flagged_rather_than_thrown()
    {
        // The distinction McpProxyTool's mapping depends on: a tool that ran and failed is a normal
        // reply carrying isError, not a JSON-RPC error. Only the latter aborts the call.
        var host = new FakeHost
        {
            Tools = [Tool("alpha")],
            Behaviour = (_, _) => ToolResult.Fail("no such repository")
        };
        var (session, toServer, toClient) = Session();
        using var stop = new CancellationTokenSource();
        var serving = ServeAsync(host, toServer, toClient, stop.Token);

        await session.ConnectAsync();
        var result = await session.CallToolAsync("alpha", "{}");

        Assert.True(result["isError"]?.GetValue<bool>());
        Assert.Contains("no such repository", result["content"]?[0]?["text"]?.GetValue<string>());

        await session.DisposeAsync();
        await stop.CancelAsync();
    }

    [Fact]
    public async Task A_request_the_server_makes_of_us_is_refused_as_method_not_found()
    {
        // Wave one declares no sampling, no elicitation, no roots. A server that asks anyway must get
        // a protocol-shaped refusal it can act on — not silence it would sit and wait out.
        var (session, toServer, toClient) = Session();
        var server = new ScriptedServer(toServer, toClient);
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var connecting = session.ConnectAsync(stop.Token);
        await server.HandshakeAsync(ct: stop.Token);
        await connecting;

        server.Send(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 99,
            ["method"] = "elicitation/create",
            ["params"] = new JsonObject { ["message"] = "prod or staging?" }
        });

        var refusal = await server.ReadAsync(stop.Token);

        Assert.Equal(99, refusal["id"]!.GetValue<int>());
        Assert.Equal(-32601, refusal["error"]!["code"]!.GetValue<int>());
        Assert.Contains("elicitation/create", refusal["error"]!["message"]!.GetValue<string>());

        await session.DisposeAsync();
    }

    [Fact]
    public async Task Progress_notifications_reach_the_callback_that_asked_for_them()
    {
        var (session, toServer, toClient) = Session();
        var server = new ScriptedServer(toServer, toClient);
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var connecting = session.ConnectAsync(stop.Token);
        await server.HandshakeAsync(ct: stop.Token);
        await connecting;

        var ticks = new List<McpProgress>();
        var call = session.CallToolAsync("slow", "{}", ticks.Add, stop.Token);

        var request = await server.ReadAsync(stop.Token);
        var token = request["params"]!["_meta"]!["progressToken"]!.GetValue<string>();

        server.Send(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "notifications/progress",
            ["params"] = new JsonObject
            {
                ["progressToken"] = token,
                ["progress"] = 3,
                ["total"] = 10,
                ["message"] = "cloning"
            }
        });

        // Ordered behind the notification on the same pipe, so by the time the reply is parsed the
        // tick already has been — no polling, no sleep.
        server.Reply(request, new JsonObject
        {
            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = "done" })
        });

        await call;

        var tick = Assert.Single(ticks);
        Assert.Equal(3, tick.Progress);
        Assert.Equal(10, tick.Total);
        Assert.Equal("cloning", tick.Message);

        await session.DisposeAsync();
    }

    [Fact]
    public async Task A_call_in_flight_fails_when_the_server_hangs_up()
    {
        // Otherwise the call would sit until its own deadline — minutes of a chat spent waiting on a
        // process that is already gone.
        var (session, toServer, toClient) = Session();
        var server = new ScriptedServer(toServer, toClient);
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var connecting = session.ConnectAsync(stop.Token);
        await server.HandshakeAsync(ct: stop.Token);
        await connecting;

        var call = session.CallToolAsync("slow", "{}", null, stop.Token);
        await server.ReadAsync(stop.Token);

        toClient.Close();

        await Assert.ThrowsAnyAsync<Exception>(() => call);
        Assert.NotEqual(McpSessionState.Ready, session.State);

        await session.DisposeAsync();
    }

    [Fact]
    public async Task Calling_a_server_that_is_not_connected_says_so_instead_of_hanging()
    {
        var (session, _, _) = Session();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => session.CallToolAsync("slow", "{}"));

        Assert.Contains("disconnected", ex.Message);
        await session.DisposeAsync();
    }
}
