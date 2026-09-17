namespace SPLA.Plugins.Geometry.Model;

/// <summary>What a marked object is. v1 has two kinds; Pose and Polygon are out of scope (ADR §3.2).</summary>
internal enum ObjectKind
{
    Box,
    Point
}

/// <summary>Where an object is in the place → look → correct loop. Status is what the renderer
/// colours by: an accepted object is drawn muted, an editing one saturated.</summary>
internal enum ObjectStatus
{
    Editing,
    Accepted
}

/// <summary>
/// One marked object. The plugin knows nothing about what it depicts — the name is an arbitrary
/// string the caller chose (ADR §3.1), and it is the object's key within a session.
/// </summary>
internal sealed class GeometryObject
{
    public string Name { get; init; } = "";

    public ObjectKind Kind { get; init; }

    /// <summary>Set when <see cref="Kind"/> is <see cref="ObjectKind.Box"/>. In SOURCE coordinates.</summary>
    public Obb? Box { get; set; }

    /// <summary>Set when <see cref="Kind"/> is <see cref="ObjectKind.Point"/>. In SOURCE coordinates.</summary>
    public (double X, double Y) Point { get; set; }

    public ObjectStatus Status { get; set; } = ObjectStatus.Editing;
}
