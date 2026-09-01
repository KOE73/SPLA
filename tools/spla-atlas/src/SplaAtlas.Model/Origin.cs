namespace SplaAtlas.Model;

/// <summary>Who keeps a record: the utility, or a person.</summary>
public enum Origin
{
    /// <summary>Derived from C# and reconciled against it. The utility owns these.</summary>
    Code,

    /// <summary>Written by a person. The utility does not touch them, <c>status</c> included.</summary>
    Authored,
}

/// <summary>
/// The on-disk spelling of <see cref="Origin"/>.
/// </summary>
/// <remarks>
/// <para>
/// v3 renamed <c>manual</c> to <c>authored</c>, and the rename is not finished on disk: 21 relations
/// across the live projects still say <c>manual</c>. Reading maps the old token onto
/// <see cref="Origin.Authored"/>, so nothing downstream has to know about it.
/// </para>
/// <para>
/// Rewriting it is a separate matter. The codec does not touch a value it was only asked to read —
/// otherwise merely opening a project would produce a diff, and the migration would arrive
/// scattered through whatever unrelated run happened to touch that file first. Renaming the token
/// is a deliberate act; see <c>MigrateLegacyOrigins</c> on the catalogs.
/// </para>
/// </remarks>
public static class OriginToken
{
    public const string Code = "code";
    public const string Authored = "authored";

    /// <summary>The v2 spelling of <see cref="Origin.Authored"/>.</summary>
    public const string LegacyAuthored = "manual";

    /// <summary>
    /// Reads a token. Returns null for anything unrecognised — an origin the contract does not
    /// define is drift to report, not a value to guess at.
    /// </summary>
    public static Origin? Parse(string? token) => token switch
    {
        Code => Origin.Code,
        Authored or LegacyAuthored => Origin.Authored,
        _ => null,
    };

    public static string ToToken(Origin origin) => origin switch
    {
        Origin.Code => Code,
        Origin.Authored => Authored,
        _ => throw new ArgumentOutOfRangeException(nameof(origin)),
    };

    /// <summary>Whether the token is the retired v2 spelling.</summary>
    public static bool IsLegacy(string? token) => token == LegacyAuthored;
}
