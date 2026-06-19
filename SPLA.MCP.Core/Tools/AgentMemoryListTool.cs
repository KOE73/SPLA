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
    private readonly IKeyValueStore _session;
    private readonly IKeyValueStore _project;

    public AgentMemoryListTool(IKeyValueStore session, IKeyValueStore project)
    {
        _session = session;
        _project = project;
    }

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
                    filter = new { type = "string", description = "Key filter, case-insensitive. Supports glob * (e.g. 'host:*:tls', 'context:'). Omit for all keys." },
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
            var filter = ToolJson.GetString(root, "filter");
            var where  = ToolJson.GetString(root, "where");
            var top    = ToolJson.GetInt32(root, "top");
            var skip   = ToolJson.GetInt32(root, "skip") ?? 0;
            var scope  = ToolJson.GetString(root, "scope");

            var store = AgentMemoryHelpers.SelectStore(_session, _project, scope);
            var sb    = new StringBuilder();
            AgentMemoryHelpers.AppendList(sb, store, filter, top, skip, where);
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

        glob rules:
          host:*:tls    — keys starting with 'host:' and ending with ':tls'
          host:172.16.* — keys starting with 'host:172.16.'
          *:smtp        — keys ending with ':smtp'
          host:         — original substring behavior (no *)

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
