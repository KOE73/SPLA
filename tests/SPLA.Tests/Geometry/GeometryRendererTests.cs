using SkiaSharp;
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

        // Inside the box, away from every edge: the outline is a stroke, not a fill.
        var inside = decoded.GetPixel(200, 150);
        Assert.Equal(background.Red, inside.Red);
    }

    [Fact]
    public void Status_changes_the_colour_and_nothing_else_does()
    {
        using var session = Open();
        var box = new Obb(200, 150, 100, 60, 0);
        session.Objects.Add(new GeometryObject { Name = "a", Kind = ObjectKind.Box, Box = box });

        var cfg = new GeometrySettings();
        using var editing = Decode(GeometryRenderer.Render(session, session.CurrentView, false, cfg));

        session.Objects[0].Status = ObjectStatus.Accepted;
        using var accepted = Decode(GeometryRenderer.Render(session, session.CurrentView, false, cfg));

        // Editing is red-dominant, accepted green-dominant, on the same outline pixel.
        var before = editing.GetPixel(200, 120);
        var after = accepted.GetPixel(200, 120);
        Assert.True(before.Red > before.Green);
        Assert.True(after.Green > after.Red);
    }

    [Fact]
    public void Two_objects_of_the_same_status_are_drawn_in_the_same_colour()
    {
        // Colour carries status, not identity — the name label is what distinguishes objects.
        using var session = Open();
        session.Objects.Add(new GeometryObject { Name = "a", Kind = ObjectKind.Box, Box = new Obb(100, 80, 60, 40, 0) });
        session.Objects.Add(new GeometryObject { Name = "b", Kind = ObjectKind.Box, Box = new Obb(300, 220, 60, 40, 0) });

        using var decoded = Decode(GeometryRenderer.Render(session, session.CurrentView, false, new GeometrySettings()));

        var first = decoded.GetPixel(100, 60);
        var second = decoded.GetPixel(300, 200);
        Assert.Equal(first.Red, second.Red);
        Assert.Equal(first.Green, second.Green);
        Assert.Equal(first.Blue, second.Blue);
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

        Assert.NotEqual(background.Red, decoded.GetPixel(208, 150).Red);   // right arm
        Assert.NotEqual(background.Red, decoded.GetPixel(200, 142).Red);   // upper arm
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
