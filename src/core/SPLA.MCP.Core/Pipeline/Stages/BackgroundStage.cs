using Microsoft.Extensions.Logging;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Permissions;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Lets a call detach from its turn: the model gets a task id back immediately, and the call keeps
/// running under <see cref="Task.Run(Func{Task})"/> until it produces a result nobody in this turn
/// is waiting on. See <c>docs/adr/ADR_20260824-2_core_background-tool-calls.md</c>.
/// <para>
/// <b>Inside <see cref="ToolPipelineStage.Ambient"/>, outside <see cref="ToolPipelineStage.Progress"/>.</b>
/// Inside Ambient because a detached call must inherit the ambient scopes — <c>AsyncLocal</c> is
/// copied at the moment <see cref="Task.Run(Func{Task})"/> creates its task, and this link sits
/// where <see cref="AmbientHostStage"/> has already published them. Outside Progress because the
/// task needs its own tree root, not a node under a turn that is about to close.
/// </para>
/// <para>
/// Runs synchronously — a plain call to <c>next</c> — unless <i>all</i> of: the tool declared
/// <see cref="ToolFunctionDefinition.SupportsBackground"/>, the call's arguments carry
/// <c>"background": true</c>, the current <see cref="IAgentSession"/> actually offers
/// <see cref="IAgentSession.Background"/>, and the call is not already running inside a detached
/// task (see <see cref="_insideDetachedRun"/> — depth is capped at one). Each of those degrades to
/// "run it now" rather than a refusal: a sub-agent's session leaves <c>Background</c> null on
/// purpose (see <see cref="IBackgroundTaskHost"/>), and the model gets its answer either way, just
/// not detached.
/// </para>
/// </summary>
public sealed class BackgroundStage : IToolMiddleware
{
    private readonly ILogger? _logger;

    public BackgroundStage(ILogger? logger = null) => _logger = logger;

    public ToolPipelineStage Stage => ToolPipelineStage.Background;

    /// <summary>
    /// True while the current async flow is already running inside a detached task. Set for the
    /// whole body of <see cref="RunDetachedAsync"/> and, because <c>AsyncLocal</c> flows into
    /// <see cref="Task.Run(Func{Task})"/> the same way the ambient scopes do, into anything that
    /// call itself spawns — a script's <c>ctx.Run</c>, a nested tool call, any depth.
    /// <para>
    /// This is plan pitfall 9, "фон в фоне": without it, a call already running detached could ask
    /// to detach again, and a task that launches tasks has no natural bound. One flag closes every
    /// depth at once rather than threading a counter through the call.
    /// </para>
    /// </summary>
    private static readonly AsyncLocal<bool> _insideDetachedRun = new();

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        var def = call.Tool!.GetDefinition().Function;
        if (!def.SupportsBackground) return await next(call, ct);

