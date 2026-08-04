using System;
using System.Collections.Generic;
using System.Linq;
using SPLA.Library.Catalog;
using SPLA.Library.Sources;

namespace SPLA.Library.Librarians;

/// <summary>
/// Tag intersection plus a text pass over id and description.
///
/// <para><b>Why two mechanisms rather than one.</b> Tags are exact and cheap but only reach what an
/// author thought to write down; text reaches the rest but matches noise. Ranking tags above text
/// keeps the deterministic answer on top while leaving the fuzzy one available underneath, which is
/// the honest ordering: a tag match is a fact, a text match is a guess.</para>
///
/// <para>Reads the library's holdings live on every call rather than caching. The catalog is rebuilt
/// on every source change, and a librarian answering from a stale index is the one failure mode a
/// person will never think to suspect.</para>
/// </summary>
public sealed class TagLibrarian : ITagLibrarian
{
    private const int TagWeight = 10;
    private const int DescriptionWeight = 2;
    private const int IdWeight = 3;

    private readonly SkillLibrary _library;

    public TagLibrarian(SkillLibrary library) => _library = library;

    /// <summary>Everything the librarian could match on — the catalog minus what is not in it.</summary>
    public TagVocabulary Vocabulary => TagVocabulary.From(Searchable());

    /// <summary>
    /// Skills the librarian will consider. <see cref="SourceLevel.OutOfCatalog"/> is excluded, and
    /// that exclusion is the level's whole meaning: a source the model is not told about must not
    /// become discoverable by asking. Everything else is fair game — including <c>Findable</c>, which
    /// exists precisely to be reachable here and nowhere else.
    /// </summary>
    private IEnumerable<SkillCard> Searchable() =>
        _library.Catalog().Where(c => c.Level != SourceLevel.OutOfCatalog);

    public IReadOnlyList<SkillMatch> Find(SkillQuery query, int limit = 5)
    {
        if (query.IsEmpty || limit <= 0) return [];

        var wanted = SkillTag.NormalizeQuery(query.Tags);
        var terms = Terms(query.Text);

        var matches = new List<SkillMatch>();
        foreach (var card in Searchable())
        {
            var matchedTags = wanted.Count == 0
                ? []
                : wanted.Where(t => card.Tags.Contains(t, StringComparer.Ordinal)).ToList();

            var score = matchedTags.Count * TagWeight + TextScore(card, terms);
            if (score > 0) matches.Add(new SkillMatch(card, matchedTags, score));
        }

        return matches
            .OrderByDescending(m => m.Score)
            .ThenBy(m => m.Card.Id, StringComparer.OrdinalIgnoreCase)   // stable across rebuilds
            .Take(limit)
            .ToList();
    }

    private static int TextScore(SkillCard card, IReadOnlyList<string> terms)
    {
        var score = 0;
        foreach (var term in terms)
        {
            if (card.Id.Contains(term, StringComparison.OrdinalIgnoreCase)) score += IdWeight;
            else if (card.Description.Contains(term, StringComparison.OrdinalIgnoreCase)) score += DescriptionWeight;
        }

        return score;
    }

    /// <summary>Splits free text into words worth matching. One- and two-letter words are dropped:
    /// "on", "to", "my" match everything and would flatten the ranking into noise.</summary>
    private static IReadOnlyList<string> Terms(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split([' ', ',', ';', '\t', '\n', '\r', '.', '"', '\''], StringSplitOptions.RemoveEmptyEntries)
                  .Where(w => w.Length > 2)
                  .Distinct(StringComparer.OrdinalIgnoreCase)
                  .ToList();
}
