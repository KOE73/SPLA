using SkiaSharp;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Session;
using System;
using System.Linq;

namespace SPLA.Plugins.Geometry.Render;

/// <summary>
/// Draws what the session holds, in the pixels of one view. This is the half of the loop the model
/// actually reads: it never gets the coordinates back as numbers to verify, it gets the picture and
/// decides whether the outline sits on the thing.
/// <para>
/// <b>The editing box gets four edge colours; everything else gets one muted colour.</b> A term the
/// model has to remember is a place where its habit can differ from ours; a term it can see in the
/// picture cannot differ. So every number the model must supply has a visible counterpart here, named
/// by the same word in the reply text (ADR_20260914-3 §2). Identity of the four edges lives inside the
/// one box being placed; status stays at the level of the object. Accepted boxes are drawn in a single
/// muted colour with no corner dots — five accepted objects would otherwise mean twenty coloured edges
/// and no picture at all (ADR §3.4, overriding PLAN_20260914 §2.1).
/// </para>
/// </summary>
internal static class GeometryRenderer
{
    /// <summary>
    /// The four edge colours, in the winding order of <see cref="Obb.Corners"/>: index 0 is the edge
    /// from corner 0 to corner 1 — the box's own top — then right, bottom, left.
    /// <para>
    /// <b>This order and these colours are frozen.</b> The model is told to name an edge by its colour,
    /// so changing either silently changes the meaning of every call. Recorded in the plugin's
    /// AGENTS.md for the same reason.
    /// </para>
    /// <para>
    /// Why these four: the leading subject is a grey-white sack carrying red and blue print, so red is
    /// unusable — it vanishes into the content. They are also spread in <i>lightness</i>, not only in
    /// hue (yellow brightest, then cyan, then magenta, then the darkest, green), so nothing here rests
    /// on colour vision alone (ADR §3.5). All four are light enough to read against a dark photograph:
    /// a colour the owner has to hunt for on the screen is not a vocabulary, and the deep green and
    /// deep magenta this started with were exactly that.
    /// </para>
    /// </summary>
    internal static readonly SKColor[] EdgeColors =
    [
        new(0x00, 0xCF, 0xFF), // cyan    — top
        new(0xFF, 0x74, 0xE4), // magenta — right
        new(0xFF, 0xE1, 0x00), // yellow  — bottom
        new(0x3F, 0xD6, 0x5C), // green   — left
    ];

    /// <summary>The names of <see cref="EdgeColors"/>, in the same order. The <c>edge</c> argument of
    /// <c>geom_box</c> and the mapping line printed in every reply both come from here, so the word on
    /// the screen, the word in the text and the word in the call are one word by construction.</summary>
    internal static readonly string[] EdgeNames = ["cyan", "magenta", "yellow", "green"];

    /// <summary>Which side of the unrotated box each entry of <see cref="EdgeNames"/> is.</summary>
    internal static readonly string[] EdgeSides = ["top", "right", "bottom", "left"];

    /// <summary>Muted slate: this one is settled and only there for context. Deliberately nothing near
    /// the four edge colours, so an accepted outline can never be read as an edge of the live box.</summary>
    private static readonly SKColor AcceptedColor = new(0x7A, 0x8C, 0xA0);

    /// <summary>The live box's name plate and its centre mark: neutral, outside the edge palette, so
    /// neither competes with the four colours the model has to name.</summary>
    private static readonly SKColor EditingNeutral = new(0xFF, 0xFF, 0xFF);

    internal static readonly SKColor LabelPlate = new(0x00, 0x00, 0x00, 0xC0);

    /// <summary>What covers an accepted object during a probe round: dark enough that its content stops
    /// competing for an answer, not so dark that the model loses where it is.</summary>
    private static readonly SKColor VeilColor = new(0x00, 0x00, 0x00, 0x8C);

    /// <summary>Alpha a fine grid line keeps, as a fraction of the configured colour's. Every line
    /// both measures and obscures, and on blurred small print a dense grid can cost more legibility
    /// than it returns in measurement — which is what the major/minor split is for. The fine lines
    /// have to stay genuinely faint for that split to mean anything.</summary>
    private const float MinorAlpha = 0.35f;

    /// <summary>Half-length of a point's cross arms, in view pixels. A point is never drawn as a
    /// pixel: a one-pixel dot is invisible to the model, which defeats the purpose of rendering at
    /// all.
    /// <para>
    /// The arms run <b>diagonally</b>, an X rather than a +. That is not decoration: the centre of the
    /// editing box is now drawn too, and two different meanings sharing one glyph is the same disease
    /// as two meanings sharing one word (ADR §3.2). A point is an X in a circle; a centre is a filled
    /// dot in a ring, and no arms at all.
    /// </para></summary>
    private const float CrossArm = 12f;

