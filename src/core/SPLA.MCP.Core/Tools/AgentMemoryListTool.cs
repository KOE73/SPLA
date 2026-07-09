using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

public sealed class AgentMemoryListTool : IMcpTool, IToolHelpProvider
{
    private readonly IKeyValueStore _project;

    public AgentMemoryListTool(IKeyValueStore project) => _project = project;

    public string Name => "agent_memory_list";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "List key/value entries in agent working memory with optional filter, where clause, and pagination. " +
                          "filter supports glob patterns with * (e.g. 'host:*:tls'). " +
                          "where filters by value field: 'field=pattern' (e.g. 'hostname=*kombinat*'). " +
                          "Auto-mode (top=null): returns all if ≤25 matches, first 10 + hint if more. " +
                          "scope: session (default) or project.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    filter      = new { type = "string", description = "Key filter. In glob mode (default): '*' matches any sequence, no '*' = substring match. In regex mode: full .NET regex. Omit for all keys." },
                    filter_mode = new { type = "string", @enum = new[] { "glob", "regex" }, description = "glob (default) — '*' wildcard + substring fallback. regex — full .NET regex, case-insensitive." },
                    where  = new { type = "string", description = "Value filter: 'field=pattern'. Extracts field from JSON value, glob-matches pattern. Use '=pattern' to match raw value. E.g. 'hostname=*kombinat*', 'ping=true'." },
                    top    = new { type = "integer", description = "Max entries to return. Omit = auto (all if ≤25 matches, first 10 + hint if more)." },
                    skip   = new { type = "integer", description = "Entries to skip before applying top (pagination offset). Omit = 0." },
                    scope  = new { type = "string", @enum = new[] { "session", "project" }, description = "session = this chat (default); project = shared, persistent." }
                },
                required = Array.Empty<string>()
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root   = doc.RootElement;
            var filter     = ToolJson.GetString(root, "filter");
            var filterMode = ToolJson.GetString(root, "filter_mode");
            var where      = ToolJson.GetString(root, "where");
            var top        = ToolJson.GetInt32(root, "top");
            var skip       = ToolJson.GetInt32(root, "skip") ?? 0;
            var scope      = ToolJson.GetString(root, "scope");
            var regexMode  = string.Equals(filterMode, "regex", StringComparison.OrdinalIgnoreCase);

            var store = AgentMemoryHelpers.SelectStore(_project, scope);
            if (store is null) return Task.FromResult("error: no active chat session");
            var sb    = new StringBuilder();
            AgentMemoryHelpers.AppendList(sb, store, filter, top, skip, where, regexMode);
            var text = sb.ToString().TrimEnd();
            return Task.FromResult(text.Length == 0
                ? (string.IsNullOrEmpty(filter) ? "(empty)" : $"(no entries matching '{filter}')")
                : text);
        }
        catch (JsonException) { return Task.FromResult("error: invalid_json"); }
    }

    public string? GetHelpText() => """
        tool: agent_memory_list

        summary: List key/value entries with glob key filter, value where clause, and pagination.

        arguments:
          filter: key filter, supports glob * (e.g. 'host:*:tls', 'context:', 'host:172.16.*')
          where:  value filter — 'field=pattern' extracts JSON field and glob-matches
                  '=pattern' matches the raw value string
                  examples: 'hostname=*kombinat*', 'ping=true', 'class=active'
          top:    max entries to return
          skip:   pagination offset
          scope:  session (default) or project

        filter_mode:
          glob  (default) — '*' matches any sequence; no '*' = case-insensitive substring
          regex           — full .NET regex, case-insensitive; '.' is NOT auto-escaped

        glob examples:
          host:*:tls        — all TLS records
          host:172.16.*     — subnet prefix
          *:smtp            — keys ending with :smtp
          host:             — substring match

        regex examples:
          filter='host:172\.16\.(20|21)\.17[0-4].*'  filter_mode='regex'
          filter='host:172\.16\.20\.17[0-4].*'        filter_mode='regex'

        default behaviour (top=null):
          match ≤ 25  → return all
          match > 25  → return first 10 + hint with next-page args

        examples:
          filter='host:*:tls'                    — all TLS records
          filter='host:*' where='ping=true'      — all hosts that answered ping
          filter='host:*:http' where='status=200'— HTTP records with 200 OK
          filter='host:*' where='class=active'   — active hosts only
        """;
}
