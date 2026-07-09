namespace SPLA.Plugins.Sql.Tools;

/// <summary>
/// Shared helpers for all SQL tools. Uses <see cref="SqlConnectionRegistry"/> so that
/// connections added via sql_manage_connection are visible immediately in the same session.
/// </summary>
public abstract class SqlToolBase
{
    protected readonly SqlConnectionRegistry Registry;

    protected SqlToolBase(SqlConnectionRegistry registry) => Registry = registry;

    protected bool TryResolve(string? name, out SqlConnectionConfig? cfg, out string? error)
    {
        var key = name ?? Registry.DefaultConnection;
        if (key is null)
        {
            cfg = null;
            error = "No default connection configured. Specify the 'connection' parameter.";
            return false;
        }
        if (!Registry.Connections.TryGetValue(key, out cfg))
        {
            error = $"Unknown connection '{key}'. Available: {string.Join(", ", Registry.Connections.Keys)}";
            return false;
        }
        // Materialize the password (secret:/env: references) just before use; stored config is untouched.
        cfg = Registry.ResolveForOpen(cfg);
        error = null;
        return true;
    }
}
