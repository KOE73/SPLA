using SkiaSharp;
using SPLA.Domain.Security;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Render;
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

    /// <summary>The probing in progress (<c>geom_probe</c>), or null. One at a time: a round is a question
    /// about one picture, and the model answers the last picture it was shown.</summary>
    public ProbeState? Probe { get; set; }

    /// <summary>The probe look <c>geom_probe_legibility</c> found this model reads, or null for the settings'.</summary>
    public ProbeStyle? ProbeStyle { get; set; }

    /// <summary>The legibility chart on the last picture, waiting for its answer, or null.</summary>
    public LegibilityChart? Chart { get; set; }

    private int _probeRounds;

    /// <summary>The next probe round's number. Never reused in a session, so an answer written for an
    /// older picture cannot match a newer one by accident (ADR_20260916 §2.4).</summary>
    public int NextProbeRound() => ++_probeRounds;

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

    // ── the render ring ───────────────────────────────────────────────────────

    /// <summary>The name of the most recent render (<c>geom_render_&lt;n&gt;</c>), or null before the
    /// first one. The head of the ring: anything reading the history — the panel, a human in the
    /// debug window — needs to know which of the N names is newest, because the numbers wrap.</summary>
    public string? LastRenderName { get; private set; }

    private int _renders;

    /// <summary>
    /// The name the next render is stored under. Names rotate through <c>geom_render_1..history</c>,
    /// and the blob store overwrites an entry with the same name, so a chat holds at most
    /// <c>history</c> renders however long the markup loop runs.
    /// <para>The chat id is deliberately NOT part of the name: every chat has its own blob store
    /// (ChatRuntime builds an AgentSession per chat), so a chat id here would suggest a shared store
    /// that does not exist.</para>
    /// </summary>
    public string NextRenderName(int history)
    {
        if (history < 1) history = 1;
        var slot = _renders++ % history + 1;
        return LastRenderName = $"geom_render_{slot}";
    }

    /// <summary>The next free crop id — <c>crop_1</c>, <c>crop_2</c>, … Ids are never reused, since a
    /// view the model has already been shown must keep meaning what it meant.</summary>
    public string NextViewId() => $"crop_{Views.Count}";

    public void Dispose() => Source.Dispose();
}
