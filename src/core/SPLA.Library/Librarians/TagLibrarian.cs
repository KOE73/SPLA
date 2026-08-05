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
        if (terms.Count == 0) return 0;

        var idWords = Words(card.Id);
        var descriptionWords = Words(card.Description);

        var score = 0;
        foreach (var term in terms)
        {
            if (Matches(idWords, term)) score += IdWeight;
            else if (Matches(descriptionWords, term)) score += DescriptionWeight;
        }

        return score;
    }

    /// <summary>
    /// A term matches a whole word, or shares a long enough prefix with one — in either direction,
    /// so both "rebuild"/"Rebuilds" and "relays"/"relay" work. Which of the query and the text
    /// carries the plural is not something a searcher should have to guess.
    ///
    /// <para><b>Never a substring.</b> That was the original spelling and it made the text pass fire
    /// on almost anything: <c>our</c> is inside <c>behaviour</c>, so "our outgoing email" matched an
    /// SMTP skill for no reason at all. A pass that always succeeds is worse than one that never
    /// does — it reports a false hit AND stops the model-backed librarian from ever being reached.</para>
    /// </summary>
    private static bool Matches(IReadOnlyList<string> words, string term) =>
        words.Any(w => w.Equals(term, StringComparison.OrdinalIgnoreCase) || SharePrefix(w, term));

    private static bool SharePrefix(string word, string term) =>
        word.Length >= MinPrefix && term.Length >= MinPrefix &&
        (word.StartsWith(term, StringComparison.OrdinalIgnoreCase) ||
         term.StartsWith(word, StringComparison.OrdinalIgnoreCase));

    /// <summary>Below this a shared prefix is a coincidence, not a stem.</summary>
    private const int MinPrefix = 4;

    private static IReadOnlyList<string> Words(string text) =>
        text.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

    private static readonly char[] Separators =
        [' ', ',', ';', ':', '\t', '\n', '\r', '.', '"', '\'', '(', ')', '/', '\\', '-', '_', '—'];

    /// <summary>
    /// Splits free text into words worth matching, dropping the ones that carry no subject.
    ///
    /// <para>Both filters are needed and neither replaces the other. Short words match too much;
    /// stopwords are the ones that are long enough to survive a length rule and still mean nothing —
    /// "recipe for borscht" matched a DNS skill through <c>for</c>, which appears in half the
    /// descriptions ever written.</para>
    /// </summary>
    private static IReadOnlyList<string> Terms(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split(Separators, StringSplitOptions.RemoveEmptyEntries)
                  .Where(w => w.Length > 2 && !Stopwords.Contains(w))
                  .Distinct(StringComparer.OrdinalIgnoreCase)
                  .ToList();

    /// <summary>Deliberately small: only words that are common AND carry no subject. Anything
    /// domain-ish stays — "host" and "server" are exactly what someone searching means.</summary>
    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "with", "from", "into", "out", "our", "your", "their", "its",
        "this", "that", "these", "those", "any", "all", "some", "not", "but", "you", "was", "were",
        "are", "can", "will", "would", "should", "could", "how", "why", "what", "when", "where",
        "need", "want", "make", "get", "got", "has", "have", "had", "does", "did", "please"
    };
}
