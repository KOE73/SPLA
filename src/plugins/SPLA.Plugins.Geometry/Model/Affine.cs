using System;

namespace SPLA.Plugins.Geometry.Model;

/// <summary>
/// A 2x3 affine transform, in the same component order Skia and PostScript use:
/// <c>x' = A*x + C*y + E</c>, <c>y' = B*x + D*y + F</c>.
/// <para>
/// Why a matrix and not a pair of offsets: a view can be cropped to a box that sits at an angle and
/// then deskewed so that box reads horizontally (plan step 2.3). A rotation is not expressible as
/// <c>offset_x/offset_y</c>, and discovering that after the session format is fixed costs a rewrite —
/// see ADR §3.3.
/// </para>
/// <para>
/// Angles are measured in image coordinates, where y grows downward, so a positive angle turns
/// clockwise on screen.
/// </para>
/// </summary>
internal readonly record struct Affine(double A, double B, double C, double D, double E, double F)
{
    public static Affine Identity { get; } = new(1, 0, 0, 1, 0, 0);

    public static Affine Translate(double dx, double dy) => new(1, 0, 0, 1, dx, dy);

    public static Affine Scale(double s) => new(s, 0, 0, s, 0, 0);

    public static Affine RotateDeg(double deg)
    {
        var rad = deg * Math.PI / 180.0;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);
        return new Affine(cos, sin, -sin, cos, 0, 0);
    }

    /// <summary>Applies <paramref name="next"/> after this one: <c>next(this(p))</c>.</summary>
    public Affine Then(in Affine next) => new(
        A: next.A * A + next.C * B,
        B: next.B * A + next.D * B,
        C: next.A * C + next.C * D,
        D: next.B * C + next.D * D,
        E: next.A * E + next.C * F + next.E,
        F: next.B * E + next.D * F + next.F);

    /// <summary>The inverse transform. Throws for a degenerate matrix — a zero-area view is a bug
    /// upstream, and silently returning identity would hide it inside coordinates that still look
    /// plausible.</summary>
    public Affine Invert()
    {
        var det = A * D - B * C;
        if (det == 0 || double.IsNaN(det) || double.IsInfinity(det))
            throw new InvalidOperationException("Affine is not invertible (zero determinant).");

        double ia = D / det, ib = -B / det, ic = -C / det, id = A / det;
        return new Affine(ia, ib, ic, id,
            -(ia * E + ic * F),
            -(ib * E + id * F));
    }

    public (double X, double Y) Apply(double x, double y) => (A * x + C * y + E, B * x + D * y + F);

    /// <summary>The mean linear scale — the square root of the absolute determinant. Used to carry a
    /// box's width/height across a transform, where the two axes scale together by construction
    /// (every transform this plugin builds is a similarity: translate, uniform scale, rotate).</summary>
    public double ScaleFactor => Math.Sqrt(Math.Abs(A * D - B * C));

    /// <summary>How far this transform turns, in degrees, for carrying a box's angle across it.</summary>
    public double RotationDeg => Math.Atan2(B, A) * 180.0 / Math.PI;
}
