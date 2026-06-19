using SPLA.Domain.Models;
using System;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Permissions;

/// <summary>
/// Ambient channel for interactive tool-permission prompts. A chat opens a scope with
/// <see cref="Begin"/> and supplies a handler that surfaces the confirmation to that chat's own UI
/// (and persists the decision); <see cref="McpHost"/> calls <see cref="RequestAsync"/> when a tool
/// needs confirmation. Because it flows on the current async flow via <see cref="AsyncLocal{T}"/>,
/// concurrent background chats each get their own prompt routed to their own view — no shared modal.
/// When no scope is open, <see cref="RequestAsync"/> returns <c>null</c>.
/// </summary>
public static class PermissionScope
{
    private static readonly AsyncLocal<Func<ToolFunctionDefinition, string, Task<PermissionDecision>>?> _handler = new();

    /// <summary>
    /// Routes permission requests on the current async flow to <paramref name="handler"/> until the
    /// returned handle is disposed. Nesting restores the previous handler.
    /// </summary>
    public static IDisposable Begin(Func<ToolFunctionDefinition, string, Task<PermissionDecision>> handler)
    {
        var previous = _handler.Value;
        _handler.Value = handler;
        return new Restore(previous);
    }

    /// <summary>
    /// Emits a permission request and awaits the user's decision. Returns <c>null</c> if no scope is
    /// active (no UI to ask) — the caller decides how to treat an unhandled request.
    /// </summary>
    public static Task<PermissionDecision>? RequestAsync(ToolFunctionDefinition def, string args)
        => _handler.Value?.Invoke(def, args);

    private sealed class Restore : IDisposable
    {
        private readonly Func<ToolFunctionDefinition, string, Task<PermissionDecision>>? _previous;
        public Restore(Func<ToolFunctionDefinition, string, Task<PermissionDecision>>? previous) => _previous = previous;
        public void Dispose() => _handler.Value = _previous;
    }
}
