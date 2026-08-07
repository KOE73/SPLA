using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.Observability;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Records what the tool returned: one row per execution that actually produced a result.
/// <para>
/// Innermost, next to the tool, so that everything it counts is a call that ran. Refusals from the
/// links outside never reach it — a call that policy stopped is not a tool call with an error, it is
/// not a tool call — and neither do exceptions, which the fault link outside absorbs.
/// </para>
/// <para>
/// A returned failure counts as an error just as a thrown one does. That equivalence is the point of
/// the typed result: while the result was a string, only a throw was visible here, so a tool that
/// returned <c>"error: …"</c> was recorded as a success and the error rate flattered itself.
/// </para>
/// </summary>
public sealed class AccountingStage : IToolMiddleware
{
    private readonly ILogger? _logger;

    public AccountingStage(ILogger? logger) => _logger = logger;

    public ToolPipelineStage Stage => ToolPipelineStage.Accounting;

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        var result = await next(call, ct);

        if (result.IsError)
            call.Activity?.SetStatus(ActivityStatusCode.Error, result.Reason);

        var elapsedMs = Stopwatch.GetElapsedTime(call.StartedTimestamp).TotalMilliseconds;
        SplaTelemetry.ToolCalls.Add(1);
        SplaTelemetry.ToolDurationMs.Record(elapsedMs);
        if (result.IsError) SplaTelemetry.ToolErrors.Add(1);
        _logger?.LogInformation(
            "Tool execution finished. Tool={ToolName} Outcome={Outcome} DurationMs={DurationMs:F1} ResultLength={ResultLength}",
            call.Name,
            result.Outcome,
            elapsedMs,
            result.TextContent.Length);

        return result;
    }
}
