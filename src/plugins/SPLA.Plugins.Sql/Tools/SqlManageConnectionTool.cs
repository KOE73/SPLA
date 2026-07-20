using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Sql.Tools;

/// <summary>
/// Adds, updates, removes, or lists named SQL connections at runtime.
/// Changes are immediately available to other sql tools in the current session
/// AND are persisted back into the .spla project file.
/// </summary>
public class SqlManageConnectionTool : IMcpTool
{
    private readonly SqlConnectionRegistry _registry;

    public SqlManageConnectionTool(SqlConnectionRegistry registry) => _registry = registry;

    public string Name => "sql_manage_connection";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = """
                Add, update, remove, or list named SQL connections.
                Changes apply immediately in this session AND are saved to the .spla project file.
                Actions:
                  list   — show all current connections
                  add    — create or overwrite a named connection (requires: name, provider, server/host/file, database)
                  remove — delete a named connection (requires: name)
                  default — set the default connection (requires: name)
                """,
            Scope = ToolScope.Local,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    action = new
                    {
                        type = "string",
                        @enum = new[] { "list", "add", "remove", "default" },
                        description = "Operation to perform."
                    },
                    name = new { type = "string", description = "Connection name (key)." },
                    provider = new { type = "string", @enum = new[] { "mssql", "postgres", "sqlite" }, description = "Database provider." },
                    server = new { type = "string", description = "Host/server name or instance (mssql/postgres). For sqlite: file path." },
                    database = new { type = "string", description = "Database name." },
                    trusted_connection = new { type = "boolean", description = "Use Windows/domain auth (mssql only). Default: true." },
                    user = new { type = "string", description = "Login (when not using Windows auth). Optional if the credential entry carries a 'user' field." },
                    credential = new { type = "string", description = "Secret-store entry name holding user+password. Preferred over 'password' — the literal never enters the project file." },
                    password = new { type = "string", description = "Password or env:VAR_NAME reference. Ignored when 'credential' is set." },
                    description = new { type = "string", description = "Human-readable description shown to the AI." }
                },
                required = new[] { "action" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        var action = root.GetString("action") ?? "list";

        return action switch
        {
            "list"    => Task.FromResult(List()),
            "add"     => Task.FromResult(Add(root)),
            "remove"  => Task.FromResult(Remove(root)),
            "default" => Task.FromResult(SetDefault(root)),
            _         => Task.FromResult($"Unknown action '{action}'. Use: list, add, remove, default.")
        };
    }

    private string List()
    {
        if (_registry.Connections.Count == 0)
            return "No connections configured.";

        var sb = new StringBuilder();
        sb.AppendLine("Current connections:");
        foreach (var (name, cfg) in _registry.Connections)
        {
            var mark = string.Equals(name, _registry.DefaultConnection, StringComparison.OrdinalIgnoreCase) ? " [default]" : "";
            var target = cfg.Server ?? cfg.Host ?? cfg.File ?? "(no server)";
            var auth = cfg.TrustedConnection ? "Windows auth" : $"user={cfg.User}";
            sb.AppendLine($"  {name}{mark} — {cfg.Provider} / {target} / {cfg.Database} ({auth})");
            if (!string.IsNullOrWhiteSpace(cfg.Description))
                sb.AppendLine($"    {cfg.Description}");
        }
        return sb.ToString().TrimEnd();
    }

    private string Add(JsonElement root)
    {
        var name = root.GetString("name");
        if (string.IsNullOrWhiteSpace(name)) return "Error: 'name' is required for action=add.";

        var provider = root.GetString("provider") ?? "mssql";
        var server = root.GetString("server");
        var database = root.GetString("database");
        var trustedConnection = root.TryGetProperty("trusted_connection", out var tc) ? tc.GetBoolean() : true;
        var user = root.GetString("user");
        var credential = root.GetString("credential");
        var password = root.GetString("password");
        var description = root.GetString("description");

        var cfg = new SqlConnectionConfig
        {
            Provider = provider,
            Server = provider != "sqlite" ? server : null,
            File = provider == "sqlite" ? server : null,
            Database = database,
            TrustedConnection = trustedConnection,
            User = user,
            Credential = credential,
            Password = credential is { Length: > 0 } ? null : password,
            Description = description
        };

        _registry.Add(name, cfg);
        var persist = _registry.Persist();
        return $"Connection '{name}' added/updated. {persist}";
    }

    private string Remove(JsonElement root)
    {
        var name = root.GetString("name");
        if (string.IsNullOrWhiteSpace(name)) return "Error: 'name' is required for action=remove.";

        if (!_registry.Remove(name))
            return $"Connection '{name}' not found.";

        var persist = _registry.Persist();
        return $"Connection '{name}' removed. {persist}";
    }

    private string SetDefault(JsonElement root)
    {
        var name = root.GetString("name");
        if (string.IsNullOrWhiteSpace(name)) return "Error: 'name' is required for action=default.";

        if (!_registry.Connections.ContainsKey(name))
            return $"Connection '{name}' not found. Use action=list to see available connections.";

        _registry.SetDefault(name);
        var persist = _registry.Persist();
        return $"Default connection set to '{name}'. {persist}";
    }
}

file static class JsonElementExtensions
{
    public static string? GetString(this JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}
