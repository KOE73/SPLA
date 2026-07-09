using SPLA.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SPLA.Domain.Tools;

/// <summary>
/// The ambient channel a tool uses to report progress, and the mechanism that assembles a
/// <see cref="ProgressTree"/> without touching any tool signature. Two layers of ambient context,
/// both <see cref="AsyncLocal{T}"/> (the same approach used for telemetry):
/// <list type="bullet">
///   <item>the orchestrator opens a <see cref="BeginTree"/> scope once per turn;</item>
///   <item>the host opens a <see cref="BeginNode"/> scope around every tool call — at any depth.</item>
/// </list>
/// Because <see cref="AsyncLocal{T}"/> forks across <c>Task</c> boundaries, tools invoked in parallel
/// (e.g. from a script) each see their own current node and attach as children of the node that
/// spawned them — the tree builds itself. A tool, however deep, calls <see cref="Report(ToolProgress)"/>
/// and the tick reaches its node. When no tree is open, <c>BeginNode</c>/<c>Report</c> are no-ops.
/// </summary>
public static class ProgressScope
{
    private static readonly AsyncLocal<ProgressTree?> _tree = new();
    private static readonly AsyncLocal<ProgressNode?> _node = new();

    /// <summary>The tree active on the current async flow, if any.</summary>
    public static ProgressTree? CurrentTree => _tree.Value;

    /// <summary>
    /// Routes all node activity on the current async flow into <paramref name="tree"/> until the
    /// returned handle is disposed. Resets the current node to "root" for the duration. Nesting
    /// restores the previous tree and node on dispose.
    /// </summary>
    public static IDisposable BeginTree(ProgressTree tree)
    {
        var prevTree = _tree.Value;
        var prevNode = _node.Value;
        _tree.Value = tree;
        _node.Value = null;
        return new Restore(() => { _tree.Value = prevTree; _node.Value = prevNode; });
    }

    /// <summary>
    /// Opens a node for a tool call under the current node (or as a root when none is open) and makes
    /// it the current node until the returned handle is disposed. On dispose the node is marked
    /// completed, or failed if <see cref="INodeScope.Fail"/> was called first. A no-op handle is
    /// returned when no tree is active.
    /// </summary>
    public static INodeScope BeginNode(string label)
    {
        var tree = _tree.Value;
        if (tree is null) return NoopNodeScope.Instance;

        var node = tree.AddNode(label, _node.Value?.Id);
        var prevNode = _node.Value;
        _node.Value = node;
        return new NodeScope(tree, node, prevNode);
    }

    /// <summary>Emits a progress tick to the current node, if any. Safe to call with no scope active.</summary>
    public static void Report(ToolProgress progress)
    {
        var tree = _tree.Value;
        var node = _node.Value;
        if (tree is not null && node is not null) tree.UpdateNode(node, progress);
    }

    /// <summary>Convenience for the common "N of total" case.</summary>
    public static void Report(long current, long total, string? message = null,
        IReadOnlyList<ToolProgressDetail>? details = null)
        => Report(new ToolProgress { Current = current, Total = total, Message = message, Details = details });

    /// <summary>Handle for an open node; dispose to complete, <see cref="Fail"/> to mark it failed.</summary>
    public interface INodeScope : IDisposable
    {
        /// <summary>Marks the node as failed; the subsequent dispose records it as such.</summary>
        void Fail();
    }

    private sealed class NodeScope : INodeScope
    {
        private readonly ProgressTree _tree;
        private readonly ProgressNode _target;
        private readonly ProgressNode? _previous;
        private bool _failed;
        private bool _disposed;

        public NodeScope(ProgressTree tree, ProgressNode target, ProgressNode? previous)
        {
            _tree = tree;
            _target = target;
            _previous = previous;
        }

        public void Fail() => _failed = true;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _node.Value = _previous; // restore current node on this async flow
            _tree.CompleteNode(_target, !_failed);
        }
    }

    private sealed class NoopNodeScope : INodeScope
    {
        public static readonly NoopNodeScope Instance = new();
        public void Fail() { }
        public void Dispose() { }
    }

    private sealed class Restore : IDisposable
    {
        private readonly Action _undo;
        public Restore(Action undo) => _undo = undo;
        public void Dispose() => _undo();
    }
}
