using SkiaSharp;
using SPLA.Plugins.Geometry;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Session;

namespace SPLA.Tests.Geometry;

/// <summary>
/// The crop stack is where a silent off-by-a-transform bug would live: everything still looks
/// plausible, and the coordinates are simply wrong. So each test goes the full way round — place
/// something while looking through a crop, then check where it landed in the source.
/// </summary>
public sealed class GeometryViewTests
{
    private static byte[] Frame(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.DarkGray);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static GeometrySession Open(int width = 2000, int height = 1500)
    {
        var session = GeometrySession.Open(Frame(width, height), "test://frame.png", null, new GeometrySettings(), out var error);
        Assert.Null(error);
        return session!;
    }

    [Fact]
    public void A_crop_to_a_box_enlarges_it_to_the_working_size()
    {
        // The root view never enlarges; a crop always does, because magnifying is what it is for.
        using var session = Open();
        var box = new GeometryObject { Name = "bag", Kind = ObjectKind.Box, Box = new Obb(1000, 750, 400, 200, 0) };

        var crop = ViewBuilder.Crop(session, box, "source", padding: 0, deskew: false, new GeometrySettings());

        Assert.Equal(1024, crop.Width);
        Assert.Equal(512, crop.Height);
        Assert.Equal("bag", crop.FromBox);
        Assert.Equal("source", crop.ParentId);

        // The box's own centre sits at the centre of the crop.
        var centre = crop.SourceToView.Apply(1000, 750);
        Assert.Equal(512, centre.X, 0.5);
        Assert.Equal(256, centre.Y, 0.5);
    }

    [Fact]
    public void Padding_leaves_a_margin_around_the_box()
    {
        using var session = Open();
        var box = new GeometryObject { Name = "bag", Kind = ObjectKind.Box, Box = new Obb(1000, 750, 400, 200, 0) };

        var crop = ViewBuilder.Crop(session, box, "source", padding: 0.25, deskew: false, new GeometrySettings());

        // 400 wide + 25% each side = 600 source px mapped onto 1024 view px.
        var left = crop.SourceToView.Apply(800, 750);
        var right = crop.SourceToView.Apply(1200, 750);
        Assert.Equal(1024 * 100.0 / 600, left.X, 0.5);
        Assert.Equal(1024 * 500.0 / 600, right.X, 0.5);
    }

    [Fact]
    public void A_point_placed_inside_a_crop_lands_where_it_was_meant_to_in_the_source()
    {
        using var session = Open();
        var box = new GeometryObject { Name = "bag", Kind = ObjectKind.Box, Box = new Obb(1000, 750, 400, 200, 0) };
        var crop = ViewBuilder.Crop(session, box, "source", padding: 0, deskew: false, new GeometrySettings());

        // The model, looking at the 1024x512 crop, says "here" a quarter across and a quarter down.
        var inSource = crop.ViewToSource.Apply(256, 128);

        Assert.Equal(900, inSource.X, 1);
        Assert.Equal(700, inSource.Y, 1);

        // And the tool shows it back in the same place it was named.
        var backInView = crop.SourceToView.Apply(inSource.X, inSource.Y);
        Assert.Equal(256, backInView.X, 1);
        Assert.Equal(128, backInView.Y, 1);
    }

    [Fact]
    public void Deskew_turns_an_angled_box_upright_without_moving_it()
    {
        using var session = Open();
        var box = new Obb(1000, 750, 400, 200, 30);
        var obj = new GeometryObject { Name = "bag", Kind = ObjectKind.Box, Box = box };

        var crop = ViewBuilder.Crop(session, obj, "source", padding: 0, deskew: true, new GeometrySettings());

        Assert.True(crop.Deskewed);

        var seen = box.Transformed(crop.SourceToView);
        Assert.Equal(0, seen.AngleDeg, 1e-6);              // upright in the picture
        Assert.Equal(crop.Width / 2.0, seen.Cx, 0.5);      // and centred in it
        Assert.Equal(crop.Height / 2.0, seen.Cy, 0.5);
        Assert.Equal(crop.Width, seen.Width, 1);           // filling it edge to edge at padding 0
    }

