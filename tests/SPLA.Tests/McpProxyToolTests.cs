using System.Text.Json.Nodes;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Mcp;
using SPLA.Mcp.Client;
using static SPLA.Tests.McpTestDuplex;

namespace SPLA.Tests;

/// <summary>
/// Шаг 3 of ../../docs/plans/PLAN_20260826_service_mcp-client.md: turning one foreign tool into an
/// <c>IMcpTool</c>. What matters here is not that the plumbing works — <c>McpClientSessionTests</c>
/// already covers the session — but that the verdict is naive by construction (ADR §2) and that the
/// wire's content blocks decode into exactly what the outward server encodes them from
/// (<c>McpStdioServer.Project</c>, the mirror image).
/// </summary>
public sealed class McpProxyToolTests
{
    private static IDisposable Scope(out SPLA.Domain.Security.ChatDoubt doubt)
    {
        var session = new AgentSession(
            new SPLA.Domain.Agent.KeyValueStore("session"),
            new SPLA.Domain.Agent.MarkManager(),
            new SPLA.Domain.Agent.SkillSession());
        doubt = session.Doubt;
        return AgentSessionScope.Begin(session);
    }

    private static McpToolInfo Info(
        string name = "create_issue",
        string description = "Opens an issue.",
        bool? destructiveHint = null,
        bool? readOnlyHint = null) => new()
    {
        Name = name,
        Description = description,
        InputSchema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject { ["title"] = new JsonObject { ["type"] = "string" } }
        },
        DestructiveHint = destructiveHint,
        ReadOnlyHint = readOnlyHint
    };

    // ── GetDefinition: the naive verdict ────────────────────────────────────

    [Fact]
    public void The_definition_is_prefixed_scoped_foreign_and_high_risk_by_default()
    {
        var (session, _, _) = Session("ghmcp");
        var info = Info();
        var tool = new McpProxyTool(session, info, "ghmcp", serverIsNamedOrigin: false);

        var def = tool.GetDefinition();

        Assert.Equal("ghmcp_create_issue", tool.Name);
        Assert.Equal(ToolScope.Foreign, def.Function.Scope);
        Assert.Equal(ToolEffect.Write, def.Function.Effect);
        Assert.Equal(ToolRisk.High, def.Function.Risk);
        Assert.False(def.Function.StrictSchema);
        Assert.False(def.Function.ConversationBound);
        Assert.False(def.Function.SupportsBackground);
        // The server's schema is passed on verbatim, not rewritten — a stranger's schema that does
        // not satisfy strict mode must not be "fixed" into one that silently means something else.
        Assert.Same(info.InputSchema, def.Function.Parameters);
    }

    [Fact]
    public void DestructiveHint_raises_the_risk_to_danger()
    {
        var (session, _, _) = Session("ghmcp");
        var tool = new McpProxyTool(session, Info(destructiveHint: true), "ghmcp", serverIsNamedOrigin: false);

        Assert.Equal(ToolRisk.Danger, tool.GetDefinition().Function.Risk);
    }

    [Fact]
    public void ReadOnlyHint_is_never_acted_on()
    {
        // The one direction lying about an annotation pays off, so the one hint we honour is the
        // one that can only make the verdict stricter. A server claiming read-only must not be
        // able to talk its way to a lighter risk.
        var (session, _, _) = Session("ghmcp");
        var tool = new McpProxyTool(session, Info(readOnlyHint: true), "ghmcp", serverIsNamedOrigin: false);

        Assert.Equal(ToolRisk.High, tool.GetDefinition().Function.Risk);
    }

    [Fact]
    public void The_description_names_the_server_that_provided_it()
    {
        var (session, _, _) = Session("ghmcp");
        var tool = new McpProxyTool(session, Info(), "ghmcp", serverIsNamedOrigin: false);

        Assert.Contains("MCP server 'ghmcp'", tool.GetDefinition().Function.Description);
    }

    [Fact]
    public void A_tool_name_that_cannot_be_registered_throws_at_construction()
    {
        var (session, _, _) = Session("GH-MCP");   // invalid server id

        var ex = Assert.Throws<ArgumentException>(
            () => new McpProxyTool(session, Info(), "GH-MCP", serverIsNamedOrigin: false));
        Assert.Contains("server id", ex.Message);
    }

    // ── ExecuteAsync: content mapping and outcome ───────────────────────────

    [Fact]
    public async Task Calling_before_the_server_is_connected_fails_without_reaching_the_wire()
    {
        var (session, _, _) = Session();
        var tool = new McpProxyTool(session, Info(), "probe", serverIsNamedOrigin: true);

        var result = await tool.ExecuteAsync("{}");

        Assert.True(result.IsError);
        Assert.Contains("not connected", result.TextContent);
    }

    [Fact]
    public async Task A_successful_call_maps_text_image_and_resource_blocks()
    {
        var (session, toServer, toClient) = Session();
        var server = new ScriptedServer(toServer, toClient);
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var connecting = session.ConnectAsync(stop.Token);
        await server.HandshakeAsync(ct: stop.Token);
        await connecting;

        var tool = new McpProxyTool(session, Info(), "probe", serverIsNamedOrigin: true);
        var executing = tool.ExecuteAsync("{}", stop.Token);

        var request = await server.ReadAsync(stop.Token);
        server.Reply(request, new JsonObject
        {
            ["content"] = new JsonArray(
                new JsonObject { ["type"] = "text", ["text"] = "opened #42" },
                new JsonObject { ["type"] = "image", ["data"] = "Zm9v", ["mimeType"] = "image/png" },
                new JsonObject
                {
                    ["type"] = "resource",
                    ["resource"] = new JsonObject
                    {
                        ["uri"] = "file:///log.txt",
                        ["mimeType"] = "text/plain",
                        ["text"] = "the log"
                    }
                })
        });

        var result = await executing;

        Assert.False(result.IsError);
        Assert.Equal(3, result.Content.Count);
        Assert.Equal("opened #42", Assert.IsType<ToolText>(result.Content[0]).Text);
        var image = Assert.IsType<ToolImage>(result.Content[1]);
        Assert.Equal("Zm9v", image.Data);
        Assert.Equal("image/png", image.MimeType);
        var resource = Assert.IsType<ToolResource>(result.Content[2]);
        Assert.Equal("file:///log.txt", resource.Uri);

        await session.DisposeAsync();
    }

    [Fact]
    public async Task A_resource_link_block_is_read_the_same_as_a_resource_block()
    {
        var (session, toServer, toClient) = Session();
        var server = new ScriptedServer(toServer, toClient);
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var connecting = session.ConnectAsync(stop.Token);
        await server.HandshakeAsync(ct: stop.Token);
        await connecting;

        var tool = new McpProxyTool(session, Info(), "probe", serverIsNamedOrigin: true);
        var executing = tool.ExecuteAsync("{}", stop.Token);

        var request = await server.ReadAsync(stop.Token);
        server.Reply(request, new JsonObject
        {
            ["content"] = new JsonArray(new JsonObject
            {
                ["type"] = "resource_link",
                ["uri"] = "file:///README.md",
                ["name"] = "readme"
            })
        });

        var result = await executing;

        var resource = Assert.IsType<ToolResource>(Assert.Single(result.Content));
        Assert.Equal("file:///README.md", resource.Uri);

        await session.DisposeAsync();
    }

    [Fact]
    public async Task IsError_from_the_server_maps_to_a_failed_outcome_not_a_thrown_exception()
    {
        var (session, toServer, toClient) = Session();
        var server = new ScriptedServer(toServer, toClient);
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var connecting = session.ConnectAsync(stop.Token);
        await server.HandshakeAsync(ct: stop.Token);
        await connecting;

        var tool = new McpProxyTool(session, Info(), "probe", serverIsNamedOrigin: true);
        var executing = tool.ExecuteAsync("{}", stop.Token);

        var request = await server.ReadAsync(stop.Token);
        server.Reply(request, new JsonObject
        {
            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = "no such repo" }),
            ["isError"] = true
        });

        var result = await executing;

        Assert.True(result.IsError);
        Assert.Contains("no such repo", result.TextContent);

        await session.DisposeAsync();
    }

    // ── The doubt flag ───────────────────────────────────────────────────────

    [Fact]
    public async Task An_unnamed_server_raises_the_chat_doubt_flag_on_a_successful_call()
    {
        using var scope = Scope(out var doubt);
        var (session, toServer, toClient) = Session("ghmcp");
        var server = new ScriptedServer(toServer, toClient);
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var connecting = session.ConnectAsync(stop.Token);
        await server.HandshakeAsync(ct: stop.Token);
        await connecting;

        var tool = new McpProxyTool(session, Info(), "ghmcp", serverIsNamedOrigin: false);
        var executing = tool.ExecuteAsync("{}", stop.Token);
        var request = await server.ReadAsync(stop.Token);
        server.Reply(request, new JsonObject
        {
            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = "ok" })
        });
        await executing;

        Assert.True(doubt.IsRaised);
        Assert.Contains(doubt.Causes, c => c.Origin.Zone == "mcp:ghmcp" && !c.Origin.OperatorNamed);

        await session.DisposeAsync();
    }

    [Fact]
    public async Task A_named_server_does_not_raise_the_doubt_flag()
    {
        using var scope = Scope(out var doubt);
        var (session, toServer, toClient) = Session("internal");
        var server = new ScriptedServer(toServer, toClient);
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var connecting = session.ConnectAsync(stop.Token);
        await server.HandshakeAsync(ct: stop.Token);
        await connecting;

        var tool = new McpProxyTool(session, Info(), "internal", serverIsNamedOrigin: true);
        var executing = tool.ExecuteAsync("{}", stop.Token);
        var request = await server.ReadAsync(stop.Token);
        server.Reply(request, new JsonObject
        {
            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = "ok" })
        });
        await executing;

        Assert.False(doubt.IsRaised);

        await session.DisposeAsync();
    }
}