    /// <summary>Radius of the filled dot that marks the editing box's centre, in view pixels.</summary>
    private const float CentreDot = 4.5f;

    /// <summary>Radius of the thin ring around that dot. Far enough out that the gap between dot and
    /// ring survives at render scale — a solid blob and a ringed dot are one glyph from two metres.</summary>
    private const float CentreRing = 10f;

    /// <summary>Radius of a corner dot, in view pixels.</summary>
    private const float CornerDot = 5f;

    /// <param name="probes">A probe round to draw on top, or null. A probe round draws no view grid and
    /// no edge rulers — three layers of marks on one picture is a picture nobody reads — draws no unfinished
    /// object unless the overlay asks for its box (ADR_20260916-3), and veils the
    /// accepted objects when <c>probe_veil</c> says so (ADR_20260916 §2.5, §2.6).</param>
    public static byte[] Render(
        GeometrySession session, GeometryView view, bool? grid, bool? rulers, GeometrySettings cfg,
        ProbeOverlay? probes = null)
    {
        if (probes is not null) (grid, rulers) = (false, false);

        using var bitmap = new SKBitmap(view.Width, view.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Black);
            DrawFrame(canvas, session.Source, view);

            // The call wins for this call; the project setting decides when the call says nothing.
            if (grid ?? cfg.Grid) DrawGrid(canvas, view, cfg);

            var veil = probes is not null && cfg.ProbeVeil;
            // Unless asked for, a probe round draws only what is finished: a live box's coloured lines
            // run right through its rows of probes and hide the pixels the question is about.
            foreach (var obj in session.Objects.Where(o => IsVisible(o, view)))
            {
                if (probes is { ShowBox: false } && obj.Status != ObjectStatus.Accepted) continue;
                DrawObject(canvas, obj, view, rulers, cfg, veil);
            }

            if (probes is not null) ProbeRenderer.Draw(canvas, probes);
        }

