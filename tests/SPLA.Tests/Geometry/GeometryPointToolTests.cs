using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry;

namespace SPLA.Tests.Geometry;

/// <summary>
/// A point is its own tool, not a flag on geom_box: two numbers instead of five, and one schema
/// covering both shapes is how a small model ends up sending a box with no size.
/// </summary>
public sealed class GeometryPointToolTests
{
    private static Dictionary<string, IMcpTool> Tools() =>
        new GeometryPlugin().Initialize(new ResolvedSettings()).ToDictionary(t => t.Name);

    private static (AgentSession Chat, Dictionary<string, IMcpTool> Tools, IDisposable Scope) Begin()
    {
        var chat = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        var scope = AgentSessionScope.Begin(chat);
        return (chat, Tools(), scope);
    }

    private static async Task OpenFrame(AgentSession chat, Dictionary<string, IMcpTool> tools)
    {
        var opened = await tools["geom_open"].ExecuteAsync($$"""{"image":"{{GeometryToolsTests.Handle(chat)}}"}""");
        Assert.False(opened.IsError, opened.TextContent);
    }

    [Fact]
    public void The_point_schema_has_no_box_fields_in_it()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(
            Tools()["geom_point"].GetDefinition().Function.Parameters);
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        var keys = doc.RootElement.GetProperty("properties").EnumerateObject().Select(p => p.Name).ToArray();
        Assert.Equal(["name", "x", "y", "dx", "dy", "delete"], keys);
    }

    [Fact]
    public async Task A_point_is_created_moved_and_deleted()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var created = await tools["geom_point"].ExecuteAsync("""{"name":"mark","x":418,"y":203}""");
        Assert.False(created.IsError, created.TextContent);
        Assert.Single(created.Content.OfType<ToolImage>());
        Assert.Contains("created 'mark'", created.TextContent);
        Assert.Contains("point  x=418 y=203", created.TextContent);

        var moved = await tools["geom_point"].ExecuteAsync("""{"name":"mark","dx":-8,"dy":5}""");
        Assert.Contains("x=410 y=208", moved.TextContent);

        var deleted = await tools["geom_point"].ExecuteAsync("""{"name":"mark","delete":true}""");
        Assert.Contains("deleted 'mark'", deleted.TextContent);
        Assert.Contains("objects here: none", deleted.TextContent);
    }

    [Fact]
    public async Task Absolute_and_delta_in_one_call_are_refused()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_point"].ExecuteAsync("""{"name":"mark","x":100,"y":100}""");
        var result = await tools["geom_point"].ExecuteAsync("""{"name":"mark","x":120,"dy":4}""");

        Assert.True(result.IsError);
        Assert.Contains("x, y", result.TextContent);
        Assert.Contains("dx, dy", result.TextContent);
    }

    [Fact]
    public async Task A_name_already_taken_by_a_box_is_refused_rather_than_overwritten()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":200,"height":100}""");
        var asPoint = await tools["geom_point"].ExecuteAsync("""{"name":"bag","x":10,"y":10}""");

        Assert.True(asPoint.IsError);
        Assert.Contains("geom_box", asPoint.TextContent);

        var asBox = await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":401}""");
        Assert.False(asBox.IsError);
    }

    [Fact]
    public async Task A_delta_on_a_point_that_does_not_exist_is_refused()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_point"].ExecuteAsync("""{"name":"mark","dx":3}""");

        Assert.True(result.IsError);
        Assert.Contains("x and y", result.TextContent);
    }
}
