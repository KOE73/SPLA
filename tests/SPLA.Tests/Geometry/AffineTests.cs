using SPLA.Plugins.Geometry.Model;

namespace SPLA.Tests.Geometry;

/// <summary>
/// Every coordinate the model ever passes goes through one of these matrices and comes back through
/// its inverse. A wrong composition order here produces coordinates that still look plausible — the
/// failure is silent — so the round trips below are deliberately built out of translate, rotate and
/// scale together rather than one operation at a time.
/// </summary>
public sealed class AffineTests
{
    private const double Tol = 1e-9;

    [Fact]
    public void Identity_leaves_a_point_where_it_was()
    {
        var (x, y) = Affine.Identity.Apply(12.5, -3.25);
        Assert.Equal(12.5, x, Tol);
        Assert.Equal(-3.25, y, Tol);
    }

    [Fact]
    public void Rotate_90_turns_the_x_axis_onto_the_y_axis()
    {
        // Image coordinates: y grows downward, so +90 degrees turns clockwise on screen.
        var (x, y) = Affine.RotateDeg(90).Apply(1, 0);
        Assert.Equal(0, x, Tol);
        Assert.Equal(1, y, Tol);
    }

    [Fact]
    public void Then_applies_this_first_and_the_argument_second()
    {
        // Translate then scale must scale the translation too; scale then translate must not.
        var translateThenScale = Affine.Translate(10, 0).Then(Affine.Scale(2));
        var scaleThenTranslate = Affine.Scale(2).Then(Affine.Translate(10, 0));

        Assert.Equal((20.0, 0.0), Round(translateThenScale.Apply(0, 0)));
        Assert.Equal((10.0, 0.0), Round(scaleThenTranslate.Apply(0, 0)));
    }

    [Fact]
    public void Compose_matches_applying_the_steps_one_by_one()
    {
        var a = Affine.Translate(7, -3);
        var b = Affine.RotateDeg(35);
        var c = Affine.Scale(2.5);
        var composed = a.Then(b).Then(c);

        var (px, py) = (13.75, -42.5);
        var stepwise = c.Apply(b.Apply(a.Apply(px, py).X, a.Apply(px, py).Y).X,
                               b.Apply(a.Apply(px, py).X, a.Apply(px, py).Y).Y);
        var atOnce = composed.Apply(px, py);

        Assert.Equal(stepwise.X, atOnce.X, Tol);
        Assert.Equal(stepwise.Y, atOnce.Y, Tol);
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1023.0, 767.0)]
    [InlineData(-58.25, 913.125)]
    public void A_point_survives_a_round_trip_through_a_composed_transform(double x, double y)
    {
        // The shape of a real deskewed crop: move the box centre to the origin, turn it upright,
        // scale to the working render size, then push it into the view's own top-left corner.
        var t = Affine.Translate(-470, -325)
            .Then(Affine.RotateDeg(-37.5))
            .Then(Affine.Scale(0.6375))
            .Then(Affine.Translate(512, 384));

        var forward = t.Apply(x, y);
        var back = t.Invert().Apply(forward.X, forward.Y);

        Assert.Equal(x, back.X, Tol);
        Assert.Equal(y, back.Y, Tol);
    }

    [Fact]
    public void Invert_composed_with_the_original_is_the_identity()
    {
        var t = Affine.Translate(3, 9).Then(Affine.RotateDeg(115)).Then(Affine.Scale(0.25));
        var round = t.Then(t.Invert());

        Assert.Equal(1, round.A, Tol);
        Assert.Equal(0, round.B, Tol);
        Assert.Equal(0, round.C, Tol);
        Assert.Equal(1, round.D, Tol);
        Assert.Equal(0, round.E, Tol);
        Assert.Equal(0, round.F, Tol);
    }

    [Fact]
    public void Scale_factor_and_rotation_are_read_back_off_a_composed_transform()
    {
        var t = Affine.RotateDeg(30).Then(Affine.Scale(4)).Then(Affine.Translate(100, 100));

        Assert.Equal(4, t.ScaleFactor, Tol);
        Assert.Equal(30, t.RotationDeg, 1e-9);
    }

    [Fact]
    public void A_degenerate_transform_refuses_to_invert()
        => Assert.Throws<InvalidOperationException>(() => Affine.Scale(0).Invert());

    private static (double, double) Round((double X, double Y) p)
        => (Math.Round(p.X, 9), Math.Round(p.Y, 9));
}
