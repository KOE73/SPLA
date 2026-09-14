using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry;

namespace SPLA.Tests.Geometry;

/// <summary>
/// geom_box is the tool the loop turns on, so what is pinned here is mostly what it must refuse:
/// a call that mixes absolute fields with deltas has no single meaning, and guessing one leaves the
/// model correcting a box that moved somewhere it never asked for.
/// </summary>
public sealed class GeometryBoxToolTests
{
    private static Dictionary<string, IMcpTool> Tools() =>
        new GeometryPlugin().Initialize(new ResolvedSettings()).ToDictionary(t => t.Name);

    /// <summary>The ambient chat has to be opened in the test method itself: AgentSessionScope is an
    /// AsyncLocal, and a scope begun inside an awaited helper does not flow back out of it.</summary>
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
    public async Task A_new_box_is_created_drawn_and_echoed_back()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","cx":400,"cy":300,"width":400,"height":300,"angle":-6}""");

        Assert.False(result.IsError);
        Assert.Single(result.Content.OfType<ToolImage>());
        Assert.Contains("created 'bag'", result.TextContent);
        Assert.Contains("cx=400 cy=300 w=400 h=300 angle=-6 (tilted up to the right)", result.TextContent);
        Assert.Contains("editing", result.TextContent);
    }

    [Fact]
    public async Task Deltas_move_the_box_from_where_it_is()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        var result = await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","dx":-20,"dy":10,"dw":40,"dangle":-6}""");

        Assert.False(result.IsError);
        Assert.Contains("updated 'bag'", result.TextContent);
        Assert.Contains("cx=380 cy=310 w=440 h=300 angle=-6 (tilted up to the right)", result.TextContent);
    }

    [Fact]
    public async Task Absolute_fields_and_deltas_in_one_call_are_refused_by_name()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        var result = await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":420,"dx":-20}""");

        Assert.True(result.IsError);
        Assert.Contains("cx", result.TextContent);
        Assert.Contains("dx", result.TextContent);
        Assert.Empty(result.Content.OfType<ToolImage>());

        // And the box did not move.
        var look = await tools["geom_box"].ExecuteAsync("""{"name":"bag","dx":0}""");
        Assert.Contains("cx=400", look.TextContent);
    }

    [Fact]
    public async Task A_delta_on_a_box_that_does_not_exist_is_refused()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_box"].ExecuteAsync("""{"name":"ghost","dx":10}""");

        Assert.True(result.IsError);
        Assert.Contains("ghost", result.TextContent);
        Assert.Contains("cx, cy, width and height", result.TextContent);
    }

    [Fact]
    public async Task A_new_box_without_a_size_is_refused()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300}""");

        Assert.True(result.IsError);
        Assert.Contains("needs cx, cy, width and height", result.TextContent);
    }

    [Fact]
    public async Task A_partial_absolute_update_leaves_the_rest_alone()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300,"angle":10}""");
        var result = await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":420}""");

        Assert.Contains("cx=420 cy=300 w=400 h=300 angle=+10 (tilted down to the right)", result.TextContent);
    }

    [Fact]
    public async Task Delete_removes_the_box_and_says_so_when_there_is_none()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        var deleted = await tools["geom_box"].ExecuteAsync("""{"name":"bag","delete":true}""");

        Assert.False(deleted.IsError);
        Assert.Contains("deleted 'bag'", deleted.TextContent);
        Assert.Contains("objects here: none", deleted.TextContent);

        var again = await tools["geom_box"].ExecuteAsync("""{"name":"bag","delete":true}""");
        Assert.True(again.IsError);
    }

    [Fact]
    public async Task An_empty_update_is_refused_with_the_way_to_just_look_again()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        var result = await tools["geom_box"].ExecuteAsync("""{"name":"bag"}""");

        Assert.True(result.IsError);
        Assert.Contains("geom_view", result.TextContent);
    }

    [Fact]
    public async Task Without_an_open_image_it_refuses_instead_of_throwing()
    {
        var chat = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        using var _ = AgentSessionScope.Begin(chat);

        var result = await Tools()["geom_box"].ExecuteAsync("""{"name":"bag","cx":1,"cy":1,"width":2,"height":2}""");

        Assert.True(result.IsError);
        Assert.Contains("geom_open", result.TextContent);
    }

    /// <summary>The sign of the angle is the one thing a model reliably gets backwards: mathematics
    /// puts y upward, an image puts it downward, and "positive clockwise" in a description loses to
    /// that prior. Every printed angle therefore says what it looks like, so the model can check it
    /// against the picture rather than recall the rule.
    /// </summary>
    [Fact]
    public async Task Every_printed_angle_says_which_way_the_box_leans()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var clockwise = await tools["geom_box"].ExecuteAsync(
            """{"name":"mark","cx":400,"cy":300,"width":200,"height":80,"angle":8}""");
        Assert.Contains("angle=+8 (tilted down to the right)", clockwise.TextContent);

        var flipped = await tools["geom_box"].ExecuteAsync("""{"name":"mark","angle":-8}""");
        Assert.Contains("angle=-8 (tilted up to the right)", flipped.TextContent);

        // Zero leans neither way, and saying so would be noise on every upright box.
        var straight = await tools["geom_box"].ExecuteAsync("""{"name":"mark","angle":0}""");
        Assert.Contains("angle=0", straight.TextContent);
        Assert.DoesNotContain("tilted", straight.TextContent);
    }
}
