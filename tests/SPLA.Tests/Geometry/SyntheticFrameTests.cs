using SkiaSharp;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry;
using System.Globalization;
using System.Text.Json;

namespace SPLA.Tests.Geometry;

/// <summary>
/// The check the rest of the suite cannot make: do the numbers mean anything in an actual picture?
/// <para>
/// Every other geometry test compares the pipeline with itself. Here the frame is drawn from a
/// declared ground truth, and the test asks two separate questions of it — does the coordinate space
/// the model is told about land on the pixels it is shown, and does the loop hand back, in source
/// pixels, the numbers the frame was drawn from.
/// </para>
/// </summary>
public sealed class SyntheticFrameTests
{
    private static (AgentSession Chat, Dictionary<string, IMcpTool> Tools, IDisposable Scope) Begin()
    {
        var chat = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        var scope = AgentSessionScope.Begin(chat);
        var tools = new GeometryPlugin().Initialize(new ResolvedSettings()).ToDictionary(t => t.Name);
        return (chat, tools, scope);
    }

    /// <summary>The picture a tool just handed the model, decoded back into pixels.</summary>
    private static SKBitmap Picture(ToolResult result)
    {
        var image = Assert.Single(result.Content.OfType<ToolImage>());
        return SKBitmap.Decode(Convert.FromBase64String(image.Data));
    }

    private static async Task<ToolResult> Open(AgentSession chat, Dictionary<string, IMcpTool> tools, byte[] png)
    {
        var handle = chat.Blobs.Put(BlobPayload.OfBytes(png, "image/png"), name: null);
        var opened = await tools["geom_open"].ExecuteAsync($$"""{"image":"{{handle}}"}""");
        Assert.False(opened.IsError, opened.TextContent);
        return opened;
    }

    [Fact]
    public async Task The_declared_coordinate_space_lands_on_the_pixels_the_model_is_shown()
    {
        // A transform chain can be perfectly self-consistent and still be shifted or scaled relative
        // to the picture. The only way to catch that is to look at the picture: take a source point
        // whose colour is known by construction, map it into the view the way the model is told the
        // view works, and read the pixel there.
        var frame = SyntheticFixtures.Standard();
        var (chat, tools, scope) = Begin();
        using var _scope = scope;

        var opened = await Open(chat, tools, frame.Png);
        // The probes below read a pixel's colour, and a grid line over a probe would answer for it, so
        // this one frame is taken bare. Nothing here is about the grid.
        var bare = await tools["geom_view"].ExecuteAsync("""{"to":"current","grid":false}""");
        Assert.False(bare.IsError, bare.TextContent);
        using var picture = Picture(bare);

        // The root view is the source scaled to render_max_side; the reply says so in as many words.
        var scale = 1024.0 / frame.Width;
        Assert.Equal(1024, picture.Width);
        Assert.Equal((int)Math.Round(frame.Height * scale), picture.Height);
        Assert.Contains($"ALL coordinates you pass are in this {picture.Width}x{picture.Height} space.",
            opened.TextContent);

        // The bag's centre, in the space the model was just told to speak in, is on the bag.
        Assert.True(
            SyntheticFixtures.IsColorAt(picture, frame.BagCx * scale, frame.BagCy * scale, SyntheticFixtures.BagColor),
            "the bag's centre does not land on a bag-coloured pixel: the view space is offset or scaled " +
            "relative to the picture the model is shown.");

        // The marking, a small thing well away from that centre, is where it was drawn too.
        Assert.True(
            SyntheticFixtures.IsColorAt(picture, frame.MarkCx * scale, frame.MarkCy * scale, SyntheticFixtures.MarkColor),
            "the marking is not where its source coordinates say it is.");

        // And a point well outside the bag is not on it — otherwise the assertion above would pass on
        // a picture that was bag-coloured everywhere.
        Assert.False(
            SyntheticFixtures.IsColorAt(picture, 20, 20, SyntheticFixtures.BagColor),
            "the frame's corner reads as bag: the probe proves nothing.");
        Assert.True(SyntheticFixtures.IsColorAt(picture, 20, 20, SyntheticFixtures.Background));
    }

