using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.MCP.Core.Pipeline;

/// <summary>
/// The declared composition of the tool pipeline — a list, not a running thing. Baked once, when the
/// host is built; a call assembles nothing, it only creates a <see cref="ToolCallInvocation"/>.
/// <para>
/// Links are host-owned for now, and there is deliberately no plugin-facing entry point: a place to
/// hang a stage that nobody hangs anything on is the speculative generality this pipeline was
/// extracted to avoid. When plugins do get to contribute, the shape to copy is
/// <c>LlmPipelineBlueprint.UseFromPlugin</c> — open the stages that shape a call, seal the ones that
/// account for it.
/// </para>
/// </summary>
public sealed class ToolPipelineBlueprint
{
    private readonly List<IToolMiddleware> _links = new();

    /// <summary>
    /// What was declared, in the order it will be folded. Exposed so that the composition can be
    /// asserted rather than trusted: without it a test can only check fakes it registered itself, and
    /// the one arrangement that actually matters — the chain the host builds — stays unverified. A
    /// debug view of "which links are standing" reads the same list.
    /// </summary>
    public IReadOnlyList<IToolMiddleware> Links => _links.OrderBy(l => l.Stage).ToList();

    public ToolPipelineBlueprint Use(IToolMiddleware link)
    {
        ArgumentNullException.ThrowIfNull(link);
        _links.Add(link);
        return this;
    }

    /// <summary>
    /// Folds the chain once, from the inside out, so the first link of the ordered list ends up
    /// outermost. Ordering is by <see cref="IToolMiddleware.Stage"/> and stable within a stage, so
    /// registration order still settles ties but can never violate the stage contract.
    /// </summary>
    public ToolCallDelegate Build(ToolCallDelegate execute)
    {
        ArgumentNullException.ThrowIfNull(execute);

        var pipeline = execute;

        foreach (var link in _links.OrderBy(l => l.Stage).Reverse())
        {
            var next = pipeline;
            var current = link;
            pipeline = (call, ct) => current.InvokeAsync(call, next, ct);
        }

        return pipeline;
    }
}
