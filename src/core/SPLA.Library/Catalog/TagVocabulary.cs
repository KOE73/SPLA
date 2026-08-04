using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Library.Catalog;

/// <summary>One tag and how many skills carry it.</summary>
/// <param name="Tag">Normalised tag.</param>
/// <param name="Count">Skills carrying it, within whatever set the vocabulary was built from.</param>
public sealed record TagCount(string Tag, int Count);

/// <summary>
/// Every subject word in a set of cards, with counts.
///
/// <para>This is what replaces the catalog in the prompt: fifty words instead of a hundred
/// descriptions, and the cost stops growing with the holdings — a new skill tagged <c>ssh</c> adds a
/// number, not a line. It is also the vocabulary a person needs in order to notice the drift the
/// normaliser cannot catch: <c>ssh</c> and <c>ssh-access</c> both normalise cleanly and are still two
/// words for one subject. Showing the whole vocabulary is the only thing that makes that visible.</para>
///
/// <para>Built from a set of cards rather than owned by the library, so the same type serves the
/// prompt (available skills only), the settings panel (everything), and a librarian's own view.</para>
/// </summary>
public sealed class TagVocabulary
{
    private readonly Dictionary<string, int> _counts;

    public static readonly TagVocabulary Empty = new([]);

    private TagVocabulary(Dictionary<string, int> counts) => _counts = counts;

    /// <summary>Counts every tag across <paramref name="cards"/>. Untagged cards contribute nothing
    /// and are not an error.</summary>
    public static TagVocabulary From(IEnumerable<SkillCard> cards)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var card in cards)
            foreach (var tag in card.Tags)
                counts[tag] = counts.GetValueOrDefault(tag) + 1;

        return new TagVocabulary(counts);
    }

    public int Count => _counts.Count;

    public bool IsEmpty => _counts.Count == 0;

    /// <summary>Every tag with its count, commonest first and alphabetical within a count. Frequency
    /// first because that is the order a reader scans: the big subjects are the ones worth asking
    /// about, and alphabetical within a tie keeps the list stable between rebuilds.</summary>
    public IReadOnlyList<TagCount> All() =>
        _counts.Select(kv => new TagCount(kv.Key, kv.Value))
               .OrderByDescending(t => t.Count)
               .ThenBy(t => t.Tag, StringComparer.Ordinal)
               .ToList();

    /// <summary>Just the words, in the same order. What the prompt prints.</summary>
    public IReadOnlyList<string> Tags() => All().Select(t => t.Tag).ToList();

    public int CountOf(string tag) =>
        SkillTag.Normalize(tag) is { } normalized ? _counts.GetValueOrDefault(normalized) : 0;

    /// <summary>Whether a term is a word this fond actually uses — the check that lets a librarian
    /// tell "no skills match" apart from "that is not a word here", which are different answers.</summary>
    public bool Knows(string tag) => CountOf(tag) > 0;
}
