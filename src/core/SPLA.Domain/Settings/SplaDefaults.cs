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

    /// <summary>Named connections available to all chats. Optional — a default is synthesized from
    /// <see cref="Llm"/> when empty.</summary>
    [YamlMember(Alias = "connections")]
    public List<SplaConnectionSection>? Connections { get; set; }

    [YamlMember(Alias = "agent")]
    public SplaAgentSection? Agent { get; set; }

    [YamlMember(Alias = "ui")]
    public SplaUiSection? Ui { get; set; }

    /// <summary>Secret-store backend selection. Machine-only; absent = plaintext file store.</summary>
    [YamlMember(Alias = "secrets")]
    public SplaSecretsSection? Secrets { get; set; }
}
