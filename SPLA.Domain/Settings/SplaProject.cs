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

    [YamlMember(Alias = "workspace")]
    public string? Workspace { get; set; }

    [YamlMember(Alias = "agent")]
    public SplaAgentSection? Agent { get; set; }

    [YamlMember(Alias = "llm")]
    public SplaLlmSection? Llm { get; set; }

    [YamlMember(Alias = "ui")]
    public SplaUiSection? Ui { get; set; }

    [YamlMember(Alias = "permissions")]
    public SplaPermissionsSection? Permissions { get; set; }

    [YamlMember(Alias = "plugins")]
    public Dictionary<string, SplaPluginSection>? Plugins { get; set; }

    [YamlMember(Alias = "skills")]
    public Dictionary<string, SplaSkillSection>? Skills { get; set; }

    [YamlMember(Alias = "docs")]
    public List<string>? Docs { get; set; }

    [YamlMember(Alias = "ignore")]
    public List<string>? Ignore { get; set; }
}
