using SPLA.Domain.Models;
using System;
using System.Threading.Tasks;

namespace SPLA.Domain.Tools;

/// <summary>
/// Ambient channel for structured clarification requests. A tool calls
/// <see cref="AskAsync"/> and awaits the user's answer; a consumer (the orchestrator or host)
/// opens a scope with <see cref="Begin"/> and supplies a handler that surfaces the question to
/// the UI or CLI and returns the chosen option.
/// <para>
/// Same <see cref="AsyncLocal{T}"/> approach as <see cref="ProgressScope"/> — the tool
/// needs no signature change and no direct dependency on the host.
/// When no scope is open, <see cref="AskAsync"/> returns <c>null</c> immediately.
/// </para>
/// </summary>
public static class ClarifyScope
{
    private static readonly AsyncLocal<Func<ClarifyRequest, Task<string?>>?> _handler = new();

    /// <summary>
    /// Routes clarification requests on the current async flow to <paramref name="handler"/>
    /// until the returned handle is disposed. Nesting restores the previous handler.
    /// </summary>
    public static IDisposable Begin(Func<ClarifyRequest, Task<string?>> handler)
    {
        var previous = _handler.Value;
        _handler.Value = handler;
        return new Restore(previous);
    }

    /// <summary>
    /// Emits a clarification request and awaits the user's choice.
    /// Returns <c>null</c> if no scope is active (autonomous/headless mode — skip clarification).
    /// </summary>
    public static Task<string?> AskAsync(ClarifyRequest request)
        => _handler.Value?.Invoke(request) ?? Task.FromResult<string?>(null);

    private sealed class Restore : IDisposable
    {
        private readonly Func<ClarifyRequest, Task<string?>>? _previous;
        public Restore(Func<ClarifyRequest, Task<string?>>? previous) => _previous = previous;
        public void Dispose() => _handler.Value = _previous;
    }
}
