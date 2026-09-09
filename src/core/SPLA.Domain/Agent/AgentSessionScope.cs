using SPLA.Domain.Host;
using SPLA.Domain.Tools;

namespace SPLA.Domain.Agent;

/// <summary>
/// What a chat offers a call that wants to detach from its turn — see
/// <c>docs/adr/ADR_20260824-2_core_background-tool-calls.md</c>.
/// <para>
/// A capability, not a given: only <c>ChatRuntime</c> implements this today. A spawned sub-agent's
/// <see cref="AgentSession"/> leaves <see cref="IAgentSession.Background"/> null on purpose — its
/// run has no chat to deliver a detached result to and no boundary between calls, so a call asking
/// to background there degrades to running in place rather than launching into a void nothing reads.
/// See ADR §4 open question 1 / plan pitfall 12: "фон — способность хоста".
/// </para>
/// </summary>
public interface IBackgroundTaskHost
{
    BackgroundTaskRegistry Tasks { get; }
    ProgressHub Progress { get; }
    ChatInbox Inbox { get; }
}

/// <summary>How a call into <see cref="ICorrespondenceHost.Correspond"/> ended — the tool-facing
/// shape <c>agent_correspond</c> (PLAN_20260902 wave 5) maps onto a <see cref="Models.ToolResult"/>.
/// Finer than a bool for the same reason <c>ChatRuntime.ReplyOutcome</c> is: a bad argument, an
/// unknown role, a denied edge and a vanished correspondent are different news to whoever reads the
/// logs, even though a model sees the same "did not work" either way.</summary>
public enum CorrespondOutcome
{
    /// <summary>The correspondence exists (found or freshly opened) and the reply was queued.</summary>
    Delivered,

    /// <summary>A required argument was missing or empty — role or text (<c>purpose</c> is optional
    /// since PLAN_20260906 wave 0).</summary>
    InvalidArgument,

    /// <summary>The named role is not declared in the project manifest. Roles do not self-assign
    /// (<c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.1) — opening a correspondence with an
    /// undeclared role would let a model invent one exactly the way spawning one already cannot.</summary>
    UnknownRole,

    /// <summary>This chat's own capability gate refused the edge (<see cref="Host.ICapabilityGate.CanCorrespond"/>).</summary>
    Denied,

    /// <summary>The correspondent's chat could not be reached (archived/deleted/unreachable).</summary>
    CorrespondentGone
}

/// <summary>A delivery receipt, never the correspondent's answer — see
/// <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.3 and plan trap 10. The whole reason this is a
/// distinct type from a plain string: a receipt that merely happens to read like prose is exactly what
/// a model would start treating as the correspondent's own reply, and stop waiting for the real one.</summary>
public readonly record struct CorrespondResult(CorrespondOutcome Outcome, string Message)
{
    public bool Delivered => Outcome == CorrespondOutcome.Delivered;
}

/// <summary>How a call into <see cref="ICorrespondenceHost.Introduce"/> ended
/// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.4). Its own enum rather than three more values
/// on <see cref="CorrespondOutcome"/>: every refusal here is about a party the caller is <i>not</i>,
/// which is a question <c>agent_correspond</c> cannot even ask, and folding them together would leave
/// both tools' result mapping carrying outcomes that can never reach it.</summary>
public enum IntroduceOutcome
{
    /// <summary>Both halves of the edge are open and the introduction reached the first party.</summary>
    Introduced,

    /// <summary>A required argument was missing or empty — one of the two names, or the text.</summary>
    InvalidArgument,

    /// <summary>One of the names is nobody's public name. An introduction takes two chats that already
    /// exist: unlike <c>agent_correspond</c>, naming a role here would have to mint a correspondent, and
    /// a chat conjured to be introduced to somebody is a chat the introducer should have asked for
    /// himself.</summary>
    UnknownAddressee,

    /// <summary>Both names resolved to the same chat. Introducing somebody to himself has no edge to
    /// open at all.</summary>
    SameChat,

