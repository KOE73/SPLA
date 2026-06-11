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
}
