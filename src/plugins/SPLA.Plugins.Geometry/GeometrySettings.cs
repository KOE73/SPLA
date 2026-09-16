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

    /// <summary>Spacing of the fine grid lines, in view pixels. <c>0</c> — the default — means the
    /// spacing is derived from the size of the frame instead of being fixed: see
    /// <see cref="EffectiveGridStep"/>. A non-zero value pins the step and the frame's size stops
    /// mattering, which is what you want when comparing two renders line for line.</summary>
    [YamlMember(Alias = "grid_step")]
    public int GridStep { get; set; }

    /// <summary>The finest spacing the derived step is allowed to use, and the unit every derived
    /// step is a multiple of, in view pixels. Below this the grid stops measuring anything the model
    /// can act on: it reads the frame in patches of roughly this size, so lines closer together than
    /// a patch land inside one token and only cost legibility. Exposed so the threshold can be moved
    /// without a rebuild — 16 and 32 are both defensible and the difference is worth trying.</summary>
    [YamlMember(Alias = "grid_min_step")]
    public int GridMinStep { get; set; } = 32;

    /// <summary>How many fine lines the derived step aims to put across the frame's longer side. The
    /// count, not the spacing, is what stays constant as the frame grows or shrinks: a crop half the
    /// size gets half the spacing and the same grid to read.</summary>
    [YamlMember(Alias = "grid_lines")]
    public int GridLines { get; set; } = 24;

    /// <summary>How transparent the grid lines are, in percent: <c>0</c> is solid, <c>100</c>
    /// invisible. The default is a token 10 — the lines must read as lines. A faint instrument is no
    /// instrument: a line the model has to hunt for it will instead guess past, and a guessed
    /// distance is exactly what this whole picture exists to replace. Whatever the grid hides it
    /// hides; the answer to that is a coarser step, not a paler line.</summary>
    [YamlMember(Alias = "grid_transparency")]
    public int GridTransparency { get; set; } = 10;

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

    /// <summary>Whether the box being edited carries a ruler along each of its edges unless a call
    /// says otherwise. On by default, and for the same reason the view grid is: the correction the
    /// model writes is <c>edge</c>+<c>by</c> in pixels from an edge, and this is the only instrument
    /// that reads in that unit — it can transcribe a number instead of estimating one.</summary>
    [YamlMember(Alias = "edge_rulers")]
    public bool EdgeRulers { get; set; } = true;

    /// <summary>How transparent the ruler lines are, in percent, on the same scale as
    /// <see cref="GridTransparency"/>, and a token 10 for the same reason. The number plates are
    /// never faded at all — a line may be a hint, a digit is readable or useless.</summary>
    [YamlMember(Alias = "ruler_transparency")]
    public int RulerTransparency { get; set; } = 10;

    /// <summary>Whether the ruler lines carry their numbers. On by default, but switchable: the step
    /// is also stated in the reply's text, so a model that trusts the text can have the picture back
    /// clean — and a model that cannot read small digits off a blurred photograph loses nothing it
    /// was actually using. Which of those is true is a question for a live model, not for us.</summary>
    [YamlMember(Alias = "ruler_labels")]
    public bool RulerLabels { get; set; } = true;

    /// <summary>How many renders the chat's blob store keeps. Renders are written under the rotating
    /// names <c>geom_render_1..N</c>, so the store holds this many at most instead of one blob per
    /// step of the loop (a single frame's markup used to leave dozens of megabytes behind).</summary>
    [YamlMember(Alias = "render_history")]
    public int RenderHistory { get; set; } = 5;

    // ── probes (geom_probe, ADR_20260916) ─────────────────────────────────────

    /// <summary>What a probe looks like: <c>ring</c> — a hollow ring, number on a plate beside it;
    /// <c>dot</c> — a small translucent dot, number beside it; <c>badge</c> — a disc with the number
    /// inside. Which one a given model reads best is a question for a live run, so all three stay.</summary>
    [YamlMember(Alias = "probe_marker")]
    public string ProbeMarker { get; set; } = "ring";

    /// <summary><c>mono</c> — one colour for every probe; <c>edge</c> — the colour of the edge a probe
    /// answers for.</summary>
    [YamlMember(Alias = "probe_color")]
    public string ProbeColor { get; set; } = "mono";

    /// <summary><c>grid</c> — probes on their lattice slots; <c>jitter</c> — shifted randomly within
    /// the slot, seeded by the round so a render can be reproduced.</summary>
    [YamlMember(Alias = "probe_layout")]
    public string ProbeLayout { get; set; } = "grid";

    /// <summary>One probe per this many view pixels along an edge. A density, not a count: how many
    /// probes a round gets follows from the box and the frame.</summary>
    [YamlMember(Alias = "probe_spacing")]
    public int ProbeSpacing { get; set; } = 48;

    /// <summary>Spacing of the lattice that covers the whole view when there is no box yet, and of the
    /// rows across a band still too wide to halve. Coarser than <see cref="ProbeSpacing"/>: at the
    /// fine spacing a whole frame is hundreds of numbers to list.</summary>
    [YamlMember(Alias = "probe_scan_spacing")]
    public int ProbeScanSpacing { get; set; } = 96;

    /// <summary>How many rows of probes go across an edge's band once it is narrow enough to halve.</summary>
    [YamlMember(Alias = "probe_rows")]
    public int ProbeRows { get; set; } = 3;

    /// <summary>An edge whose band is no wider than this many view pixels, with no contradicting
    /// answers, is settled and gets no more probes.</summary>
    [YamlMember(Alias = "probe_tolerance")]
    public int ProbeTolerance { get; set; } = 4;

    /// <summary>Point size of the probe numbers. A number too small to read is read anyway, wrongly.</summary>
    [YamlMember(Alias = "probe_font_size")]
    public int ProbeFontSize { get; set; } = 16;

    /// <summary>Whether accepted objects are covered by a dark veil in probe rounds, so what is already
    /// marked neither distracts nor gets answered about a second time.</summary>
    [YamlMember(Alias = "probe_veil")]
    public bool ProbeVeil { get; set; } = true;


    /// <summary>
    /// The spacing the grid actually uses for a frame of this size: <see cref="GridStep"/> when it is
    /// pinned, otherwise the smallest multiple of <see cref="GridMinStep"/> that keeps the frame's
    /// longer side under <see cref="GridLines"/> lines.
    /// <para>
    /// Deriving it is the point. A crop of a fingernail and a full photograph are the same number of
    /// view pixels across only by accident, and a spacing that suits one covers the other in hatching
    /// or leaves it with three lines. What the model needs held constant is the <i>density</i> of the
    /// ruler in the picture it is looking at, and that is what a count fixes and a spacing cannot.
    /// </para>
    /// </summary>
    public int EffectiveGridStep(int viewWidth, int viewHeight)
    {
        if (GridStep > 0) return GridStep;
        var side = viewWidth > viewHeight ? viewWidth : viewHeight;
        var units = (int)System.Math.Ceiling((double)side / GridLines / GridMinStep);
        return GridMinStep * (units < 1 ? 1 : units);
    }

    /// <summary>A percentage of transparency as the alpha multiplier the painters want.</summary>
    public static float Opacity(int transparency) => (100 - transparency) / 100f;

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
        // 0 keeps the derived step; anything else is a fixed spacing, and one above a quarter of the
        // smallest sensible view stops being a ruler.
        GridStep = GridStep <= 0 ? 0 : Clamp(GridStep, 4, 500);
        GridMinStep = Clamp(GridMinStep, 4, 256);
        GridLines = Clamp(GridLines, 4, 100);
        GridTransparency = Clamp(GridTransparency, 0, 95);
        RulerTransparency = Clamp(RulerTransparency, 0, 95);
        // 1 means every line is major — legal, and what a coarse step wants.
        GridMajorEvery = Clamp(GridMajorEvery, 1, 20);
        if (string.IsNullOrWhiteSpace(GridColor)) GridColor = new GeometrySettings().GridColor;
        ProbeMarker = OneOf(ProbeMarker, "ring", "ring", "dot", "badge");
        ProbeColor = OneOf(ProbeColor, "mono", "mono", "edge");
        ProbeLayout = OneOf(ProbeLayout, "grid", "grid", "jitter");
        ProbeSpacing = Clamp(ProbeSpacing, 16, 256);
        ProbeScanSpacing = Clamp(ProbeScanSpacing, 24, 512);
        ProbeRows = Clamp(ProbeRows, 1, 8);
        ProbeTolerance = Clamp(ProbeTolerance, 1, 64);
        ProbeFontSize = Clamp(ProbeFontSize, 10, 48);
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

    /// <summary>A word from a closed list, case-insensitive; anything else falls back to the default.</summary>
    private static string OneOf(string? value, string fallback, params string[] allowed)
    {
        var word = value?.Trim().ToLowerInvariant();
        return word is not null && System.Array.IndexOf(allowed, word) >= 0 ? word : fallback;
    }

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
}
