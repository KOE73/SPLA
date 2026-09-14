using SkiaSharp;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry;

namespace SPLA.Tests.Geometry;

/// <summary>
/// Moving between views is where the model would lose track if the tool let it: the numbers it is
/// shown and the numbers it sends must both be in the picture in front of it, and nothing marked
/// earlier may move because of a change of view.
/// </summary>
public sealed class GeometryViewToolTests
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
    public async Task Neither_a_target_nor_a_rectangle_is_refused()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_view"].ExecuteAsync("{}");
        Assert.True(result.IsError);
        Assert.Contains("'rect'", result.TextContent);

        var malformed = await tools["geom_view"].ExecuteAsync("""{"rect":[10,20,30]}""");
        Assert.True(malformed.IsError);
        Assert.Contains("four numbers", malformed.TextContent);
    }

    [Fact]
    public async Task Zooming_into_a_box_restates_the_coordinate_space_and_the_box_within_it()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);   // 800x600, delivered as 800x600

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        var zoomed = await tools["geom_view"].ExecuteAsync("""{"to":"bag","padding":0}""");

        Assert.False(zoomed.IsError, zoomed.TextContent);
        Assert.Contains("zoomed into 'bag'", zoomed.TextContent);
        Assert.Contains("view: crop_1", zoomed.TextContent);
        Assert.Contains("from box 'bag'", zoomed.TextContent);

        // The crop fills the working size: 400x300 source px become 1024x768.
        Assert.Contains("ALL coordinates you pass are in this 1024x768 space.", zoomed.TextContent);
        Assert.Contains("cx=512 cy=384 w=1024 h=768", zoomed.TextContent);

        var image = zoomed.Content.OfType<ToolImage>().Single();
        using var decoded = SKBitmap.Decode(Convert.FromBase64String(image.Data));
        Assert.Equal(1024, decoded.Width);
        Assert.Equal(768, decoded.Height);
    }

    [Fact]
    public async Task An_object_marked_in_a_crop_keeps_its_place_when_the_view_goes_back_out()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        await tools["geom_view"].ExecuteAsync("""{"to":"bag","padding":0}""");

        // In the 1024x768 crop, a quarter across and a quarter down.
        await tools["geom_point"].ExecuteAsync("""{"name":"mark","x":256,"y":192}""");

        var back = await tools["geom_view"].ExecuteAsync("""{"to":"source"}""");
        Assert.Contains("view: source", back.TextContent);

        // Source frame: the crop covered x 200..600, y 150..450, so a quarter in is (300, 225).
        Assert.Contains("x=300 y=225", back.TextContent);
    }

    [Fact]
    public async Task Parent_and_current_move_without_creating_new_views()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        await tools["geom_view"].ExecuteAsync("""{"to":"bag"}""");

        var again = await tools["geom_view"].ExecuteAsync("""{"to":"current"}""");
        Assert.Contains("redrawn crop_1", again.TextContent);

        var out1 = await tools["geom_view"].ExecuteAsync("""{"to":"parent"}""");
        Assert.Contains("view: source", out1.TextContent);

        var out2 = await tools["geom_view"].ExecuteAsync("""{"to":"parent"}""");
        Assert.Contains("already at the whole image", out2.TextContent);

        // Back into the crop by its id — views are kept, not rebuilt.
        var backIn = await tools["geom_view"].ExecuteAsync("""{"to":"crop_1"}""");
        Assert.Contains("back to crop_1", backIn.TextContent);
        Assert.DoesNotContain("crop_2", backIn.TextContent);
    }

    [Fact]
    public async Task A_rectangle_is_read_in_the_pixels_of_the_picture_just_shown()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_view"].ExecuteAsync("""{"rect":[400,300,200,150]}""");
        // The bottom-right quarter of the frame, enlarged: mark its centre.
        await tools["geom_point"].ExecuteAsync("""{"name":"mark","x":512,"y":384}""");

        var back = await tools["geom_view"].ExecuteAsync("""{"to":"source"}""");
        Assert.Contains("x=500 y=375", back.TextContent);
    }

    [Fact]
    public async Task Deskew_turns_an_angled_box_upright_in_the_picture()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":200,"angle":30}""");
        var straight = await tools["geom_view"].ExecuteAsync("""{"to":"bag","padding":0,"deskew":true}""");

        Assert.Contains("deskewed", straight.TextContent);
        Assert.Contains("angle=0", straight.TextContent);

        // And the bag's own tilt is untouched back in the source.
        var back = await tools["geom_view"].ExecuteAsync("""{"to":"source"}""");
        Assert.Contains("angle=+30 (tilted down to the right)", back.TextContent);
    }

    [Fact]
    public async Task An_unknown_target_says_what_it_would_have_accepted()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var result = await tools["geom_view"].ExecuteAsync("""{"to":"nothing_like_this"}""");

        Assert.True(result.IsError);
        Assert.Contains("nothing_like_this", result.TextContent);
        Assert.Contains("'source'", result.TextContent);
    }

    [Fact]
    public async Task An_object_out_of_the_picture_is_listed_rather_than_forgotten()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_point"].ExecuteAsync("""{"name":"corner","x":40,"y":40}""");
        var zoomed = await tools["geom_view"].ExecuteAsync("""{"rect":[500,400,200,150]}""");

        Assert.Contains("outside this view: corner", zoomed.TextContent);
    }

    /// <summary>Straight from a live log: every geometry tool declares StrictSchema, so the model must
    /// send every property, and a small one fills the unused ones with the four characters "null"
    /// instead of a JSON null. It worked there only because nothing read 'to'. Every optional string
    /// argument now reads the placeholder as absent.</summary>
    [Fact]
    public async Task The_string_null_counts_as_not_supplied()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var cropped = await tools["geom_view"].ExecuteAsync("""{"to":"null","rect":[100,100,200,150]}""");

        Assert.False(cropped.IsError, cropped.TextContent);
        Assert.Contains("viewing rectangle [100, 100, 200, 150]", cropped.TextContent);

        // With nothing else to go on it is the missing argument it stands for, not an object name.
        var nothing = await tools["geom_view"].ExecuteAsync("""{"to":"null","rect":null}""");
        Assert.True(nothing.IsError);
        Assert.DoesNotContain("nothing called 'null'", nothing.TextContent);
        Assert.Contains("'rect'", nothing.TextContent);
    }

    /// <summary>The edge rulers are an instrument the model picks up mid-task: asked for once, they
    /// stay on while the model works, and a fresh crop inherits them from the view the model came
    /// from — the same rule the view grid follows.</summary>
    [Fact]
    public async Task A_new_crop_inherits_the_edge_rulers()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var on = await tools["geom_view"].ExecuteAsync("""{"to":"current","edge_rulers":true}""");
        Assert.False(on.IsError, on.TextContent);

        var session = SPLA.Plugins.Geometry.Session.GeometrySessionRegistry.TryGet(chat);
        Assert.NotNull(session);
        Assert.True(session!.CurrentView.EdgeRulers);

        var cropped = await tools["geom_view"].ExecuteAsync("""{"rect":[100,100,200,150]}""");
        Assert.False(cropped.IsError, cropped.TextContent);
        Assert.True(session.CurrentView.EdgeRulers);
        Assert.NotEqual("source", session.CurrentView.Id);

        var off = await tools["geom_view"].ExecuteAsync("""{"to":"current","edge_rulers":false}""");
        Assert.False(off.IsError, off.TextContent);
        Assert.False(session.CurrentView.EdgeRulers);
    }

}
