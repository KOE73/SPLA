using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Opens the progress-tree node for this call. The single place nodes are created, so a tool the
/// model called directly and a tool a script invoked through the host nest correctly with no extra
/// wiring — the script's call sits above its parallel children.
/// <para>
/// After the permission link, deliberately: a node opened before the decision would leave calls that
/// were refused, or that a human said no to, sitting in the tree as if they had happened.
/// </para>
/// <para>Nesting is the point, not a hazard: <see cref="ProgressScope.BeginNode"/> attaches to
/// whatever node is current, which is how a re-entrant call becomes a child rather than a sibling.</para>
/// </summary>
public sealed class ProgressNodeStage : IToolMiddleware
{
    public ToolPipelineStage Stage => ToolPipelineStage.Progress;

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        using var node = ProgressScope.BeginNode(call.Name);
        try
        {
            var result = await next(call, ct);
            // A tool that reports failure is a failure, whether it said so by throwing or by
            // returning — the fault link inside has already turned the former into the latter.
            if (result.IsError) node.Fail();
            return result;
        }
        catch
        {
            // Only cancellation reaches here; the fault link converts everything else.
            node.Fail();
            throw;
        }
    }
}
