using SPLA.MCP.Core.Interfaces;
using System.Collections.Generic;

namespace SPLA.MCP.Core.Agent;

/// <summary>
/// One modular built-in agent capability: a bundle of tools plus (optionally) the system-prompt
/// fragment that teaches the model how to use them. A feature is the unit the <c>agent.capabilities</c>
/// project setting turns on/off — enabling a feature registers its tools AND surfaces its prompt text
/// together, so "prompt describes a tool the model doesn't have" cannot happen by construction.
/// </summary>
public interface IAgentFeature
{
    /// <summary>Dotted, stable id, e.g. <c>"core.memory"</c>.</summary>
    string Id { get; }

    /// <summary>Ids of other features this one depends on. Empty when none. Resolved transitively —
    /// enabling this feature auto-enables its requirements.</summary>
    IReadOnlyList<string> Requires { get; }

    /// <summary>Tools this feature contributes to the tool host. May be empty (prompt-only feature).</summary>
    IEnumerable<IMcpTool> Tools { get; }

    /// <summary>System-prompt text this feature contributes, or null when it has none (tools-only
    /// feature). Rendered as its own <c>Core: {Id}</c> segment by <c>SystemPromptBuilder</c>.</summary>
    string? PromptFragment { get; }
}
