using SPLA.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline;

/// <summary>One link of the tool-call pipeline; call <c>next</c> to continue, or don't to refuse the
/// call.</summary>
public delegate Task<ToolResult> ToolCallDelegate(ToolCallInvocation call, CancellationToken ct);

/// <summary>
/// Where a link sits. The pipeline is ordered by this, not by registration order, so the constraints
/// that matter are expressed in the type rather than trusted to whoever writes the list — the same
/// arrangement <c>SPLA.Domain.Llm.LlmPipelineStage</c> already uses for the model call.
/// <para>
/// Two orderings here are load-bearing and were the reason to make the order a type at all:
/// </para>
/// <list type="bullet">
///   <item><see cref="Disclosure"/> comes <b>before</b> <see cref="Policy"/>: there is no sense in
///   weighing the rights of a tool that is not revealed to the caller in the first place.</item>
///   <item><see cref="Progress"/> comes <b>after</b> <see cref="Policy"/>: a node opened before the
///   decision would put calls that never happened into the progress tree.</item>
/// </list>
/// <para>
/// Every link must survive a re-entrant call: a script's <c>ctx.Run</c> enters this pipeline from
/// inside a call that is already running. Nothing here may hold per-call state on itself — the state
/// of a call lives in its <see cref="ToolCallInvocation"/>, which is fresh every time.
/// </para>
/// </summary>
public enum ToolPipelineStage
{
    /// <summary>Outermost. Turning a name into a tool; an unknown name ends here.</summary>
    Resolution = 0,

    /// <summary>Does the tool exist for this project right now — is its plugin enabled.</summary>
    Availability = 100,

    /// <summary>Is the tool revealed to the caller — is its tool set raised in this chat.</summary>
    Disclosure = 200,

    /// <summary>Tracing and the call's clock. Outside <see cref="Policy"/> so that a refusal is
    /// traced too, inside <see cref="Disclosure"/> so that a tool the caller cannot see leaves no
    /// span at all.</summary>
    Telemetry = 300,

    /// <summary>May this caller run this tool: the permission verdict, and the human it may have to
    /// ask.</summary>
    Policy = 400,

    /// <summary>Publishing the host and mode so a running tool can invoke other tools by name.</summary>
    Ambient = 500,

    /// <summary>The progress-tree node for this call. The single place nodes are created, which is
    /// what makes a script's children nest under it.</summary>
    Progress = 600,

    /// <summary>
    /// Turning an exception into a result. Its own stage, and deliberately <b>outside</b>
    /// <see cref="Accounting"/>: a call that threw did not produce a result to account for, and were
    /// this inside, the failure it manufactures would be recorded as if the tool had returned it —
    /// counting the same fault twice.
    /// </summary>
    Fault = 700,

    /// <summary>Innermost, next to the tool. Recording what the tool returned, once per actual
    /// execution.</summary>
    Accounting = 800
}

/// <summary>
/// A cross-cutting concern wrapped around the execution of one tool.
/// <para>
/// This is where a concurrency limit, a resource-ownership check, an audit trail or (later) a
/// sandbox splice belong. Until this existed there was no such place: every concern was unrolled by
/// hand into one method, each with its own early return.
/// </para>
/// </summary>
public interface IToolMiddleware
{
    ToolPipelineStage Stage { get; }

    Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct);
}
