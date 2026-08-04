using System.Collections.Generic;
using SPLA.Library.Catalog;

namespace SPLA.Library.Librarians;

/// <summary>What was asked for at the desk. Both halves are optional and they compose: tags narrow,
/// text ranks.</summary>
/// <param name="Tags">Subject words. Normalised by the librarian, so a caller may pass them as
/// written.</param>
/// <param name="Text">Free text matched against id and description — the escape hatch for a subject
/// nobody thought to tag.</param>
public sealed record SkillQuery(IReadOnlyList<string>? Tags = null, string? Text = null)
{
    public bool IsEmpty => (Tags is null || Tags.Count == 0) && string.IsNullOrWhiteSpace(Text);
}

/// <summary>One answer: the card, and why it surfaced.</summary>
/// <param name="Card">The catalog card — never the body. A librarian hands over annotations; reading
/// is a separate act.</param>
/// <param name="MatchedTags">Which of the asked-for tags this card carries.</param>
/// <param name="Score">Higher is better. Comparable only within one result set.</param>
public sealed record SkillMatch(SkillCard Card, IReadOnlyList<string> MatchedTags, int Score);

/// <summary>
/// Answers "what have you got on this subject" without putting the catalog in anyone's prompt.
///
/// <para>The cheapest of the three librarians in the ADR and the only deterministic one: set
/// intersection, not judgement. It cannot understand a synonym nobody tagged — that is what the
/// sub-agent librarian is for — but it costs nothing and never invents a skill that does not
/// exist.</para>
/// </summary>
public interface ITagLibrarian
{
    /// <summary>Cards matching the query, best first. Never returns a skill whose source is
    /// <see cref="Sources.SourceLevel.OutOfCatalog"/>: that level means the model is not told, and a
    /// search that reveals it would make the level a lie.</summary>
    IReadOnlyList<SkillMatch> Find(SkillQuery query, int limit = 5);

    /// <summary>The vocabulary a caller may ask with — everything the librarian could match on.</summary>
    TagVocabulary Vocabulary { get; }
}
