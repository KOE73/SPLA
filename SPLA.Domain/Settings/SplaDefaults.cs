using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

/// <summary>
/// Model for the global defaults.yaml file (~/.spla/defaults.yaml).
/// </summary>
public class SplaDefaults
{
    [YamlMember(Alias = "version")]
    public int Version { get; set; } = 1;

    [YamlMember(Alias = "llm")]
    public SplaLlmSection? Llm { get; set; }

    [YamlMember(Alias = "agent")]
    public SplaAgentSection? Agent { get; set; }

    [YamlMember(Alias = "ui")]
    public SplaUiSection? Ui { get; set; }
}
