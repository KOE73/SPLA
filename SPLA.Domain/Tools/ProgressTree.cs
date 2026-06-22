using SPLA.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SPLA.Domain.Tools;

/// <summary>
/// The live progress for one agent turn: a thread-safe forest of <see cref="ProgressNode"/>s plus a
/// single change notification. The orchestrator creates one tree per turn; <see cref="ProgressScope"/>
/// populates it as tools start, report, and finish — including tools invoked in parallel inside a
/// script, which land as child nodes. Consumers (status bar, CLI, Avalonia, Blazor) subscribe to
/// <see cref="NodeChanged"/> and read <see cref="Nodes"/>; the tree is deliberately UI-agnostic — a
/// plain .NET event, no Rx, no framework dependency.
/// </summary>
public sealed class ProgressTree
{
    private readonly object _gate = new();
    private readonly List<ProgressNode> _nodes = new();
    private int _seq;

    /// <summary>
    /// Raised after a node is added, updated, or completed, carrying the affected node. Fire-and-forget
    /// like progress itself: handlers must not block and must marshal to their own UI thread. May fire
    /// from background threads (parallel tool work).
    /// </summary>
    public event Action<ProgressNode>? NodeChanged;

    /// <summary>A snapshot of every node created so far this turn, in creation order.</summary>
    public IReadOnlyList<ProgressNode> Nodes
    {
        get { lock (_gate) return _nodes.ToArray(); }
    }

    internal ProgressNode AddNode(string label, string? parentId)
    {
        var id = "n" + Interlocked.Increment(ref _seq).ToString();
        var node = new ProgressNode(id, parentId, label);
        lock (_gate) _nodes.Add(node);
        NodeChanged?.Invoke(node);
        return node;
    }

    internal void UpdateNode(ProgressNode node, ToolProgress progress)
    {
        node.Latest = progress;
        NodeChanged?.Invoke(node);
    }

    internal void CompleteNode(ProgressNode node, bool success)
    {
        node.State = success ? ProgressState.Completed : ProgressState.Failed;
        node.FinishedAt = DateTimeOffset.UtcNow;
        NodeChanged?.Invoke(node);
    }
}
