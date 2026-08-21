using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPLA.Domain.Agent;
using SPLA.Domain.Formats;
using SPLA.Domain.Resources;
using SPLA.Domain.Settings;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Formats;
using SPLA.MCP.Core.Tools;
using Xunit;
using YamlDotNet.Serialization;

namespace SPLA.Tests;

/// <summary>A registry with the host's day-one projections in it — what the runtime builds at startup.</summary>
public static class TestConverters
{
    public static FormatConverterRegistry Registry()
    {
        var registry = FormatConverterRegistry.For(new ResolvedSettings());
        BuiltInConverters.RegisterInto(registry);
        return registry;
    }
}

/// <summary>
/// The three built-in converters, and the one live consumer wired onto them. Identity proves the
/// lookup runs on the commonest call; the UTF-8 decoder proves a conversion that cannot be done says
/// so; json→yaml proves a real reshape survives the contract.
/// </summary>
public sealed class BuiltInConvertersTests
{
    private static ResourceContent Content(byte[] bytes, string type) => new(bytes, type);

    // ── identity ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Identity_returns_the_same_bytes_and_the_same_type()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3 };
        var result = await new IdentityConverter().ConvertAsync(Content(bytes, "image/png"), null);

        Assert.Same(bytes, result.Bytes);
        Assert.Equal("image/png", result.ContentType);
    }

    [Fact]
    public async Task Identity_preserves_the_source_type_rather_than_imposing_one()
    {
        var result = await new IdentityConverter().ConvertAsync(Content(new byte[] { 0xFF, 0xD8 }, "image/jpeg"), null);
        Assert.Equal("image/jpeg", result.ContentType);
    }

    [Fact]
    public void Identity_is_reached_through_the_lookup_like_any_other_pair()
    {
        var registry = TestConverters.Registry();

        Assert.True(registry.TryResolve("image/png", "image/png", out var converter, out _));
        Assert.IsType<IdentityConverter>(converter);
    }

    // ── UTF-8 text ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Utf8_converter_turns_good_bytes_into_text()
    {
        var bytes = Encoding.UTF8.GetBytes("привет, world");
        var result = await new Utf8TextConverter("text/*").ConvertAsync(Content(bytes, "text/plain"), null);

        Assert.Equal(ContentTypes.Text, result.ContentType);
        Assert.Equal("привет, world", Encoding.UTF8.GetString(result.Bytes));
    }

    [Fact]
    public async Task Utf8_converter_fails_loudly_on_bytes_that_are_not_text()
    {
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
        var converter = new Utf8TextConverter(ContentTypes.Unknown);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => converter.ConvertAsync(Content(png, "image/png"), null));

        Assert.Contains("UTF-8", ex.Message);
        Assert.Contains("binary", ex.Message);
        Assert.Contains("image/png", ex.Message);
    }

    [Fact]
    public void Both_text_sources_are_registered_on_the_one_implementation()
    {
        var registry = TestConverters.Registry();

        Assert.True(registry.TryResolve("text/markdown", ContentTypes.Text, out var fromText, out _));
        Assert.True(registry.TryResolve(ContentTypes.Unknown, ContentTypes.Text, out var fromBytes, out _));
        Assert.IsType<Utf8TextConverter>(fromText);
        Assert.IsType<Utf8TextConverter>(fromBytes);
    }

    // ── json → yaml ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Json_becomes_yaml_with_the_same_values()
    {
        var json = Encoding.UTF8.GetBytes("""{"name":"spla","count":3,"ok":true,"tags":["a","b"]}""");
        var result = await new JsonToYamlConverter().ConvertAsync(Content(json, "application/json"), null);

        Assert.Equal("application/yaml", result.ContentType);

        var back = new DeserializerBuilder().Build()
            .Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(result.Bytes));

        Assert.Equal("spla", back["name"]);
        Assert.Equal("3", back["count"]!.ToString());
        Assert.Equal("true", back["ok"]!.ToString()!.ToLowerInvariant());
        Assert.Equal(new List<object> { "a", "b" }, back["tags"]);
    }

    [Fact]
    public async Task Malformed_json_fails_and_says_where()
    {
        var broken = Encoding.UTF8.GetBytes("""{"name": "spla", }""");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new JsonToYamlConverter().ConvertAsync(Content(broken, "application/json"), null));

        Assert.Contains("not valid JSON", ex.Message);
        Assert.Contains("line", ex.Message);
        Assert.Contains("position", ex.Message);
    }

    // ── the live consumer ────────────────────────────────────────────────────

    [Fact]
    public async Task Image_view_refuses_a_pdf_by_naming_what_that_type_can_reach()
    {
        var session = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        using var _ = AgentSessionScope.Begin(session);

        // %PDF — sniffed, so nobody has to declare it.
        var handle = session.Blobs.Put(BlobPayload.OfBytes(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31 }));

        var result = await new ImageViewTool(TestConverters.Registry())
            .ExecuteAsync($$"""{"handle":"{{handle}}"}""");

        Assert.NotEqual(ToolOutcome.Ok, result.Outcome);
        Assert.Contains("No conversion is registered from 'application/pdf' to 'image/png'", result.TextContent);
        Assert.Contains("Nothing can be produced from 'application/pdf' at all", result.TextContent);
        Assert.DoesNotContain("not a viewable image", result.TextContent);
        Assert.Empty(session.Images.DrainAll());
    }

    /// <summary>The regression that matters most: an ordinary screenshot goes through the registry and
    /// comes out exactly as it went in.</summary>
    [Fact]
    public async Task Image_view_still_shows_a_png_unchanged()
    {
        var session = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        using var _ = AgentSessionScope.Begin(session);

        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 9, 9 };
        var handle = session.Blobs.Put(BlobPayload.OfBytes(bytes, "image/png"));

        var result = await new ImageViewTool(TestConverters.Registry())
            .ExecuteAsync($$"""{"handle":"{{handle}}"}""");

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        var image = Assert.Single(result.Content.OfType<ToolImage>());
        Assert.Equal("image/png", image.MimeType);
        Assert.Equal(Convert.ToBase64String(bytes), image.Data);
    }
}
