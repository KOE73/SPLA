using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Diagnostics;

namespace SPLA.MCP.Core.Pipeline;

/// <summary>
/// One trip through the pipeline. Created per call and never shared, which is what makes the
/// pipeline re-entrant: a script's <c>ctx.Run</c> enters the same baked chain of links with its own
/// invocation, so nothing a link writes here can leak into the call that is waiting above it.
/// <para>
/// The immutable half — who is calling and with what — is set at construction. The mutable half is
/// what an outer link discovered and an inner link needs: the resolved tool, and the span the inner
/// links stamp their outcome on.
/// </para>
/// </summary>
public sealed class ToolCallInvocation
{
    public ToolCallInvocation(AgentMode mode, string name, string argumentsJson)
    {
        Mode = mode;
        Name = name;
        ArgumentsJson = argumentsJson;
    }

    /// <summary>The mode the call runs under — the ceiling on what it may do.</summary>
    public AgentMode Mode { get; }

    public string Name { get; }

    public string ArgumentsJson { get; }

    /// <summary>Set by the resolution link; non-null everywhere downstream of it.</summary>
    public IMcpTool? Tool { get; set; }

    /// <summary>The span opened by the telemetry link, so the links inside it can record how the
    /// call ended. Null when nothing is listening.</summary>
    public Activity? Activity { get; set; }

    /// <summary>When the call started, as a <see cref="Stopwatch"/> timestamp.</summary>
    public long StartedTimestamp { get; set; }
}