        return Encode(bitmap, cfg);
    }

    /// <summary>True when any part of the object lands inside the view's own rectangle. What is not
    /// visible is not drawn and is listed in the reply as being outside instead — a box silently
    /// missing from the picture reads to the model as a box that was never created.</summary>
    public static bool IsVisible(GeometryObject obj, GeometryView view)
    {
        var t = view.SourceToView;
        if (obj.Kind == ObjectKind.Point)
        {
            var (x, y) = t.Apply(obj.Point.X, obj.Point.Y);
            return x >= 0 && y >= 0 && x <= view.Width && y <= view.Height;
        }

        if (obj.Box is not { } box) return false;

        var corners = box.Transformed(t).Corners();
        double minX = corners.Min(c => c.X), maxX = corners.Max(c => c.X);
        double minY = corners.Min(c => c.Y), maxY = corners.Max(c => c.Y);
        return maxX >= 0 && maxY >= 0 && minX <= view.Width && minY <= view.Height;
    }

    private static void DrawFrame(SKCanvas canvas, SKBitmap source, GeometryView view)
    {
        var t = view.SourceToView;
        var matrix = new SKMatrix(
            (float)t.A, (float)t.C, (float)t.E,
            (float)t.B, (float)t.D, (float)t.F,
            0, 0, 1);

        canvas.Save();
        canvas.SetMatrix(matrix);
        using var paint = new SKPaint { FilterQuality = SKFilterQuality.High, IsAntialias = true };
        canvas.DrawBitmap(source, 0, 0, paint);
        canvas.Restore();
    }

    private static void DrawObject(
        SKCanvas canvas, GeometryObject obj, GeometryView view, bool? rulers, GeometrySettings cfg, bool veil)
    {
        var accepted = obj.Status == ObjectStatus.Accepted;
        // An accepted outline steps back rather than disappearing: it is context for placing the
        // next object, not the subject of the current look.
        var width = Math.Max(1f, accepted ? cfg.LineWidth * 0.6f : cfg.LineWidth);
        var labelColor = accepted ? AcceptedColor : EditingNeutral;

        using var stroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = width,
            Color = AcceptedColor,
            IsAntialias = true,
            StrokeJoin = SKStrokeJoin.Round,
            StrokeCap = SKStrokeCap.Round,
        };

        var t = view.SourceToView;
        float labelX, labelY;

        if (obj.Kind == ObjectKind.Box && obj.Box is { } box)
        {
            var inView = box.Transformed(t);
            var corners = inView.Corners();

            if (accepted)
            {
                using var path = new SKPath();
                path.MoveTo((float)corners[0].X, (float)corners[0].Y);
                for (var i = 1; i < 4; i++) path.LineTo((float)corners[i].X, (float)corners[i].Y);
                path.Close();
                if (veil)
                {
                    using var shade = new SKPaint { Style = SKPaintStyle.Fill, Color = VeilColor, IsAntialias = true };
                    canvas.DrawPath(path, shade);
                }
                canvas.DrawPath(path, stroke);
            }
            else
            {
                DrawEditingBox(canvas, corners, inView, stroke, view, rulers, cfg);
            }

            // Clear of the top-left corner dot in both directions: the plate is opaque, and a plate
            // sitting on a corner dot hides the one colour that says which way the box is wound.
            labelX = (float)corners.Min(c => c.X) + CornerDot + 4;
            labelY = (float)corners.Min(c => c.Y) - CornerDot - 8;
        }
        else
        {
            stroke.Color = accepted ? AcceptedColor : EditingNeutral;
            var (px, py) = t.Apply(obj.Point.X, obj.Point.Y);
            float x = (float)px, y = (float)py;
            var arm = CrossArm * 0.707f;
            canvas.DrawLine(x - arm, y - arm, x + arm, y + arm, stroke);
            canvas.DrawLine(x - arm, y + arm, x + arm, y - arm, stroke);
            canvas.DrawCircle(x, y, CrossArm * 0.55f, stroke);

            labelX = x + CrossArm + 2;
            labelY = y - CrossArm * 0.6f;
        }

        DrawLabel(canvas, obj.Name, labelX, labelY, labelColor, cfg);
    }

    /// <summary>
    /// The box being placed: four coloured edges, a dot at each corner in the colour of the edge that
    /// <b>starts</b> there, and the centre marked.
    /// <para>
    /// The colours are bound to the box's own axes, not the screen's, so the cyan edge stays the same
    /// edge at any angle — which is the whole reason side names like "left" were rejected (ADR §3.1).
    /// The corner dots come for free out of that and pay for themselves: the run of colours around the
    /// outline shows the winding, so the box's orientation is legible without reading the angle.
    /// </para>
    /// <para>
    /// The centre is here because <c>dx</c>/<c>dy</c> move exactly it and the model, until now, could
    /// not see what it was moving (ADR §3.2).
    /// </para>
    /// </summary>
    private static void DrawEditingBox(
        SKCanvas canvas, (double X, double Y)[] corners, Obb inView, SKPaint stroke,
        GeometryView view, bool? rulers, GeometrySettings cfg)
    {
        // Under the edges, never over them: the four colours are the vocabulary, the scales are a
        // ruler. The call wins for this call; the project setting decides when the call says nothing.
        if (rulers ?? cfg.EdgeRulers)
            EdgeRuler.Draw(canvas, corners, inView, view, EdgeColors, cfg);

        for (var i = 0; i < 4; i++)
        {
            var a = corners[i];
            var b = corners[(i + 1) % 4];
            stroke.Color = EdgeColors[i];
            canvas.DrawLine((float)a.X, (float)a.Y, (float)b.X, (float)b.Y, stroke);
        }

        using var fill = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };
        for (var i = 0; i < 4; i++)
        {
            fill.Color = EdgeColors[i];
            canvas.DrawCircle((float)corners[i].X, (float)corners[i].Y, CornerDot, fill);
        }

        float cx = (float)inView.Cx, cy = (float)inView.Cy;

        // A dark halo first: the centre mark is white, and white on a white sack is nothing.
        using var halo = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = stroke.StrokeWidth + 2,
            Color = new SKColor(0x00, 0x00, 0x00, 0xB0),
            IsAntialias = true,
        };
        canvas.DrawCircle(cx, cy, CentreRing, halo);

        using var ring = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = Math.Max(1.5f, stroke.StrokeWidth * 0.6f),
            Color = EditingNeutral,
            IsAntialias = true,
        };
        canvas.DrawCircle(cx, cy, CentreRing, ring);

        fill.Color = new SKColor(0x00, 0x00, 0x00, 0xB0);
        canvas.DrawCircle(cx, cy, CentreDot + 1.5f, fill);
        fill.Color = EditingNeutral;
        canvas.DrawCircle(cx, cy, CentreDot, fill);
    }

    /// <summary>A label on a dark plate. Coloured text straight onto a photograph is unreadable
    /// wherever the photograph happens to be pale, and the model cannot ask which box is which.</summary>
    private static void DrawLabel(SKCanvas canvas, string name, float x, float y, SKColor color, GeometrySettings cfg)
    {
        if (string.IsNullOrEmpty(name)) return;

        using var text = new SKPaint
        {
            Color = color,
            TextSize = cfg.FontSize,
            IsAntialias = true,
            Typeface = SKTypeface.Default,
        };

        var textWidth = text.MeasureText(name);
        var metrics = text.FontMetrics;
        // Keep the plate inside the frame: a label pushed off the top or the right edge takes the
        // object's only identifier with it.
        var baseline = Math.Max(y, -metrics.Ascent + 2);
        var left = Math.Min(Math.Max(x, 0), Math.Max(0, canvas.DeviceClipBounds.Width - textWidth - 4));

        using var plate = new SKPaint { Color = LabelPlate, Style = SKPaintStyle.Fill, IsAntialias = true };
        canvas.DrawRect(
            new SKRect(left - 2, baseline + metrics.Ascent - 2, left + textWidth + 2, baseline + metrics.Descent + 2),
            plate);
        canvas.DrawText(name, left, baseline, text);
    }

    /// <summary>
    /// The grid in the view's own coordinates — the ruler the model reads numbers off.
    /// <para>
    /// Fine lines every <c>grid_step</c> px, unlabelled; every <c>grid_major_every</c>-th line
    /// stronger and carrying its coordinate. Uniform labelling at a fine step turns the picture into
    /// noise, and the picture is what the model is here to read.
    /// </para>
    /// <para>
    /// Labels sit on the top and left borders of the frame, not where the lines cross, so they stay
    /// off the thing being marked wherever that is possible at all.
    /// </para>
    /// </summary>
    private static void DrawGrid(SKCanvas canvas, GeometryView view, GeometrySettings cfg)
    {
        var color = ParseColor(cfg.GridColor);
        // The configured transparency applies to the whole grid; the major/minor split then divides
        // what is left, so moving the one setting keeps the two ranks in proportion to each other.
        var opacity = GeometrySettings.Opacity(cfg.GridTransparency);
        color = color.WithAlpha((byte)(color.Alpha * opacity));
        var minorColor = color.WithAlpha((byte)(color.Alpha * MinorAlpha));

        using var minor = new SKPaint { Color = minorColor, StrokeWidth = 1, IsAntialias = false };
        using var major = new SKPaint { Color = color, StrokeWidth = 2, IsAntialias = false };
        using var label = new SKPaint
        {
            Color = SKColors.White,
            TextSize = Math.Max(10, cfg.FontSize - 4),
            IsAntialias = true,
            Typeface = SKTypeface.Default,
        };
        using var plate = new SKPaint { Color = LabelPlate, Style = SKPaintStyle.Fill };

        var step = cfg.EffectiveGridStep(view.Width, view.Height);
        var every = cfg.GridMajorEvery;

        for (int x = step, i = 1; x < view.Width; x += step, i++)
        {
            var strong = i % every == 0;
            canvas.DrawLine(x, 0, x, view.Height, strong ? major : minor);
            if (strong) Tick(canvas, label, plate, x.ToString(), x + 2, label.TextSize + 2);
        }

        for (int y = step, i = 1; y < view.Height; y += step, i++)
        {
            var strong = i % every == 0;
            canvas.DrawLine(0, y, view.Width, y, strong ? major : minor);
            if (strong) Tick(canvas, label, plate, y.ToString(), 2, y - 2);
        }
    }

    /// <summary>The configured grid colour, <c>#RRGGBB</c> or <c>#AARRGGBB</c>. An unparseable value
    /// falls back to the default rather than failing the render: a hand-edited settings file degrades
    /// here the same way every other value in <see cref="GeometrySettings"/> does.</summary>
    private static SKColor ParseColor(string value) =>
        SKColor.TryParse(value, out var color) ? color : SKColor.Parse(new GeometrySettings().GridColor);

    private static void Tick(SKCanvas canvas, SKPaint text, SKPaint plate, string value, float x, float y)
    {
        var width = text.MeasureText(value);
        var metrics = text.FontMetrics;
        canvas.DrawRect(new SKRect(x - 1, y + metrics.Ascent - 1, x + width + 1, y + metrics.Descent + 1), plate);
        canvas.DrawText(value, x, y, text);
    }

    /// <summary>PNG unless the project asked for JPEG. PNG keeps outlines crisp; JPEG is there for
    /// frames where the byte count matters more than the edges.</summary>
    public static byte[] Encode(SKBitmap bitmap, GeometrySettings cfg)
    {
        var (format, quality) = cfg.JpegQuality == 0
            ? (SKEncodedImageFormat.Png, 100)
            : (SKEncodedImageFormat.Jpeg, cfg.JpegQuality);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, quality);
        return data.ToArray();
    }

    /// <summary>The media type the bytes from <see cref="Render"/> carry.</summary>
    public static string MimeType(GeometrySettings cfg) => cfg.JpegQuality == 0 ? "image/png" : "image/jpeg";
}
