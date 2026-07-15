using SPLA.MCP.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace SPLA.MCP.Core.Agent;

/// <summary>
/// Generic <see cref="IAgentFeature"/> — a plain bundle of already-constructed tools plus an optional
/// prompt fragment. Concrete tool instances still need per-project dependencies (KV store, SkillManager,
/// SpawnedAgentRunner, ...) exactly as before; this type only groups the finished instances under one
/// id/requires/prompt triple so the composition root (AgentRuntime) can gate them as a unit.
/// </summary>
public sealed class AgentFeature : IAgentFeature
{
    public string Id { get; }
    public IReadOnlyList<string> Requires { get; }
    public IEnumerable<IMcpTool> Tools { get; }
    public string? PromptFragment { get; }

    public AgentFeature(
        string id,
        IEnumerable<IMcpTool>? tools = null,
        string? promptFragment = null,
        IReadOnlyList<string>? requires = null)
    {
        Id = id;
        Tools = tools ?? Array.Empty<IMcpTool>();
        PromptFragment = promptFragment;
        Requires = requires ?? Array.Empty<string>();
    }
}
