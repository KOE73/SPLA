using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.MCP.Core.Composition;

/// <summary>
/// What a contributor is handed when the surface is assembled. Deliberately small: everything that
/// varies per chat (the active skill, this chat's working memory) is resolved by the contributor
/// itself through the ambient <c>AgentSessionScope</c>, exactly as the skill tools do — a contributor
/// is a process-wide object, a chat is not, and passing chat state through here would only make that
/// asymmetry look solved.
/// <para>
/// <paramref name="ModeOverride"/> is the one deliberate exception: a chat (or role) may run a mode
/// narrower than the project default, and the mode preamble must say so. It is threaded explicitly
/// rather than through the ambient session because it has to be correct for a caller with no chat at
/// all (<c>AgentRuntime.ComposeContext()</c>, the CLI's own composition) just as much as for one —
/// null there simply means "the project's own default", which is what <see cref="Mode"/> falls back
/// to.
/// </para>
/// </summary>
public sealed record AgentContributionContext(ResolvedSettings Settings, string WorkingDirectory, AgentMode? ModeOverride = null)
{
    /// <summary>The mode this composition actually runs under — the caller's override when it gave
    /// one, otherwise the project's own default.</summary>
    public AgentMode Mode => ModeOverride ?? Settings.Mode;
}

/// <summary>
/// One source of the agent's assembled surface. The mode preamble, the built-in capabilities, the
/// instruction files, the skills, each plugin's prompt — today all of them, and tomorrow anything
/// else — reach the model through this one method instead of a hard-coded call inside the prompt
/// builder.
///
/// <para><b>The mechanism is one; the kinds of contribution stay separate types.</b> That constraint
/// matters more than the interface: "adds 800 tokens of text" and "runs PowerShell" must not become
/// the same thing, because trust, permissions and context budget are all decided on that difference.
/// <see cref="AgentContribution"/> therefore carries typed lists, and the right to a given kind is
/// checked on the type — an untrusted source may contribute a <see cref="ContextItem"/> and nothing
/// else.</para>
///
/// <para>Synchronous on purpose: the surface is reassembled on every iteration of the agent loop, and
/// no contributor today does I/O beyond reading an instruction file. A contributor that genuinely
/// needs to await (git state, a remote index) is the signal to add an async variant — not a reason
/// to make every call site await now.</para>
/// </summary>
public interface IAgentContributor
{
    /// <summary>Stable id, unique within one composer. Shown in the manifest as the answer to
    /// "who put this here": <c>mode</c>, <c>core</c>, <c>skills</c>, <c>plugins</c>.</summary>
    string Id { get; }

    /// <summary>Everything this contributor adds right now. Must not throw — the composer catches
    /// and records it, but a contributor that fails silently is one that vanishes from the prompt
    /// with no trace.</summary>
    AgentContribution Contribute(AgentContributionContext context);
}
