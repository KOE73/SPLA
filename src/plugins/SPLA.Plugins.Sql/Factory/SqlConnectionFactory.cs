using System.Data;
using Microsoft.Data.SqlClient;

namespace SPLA.Plugins.Sql.Factory;

public static class SqlConnectionFactory
{
    public static async Task<IDbConnection> CreateAsync(SqlConnectionConfig cfg, CancellationToken ct = default)
    {
        return cfg.Provider.ToLowerInvariant() switch
        {
            "mssql"    => await CreateMssqlAsync(cfg, ct),
            "postgres" => await CreatePostgresAsync(cfg, ct),
            "sqlite"   => await CreateSqliteAsync(cfg, ct),
            _          => throw new NotSupportedException($"Unknown SQL provider: '{cfg.Provider}'")
        };
    }

    public static string BuildConnectionString(SqlConnectionConfig cfg) =>
        cfg.Provider.ToLowerInvariant() switch
        {
            "mssql"    => BuildMssqlConnectionString(cfg),
            "postgres" => BuildPostgresConnectionString(cfg),
            "sqlite"   => BuildSqliteConnectionString(cfg),
            _          => throw new NotSupportedException($"Unknown SQL provider: '{cfg.Provider}'")
        };

    // ── MSSQL ─────────────────────────────────────────────────────────────────

    private static async Task<IDbConnection> CreateMssqlAsync(SqlConnectionConfig cfg, CancellationToken ct)
    {
        var conn = new SqlConnection(BuildMssqlConnectionString(cfg));
        await conn.OpenAsync(ct);
        return conn;
    }

    public static string BuildMssqlConnectionString(SqlConnectionConfig cfg)
    {
        var b = new SqlConnectionStringBuilder
        {
            DataSource         = cfg.Server ?? "localhost",
            InitialCatalog     = cfg.Database ?? "",
            Encrypt            = SqlConnectionEncryptOption.Optional,
            TrustServerCertificate = true,
            ConnectTimeout     = 15,
            ApplicationName    = "SPLA"
        };

        if (cfg.TrustedConnection)
        {
            // Windows / domain Kerberos — no user/password needed
            b.IntegratedSecurity = true;
        }
        else
        {
            b.UserID   = cfg.User ?? "";
            b.Password = ResolvePassword(cfg.Password) ?? "";
        }

        return b.ConnectionString;
    }

    // ── PostgreSQL ─────────────────────────────────────────────────────────────

    private static Task<IDbConnection> CreatePostgresAsync(SqlConnectionConfig cfg, CancellationToken ct)
    {
        // TODO: add Npgsql NuGet reference
        throw new NotImplementedException("Postgres: add Npgsql NuGet reference");
    }

    public static string BuildPostgresConnectionString(SqlConnectionConfig cfg)
    {
        // TODO: use NpgsqlConnectionStringBuilder
        throw new NotImplementedException("Postgres: add Npgsql NuGet reference");
    }

    // ── SQLite ─────────────────────────────────────────────────────────────────

    private static Task<IDbConnection> CreateSqliteAsync(SqlConnectionConfig cfg, CancellationToken ct)
    {
        // TODO: add Microsoft.Data.Sqlite NuGet reference
        throw new NotImplementedException("SQLite: add Microsoft.Data.Sqlite NuGet reference");
    }

    public static string BuildSqliteConnectionString(SqlConnectionConfig cfg)
    {
        if (string.IsNullOrWhiteSpace(cfg.File))
            throw new InvalidOperationException("SQLite requires 'file' path.");
        return $"Data Source={cfg.File}";
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    public static string? ResolvePassword(string? raw)
    {
        if (raw is null) return null;
        if (raw.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
            return Environment.GetEnvironmentVariable(raw[4..].Trim());
        return raw;
    }
}
