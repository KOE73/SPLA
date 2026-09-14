using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Plugins.Geometry;

/// <summary>
/// Plugin-owned settings (geometry section in .spla file under plugins.geometry.settings).
/// Values are clamped to sane ranges rather than rejected, the way <c>AndroidSettings</c> does it:
/// a hand-edited file degrades instead of breaking the plugin.
/// </summary>
public sealed class GeometrySettings
{
    private static readonly ISerializer Ser = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
    private static readonly IDeserializer De = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties().Build();

    /// <summary>Longest side of a rendered frame in pixels. This is also the coordinate space the
    /// model works in: every render is delivered at this working size and every coordinate the model
    /// passes is read in it.</summary>
    [YamlMember(Alias = "render_max_side")]
    public int RenderMaxSide { get; set; } = 1024;

    /// <summary>Outline thickness in render pixels.</summary>
    [YamlMember(Alias = "line_width")]
    public int LineWidth { get; set; } = 3;

    /// <summary>Point size of the object-name labels drawn next to each object.</summary>
    [YamlMember(Alias = "font_size")]
    public int FontSize { get; set; } = 16;

    /// <summary>Default margin around a box when a view is cropped to it, as a fraction of the
    /// box's size.</summary>
    [YamlMember(Alias = "crop_padding")]
    public double CropPadding { get; set; } = 0.05;

    /// <summary>0 renders to PNG; 1-100 renders to JPEG at that quality.</summary>
    [YamlMember(Alias = "jpeg_quality")]
    public int JpegQuality { get; set; } = 0;

    /// <summary>How many renders the chat's blob store keeps. Renders are written under the rotating
    /// names <c>geom_render_1..N</c>, so the store holds this many at most instead of one blob per
    /// step of the loop (a single frame's markup used to leave dozens of megabytes behind).</summary>
    [YamlMember(Alias = "render_history")]
    public int RenderHistory { get; set; } = 5;

    public static GeometrySettings FromBlob(Dictionary<string, object>? blob)
    {
        if (blob is null || blob.Count == 0) return new();
        var settings = De.Deserialize<GeometrySettings>(Ser.Serialize(blob)) ?? new();
        settings.Clamp();
        return settings;
    }

    private void Clamp()
    {
        RenderMaxSide = Clamp(RenderMaxSide, 256, 4096);
        LineWidth = Clamp(LineWidth, 1, 16);
        FontSize = Clamp(FontSize, 8, 48);
        CropPadding = CropPadding < 0 ? 0 : CropPadding > 1 ? 1 : CropPadding;
        RenderHistory = Clamp(RenderHistory, 1, 20);
        // 0 means PNG; any other value is a JPEG quality, and quality below 30 is not worth the
        // artefacts on a frame the model has to read geometry off, so it snaps up.
        JpegQuality = JpegQuality switch
        {
            <= 0 => 0,
            < 30 => 30,
            > 100 => 100,
            _ => JpegQuality,
        };
    }

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
}
