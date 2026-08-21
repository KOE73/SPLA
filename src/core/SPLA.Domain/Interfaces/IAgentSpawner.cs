using SPLA.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Interfaces;

/// <summary>
/// Runs a task headlessly in an isolated agent session and returns the last assistant message.
/// Implemented by <c>SPLA.Agent.SpawnedAgentRunner</c>; injected into <c>AgentSpawnTool</c>
/// without creating a circular dependency between SPLA.MCP.Core and SPLA.Agent.
/// </summary>
public interface IAgentSpawner
{
    /// <param name="skillId">Skill to pin for the run, or <c>null</c> for a free-form task. A pinned
    /// skill puts its procedure in the sub-agent's prompt and is the only thing it does; without one
    /// the sub-agent starts from the base prompt and works from <paramref name="input"/> alone —
    /// including finding and activating a skill itself, if one turns out to fit.</param>
    Task<string> RunAsync(string? skillId, string input, AgentMode mode,
        CancellationToken cancellationToken = default);
}
