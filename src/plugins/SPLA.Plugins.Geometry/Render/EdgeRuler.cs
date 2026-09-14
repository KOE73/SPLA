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
/// <b>Outward matters as much as inward.</b> When the print sticks out past an edge, "how far out" is
/// the same question, and nothing else in the picture answers it.
/// </para>
/// <para>
/// <b>Only the box being edited gets rulers</b>, for the reason only it gets the four colours: four
/// edges × two directions × several labelled lines is already a lot of ink, and a second object's
/// worth would be mush.
/// </para>
/// </summary>
internal static class EdgeRuler
{
    /// <summary>Steps an inward scale is allowed to use, in view pixels. Round numbers a reader adds
    /// in their head: a scale stepping by 37 is arithmetic, not a ruler.</summary>
    internal static readonly int[] Ladder = [10, 20, 25, 50, 100, 200, 500];

    /// <summary>The finest step. Below this the lines are closer together than their own labels.</summary>
    internal const int MinStep = 10;

    /// <summary>Most inward lines one side may carry. Four to six is a scale; ten is hatching.</summary>
    internal const int MaxInward = 6;

    /// <summary>Outward distances, in view pixels. Fine near the edge, then a jump: an overshoot of a
    /// few pixels needs resolution, an overshoot of eighty needs to be <i>read</i> rather than counted,
    /// and the two are answered by different progressions. Inward you check a tight fit; outward you
    /// measure a miss that may be large.</summary>
    internal static readonly int[] OutwardLadder = [10, 20, 50, 100];

    /// <summary>Fraction of the view's shorter side the outward scale may reach. It must stop well
    /// short of the frame: a scale that runs to the border stops reading as belonging to an edge.</summary>
    private const double OutwardReach = 0.25;

    /// <summary>How much of an edge's length one inward segment covers, at each end.</summary>
    private const float EndFraction = 0.22f;

    /// <summary>Share of an edge an outward line spans, centred. Corners are already the busiest part
    /// of the picture — four colours, two dots and the ends of two inward scales meet there — and an
    /// overshoot happens where the print is, which is the middle.</summary>
    private const float OutwardSpan = 0.8f;

    /// <summary>Longest an inward segment gets however long the edge is.</summary>
    private const float MaxSegment = 48f;

    /// <summary>
    /// The inward scale for a side with <paramref name="depth"/> view pixels of room — half the box's
    /// extent perpendicular to that edge, so the scales of two opposite edges meet at the middle
    /// instead of crossing.
    /// <para>
    /// The coarsest-first rule is "take the finest step on the ladder that does not put more than
    /// <see cref="MaxInward"/> lines on the side", which lands on 4–6 lines for any ordinary box and
    /// degrades on its own for a small one. A box too narrow for even one step of 10 gets a single
    /// line at 10 if its full extent can hold it, and nothing at all otherwise — one honest line beats
    /// a crowd of unreadable ones.
    /// </para>
    /// </summary>
    internal static (int Step, int Count) InwardScale(double depth)
    {
        foreach (var step in Ladder)
        {
            var count = (int)Math.Floor(depth / step);
            if (count > MaxInward) continue;
            if (count >= 1) return (step, count);
            // Finer than the finest step: fall back to one line, if the box can hold it at all.
            return (MinStep, depth * 2 >= MinStep ? 1 : 0);
        }

        var last = Ladder[^1];
        return (last, Math.Max(1, (int)Math.Floor(depth / last)));
    }

    /// <summary>The outward distances that fit in this view. Capped so the scale never reaches the
    /// frame's border.</summary>
    internal static int[] OutwardScale(int viewWidth, int viewHeight)
    {
        var reach = Math.Min(viewWidth, viewHeight) * OutwardReach;
        var kept = new System.Collections.Generic.List<int>();
        foreach (var d in OutwardLadder)
            if (d <= reach) kept.Add(d);
        // A view too small for even the first mark still gets it: without one, "sticks out" has no
        // number at all.
        if (kept.Count == 0) kept.Add(OutwardLadder[0]);
        return [.. kept];
    }

    /// <summary>
    /// Draws all four rulers. Inward lines are kept short and grouped at the two ends of each edge:
    /// the middle of the box is where the marked thing is, and it is the one place that must stay
    /// readable. Outward lines run the edge's full span, because an overshoot happens wherever the
    /// print happens to stick out and a line only measures what it runs beside.
    /// </summary>
    public static void Draw(
        SKCanvas canvas, (double X, double Y)[] corners, Obb inView, GeometryView view,
        SKColor[] edgeColors, GeometrySettings cfg)
    {
        var fontSize = Math.Max(9f, cfg.FontSize - 7f);
        using var text = new SKPaint
        {
            TextSize = fontSize,
            IsAntialias = true,
            Typeface = SKTypeface.Default,
        };
        using var plate = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };
        using var line = new SKPaint { StrokeWidth = 1f, IsAntialias = true };

