using SkiaSharp;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry;
using System.Text.Json;

namespace SPLA.Tests.Geometry;

/// <summary>
/// The tools as the model meets them: well-formed metadata, a clear refusal instead of an exception
/// when there is no frame open, and the loop itself.
/// </summary>
public sealed class GeometryToolsTests
{
    private static AgentSession Chat() => new(new KeyValueStore("session"), new MarkManager(), new SkillSession());

    private static Dictionary<string, IMcpTool> Tools() =>
        new GeometryPlugin().Initialize(new ResolvedSettings()).ToDictionary(t => t.Name);

    /// <summary>A frame with something in it to mark: a pale slab on a dark ground.</summary>
    internal static byte[] Frame(int width = 800, int height = 600)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(new SKColor(0x30, 0x30, 0x30));
        using var paint = new SKPaint { Color = new SKColor(0xC0, 0xB0, 0x90), Style = SKPaintStyle.Fill };
        canvas.DrawRect(new SKRect(200, 150, 600, 450), paint);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>Puts a frame in the chat's blob store and gives back the handle geom_open takes.</summary>
    internal static string Handle(AgentSession chat, byte[]? bytes = null) =>
        chat.Blobs.Put(BlobPayload.OfBytes(bytes ?? Frame(), "image/png"), name: null);

    internal static async Task<ToolResult> Call(IMcpTool tool, string args)
        => await tool.ExecuteAsync(args);

    [Fact]
    public void Every_tool_is_uniquely_named_and_well_formed()
    {
        var tools = new GeometryPlugin().Initialize(new ResolvedSettings()).ToList();

        Assert.NotEmpty(tools);
        Assert.Equal(tools.Count, tools.Select(t => t.Name).Distinct().Count());

        foreach (var tool in tools)
        {
            var def = tool.GetDefinition();
            Assert.Equal(tool.Name, def.Function.Name);
            Assert.StartsWith("geom_", def.Function.Name);
            Assert.False(string.IsNullOrWhiteSpace(def.Function.Description));
            Assert.Equal(ToolScope.Project, def.Function.Scope);
            Assert.True(def.Function.StrictSchema);

            var json = JsonSerializer.Serialize(def.Function.Parameters);
            using var doc = JsonDocument.Parse(json);
            Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);

            // Strict schemas: every declared property is also listed as required, or the provider
            // refuses the grammar.
            var properties = doc.RootElement.GetProperty("properties").EnumerateObject().Select(p => p.Name).ToHashSet();
            var required = doc.RootElement.GetProperty("required").EnumerateArray().Select(e => e.GetString()!).ToHashSet();
            Assert.Equal(properties, required);
            Assert.False(doc.RootElement.GetProperty("additionalProperties").GetBoolean());
        }
    }

    [Fact]
    public async Task Every_tool_says_so_plainly_when_no_chat_session_is_active()
    {
        foreach (var tool in Tools().Values)
        {
            var result = await Call(tool, "{}");
            Assert.True(result.IsError);
            Assert.Contains("chat session", result.TextContent);
        }
    }

    [Fact]
    public async Task Geom_open_reads_a_blob_handle_and_answers_with_a_picture()
    {
        var chat = Chat();
        using var _ = AgentSessionScope.Begin(chat);
        var handle = Handle(chat);

        var result = await Call(Tools()["geom_open"], $$"""{"image":"{{handle}}"}""");

        Assert.False(result.IsError);
        Assert.Single(result.Content.OfType<ToolImage>());
        Assert.Contains("800x600", result.TextContent);      // the source size, stated once
        Assert.Contains("view: source", result.TextContent);
        Assert.Contains("ALL coordinates you pass are in this 800x600 space.", result.TextContent);
        Assert.Contains("objects here: none", result.TextContent);
    }

    [Fact]
    public async Task Geom_open_states_the_working_size_not_the_file_size()
    {
        // The picture is what the model measures in, so a large frame must announce the size it was
        // actually delivered at — this is the invariant the whole plugin rests on.
        var chat = Chat();
        using var _ = AgentSessionScope.Begin(chat);
        var handle = Handle(chat, Frame(4000, 2000));

        var result = await Call(Tools()["geom_open"], $$"""{"image":"{{handle}}"}""");

        Assert.Contains("source image 4000x2000 px", result.TextContent);
        Assert.Contains("ALL coordinates you pass are in this 1024x512 space.", result.TextContent);

        var image = result.Content.OfType<ToolImage>().Single();
        using var decoded = SKBitmap.Decode(Convert.FromBase64String(image.Data));
        Assert.Equal(1024, decoded.Width);
        Assert.Equal(512, decoded.Height);
    }

    [Fact]
    public async Task Geom_open_explains_an_address_it_cannot_read()
    {
        var chat = Chat();
        using var _ = AgentSessionScope.Begin(chat);

        var missing = await Call(Tools()["geom_open"], """{"image":"blob:nothing-here"}""");
        Assert.True(missing.IsError);
        Assert.Contains("nothing-here", missing.TextContent);

        var notAnImage = await Call(Tools()["geom_open"],
            $$"""{"image":"{{chat.Blobs.Put(BlobPayload.OfBytes([1, 2, 3, 4], "application/octet-stream"))}}"}""");
        Assert.True(notAnImage.IsError);
        Assert.Contains("decode", notAnImage.TextContent);

        var nothing = await Call(Tools()["geom_open"], "{}");
        Assert.True(nothing.IsError);
        Assert.Contains("image is required", nothing.TextContent);
    }

    [Fact]
    public async Task Invalid_json_is_an_answer_not_an_exception()
    {
        var chat = Chat();
        using var _ = AgentSessionScope.Begin(chat);

        var result = await Call(Tools()["geom_open"], "not json at all");
        Assert.True(result.IsError);
        Assert.Contains("invalid JSON", result.TextContent);
    }
}
