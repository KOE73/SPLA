using SPLA.Domain.Agent;
using SPLA.Domain.Identity;
using SPLA.Domain.Secrets;

namespace SPLA.Domain.Tools;

/// <summary>
/// Everything a single tool call needs to know about who is making it. The third lifetime, next to
/// the two that already existed: the project (<c>AgentRuntime</c> — host, permissions, tool sets,
/// plugins) and the chat (<c>AgentSession</c> — memory, blobs, marks, skills).
/// <para>
/// Until now this level was implicit: a call carried no statement of whose it was, and the answer
/// was reconstructed at the bottom of the stack by reading ambient state. That works while every
/// caller is a chat on the same machine. It stops working the moment a caller is not a chat — a
/// second head over a wire, a queued job, anything that has no ambient state to read — and it is
/// awkward long before that, because "who is calling" is not something a function should have to
/// deduce.
/// </para>
/// <para>
/// <b>The context is the source; the ambient scopes stay the access path.</b> A caller states the
/// context, the host opens the familiar <see cref="AgentSessionScope"/> and
/// <see cref="SecretCallerScope"/> from it, and tools go on reading <c>Current</c> exactly as
/// before. That is deliberate: it makes the boundary explicit without touching a hundred-odd tool
/// implementations, and it leaves the ambient mechanism — which is what lets ten chats run
/// concurrently without colliding — exactly as it is.
/// </para>
/// </summary>
public sealed record ToolCallContext
{
    /// <summary>The chat state this call acts on. Null only where there is genuinely no chat: a
    /// unit test, or a CLI entry point that never opened a session.</summary>
    public IAgentSession? Session { get; init; }

    /// <summary>Whose rights apply — checked when a plugin resolves a credential. Null means "take
    /// whatever the ambient scope says", which locally is the single local user.</summary>
    public IIdentity? Identity { get; init; }

    /// <summary>
    /// Reads the context out of the ambient scopes: what an existing caller means when it passes
    /// nothing. The compatibility path, and the one that keeps this change behaviour-preserving.
    /// </summary>
    public static ToolCallContext FromAmbient() => new()
    {
        Session = AgentSessionScope.Current,
        Identity = SecretCallerScope.Identity
    };

    /// <summary>
    /// Routes the ambient scopes to this context for the duration of the returned handle. Opening a
    /// scope over the value that is already current is a no-op in effect — which is exactly why the
    /// compatibility path costs nothing.
    /// </summary>
    public IDisposable Enter()
    {
        var handles = new List<IDisposable>(2);
        if (Session is not null) handles.Add(AgentSessionScope.Begin(Session));
        if (Identity is not null) handles.Add(SecretCallerScope.Begin(Identity));
        return new Handles(handles);
    }

    /// <summary>Disposes in reverse order, so nesting unwinds the way it was built.</summary>
    private sealed class Handles(List<IDisposable> items) : IDisposable
    {
        private bool _done;

        public void Dispose()
        {
            if (_done) return;
            _done = true;
            for (var i = items.Count - 1; i >= 0; i--) items[i].Dispose();
        }
    }
}
