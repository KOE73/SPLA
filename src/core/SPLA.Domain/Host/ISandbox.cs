namespace SPLA.Domain.Host;

/// <summary>
/// The host boundary: everything an agent can use to touch the real system, gathered in one place.
/// Not the architectural top (that is the agent context) but the <em>risk centre</em> — memory,
/// secrets and the LLM are safe by nature and live outside it.
/// <para>
/// The enforcement mechanism differs per member: <see cref="Workspace"/> is bounded in code via
/// <see cref="Gate"/>, while <see cref="Shell"/> (arbitrary code) needs OS-level isolation in
/// untrusted scenarios — or is simply absent (<c>null</c>).
/// </para>
/// </summary>
public interface ISandbox
{
    IWorkspace Workspace { get; }

    /// <summary><c>null</c> when execution is disabled in this scenario.</summary>
    IShell? Shell { get; }

    ICapabilityGate Gate { get; }

    /// <summary>
    /// A sandbox for one chat: shares what belongs to the <i>project</i> — the workspace boundary and
    /// the gate, which must read the same for everybody — and gives the chat its own copy of whatever
    /// holds live state that has to die with it.
    /// <para>
    /// Today that is exactly <see cref="Shell"/>: <c>LocalShell</c> keeps its interactive sessions in
    /// the instance, so while every chat shared one, "kill this chat's shell sessions" could not be
    /// said at all, and the cap on live sessions was shared out between chats that knew nothing of
    /// each other.
    /// </para>
    /// <para>
    /// The default is to share <c>this</c>, which is correct for any sandbox that owns nothing
    /// per-chat (a shell-less one, an in-memory one in tests). An implementation that <i>does</i>
    /// hold live state must override, and the caller is expected to dispose what it gets back.
    /// </para>
    /// </summary>
    ISandbox ForChat() => this;
}
