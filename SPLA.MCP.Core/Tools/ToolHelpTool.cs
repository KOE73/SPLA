using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Text.Json;

namespace SPLA.MCP.Core.Tools;

public sealed class ToolHelpTool : IMcpTool, IToolHelpProvider
{
    private readonly McpHost _host;

    public ToolHelpTool(McpHost host)
    {
        _host = host;
    }

    public string Name => "tool.help";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Returns detailed usage help for an active registered tool by name.",
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    name = new { type = "string", description = "Tool name or partial name to look up." }
                },
                required = new[] { "name" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("name", out var nameElement))
            {
                return Task.FromResult("found: false\nreason: missing_name");
            }

            var name = nameElement.GetString();
            return Task.FromResult(_host.GetToolHelp(name));
        }
        catch (JsonException)
        {
            return Task.FromResult("found: false\nreason: invalid_json");
        }
    }

    public Task<string> ExecuteAsync(AgentMode mode, string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("name", out var nameElement))
            {
                return Task.FromResult("found: false\nreason: missing_name");
            }

            var name = nameElement.GetString();
            return Task.FromResult(_host.GetToolHelp(name, mode));
        }
        catch (JsonException)
        {
            return Task.FromResult("found: false\nreason: invalid_json");
        }
    }

    public string? GetHelpText() =>
        """
        tool: tool.help

        summary: Returns detailed documentation for active registered tools. This is a system meta-tool; plugin internals are hidden behind the registry.

        arguments:
          name:
            formats:
              - exact_tool_name
              - partial_tool_name_for_suggestions
            examples:
              - system.fs.search_text
              - network.scan.ports
              - scan

        behavior:
          exact_match: returns tool name, plugin id, description, parameters, and help text when provided by the tool.
          partial_match: returns found: false with suggestions.
          disabled_or_missing_tool: returns found: false and reason: tool_not_registered_or_plugin_disabled.

        examples:
          - request:
              name: network.scan.ports
        """;
}
