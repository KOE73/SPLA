using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

public sealed class AgentMemoryDeleteTool : IMcpTool
{
    private readonly IKeyValueStore _session;
    private readonly IKeyValueStore _project;

    public AgentMemoryDeleteTool(IKeyValueStore session, IKeyValueStore project)
    {
        _session = session;
        _project = project;
    }

    public string Name => "agent_memory_delete";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Delete a single key/value entry from agent working memory. Use agent_memory_clear for bulk deletion.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    key   = new { type = "string",                          description = "Entry key to delete." },
                    scope = new { type = "string", @enum = new[] { "session", "project" }, description = "session = this chat (default); project = shared, persistent." }
                },
                required = new[] { "key" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root  = doc.RootElement;
            var key   = ToolJson.GetString(root,"key");
            var scope = ToolJson.GetString(root,"scope");

            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult("error: key is required");

            var store = AgentMemoryHelpers.SelectStore(_session, _project, scope);
            return Task.FromResult(store.Delete(key)
                ? $"ok: deleted [{store.Scope}] {key}"
                : $"not_found: [{store.Scope}] {key}");
        }
        catch (JsonException) { return Task.FromResult("error: invalid_json"); }
    }
}
