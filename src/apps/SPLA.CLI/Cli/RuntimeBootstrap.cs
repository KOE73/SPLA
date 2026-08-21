using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Composition;
using SPLA.Runtime;

namespace SPLA.CLI;

/// <summary>Builds the shared agent stack for the chat-family commands (list/open/fork/run, and the
/// default REPL) and announces the tool count — the one line every one of them printed right after
/// building <see cref="AgentRuntime"/> back when this was all inline in <c>Program.cs</c>.</summary>
internal static class RuntimeBootstrap
{
    /// <param name="hostContributors">Prompt additions this invocation makes (see
    /// <c>Batch/CliContributor.cs</c>). Passed at construction because the composer is immutable once
    /// built — a runtime is shared by every chat of the project and recomposed per loop iteration.</param>
    /// <param name="instanceMode">What this invocation is doing with the project, for the instance
    /// lock — see <see cref="SPLA.Domain.Project.InstanceLock"/>.</param>
    public static AgentRuntime Build(
        ResolvedSettings settings,
        ILoggerFactory loggerFactory,
        IEnumerable<IAgentContributor>? hostContributors = null,
        string instanceMode = "repl")
    {
        var runtime = new AgentRuntime(settings, loggerFactory, hostContributors, instanceMode);
        Console.WriteLine($"Tools registered: {runtime.McpHost.GetToolDefinitions().Count()}");
        return runtime;
    }
}
