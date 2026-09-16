using SkiaSharp;
using SPLA.Plugins.Geometry.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SPLA.Plugins.Geometry.Render;

/// <summary>What a probe round adds to a render: the numbered probes, or the lettered groups to pick from.</summary>
internal sealed record ProbeOverlay(IReadOnlyList<Probe> Probes, IReadOnlyList<ProbeGroup> Groups);

/// <summary>
/// Draws probes. The model is asked one question per probe — is its point on the thing or not — so
/// everything here serves two needs only: the point must be findable, and its number must be readable
/// and unmistakably its own (ADR_20260916 §2.6).
/// </summary>
internal static class ProbeRenderer
{
    private static readonly SKColor Halo = new(0x00, 0x00, 0x00, 0xB0);

    public static void Draw(SKCanvas canvas, ProbeOverlay overlay, GeometrySettings cfg)
    {
        using var text = new SKPaint
        {
            TextSize = cfg.ProbeFontSize,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold),
        };

        foreach (var group in overlay.Groups) DrawGroup(canvas, group, text, cfg);

        // Every marker first, then the plates, each placed clear of all markers and of the plates before
        // it. Plates of inside probes all lean towards the centre, and left to that they pile up on one
        // another exactly where the print being judged is.
        var occupied = new List<SKRect>();
        foreach (var probe in overlay.Probes)
        {
            var radius = DrawMarker(canvas, probe, text, cfg);
            occupied.Add(new SKRect((float)probe.X - radius, (float)probe.Y - radius, (float)probe.X + radius, (float)probe.Y + radius));
        }