    [Fact]
    public void A_box_marked_inside_a_deskewed_crop_comes_back_rotated_in_the_source()
    {
        using var session = Open();
        var outer = new GeometryObject
        {
            Name = "bag",
            Kind = ObjectKind.Box,
            Box = new Obb(1000, 750, 400, 200, 30),
        };
        var crop = ViewBuilder.Crop(session, outer, "source", padding: 0, deskew: true, new GeometrySettings());

        // Inside the upright crop the model marks an upright box — the marking on the angled bag.
        var marked = new Obb(crop.Width / 2.0, crop.Height / 2.0, 200, 100, 0);
        var inSource = marked.Transformed(crop.ViewToSource);

        Assert.Equal(1000, inSource.Cx, 1);
        Assert.Equal(750, inSource.Cy, 1);
        Assert.Equal(30, inSource.AngleDeg, 1e-6);   // it inherits the bag's tilt, as it must
        Assert.Equal(400 * 200.0 / crop.Width, inSource.Width, 1);
    }

    [Fact]
    public void A_rectangle_crop_is_read_in_the_pixels_of_the_view_it_was_named_in()
    {
        using var session = Open();
        var root = session.CurrentView;   // 2000x1500 source -> 1024x768 root view

        var crop = ViewBuilder.Crop(session, root, 100, 200, 256, 128, new GeometrySettings());

        Assert.Equal(1024, crop.Width);
        Assert.Equal(512, crop.Height);
        Assert.Equal(root.Id, crop.ParentId);
        Assert.Null(crop.FromBox);

        // The rectangle's own top-left corner is the crop's origin.
        var source = root.ViewToSource.Apply(100, 200);
        var corner = crop.SourceToView.Apply(source.X, source.Y);
        Assert.Equal(0, corner.X, 0.5);
        Assert.Equal(0, corner.Y, 0.5);
    }

    [Fact]
    public void A_crop_of_a_crop_still_agrees_with_the_source()
    {
        using var session = Open();
        var box = new GeometryObject { Name = "bag", Kind = ObjectKind.Box, Box = new Obb(1000, 750, 400, 200, 0) };

        var first = ViewBuilder.Crop(session, box, "source", 0, false, new GeometrySettings());
        session.Views.Add(first);
        var second = ViewBuilder.Crop(session, first, 0, 0, 256, 256, new GeometrySettings());

        Assert.Equal("crop_1", first.Id);
        Assert.Equal("crop_2", second.Id);

        // A point named in the innermost view resolves to the same source pixel either way round.
        var viaSecond = second.ViewToSource.Apply(512, 512);
        var viaFirst = first.ViewToSource.Apply(128, 128);
        Assert.Equal(viaFirst.X, viaSecond.X, 0.5);
        Assert.Equal(viaFirst.Y, viaSecond.Y, 0.5);
    }

    [Fact]
    public void A_crop_around_a_point_is_a_square_of_the_padded_size()
    {
        using var session = Open(2000, 1500);
        var point = new GeometryObject { Name = "mark", Kind = ObjectKind.Point, Point = (1000, 750) };

        var crop = ViewBuilder.Crop(session, point, "source", padding: 0.05, deskew: false, new GeometrySettings());

        // 2 x 0.05 x min(2000, 1500) = 150 source px, square.
        Assert.Equal(crop.Width, crop.Height);
        var centre = crop.ViewToSource.Apply(crop.Width / 2.0, crop.Height / 2.0);
        Assert.Equal(1000, centre.X, 1);
        Assert.Equal(750, centre.Y, 1);

        var span = crop.ViewToSource.Apply(crop.Width, 0).X - crop.ViewToSource.Apply(0, 0).X;
        Assert.Equal(150, span, 1);
    }
}
