using SkiaSharp;
using SPLA.Plugins.Geometry.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SPLA.Plugins.Geometry.Render;

/// <summary>How a probe is drawn. The settings give one; <c>geom_probe_legibility</c> can replace it for a
/// session with the smallest one the model proved it reads.</summary>
/// <param name="Marker"><c>ring</c>, <c>dot</c>, <c>badge</c> or <c>bare</c> (the number alone, centred on the point).</param>
/// <param name="Label"><c>plate</c> — digits on a dark plate; <c>outline</c> — white digits with a dark outline;
/// <c>dark</c> — dark digits with a white outline. A badge ignores it; a bare number on a plate is drawn outlined.</param>
/// <param name="FontSize">Pixel size of the digits.</param>
/// <param name="Color"><c>mono</c> or <c>edge</c>.</param>
internal sealed record ProbeStyle(string Marker, string Label, int FontSize, string Color)
{
    public static ProbeStyle From(GeometrySettings cfg) => new(cfg.ProbeMarker, cfg.ProbeLabel, cfg.ProbeFontSize, cfg.ProbeColor);

    /// <summary>What the picture looks like, in the words the reply uses.</summary>
    public string Describe() => Marker switch
    {
        "badge" => "a probe's point is the centre of a numbered disc.",
        "bare" => "a probe is just its number, and its point is the middle of the number.",
        "dot" => $"a probe's point is a small dot, and its number is {Beside}.",
        _ => $"a probe's point is the centre of a small hollow ring, and its number is {Beside}.",
    };

    private string Beside => Label == "plate" ? "on the plate next to it" : "written next to it";

    /// <summary>A bare number cannot sit on a plate — the plate would be the marker — so it is outlined.</summary>
    public string OutlineLabel => Label == "dark" ? "dark" : "outline";

    public override string ToString() => Marker switch
    {
        "badge" => $"badge {FontSize}px",
        "bare" => $"bare + {OutlineLabel} {FontSize}px",
        _ => $"{Marker} + {Label} {FontSize}px",
    };
}

/// <summary>What a probe round adds to a render: the numbered probes, or the lettered groups to pick from.</summary>
/// <param name="Styles">One style per probe, index-aligned — the legibility chart draws every cell differently.
/// Null draws all probes in <paramref name="Style"/>.</param>
/// <param name="ShowBox">Whether the box being probed is drawn under the probes.</param>
internal sealed record ProbeOverlay(
    IReadOnlyList<Probe> Probes, IReadOnlyList<ProbeGroup> Groups, ProbeStyle Style,
    bool ShowBox = false, IReadOnlyList<ProbeStyle>? Styles = null);

/// <summary>
/// Draws probes. The model is asked one question per probe — is its point on the thing or not — so
/// everything here serves two needs only: the point must be findable, and its number must be readable
/// and unmistakably its own (ADR_20260916 §2.6). Everything else is kept as thin as those two allow:
/// a mark covers the very picture the question is about (ADR_20260916-3).
/// </summary>
internal static class ProbeRenderer
{
    private static readonly SKColor Halo = new(0x00, 0x00, 0x00, 0xB0);

    public static void Draw(SKCanvas canvas, ProbeOverlay overlay)
    {
        foreach (var group in overlay.Groups) DrawGroup(canvas, group, overlay.Style);

        // Every marker first, then the labels, each placed clear of all markers and of the labels before
        // it. Labels of inside probes all lean towards the centre, and left to that they pile up on one
        // another exactly where the print being judged is.
        var occupied = new List<SKRect>();
        for (var i = 0; i < overlay.Probes.Count; i++)
        {
            var probe = overlay.Probes[i];
            using var text = TextPaint(StyleOf(overlay, i));
            var radius = DrawMarker(canvas, probe, text, StyleOf(overlay, i));
            occupied.Add(new SKRect((float)probe.X - radius, (float)probe.Y - radius, (float)probe.X + radius, (float)probe.Y + radius));
        }

        for (var i = 0; i < overlay.Probes.Count; i++)
        {
            var style = StyleOf(overlay, i);
            if (style.Marker is "badge" or "bare") continue;
            using var text = TextPaint(style);
            DrawLabel(canvas, overlay.Probes[i], occupied[i].Width / 2, text, style, occupied);
        }
    }

    private static ProbeStyle StyleOf(ProbeOverlay overlay, int index) =>
        overlay.Styles is { } styles && index < styles.Count ? styles[index] : overlay.Style;

