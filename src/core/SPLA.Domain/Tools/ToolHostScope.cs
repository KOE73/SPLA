using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using System;
using System.Threading;

namespace SPLA.Domain.Tools;

/// <summary>
/// Makes the tool host and the current agent mode ambiently available to a running tool, so a tool
/// can invoke other tools by name without being constructed with a host reference (plugins are
/// initialized with settings only). Same <see cref="AsyncLocal{T}"/> pattern as
/// <see cref="ProgressScope"/> and <see cref="AgentSessionScope"/>. The host opens a scope around
/// every tool call; nested calls (e.g. a script's <c>ctx.Run</c>) see the same host and mode, so
/// permissions and progress continue to apply. <see cref="Current"/> is <c>null</c> when no scope
/// is open (e.g. a tool called directly in a unit test).
/// </summary>
public static class ToolHostScope
{
    private static readonly AsyncLocal<State?> _current = new();

    /// <summary>The host and mode active on the current async flow, or <c>null</c>.</summary>
    public static State? Current => _current.Value;

    /// <summary>Routes <see cref="Current"/> to (<paramref name="host"/>, <paramref name="mode"/>) until disposed.</summary>
    public static IDisposable Begin(IToolHost host, AgentMode mode)
    {
        var previous = _current.Value;
        _current.Value = new State(host, mode);
        return new Restore(previous);
    }

    /// <summary>The ambient host plus the mode tool calls should run under.</summary>
    public sealed record State(IToolHost Host, AgentMode Mode);

    private sealed class Restore : IDisposable
    {
        private readonly State? _previous;
        public Restore(State? previous) => _previous = previous;
        public void Dispose() => _current.Value = _previous;
    }
}
