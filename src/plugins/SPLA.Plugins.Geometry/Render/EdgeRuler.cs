using SkiaSharp;
using SPLA.Plugins.Geometry.Model;
using System;

namespace SPLA.Plugins.Geometry.Render;

/// <summary>
/// A labelled scale along each edge of the box being edited, measuring distance <b>from that edge, in
/// view pixels</b>, inward and outward.
/// <para>
/// <b>Why pixels from an edge and nothing else.</b> The correction the model has to write is
/// <c>geom_box{edge:"green", by:-15}</c>, and <c>by</c> is pixels from an edge. So the instrument that
/// answers "how far is the text from the green side" must speak in exactly that: the model reads "the
/// text sits between the 10 and the 20 from green" and passes the number it just read. No conversion,
/// no subtraction, no coordinates. This is the same principle as the edge colours — every number the
/// model must supply has a visible counterpart (ADR_20260914-3 §2) — applied to distance, and it is
/// what the proportional box grid could not do: a cell's size depends on the box's size, so a cell
/// count was never a number <c>by</c> would accept.
/// </para>
/// <para>
/// <b>Outward matters as much as inward, and is drawn solid.</b> When the print sticks out past an
/// edge, "how far out" is the same question, and nothing else in the picture answers it. But a line
/// 32 px inside an edge and a line 32 px outside it are the same colour, the same distance away and
/// carry the same number, so the only thing separating them is which side of the edge they fall on —
/// and "which side" is precisely the judgement by eye this instrument exists to remove. So the two
/// directions differ in <i>stroke</i>: <b>dashed is inside the box, solid is outside it</b>. A property
/// the reader can name without measuring anything, and the tool help says it in those words.
/// <para>
/// Dashed is the one that goes inside because inside is where the print is: the gaps let the letters
/// through, and the thing the ruler lies across stays readable. Outside there is nothing to protect,
/// so the line can be solid — and solid is also the easier of the two to trace to its end when the
/// overshoot being measured is a long way from the box.
/// </para>
/// </para>
/// <para>
/// <b>Only the box being edited gets rulers</b>, for the reason only it gets the four colours: four
/// edges × two directions × several labelled lines is already a lot of ink, and a second object's
/// worth would be mush.
/// </para>
/// </summary>
internal static class EdgeRuler
{
    /// <summary>How many inset rectangles the box may carry. Two, and one when two will not fit.
    /// <para>
    /// A ruler is read by counting rings, and past two there is nothing left to count that the model
    /// could not have got from the numbers themselves — while every extra ring covers more of the very
    /// print the box is being fitted to. Two gives the reader the one thing a single ring cannot: a
    /// <i>direction</i>, so "the text is between the first and the second" is a sentence.
    /// </para></summary>
    internal const int MaxRings = 2;

    /// <summary>Outward distances, as multiples of the finest step. Fine near the edge, then a jump:
    /// an overshoot of a few pixels needs resolution, an overshoot of eighty needs to be <i>read</i>
    /// rather than counted, and the two are answered by different progressions. Inward you check a
    /// tight fit; outward you measure a miss that may be large.</summary>
    internal static readonly int[] OutwardRungs = [1, 2, 4, 8];

    /// <summary>Length of a dash and of the gap after it, in view pixels. Long enough that the
    /// stroke reads as dashed at a glance rather than as a line that happens to be thin.</summary>
    private const float DashOn = 9f;

    /// <inheritdoc cref="DashOn"/>
    private const float DashOff = 7f;

    /// <summary>Fraction of the view's shorter side the outward scale may reach. It must stop well
    /// short of the frame: a scale that runs to the border stops reading as belonging to an edge.</summary>
    private const double OutwardReach = 0.25;

