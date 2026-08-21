using System;

namespace SPLA.Domain.Resources;

/// <summary>
/// One resource address: <c>scheme://authority/path</c>.
///
/// <para>The whole point of this type is that the address, not the tool name, says where something
/// lives. <c>file:///src/app.cs</c> and <c>sftp://ioBroker/etc/nginx.conf</c> differ in the string,
/// not in which method you call — which is what lets a new storage arrive without a new tool.</para>
///
/// <para><b>Deliberately not <see cref="System.Uri"/>.</b> The BCL type normalises percent-encoding,
/// collapses <c>..</c>, lowercases the host and has opinions about which schemes may carry an
/// authority. Every one of those is a silent rewrite of an address a provider is about to resolve
/// against a real store, and a silent rewrite is exactly how a path escapes a boundary. This parses
/// the three parts and hands them over unchanged; judging them is the provider's job, done where
/// being wrong would matter.</para>
/// </summary>
/// <param name="Scheme">Lowercased, without the colon: <c>file</c>, <c>sftp</c>, <c>memory</c>.</param>
/// <param name="Authority">Which instance — an SSH host, a memory tier, a bucket. Empty when the
/// scheme has only one (<c>file:///…</c> has an empty authority by construction).</param>
/// <param name="Path">Everything after the authority, leading slash stripped, separators normalised
/// to <c>/</c>. Never percent-decoded — see the class remarks.</param>
public readonly record struct ResourceUri(string Scheme, string Authority, string Path)
{
    /// <summary>
    /// Parses an address, or explains in <paramref name="error"/> why it is not one.
    ///
    /// <para>Returns false rather than throwing because a malformed URI is nearly always something a
    /// model typed, and a refusal it can read and correct beats an exception the pipeline has to
    /// translate.</para>
    /// </summary>
    public static bool TryParse(string? text, out ResourceUri uri, out string? error)
    {
        uri = default;
        error = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            error = "empty address";
            return false;
        }

        var raw = text.Trim();
        var colon = raw.IndexOf(':');
        if (colon <= 0)
        {
            error = $"'{raw}' is not a resource address — expected scheme://path, e.g. file:///src/app.cs";
            return false;
        }

        var scheme = raw[..colon].ToLowerInvariant();
        foreach (var c in scheme)
        {
            // RFC 3986 scheme charset. Checked because a scheme is a dictionary key: letting an
            // arbitrary prefix through would make "c:/tmp/x" register as scheme "c".
            if (!char.IsAsciiLetterOrDigit(c) && c != '+' && c != '-' && c != '.')
            {
                error = $"'{raw[..colon]}' is not a valid scheme name";
                return false;
            }
        }

        if (!char.IsAsciiLetter(scheme[0]))
        {
            error = $"'{scheme}' is not a valid scheme name — must start with a letter";
            return false;
        }

        var rest = raw[(colon + 1)..];
        string authority;

        if (rest.StartsWith("//", StringComparison.Ordinal))
        {
            rest = rest[2..];
            var slash = rest.IndexOf('/');
            if (slash < 0)
            {
                // "sftp://ioBroker" — an authority and no path. Legitimate: it names the root.
                authority = rest;
                rest = string.Empty;
            }
            else
            {
                authority = rest[..slash];
                rest = rest[(slash + 1)..];
            }
        }
        else
        {
            // The opaque form, "blob:handle123". No authority; everything after the colon is path.
            authority = string.Empty;
        }

        uri = new ResourceUri(scheme, authority, Normalise(rest));
        return true;
    }

    /// <summary>Backslashes to slashes and no leading slash, so a provider always sees one shape.
    /// Trailing slashes are kept — on a directory listing they are meaningful.</summary>
    private static string Normalise(string path)
        => path.Replace('\\', '/').TrimStart('/');

    /// <summary>
    /// The address as written back out. Re-parsing this yields the same three fields — but not
    /// necessarily the same text: the opaque form <c>blob:handle123</c> and the authority-less form
    /// <c>blob:///handle123</c> parse identically, so one of them is what comes back. Stable in
    /// meaning, not byte-for-byte, which is why nothing should compare addresses as strings.
    /// </summary>
    public override string ToString()
        => Authority.Length == 0 && Path.Length == 0 ? $"{Scheme}:"
         : Authority.Length == 0 ? $"{Scheme}:///{Path}"
         : $"{Scheme}://{Authority}/{Path}";
}
