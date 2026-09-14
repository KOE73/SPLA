using SkiaSharp;
using System;
using SPLA.Plugins.Geometry;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Render;
using SPLA.Plugins.Geometry.Session;

namespace SPLA.Tests.Geometry;

/// <summary>
/// The render is the only thing the model ever sees, so the properties pinned here are the ones it
/// depends on: the picture is exactly the size the reply claims, something is actually drawn where
/// the box is, and the frame is left alone everywhere else.
/// </summary>
public sealed class GeometryRendererTests
{
    private static byte[] Frame(int width, int height, SKColor fill)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(fill);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static GeometrySession Open(int width = 400, int height = 300, GeometrySettings? cfg = null)
    {
        var session = GeometrySession.Open(
            Frame(width, height, new SKColor(0x80, 0x80, 0x80)), "test://frame.png", null,
            cfg ?? new GeometrySettings(), out var error);
        Assert.Null(error);
        return session!;
    }

    private static SKBitmap Decode(byte[] bytes) => SKBitmap.Decode(bytes);

    [Fact]
    public void A_render_is_exactly_the_size_the_view_announces()
    {
        using var session = Open(2048, 1024);
        var bytes = GeometryRenderer.Render(session, session.CurrentView, grid: false, new GeometrySettings());

        using var decoded = Decode(bytes);
        Assert.Equal(session.CurrentView.Width, decoded.Width);
        Assert.Equal(session.CurrentView.Height, decoded.Height);
        Assert.Equal(1024, decoded.Width);
    }

    [Fact]
    public void A_box_is_drawn_on_its_outline_and_nowhere_else()
    {
        using var session = Open();
        session.Objects.Add(new GeometryObject
        {
            Name = "bag",
            Kind = ObjectKind.Box,
            Box = new Obb(200, 150, 100, 60, 0),
        });

        var cfg = new GeometrySettings();
        var bytes = GeometryRenderer.Render(session, session.CurrentView, grid: false, cfg);
        using var decoded = Decode(bytes);

        var background = new SKColor(0x80, 0x80, 0x80);

        // Middle of the top edge: y = 150 - 30 = 120, x = 200.
        Assert.NotEqual(background.Red, decoded.GetPixel(200, 120).Red);

        // Far from the outline and from the label: untouched frame.
        var away = decoded.GetPixel(30, 280);
        Assert.Equal(background.Red, away.Red);
        Assert.Equal(background.Green, away.Green);
        Assert.Equal(background.Blue, away.Blue);

        // Inside the box, away from every edge and from the centre mark: the outline is a stroke,
        // not a fill.
        var inside = decoded.GetPixel(225, 135);
        Assert.Equal(background.Red, inside.Red);
    }

    [Fact]
    public void The_editing_box_carries_the_four_edge_colours_and_an_accepted_one_does_not()
    {
        // The mapping pinned here is the one the reply text names and the model is told to call by:
        // cyan=top, magenta=right, yellow=bottom, green=left, bound to the box's own axes.
        using var session = Open();
        session.Objects.Add(new GeometryObject
        {
            Name = "a", Kind = ObjectKind.Box, Box = new Obb(200, 150, 120, 80, 0),
        });

        var cfg = new GeometrySettings();
        using var editing = Decode(GeometryRenderer.Render(session, session.CurrentView, false, cfg));

        AssertNear(editing.GetPixel(200, 110), 0x00, 0xCF, 0xFF);   // top    - cyan
        AssertNear(editing.GetPixel(260, 150), 0xFF, 0x2B, 0xD6);   // right  - magenta
        AssertNear(editing.GetPixel(200, 190), 0xFF, 0xE1, 0x00);   // bottom - yellow
        AssertNear(editing.GetPixel(140, 150), 0x0E, 0x8A, 0x26);   // left   - green

        session.Objects[0].Status = ObjectStatus.Accepted;
        using var accepted = Decode(GeometryRenderer.Render(session, session.CurrentView, false, cfg));

        // One muted colour all the way round: no edge of an accepted box can be read as an edge
        // of the box being placed.
        AssertNear(accepted.GetPixel(200, 110), 0x7A, 0x8C, 0xA0);
        AssertNear(accepted.GetPixel(200, 190), 0x7A, 0x8C, 0xA0);
    }

    [Fact]
    public void The_edge_colours_turn_with_the_box()
    {
        // Ninety degrees clockwise: the box's own top edge now faces screen-right, and the cyan
        // must have gone with it. This is what side names like "left" could not have given us.
        using var session = Open();
        session.Objects.Add(new GeometryObject
        {
            Name = "a", Kind = ObjectKind.Box, Box = new Obb(200, 150, 120, 80, 90),
        });

        using var decoded = Decode(GeometryRenderer.Render(session, session.CurrentView, false, new GeometrySettings()));

        AssertNear(decoded.GetPixel(240, 150), 0x00, 0xCF, 0xFF);   // the top edge, now on the right
        AssertNear(decoded.GetPixel(160, 150), 0xFF, 0xE1, 0x00);   // the bottom edge, now on the left
    }

