using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry;
using System.Text.Json;

namespace SPLA.Tests.Geometry;

/// <summary>
/// The result, and the whole loop that leads to it. The end-to-end test is the one that proves the
/// idea works: a model that only ever spoke in the pixels of the picture in front of it ends up with
/// correct numbers in the original image.
/// </summary>
public sealed class GeometryResultToolTests
{
    private static Dictionary<string, IMcpTool> Tools() =>
        new GeometryPlugin().Initialize(new ResolvedSettings()).ToDictionary(t => t.Name);

    private static (AgentSession Chat, Dictionary<string, IMcpTool> Tools, IDisposable Scope) Begin()
    {
        var chat = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        var scope = AgentSessionScope.Begin(chat);
        return (chat, Tools(), scope);
    }

    private static async Task OpenFrame(AgentSession chat, Dictionary<string, IMcpTool> tools, int width, int height)
    {
        var handle = GeometryToolsTests.Handle(chat, GeometryToolsTests.Frame(width, height));
        var opened = await tools["geom_open"].ExecuteAsync($$"""{"image":"{{handle}}"}""");
        Assert.False(opened.IsError, opened.TextContent);
    }

    private static JsonDocument Json(ToolResult result) => JsonDocument.Parse(result.TextContent);

    [Fact]
    public async Task Geom_result_is_the_one_tool_that_returns_no_picture()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools, 800, 600);
        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");

        var result = await tools["geom_result"].ExecuteAsync("{}");

        Assert.False(result.IsError);
        Assert.Empty(result.Content.OfType<ToolImage>());
        using var json = Json(result);
        Assert.Equal("box", json.RootElement.GetProperty("objects")[0].GetProperty("kind").GetString());
    }

    [Fact]
    public async Task Every_box_carries_its_corners_in_source_coordinates()
    {
        // Angle conventions differ between libraries; four corners cannot be misread.
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools, 800, 600);
        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":200,"height":100,"angle":0}""");

        using var json = Json(await tools["geom_result"].ExecuteAsync("{}"));
        var corners = json.RootElement.GetProperty("objects")[0]
            .GetProperty("coordinates").GetProperty("source").GetProperty("corners");

        Assert.Equal(4, corners.GetArrayLength());
        Assert.Equal(300, corners[0][0].GetDouble(), 1);   // top-left x
        Assert.Equal(250, corners[0][1].GetDouble(), 1);   // top-left y
        Assert.Equal(500, corners[2][0].GetDouble(), 1);   // bottom-right x
        Assert.Equal(350, corners[2][1].GetDouble(), 1);
    }

    [Fact]
    public async Task Output_blob_hands_the_json_on_without_inlining_it()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools, 800, 600);
        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":200,"height":100}""");

        var result = await tools["geom_result"].ExecuteAsync("""{"output":"blob","output_name":"markup"}""");

        Assert.Contains("blob:markup", result.TextContent);
        Assert.DoesNotContain("\"corners\"", result.TextContent);

        var stored = chat.Blobs.Get("blob:markup");
        Assert.NotNull(stored);
        using var json = JsonDocument.Parse(stored!.Text!);
        Assert.Equal("bag", json.RootElement.GetProperty("objects")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task The_whole_loop_lands_on_the_right_numbers_in_the_source_image()
    {
        // open -> box -> box(delta) -> accept -> view(to:box) -> point -> accept -> result.
        // The frame is 1600x1200, so the root view is 1024x768 and the model never once sees or
        // sends a source-image pixel value. What it gets right at the end is exactly what this
        // whole plugin is for.
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools, 1600, 1200);

        var opened = await tools["geom_view"].ExecuteAsync("""{"to":"current"}""");
        Assert.Contains("ALL coordinates you pass are in this 1024x768 space.", opened.TextContent);

        // A first guess, deliberately off, in the 1024x768 picture.
        var guess = await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","cx":500,"cy":380,"width":600,"height":400,"angle":0}""");
        Assert.Contains("created 'bag'", guess.TextContent);

        // Corrected by eye: +12 right, -4 up, a little wider and turned.
        var corrected = await tools["geom_box"].ExecuteAsync("""{"name":"bag","dx":12,"dy":-4,"dw":24,"dangle":10}""");
        Assert.Contains("cx=512 cy=376 w=624 h=400 angle=10", corrected.TextContent);

        await tools["geom_accept"].ExecuteAsync("""{"name":"bag"}""");

        // Zoom into the bag to place the marking on it, with no margin so the arithmetic is plain.
        var zoomed = await tools["geom_view"].ExecuteAsync("""{"to":"bag","padding":0,"deskew":true}""");
        Assert.Contains("deskewed", zoomed.TextContent);
        Assert.Contains("ALL coordinates you pass are in this 1024x656 space.", zoomed.TextContent);

        // Dead centre of the deskewed crop is the centre of the bag itself.
        await tools["geom_point"].ExecuteAsync("""{"name":"mark","x":512,"y":328}""");
        await tools["geom_accept"].ExecuteAsync("{}");

        using var json = Json(await tools["geom_result"].ExecuteAsync("{}"));
        var root = json.RootElement;

        Assert.Equal(1600, root.GetProperty("source").GetProperty("width").GetInt32());
        Assert.Equal(1200, root.GetProperty("source").GetProperty("height").GetInt32());

        var objects = root.GetProperty("objects");
        Assert.Equal(2, objects.GetArrayLength());

        // The bag, in source pixels: the root view scaled 1600 -> 1024, so view numbers scale by
        // 1600/1024 = 1.5625. cx 512 -> 800, cy 376 -> 587.5, w 624 -> 975, h 400 -> 625.
        var bag = objects[0].GetProperty("coordinates").GetProperty("source");
        Assert.Equal("accepted", objects[0].GetProperty("status").GetString());
        Assert.Equal(800, bag.GetProperty("cx").GetDouble(), 0.5);
        Assert.Equal(587.5, bag.GetProperty("cy").GetDouble(), 0.5);
        Assert.Equal(975, bag.GetProperty("width").GetDouble(), 0.5);
        Assert.Equal(625, bag.GetProperty("height").GetDouble(), 0.5);
        Assert.Equal(10, bag.GetProperty("angle").GetDouble(), 0.01);

        // Its corners are consistent with that centre, size and angle.
        var corners = bag.GetProperty("corners");
        Assert.Equal(4, corners.GetArrayLength());
        var centreX = Enumerable.Range(0, 4).Average(i => corners[i][0].GetDouble());
        var centreY = Enumerable.Range(0, 4).Average(i => corners[i][1].GetDouble());
        Assert.Equal(800, centreX, 0.5);
        Assert.Equal(587.5, centreY, 0.5);
        Assert.Equal(975, Distance(corners[0], corners[1]), 0.5);
        Assert.Equal(625, Distance(corners[1], corners[2]), 0.5);

        // The point was placed in the middle of a deskewed crop of the bag, so in source pixels it
        // is the bag's own centre — a value the model never named and could not have computed.
        var mark = objects[1].GetProperty("coordinates").GetProperty("source");
        Assert.Equal("mark", objects[1].GetProperty("name").GetString());
        Assert.Equal("accepted", objects[1].GetProperty("status").GetString());
        Assert.Equal(800, mark.GetProperty("x").GetDouble(), 1);
        Assert.Equal(587.5, mark.GetProperty("y").GetDouble(), 1);

        // And it is also reported in the crop it was placed in, where it reads as that crop's centre.
        var inCrop = objects[1].GetProperty("coordinates").GetProperty("crop_1");
        Assert.Equal(512, inCrop.GetProperty("x").GetDouble(), 1);
        Assert.Equal(328, inCrop.GetProperty("y").GetDouble(), 1);

        // The views are published with their transforms, deskew included.
        var views = root.GetProperty("views");
        Assert.Equal(2, views.GetArrayLength());
        Assert.Equal("source", views[0].GetProperty("id").GetString());
        Assert.True(views[1].GetProperty("deskewed").GetBoolean());
        Assert.Equal("bag", views[1].GetProperty("from_box").GetString());
    }

    private static double Distance(JsonElement a, JsonElement b)
    {
        double dx = a[0].GetDouble() - b[0].GetDouble(), dy = a[1].GetDouble() - b[1].GetDouble();
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