        if (cfg.ProbeMarker == "badge") return;
        for (var i = 0; i < overlay.Probes.Count; i++)
            DrawPlate(canvas, overlay.Probes[i], occupied[i].Width / 2, text, cfg, occupied);
    }

    /// <summary>A group to pick from: a dashed outline and a large letter. The model chooses a visible
    /// thing by the letter on it instead of describing it in words.</summary>
    private static void DrawGroup(SKCanvas canvas, ProbeGroup group, SKPaint text, GeometrySettings cfg)
    {
        var pad = cfg.ProbeFontSize * 0.75f;
        var rect = new SKRect((float)group.MinX - pad, (float)group.MinY - pad, (float)group.MaxX + pad, (float)group.MaxY + pad);

        using var halo = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 5, Color = Halo, IsAntialias = true };
        using var line = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2.5f,
            Color = SKColors.White,
            IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash([10, 6], 0),
        };
        canvas.DrawRect(rect, halo);
        canvas.DrawRect(rect, line);

        var saved = text.TextSize;
        text.TextSize = saved * 1.6f;
        Plate(canvas, text, group.Letter.ToString(), rect.Left + text.TextSize * 0.5f, rect.Top, GeometryRenderer.LabelPlate, SKColors.White);
        text.TextSize = saved;
    }

    private static SKColor ColourOf(Probe probe, GeometrySettings cfg) =>
        cfg.ProbeColor == "edge" && probe.Edge >= 0 ? GeometryRenderer.EdgeColors[probe.Edge] : SKColors.White;

    /// <summary>Draws the marker and returns its radius.</summary>
    private static float DrawMarker(SKCanvas canvas, Probe probe, SKPaint text, GeometrySettings cfg)
    {
        var colour = ColourOf(probe, cfg);
        float x = (float)probe.X, y = (float)probe.Y;

        if (cfg.ProbeMarker == "badge")
        {
            // The number is the marker: nothing to associate by proximity, at the price of covering
            // the very point the question is about.
            var label = probe.Number.ToString(CultureInfo.InvariantCulture);
            var metrics = text.FontMetrics;
            var radius = Math.Max(text.MeasureText(label), metrics.Descent - metrics.Ascent) / 2 + 4;
            using var fill = new SKPaint { Style = SKPaintStyle.Fill, Color = colour.WithAlpha(0xE6), IsAntialias = true };
            using var rim = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = Halo, IsAntialias = true };
            canvas.DrawCircle(x, y, radius, fill);
            canvas.DrawCircle(x, y, radius, rim);
            text.Color = Contrast(colour);
            canvas.DrawText(label, x - text.MeasureText(label) / 2, y - (metrics.Ascent + metrics.Descent) / 2, text);
            return radius;
        }

        if (cfg.ProbeMarker == "dot")
        {
            var radius = Math.Max(3f, cfg.ProbeFontSize * 0.22f);
            using var fill = new SKPaint { Style = SKPaintStyle.Fill, Color = colour.WithAlpha(0x99), IsAntialias = true };
            using var rim = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = Halo.WithAlpha(0x99), IsAntialias = true };
            canvas.DrawCircle(x, y, radius, fill);
            canvas.DrawCircle(x, y, radius, rim);
            return radius;
        }

        // Hollow: the point itself stays visible, which is the whole reason for a ring. The dark halo
        // sits outside the coloured ring only, so on a pale ground it does not read as a second ring.
        var ringRadius = Math.Max(8f, cfg.ProbeFontSize * 0.55f);
        using var halo = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2f, Color = Halo, IsAntialias = true };
        using var ring = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f, Color = colour, IsAntialias = true };
        canvas.DrawCircle(x, y, ringRadius + 2f, halo);
        canvas.DrawCircle(x, y, ringRadius, ring);
        return ringRadius + 3f;
    }

    /// <summary>Turns tried, in order, from the preferred direction when the plate there would cover
    /// another marker or plate.</summary>
    private static readonly double[] PlateTurns = [0, 45, -45, 90, -90, 135, -135, 180];

    /// <summary>
    /// The number plate beside a ring or a dot. It goes away from where the answer is decided — further
    /// out for a probe outside the box, towards the centre for one inside — so the strip around the edge
    /// stays uncovered; when that spot is taken it turns until it finds a free one.
    /// </summary>
    private static void DrawPlate(
        SKCanvas canvas, Probe probe, float markerRadius, SKPaint text, GeometrySettings cfg, List<SKRect> occupied)
    {
        var label = probe.Number.ToString(CultureInfo.InvariantCulture);
        var fm = text.FontMetrics;
        var halfWidth = text.MeasureText(label) / 2 + 3;
        var halfHeight = (fm.Descent - fm.Ascent) / 2 + 2;
        var own = new SKRect((float)probe.X - markerRadius, (float)probe.Y - markerRadius, (float)probe.X + markerRadius, (float)probe.Y + markerRadius);

        SKRect? chosen = null;
        foreach (var turn in PlateTurns)
        {
            var rad = turn * Math.PI / 180;
            var dx = probe.PlateX * Math.Cos(rad) - probe.PlateY * Math.Sin(rad);
            var dy = probe.PlateX * Math.Sin(rad) + probe.PlateY * Math.Cos(rad);
            var reach = markerRadius + 2 + (float)(Math.Abs(dx) * halfWidth + Math.Abs(dy) * halfHeight);
            float cx = (float)probe.X + (float)dx * reach, cy = (float)probe.Y + (float)dy * reach;
            var rect = new SKRect(cx - halfWidth, cy - halfHeight, cx + halfWidth, cy + halfHeight);
            chosen ??= rect;
            if (occupied.Any(o => o != own && o.IntersectsWith(rect))) continue;
            chosen = rect;
            break;
        }

        var place = chosen!.Value;
        occupied.Add(place);

        var colour = ColourOf(probe, cfg);
        var (plate, digits) = cfg.ProbeColor == "edge" && probe.Edge >= 0
            ? (colour.WithAlpha(0xF2), Contrast(colour))
            : (GeometryRenderer.LabelPlate, SKColors.White);
        Plate(canvas, text, label, place.MidX, place.MidY, plate, digits);
    }

    /// <summary>A label on a plate, centred on the point given.</summary>
    private static void Plate(SKCanvas canvas, SKPaint text, string value, float centreX, float centreY, SKColor plate, SKColor digits)
    {
        var width = text.MeasureText(value);
        var metrics = text.FontMetrics;
        var baseline = centreY - (metrics.Ascent + metrics.Descent) / 2;
        var left = centreX - width / 2;

        using var fill = new SKPaint { Style = SKPaintStyle.Fill, Color = plate, IsAntialias = true };
        canvas.DrawRect(new SKRect(left - 3, baseline + metrics.Ascent - 2, left + width + 3, baseline + metrics.Descent + 2), fill);
        text.Color = digits;
        canvas.DrawText(value, left, baseline, text);
    }

    private static SKColor Contrast(SKColor colour) =>
        0.299 * colour.Red + 0.587 * colour.Green + 0.114 * colour.Blue > 140 ? SKColors.Black : SKColors.White;
}
