using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.Observability;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Turns a tool that threw into a result the conversation can carry, so that one broken tool does
/// not end the turn.
/// <para>
/// Outside the accounting link, and that placement is the whole reason this is a link of its own: a
/// call that threw produced no result to account for. Were the conversion to happen further in,
/// accounting would see the failure this link manufactures and record it as a returned one — the
/// same fault counted twice, once here and once there.
/// </para>
/// <para>Cancellation is not a fault and is not converted: the turn is being torn down, and a
/// cancelled call must not come back as an answer.</para>
/// </summary>
public sealed class FaultStage : IToolMiddleware
{
    private readonly ILogger? _logger;

    public FaultStage(ILogger? logger) => _logger = logger;

    public ToolPipelineStage Stage => ToolPipelineStage.Fault;

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        try
        {
            return await next(call, ct);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("Tool execution canceled. Tool={ToolName} Mode={Mode}", call.Name, call.Mode);
            throw;
        }
        catch (SPLA.Domain.Host.PathBoundaryException ex)
        {
            // A boundary refusal is a decision, not a fault, and the difference is what the model
            // does next: told "error reading file" it will try to fix a file it was never allowed to
            // touch. Caught here rather than in each tool because all twelve of them reach the disk
            // through the same seam.
            _logger?.LogInformation(
                "Tool refused by the path boundary. Tool={ToolName} Reason={Reason}", call.Name, ex.Refusal);
            return ToolResult.Refuse($"Refused: {ex.Message}.", ex.Refusal.ToString());
        }
        catch (Exception ex)
        {
            call.Activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            SplaTelemetry.ToolErrors.Add(1);
            _logger?.LogError(ex, "Tool execution failed. Tool={ToolName} Mode={Mode}", call.Name, call.Mode);
            return ToolResult.Fail($"Error executing tool {call.Name}: {ex.Message}", ex.GetType().Name);
        }
    }
}
