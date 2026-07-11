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

    [YamlMember(Alias = "reasoning_level")]
    public string? ReasoningLevel { get; set; }

    [YamlMember(Alias = "presence_penalty")]
    public double? PresencePenalty { get; set; }

    [YamlMember(Alias = "frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    [YamlMember(Alias = "repeat_penalty")]
    public double? RepeatPenalty { get; set; }
}

/// <summary>
/// A named LLM connection in the project's connection list. A connection bundles a provider with a
/// concrete endpoint and model; a chat references one by <see cref="Id"/> and can switch live.
/// Behaviour knobs (mode/temperature/reasoning) live on the chat, not here.
/// </summary>
public class SplaConnectionSection
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    [YamlMember(Alias = "provider")]
    public string? Provider { get; set; }

    [YamlMember(Alias = "endpoint")]
    public string? Endpoint { get; set; }

    [YamlMember(Alias = "api_key")]
    public string? ApiKey { get; set; }

    [YamlMember(Alias = "model")]
    public string? Model { get; set; }

    /// <summary>
    /// Manual context-window override in tokens for this connection. When set it wins over any
    /// auto-detected value (LM Studio native API / vLLM <c>max_model_len</c>) — for providers that
    /// report nothing, or when the user knows better. Null/0 = auto-detect.
    /// </summary>
    [YamlMember(Alias = "context_length")]
    public int? ContextLength { get; set; }

    /// <summary>When true the model field is locked — no picker shown in UI, model can only be viewed.</summary>
    [YamlMember(Alias = "lock_model")]
    public bool LockModel { get; set; }

    /// <summary>When true picking a different model triggers LM Studio unload+load via the management API.</summary>
    [YamlMember(Alias = "swap_model")]
    public bool SwapModel { get; set; }

    /// <summary>Display label for the picker — falls back to the model or id. Computed, never persisted.</summary>
    [YamlIgnore]
    public string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name!
        : !string.IsNullOrWhiteSpace(Model) ? Model!
        : Id;
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
