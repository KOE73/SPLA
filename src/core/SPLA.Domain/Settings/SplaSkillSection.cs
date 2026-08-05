using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

/// <summary>
/// The <c>skills:</c> block of a .spla / defaults.yaml file.
///
/// <para>Two independent concerns live here on purpose. <see cref="Sources"/> says WHERE skills come
/// from; <see cref="Items"/> says which individual skills are on. Both merge by key — sources by
/// their declared <c>id</c>, items by skill id — which is what every other named collection in these
/// files already does.</para>
///
/// <para>Sources used to be REPLACED wholesale by the more specific layer, and the comment justifying
/// it named the symptom rather than the cause: an entry had no name, so there was no key to merge on
/// and replacement was the only expressible thing. Once the entry declares an id, adding a folder is
/// one line in any layer instead of restating the whole list.</para>
/// </summary>
public class SplaSkillsSection
{
    /// <summary>Skill providers, in declaration order. Merged with the layers below by
    /// <see cref="SplaSkillSourceSection.Id"/>; an absent or empty list adds nothing and drops
    /// nothing. To stop inheriting entirely, say <see cref="InheritDefaults"/> instead.</summary>
    [YamlMember(Alias = "sources")]
    public List<SplaSkillSourceSection>? Sources { get; set; }

    /// <summary>
    /// Whether the built-in entries (<c>repo</c>, <c>local</c>, <c>machine</c>, <c>builtin</c>) are
    /// part of the fond. Null = true.
    ///
    /// <para>Exists for deployment, not for convenience: an administrator needs a white list — "only
    /// what I named" — rather than the hope that nobody forgot to switch off an extra folder. It is
    /// deliberately a separate flag, so clearing the inherited set is something you say out loud
    /// instead of a side effect of having written your own list.</para>
    /// </summary>
    [YamlMember(Alias = "inherit_defaults")]
    public bool? InheritDefaults { get; set; }

    /// <summary>Per-skill overrides, keyed by skill id.</summary>
    [YamlMember(Alias = "items")]
    public Dictionary<string, SplaSkillSection>? Items { get; set; }

    /// <summary>The model-backed librarian. Absent = off, and skill_find stays purely deterministic.</summary>
    [YamlMember(Alias = "librarian")]
    public SplaLibrarianSection? Librarian { get; set; }
}

/// <summary>
/// The librarian that understands a question instead of matching words.
///
/// <para><b>Why it has its own model.</b> The catalog goes into ITS system prompt, not the chat's, so
/// the expensive context is spent once in a throwaway call. That makes "weak model in the chat, a
/// competent one at the desk" a configuration rather than a compromise — which is the whole point of
/// putting the index somewhere other than the conversation.</para>
/// </summary>
public class SplaLibrarianSection
{
    /// <summary>Model entry id to ask. Null = the project's default model, i.e. the same one the chat
    /// uses — useful only when the chat model is already competent.</summary>
    [YamlMember(Alias = "model")]
    public string? Model { get; set; }

    /// <summary>Off by default: this costs an LLM call before work begins, and a deterministic tag
    /// match already answers most questions. Turn it on when the fond outgrows its vocabulary.</summary>
    [YamlMember(Alias = "enabled")]
    public bool? Enabled { get; set; }
}

/// <summary>
/// One configured skill provider. <see cref="Type"/> selects the factory; everything else is that
/// factory's business — the core never interprets these fields, it hands the whole section over and
/// lets the factory validate. <see cref="Path"/> is spelled out rather than buried in
/// <see cref="Options"/> only because every file-shaped source needs it.
/// </summary>
public class SplaSkillSourceSection
{
    /// <summary>
    /// The branch's declared name — a short word, unique across the whole library, and the key every
    /// layer merges on.
    ///
    /// <para>Absent is legal and falls back to a name derived from the entry itself (for a folder,
    /// the conventional name of that location, else the folder name). That is a fallback, not the
    /// identity: an entry that wants to be extended or switched off from another layer says its name.
    /// The path is an ordinary field, and renaming a folder no longer renames the branch.</para>
    /// </summary>
    [YamlMember(Alias = "id")]
    public string? Id { get; set; }

    /// <summary>Off switch. Null = on. This is how an inherited branch is dropped: it goes dark and
    /// stays visible in the panel, rather than vanishing so that nobody can remember it existed.
    /// There is no way to delete an inherited entry, and that is the intended shape.</summary>
    [YamlMember(Alias = "enabled")]
    public bool? Enabled { get; set; }

    /// <summary>Registered factory id, e.g. "directory".</summary>
    [YamlMember(Alias = "type")]
    public string? Type { get; set; }

    /// <summary>Location for file-shaped sources. Relative paths resolve against the workspace;
    /// <c>~</c> and environment variables are expanded.</summary>
    [YamlMember(Alias = "path")]
    public string? Path { get; set; }

    /// <summary>How much of this source reaches the model unasked: "out-of-catalog", "findable",
    /// "in-catalog" or "on-shelf". Absent = on-shelf, which is how sources have always behaved.
    /// <para>Separate from <c>trust</c> on purpose: trust says whether the skills may be used at all,
    /// level says only who is told they exist.</para></summary>
    [YamlMember(Alias = "level")]
    public string? Level { get; set; }

    /// <summary>"trusted" or "untrusted". Absent = trusted for local sources.</summary>
    [YamlMember(Alias = "trust")]
    public string? Trust { get; set; }

    /// <summary>Display name override for the settings panel.</summary>
    [YamlMember(Alias = "label")]
    public string? Label { get; set; }

    /// <summary>Type-specific settings for factories that need more than a path.</summary>
    [YamlMember(Alias = "options")]
    public Dictionary<string, string>? Options { get; set; }
}

/// <summary>Per-skill override. Null = "leave the skill's own frontmatter default".
/// <para>Only on/off lives here. Text that must always be in the prompt is not a skill setting — it
/// is <c>agent.instructions</c>, which owns that job outright.</para></summary>
public class SplaSkillSection
{
    [YamlMember(Alias = "enabled")]
    public bool? Enabled { get; set; }
}