    /// <summary>
    /// How many inset rectangles a box with <paramref name="room"/> view pixels to spare can hold —
    /// half its <b>shorter</b> extent, since a ring is inset by the same amount on all four sides and
    /// the narrow direction is what runs out first.
    /// <para>
    /// The inset is always <paramref name="minStep"/>, the same number on every side and for every
    /// box. That is the point of it: "32 in from each side" is one fact the reader holds, while a
    /// scale that adapts its step per side is four facts, each of which has to be looked up before any
    /// distance can be read — and looking it up means reading a small digit off a blurred photograph,
    /// which is the one thing that cannot be relied on here.
    /// </para>
    /// </summary>
    internal static int InwardRings(double room, int minStep)
    {
        var fits = (int)Math.Floor(room / minStep);
        return fits < 0 ? 0 : fits > MaxRings ? MaxRings : fits;
    }

    /// <summary>The outward distances that fit in this view. Capped so the scale never reaches the
    /// frame's border.</summary>
    internal static int[] OutwardScale(int viewWidth, int viewHeight, int minStep)
    {
        var reach = Math.Min(viewWidth, viewHeight) * OutwardReach;
        var kept = new System.Collections.Generic.List<int>();
        foreach (var rung in OutwardRungs)
            if (rung * minStep <= reach) kept.Add(rung * minStep);
        // A view too small for even the first mark still gets it: without one, "sticks out" has no
        // number at all.
        if (kept.Count == 0) kept.Add(minStep);
        return [.. kept];
    }

