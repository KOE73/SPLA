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
/// <b>Adding a stage is not choosing a number.</b> A new concern must state its relation to three
/// points, and the number falls out of the answers:
/// </para>
/// <list type="number">
///   <item><see cref="Policy"/> — before the verdict or after it. Before means the concern also
///   applies to calls that never run.</item>
///   <item><see cref="Background"/> — outside means the link sees the <i>launch</i> and is done in
///   milliseconds; inside means it sees the <i>work</i> and lives as long as the task does.</item>
///   <item><see cref="Accounting"/> — outside means this call is counted as one that produced a
///   result; inside means it is not.</item>
/// </list>
/// <para>
/// Those three are where a mistake is invisible in tests and surfaces later as numbers somebody
/// trusts. The standing example: <see cref="Telemetry"/> (300) sits <b>outside</b>
/// <see cref="Background"/>, so a backgrounded call's span closes the moment the task detaches and
/// measures the launch rather than the work. That is fixed in the background link — its own
/// <c>Activity</c> linked to the parent span — not by moving telemetry.
/// </para>
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

    /// <summary>
    /// Letting the call go on without the turn: the caller gets a task id now, the result arrives on
    /// a later turn boundary. Declared here, not yet implemented — see
    /// <c>docs/adr/ADR_20260824-2_core_background-tool-calls.md</c>.
    /// <para>
    /// <b>Inside <see cref="Ambient"/></b> because a detached task must inherit the ambient scopes:
    /// an <c>AsyncLocal</c> is copied when the <c>Task</c> is created, and the parent's later
    /// <c>Dispose</c> restores only the parent's own copy. <b>Outside <see cref="Progress"/></b>
    /// because the task needs its own tree root — a node under a turn that is about to close would
    /// take the running work down with it.
    /// </para>
    /// <para>
    /// This is the stage that splits the pipeline in two halves, and every link has to know which
    /// half it is in: outside sees the launch, inside sees the work.
    /// </para>
    /// </summary>
    Background = 550,

    /// <summary>The progress-tree node for this call. The single place nodes are created, which is
    /// what makes a script's children nest under it.</summary>
    Progress = 600,

    /// <summary>
    /// Shaping the result on the way out: cutting bulk down to a summary plus a handle so a
    /// five-megabyte answer does not destroy the context window. Declared here, not yet implemented.
    /// <para>
    /// <b>Outside <see cref="Fault"/></b> so that what the fault link manufactures from an exception
    /// is trimmed too — a giant stderr is still a result. <b>Outside <see cref="Accounting"/></b> so
    /// that the ledger sees the real size rather than the trimmed one. <b>Inside
    /// <see cref="Progress"/></b> so the node closes on the final result. <b>Inside
    /// <see cref="Background"/></b> so background delivery uses this trimming instead of inventing a
    /// size threshold of its own.
    /// </para>
    /// <para>
    /// The one stage open to plugins — and, being outside <see cref="Fault"/>, the one place where a
    /// thrown exception is converted by nobody: <see cref="Stages.ProgressNodeStage"/> assumes only
    /// cancellation reaches its <c>catch</c>. A plugin-contributed link is therefore wrapped in a
    /// guard at registration, so a fault in it is a log line and a pass-through, never a dead turn.
    /// </para>
    /// </summary>
    Post = 650,

    /// <summary>
    /// Turning an exception into a result. Its own stage, and deliberately <b>outside</b>
    /// <see cref="Accounting"/>: a call that threw did not produce a result to account for, and were
    /// this inside, the failure it manufactures would be recorded as if the tool had returned it —
    /// counting the same fault twice.
    /// </summary>
    Fault = 700,

    /// <summary>
    /// The call's deadline. Declared here, not yet implemented.
    /// <para>
    /// <b>Inside <see cref="Fault"/></b> — outside it, the link would intercept the cancellations
    /// that tear a turn down and mistake them for its own. <b>Outside
    /// <see cref="Accounting"/></b> — a call that never returned a result is nothing to count.
    /// <b>Inside <see cref="Background"/></b> — the same code then guards a synchronous call and a
    /// detached task alike.
    /// </para>
    /// <para>
    /// <b>The link converts its own expiry itself.</b> <see cref="Stages.FaultStage"/> deliberately
    /// rethrows <see cref="System.OperationCanceledException"/> — a cancelled call must not come back
    /// as an answer — so a deadline let out as an exception would fly straight through it and end the
    /// turn. The link tells its own token from an outer cancellation and returns a
    /// <see cref="ToolResult"/>. The progress node needs no teaching: it reads <c>IsError</c>.
    /// </para>
    /// </summary>
    Timeout = 750,

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
