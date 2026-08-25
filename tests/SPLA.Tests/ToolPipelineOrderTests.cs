using SPLA.Domain.Models;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Pipeline;
using SPLA.MCP.Core.Pipeline.Stages;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Guards the one property the pipeline's correctness rests on: that the order of its links is
/// decided by <see cref="ToolPipelineStage"/> and not by whoever wrote the registration list.
/// <para>
/// Until this existed, every load-bearing relation lived in an XML comment — which tells the next
/// author how it is, and does not stop them changing it. Each assertion below names the failure it
/// prevents, because a bare <c>&lt;</c> would be re-derivable but not arguable.
/// </para>
/// </summary>
public class ToolPipelineOrderTests
{
    // ---- the relations, as numbers -----------------------------------------------------------

    [Fact]
    public void Disclosure_comes_before_policy()
    {
        // Weighing the rights of a tool the caller was never shown is answering a question nobody
        // asked — and would leak the tool's existence through the shape of the refusal.
        Assert.True(ToolPipelineStage.Disclosure < ToolPipelineStage.Policy);
    }

    [Fact]
    public void Telemetry_is_inside_disclosure_and_outside_policy()
    {
        // Outside Policy so a refusal is still traced; inside Disclosure so an undisclosed tool
        // leaves no span at all — a span records a call that was considered.
        Assert.True(ToolPipelineStage.Disclosure < ToolPipelineStage.Telemetry);
        Assert.True(ToolPipelineStage.Telemetry < ToolPipelineStage.Policy);
    }

    [Fact]
    public void Progress_comes_after_policy()
    {
        // A node opened before the verdict would leave refused calls — and calls a human said no to
        // — sitting in the tree as if they had happened.
        Assert.True(ToolPipelineStage.Policy < ToolPipelineStage.Progress);
    }

    [Fact]
    public void Background_is_inside_ambient_and_outside_progress()
    {
        // Inside Ambient: a detached task inherits the ambient scopes only if they are already
        // published when its Task is created. Outside Progress: the task needs its own tree root,
        // not a node under a turn that is about to close.
        Assert.True(ToolPipelineStage.Ambient < ToolPipelineStage.Background);
        Assert.True(ToolPipelineStage.Background < ToolPipelineStage.Progress);
    }

    [Fact]
    public void Post_is_inside_progress_and_outside_fault_and_accounting()
    {
        // Inside Progress: the node closes on the final result. Outside Fault: what Fault makes out
        // of an exception is a result too, and a giant stderr needs trimming just as much. Outside
        // Accounting: the ledger must see the real size, not the trimmed one.
        Assert.True(ToolPipelineStage.Progress < ToolPipelineStage.Post);
        Assert.True(ToolPipelineStage.Post < ToolPipelineStage.Fault);
        Assert.True(ToolPipelineStage.Post < ToolPipelineStage.Accounting);
    }

    [Fact]
    public void Timeout_is_inside_fault_and_outside_accounting()
    {
        // Outside Fault the link would intercept the cancellations that tear a turn down; inside
        // Accounting a call that never returned would be recorded as one that did.
        Assert.True(ToolPipelineStage.Fault < ToolPipelineStage.Timeout);
        Assert.True(ToolPipelineStage.Timeout < ToolPipelineStage.Accounting);
    }

    [Fact]
    public void Trimming_and_the_deadline_both_apply_inside_a_backgrounded_call()
    {
        // The half a link belongs to is the third relation every new stage must declare. Both of
        // these belong to the work, not the launch: a background task's result needs the same
        // trimming a synchronous one does, and the same deadline guards both.
        Assert.True(ToolPipelineStage.Background < ToolPipelineStage.Post);
        Assert.True(ToolPipelineStage.Background < ToolPipelineStage.Timeout);
    }

    [Fact]
    public void Accounting_is_innermost()
    {
        // Everything it counts must be a call that actually ran.
        Assert.Equal(
            ToolPipelineStage.Accounting,
            Enum.GetValues<ToolPipelineStage>().Max());
    }

    // ---- the fold ----------------------------------------------------------------------------

    [Fact]
    public async Task Build_orders_by_stage_not_by_registration()
    {
        var trace = new List<string>();
        var pipeline = new ToolPipelineBlueprint()
            .Use(new TracingLink(ToolPipelineStage.Accounting, "inner", trace))
            .Use(new TracingLink(ToolPipelineStage.Resolution, "outer", trace))
            .Use(new TracingLink(ToolPipelineStage.Policy, "middle", trace))
            .Build((_, _) => Task.FromResult(ToolResult.Text("done")));

        await pipeline(new ToolCallInvocation(AgentMode.Agent, "t", "{}"), CancellationToken.None);

        // Registered inner-first; must still run outermost-first.
        Assert.Equal(new[] { "outer", "middle", "inner" }, trace);
    }

    [Fact]
    public async Task Links_on_the_same_stage_keep_registration_order()
    {
        // Not a nicety: PermissionStage and ZoneShadowStage both sit on Policy today, and the shadow
        // ledger is only meaningful for calls the verdict let through.
        var trace = new List<string>();
        var pipeline = new ToolPipelineBlueprint()
            .Use(new TracingLink(ToolPipelineStage.Policy, "first", trace))
            .Use(new TracingLink(ToolPipelineStage.Policy, "second", trace))
            .Build((_, _) => Task.FromResult(ToolResult.Text("done")));

        await pipeline(new ToolCallInvocation(AgentMode.Agent, "t", "{}"), CancellationToken.None);

        Assert.Equal(new[] { "first", "second" }, trace);
    }

    // ---- the chain the host actually builds ---------------------------------------------------

    [Fact]
    public void Host_composes_the_expected_chain_in_the_expected_order()
    {
        // The relations above are about the type; this is about the one arrangement that runs.
        var host = new McpHost(new PermissionManager());

        Assert.Equal(
            new[]
            {
                typeof(ToolResolutionStage),
                typeof(PluginAvailabilityStage),
                typeof(ToolSetDisclosureStage),
                typeof(TelemetryStage),
                typeof(PermissionStage),
                typeof(ZoneShadowStage),
                typeof(AmbientHostStage),
                typeof(BackgroundStage),
                typeof(ProgressNodeStage),
                typeof(FaultStage),
                typeof(AccountingStage)
            },
            host.Pipeline.Links.Select(l => l.GetType()).ToArray());
    }

    [Fact]
    public void Host_chain_is_sorted_by_stage()
    {
        var host = new McpHost(new PermissionManager());
        var stages = host.Pipeline.Links.Select(l => l.Stage).ToList();

        Assert.Equal(stages.OrderBy(s => s), stages);
    }

    private sealed class TracingLink : IToolMiddleware
    {
        private readonly string _label;
        private readonly List<string> _trace;

        public TracingLink(ToolPipelineStage stage, string label, List<string> trace)
        {
            Stage = stage;
            _label = label;
            _trace = trace;
        }

        public ToolPipelineStage Stage { get; }

        public Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
        {
            _trace.Add(_label);
            return next(call, ct);
        }
    }
}