    /// <summary>
    /// Draws all four rulers. Every line runs the full span of the side it belongs to — a ring corner
    /// to corner, an outward line the whole length of its edge. A tick at the end of an edge measures only the end of the edge: the thing being
    /// fitted sits in the middle, and a reader cannot carry a mark across a gap by eye, which is the
    /// whole reason the number is on the picture instead of in the reply. A line the print lies under
    /// is one the print can be read against; the cost of crossing what it measures is paid by
    /// <c>ruler_transparency</c>, not by shortening the line.
    /// </summary>
    public static void Draw(
        SKCanvas canvas, (double X, double Y)[] corners, Obb inView, GeometryView view,
        SKColor[] edgeColors, GeometrySettings cfg)
    {
        // Full size, not the shrunken label of a chart axis. These digits are read off a blurred
        // photograph by something that resolves the frame in patches of ~32 px, and a number too small
        // to read is worse than no number: it is read as a number anyway, wrongly.
        using var text = new SKPaint
        {
            TextSize = cfg.FontSize,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold),
        };
        using var plate = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };
        using var line = new SKPaint { StrokeWidth = 1.5f, IsAntialias = true };
        using var dashed = new SKPaint
        {
            StrokeWidth = 1.5f,
            IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash([DashOn, DashOff], 0),
        };

        // The rulers step on the same lattice as the grid, so a distance read off one lands on the
        // other instead of between its lines.
        var minStep = cfg.GridMinStep;
        var labels = cfg.RulerLabels;
        var opacity = GeometrySettings.Opacity(cfg.RulerTransparency);
        var outward = OutwardScale(view.Width, view.Height, minStep);

        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, view.Width, view.Height));

        // The outward normal of each edge. Corners wind clockwise on screen, so turning an edge's
        // direction a quarter turn anticlockwise points away from the box.
        var normals = new (double X, double Y)[4];
        var lengths = new double[4];
        for (var i = 0; i < 4; i++)
        {
            var a = corners[i];
            var b = corners[(i + 1) % 4];
            double dx = b.X - a.X, dy = b.Y - a.Y;
            lengths[i] = Math.Sqrt(dx * dx + dy * dy);
            if (lengths[i] < 2) continue;
            normals[i] = (dy / lengths[i], -dx / lengths[i]);
        }

        // Inward is a shrunken copy of the box, corners and all — not four lines that each stop where
        // their own edge stops. A line that runs past the box or falls short of it is a line whose two
        // ends say different things about where the box is, and that is exactly the ambiguity the
        // rulers are here to remove. A ring inset by the same amount everywhere is one shape the reader
        // already knows, moved in by one known number.
        var rings = InwardRings(Math.Min(inView.Width, inView.Height) / 2, minStep);
        for (var k = 1; k <= rings; k++)
        {
            var at = k * minStep;
            // A rectangle's corner moves inward along both of the edges that meet at it.
            var inset = new (double X, double Y)[4];
            for (var i = 0; i < 4; i++)
            {
                var before = normals[(i + 3) % 4];
                var here = normals[i];
                inset[i] = (corners[i].X - (before.X + here.X) * at,
                            corners[i].Y - (before.Y + here.Y) * at);
            }

            // Each side of the ring keeps its own edge's colour: the ring is a copy of the box, so it
            // has to be readable as one.
            for (var i = 0; i < 4; i++)
            {
                if (lengths[i] < 2) continue;
                var p = inset[i];
                var q = inset[(i + 1) % 4];
                dashed.Color = edgeColors[i].WithAlpha((byte)(0xFF * opacity));
                canvas.DrawLine((float)p.X, (float)p.Y, (float)q.X, (float)q.Y, dashed);

                // The number sits at the middle of its side, not at an end. An end is where four
                // colours, two corner dots and the ends of a second scale already meet, and it is the
                // first thing to leave the frame when the box runs past it. The middle is where the
                // reader is already looking.
                if (labels)
                    Tick(canvas, text, plate, at.ToString(),
                        (float)((p.X + q.X) / 2), (float)((p.Y + q.Y) / 2), edgeColors[i]);
            }
        }

        for (var i = 0; i < 4; i++)
        {
            var a = corners[i];
            var b = corners[(i + 1) % 4];
            var length = lengths[i];
            if (length < 2) continue;

            double ux = (b.X - a.X) / length, uy = (b.Y - a.Y) / length;
            double nx = normals[i].X, ny = normals[i].Y;

            var colour = edgeColors[i];
            line.Color = colour.WithAlpha((byte)(0xFF * opacity));

            for (var k = 0; k < outward.Length; k++)
            {
                // The full length of the edge it belongs to, corner to corner. A line shorter than
                // its edge measures only the stretch it runs beside, and the print sticks out wherever
                // it happens to stick out — usually at an end, since that is where a word runs on.
                var at = outward[k];
                double px = a.X + nx * at, py = a.Y + ny * at;
                double qx = b.X + nx * at, qy = b.Y + ny * at;
                canvas.DrawLine((float)px, (float)py, (float)qx, (float)qy, line);

                if (labels)
                    Tick(canvas, text, plate, at.ToString(),
                        (float)((px + qx) / 2), (float)((py + qy) / 2), colour);
            }
        }

        canvas.Restore();
    }

    /// <summary>
    /// One number, on a plate in its own edge's colour. The colour is the whole point: a line belonging
    /// to the green edge is green, so "10 from green" needs no word for "left" and cannot be read
    /// against the view grid by mistake. The digits go black or white by the plate's lightness, since
    /// the palette deliberately spans lightness and one text colour cannot serve all four. The plate is
    /// centred on the point it is given: a number that has to be associated with a line by proximity is
    /// one more thing to get wrong, and sitting on the line is the shortest way to say "this line".
    /// </summary>
    private static void Tick(
        SKCanvas canvas, SKPaint text, SKPaint plate, string value, float x, float y, SKColor colour)
    {
        var width = text.MeasureText(value);
        var metrics = text.FontMetrics;
        var left = x - width / 2;
        var top = y + metrics.Ascent - 2;
        var bottom = y + metrics.Descent + 2;

        plate.Color = colour.WithAlpha(0xF2);
        canvas.DrawRect(new SKRect(left - 3, top, left + width + 3, bottom), plate);

        var luminance = 0.299 * colour.Red + 0.587 * colour.Green + 0.114 * colour.Blue;
        text.Color = luminance > 140 ? SKColors.Black : SKColors.White;
        canvas.DrawText(value, left, y, text);
    }
}
