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

    /// <summary>The colour the model sees and the word it is asked to send are one word, and the
    /// mapping between them is written down rather than inferred from the picture.</summary>
    [Fact]
    public async Task The_editing_box_is_told_which_colour_is_which_edge_and_an_accepted_one_is_not()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var placed = await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        Assert.Contains("edges: cyan=top, magenta=right, yellow=bottom, green=left", placed.TextContent);
        Assert.Contains("white dot in a ring = its centre", placed.TextContent);

        var accepted = await tools["geom_accept"].ExecuteAsync("""{"name":"bag"}""");
        Assert.False(accepted.IsError, accepted.TextContent);
        Assert.DoesNotContain("edges: cyan=top", accepted.TextContent);
    }

    /// <summary>
    /// One side moves and the opposite one stays put. This is the whole point of the pair: in a live
    /// run the model wrote "5-65 is cut on the left and 50KG is cut on the right" six times over,
    /// while the only size control it had was dw, symmetric about the centre.
    /// </summary>
    [Theory]
    [InlineData("cyan", 40, 400, 280, 400, 340)]      // top, outward: up by 20, 40 taller
    [InlineData("yellow", 40, 400, 320, 400, 340)]    // bottom, outward: down by 20
    [InlineData("green", 40, 380, 300, 440, 300)]     // left, outward: left by 20, 40 wider
    [InlineData("magenta", 40, 420, 300, 440, 300)]   // right, outward: right by 20
    [InlineData("green", -40, 420, 300, 360, 300)]    // left, inward: the box shrinks from the left
    [InlineData("cyan", -40, 400, 320, 400, 260)]     // top, inward
    public async Task One_named_edge_moves_and_the_others_stay(
        string edge, int by, int cx, int cy, int width, int height)
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        var result = await tools["geom_box"].ExecuteAsync(
            $$"""{"name":"bag","edge":"{{edge}}","by":{{by}}}""");

        Assert.False(result.IsError, result.TextContent);
        Assert.Contains($"cx={cx} cy={cy} w={width} h={height}", result.TextContent);
        Assert.Contains(by > 0 ? "outward" : "inward", result.TextContent);
    }

    /// <summary>The side belongs to the box, not to the screen. Turned a quarter of a circle, the
    /// cyan edge is the one now facing right, and pushing it outward must move the centre to the
    /// right — not up, which is where "top" would have sent it.</summary>
    [Fact]
    public async Task An_edge_moves_along_the_box_axis_not_the_screen()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","cx":400,"cy":300,"width":400,"height":300,"angle":90}""");
        var result = await tools["geom_box"].ExecuteAsync("""{"name":"bag","edge":"cyan","by":40}""");

        Assert.False(result.IsError, result.TextContent);
        Assert.Contains("cx=420 cy=300 w=400 h=340", result.TextContent);
    }

    [Fact]
    public async Task Edge_and_by_cannot_be_mixed_with_the_other_fields()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");

        var withDelta = await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","edge":"cyan","by":20,"dh":-10}""");
        Assert.True(withDelta.IsError);
        Assert.Contains("dh", withDelta.TextContent);

        var withAbsolute = await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","edge":"cyan","by":20,"width":500}""");
        Assert.True(withAbsolute.IsError);
        Assert.Contains("width", withAbsolute.TextContent);
    }

    [Fact]
    public async Task Half_a_pair_and_a_colour_that_is_not_an_edge_are_refused_by_name()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");

        var noBy = await tools["geom_box"].ExecuteAsync("""{"name":"bag","edge":"cyan"}""");
        Assert.True(noBy.IsError);
        Assert.Contains("outward", noBy.TextContent);

        var noEdge = await tools["geom_box"].ExecuteAsync("""{"name":"bag","by":20}""");
        Assert.True(noEdge.IsError);
        Assert.Contains("'cyan' (top)", noEdge.TextContent);

        var wrongColour = await tools["geom_box"].ExecuteAsync("""{"name":"bag","edge":"red","by":20}""");
        Assert.True(wrongColour.IsError);
        Assert.Contains("'red' is not an edge", wrongColour.TextContent);

        var noBox = await tools["geom_box"].ExecuteAsync("""{"name":"ghost","edge":"cyan","by":20}""");
        Assert.True(noBox.IsError);
        Assert.Contains("no box called 'ghost'", noBox.TextContent);
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

        // And the box did not move. Looking is geom_view's job: a zero delta asks for nothing,
        // so it cannot double as a way to re-render.
        var look = await tools["geom_view"].ExecuteAsync("""{"to":"current"}""");
        Assert.Contains("cx=400", look.TextContent);
    }

    /// <summary>The exact call a live model deadlocked on. StrictSchema forces every property to be
    /// sent, so the fields it is not using arrive as 0 and "null" — read those as intent and the
    /// mutually exclusive groups become impossible to satisfy, which is what happened: the same call
    /// was refused five times until the loop guard stopped the turn.</summary>
    [Fact]
    public async Task Unused_fields_sent_as_zero_because_the_schema_demands_them_are_not_intent()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_box"].ExecuteAsync(
            """
            {"name":"bag","cx":375,"cy":500,"width":640,"height":1900,"angle":-2,
             "dx":0,"dy":0,"dw":0,"dh":0,"dangle":0,"edge":"null","by":0,"delete":false}
            """);

        Assert.False(result.IsError);
        Assert.Contains("created 'bag'", result.TextContent);
        Assert.Contains("cx=375", result.TextContent);
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

    /// <summary>The loop's worst failure mode, pinned: a box larger than the view has edges the model
    /// cannot see, and in a live run that produced six identical "still cut off" corrections while the
    /// box grew past the picture. The reply now says which edges are off-screen — as a note, because a
    /// box that really runs off the frame (a sack filling the photograph) is correctly marked so and
    /// must never be refused.</summary>
    [Fact]
    public async Task A_box_wider_than_the_view_is_kept_and_the_hidden_edges_are_named()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);   // 800x600

        var result = await tools["geom_box"].ExecuteAsync(
            """{"name":"mark","cx":400,"cy":300,"width":1000,"height":200}""");

        Assert.False(result.IsError, result.TextContent);
        Assert.Single(result.Content.OfType<ToolImage>());
        Assert.Contains("created 'mark'", result.TextContent);
        Assert.Contains("'mark' is wider than this view (1000 > 800 across)", result.TextContent);
        Assert.Contains("left and right edges are off-screen", result.TextContent);
        Assert.Contains("geom_view {to:'mark'}", result.TextContent);
    }

    [Fact]
    public async Task A_box_taller_and_wider_than_the_view_names_all_four_edges()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_box"].ExecuteAsync(
            """{"name":"sack","cx":400,"cy":300,"width":900,"height":700}""");

        Assert.False(result.IsError, result.TextContent);
        Assert.Contains("wider and taller than this view (900 > 800 across, 700 > 600 down)", result.TextContent);
        Assert.Contains("left, right, top and bottom edges are off-screen", result.TextContent);
    }

    [Fact]
    public async Task A_box_that_fits_says_nothing_about_edges()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");

        Assert.DoesNotContain("off-screen", result.TextContent);
    }

    /// <summary>The mirror case. Accuracy in this plugin comes from working zoomed; a box that covers a
    /// few percent of the picture is being placed by eye at a scale the model cannot judge, so the
    /// reply says so and names the call that fixes it — with the magnification it would actually get.
    /// </summary>
    [Fact]
    public async Task A_box_small_in_the_view_is_told_that_zooming_would_show_it_larger()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);   // 800x600, so 15% of the shorter side is 90 px

        var result = await tools["geom_box"].ExecuteAsync(
            """{"name":"mark","cx":400,"cy":300,"width":60,"height":30}""");

        Assert.False(result.IsError, result.TextContent);
        Assert.Contains("'mark' is small in this view (60x30 px of 800x600)", result.TextContent);
        Assert.Contains("geom_view {to:'mark'}", result.TextContent);
        Assert.Contains("larger", result.TextContent);
    }

    [Fact]
    public async Task A_box_filling_a_reasonable_share_of_the_view_is_not_nudged()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");

        Assert.DoesNotContain("is small in this view", result.TextContent);
    }
}
