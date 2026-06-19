using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

public sealed class AgentMemorySetTool : IMcpTool
{
    private readonly IKeyValueStore _project;

    public AgentMemorySetTool(IKeyValueStore project) => _project = project;

    public string Name => "agent_memory_set";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Store or overwrite a key/value entry in agent working memory. " +
                          "Keys with 'context:' prefix are auto-injected into your prompt every turn — use them for live working state. " +
                          "scope: session = this chat (default); project = shared across all chats, always persistent.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    key   = new { type = "string",                          description = "Entry key. Use namespace:name (e.g. context:plan, task:current, note:finding, project:build-cmd)." },
                    value = new { type = "string",                          description = "Value to store. May be empty to clear the content while keeping the key." },
                    scope = new { type = "string", @enum = new[] { "session", "project" }, description = "session = this chat (default); project = shared, persistent." }
                },
                required = new[] { "key", "value" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root  = doc.RootElement;
            var key   = ToolJson.GetString(root, "key");
            var value = ToolJson.GetString(root,"value");
            var scope = ToolJson.GetString(root,"scope");

            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult("error: key is required");
            if (value is null)                  return Task.FromResult("error: value is required");

            var store = AgentMemoryHelpers.SelectStore(_project, scope);
            if (store is null) return Task.FromResult("error: no active chat session");
            store.Set(key, value);
            return Task.FromResult($"ok: set [{store.Scope}] {key}");
        }
        catch (JsonException) { return Task.FromResult("error: invalid_json"); }
    }
}
