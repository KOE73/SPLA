using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;

namespace SPLA.MCP.Core.Tools;

public sealed class AgentMemoryGetTool : IMcpTool
{
    private readonly IKeyValueStore _project;

    public AgentMemoryGetTool(IKeyValueStore project) => _project = project;

    public string Name => "agent_memory_get";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Read a single key/value entry from agent working memory. Returns the value or not_found.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    key   = new { type = "string",                          description = "Entry key to look up." },
                    scope = new { type = "string", @enum = new[] { "session", "project" }, description = "session = this chat (default); project = shared, persistent." }
                },
                required = new[] { "key" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root  = doc.RootElement;
            var key   = ToolJson.GetString(root,"key");
            var scope = ToolJson.GetString(root,"scope");

            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult(ToolResult.Fail("error: key is required", "missing key"));

            var store = AgentMemoryHelpers.SelectStore(_project, scope);
            if (store is null) return Task.FromResult(ToolResult.Refuse("error: no active chat session", "no chat session"));
            var entry = store.Entries().FirstOrDefault(e => e.Key == key);
            if (entry is null) return Task.FromResult(ToolResult.Text($"not_found: [{store.Scope}] {key}"));

            // Reading a labelled entry carries its label into this chat. Without it the project store
            // launders: written in a chat that had been on the open web, read back in a fresh one.
            if (entry.Origin is { } origin)
                AgentSessionScope.Current?.Doubt.Observe(origin, $"memory:{key}");

            return Task.FromResult(ToolResult.Text(entry.Value));
        }
        catch (JsonException) { return Task.FromResult(ToolResult.Fail("error: invalid_json", "invalid json")); }
    }
}
