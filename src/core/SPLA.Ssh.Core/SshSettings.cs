using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// One named SSH host. Owned by the SSH plugin (not Domain). The credential is a reference, not a
/// value: <c>credential</c> names a whole <see cref="SPLA.Domain.Secrets.SecretEntry"/> in the global
/// secret store (fields <c>user</c>, <c>password</c> or <c>private_key</c>+<c>passphrase</c>);
/// <c>password</c> / <c>key_passphrase</c> are the older single-value pointers
/// (<c>secret:KEY</c>/<c>env:VAR</c>). Everything is materialized only at connect time via the
/// host's secret resolver — this config is committable and the LLM only ever sees references,
/// never a password. Binding host→credential lives here (the <c>~/.ssh/config</c> model), the
/// credential itself in the global secret store.
/// </summary>
public sealed class SshHostConfig
{
    [YamlMember(Alias = "host")]
    public string? Host { get; set; }

    [YamlMember(Alias = "port")]
    public int Port { get; set; } = 22;

    /// <summary>Login user. Optional when <c>credential</c> is set and the entry has a <c>user</c> field.</summary>
    [YamlMember(Alias = "user")]
    public string? User { get; set; }

    /// <summary>Secret-store entry key holding this host's credential record. The entry decides the
    /// auth method by its fields: <c>private_key</c> (+<c>passphrase</c>) → key auth, else
    /// <c>password</c> → password auth. Takes precedence over the single-value fields below.</summary>
    [YamlMember(Alias = "credential")]
    public string? Credential { get; set; }

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

    /// <summary>When true the read-only guard is NOT applied to the agent's commands on this host —
    /// it may install packages, edit files, restart services (apt, systemctl restart, …). Off by
    /// default: the operator opts a host in deliberately. Human terminal input was never guarded.</summary>
    [YamlMember(Alias = "allow_write")]
    public bool AllowWrite { get; set; }
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
