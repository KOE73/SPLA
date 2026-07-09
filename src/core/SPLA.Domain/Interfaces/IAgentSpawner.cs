using SPLA.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Interfaces;

/// <summary>
/// Runs a skill headlessly in an isolated agent session and returns the last assistant message.
/// Implemented by <c>SPLA.Agent.SpawnedAgentRunner</c>; injected into <c>AgentSpawnTool</c>
/// without creating a circular dependency between SPLA.MCP.Core and SPLA.Agent.
/// </summary>
public interface IAgentSpawner
{
    Task<string> RunSkillAsync(string skillId, string input, AgentMode mode,
        CancellationToken cancellationToken = default);
}
