namespace SPLA.Plugins.Geometry.Model;

/// <summary>
/// One way of looking at the source image: the root view, or a crop pushed onto it.
/// <para>
/// <b>The root view is not the identity.</b> Even <c>source</c> is scaled down to
/// <c>render_max_side</c>, because that is the size the model is shown and therefore the coordinate
/// space it answers in. Treating the root as identity is the single easiest way to introduce a
/// silent off-by-a-scale-factor bug here, so the transform is stored for every view without
/// exception.
/// </para>
/// </summary>
internal sealed class GeometryView
{
    /// <summary><c>source</c> for the root, <c>crop_1</c>, <c>crop_2</c>, … for pushed crops.</summary>
    public string Id { get; init; } = "";

    /// <summary>Null for the root view.</summary>
    public string? ParentId { get; init; }

    /// <summary>The object this view was cropped to, when it was cropped to one.</summary>
    public string? FromBox { get; init; }

    /// <summary>Source image pixels → this view's own pixels.</summary>
    public Affine SourceToView { get; init; } = Affine.Identity;

    /// <summary>Size of this view in its own pixels — what the reply announces as the coordinate space.</summary>
    public int Width { get; init; }

    public int Height { get; init; }

    /// <summary>True when the crop was turned so the box it was cut from reads horizontally. Announced
    /// in the reply, because it is the one case where the picture is not an axis-aligned piece of the
    /// frame and the model would otherwise have no way to know.</summary>
    public bool Deskewed { get; init; }

    /// <summary>Whether renders of this view carry the debug grid. It lives on the view rather than on
    /// the call so that a model which asked to see the grid keeps seeing it while it works here,
    /// instead of repeating the flag on every correction.</summary>
    public bool Grid { get; set; }

    /// <summary>This view's pixels → source image pixels. Where a coordinate the model passed becomes
    /// canonical.</summary>
    public Affine ViewToSource => SourceToView.Invert();
}
