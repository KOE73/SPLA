namespace SPLA.Library.Sources;

/// <summary>
/// How much of a source's holdings reach the model before anyone asks. Deliberately the same shape as
/// <c>ToolSetLevel</c>: the question is not "may these skills be used" but "does the model know they
/// exist, and at what price in context".
///
/// <para><b>Why the level sits on the source and not on the skill.</b> A hundred skills would mean a
/// hundred decisions. One external repository is one decision, which is the decision a person
/// actually has an opinion about.</para>
///
/// <para><b>The level is not a permission.</b> Trust decides whether a skill may be used at all and
/// answers to the user; the level decides only who gets told it exists. A skill that is
/// <see cref="OutOfCatalog"/> is still listed in the settings panel, still activatable by a person,
/// and still refused to an untrusted source — the two axes never substitute for each other.</para>
/// </summary>
public enum SourceLevel
{
    /// <summary>Not in the catalog. The model is told nothing — not the id, not the tags, not even a
    /// count. Reachable only by a person handing the skill to a chat.
    /// <para>This is the level that makes an unvetted external repository safe to have configured:
    /// visible to its owner in the panel, invisible to the model until someone decides otherwise.</para></summary>
    OutOfCatalog,

    /// <summary>Findable but unlisted: <c>skill_find</c> may return it, and nothing about it appears in
    /// the prompt — not even its tags. Costs zero context; costs one tool call to discover.
    /// <para>Until <c>skill_find</c> exists this behaves exactly like <see cref="OutOfCatalog"/> for
    /// the model. That is the intended half-built state, not an oversight.</para></summary>
    Findable,

    /// <summary>In the catalog: the skill's tags join the prompt's tag cloud, its description does
    /// not. The model learns that a subject exists and asks for the specifics — two steps, and the
    /// price stops growing with the size of the holdings.</summary>
    InCatalog,

    /// <summary>Open shelf: id and description in the prompt, every request. One step for the model
    /// and the most expensive level there is — the only sensible setting for a handful of skills, and
    /// the wrong one for a hundred.</summary>
    OnShelf
}