    /// <summary>The introducer named himself as one of the two. That is not an introduction but an
    /// ordinary correspondence, and it has its own tool — see <c>agent_correspond</c>. Refused rather
    /// than quietly re-routed, because the two differ in who ends up on the edge, which is the entire
    /// subject of §2.4.</summary>
    IntroducerIsParty,

    /// <summary>A named chat exists but cannot be reached — archived, or gone between the lookup and
    /// the open.</summary>
    AddresseeGone,

    /// <summary>These two already correspond. Not an error the caller made, and nothing is opened a
    /// second time (the address they hold is the one they would get) — but the introduction is refused
    /// rather than delivered onto an edge somebody else built, so that a message written by a third
    /// party never appears inside a conversation he was never part of.</summary>
    AlreadyLinked,

    /// <summary>A capability gate refused the edge (<see cref="Host.ICapabilityGate.CanCorrespond"/>) —
    /// the introducer's own, or the party asked to carry the introduction.</summary>
    Denied
}

/// <summary>A receipt for an introduction, never a conversation — the same distinction
/// <see cref="CorrespondResult"/> exists to make (ADR_20260827-2 §2.3, plan trap 10), one step further
/// removed: the introducer is not even on the edge whose opening this reports.</summary>
public readonly record struct IntroduceResult(IntroduceOutcome Outcome, string Message)
{
    public bool Introduced => Outcome == IntroduceOutcome.Introduced;
}

/// <summary>
/// What a chat offers <c>agent_correspond</c> (PLAN_20260902 wave 5): the capability to open — or
/// reuse — a correspondence addressed by (role, instance number) and queue the first/next reply
/// across it.
/// <para>
/// A capability, not a given, the same way <see cref="IBackgroundTaskHost"/> is: only
/// <c>SPLA.Runtime.ChatRuntime</c> implements it. A spawned sub-agent's session leaves this null —
/// it has no chat identity of its own to correspond as, and no directory to find a correspondent's
/// chat through (<c>ChatRuntime</c>'s own <c>_registry</c> is null for the same class of caller).
/// </para>
/// </summary>
public interface ICorrespondenceHost
{
    /// <summary>
    /// Finds (by an address this chat already holds) or opens the correspondent's chat, and delivers
    /// <paramref name="text"/> across it. <paramref name="purpose"/> is free explanatory text, never
    /// mandatory and never part of the address (PLAN_20260906 wave 0 §2.1/2.3: the system-issued
    /// instance number is the address now, not the topic).
    /// <para><paramref name="role"/> names either a declared role — a correspondent of that kind,
    /// whose chat is created on demand — or an existing chat's public name, <c>architect_2</c>,
    /// which reaches that chat and creates nothing
    /// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.3). The parameter keeps its old name
    /// because the old meaning is still the common one, and the tool's own schema is where the second
    /// is explained to a model.</para>
    /// <para>Without <paramref name="another"/>, a call for a role this chat already corresponds with
    /// reuses that correspondence; with it, an additional correspondent of that role is opened. It has
    /// no effect when a public name is given: that names one chat, and this chat has at most one
    /// address to it.</para>
    /// </summary>
    CorrespondResult Correspond(string role, string purpose, string text, bool another);

    /// <summary>
    /// Puts two <i>other</i> chats in touch, without joining them
    /// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.4). Mechanically the same two
    /// <c>OpenCorrespondence</c> calls that <see cref="Correspond"/> already makes for both ends of an
    /// edge — the difference is only that the caller is neither end.
    /// <para>Both parties are named by their <b>public names</b> (<c>architect_2</c>), never by a role:
    /// an introduction connects two chats that exist. The chat identifier travels through the runtime
    /// and appears in no message — each of the two simply finds a new <c>reply_&lt;role&gt;_&lt;n&gt;</c>
    /// in its tool list (§2.4: "идентификатор идёт через рантайм, а не через текст").</para>
    /// <para><paramref name="first"/> becomes the head of the edge, deterministically, and is the one
    /// <paramref name="text"/> is delivered to — an opened correspondence by itself touches nobody's
    /// mailbox, so an introduction that delivered nothing would leave both parties standing still
    /// (§2.5, and ADR_20260825's "ход рождается из ящика"). The second party needs no message of its
    /// own: the very next thing it receives is the first party's actual reply.</para>
    /// </summary>
    IntroduceResult Introduce(string first, string second, string purpose, string text);
}

