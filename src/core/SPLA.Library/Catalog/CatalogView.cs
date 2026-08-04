using System.Collections.Generic;
using System.Linq;
using SPLA.Library.Sources;

namespace SPLA.Library.Catalog;

/// <summary>
/// What the model is told about the holdings, and what it has to ask for.
///
/// <para>Two outputs, and the split is the whole point. <see cref="Shelf"/> is the expensive one — id
/// and description per skill, in every request. <see cref="Cloud"/> is the cheap one — subject words
/// and nothing else, so a fond of five hundred costs the same as a fond of fifty as long as they
/// share subjects. Everything else the model is not told about at all.</para>
/// </summary>
public sealed class CatalogView
{
    /// <summary>How many skills may be listed in full before the shelf collapses into the cloud.
    /// <para>Twenty-five is a judgement, not a measurement: it is roughly where a list stops being
    /// something a reader scans and starts being something a reader skims, and it is about 3k tokens
    /// at the description lengths real skills use.</para></summary>
    public const int DefaultShelfLimit = 25;

    private CatalogView(IReadOnlyList<SkillCard> shelf, IReadOnlyList<SkillCard> clouded, bool collapsed)
    {
        Shelf = shelf;
        CloudedSkills = clouded;
        Cloud = TagVocabulary.From(clouded);
        Collapsed = collapsed;
    }

    /// <summary>Skills listed in full — id and description reach the prompt.</summary>
    public IReadOnlyList<SkillCard> Shelf { get; }

    /// <summary>Skills represented only by their tags.</summary>
    public IReadOnlyList<SkillCard> CloudedSkills { get; }

    /// <summary>The subject words those skills contribute.</summary>
    public TagVocabulary Cloud { get; }

    /// <summary>True when the shelf limit demoted skills that would otherwise have been listed. Worth
    /// surfacing: it is the moment the model's view of the fond changed shape.</summary>
    public bool Collapsed { get; }

    public bool IsEmpty => Shelf.Count == 0 && Cloud.IsEmpty;

    /// <summary>
    /// Splits available skills into what is listed and what is only summarised.
    ///
    /// <para><b>Untagged skills are never demoted.</b> A skill with no tags cannot be found by
    /// subject, so moving it into the cloud would not summarise it — it would delete it. It keeps its
    /// place on the shelf and keeps costing what it costs. That is deliberately visible rather than
    /// silently absorbed: the way to make a large fond cheap is to tag it, and a project that has not
    /// should feel the price rather than lose the skills.</para>
    /// </summary>
    /// <param name="available">Skills in <c>Available</c> state — this type does not re-check that.</param>
    /// <param name="shelfLimit">Listing budget; at or below it nothing is demoted.</param>
    public static CatalogView Build(IEnumerable<SkillCard> available, int shelfLimit = DefaultShelfLimit)
    {
        var shelf = new List<SkillCard>();
        var clouded = new List<SkillCard>();

        foreach (var card in available)
            switch (card.Level)
            {
                case SourceLevel.OnShelf:   shelf.Add(card); break;
                case SourceLevel.InCatalog: clouded.Add(card); break;
                // OutOfCatalog and Findable contribute nothing to the prompt. Findable differs only
                // once skill_find exists to reach it.
            }

        if (shelf.Count <= shelfLimit) return new CatalogView(shelf, clouded, collapsed: false);

        var demotable = shelf.Where(c => c.Tags.Count > 0).ToList();
        if (demotable.Count == 0) return new CatalogView(shelf, clouded, collapsed: false);

        clouded.AddRange(demotable);
        return new CatalogView(
            shelf.Where(c => c.Tags.Count == 0).ToList(), clouded, collapsed: true);
    }
}