    private static SKPaint TextPaint(ProbeStyle style) => new()
    {
        TextSize = style.FontSize,
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold),
    };

    /// <summary>A group to pick from: a dashed outline and a large letter. The model chooses a visible
    /// thing by the letter on it instead of describing it in words.</summary>
    private static void DrawGroup(SKCanvas canvas, ProbeGroup group, ProbeStyle style)
    {
        using var text = TextPaint(style);
        text.TextSize = Math.Max(14, style.FontSize * 1.6f);
        var pad = style.FontSize * 0.75f;
        var rect = new SKRect((float)group.MinX - pad, (float)group.MinY - pad, (float)group.MaxX + pad, (float)group.MaxY + pad);

        using var halo = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 3, Color = Halo, IsAntialias = true };
        using var line = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            Color = SKColors.White,
            IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash([10, 6], 0),
        };
        canvas.DrawRect(rect, halo);
        canvas.DrawRect(rect, line);
        Plate(canvas, text, group.Letter.ToString(), rect.Left + text.TextSize * 0.5f, rect.Top, GeometryRenderer.LabelPlate, SKColors.White);
    }

    private static SKColor ColourOf(Probe probe, ProbeStyle style) =>
        style.Color == "edge" && probe.Edge >= 0 ? GeometryRenderer.EdgeColors[probe.Edge] : SKColors.White;

    /// <summary>Line widths follow the digits: a 16 px ring with a 2.5 px stroke is fine, the same stroke
    /// round 8 px digits is a blot.</summary>
    private static float Stroke(ProbeStyle style) => Math.Max(1f, style.FontSize / 10f);

    /// <summary>Draws the marker and returns the radius labels must keep clear of.</summary>
    private static float DrawMarker(SKCanvas canvas, Probe probe, SKPaint text, ProbeStyle style)
    {
        var colour = ColourOf(probe, style);
        float x = (float)probe.X, y = (float)probe.Y;
        var label = probe.Number.ToString(CultureInfo.InvariantCulture);
        var metrics = text.FontMetrics;

        if (style.Marker == "bare")
        {
            // The number is the whole mark, its middle is the point. Nothing but the digits covers the
            // picture — the least a probe can cover while still saying which one it is.
            var width = text.MeasureText(label);
            Outlined(canvas, text, label, x - width / 2, y - (metrics.Ascent + metrics.Descent) / 2, colour, style, style.OutlineLabel == "dark");
            return Math.Max(width, metrics.Descent - metrics.Ascent) / 2;
        }

        if (style.Marker == "badge")
        {
            // The number is the marker: nothing to associate by proximity, at the price of covering
            // the very point the question is about.
            var radius = Math.Max(text.MeasureText(label), metrics.Descent - metrics.Ascent) / 2 + Math.Max(1.5f, style.FontSize / 6f);
            using var fill = new SKPaint { Style = SKPaintStyle.Fill, Color = colour.WithAlpha(0xE6), IsAntialias = true };
            using var rim = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = Halo, IsAntialias = true };
            canvas.DrawCircle(x, y, radius, fill);
            canvas.DrawCircle(x, y, radius, rim);
            text.Color = Contrast(colour);
            canvas.DrawText(label, x - text.MeasureText(label) / 2, y - (metrics.Ascent + metrics.Descent) / 2, text);
            return radius;
        }

        if (style.Marker == "dot")
        {
            var radius = Math.Max(2f, style.FontSize * 0.2f);
            using var fill = new SKPaint { Style = SKPaintStyle.Fill, Color = colour.WithAlpha(0x99), IsAntialias = true };
            using var rim = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = Halo.WithAlpha(0x99), IsAntialias = true };
            canvas.DrawCircle(x, y, radius, fill);
            canvas.DrawCircle(x, y, radius, rim);
            return radius;
        }

        // Hollow: the point itself stays visible, which is the whole reason for a ring. The dark halo
        // sits outside the coloured ring only, so on a pale ground it does not read as a second ring.
        var stroke = Stroke(style);
        var ringRadius = Math.Max(3f, style.FontSize * 0.35f);
        using var halo = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1f, Color = Halo, IsAntialias = true };
        using var ring = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = stroke, Color = colour, IsAntialias = true };
        canvas.DrawCircle(x, y, ringRadius + stroke / 2 + 0.5f, halo);
        canvas.DrawCircle(x, y, ringRadius, ring);
        return ringRadius + stroke;
    }

    /// <summary>Turns tried, in order, from the preferred direction when the label there would cover
    /// another marker or label.</summary>
    private static readonly double[] LabelTurns = [0, 45, -45, 90, -90, 135, -135, 180];

    /// <summary>
    /// The number beside a ring or a dot. It goes away from where the answer is decided — further out
    /// for a probe outside the box, towards the centre for one inside — so the strip around the edge
    /// stays uncovered; when that spot is taken it turns until it finds a free one.
    /// </summary>
    private static void DrawLabel(
        SKCanvas canvas, Probe probe, float markerRadius, SKPaint text, ProbeStyle style, List<SKRect> occupied)
    {
        var label = probe.Number.ToString(CultureInfo.InvariantCulture);
        var fm = text.FontMetrics;
        var pad = Pad(style);
        var halfWidth = text.MeasureText(label) / 2 + pad;
        var halfHeight = (fm.Descent - fm.Ascent) / 2 + pad * 0.5f;
        var own = new SKRect((float)probe.X - markerRadius, (float)probe.Y - markerRadius, (float)probe.X + markerRadius, (float)probe.Y + markerRadius);

        SKRect? chosen = null;
        foreach (var turn in LabelTurns)
        {
            var rad = turn * Math.PI / 180;
            var dx = probe.PlateX * Math.Cos(rad) - probe.PlateY * Math.Sin(rad);
            var dy = probe.PlateX * Math.Sin(rad) + probe.PlateY * Math.Cos(rad);
            var reach = markerRadius + 1 + (float)(Math.Abs(dx) * halfWidth + Math.Abs(dy) * halfHeight);
            float cx = (float)probe.X + (float)dx * reach, cy = (float)probe.Y + (float)dy * reach;
            var rect = new SKRect(cx - halfWidth, cy - halfHeight, cx + halfWidth, cy + halfHeight);
            chosen ??= rect;
            if (occupied.Any(o => o != own && o.IntersectsWith(rect))) continue;
            chosen = rect;
            break;
        }

        var place = chosen!.Value;
        occupied.Add(place);

        var colour = ColourOf(probe, style);
        if (style.Label is "outline" or "dark")
        {
            var width = text.MeasureText(label);
            Outlined(canvas, text, label, place.MidX - width / 2, place.MidY - (fm.Ascent + fm.Descent) / 2, colour, style, style.Label == "dark");
            return;
        }

        var (plate, digits) = style.Color == "edge" && probe.Edge >= 0
            ? (colour.WithAlpha(0xF2), Contrast(colour))
            : (GeometryRenderer.LabelPlate, SKColors.White);
        Plate(canvas, text, label, place.MidX, place.MidY, plate, digits, pad);
    }

    /// <summary>Plate padding: a pixel or two, growing slowly with the digits.</summary>
    private static float Pad(ProbeStyle style) => Math.Max(1f, style.FontSize / 8f);

    /// <summary>
    /// Digits with an outline and nothing behind them, hiding no more of the picture than their own
    /// strokes. Light digits in a dark outline, or — <paramref name="dark"/> — dark digits in an outline of
    /// the probe's colour; which one survives a given ground is for the legibility chart to find out.
    /// <para>
    /// The outline grows with the digits: a fixed pixel of it vanished on a pale sack at every size, and
    /// the digits melted into the ground (ADR_20260916-3 §2.2).
    /// </para>
    /// </summary>
    private static void Outlined(
        SKCanvas canvas, SKPaint text, string value, float left, float baseline, SKColor colour, ProbeStyle style, bool dark)
    {
        var ink = new SKColor(0x10, 0x10, 0x10);
        var (fill, outline) = dark
            ? (ink, colour)
            : (colour, Contrast(colour) == SKColors.Black ? ink : SKColors.White);

        // The stroke is centred on the glyph's edge, so half of it lies outside: 1 px at 8 px digits, 2.5 at 20.
        text.Style = SKPaintStyle.Stroke;
        text.StrokeWidth = Math.Max(2f, style.FontSize / 4f);
        text.StrokeJoin = SKStrokeJoin.Round;
        text.Color = outline;
        canvas.DrawText(value, left, baseline, text);
        text.Style = SKPaintStyle.Fill;
        text.Color = fill;
        canvas.DrawText(value, left, baseline, text);
    }

    /// <summary>A label on a plate, centred on the point given.</summary>
    private static void Plate(
        SKCanvas canvas, SKPaint text, string value, float centreX, float centreY, SKColor plate, SKColor digits, float pad = 3)
    {
        var width = text.MeasureText(value);
        var metrics = text.FontMetrics;
        var baseline = centreY - (metrics.Ascent + metrics.Descent) / 2;
        var left = centreX - width / 2;

        using var fill = new SKPaint { Style = SKPaintStyle.Fill, Color = plate, IsAntialias = true };
        canvas.DrawRect(new SKRect(left - pad, baseline + metrics.Ascent - pad * 0.5f, left + width + pad, baseline + metrics.Descent + pad * 0.5f), fill);
        text.Color = digits;
        canvas.DrawText(value, left, baseline, text);
    }

    private static SKColor Contrast(SKColor colour) =>
        0.299 * colour.Red + 0.587 * colour.Green + 0.114 * colour.Blue > 140 ? SKColors.Black : SKColors.White;
}
