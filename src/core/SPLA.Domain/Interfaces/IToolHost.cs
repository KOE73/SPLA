using SPLA.Domain.Models;
using SPLA.Domain.Tools;
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

    /// <summary>
    /// Runs a tool by name.
    /// <para>
    /// <paramref name="context"/> states whose call this is — which chat's state it acts on and
    /// whose rights apply. Omitting it means "read that from the ambient scopes", which is what
    /// every in-process caller inside a chat already implies; passing it is how a caller that has no
    /// ambient state to read — a second head, a queued job — says the same thing out loud.
    /// </para>
    /// </summary>
    Task<ToolResult> ExecuteToolAsync(
        AgentMode mode,
        string name,
        string argumentsJson,
        CancellationToken cancellationToken = default,
        ToolCallContext? context = null);
}
