using System;

namespace SPLA.Plugins.Geometry.Model;

/// <summary>
/// An oriented bounding box: a centre, a size and a rotation. Stored canonically in SOURCE image
/// coordinates; a view's own numbers are produced by transforming it, never by storing a second copy.
/// </summary>
/// <param name="AngleDeg">Clockwise on screen, since image y grows downward.</param>
internal sealed record Obb(double Cx, double Cy, double Width, double Height, double AngleDeg)
{
    /// <summary>The four corners, in the order top-left, top-right, bottom-right, bottom-left of the
    /// UNROTATED box, each turned by <see cref="AngleDeg"/> about the centre. Fixing the order here
    /// is what lets <c>geom_result</c> publish corners that disambiguate the angle convention.</summary>
    public (double X, double Y)[] Corners()
    {
        var rad = AngleDeg * Math.PI / 180.0;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);
        double hw = Width / 2, hh = Height / 2;

        (double X, double Y)[] local = [(-hw, -hh), (hw, -hh), (hw, hh), (-hw, hh)];
        var result = new (double X, double Y)[4];
        for (var i = 0; i < 4; i++)
        {
            var (x, y) = local[i];
            result[i] = (Cx + x * cos - y * sin, Cy + x * sin + y * cos);
        }
        return result;
    }

    /// <summary>The same box seen through <paramref name="t"/>. Size travels by
    /// <see cref="Affine.ScaleFactor"/> and the angle by <see cref="Affine.RotationDeg"/>, which is
    /// exact for the similarity transforms this plugin builds.</summary>
    public Obb Transformed(in Affine t)
    {
        var (cx, cy) = t.Apply(Cx, Cy);
        var scale = t.ScaleFactor;
        return new Obb(cx, cy, Width * scale, Height * scale, AngleDeg + t.RotationDeg);
    }
}
