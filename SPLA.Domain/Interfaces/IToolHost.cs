using SPLA.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Interfaces;

/// <summary>
/// Abstraction over the tool/MCP host the agent loop drives. Lets the orchestrator depend on
/// a domain interface instead of the concrete <c>McpHost</c>, so the agent core can be unit
/// tested with a fake host and reused by any entry point (CLI, UI, server).
/// </summary>
public interface IToolHost
{
    IEnumerable<ToolDefinition> GetToolDefinitions();

    Task<string> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson, CancellationToken cancellationToken = default);
}
