using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

/// <summary>
/// Agent behavior section — mode and instruction files.
/// </summary>
public class SplaAgentSection
{
    [YamlMember(Alias = "mode")]
    public string? Mode { get; set; }

    [YamlMember(Alias = "instructions")]
    public List<string>? Instructions { get; set; }

    [YamlMember(Alias = "compact_tail_messages")]
    public int? CompactTailMessages { get; set; }

    [YamlMember(Alias = "custom_prompt")]
    public string? CustomPrompt { get; set; }
}

/// <summary>
/// LLM connection section.
/// </summary>
public class SplaLlmSection
{
    [YamlMember(Alias = "provider")]
    public string? Provider { get; set; }

    [YamlMember(Alias = "endpoint")]
    public string? Endpoint { get; set; }

    [YamlMember(Alias = "api_key")]
    public string? ApiKey { get; set; }

    [YamlMember(Alias = "model")]
    public string? Model { get; set; }

    [YamlMember(Alias = "temperature")]
    public double? Temperature { get; set; }
}

/// <summary>
/// UI preferences section.
/// </summary>
public class SplaUiSection
{
    [YamlMember(Alias = "theme")]
    public string? Theme { get; set; }

    [YamlMember(Alias = "density")]
    public string? Density { get; set; }

    [YamlMember(Alias = "bubble_chat")]
    public bool? BubbleChat { get; set; }

    [YamlMember(Alias = "selected_chat_view_id")]
    public string? SelectedChatViewId { get; set; }
}

/// <summary>
/// Per-effect permission overrides. Values: "allow", "ask", "deny".
/// </summary>
public class SplaPermissionsSection
{
    [YamlMember(Alias = "read")]
    public string? Read { get; set; }

    [YamlMember(Alias = "write")]
    public string? Write { get; set; }

    [YamlMember(Alias = "shell")]
    public string? Shell { get; set; }

    [YamlMember(Alias = "internet")]
    public string? Internet { get; set; }

    [YamlMember(Alias = "tools")]
    public List<SplaToolPermissionRule>? Tools { get; set; }
}

public class SplaToolPermissionRule
{
    [YamlMember(Alias = "tool")]
    public string? Tool { get; set; }

    [YamlMember(Alias = "arguments")]
    public string? Arguments { get; set; }

    [YamlMember(Alias = "decision")]
    public string? Decision { get; set; }
}
