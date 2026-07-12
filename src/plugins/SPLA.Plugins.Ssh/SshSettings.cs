using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// One named SSH host. Owned by the SSH plugin (not Domain). The credential is a reference, not a
/// value: <c>password</c> / <c>key_passphrase</c> hold <c>secret:KEY</c> or <c>env:VAR</c> pointers,
/// materialized only at connect time via the host's secret resolver — this config is committable and
/// the LLM only ever sees the reference, never the password. Binding host→credential lives here
/// (the <c>~/.ssh/config</c> model), the credential itself in the global secret store.
/// </summary>
public sealed class SshHostConfig
{
    [YamlMember(Alias = "host")]
    public string? Host { get; set; }

    [YamlMember(Alias = "port")]
    public int Port { get; set; } = 22;

    [YamlMember(Alias = "user")]
    public string? User { get; set; }

    /// <summary>Reference to the password: <c>secret:KEY</c> or <c>env:VAR</c>. Null when using key auth.</summary>
    [YamlMember(Alias = "password")]
    public string? Password { get; set; }

    /// <summary>Path to a private key file for public-key auth. Null when using password auth.</summary>
    [YamlMember(Alias = "key_file")]
    public string? KeyFile { get; set; }

    /// <summary>Reference to the key passphrase (<c>secret:KEY</c>/<c>env:VAR</c>), if the key is encrypted.</summary>
    [YamlMember(Alias = "key_passphrase")]
    public string? KeyPassphrase { get; set; }

    /// <summary>Shown to the LLM — describes what this host is for.</summary>
    [YamlMember(Alias = "description")]
    public string? Description { get; set; }
}

/// <summary>
/// Root of the SSH plugin's settings blob (stored opaquely under <c>plugins.ssh.settings</c> in .spla).
/// </summary>
public sealed class SshSettings
{
    [YamlMember(Alias = "default_host")]
    public string? DefaultHost { get; set; }

    /// <summary>Connection timeout in seconds for connect + command execution.</summary>
    [YamlMember(Alias = "timeout_seconds")]
    public int TimeoutSeconds { get; set; } = 20;

    [YamlMember(Alias = "hosts")]
    public Dictionary<string, SshHostConfig> Hosts { get; set; } = new();

    private static readonly ISerializer Ser = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    private static readonly IDeserializer De = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static SshSettings Parse(string? yaml) =>
        string.IsNullOrWhiteSpace(yaml) ? new() : De.Deserialize<SshSettings>(yaml) ?? new();

    /// <summary>Builds settings from the opaque nested mapping the runtime receives via ResolvedSettings.</summary>
    public static SshSettings FromBlob(Dictionary<string, object>? blob)
    {
        if (blob is null || blob.Count == 0) return new();
        return Parse(Ser.Serialize(blob));
    }
}
