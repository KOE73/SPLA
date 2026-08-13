using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SPLA.Domain.Security;

/// <summary>
/// Who an island is, for the purposes of a grant. An island is a configured, credentialed system —
/// a database, a host, a foreign tool server — reachable only through the connection its operator
/// set up, and unlike the project it has no containing area to be identified by.
///
/// <para><b>Identity is the substance, not the label.</b> A grant is a promise about a thing, so it
/// must not follow the name: renaming <c>test</c> to <c>prod</c>, or swapping the two names, would
/// otherwise hand one server the permissions given for another. Equally, it must not survive the
/// thing changing underneath it — a connection repointed at a different server is a different
/// island whatever it is still called.</para>
///
/// <para>Both properties fall out of identifying a connection by a fingerprint of the fields that
/// decide WHAT it reaches and AS WHOM. The display name rides along for the grant file and the map
/// to be readable, and is never compared.</para>
/// </summary>
/// <param name="Kind">Family of system: <c>sql</c>, <c>ssh</c>, <c>mcp</c>.</param>
/// <param name="Fingerprint">Digest of the substance fields — see <see cref="Substance"/>.</param>
/// <param name="DisplayName">What the operator calls it. Cosmetic.</param>
public sealed record IslandIdentity(string Kind, string Fingerprint, string DisplayName)
{
    /// <summary>How a grant refers to this island. Stable across renames, different after a
    /// repointing.</summary>
    public string Key => $"{Kind}:{Fingerprint}";

    /// <summary>For a human: the name they know, with enough of the fingerprint to tell two
    /// same-named entries apart.</summary>
    public override string ToString() => $"{Kind}:{DisplayName} ({Fingerprint[..Math.Min(8, Fingerprint.Length)]})";
}

/// <summary>
/// Digest over the fields that make a connection the connection it is.
/// </summary>
public static class Substance
{
    /// <summary>
    /// Fingerprints an ordered set of named fields. Order matters and is the caller's — it is part of
    /// the canonical form, so two callers listing the same fields differently would disagree, which
    /// is why each config owns exactly one place that builds its list.
    ///
    /// <para><b>Values are compared as written, without case folding.</b> Hostnames are officially
    /// case-insensitive and it is tempting to normalise, but the two ways of being wrong are not
    /// symmetric: conflating two entries silently lends one the other's permissions, while treating a
    /// re-cased host as new costs a single extra confirmation. Fail towards asking.</para>
    ///
    /// <para><b>Never pass a secret.</b> Pass the reference (<c>secret:KEY</c>) or a scheme marker.
    /// A fingerprint is stored in the grant file in the clear; it must not be something an attacker
    /// would enjoy having a digest of.</para>
    /// </summary>
    public static string Of(params (string Field, string? Value)[] fields)
    {
        var canonical = string.Join(
            "\n",
            fields.Select(f => $"{f.Field}={(f.Value ?? string.Empty).Trim()}"));

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));

        // 16 hex characters — 64 bits. This is not a security boundary (an attacker who can edit the
        // config can also edit the grant file); it only has to not collide by accident.
        return Convert.ToHexString(digest)[..16].ToLowerInvariant();
    }

    /// <summary>
    /// What a credential-shaped setting contributes: which named entry is used, or — for a legacy
    /// inline pointer — only its scheme. Changing <em>as whom</em> a connection logs in is a change of
    /// island; the value that authenticates it is nobody's business here.
    /// </summary>
    public static string CredentialShape(string? credentialKey, string? inlinePointer)
    {
        if (!string.IsNullOrWhiteSpace(credentialKey)) return $"entry:{credentialKey.Trim()}";
        if (string.IsNullOrWhiteSpace(inlinePointer)) return "none";

        var pointer = inlinePointer.Trim();
        var scheme = pointer.IndexOf(':') is var i and > 0 ? pointer[..i] : "literal";
        return scheme is "secret" or "env" ? $"{scheme}:{pointer[(scheme.Length + 1)..]}" : "literal";
    }
}