        bool requested;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(call.ArgumentsJson);
            requested = ToolJson.GetBoolean(doc.RootElement, "background", false);
        }
        catch (System.Text.Json.JsonException)
        {
            // Malformed arguments are not this link's problem to diagnose — run synchronously and
            // let the tool's own parsing (or the schema layer ahead of it) produce the real error.
            requested = false;
        }
        if (!requested) return await next(call, ct);

        if (_insideDetachedRun.Value)
        {
            _logger?.LogInformation(
                "Tool asked to background from inside an already-detached run — running in place. Tool={ToolName}",
                call.Name);
            return await next(call, ct);
        }

        var host = AgentSessionScope.Current?.Background;
        if (host is null)
        {
            _logger?.LogInformation(
                "Tool asked to background but this session cannot host a detached call — running in place. Tool={ToolName}",
                call.Name);
            return await next(call, ct);
        }

        var (record, refusal) = host.Tasks.TryStart(call.Name, call.ArgumentsJson);
        if (record is null)
            return ToolResult.Refuse(refusal!, "background task cap reached");

        // Everything captured here is what the detached Task.Run body needs and the ambient scopes
        // do NOT carry across on their own: AgentSessionScope/ToolHostScope flow into Task.Run
        // automatically (AsyncLocal is copied at task-creation time), which is exactly why this link
        // sits inside Ambient — but PermissionScope/ClarifyScope must NOT flow, and a plain
        // Task.Run body needs its own progress-tree scope rather than inheriting the turn's, which is
        // about to close.
        var tree = new ProgressTree();
        record.ProgressTreeId = host.Progress.Register(tree);

        _ = RunDetachedAsync(call, next, host, record, tree);

        return ToolResult.Text(
            $"Task {record.Id} started ({call.Name}). Result will arrive when it finishes — " +
            "the conversation continues in the meantime.");
    }

    /// <summary>
    /// The task's whole life outside the turn that launched it: runs the rest of the pipeline under
    /// its own progress tree and a cancellation token linked to the chat's lifetime, then records
    /// whatever came out — success, a returned failure, a thrown exception, or a cancellation — and
    /// delivers it through the chat's inbox exactly once.
    /// </summary>
    private async Task RunDetachedAsync(
        ToolCallInvocation call, ToolCallDelegate next, IBackgroundTaskHost host,
        BackgroundTaskRecord record, ProgressTree tree)
    {
        // Scoped to this async method's own call boundary — the CLR captures/restores
        // ExecutionContext around an async method, so this is never visible back in InvokeAsync's
        // synchronous caller, only to this run and whatever it calls. No Restore needed.
        _insideDetachedRun.Value = true;

        using var permissionScope = PermissionScope.Begin(RefuseAsk);
        using var clarifyScope = ClarifyScope.Begin(RefuseClarify);
        using var treeScope = ProgressScope.BeginTree(tree);

        ToolResult result;
        BackgroundTaskState state;
        try
        {
            result = await next(call, record.Cts.Token);
            state = result.IsError && record.Cts.IsCancellationRequested
                ? BackgroundTaskState.Cancelled
                : result.IsError ? BackgroundTaskState.Failed : BackgroundTaskState.Completed;
        }
        catch (OperationCanceledException)
        {
            result = ToolResult.Refuse(
                $"Background task {record.Id} ({call.Name}) was cancelled.", "cancelled");
            state = BackgroundTaskState.Cancelled;
        }
        catch (Exception ex)
        {
            // The pipeline's own FaultStage sits further in and normally does this conversion — but
            // it converts on the way back out through every link that is still on the stack, and
            // nothing is on the stack here once the turn that called this link has moved on. This is
            // the fallback for whatever a synchronous call would have had FaultStage catch.
            _logger?.LogError(ex, "Background task failed. TaskId={TaskId} Tool={ToolName}", record.Id, call.Name);
            result = ToolResult.Fail($"Error executing tool {call.Name}: {ex.Message}", ex.GetType().Name);
            state = BackgroundTaskState.Failed;
        }

        host.Tasks.Finish(record.Id, state, result);

        // Cancelled and failed deliver exactly the same way a success does — ADR §2: "молчание хуже
        // ошибки". A model that launched a task and never hears about it again cannot tell "still
        // running" from "vanished", and the second is a bug this delivery exists to make impossible.
        var elapsed = (record.FinishedAt ?? DateTimeOffset.UtcNow) - record.StartedAt;
        var verb = state switch
        {
            BackgroundTaskState.Completed => "finished",
            BackgroundTaskState.Failed => "failed",
            BackgroundTaskState.Cancelled => "cancelled",
            _ => "ended"
        };
        host.Inbox.Enqueue(new ChatMessage
        {
            Role = ChatRole.User,
            Content = $"[Background task {record.Id} — {call.Name} — {verb} in {FormatElapsed(elapsed)}]\n{result.TextContent}",
            IsEphemeral = false
        }, InboxItemKind.TaskResult);
    }

    /// <summary>
    /// Invariant, not current-culture: this text goes to the model, and a comma decimal separator
    /// (found live, running under a Russian OS locale — "0,8s") is exactly the kind of format a
    /// smaller model misreads as a typo or a different number entirely.
    /// </summary>
    private static string FormatElapsed(TimeSpan elapsed) => elapsed.TotalMinutes >= 1
        ? $"{(int)elapsed.TotalMinutes}m{elapsed.Seconds.ToString("D2", CultureInfo.InvariantCulture)}s"
        : $"{elapsed.TotalSeconds.ToString("F1", CultureInfo.InvariantCulture)}s";

    /// <summary>
    /// Stands in for a human for the rest of this call's life. A nested call inside a background
    /// task that needs a person's yes gets an automatic no — the ADR's rule that an answer must never
    /// widen what the call was already allowed: nobody who could say yes is left to ask.
    /// <para>
    /// <b>Known imprecision:</b> <see cref="Stages.PermissionStage"/> reports a <c>Deny</c> decision
    /// as <i>"denied by the user"</i> — accurate for the human case this scope handler was written to
    /// replace, not for this one. Fixing it means threading a reason through
    /// <see cref="PermissionDecision"/> or its caller, which touches the human path too; left as a
    /// known gap rather than done in passing here.
    /// </para>
    /// </summary>
    private static Task<PermissionDecision> RefuseAsk(ToolFunctionDefinition def, string args)
        => Task.FromResult(PermissionDecision.Deny);

    private static Task<string?> RefuseClarify(ClarifyRequest request)
        => Task.FromResult<string?>(null);
}