    [Fact]
    public async Task The_loop_recovers_the_ground_truth_the_frame_was_drawn_from()
    {
        // open -> box on the outer rectangle -> view into it, deskewed -> point on the crosshair ->
        // result. Nothing here is stated in source pixels except the answer, which is compared with
        // what the frame was drawn from.
        var frame = SyntheticFixtures.Standard();
        var (chat, tools, scope) = Begin();
        using var _scope = scope;

        await Open(chat, tools, frame.Png);
        var scale = 1024.0 / frame.Width;

        var placed = await tools["geom_box"].ExecuteAsync(
            $$"""
            {"name":"bag","cx":{{N(frame.BagCx * scale)}},"cy":{{N(frame.BagCy * scale)}},
             "width":{{N(frame.BagWidth * scale)}},"height":{{N(frame.BagHeight * scale)}},"angle":{{N(frame.BagAngle)}}}
            """);
        Assert.False(placed.IsError, placed.TextContent);

        var zoomed = await tools["geom_view"].ExecuteAsync("""{"to":"bag","padding":0,"deskew":true}""");
        Assert.False(zoomed.IsError, zoomed.TextContent);

        // Where is the crosshair in the crop? Ask the rendered pixels, not the arithmetic: finding it
        // by colour is the one way of locating it that shares no code with what is being tested.
        using var crop = Picture(zoomed);
        var found = SyntheticFixtures.Centroid(crop, SyntheticFixtures.CrossColor);
        Assert.True(found is not null, "the crosshair is not visible in the deskewed crop of the bag.");
        var (crossViewX, crossViewY) = found!.Value;

        var pointed = await tools["geom_point"].ExecuteAsync(
            $$"""{"name":"mark","x":{{N(crossViewX)}},"y":{{N(crossViewY)}}}""");
        Assert.False(pointed.IsError, pointed.TextContent);

        using var json = JsonDocument.Parse((await tools["geom_result"].ExecuteAsync("{}")).TextContent);
        var objects = json.RootElement.GetProperty("objects");

        var bag = objects[0].GetProperty("coordinates").GetProperty("source");
        Assert.Equal(frame.BagCx, bag.GetProperty("cx").GetDouble(), 1.0);
        Assert.Equal(frame.BagCy, bag.GetProperty("cy").GetDouble(), 1.0);
        Assert.Equal(frame.BagWidth, bag.GetProperty("width").GetDouble(), 1.0);
        Assert.Equal(frame.BagHeight, bag.GetProperty("height").GetDouble(), 1.0);
        Assert.Equal(frame.BagAngle, bag.GetProperty("angle").GetDouble(), 0.01);

        // The crosshair was never named in source pixels by anyone: it was located in a rotated,
        // magnified crop and carried back through two transforms. It lands where it was drawn.
        var mark = objects[1].GetProperty("coordinates").GetProperty("source");
        Assert.Equal(frame.CrossX, mark.GetProperty("x").GetDouble(), 1.0);
        Assert.Equal(frame.CrossY, mark.GetProperty("y").GetDouble(), 1.0);
    }

    [Fact]
    public async Task The_frames_for_the_live_check_are_written_out_for_a_human_to_look_at()
    {
        // The live check against a real model cannot run in CI, and pretending otherwise would be
        // worse than not running it. What can be automated is the preparation: the exact pictures the
        // owner should feed LM Studio, including the gridded one that reveals a provider quietly
        // downscaling the image out from under render_max_side.
        var frame = SyntheticFixtures.Standard();
        var (chat, tools, scope) = Begin();
        using var _scope = scope;

        var directory = Path.Combine(Path.GetTempPath(), "spla-geometry-fixtures");
        Directory.CreateDirectory(directory);

        var opened = await Open(chat, tools, frame.Png);
        var gridded = await tools["geom_view"].ExecuteAsync("""{"to":"current","grid":true}""");
        Assert.False(gridded.IsError, gridded.TextContent);

        await tools["geom_box"].ExecuteAsync(
            $$"""
            {"name":"bag","cx":{{N(frame.BagCx * 0.64)}},"cy":{{N(frame.BagCy * 0.64)}},
             "width":{{N(frame.BagWidth * 0.64)}},"height":{{N(frame.BagHeight * 0.64)}},"angle":{{N(frame.BagAngle)}}}
            """);
        var zoomed = await tools["geom_view"].ExecuteAsync("""{"to":"bag","padding":0,"deskew":true}""");

        var files = new (string Name, byte[] Bytes)[]
        {
            ("source.png", frame.Png),
            ("open.png", Bytes(opened)),
            ("grid.png", Bytes(gridded)),
            ("crop-deskewed.png", Bytes(zoomed)),
        };

        foreach (var (name, bytes) in files)
        {
            var path = Path.Combine(directory, name);
            await File.WriteAllBytesAsync(path, bytes);
            Assert.True(new FileInfo(path).Length > 0);
        }

        // The truth the human is checking the model's answers against.
        await File.WriteAllTextAsync(Path.Combine(directory, "ground-truth.json"), JsonSerializer.Serialize(
            new
            {
                source = new { width = frame.Width, height = frame.Height },
                root_view = new { width = 1024, height = 768 },
                bag = new
                {
                    cx = frame.BagCx, cy = frame.BagCy,
                    width = frame.BagWidth, height = frame.BagHeight, angle = frame.BagAngle,
                    in_root_view = new { cx = frame.BagCx * 0.64, cy = frame.BagCy * 0.64 },
                },
                mark = new { cx = frame.MarkCx, cy = frame.MarkCy },
                crosshair = new { x = frame.CrossX, y = frame.CrossY },
            },
            new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>A number as JSON sees it, whatever the machine's culture thinks a decimal point is.</summary>
    private static string N(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    private static byte[] Bytes(ToolResult result) =>
        Convert.FromBase64String(Assert.Single(result.Content.OfType<ToolImage>()).Data);
}
