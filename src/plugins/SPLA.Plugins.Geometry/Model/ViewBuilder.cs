using SPLA.Plugins.Geometry.Session;
using System;
using System.Linq;

namespace SPLA.Plugins.Geometry.Model;

/// <summary>
/// Builds the crops the model looks through. A crop is how something small becomes big enough to
/// place accurately: mark the large object, view into it, mark what is inside it. Unlike the root
/// view, a crop <b>does</b> enlarge — that is the entire point of asking for one.
/// <para>
/// Every crop is a transform, never a pair of offsets: a box that sits at an angle can be cut out and
/// turned upright (<c>deskew</c>), and that is not expressible as an offset (ADR §3.3).
/// </para>
/// </summary>
internal static class ViewBuilder
{
    /// <summary>Smallest half-side of a crop around a point, in source pixels, so that a tiny padding
    /// setting cannot produce a zero-area view.</summary>
    private const double MinimumPointHalfSide = 8;

    /// <summary>A crop around one object. A box is cut to itself plus a margin; a point has no extent
    /// of its own, so it gets a square of <c>2 × padding × min(source side)</c> around it.</summary>
    public static GeometryView Crop(
        GeometrySession session, GeometryObject obj, string parentId, double padding, bool deskew,
        GeometrySettings cfg)
    {
        if (obj.Kind == ObjectKind.Point || obj.Box is not { } box)
        {
            var half = Math.Max(MinimumPointHalfSide, padding * Math.Min(session.Source.Width, session.Source.Height));
            return Axis(session, parentId, obj.Name,
                obj.Point.X - half, obj.Point.Y - half, half * 2, half * 2, cfg);
        }

        if (!deskew)
        {
            var corners = box.Corners();
            double minX = corners.Min(c => c.X), maxX = corners.Max(c => c.X);
            double minY = corners.Min(c => c.Y), maxY = corners.Max(c => c.Y);
            double padX = padding * (maxX - minX), padY = padding * (maxY - minY);
            return Axis(session, parentId, obj.Name,
                minX - padX, minY - padY, maxX - minX + padX * 2, maxY - minY + padY * 2, cfg);
        }

        // The one place the matrix really rotates: the crop is turned by -angle about the box's
        // centre, so the box reads horizontally in the picture the model gets.
        double width = box.Width * (1 + padding * 2), height = box.Height * (1 + padding * 2);
        var scale = Zoom(width, height, cfg);
        var (viewWidth, viewHeight) = Size(width, height, scale);

        var transform = Affine.Translate(-box.Cx, -box.Cy)
            .Then(Affine.RotateDeg(-box.AngleDeg))
            .Then(Affine.Scale(scale))
            .Then(Affine.Translate(viewWidth / 2.0, viewHeight / 2.0));

        return new GeometryView
        {
            Id = session.NextViewId(),
            ParentId = parentId,
            FromBox = obj.Name,
            Deskewed = true,
            SourceToView = transform,
            Width = viewWidth,
            Height = viewHeight,
        };
    }

    /// <summary>A crop to a rectangle the model named <b>in the pixels of <paramref name="from"/></b> —
    /// how it looks into a corner of the frame before anything has been marked there. The rectangle
    /// keeps that view's orientation, so cropping inside a deskewed view stays deskewed.</summary>
    public static GeometryView Crop(
        GeometrySession session, GeometryView from, double x, double y, double width, double height,
        GeometrySettings cfg)
    {
        var scale = Zoom(width, height, cfg);
        var (viewWidth, viewHeight) = Size(width, height, scale);

        var transform = from.SourceToView
            .Then(Affine.Translate(-x, -y))
            .Then(Affine.Scale(scale));

        return new GeometryView
        {
            Id = session.NextViewId(),
            ParentId = from.Id,
            FromBox = null,
            Deskewed = from.Deskewed,
            SourceToView = transform,
            Width = viewWidth,
            Height = viewHeight,
        };
    }

    /// <summary>An axis-aligned crop given in SOURCE pixels.</summary>
    private static GeometryView Axis(
        GeometrySession session, string parentId, string? fromBox,
        double x, double y, double width, double height, GeometrySettings cfg)
    {
        var scale = Zoom(width, height, cfg);
        var (viewWidth, viewHeight) = Size(width, height, scale);

        return new GeometryView
        {
            Id = session.NextViewId(),
            ParentId = parentId,
            FromBox = fromBox,
            SourceToView = Affine.Translate(-x, -y).Then(Affine.Scale(scale)),
            Width = viewWidth,
            Height = viewHeight,
        };
    }

    /// <summary>The crop fills the working size: its longest side becomes <c>render_max_side</c>,
    /// magnifying a small region rather than delivering a postage stamp.</summary>
    private static double Zoom(double width, double height, GeometrySettings cfg)
    {
        var longest = Math.Max(Math.Abs(width), Math.Abs(height));
        return longest <= 0 ? 1 : cfg.RenderMaxSide / longest;
    }

    private static (int Width, int Height) Size(double width, double height, double scale) =>
        (Math.Max(1, (int)Math.Round(Math.Abs(width) * scale)),
         Math.Max(1, (int)Math.Round(Math.Abs(height) * scale)));
}
