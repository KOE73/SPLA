using SPLA.Plugins.Geometry.Model;

namespace SPLA.Tests.Geometry;

/// <summary>
/// The corner order is part of what <c>geom_result</c> publishes to disambiguate the angle
/// convention, so it is pinned here against a hand-computed expectation rather than against the
/// implementation.
/// </summary>
public sealed class ObbTests
{
    private const double Tol = 1e-9;

    [Fact]
    public void Corners_of_an_unrotated_box_are_the_plain_rectangle()
    {
        var corners = new Obb(100, 50, 40, 20, 0).Corners();

        Assert.Equal((80.0, 40.0), Round(corners[0]));   // top-left
        Assert.Equal((120.0, 40.0), Round(corners[1]));  // top-right
        Assert.Equal((120.0, 60.0), Round(corners[2]));  // bottom-right
        Assert.Equal((80.0, 60.0), Round(corners[3]));   // bottom-left
    }

    [Fact]
    public void Corners_at_90_degrees_match_the_hand_calculation()
    {
        // +90 in image coordinates (y down) turns clockwise: the old top-left lands top-right.
        var corners = new Obb(0, 0, 40, 20, 90).Corners();

        Assert.Equal((10.0, -20.0), Round(corners[0]));
        Assert.Equal((10.0, 20.0), Round(corners[1]));
        Assert.Equal((-10.0, 20.0), Round(corners[2]));
        Assert.Equal((-10.0, -20.0), Round(corners[3]));
    }

    [Fact]
    public void Corners_stay_the_declared_size_apart_under_rotation()
    {
        var c = new Obb(470, 325, 360, 180, -37.5).Corners();

        Assert.Equal(360, Distance(c[0], c[1]), 1e-9);
        Assert.Equal(180, Distance(c[1], c[2]), 1e-9);
        Assert.Equal(360, Distance(c[2], c[3]), 1e-9);
        Assert.Equal(180, Distance(c[3], c[0]), 1e-9);
    }

    [Fact]
    public void Transformed_carries_centre_size_and_angle()
    {
        var box = new Obb(100, 100, 40, 20, 10);
        var t = Affine.RotateDeg(20).Then(Affine.Scale(2)).Then(Affine.Translate(5, -5));

        var moved = box.Transformed(t);
        var expectedCentre = t.Apply(100, 100);

        Assert.Equal(expectedCentre.X, moved.Cx, Tol);
        Assert.Equal(expectedCentre.Y, moved.Cy, Tol);
        Assert.Equal(80, moved.Width, Tol);
        Assert.Equal(40, moved.Height, Tol);
        Assert.Equal(30, moved.AngleDeg, Tol);
    }

    [Fact]
    public void Transformed_round_trips_through_the_inverse()
    {
        var box = new Obb(470, 325, 360, 180, -6);
        var t = Affine.Translate(-470, -325).Then(Affine.RotateDeg(6)).Then(Affine.Scale(0.5));

        var back = box.Transformed(t).Transformed(t.Invert());

        Assert.Equal(box.Cx, back.Cx, Tol);
        Assert.Equal(box.Cy, back.Cy, Tol);
        Assert.Equal(box.Width, back.Width, Tol);
        Assert.Equal(box.Height, back.Height, Tol);
        Assert.Equal(box.AngleDeg, back.AngleDeg, Tol);
    }

    [Fact]
    public void Transforming_the_centre_and_transforming_the_corners_agree()
    {
        // The two paths through the code must not drift: a corner of the transformed box is the
        // transform of that corner of the original.
        var box = new Obb(200, 150, 80, 40, 25);
        var t = Affine.Translate(-30, 12).Then(Affine.RotateDeg(-15)).Then(Affine.Scale(1.75));

        var viaBox = box.Transformed(t).Corners();
        var viaCorners = box.Corners().Select(c => t.Apply(c.X, c.Y)).ToArray();

        for (var i = 0; i < 4; i++)
        {
            Assert.Equal(viaCorners[i].X, viaBox[i].X, 1e-9);
            Assert.Equal(viaCorners[i].Y, viaBox[i].Y, 1e-9);
        }
    }

    private static double Distance((double X, double Y) a, (double X, double Y) b)
        => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    private static (double, double) Round((double X, double Y) p)
        => (Math.Round(p.X, 9), Math.Round(p.Y, 9));
}
