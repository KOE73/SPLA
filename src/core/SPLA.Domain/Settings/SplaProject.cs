using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

/// <summary>
/// Model for the .spla project file.
/// All properties nullable — missing means "inherit from defaults".
/// </summary>
public class SplaProject
{
    [YamlMember(Alias = "version")]
    public int Version { get; set; } = 1;

    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    // No `workspace:` field. The project root IS the directory holding this manifest — there is one
    // definition and it is not configurable, because a second one is a second answer to "where does
    // the agent work", and every boundary drawn on the first is void the moment the second wins.
    // Old manifests still carrying the key load fine: the deserializer ignores unmatched properties.

    /// <summary>Folders outside the root, named here and addressed as <c>mnt/&lt;name&gt;/...</c>.
    /// The name travels in git, the target is a property of the machine — which is the only split
    /// under which an out-of-root path is both stable in an instruction and portable.</summary>
    [YamlMember(Alias = "mounts")]
    public List<SplaMountSection>? Mounts { get; set; }

    [YamlMember(Alias = "agent")]
    public SplaAgentSection? Agent { get; set; }

    [YamlMember(Alias = "llm")]
    public SplaLlmSection? Llm { get; set; }

    /// <summary>Named connections for this project's chats (merged over defaults by id).</summary>
    [YamlMember(Alias = "connections")]
    public List<SplaConnectionSection>? Connections { get; set; }

    [YamlMember(Alias = "ui")]
    public SplaUiSection? Ui { get; set; }

    [YamlMember(Alias = "permissions")]
    public SplaPermissionsSection? Permissions { get; set; }

    [YamlMember(Alias = "plugins")]
    public Dictionary<string, SplaPluginSection>? Plugins { get; set; }

    /// <summary>Tool set id → disclosure level (<c>disabled</c>, <c>skill_demand</c>,
    /// <c>agent_demand</c>, <c>enabled</c>). A set with no entry derives its level from its
    /// supplier's on/off flag, so this section is only written when the user departs from that.
    /// See <c>agents/toolsets.md</c>.</summary>
    [YamlMember(Alias = "toolsets")]
    public Dictionary<string, string>? ToolSets { get; set; }

    /// <summary>Scheme id → enabled, for the resource-address abstraction (<see cref="SplaAgentSection.UnifiedResources"/>
    /// is the master switch; this is the per-scheme override underneath it). A scheme absent from the
    /// map is enabled — the map exists to record exceptions, not to enumerate everything registered.</summary>
    [YamlMember(Alias = "resources")]
    public Dictionary<string, bool>? Resources { get; set; }

    [YamlMember(Alias = "skills")]
    public SplaSkillsSection? Skills { get; set; }

    [YamlMember(Alias = "docs")]
    public List<string>? Docs { get; set; }

    [YamlMember(Alias = "ignore")]
    public List<string>? Ignore { get; set; }
}
