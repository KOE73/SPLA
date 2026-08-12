using System.Globalization;
using SPLA.Domain.Security;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Plugins.Sql;

/// <summary>
/// One named database connection. Owned by the SQL plugin (not Domain).
/// </summary>
public sealed class SqlConnectionConfig
{
    [YamlMember(Alias = "provider")]
    public string Provider { get; set; } = "mssql"; // mssql | postgres | sqlite

    [YamlMember(Alias = "server")]
    public string? Server { get; set; }

    [YamlMember(Alias = "host")]
    public string? Host { get; set; }

    [YamlMember(Alias = "port")]
    public int? Port { get; set; }

    [YamlMember(Alias = "database")]
    public string? Database { get; set; }

    [YamlMember(Alias = "user")]
    public string? User { get; set; }

    /// <summary>Secret-store entry key holding this connection's credential record (fields
    /// <c>user</c> + <c>password</c>). When set, it supplies the login user (unless <c>user</c> is
    /// given here) and the password; the literal never enters this committable config. Mirrors the
    /// SSH plugin's <c>credential</c> field. Takes precedence over <see cref="Password"/>.</summary>
    [YamlMember(Alias = "credential")]
    public string? Credential { get; set; }

    /// <summary>Reference to the password: <c>secret:KEY</c> or <c>env:VAR</c> (legacy single-value
    /// pointer). Ignored when <see cref="Credential"/> is set.</summary>
    [YamlMember(Alias = "password")]
    public string? Password { get; set; }

    /// <summary>MSSQL: Windows / domain Kerberos auth.</summary>
    [YamlMember(Alias = "trusted_connection")]
    public bool TrustedConnection { get; set; }

    /// <summary>SQLite file path.</summary>
    [YamlMember(Alias = "file")]
    public string? File { get; set; }

    /// <summary>Shown to the LLM — describes the business purpose of this database.</summary>
    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    /// <summary>
    /// Who this connection is as an island, for grants. Built from what it reaches and as whom —
    /// never from <paramref name="name"/>, so renaming carries no permissions with it and swapping
    /// two names swaps nothing.
    ///
    /// <para><see cref="Description"/> is absent on purpose: rewording the blurb the model reads must
    /// not cost the operator a re-approval.</para>
    /// </summary>
    public IslandIdentity Identity(string name) => new(
        "sql",
        Substance.Of(
            ("provider", Provider),
            ("server", Server),
            ("host", Host),
            ("port", Port?.ToString(CultureInfo.InvariantCulture)),
            ("database", Database),
            ("file", File),
            ("user", User),
            ("trusted", TrustedConnection ? "1" : "0"),
            ("credential", Substance.CredentialShape(Credential, Password))),
        name);
}

/// <summary>
/// Root of the SQL plugin's settings blob (stored opaquely under plugins.sql.settings in .spla).
/// </summary>
public sealed class SqlSettings
{
    [YamlMember(Alias = "default_connection")]
    public string? DefaultConnection { get; set; }

    [YamlMember(Alias = "default_limit")]
    public int DefaultLimit { get; set; } = 10;

    [YamlMember(Alias = "connections")]
    public Dictionary<string, SqlConnectionConfig> Connections { get; set; } = new();

    // ── Serialization (the host hands us a YAML string; we own the schema) ──────

    private static readonly ISerializer Ser = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    private static readonly IDeserializer De = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public string ToYaml() => Ser.Serialize(this);

    public static SqlSettings Parse(string? yaml) =>
        string.IsNullOrWhiteSpace(yaml) ? new() : De.Deserialize<SqlSettings>(yaml) ?? new();

    /// <summary>
    /// Builds settings from the opaque nested mapping the runtime receives via ResolvedSettings.
    /// </summary>
    public static SqlSettings FromBlob(Dictionary<string, object>? blob)
    {
        if (blob is null || blob.Count == 0) return new();
        return Parse(Ser.Serialize(blob));
    }
}
