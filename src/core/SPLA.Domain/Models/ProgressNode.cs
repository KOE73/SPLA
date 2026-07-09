using System;

namespace SPLA.Domain.Models;

/// <summary>Lifecycle state of a <see cref="ProgressNode"/>.</summary>
public enum ProgressState
{
    /// <summary>The work this node represents is in flight.</summary>
    Running,

    /// <summary>The work finished successfully.</summary>
    Completed,

    /// <summary>The work threw or was cancelled.</summary>
    Failed
}

/// <summary>
/// One node in a <see cref="SPLA.Domain.Tools.ProgressTree"/>: a single tool invocation. Nodes form
/// a parent/child forest via <see cref="ParentId"/> — a top-level tool call is a root
/// (<c>ParentId == null</c>), and any tool a script (or another tool) invokes underneath it becomes
/// a child. This lets the UI render both the overall picture and the parallel detail without the
/// caller threading any context through tool signatures. Mutated only by the owning
/// <see cref="SPLA.Domain.Tools.ProgressTree"/>, which raises its change event after each write.
/// </summary>
public sealed class ProgressNode
{
    /// <summary>Stable id, unique within the owning tree.</summary>
    public string Id { get; }

    /// <summary>Id of the enclosing node, or <c>null</c> for a top-level (root) tool call.</summary>
    public string? ParentId { get; }

    /// <summary>The model-facing tool name this node represents.</summary>
    public string Label { get; }

    /// <summary>Current lifecycle state. Owned by the tree.</summary>
    public ProgressState State { get; internal set; } = ProgressState.Running;

    /// <summary>The most recent progress tick reported by the tool, if any.</summary>
    public ToolProgress? Latest { get; internal set; }

    /// <summary>When the node was created (UTC).</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>When the node reached a terminal state (UTC), or <c>null</c> while running.</summary>
    public DateTimeOffset? FinishedAt { get; internal set; }

    internal ProgressNode(string id, string? parentId, string label)
    {
        Id = id;
        ParentId = parentId;
        Label = label;
        StartedAt = DateTimeOffset.UtcNow;
    }
}
