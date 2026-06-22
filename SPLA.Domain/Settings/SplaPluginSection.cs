using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

public class SplaPluginSection
{
    [YamlMember(Alias = "enabled")]
    public bool? Enabled { get; set; }

    [YamlMember(Alias = "custom_prompt")]
    public string? CustomPrompt { get; set; }

    [YamlMember(Alias = "tools")]
    public Dictionary<string, bool>? Tools { get; set; }

    /// <summary>
    /// Opaque plugin-owned settings blob. The host never interprets this — it only
    /// stores/retrieves it. Each plugin serializes its own typed model into this mapping.
    /// </summary>
    [YamlMember(Alias = "settings")]
    public Dictionary<string, object>? Settings { get; set; }
}
