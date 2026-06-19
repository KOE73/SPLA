using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

public sealed class AgentMemoryClearTool : IMcpTool
{
    private readonly IKeyValueStore _project;

    public AgentMemoryClearTool(IKeyValueStore project) => _project = project;

    public string Name => "agent_memory_clear";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Bulk-delete entries from agent working memory. " +
                          "filter=null removes ALL entries in scope (caution). " +
                          "filter='prefix:' removes only matching keys. " +
                          "scope=null clears session only. " +
                          "Returns count of removed entries.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    filter = new { type = "string", description = "Substring filter on keys (e.g. 'context:' or 'task:'). Omit to wipe entire scope." },
                    scope  = new { type = "string", @enum = new[] { "session", "project" }, description = "session = this chat only (default); project = shared store." }
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
            var filter = ToolJson.GetString(root,"filter");
            var scope  = ToolJson.GetString(root,"scope");

            var store = AgentMemoryHelpers.SelectStore(_project, scope);
            if (store is null) return Task.FromResult("error: no active chat session");
            var sb    = new StringBuilder();
            AgentMemoryHelpers.AppendClear(sb, store, filter);
            return Task.FromResult(sb.ToString().TrimEnd());
        }
        catch (JsonException) { return Task.FromResult("error: invalid_json"); }
    }
}
