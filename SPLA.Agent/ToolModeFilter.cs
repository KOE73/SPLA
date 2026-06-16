using SPLA.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Agent;

/// <summary>
/// Mode-based tool gating, independent of any UI. Decides which tools an agent mode may use,
/// based on each tool's <see cref="ToolScope"/>/<see cref="ToolEffect"/>. Entry points may
/// layer additional runtime toggles (e.g. the UI sidebar) on top of this.
/// </summary>
public static class ToolModeFilter
{
    /// <summary>Returns the subset of <paramref name="tools"/> allowed in <paramref name="mode"/>.</summary>
    public static List<ToolDefinition> Filter(IEnumerable<ToolDefinition> tools, AgentMode mode)
    {
        return tools.Where(t => IsAllowed(t, mode)).ToList();
    }

    /// <summary>Whether a single tool is allowed in <paramref name="mode"/>.</summary>
    public static bool IsAllowed(ToolDefinition tool, AgentMode mode)
    {
        // Agent-scoped capabilities (memory, info, datetime, context) are fundamental: they only
        // read/maintain the agent's own state and are available in every mode, including Chat.
        if (tool.Function.Scope == ToolScope.Agent) return true;

        if (mode == AgentMode.Chat) return false;

        // Web/Internet tools: Research, Edit, Agent
        if (tool.Function.Scope == ToolScope.Internet)
            return mode is AgentMode.Research or AgentMode.Edit or AgentMode.Agent;

        // Local read tools: Inspect, Edit, Agent
        if (tool.Function.Effect == ToolEffect.Read)
            return mode is AgentMode.Inspect or AgentMode.Edit or AgentMode.Agent;

        // Local write tools: Edit, Agent
        if (tool.Function.Effect == ToolEffect.Write)
            return mode is AgentMode.Edit or AgentMode.Agent;

        // Command execution: Agent only
        if (tool.Function.Effect == ToolEffect.Execute)
            return mode == AgentMode.Agent;

        return false;
    }
}
