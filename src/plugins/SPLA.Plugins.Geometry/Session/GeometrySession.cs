using SkiaSharp;
using SPLA.Domain.Security;
using SPLA.Plugins.Geometry.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Plugins.Geometry.Session;

/// <summary>
/// One frame being marked up. The session — not the model — is the source of truth about geometry
/// (ADR §3.4): every tool call reads it, applies one operation, and renders what it now holds.
/// <para>
/// It lives in memory for the chat and does not survive a restart. That is the nature of the task,
/// not an economy: one frame is marked up, accepted and handed on, and the next frame starts clean
/// (ADR §3.5).
/// </para>
/// </summary>
internal sealed class GeometrySession : IDisposable
{
    public const string RootViewId = "source";

    private GeometrySession(SKBitmap source, string address, DataOrigin? origin, GeometryView root)
    {
        Source = source;
        SourceAddress = address;
        Origin = origin;
        Views.Add(root);
    }

    /// <summary>The decoded source image at its full size. Geometry is canonically in its pixels.</summary>
    public SKBitmap Source { get; }

    /// <summary>The address the frame was opened from, for the record and for <c>geom_result</c>.</summary>
    public string SourceAddress { get; }

    public List<GeometryObject> Objects { get; } = [];

    public List<GeometryView> Views { get; } = [];

    public string CurrentViewId { get; set; } = RootViewId;

    /// <summary>Where the frame came from, when the source said.</summary>
    public DataOrigin? Origin { get; }

    public GeometryView CurrentView =>
        Views.FirstOrDefault(v => v.Id == CurrentViewId) ?? Views[0];

    public GeometryView? View(string id) => Views.FirstOrDefault(v => v.Id == id);

    public GeometryObject? Object(string name) =>
        Objects.FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.Ordinal));

    /// <summary>
    /// Decodes the frame and builds the root view over it, or says why it could not.
    /// </summary>
    public static GeometrySession? Open(
        byte[] bytes, string address, DataOrigin? origin, GeometrySettings cfg, out string? error)
    {
        error = null;

        // Skia answers garbage either way — null for a codec it recognises but cannot finish, an
        // exception for bytes that are not an image at all. Both are the same news to the model.
        SKBitmap? bitmap;
        try
        {
            bitmap = SKBitmap.Decode(bytes);
        }
        catch (Exception)
        {
            bitmap = null;
        }

        if (bitmap is null)
        {
            error = $"'{address}' is not an image this tool can decode (PNG, JPEG, WEBP, BMP, GIF).";
            return null;
        }

        if (bitmap.Width <= 0 || bitmap.Height <= 0)
        {
            bitmap.Dispose();
            error = $"'{address}' decoded to an empty image.";
            return null;
        }

        return new GeometrySession(bitmap, address, origin, RootView(bitmap.Width, bitmap.Height, cfg));
    }

    /// <summary>
    /// The root view — and it is <b>not</b> the identity transform. The frame is delivered to the
    /// model at the working render size, and the size it sees is the coordinate space it answers in,
    /// so even <c>source</c> carries the scale from source pixels down to that size. An image already
    /// smaller than the working size is left alone rather than blown up: enlarging adds no detail and
    /// costs tokens.
    /// </summary>
    private static GeometryView RootView(int width, int height, GeometrySettings cfg)
    {
        var longest = Math.Max(width, height);
        var scale = longest > cfg.RenderMaxSide ? (double)cfg.RenderMaxSide / longest : 1.0;

        return new GeometryView
        {
            Id = RootViewId,
            ParentId = null,
            FromBox = null,
            SourceToView = Affine.Scale(scale),
            Width = Math.Max(1, (int)Math.Round(width * scale)),
            Height = Math.Max(1, (int)Math.Round(height * scale)),
        };
    }

    /// <summary>The next free crop id — <c>crop_1</c>, <c>crop_2</c>, … Ids are never reused, since a
    /// view the model has already been shown must keep meaning what it meant.</summary>
    public string NextViewId() => $"crop_{Views.Count}";

    public void Dispose() => Source.Dispose();
}
