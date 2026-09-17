using SkiaSharp;
using System;

namespace SPLA.Tests.Geometry;

/// <summary>
/// Frames built in code from a declared ground truth, so a test can check the numbers the pipeline
/// recovers against the numbers the picture was drawn from.
/// <para>
/// Everything up to here proves the plumbing is self-consistent: a coordinate survives a round trip
/// through the transforms. That is not the same as being right. A renderer that drew every outline
/// twenty pixels to the left would pass all of it. These frames close that gap — the truth is known
/// by construction, and it is known in <b>source</b> pixels, which is the one space the model never
/// speaks in.
/// </para>
/// <para>
/// No fixture files are committed: a PNG in the repository is a number nobody can check against the
/// code that produced it.
/// </para>
/// </summary>
internal static class SyntheticFixtures
{
    public static readonly SKColor Background = new(0x80, 0x80, 0x80);

    /// <summary>The large object — the "bag" of the leading use case.</summary>
    public static readonly SKColor BagColor = new(0x1E, 0x64, 0xC8);

    /// <summary>The small object inside it — the "marking".</summary>
    public static readonly SKColor MarkColor = new(0xE8, 0xA0, 0x20);

    /// <summary>The crosshair. Deliberately a colour nothing else on the frame comes near, so it can
    /// be found in the rendered pixels by colour alone — including nothing the renderer itself draws:
    /// it used to be magenta, which is now the colour of the editing box's right-hand edge, and the
    /// outline would have been counted as part of the crosshair.</summary>
    public static readonly SKColor CrossColor = new(0xFF, 0x00, 0x00);

    /// <summary>Half-length of the crosshair arms, in source pixels.</summary>
    private const float CrossArm = 26f;

    private const float CrossThickness = 7f;

    /// <summary>
    /// A frame and the truth it was drawn from. Every coordinate here is in SOURCE pixels.
    /// </summary>
    internal sealed record SyntheticFrame(
        byte[] Png,
        int Width,
        int Height,
        double BagCx,
        double BagCy,
        double BagWidth,
        double BagHeight,
        double BagAngle,
        double MarkCx,
        double MarkCy,
        double MarkWidth,
        double MarkHeight,
        double CrossX,
        double CrossY);

    /// <summary>
    /// The standard frame: a grey ground, a rotated rectangle on it, a smaller rotated rectangle
    /// inside that one, and a crosshair at a third known point. The inner rectangle and the crosshair
    /// sit on either side of the bag's centre, so the centre itself stays bag-coloured and can be used
    /// as a pixel probe.
    /// </summary>
    public static SyntheticFrame Standard(int width = 1600, int height = 1200)
    {
        double cx = width / 2.0, cy = height / 2.0;
        const double bagWidth = 600, bagHeight = 400, angle = 20;
        const double markWidth = 160, markHeight = 100;

        // Offsets in the bag's own frame, turned into source coordinates by hand — plain trigonometry,
        // so the fixture owes nothing to the transform code it is used to test.
        var (markCx, markCy) = Rotate(cx, cy, 150, 62, angle);
        var (crossX, crossY) = Rotate(cx, cy, -160, -70, angle);

        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(Background);

            using var fill = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };

            canvas.Save();
            canvas.RotateDegrees((float)angle, (float)cx, (float)cy);

            fill.Color = BagColor;
            canvas.DrawRect(Centred((float)cx, (float)cy, (float)bagWidth, (float)bagHeight), fill);

            fill.Color = MarkColor;
            canvas.DrawRect(Centred((float)cx + 150, (float)cy + 62, (float)markWidth, (float)markHeight), fill);

            canvas.Restore();

            // The crosshair is drawn upright: it marks a point, and a point has no orientation.
            fill.Color = CrossColor;
            canvas.DrawRect(
                new SKRect((float)crossX - CrossArm, (float)crossY - CrossThickness / 2,
                           (float)crossX + CrossArm, (float)crossY + CrossThickness / 2), fill);
            canvas.DrawRect(
                new SKRect((float)crossX - CrossThickness / 2, (float)crossY - CrossArm,
                           (float)crossX + CrossThickness / 2, (float)crossY + CrossArm), fill);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return new SyntheticFrame(
            data.ToArray(), width, height,
            cx, cy, bagWidth, bagHeight, angle,
            markCx, markCy, markWidth, markHeight,
            crossX, crossY);
    }

    private static SKRect Centred(float cx, float cy, float width, float height) =>
        new(cx - width / 2, cy - height / 2, cx + width / 2, cy + height / 2);

    /// <summary>A point given in the bag's own frame, expressed in source pixels. Clockwise, because
    /// image y grows downward.</summary>
    private static (double X, double Y) Rotate(double cx, double cy, double dx, double dy, double degrees)
    {
        var rad = degrees * Math.PI / 180.0;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);
        return (cx + dx * cos - dy * sin, cy + dx * sin + dy * cos);
    }

    // ── reading pixels back ───────────────────────────────────────────────────

    /// <summary>True when the pixel at <paramref name="x"/>,<paramref name="y"/> is that colour, give
    /// or take the antialiasing and the encoder.</summary>
    public static bool IsColorAt(SKBitmap bitmap, double x, double y, SKColor expected, int tolerance = 24)
    {
        var ix = (int)Math.Round(x);
        var iy = (int)Math.Round(y);
        if (ix < 0 || iy < 0 || ix >= bitmap.Width || iy >= bitmap.Height) return false;
        return Near(bitmap.GetPixel(ix, iy), expected, tolerance);
    }

    /// <summary>The centre of mass of every pixel of <paramref name="color"/> in the picture, or null if
    /// there are none. This is how a test asks the rendered image itself where something is, instead of
    /// recomputing it with the same arithmetic it is trying to check.</summary>
    public static (double X, double Y)? Centroid(SKBitmap bitmap, SKColor color, int tolerance = 48)
    {
        double sumX = 0, sumY = 0;
        var count = 0;
        for (var y = 0; y < bitmap.Height; y++)
        for (var x = 0; x < bitmap.Width; x++)
        {
            if (!Near(bitmap.GetPixel(x, y), color, tolerance)) continue;
            sumX += x;
            sumY += y;
            count++;
        }

        return count == 0 ? null : (sumX / count, sumY / count);
    }

    private static bool Near(SKColor actual, SKColor expected, int tolerance) =>
        Math.Abs(actual.Red - expected.Red) <= tolerance
        && Math.Abs(actual.Green - expected.Green) <= tolerance
        && Math.Abs(actual.Blue - expected.Blue) <= tolerance;
}
