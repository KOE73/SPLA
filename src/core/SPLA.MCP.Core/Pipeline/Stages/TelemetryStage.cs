using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.Observability;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Opens the span for the call and starts its clock, then hands both to the links inside via the
/// invocation — the accounting link stamps the outcome on the span, and reads the clock to measure
/// how long the tool ran.
/// <para>
/// Sits outside the permission link so that a refusal is still traced, and inside the disclosure
/// links so that a tool the caller could not see leaves no span at all: a span is a record of a call
/// that was considered, and a call to something undisclosed was never considered.
/// </para>
/// <para>Re-entrant by construction — <c>StartActivity</c> nests, and the clock lives on the
/// invocation, not on this link.</para>
/// </summary>
public sealed class TelemetryStage : IToolMiddleware
{
    private readonly ILogger? _logger;

    public TelemetryStage(ILogger? logger) => _logger = logger;

    public ToolPipelineStage Stage => ToolPipelineStage.Telemetry;

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        using var activity = SplaTelemetry.StartActivity("mcp.tool.execute");
        activity?.SetTag("spla.tool.name", call.Name);
        activity?.SetTag("spla.agent.mode", call.Mode.ToString());
        activity?.SetTag("spla.tool.source", call.Source ?? "agent");

        call.Activity = activity;
        call.StartedTimestamp = Stopwatch.GetTimestamp();

        // Source is null for a call from the agent's own loop (a chat, or a nested ctx.Run) and set
        // to a transport label ("mcp-stdio", "mcp-http <ip>") for a call that entered from outside the
        // process — see ToolCallContext.Source. Logging both that and the raw arguments here, at the
        // one point every call (chat-originated or foreign) passes through, is what lets an audit trail
        // answer "who asked for this, over what channel, with what parameters" without guessing.
        _logger?.LogInformation(
            "Tool execution started. Tool={ToolName} Mode={Mode} Source={Source} Arguments={Arguments}",
            call.Name, call.Mode, call.Source ?? "agent", call.ArgumentsJson);

        return await next(call, ct);
    }
}
