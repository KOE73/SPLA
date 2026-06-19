using SPLA.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SPLA.Domain.Tools;

/// <summary>
/// The ambient channel a tool uses to report progress. A consumer (the orchestrator) opens a scope
/// around a tool call with <see cref="Begin"/>; the tool, however deep in the call stack and across
/// async/parallel boundaries, calls <see cref="Report(ToolProgress)"/> and the tick reaches the open
/// sink. Implemented with <see cref="AsyncLocal{T}"/> — the same ambient-context approach used for
/// telemetry — so tools opt in with one line and need no signature, parameter, or interface change.
/// When no scope is open, <c>Report</c> is a no-op.
/// </summary>
public static class ToolProgressScope
{
    private static readonly AsyncLocal<Action<ToolProgress>?> _sink = new();

    /// <summary>
    /// Routes progress emitted on the current async flow to <paramref name="sink"/> until the
    /// returned handle is disposed. Nesting restores the previous sink on dispose.
    /// </summary>
    public static IDisposable Begin(Action<ToolProgress> sink)
    {
        var previous = _sink.Value;
        _sink.Value = sink;
        return new Restore(previous);
    }

    /// <summary>Emits a progress tick to the open sink, if any. Safe to call when no scope is active.</summary>
    public static void Report(ToolProgress progress) => _sink.Value?.Invoke(progress);

    /// <summary>Convenience for the common "N of total" case.</summary>
    public static void Report(long current, long total, string? message = null,
        IReadOnlyList<ToolProgressDetail>? details = null)
        => Report(new ToolProgress { Current = current, Total = total, Message = message, Details = details });

    private sealed class Restore : IDisposable
    {
        private readonly Action<ToolProgress>? _previous;
        public Restore(Action<ToolProgress>? previous) => _previous = previous;
        public void Dispose() => _sink.Value = _previous;
    }
}
