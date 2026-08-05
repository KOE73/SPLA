using System.Collections.Generic;

namespace SPLA.Library.Catalog;

/// <summary>
/// The answer to "which book is this" — and there are three of them, not two.
///
/// <para>The third one is the point. A librarian who decides for you which of two editions you meant
/// will be wrong once, quietly, and nobody will think to check. So a name that matches two books is
/// an <b>answer</b> with candidates attached, not a silent pick and not a plain miss: the caller is
/// obliged to say which ones it found, because an ambiguity error that does not name the alternatives
/// converts a deliberate cost into pure irritation.</para>
/// </summary>
public sealed class SkillLookup
{
    private SkillLookup(SkillCard? card, IReadOnlyList<SkillCard> candidates)
    {
        Card = card;
        Candidates = candidates;
    }

    public static SkillLookup Hit(SkillCard card) => new(card, [card]);
    public static SkillLookup Miss() => new(null, []);
    public static SkillLookup Ambiguous(IReadOnlyList<SkillCard> candidates) => new(null, candidates);

    /// <summary>The one book meant, or null when the name matched none or several.</summary>
    public SkillCard? Card { get; }

    /// <summary>Everything the name matched. Empty on a miss, one entry on a hit, several when
    /// ambiguous — which is what the error message has to print.</summary>
    public IReadOnlyList<SkillCard> Candidates { get; }

    public bool IsAmbiguous => Card is null && Candidates.Count > 0;
}