/// <summary>
/// How much room the model has left, in tokens — the window it was given and what the last request
/// actually occupied.
/// <para>
/// Both figures are <b>measured, not estimated</b>: the window comes from the provider (or the
/// connection's explicit setting) and the occupancy is the <c>prompt_tokens</c> the provider counted
/// for the previous call. That is the whole reason this type is worth having — the question "will
/// this fit" has an honest answer only where those two numbers are known, and everywhere else the
/// answer must be "I don't know" rather than a guess dressed up as one.
/// </para>
/// </summary>
public readonly record struct ContextBudget(int WindowTokens, int UsedTokens)
{
    /// <summary>Room left before the endpoint refuses the request. Never negative — a request that
    /// already exceeded the window reports zero, which is the same news.</summary>
    public int RemainingTokens => Math.Max(0, WindowTokens - UsedTokens);
}

/// <summary>
/// What a chat knows about its own context occupancy. A capability, not a given — the same shape as
/// <see cref="IBackgroundTaskHost"/>: only a chat with a model behind it and at least one measured
/// turn can answer, and a spawned run, a CLI entry point or a unit test simply cannot.
/// <para>
/// It exists so that the decision "does this result fit" is taken where the numbers are, rather than
/// by each tool guessing. A tool knows how many characters it produced and nothing about the model
/// reading them; this knows the model and nothing about what any tool is doing. Neither can decide
/// alone, which is why the decision belongs to the pipeline stage that can see both.
/// </para>
/// </summary>
public interface IContextBudgetHost
{
    /// <summary>The current budget, or null when it is genuinely unknown — no turn has reported
    /// usage yet, or the provider never said how large the window is. Null is an answer and must
    /// stay one: a caller that invents a default here turns "I don't know" into a number somebody
    /// downstream will trust.</summary>
    ContextBudget? Budget { get; }
}

/// <summary>
/// The per-chat agent state that tools resolve at execution time: working memory, the
/// checkpoint/mark manager, and the active-skill session. Each chat owns its own instance;
/// nothing here is shared between chats.
/// </summary>
public interface IAgentSession
{
    /// <summary>This chat's session-scoped working memory (persisted with the chat).</summary>
    IKeyValueStore SessionKv { get; }

    /// <summary>This chat's transient data-channel store for bulk tool output (not persisted).</summary>
    IBlobStore Blobs { get; }

    /// <summary>This chat's checkpoint/mark manager (bound to this chat's conversation).</summary>
    MarkManager Checkpoint { get; }

    /// <summary>This chat's single active-skill session.</summary>
    ISkillSession Skills { get; }

    /// <summary>Tool sets raised in this chat. The level that permits raising them at all is a
    /// project setting; this is what is armed here and now.</summary>
    IToolSetSession ToolSets { get; }

    /// <summary>The host boundary (files, shell, capability gate) this chat's tools act through.
    /// See <see cref="ISandbox"/>. Local chats share a passthrough sandbox; server chats get a
    /// scoped, sandboxed one — tools never know the difference.</summary>
    ISandbox Sandbox { get; }

    /// <summary>Whether this chat has taken in anything from a source nobody named. One bit, raised
    /// by arrivals from the open web and never lowered by anything automatic — see
    /// <see cref="Security.ChatDoubt"/>.</summary>
    Security.ChatDoubt Doubt { get; }

    /// <summary>Null when this session cannot host a detached call — see <see cref="IBackgroundTaskHost"/>.</summary>
    IBackgroundTaskHost? Background { get; }