    [Fact]
    public void The_editing_box_marks_its_centre()
    {
        // dx/dy move exactly this point, and until it was drawn the model was correcting blind.
        using var session = Open();
        session.Objects.Add(new GeometryObject
        {
            Name = "a", Kind = ObjectKind.Box, Box = new Obb(200, 150, 120, 80, 0),
        });

        var cfg = new GeometrySettings();
        using var editing = Decode(GeometryRenderer.Render(session, session.CurrentView, false, cfg));
        AssertNear(editing.GetPixel(200, 150), 0xFF, 0xFF, 0xFF);

        session.Objects[0].Status = ObjectStatus.Accepted;
        using var accepted = Decode(GeometryRenderer.Render(session, session.CurrentView, false, cfg));
        Assert.Equal(0x80, accepted.GetPixel(200, 150).Red);   // untouched frame
    }

    [Fact]
    public void Every_corner_carries_the_colour_of_the_edge_that_starts_there()
    {
        using var session = Open();
        session.Objects.Add(new GeometryObject
        {
            Name = "a", Kind = ObjectKind.Box, Box = new Obb(200, 150, 120, 80, 0),
        });

        using var decoded = Decode(GeometryRenderer.Render(session, session.CurrentView, false, new GeometrySettings()));

        AssertNear(decoded.GetPixel(140, 110), 0x00, 0xCF, 0xFF);   // top-left,     cyan starts here
        AssertNear(decoded.GetPixel(260, 110), 0xFF, 0x2B, 0xD6);   // top-right,    magenta
        AssertNear(decoded.GetPixel(260, 190), 0xFF, 0xE1, 0x00);   // bottom-right, yellow
        AssertNear(decoded.GetPixel(140, 190), 0x0E, 0x8A, 0x26);   // bottom-left,  green
    }

    private static void AssertNear(SKColor actual, byte r, byte g, byte b, int tolerance = 40)
    {
        Assert.True(
            Math.Abs(actual.Red - r) <= tolerance
            && Math.Abs(actual.Green - g) <= tolerance
            && Math.Abs(actual.Blue - b) <= tolerance,
            $"expected about #{r:X2}{g:X2}{b:X2}, got #{actual.Red:X2}{actual.Green:X2}{actual.Blue:X2}");
    }

    [Fact]
    public void A_point_is_drawn_wide_enough_to_be_seen()
    {
        // The whole reason the renderer exists is that the model has to see the mark. A single
        // pixel would be invisible; the crosshair arms must reach several pixels out.
        using var session = Open();
        session.Objects.Add(new GeometryObject { Name = "mark", Kind = ObjectKind.Point, Point = (200, 150) });

        using var decoded = Decode(GeometryRenderer.Render(session, session.CurrentView, false, new GeometrySettings()));
        var background = new SKColor(0x80, 0x80, 0x80);

        // The arms run diagonally: a point is an X in a circle, a box's centre is a dot in a ring.
        // Two meanings must not share one glyph (ADR_20260914-3 §3.2).
        Assert.NotEqual(background.Red, decoded.GetPixel(206, 156).Red);   // lower-right arm
        Assert.NotEqual(background.Red, decoded.GetPixel(194, 144).Red);   // upper-left arm
        Assert.Equal(background.Red, decoded.GetPixel(200, 250).Red);      // well clear of it
    }

    [Fact]
    public void An_object_outside_the_view_is_reported_as_such_and_not_drawn()
    {
        using var session = Open();
        var outside = new GeometryObject { Name = "far", Kind = ObjectKind.Point, Point = (5000, 5000) };
        var inside = new GeometryObject { Name = "near", Kind = ObjectKind.Point, Point = (200, 150) };

        Assert.False(GeometryRenderer.IsVisible(outside, session.CurrentView));
        Assert.True(GeometryRenderer.IsVisible(inside, session.CurrentView));
    }

    [Fact]
    public void The_grid_is_drawn_only_when_asked_for()
    {
        using var session = Open();
        var cfg = new GeometrySettings();

        using var plain = Decode(GeometryRenderer.Render(session, session.CurrentView, false, cfg));
        using var ruled = Decode(GeometryRenderer.Render(session, session.CurrentView, true, cfg));

        var background = new SKColor(0x80, 0x80, 0x80);
        Assert.Equal(background.Red, plain.GetPixel(100, 250).Red);
        Assert.NotEqual(background.Red, ruled.GetPixel(100, 250).Red);
    }

    [Fact]
    public void Jpeg_quality_switches_the_encoding()
    {
        using var session = Open();

        var png = GeometryRenderer.Render(session, session.CurrentView, false, new GeometrySettings());
        var jpeg = GeometryRenderer.Render(session, session.CurrentView, false,
            GeometrySettings.FromBlob(new() { ["jpeg_quality"] = 80 }));

        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, png[..4]);
        Assert.Equal(new byte[] { 0xFF, 0xD8 }, jpeg[..2]);
        Assert.Equal("image/png", GeometryRenderer.MimeType(new GeometrySettings()));
    }
}
