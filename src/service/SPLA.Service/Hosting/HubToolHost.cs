using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.Instances;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Service;

/// <summary>
/// A minimal <see cref="IToolHost"/> for the hub itself: no permission pipeline, no plugins, no
/// sessions — the hub has none of those, only a handful of tools that describe the machine rather
/// than one project. Deliberately not <c>McpHost</c>, which is built around a chat's ambient state
/// (<c>AgentSessionScope</c>, tool sets, zones) that does not exist at this level.
///
/// <para>Read-only today: one tool, <see cref="HubProjectsListTool"/>. Starting or stopping a project
/// from here is a real question (who may act, and through which of the existing
/// <see cref="RegistryRoutes.Start"/>/<see cref="RegistryRoutes.Stop"/> semantics) deliberately left
/// open rather than folded in on the first pass — see the discussion this shipped from.</para>
/// </summary>
internal sealed class HubToolHost : IToolHost
{
    private readonly Dictionary<string, IMcpTool> _tools;

    public HubToolHost(RegistryHub hub)
    {
        IMcpTool[] tools = [new HubProjectsListTool(hub)];
        _tools = tools.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<ToolDefinition> GetToolDefinitions() => _tools.Values.Select(t => t.GetDefinition());

    public async Task<ToolResult> ExecuteToolAsync(
        AgentMode mode,
        string name,
        string argumentsJson,
        CancellationToken cancellationToken = default,
        ToolCallContext? context = null)
    {
        if (!_tools.TryGetValue(name, out var tool))
            return ToolResult.Fail($"Error: Tool '{name}' not found.");

        return await tool.ExecuteAsync(argumentsJson, cancellationToken);
    }
}

/// <summary>
/// Lists every project this hub knows about — running or merely remembered — with whatever is
/// currently registered against it. The MCP-facing twin of <c>GET /registry/projects</c>
/// (<see cref="RegistryEndpoints.KnownProjects"/>), reusing the exact same data so a model asking
/// "what projects are on this machine" sees the same rows the Projects window does.
/// </summary>
internal sealed class HubProjectsListTool(RegistryHub hub) : IMcpTool
{
    public string Name => "hub_projects_list";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Lists every SPLA project this hub knows about on this machine — running " +
                "or merely remembered — with its current state (running/idle), how many windows are " +
                "attached, and whether it has an MCP endpoint available. Use this to discover which " +
                "projects exist before addressing one directly at its own MCP endpoint.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new { },
                required = Array.Empty<string>()
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var projects = RegistryEndpoints.KnownProjects(hub);
        var json = System.Text.Json.JsonSerializer.Serialize(projects, RegistryJson.Options);
        return Task.FromResult(ToolResult.Text(json));
    }
}
