using SPLA.Plugins.Sql.Factory;

namespace SPLA.Plugins.Sql;

/// <summary>
/// Single shared implementation of "open a connection and report the outcome".
/// Both the <c>sql_test_connection</c> agent tool and the Avalonia settings editor call this,
/// so they produce identical results. Unification is at the source level (one method), NOT at
/// runtime — each side runs it inside its own AssemblyLoadContext, sharing no live instance.
/// </summary>
public static class SqlConnectionTester
{
    public readonly record struct Result(bool Ok, string Message);

    public static async Task<Result> TestAsync(SqlConnectionConfig cfg, CancellationToken ct = default)
    {
        try
        {
            using var conn = await SqlConnectionFactory.CreateAsync(cfg, ct);
            var target = cfg.Server ?? cfg.Host ?? cfg.File ?? "(unknown)";
            var db = string.IsNullOrWhiteSpace(cfg.Database) ? "" : $" / {cfg.Database}";
            var auth = cfg is { Provider: "mssql", TrustedConnection: true } ? " [Windows auth]" : "";
            return new Result(true, $"OK — connected to {target}{db} ({cfg.Provider}){auth}.");
        }
        catch (Exception ex)
        {
            return new Result(false, $"Failed: {ex.Message}");
        }
    }
}
