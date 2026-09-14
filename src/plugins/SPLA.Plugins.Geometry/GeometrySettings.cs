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

    /// <summary>Whether renders carry the view grid unless a call says otherwise. On by default: the
    /// model cannot judge a distance by eye but reads coordinates off a grid accurately, so the grid
    /// is the ruler that makes the look → correct loop converge, not debug decoration.</summary>
    [YamlMember(Alias = "grid")]
    public bool Grid { get; set; } = true;

    /// <summary>Spacing of the fine grid lines, in view pixels.</summary>
    [YamlMember(Alias = "grid_step")]
    public int GridStep { get; set; } = 50;

    /// <summary>Every Nth line is drawn stronger and carries the coordinate label; the fine lines
    /// between it carry none. Labelling every fine line turns the picture into noise, and the picture
    /// is the thing being read.</summary>
    [YamlMember(Alias = "grid_major_every")]
    public int GridMajorEvery { get; set; } = 4;

    /// <summary>Grid colour as <c>#RRGGBB</c> or <c>#AARRGGBB</c>. The default is near-black: the
    /// leading subject is a grey-white sack, where a white grid is invisible, and it stays clear of
    /// the frozen edge palette (cyan/magenta/yellow/green) so a grid line can never be read as an
    /// edge. Fine lines are drawn at a fraction of this alpha, major lines at full.</summary>
    [YamlMember(Alias = "grid_color")]
    public string GridColor { get; set; } = "#141414";

    /// <summary>Whether the box being edited carries a grid along its <b>own</b> axes. Off by default:
    /// this is the experimental instrument. "Is the box tight on the thing" is a property relative to
    /// the box, and it is measured naturally in the box's own cells — "the text starts two cells in
    /// from the green edge" — which the view grid, in the view's axes, cannot say.</summary>
    [YamlMember(Alias = "box_grid")]
    public bool BoxGrid { get; set; }

    /// <summary>How many cells the box grid cuts each side into.</summary>
    [YamlMember(Alias = "box_grid_divisions")]
    public int BoxGridDivisions { get; set; } = 4;

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
        // A step below 10 px is a wash of lines at any working size, and one above a quarter of the
        // smallest sensible view stops being a ruler.
        GridStep = Clamp(GridStep, 10, 500);
        // 1 means every line is major — legal, and what a coarse step wants.
        GridMajorEvery = Clamp(GridMajorEvery, 1, 20);
        BoxGridDivisions = Clamp(BoxGridDivisions, 1, 20);
        if (string.IsNullOrWhiteSpace(GridColor)) GridColor = new GeometrySettings().GridColor;
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
