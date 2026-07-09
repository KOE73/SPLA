using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

public class SplaSkillSection
{
    [YamlMember(Alias = "enabled")]
    public bool? Enabled { get; set; }

    [YamlMember(Alias = "preloaded")]
    public bool? Preloaded { get; set; }
}