    /// <summary>Null when this session cannot correspond — see <see cref="ICorrespondenceHost"/>.</summary>
    ICorrespondenceHost? Correspondence { get; }

    /// <summary>Null when this session cannot say how full its context is — see
    /// <see cref="IContextBudgetHost"/>.</summary>
    IContextBudgetHost? ContextBudget { get; }

    /// <summary>
    /// The chat id this session lives as, or null for a session with no chat behind it (a bare CLI or
    /// worker entry point). A spawned run reads its caller's <see cref="AgentSessionScope.Current"/>
    /// for this value to learn its own <c>parent:</c> — the same ambient read the depth counter and
    /// sandbox inheritance already use — and its own spawned session carries its own chat id here, so
    /// a nested spawn's parent is always the chat that actually spawned it, human or spawned alike.
    /// </summary>
    string? ChatId { get; }
}

/// <summary>Plain bundle of the per-chat agent dependencies. Used by the UI chat VM and by
/// spawned sub-agents to open an <see cref="AgentSessionScope"/> over an isolated state set.</summary>
public sealed class AgentSession : IAgentSession
{
    public AgentSession(IKeyValueStore sessionKv, MarkManager checkpoint, ISkillSession skills,
        IBlobStore? blobs = null, ISandbox? sandbox = null,
        IToolSetSession? toolSets = null, Security.ChatDoubt? doubt = null,
        IBackgroundTaskHost? background = null, string? chatId = null,
        ICorrespondenceHost? correspondence = null, IContextBudgetHost? contextBudget = null)
    {
        ContextBudget = contextBudget;
        Doubt = doubt ?? new Security.ChatDoubt();
        SessionKv = sessionKv;
        Checkpoint = checkpoint;
        Skills = skills;
        ToolSets = toolSets ?? new ToolSetSession();
        Blobs = blobs ?? new BlobStore();
        Sandbox = sandbox ?? PassthroughSandbox.Default;
        Background = background;
        ChatId = chatId;
        Correspondence = correspondence;
    }

    public IKeyValueStore SessionKv { get; }
    public IBlobStore Blobs { get; }
    public MarkManager Checkpoint { get; }
    public ISkillSession Skills { get; }
    public IToolSetSession ToolSets { get; }
    public ISandbox Sandbox { get; }
    public Security.ChatDoubt Doubt { get; }
    public IBackgroundTaskHost? Background { get; }
    public ICorrespondenceHost? Correspondence { get; }
    public IContextBudgetHost? ContextBudget { get; }
    public string? ChatId { get; }
}

/// <summary>
/// Ambient channel that carries the <see cref="IAgentSession"/> of the chat whose conversation
/// loop is currently running. A chat opens a scope with <see cref="Begin"/> around its run; tools
/// — however deep in the call stack and across async/parallel boundaries — read
/// <see cref="Current"/> to act on the right chat's state. Same <see cref="AsyncLocal{T}"/>
/// approach as <see cref="Tools.ClarifyScope"/> and <see cref="Tools.ProgressScope"/>, so
/// multiple chats can run concurrently in the background without their tool calls colliding.
/// When no scope is open, <see cref="Current"/> is <c>null</c>.
/// </summary>
public static class AgentSessionScope
{
    private static readonly AsyncLocal<IAgentSession?> _current = new();

    /// <summary>The agent session for the current async flow, or <c>null</c> when no scope is open.</summary>
    public static IAgentSession? Current => _current.Value;

    /// <summary>
    /// Routes agent-state access on the current async flow to <paramref name="session"/> until the
    /// returned handle is disposed. Nesting restores the previous session (e.g. a spawned sub-agent).
    /// </summary>
    public static IDisposable Begin(IAgentSession session)
    {
        var previous = _current.Value;
        _current.Value = session;
        return new Restore(previous);
    }

    private sealed class Restore : IDisposable
    {
        private readonly IAgentSession? _previous;
        public Restore(IAgentSession? previous) => _previous = previous;
        public void Dispose() => _current.Value = _previous;
    }
}