        var outward = OutwardScale(view.Width, view.Height);

        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, view.Width, view.Height));

        for (var i = 0; i < 4; i++)
        {
            var a = corners[i];
            var b = corners[(i + 1) % 4];
            double dx = b.X - a.X, dy = b.Y - a.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 2) continue;

            // Along the edge, and the outward normal of it. Corners wind clockwise on screen, so
            // turning the direction a quarter turn anticlockwise points away from the box.
            double ux = dx / length, uy = dy / length;
            double nx = uy, ny = -ux;

            var colour = edgeColors[i];
            line.Color = colour.WithAlpha(0xB0);

            // Edges 0 and 2 are the box's top and bottom, so the room behind them is its height.
            var depth = (i % 2 == 0 ? inView.Height : inView.Width) / 2;
            var (step, count) = InwardScale(depth);
            var segment = (float)Math.Min(Math.Min(length * EndFraction, MaxSegment), length * 0.45);

            for (var k = 1; k <= count; k++)
            {
                var at = k * step;
                double px = a.X - nx * at, py = a.Y - ny * at;
                double qx = b.X - nx * at, qy = b.Y - ny * at;

                canvas.DrawLine(
                    (float)px, (float)py, (float)(px + ux * segment), (float)(py + uy * segment), line);
                canvas.DrawLine(
                    (float)(qx - ux * segment), (float)(qy - uy * segment), (float)qx, (float)qy, line);

                // When the step is fine the labels would sit on top of one another at one end, so
                // they alternate ends and each end sees half of them.
                var atFar = step < 2 * MinStep && k % 2 == 1;
                var lx = atFar ? qx - ux * (segment + 4) : px + ux * (segment + 4);
                var ly = atFar ? qy - uy * (segment + 4) : py + uy * (segment + 4);
                Tick(canvas, text, plate, at.ToString(), (float)lx, (float)ly, colour, atFar);
            }

            // Outward labels live in the middle of the edge, spread along it: that space is outside
            // the box, so nothing is hidden, and it keeps them clear of the inward labels at the ends.
            var midX = (a.X + b.X) / 2;
            var midY = (a.Y + b.Y) / 2;
            var spread = Math.Min(26.0, length / (outward.Length + 1));

            for (var k = 0; k < outward.Length; k++)
            {
                var at = outward[k];
                var inset = length * (1 - OutwardSpan) / 2;
                double px = a.X + nx * at + ux * inset, py = a.Y + ny * at + uy * inset;
                double qx = b.X + nx * at - ux * inset, qy = b.Y + ny * at - uy * inset;
                canvas.DrawLine((float)px, (float)py, (float)qx, (float)qy, line);

                var along = (k - (outward.Length - 1) / 2.0) * spread;
                var lx = midX + nx * at + ux * along;
                var ly = midY + ny * at + uy * along;
                Tick(canvas, text, plate, at.ToString(), (float)lx, (float)ly, colour, false);
            }
        }

        canvas.Restore();
    }

    /// <summary>
    /// One number, on a plate in its own edge's colour. The colour is the whole point: a line belonging
    /// to the green edge is green, so "10 from green" needs no word for "left" and cannot be read
    /// against the view grid by mistake. The digits go black or white by the plate's lightness, since
    /// the palette deliberately spans lightness and one text colour cannot serve all four.
    /// </summary>
    private static void Tick(
        SKCanvas canvas, SKPaint text, SKPaint plate, string value, float x, float y, SKColor colour,
        bool rightAligned)
    {
        var width = text.MeasureText(value);
        var metrics = text.FontMetrics;
        var left = rightAligned ? x - width : x;
        var top = y + metrics.Ascent - 1;
        var bottom = y + metrics.Descent + 1;

        plate.Color = colour.WithAlpha(0xE6);
        canvas.DrawRect(new SKRect(left - 2, top, left + width + 2, bottom), plate);

        var luminance = 0.299 * colour.Red + 0.587 * colour.Green + 0.114 * colour.Blue;
        text.Color = luminance > 140 ? SKColors.Black : SKColors.White;
        canvas.DrawText(value, left, y, text);
    }
}
