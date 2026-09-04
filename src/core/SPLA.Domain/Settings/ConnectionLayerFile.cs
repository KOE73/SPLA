using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

/// <summary>
/// The on-disk shape of a <see cref="ConnectionScope.User"/> or <see cref="ConnectionScope.Shared"/>
/// connection file. One key, so the file reads the same as the <c>connections:</c> block a manifest
/// carries — a person moving an entry between scopes moves the same lines.
///
/// <para>A separate file rather than more keys in <c>defaults.yaml</c>: the settings panel rewrites
/// this whole list on save, and rewriting a file that also holds the machine's <c>llm:</c>, agent and
/// skill defaults would put unrelated hand-written config in the blast radius of every connection
/// edit. Same reason secrets are their own file.</para>
/// </summary>
public sealed class SplaConnectionLayer
{
    [YamlMember(Alias = "version")]
    public int Version { get; set; } = 1;

    [YamlMember(Alias = "connections")]
    public List<SplaConnectionSection>? Connections { get; set; }
}
