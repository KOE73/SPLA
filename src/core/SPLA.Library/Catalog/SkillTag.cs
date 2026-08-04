using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPLA.Library.Catalog;

/// <summary>
/// The one place a tag becomes a tag.
///
/// <para>Tags buy a deterministic catalog — intersection of sets rather than a guess — and they pay
/// for it in vocabulary discipline. <c>ssh</c>, <c>SSH</c> and <c>SSH access</c> are one subject that
/// a year of casual authoring turns into three, and a vocabulary that has drifted is worse than no
/// vocabulary: the model picks from a list of words, so a split word is a subject it can no longer
/// find. Normalising on the way in is what keeps that from happening, and it has to happen in exactly
/// one function or it has not happened at all.</para>
/// </summary>
public static class SkillTag
{
    /// <summary>
    /// Lower-case, kebab-case, no punctuation. <c>"SSH Access"</c>, <c>"ssh_access"</c> and
    /// <c>"  SSH---access "</c> all become <c>ssh-access</c>.
    ///
    /// <para>Returns null for anything that normalises to nothing, so a stray <c>"!!!"</c> in a tag
    /// list is dropped rather than becoming an empty tag that matches everything.</para>
    /// </summary>
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var sb = new StringBuilder(raw.Length);
        foreach (var ch in raw.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            // Everything else collapses to a single separator — spaces, underscores, dots, slashes
            // and runs of dashes are all the same intent badly typed.
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }

        var result = sb.ToString().Trim('-');
        return result.Length == 0 ? null : result;
    }

    /// <summary>Normalises a list, drops what normalises to nothing, and deduplicates while keeping
    /// the author's order — the first spelling wins its position, later duplicates vanish.</summary>
    public static IReadOnlyList<string> NormalizeAll(IEnumerable<string?>? raw)
    {
        if (raw is null) return [];

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>();
        foreach (var tag in raw)
            if (Normalize(tag) is { } normalized && seen.Add(normalized))
                result.Add(normalized);

        return result;
    }

    /// <summary>Normalises a query term the same way the stored tags were, so a model that writes
    /// "SSH" matches a skill tagged "ssh". Query and storage must share this function; two
    /// normalisers that agree today are two normalisers that disagree later.</summary>
    public static IReadOnlyList<string> NormalizeQuery(IEnumerable<string?>? terms) => NormalizeAll(terms);
}
