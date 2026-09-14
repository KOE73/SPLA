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
/// <b>Colour carries status, not identity.</b> An editing object is saturated, an accepted one muted;
/// two different objects of the same status look the same on purpose and are told apart by the name
/// printed beside them. A palette per object would make the model reason about a legend instead of
/// about the picture.
/// </para>
/// </summary>
internal static class GeometryRenderer
{
    /// <summary>Vivid: this one is still being placed.</summary>
    private static readonly SKColor EditingColor = new(0xFF, 0x3B, 0x30);

    /// <summary>Muted: this one is settled and only there for context.</summary>
    private static readonly SKColor AcceptedColor = new(0x34, 0xC7, 0x59);

    private static readonly SKColor GridColor = new(0xFF, 0xFF, 0xFF, 0x66);

    private static readonly SKColor LabelPlate = new(0x00, 0x00, 0x00, 0xC0);

    /// <summary>Spacing of the optional debug grid, in view pixels.</summary>
    private const int GridStep = 100;

    /// <summary>Half-length of a point's crosshair arms, in view pixels. A point is never drawn as a
    /// pixel: a one-pixel dot is invisible to the model, which defeats the purpose of rendering at
    /// all.</summary>
    private const float CrossArm = 12f;

    public static byte[] Render(GeometrySession session, GeometryView view, bool grid, GeometrySettings cfg)
    {
        using var bitmap = new SKBitmap(view.Width, view.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Black);
            DrawFrame(canvas, session.Source, view);

            if (grid) DrawGrid(canvas, view, cfg);

            foreach (var obj in session.Objects.Where(o => IsVisible(o, view)))
                DrawObject(canvas, obj, view, cfg);
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

    private static void DrawObject(SKCanvas canvas, GeometryObject obj, GeometryView view, GeometrySettings cfg)
    {
        var accepted = obj.Status == ObjectStatus.Accepted;
        var color = accepted ? AcceptedColor : EditingColor;
        // An accepted outline steps back rather than disappearing: it is context for placing the
        // next object, not the subject of the current look.
        var width = Math.Max(1f, accepted ? cfg.LineWidth * 0.6f : cfg.LineWidth);

        using var stroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = width,
            Color = color,
            IsAntialias = true,
            StrokeJoin = SKStrokeJoin.Round,
        };

        var t = view.SourceToView;
        float labelX, labelY;

        if (obj.Kind == ObjectKind.Box && obj.Box is { } box)
        {
            var corners = box.Transformed(t).Corners();
            using var path = new SKPath();
            path.MoveTo((float)corners[0].X, (float)corners[0].Y);
            for (var i = 1; i < 4; i++) path.LineTo((float)corners[i].X, (float)corners[i].Y);
            path.Close();
            canvas.DrawPath(path, stroke);

            labelX = (float)corners.Min(c => c.X);
            labelY = (float)corners.Min(c => c.Y) - 4;
        }
        else
        {
            var (px, py) = t.Apply(obj.Point.X, obj.Point.Y);
            float x = (float)px, y = (float)py;
            canvas.DrawLine(x - CrossArm, y, x + CrossArm, y, stroke);
            canvas.DrawLine(x, y - CrossArm, x, y + CrossArm, stroke);
            canvas.DrawCircle(x, y, CrossArm * 0.55f, stroke);

            labelX = x + CrossArm + 2;
            labelY = y - CrossArm * 0.6f;
        }

        DrawLabel(canvas, obj.Name, labelX, labelY, color, cfg);
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

    /// <summary>Optional debug grid in view coordinates. Off by default: it was a crutch for guessing
    /// coordinates in one shot, and the place → look → correct loop is what replaced that (ADR §3.6).
    /// It stays useful for one thing — checking that the provider did not resize the picture under us.</summary>
    private static void DrawGrid(SKCanvas canvas, GeometryView view, GeometrySettings cfg)
    {
        using var line = new SKPaint { Color = GridColor, StrokeWidth = 1, IsAntialias = false };
        using var label = new SKPaint
        {
            Color = SKColors.White,
            TextSize = Math.Max(10, cfg.FontSize - 4),
            IsAntialias = true,
            Typeface = SKTypeface.Default,
        };
        using var plate = new SKPaint { Color = LabelPlate, Style = SKPaintStyle.Fill };

        for (var x = GridStep; x < view.Width; x += GridStep)
        {
            canvas.DrawLine(x, 0, x, view.Height, line);
            Tick(canvas, label, plate, x.ToString(), x + 2, label.TextSize + 2);
        }

        for (var y = GridStep; y < view.Height; y += GridStep)
        {
            canvas.DrawLine(0, y, view.Width, y, line);
            Tick(canvas, label, plate, y.ToString(), 2, y - 2);
        }
    }

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
