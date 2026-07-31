using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

/// <summary>
/// The <c>skills:</c> block of a .spla / defaults.yaml file.
///
/// <para>Two independent concerns live here on purpose. <see cref="Sources"/> says WHERE skills come
/// from; <see cref="Items"/> says which individual skills are on. Sources are replaced wholesale by
/// the more specific layer (otherwise a project could never drop an inherited source), while items
/// merge by id.</para>
/// </summary>
public class SplaSkillsSection
{
    /// <summary>Ordered list of skill providers. Earlier entries win when two sources offer the same
    /// skill id. Null (absent) means "use the built-in default set" — see SkillSourceRegistry.</summary>
    [YamlMember(Alias = "sources")]
    public List<SplaSkillSourceSection>? Sources { get; set; }

    /// <summary>Per-skill overrides, keyed by skill id.</summary>
    [YamlMember(Alias = "items")]
    public Dictionary<string, SplaSkillSection>? Items { get; set; }
}

/// <summary>
/// One configured skill provider. <see cref="Type"/> selects the factory; everything else is that
/// factory's business — the core never interprets these fields, it hands the whole section over and
/// lets the factory validate. <see cref="Path"/> is spelled out rather than buried in
/// <see cref="Options"/> only because every file-shaped source needs it.
/// </summary>
public class SplaSkillSourceSection
{
    /// <summary>Registered factory id, e.g. "directory".</summary>
    [YamlMember(Alias = "type")]
    public string? Type { get; set; }

    /// <summary>Location for file-shaped sources. Relative paths resolve against the workspace;
    /// <c>~</c> and environment variables are expanded.</summary>
    [YamlMember(Alias = "path")]
    public string? Path { get; set; }

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

/// <summary>Per-skill override. Both fields null = "leave the skill's own frontmatter default".</summary>
public class SplaSkillSection
{
    [YamlMember(Alias = "enabled")]
    public bool? Enabled { get; set; }

    [YamlMember(Alias = "preloaded")]
    public bool? Preloaded { get; set; }
}
