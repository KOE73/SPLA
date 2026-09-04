using Microsoft.Extensions.Logging;
using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.Library.Catalog;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.ToolSets;
using System.IO;

namespace SPLA.Runtime;

/// <summary>
/// One live chat: its conversation history, session-scoped working memory, checkpoint manager,
/// agent session, and orchestrator. This is the per-chat "VM" — created when a client opens a chat,
/// it owns everything that must not be shared between chats. The shared, process-wide pieces (LLM,
/// tools, plugins, prompt, project KV) come from the injected <see cref="AgentRuntime"/>.
/// <para>
/// A single chat runs one turn at a time (guarded by <see cref="_turnGate"/>), but distinct
/// <see cref="ChatRuntime"/> instances run fully in parallel: each opens its own ambient
/// <see cref="AgentSessionScope"/>/<see cref="PermissionScope"/>/<see cref="ClarifyScope"/> around
/// its run, so tool calls from concurrent chats never collide.
/// </para>
/// </summary>
public sealed class ChatRuntime : IDisposable, SPLA.Domain.Agent.IBackgroundTaskHost, SPLA.Domain.Agent.ICorrespondenceHost, IReplyToolSource
{
    private readonly AgentRuntime _runtime;

    /// <summary>This chat's own boundary — the project's workspace and gate, its own shell. Owned,
    /// and therefore ended in <see cref="Dispose"/>.</summary>
    private readonly SPLA.Domain.Host.ISandbox _sandbox;

    /// <summary>The project's chat directory — what lets this chat resolve a correspondent's chat id
    /// to a live runtime (waking a sleeping one) or find out it went away. Null for a
    /// <see cref="ChatRuntime"/> built outside a registry (a bare CLI chat) — correspondence simply
    /// does not work there, the same way roles and spawning degrade gracefully without their own
    /// optional collaborators elsewhere in this codebase.</summary>
    private readonly ChatRegistry? _registry;

    /// <summary>This chat's live correspondences, keyed by (role, topic) exactly as wave 5's virtual
    /// <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c> tool name will be. See <see cref="Correspondences"/>.</summary>
    private readonly Dictionary<(string Role, string Topic), Correspondence> _correspondences = new();

    /// <summary>Ended correspondences (ADR_20260904 §2.1) — kept out of <see cref="_correspondences"/>
    /// so that everything reading the live dictionary (the tool surface, the liveness pass, the
    /// tool-name collision check) goes on seeing only what is still open, and so that reopening the
    /// same (role, topic) later is an ordinary insert rather than a resurrection.</summary>
    private readonly List<Correspondence> _ended = new();

    private int _disposed;

    /// <summary>Cancelled exactly once, in <see cref="Dispose"/>. Every background task's own
    /// cancellation source (see <see cref="BackgroundTaskRegistry"/>) is linked to this token, which
    /// is what lets closing the chat end every live task without the registry having to be told to
    /// walk its own list — the same "one cancel, whole chat" property <see cref="_sandbox"/> gets
    /// from owning its own shell.</summary>
    private readonly CancellationTokenSource _chatLifetime = new();

    /// <summary>
    /// Every progress root live in this chat, turn or background task alike. Populated in
    /// <see cref="SendAsync"/> by registering the turn's own tree the moment the orchestrator hands
    /// it out — additive to whatever <c>OnProgressTree</c> the caller supplied, so a caller that
    /// never looks at this still gets exactly the behaviour it had before.
    /// </summary>
    public SPLA.Domain.Tools.ProgressHub Progress { get; } = new();

    /// <summary>What this chat has queued for delivery with no turn of its own to arrive on — see
    /// <see cref="SPLA.Domain.Tools.ChatInbox"/>. Drained at the top of every loop iteration inside
    /// <see cref="SendAsync"/>; fed by a background task's completion (<see cref="DeliverBackgroundResult"/>).</summary>
    public SPLA.Domain.Tools.ChatInbox Inbox { get; } = new();

    /// <summary>This chat's live and recently-finished detached calls. Reachable from inside a
    /// running tool call only through <see cref="SPLA.Domain.Agent.AgentSessionScope.Current"/>'s
    /// <see cref="SPLA.Domain.Agent.IAgentSession.Background"/> — <c>ChatRuntime</c> implements
    /// <see cref="SPLA.Domain.Agent.IBackgroundTaskHost"/> itself and is handed to its own
    /// <see cref="AgentSession"/> as that capability, so <c>BackgroundStage</c> (which knows nothing
    /// about chats) reaches it the same ambient way it reaches everything else per-chat.</summary>
    public SPLA.Domain.Tools.BackgroundTaskRegistry Tasks { get; }

    /// <summary>
    /// The hub id (<c>ProgressHub.Register</c>'s return) of the tree the CURRENTLY running turn is
    /// using — set the moment the orchestrator hands the tree out, before the first LLM call of the
    /// turn. Lets a caller (see <c>ClientConnection</c>'s <c>OnLlmTurnStart</c>) tell the client which
    /// wire-namespaced node ids (<c>"{treeId}:{nodeId}"</c> — see <c>SplaServiceHost.WireChatProgress</c>)
    /// belong to the turn that is starting, as opposed to a background task's own tree, which must
    /// survive the client's per-turn reset.
    /// </summary>
    public string? CurrentTurnTreeId { get; private set; }

    /// <summary>
    /// This chat's settings under its own <c>as:</c> role (PLAN_20260902 wave 5б) — resolved once
    /// here, when the chat opens, not on every turn: a role's file does not change mid-chat the way a
    /// live settings edit does, and re-resolving per turn would only cost work for no behaviour a
    /// person could see. Null for a chat with no <c>as:</c> — the case that must narrow nothing at
    /// all, not an empty selection (see <see cref="ResolveMode"/> and the <see cref="ChatToolHost"/>
    /// built in the constructor, both of which treat null as "behave exactly as before this wave").
    /// <para>
    /// Also null when a role WAS named but no longer resolves — struck from the manifest, its file
    /// gone, or this chat has no project to resolve one against — rather than throwing out of the
    /// constructor and refusing to open the chat at all. Roles and spawning already degrade this way
    /// elsewhere in this codebase for an optional collaborator that isn't there (see <see cref="_registry"/>'s
    /// own comment); a stale role tag on a chat someone still wants to open is exactly that case.
    /// </para>
    /// </summary>
    private readonly ResolvedSettings? _roleSettings;

    private readonly ChatSession _chat;
    private readonly Conversation _conversation = new();
    private readonly KeyValueStore _sessionKv = new("session");
    private readonly SkillSession _skillSession = new();
    private readonly ToolSetSession _toolSetSession = new();
    private readonly CheckpointManager _checkpoint = new();
    private readonly AgentSession _agentSession;
    private readonly ChatToolHost _toolHost;
    private readonly ConversationOrchestrator _orchestrator;
    private readonly SemaphoreSlim _turnGate = new(1, 1);

    /// <summary>Maps a user message to the sidecar image filenames persisted for it. The binary lives
    /// on disk under <c>.spla/chat-images/&lt;chatId&gt;/</c>; only filenames ride in the chat YAML.</summary>
    private readonly Dictionary<ChatMessage, List<string>> _imageFiles = new();

    public string ChatId => _chat.Id;
    public string Title => _chat.Title;
    public ChatSession Session => _chat;
    public IReadOnlyList<ChatMessage> Messages => _conversation.Messages;

    private int _turnsInFlight;

    /// <summary>True while a turn is running (or queued) in this chat. Chat-level truth, deliberately:
    /// a turn is gated by the CHAT, not by the connection that started it, so a window opening this
    /// chat mid-turn (another window, a reload, a second user) can show Stop instead of an input that
    /// looks ready. Counted rather than read off the gate so a turn still waiting for the gate also
    /// reads as busy — to the person looking at the chat there is no difference.</summary>
    public bool IsTurnRunning => Volatile.Read(ref _turnsInFlight) > 0;

    private int _humanTurnCount;

    /// <summary>How many turns in this chat's life were started with an actual human message (as
    /// opposed to a pump-woken turn started with <c>text: null</c>, see <see cref="SendAsync"/>).
    /// The pump's self-feeding guard (ADR §2.6) watches this rise to tell "a person spoke since my
    /// last auto-wake" — reading a count rather than subscribing to an event keeps that guard from
    /// needing any coupling back into the chat beyond this one number.</summary>
    public int HumanTurnCount => Volatile.Read(ref _humanTurnCount);

    private int _autoWakeSuppressed;

    /// <summary>
    /// True after Stop has disarmed the pump (PLAN_20260825 wave C, ADR §2.4) — no auto-wake until the
    /// next real human message. Set by <c>CorrelationHandlers.Cancel</c>, read by <see cref="ChatPump"/>'s
    /// injected <c>autoWakeSuppressed</c> delegate, cleared in the same place <see cref="_humanTurnCount"/>
    /// is bumped: "stop" and the pump's own self-feeding pause are the same state reached by different
    /// roads, and a person speaking is what ends both. <c>Volatile</c> rather than a lock — a single
    /// flag read/written from different threads (the cancel handler, the pump's timer callback, a
    /// turn's own start) needs visibility, not mutual exclusion.
    /// </summary>
    public bool AutoWakeSuppressed => Volatile.Read(ref _autoWakeSuppressed) != 0;

    /// <summary>Disarms the pump until the next human turn. See <see cref="AutoWakeSuppressed"/>.</summary>
    public void SuppressAutoWake() => Volatile.Write(ref _autoWakeSuppressed, 1);

    /// <summary>Marks that a human's own words are about to enter this chat — called by whoever enqueues
    /// a <see cref="SPLA.Domain.Tools.InboxItemKind.Human"/> item, before enqueueing it, so the pump wakes
    /// unconditionally for it (ADR §2.1: "wakes immediately and always") and any auto-wake pause a Stop or
    /// the self-feeding guard left behind ends right here — same state, same clearing this chat's own
    /// SendAsync text-path already does for a direct send (see below); this is the queued-path's copy of
    /// that exact bookkeeping.</summary>
    public void NoteHumanMessage()
    {
        Interlocked.Increment(ref _humanTurnCount);
        Volatile.Write(ref _autoWakeSuppressed, 0);
    }

    /// <summary>The current turn's own onUserMessage callback, stashed here so the DrainInbox closure
    /// below (built once in the constructor, but invoked from inside whichever turn is live) can echo a
    /// Human- or Peer-kind drained message back to watchers exactly like a directly-sent one. Only one
    /// turn ever runs at a time (guarded by _turnGate), so there is no re-entrancy to worry about.</summary>
    private Action<ChatMessage>? _activeOnUserMessage;

    /// <summary>Human- and Peer-kind messages this turn's DrainInbox has pulled off the queue but which
    /// have not yet been added to the conversation (and so have no MsgId yet) — see the constructor's
    /// DrainInbox/OnMessageDelivered pair for why the echo has to wait that long. Peer joined Human here
    /// for wave 7: an incoming reply must render live, as speech ("← from &lt;role&gt;"), the same
    /// moment it lands, not only the next time the chat is reopened — <see cref="ChatMessage.PeerFrom"/>
    /// already rides on the message itself, so nothing else about this plumbing needs to know which kind
    /// it was. Reference-keyed: two messages are never "the same" here unless they are literally the
    /// same instance.</summary>
    private readonly HashSet<ChatMessage> _pendingEchoes = new(ReferenceEqualityComparer.Instance);

    private int _bubbleSeq;

    /// <summary>
    /// The next streaming-bubble index for this chat, monotonic for the chat's whole life.
    /// <para>It lives here rather than in the caller's per-turn state because a per-turn counter
    /// restarted at zero on every turn: the second turn's first bubble reused the first turn's key, so
    /// two live bubbles in one chat shared an identity and the client's stream bookkeeping collided.
    /// On the chat means it is also correct when two connections drive the same chat.</para>
    /// </summary>
    public int NextBubbleIndex() => Interlocked.Increment(ref _bubbleSeq);

    /// <summary>The skill running in this chat, or null when idle.</summary>
    public string? ActiveSkillId => _skillSession.ActiveSkillId;

    /// <summary>
    /// Ends the running skill from outside the model — the user's way out.
    ///
    /// <para>Needed because <c>skill_deactivate</c> is the model's own decision, and a model that
    /// simply never calls it leaves the chat wedged: the skills index is suppressed while a skill is
    /// active, so it cannot be told about another one, and <c>skill_activate</c> refuses a second.
    /// Hosts bind this to an "Unload skill" control.</para>
    /// </summary>
    /// <summary>
    /// Hands a skill to this chat on the user's say-so — the loan-desk counterpart of
    /// <see cref="DeactivateSkill"/>, and the third way of taking a book out that the ADR names.
    ///
    /// <para><b>Level is deliberately not consulted.</b> A person choosing from a list they can see
    /// is the whole point of an out-of-catalog source: invisible to the model, perfectly visible to
    /// its owner. State still is: a skill switched off, untrusted, or missing its tools must not slip
    /// in through a different door than the model's.</para>
    ///
    /// <para>Costs the prompt nothing beyond the procedure itself. The index is suppressed while a
    /// skill is active, so a handed-out chat carries no catalog at all — which is what makes this the
    /// answer for a small context window.</para>
    /// </summary>
    /// <returns>Null on success, else a human-readable reason.</returns>
    public string? ActivateSkill(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId)) return "no skill id given";
        if (_skillSession.ActiveSkillId is { } running)
            return $"skill '{running}' is already active — end it first";

        var lookup = _runtime.SkillLibrary.Resolve(skillId);
        if (lookup.IsAmbiguous)
            return $"'{skillId}' is held by more than one source — ask for one of: " +
                   string.Join(", ", lookup.Candidates.Select(c => c.Address));

        var skill = lookup.Card;
        if (skill is null) return $"unknown skill '{skillId}'";
        if (skill.State != SkillState.Available) return $"'{skill.DisplayId}' is not available — {skill.StateReason}";

        var body = _runtime.SkillLibrary.LoadBody(skill.Address);
        if (string.IsNullOrWhiteSpace(body))
            return $"'{skill.DisplayId}' has no readable procedure — its source '{skill.SourceId}' returned nothing";

        _skillSession.Activate(skill.DisplayId, body, skill.SourceId, skill.Ref,
            _runtime.SkillLibrary.ListResources(skill.Address));

        // The same sets skill_activate would have raised. A procedure handed over by a person must
        // arrive able to run, and the sets waiting on exactly this were declared by the skill itself.
        foreach (var toolName in skill.Requires.Tools)
        {
            if (_runtime.ToolSets.SetOfTool(toolName) is not { } setId) continue;
            if (_runtime.ToolSets.LevelOf(setId) != SPLA.MCP.Core.ToolSets.ToolSetLevel.SkillDemand) continue;
            if (_toolSetSession.IsActive(setId)) continue;

            _toolSetSession.Activate(setId, ToolSetActivationBy.Skill, $"required by skill '{skill.Id}'");
        }

        return null;
    }

    /// <returns>The id that was deactivated, or null when nothing was running.</returns>
    public string? DeactivateSkill()
    {
        var previous = _skillSession.ActiveSkillId;
        _skillSession.Deactivate();
        _toolSetSession.DeactivateAllBy(ToolSetActivationBy.Skill);

        // Returning the book is the moment a fond change held back for it can safely land. Without
        // this, a folder added mid-procedure would simply never appear — nothing else is scheduled
        // to make it appear.
        _runtime.SkillLibrary.ApplyDeferredRebuild();
        return previous;
    }

    /// <summary>Tool sets raised in this chat, in the order they were raised — what the status bar
    /// shows and what a host offers to lower.</summary>
    public IReadOnlyList<ToolSetActivation> ActiveToolSets => _toolSetSession.Active;

    /// <summary>Lowers a raised set from outside the model — the person's way out, and the reason
    /// <c>toolset_deactivate</c> can stay a permission for the model rather than a duty.</summary>
    /// <returns>True when something was actually lowered.</returns>
    public bool DeactivateToolSet(string setId) => _toolSetSession.Deactivate(setId);

    /// <summary>
    /// Composes this chat's context surface for inspection — the same call the agent loop makes,
    /// wrapped in this chat's <see cref="AgentSessionScope"/> so contributors see the right chat's
    /// active skill and working memory. Without the scope the debug view would show the surface of
    /// an idle chat and quietly disagree with what was actually sent.
    /// </summary>
    public SPLA.MCP.Core.Composition.ComposedContext ComposeContext()
    {
        using var scope = AgentSessionScope.Begin(_agentSession);
        return _runtime.ComposeContext();
    }

    /// <summary>
    /// This chat's own tool surface — before mode gating, exactly as <see cref="ChatToolHost"/> hands
    /// it to the orchestrator (see <see cref="_toolHost"/>). For inspection and tests: proves a role's
    /// narrowing (or its absence) the same way <c>ComposeContext</c> above proves the prompt surface,
    /// without needing to drive a whole turn through a fake LLM to observe what reached it.
    /// </summary>
    public IEnumerable<string> AvailableToolNames() => _toolHost.GetToolDefinitions().Select(d => d.Function.Name);

    /// <summary>This chat's session-scoped working memory entries (for the debug inspector).</summary>
    public IEnumerable<(string Key, string Value)> SessionKvEntries
        => _sessionKv.List().Select(e => (e.Key, e.Value));

    /// <summary>This chat's session memory with origin labels (for the debug inspector).</summary>
    public IReadOnlyList<SPLA.Domain.Agent.KvEntry> SessionKvOrigins => _sessionKv.Entries();

    /// <summary>Whether this chat has taken in anything from a source nobody named.</summary>
    public SPLA.Domain.Security.ChatDoubt Doubt => _agentSession.Doubt;

    /// <summary>This chat's data-channel blobs (for the debug inspector).</summary>
    public IReadOnlyList<BlobEntry> BlobEntries => _agentSession.Blobs.List();

    /// <summary>The exact message list sent in the most recent LLM request (for the debug inspector).
    /// Captured via the turn's OnLlmTurnStart callback; empty until the first turn runs.</summary>
    public IReadOnlyList<ChatMessage> LastContext { get; private set; } = System.Array.Empty<ChatMessage>();

    public void CaptureLastContext(IReadOnlyList<ChatMessage> context) => LastContext = context;

    /// <summary>Injects a message directly into the conversation without running a turn (e.g. skill load).
    /// The message is persisted immediately.</summary>
    public void InjectMessage(ChatRole role, string content)
    {
        _conversation.Add(new ChatMessage { Role = role, Content = content });
        Save();
    }

    /// <summary>This chat's effective mode name (its own, or the project default).</summary>
    public string ModeName => ResolveMode().ToString();

    /// <summary>The model entry this chat points at, if any.</summary>
    public string? ModelId => _chat.ModelId;

    /// <summary>Changes the chat's mode and/or model entry (null = leave as-is) and persists it.</summary>
    public void ApplySettings(string? mode, string? modelId, double? temperature = null, string? reasoning = null)
    {
        if (!string.IsNullOrWhiteSpace(mode))
        {
            _chat.Agent ??= new SplaAgentSection();
            _chat.Agent.Mode = mode;
        }
        if (modelId != null) _chat.ModelId = modelId;

        // Both knobs live in the chat's own llm section, layered over the project's on every turn.
        // An empty reasoning string is a real choice — "stop overriding, take the project default" —
        // so it is written as null rather than skipped.
        if (temperature is { } t)
        {
            _chat.Model ??= new SplaLlmSection();
            _chat.Model.Temperature = t;
        }
        if (reasoning != null)
        {
            _chat.Model ??= new SplaLlmSection();
            _chat.Model.ReasoningLevel = reasoning.Length == 0 ? null : reasoning;
        }

        Save();
    }

    /// <summary>This chat's OPEN correspondences (PLAN_20260902 wave 4) — a set, never a single link
    /// back to whoever spawned this chat. Snapshot: safe to enumerate while <see cref="SendReply"/> or
    /// <see cref="RefreshCorrespondences"/> mutates the live dictionary underneath.
    /// <para>
    /// Ended ones are not here; they live in <see cref="EndedCorrespondences"/>
    /// (ADR_20260904 §2.1). Keeping the two apart rather than flagging one collection is what lets the
    /// live tool surface stay unchanged: <c>ChatToolHost</c> reads this and therefore stops offering a
    /// dead address's <c>reply_*</c> without knowing tombstones exist, and
    /// <see cref="RefreshCorrespondences"/> cannot re-strike (and re-announce) something already ended.
    /// </para></summary>
    public IReadOnlyCollection<Correspondence> Correspondences => _correspondences.Values.ToList();

    /// <summary>Correspondences that have ended — the tombstones ADR_20260904 §2.1 keeps instead of
    /// deleting. Never offered as a tool and never re-checked for liveness; persisted alongside the open
    /// ones so the fact that this chat once corresponded with that role survives on THIS side, not only
    /// in the archived correspondent's own file.</summary>
    public IReadOnlyCollection<Correspondence> EndedCorrespondences => _ended.ToList();

    /// <summary>
    /// Registers (or returns the existing) correspondence for (<paramref name="role"/>,
    /// <paramref name="topic"/>) — the machinery wave 5's <c>agent_correspond</c> tool opens a
    /// correspondence through. Idempotent on purpose: opening the same address twice must not reset
    /// an already-running exchange's depth or initiator.
    /// </summary>
    public Correspondence OpenCorrespondence(
        string role, string topic, string correspondentChatId, CorrespondenceInitiator initiator)
    {
        var key = (role, topic);
        if (_correspondences.TryGetValue(key, out var existing)) return existing;

        // Decided once, here, and frozen into the record: whether another correspondent already
        // holds this role AT THIS MOMENT is what earns the topic a place in the name (ADR §2.3). A
        // role that gains a second correspondent later does not reach back and rename this one —
        // see Correspondence.ToolName's own comment (plan trap 11).
        var collides = _correspondences.Values.Any(c => string.Equals(c.Role, role, StringComparison.OrdinalIgnoreCase));
        var toolName = ReplyToolNaming.BuildToolName(role, topic, includeTopic: collides);

        var correspondence = new Correspondence
        {
            Role = role, Topic = topic, ChatId = correspondentChatId, Initiator = initiator, ToolName = toolName
        };
        _correspondences[key] = correspondence;
        return correspondence;
    }

    /// <summary>
    /// The soft-link liveness pass (ADR_20260827-2 §2.4): for every correspondence this chat holds,
    /// asks <see cref="ChatRegistry.Locate"/> where the correspondent's chat currently is.
    /// <list type="bullet">
    /// <item><description><see cref="SPLA.Domain.Settings.ChatLocation.Active"/> — reaches
    /// <see cref="ChatRegistry.GetOrOpen"/>, which answers AND wakes a sleeping chat in the same
    /// call. Nothing else happens: waking is the whole point, and there is no result to act on.</description></item>
    /// <item><description><see cref="SPLA.Domain.Settings.ChatLocation.Archived"/> or
    /// <see cref="SPLA.Domain.Settings.ChatLocation.Missing"/> — the correspondence is dead. Struck
    /// lazily, right here, and a <see cref="InboxItemKind.Notice"/> is queued so the model does not
    /// find a tool it used last turn simply gone (see <see cref="InboxItemKind"/>'s own comment).</description></item>
    /// </list>
    /// Deliberately never a subscription to <see cref="ChatRegistry.RuntimeClosed"/> (trap 3): that
    /// event fires on an ordinary sleep too, and a subscription would tear down a correspondence with
    /// a chat nobody killed. Called once per turn, near the top of <see cref="SendAsync"/> — the
    /// practical wave-4 stand-in for "at turn surface assembly": the virtual <c>reply_*</c> tools that
    /// will actually BE that surface are wave 5's, so there is nothing yet to refresh per LLM
    /// iteration rather than once per turn.
    /// </summary>
    public void RefreshCorrespondences()
    {
        if (_registry is null || _correspondences.Count == 0) return;

        foreach (var key in _correspondences.Keys.ToList())
        {
            var correspondence = _correspondences[key];
            switch (_registry.Locate(correspondence.ChatId))
            {
                case SPLA.Domain.Settings.ChatLocation.Active:
                    _registry.GetOrOpen(correspondence.ChatId);
                    break;
                case SPLA.Domain.Settings.ChatLocation.Archived:
                    StrikeCorrespondence(key, correspondence, archived: true);
                    break;
                case SPLA.Domain.Settings.ChatLocation.Missing:
                    StrikeCorrespondence(key, correspondence, archived: false);
                    break;
            }
        }
    }

    /// <summary>Removes a dead correspondence and tells this chat about it — different wording for
    /// "archived" vs "deleted" (ADR §2.4: "для текста уведомления это разные новости"). Queued through
    /// <see cref="Inbox"/> like an ordinary <see cref="InboxItemKind.Notice"/>: it must reach the
    /// model's context on the next turn, but must never itself wake one (<c>ChatPump.OnEnqueued</c>
    /// ignores this kind).</summary>
    private void StrikeCorrespondence((string Role, string Topic) key, Correspondence correspondence, bool archived)
    {
        // Moved to the tombstone list, never dropped (ADR_20260904 §2.1). Removing it from the live
        // dictionary is still what stops the reply tool being offered and what keeps the liveness pass
        // from re-announcing this every turn — the record itself survives to be persisted.
        _correspondences.Remove(key);
        correspondence.EndedAt = DateTimeOffset.UtcNow;
        correspondence.EndedReason = archived ? "archived" : "deleted";
        _ended.Add(correspondence);
        var topicSuffix = string.IsNullOrEmpty(correspondence.Topic) ? "" : $" ({correspondence.Topic})";
        var text = archived
            ? $"Correspondence with {correspondence.Role}{topicSuffix} has gone quiet — their chat was archived."
            : $"Correspondence with {correspondence.Role}{topicSuffix} has ended — their chat was deleted.";

        Inbox.Enqueue(new ChatMessage
        {
            Role = ChatRole.System,
            Content = text,
            RetentionPolicy = SPLA.Domain.Models.ContextRetention.Persistent
        }, InboxItemKind.Notice);
    }

    /// <summary>What became of a <see cref="SendReply"/> call.</summary>
    public enum ReplyOutcome { Delivered, Denied, CorrespondentGone, UnknownCorrespondence }

    /// <summary>A delivery receipt, never the correspondent's answer (ADR §2.3: "инструмент возвращает
    /// квитанцию о доставке, а не ответ") — wave 5's virtual tool is what turns this into the actual
    /// tool result text a model sees.</summary>
    public readonly record struct ReplyResult(ReplyOutcome Outcome, string? Reason)
    {
        public bool Delivered => Outcome == ReplyOutcome.Delivered;
    }

    /// <summary>
    /// Sends one reply across an already-open correspondence — the machinery wave 5's virtual
    /// <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c> tool calls into. A reply is an edge source→sink
    /// (ADR §2.2), and this chat is the source: its own <see cref="ISandbox.Gate"/> is what gets
    /// asked, not the recipient's and not some separate correspondence-only permission (ADR §2.4:
    /// "гранты те же" — the same gate every other call already goes through).
    /// </summary>
    public ReplyResult SendReply(string role, string topic, string text)
    {
        if (!_correspondences.TryGetValue((role, topic), out var correspondence))
            return new ReplyResult(ReplyOutcome.UnknownCorrespondence,
                $"no open correspondence with '{role}'" + (topic.Length > 0 ? $" ({topic})" : ""));

        if (!_sandbox.Gate.CanCorrespond())
            return new ReplyResult(ReplyOutcome.Denied, "correspondence is not permitted for this chat");

        if (_registry is null)
            return new ReplyResult(ReplyOutcome.CorrespondentGone, "this chat has no directory to reach a correspondent through");

        var location = _registry.Locate(correspondence.ChatId);
        if (location != SPLA.Domain.Settings.ChatLocation.Active)
        {
            var archived = location == SPLA.Domain.Settings.ChatLocation.Archived;
            StrikeCorrespondence((role, topic), correspondence, archived);
            return new ReplyResult(ReplyOutcome.CorrespondentGone,
                archived ? "their chat was archived" : "their chat was deleted");
        }

        var target = _registry.GetOrOpen(correspondence.ChatId);
        if (target is null)
        {
            // Tombstoned rather than dropped, like every other ending (ADR_20260904 §2.1). Deliberately
            // NOT routed through StrikeCorrespondence: that one also queues a notice, and this path
            // never did — the caller is already being told, in the return value, on this very turn.
            _correspondences.Remove((role, topic));
            correspondence.EndedAt = DateTimeOffset.UtcNow;
            correspondence.EndedReason = "unreachable";
            _ended.Add(correspondence);
            return new ReplyResult(ReplyOutcome.CorrespondentGone, "their chat could not be reached");
        }

        // An incoming reply is an ordinary conversation message, not a service result (ADR §2.5:
        // "входящая приезжает обычным user-сообщением") — Persistent is already ChatMessage's default,
        // set explicitly here so the intent survives a future change to that default.
        // ownRole is this chat's own role (defaulting to "agent", role zero) — the attribution the
        // RECIPIENT needs to render this as "← from <ownRole>" (ADR §2.5) rather than an ordinary
        // human message. Display metadata only; ChatMessage.PeerFrom never reaches the provider.
        var ownRoleForPeer = string.IsNullOrWhiteSpace(_chat.As) ? "agent" : _chat.As!;
        target.Inbox.Enqueue(new ChatMessage
        {
            Role = ChatRole.User,
            Content = text,
            RetentionPolicy = SPLA.Domain.Models.ContextRetention.Persistent,
            PeerFrom = ownRoleForPeer
        }, InboxItemKind.Peer);

        correspondence.LastReplyAt = DateTimeOffset.UtcNow;
        correspondence.Depth++;
        // Wave 7б (ADR §2.5's last row): the edge's volume is the replies themselves, never a slice of
        // this turn's real provider usage — see Correspondence.VolumeEstimate's own comment for why.
        correspondence.VolumeEstimate += SPLA.MCP.Core.Composition.TokenEstimate.Of(text);

        return new ReplyResult(ReplyOutcome.Delivered, null);
    }

    /// <summary>
    /// <see cref="SPLA.Domain.Agent.ICorrespondenceHost.Correspond"/> — the machinery
    /// <c>agent_correspond</c> (PLAN_20260902 wave 5) calls into. Finds this chat's already-open
    /// address for (<paramref name="role"/>, <paramref name="topic"/>) or, on demand, creates a fresh
    /// chat under that role and opens the correspondence on both sides, then delivers
    /// <paramref name="text"/> through the same <see cref="SendReply"/> an ordinary
    /// <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c> call would use — so the very first message and every
    /// one after it go through one edge, one gate check, one depth counter.
    /// </summary>
    public SPLA.Domain.Agent.CorrespondResult Correspond(string role, string topic, string text)
    {
        role = role?.Trim() ?? "";
        topic = topic?.Trim() ?? "";
        text = text?.Trim() ?? "";

        if (role.Length == 0)
            return new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.InvalidArgument, "error: 'role' is required");
        if (topic.Length == 0)
            return new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.InvalidArgument,
                "error: 'topic' is required — it is the only thing that tells two correspondents holding the same role apart");
        if (text.Length == 0)
            return new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.InvalidArgument, "error: 'text' is required");

        if (_registry is null)
            return new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.CorrespondentGone,
                "error: this chat has no project chat directory to reach a correspondent through");

        // Roles do not self-assign (ADR §2.1) — a correspondence that spun up an undeclared role's
        // chat would let a model invent an actor the owner never named, exactly the hole role
        // validation on agent_spawn already closes for the errand side of the same mechanism.
        var availableRoles = _runtime.Settings.Manifest?.Roles ?? new List<string>();
        if (!availableRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            var rolesList = availableRoles.Count == 0
                ? "none declared"
                : string.Join(", ", availableRoles.OrderBy(r => r, StringComparer.OrdinalIgnoreCase));
            return new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.UnknownRole,
                $"error: role '{role}' is not available. Available roles: {rolesList}");
        }

        if (!_correspondences.ContainsKey((role, topic)))
        {
            // Not found — create the correspondent's chat on demand (ADR §2.2: "чат собеседника
            // создаётся по требованию") and open the address on both sides. The correspondent
            // addresses this chat back by ITS OWN role — "agent" (role zero) when this chat has none
            // — so its own reply_* tool has something meaningful to be named after.
            //
            // The role travels into CreateNew itself (wave 5б) rather than being patched onto
            // Session.As afterward: the correspondent's ChatRuntime constructor resolves its role's
            // settings once, right there, so a role stamped on only AFTER that constructor already ran
            // would narrow nothing for this chat's whole life.
            var correspondentChat = _registry.CreateNew($"{role}: {topic}", role);

            var ownRole = string.IsNullOrWhiteSpace(_chat.As) ? "agent" : _chat.As!;

            OpenCorrespondence(role, topic, correspondentChat.ChatId, CorrespondenceInitiator.Self);
            correspondentChat.OpenCorrespondence(ownRole, topic, ChatId, CorrespondenceInitiator.Correspondent);
        }

        var reply = SendReply(role, topic, text);
        var toolName = _correspondences.TryGetValue((role, topic), out var c) ? c.ToolName : null;

        return reply.Outcome switch
        {
            ReplyOutcome.Delivered => new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.Delivered,
                $"delivered: correspondence with '{role}' ({topic}) is open — this is a delivery " +
                $"receipt, not their answer. Their reply will arrive on its own; keep working or wait " +
                (toolName is null ? "for it." : $"for it. Use '{toolName}' to send your next message.")),
            ReplyOutcome.Denied => new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.Denied, $"error: {reply.Reason}"),
            ReplyOutcome.CorrespondentGone => new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.CorrespondentGone, $"error: {reply.Reason}"),
            _ => new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.CorrespondentGone, $"error: {reply.Reason}")
        };
    }

    public ChatRuntime(AgentRuntime runtime, ChatSession chat, ChatRegistry? registry = null)
    {
        _runtime = runtime;
        _chat = chat;
        _registry = registry;

        // Resolve this chat's own role settings once, up front — see _roleSettings' own comment for
        // why once-at-open and why a resolution failure degrades to "no role" rather than refusing to
        // open the chat.
        if (!string.IsNullOrWhiteSpace(chat.As) &&
            runtime.Settings.Manifest is { } manifest && runtime.Settings.ProjectFilePath is { } projectFilePath)
        {
            try
            {
                // Beside the manifest, not beside the workspace — same reasoning as SpawnedAgentRunner's
                // identical lookup: roles travel with the manifest in git (ADR_20260827-2).
                var manifestDirectory = Path.GetDirectoryName(projectFilePath)!;
                var roleSection = ConfigLoader.LoadRole(manifestDirectory, chat.As!);
                _roleSettings = SettingsResolver.ResolveForRole(runtime.Settings, manifest, chat.As!, roleSection);
            }
            catch (InvalidOperationException)
            {
                _roleSettings = null;
            }
        }

        // Seed the conversation: system prompt + any persisted messages.
        _conversation.Add(new ChatMessage { Role = ChatRole.System, Content = runtime.SystemPrompt });
        foreach (var m in chat.Messages)
        {
            var msg = new ChatMessage
            {
                Role = m.Role.ToLower() switch
                {
                    "user" => ChatRole.User,
                    "assistant" => ChatRole.Assistant,
                    "tool" => ChatRole.Tool,
                    _ => ChatRole.System
                },
                Content = m.Content,
                Reasoning = string.IsNullOrEmpty(m.Reasoning) ? null : m.Reasoning,
                CreatedAt = m.CreatedAt,
                PeerFrom = m.PeerFrom,
                PromptTokens = m.PromptTokens,
                CompletionTokens = m.CompletionTokens,
                // Restored whenever they were written, independent of today's save_attempts value —
                // a chat opened after the setting was turned off must still show what it recorded
                // while it was on.
                Attempts = m.Attempts is { Count: > 0 }
                    ? m.Attempts.Select(a => new SPLA.Domain.Llm.GenerationAttempt
                    {
                        Index = a.Index,
                        Outcome = Enum.TryParse<SPLA.Domain.Llm.AttemptOutcome>(a.Outcome, true, out var o)
                            ? o : SPLA.Domain.Llm.AttemptOutcome.Repetition,
                        Content = a.Content,
                        Reasoning = a.Reasoning,
                        Note = a.Note,
                        Chars = a.Chars,
                        Duration = TimeSpan.FromMilliseconds(a.DurationMs)
                    }).ToList()
                    : null
            };
            _conversation.Add(msg);
            // Re-link persisted sidecar image filenames so they survive re-saves and show on reopen.
            if (m.Images is { Count: > 0 }) _imageFiles[msg] = new List<string>(m.Images);
        }

        // Restore this chat's session memory (survives restart) and feed live context:* each turn.
        _sessionKv.LoadFrom(chat.Kv);

        // The project's boundary, not a fresh passthrough: until now every chat got
        // PassthroughSandbox.Default and the seam ran empty in production, so a sandbox existed in
        // the type system and nowhere else.
        //
        // Per chat, not the runtime's own: the workspace boundary and the gate are still the
        // project's and still shared, but the shell is this chat's. LocalShell keeps its interactive
        // sessions in the instance, so while every chat pointed at one shell, a process started here
        // outlived the chat that started it with nothing able to say otherwise — and the cap on live
        // sessions was quietly shared out between chats that knew nothing of each other.
        _sandbox = runtime.Sandbox.ForChat();
        Tasks = new SPLA.Domain.Tools.BackgroundTaskRegistry(_chatLifetime.Token);
        _agentSession = new AgentSession(
            _sessionKv, _checkpoint, _skillSession, sandbox: _sandbox, toolSets: _toolSetSession,
            // ChatRuntime implements IBackgroundTaskHost itself (Tasks/Progress/Inbox above) — a
            // background call reaches all three the same ambient way it already reaches everything
            // else per-chat, through AgentSessionScope.Current.Background.
            background: this, chatId: _chat.Id,
            // Same shape again: ChatRuntime implements ICorrespondenceHost itself, so
            // agent_correspond reaches OpenCorrespondence/SendReply through the identical ambient
            // path rather than needing its own way to find "this chat".
            correspondence: this);

        // A reopened chat is as doubtful as it was when it closed. Restored rather than recomputed:
        // what raised the flag was an arrival, and arrivals do not happen again on load.
        if (chat.Doubt.Count > 0)
            _agentSession.Doubt.Restore(chat.Doubt.Select(d => new SPLA.Domain.Security.DoubtCause(
                new SPLA.Domain.Security.DataOrigin(d.Zone, OperatorNamed: false),
                d.What,
                new DateTimeOffset(DateTime.SpecifyKind(d.At, DateTimeKind.Utc)))));

        // Wave 7б: restore correspondences straight into the live dictionary rather than through
        // OpenCorrespondence — that method decides ToolName fresh from "does a same-role correspondent
        // already exist", which is exactly wrong here: the persisted ToolName was decided once, in the
        // past, and must come back unchanged (plan trap 11) even if today's in-memory collision check
        // would compute something different.
        if (chat.Correspondences is { Count: > 0 })
        {
            foreach (var c in chat.Correspondences)
            {
                var restored = new Correspondence
                {
                    Role = c.Role,
                    Topic = c.Topic,
                    ChatId = c.ChatId,
                    Initiator = string.Equals(c.Initiator, "correspondent", StringComparison.OrdinalIgnoreCase)
                        ? CorrespondenceInitiator.Correspondent : CorrespondenceInitiator.Self,
                    ToolName = c.ToolName,
                    LastReplyAt = c.LastReplyAt,
                    Depth = c.Depth,
                    VolumeEstimate = c.VolumeEstimate,
                    EndedAt = c.EndedAt,
                    EndedReason = c.EndedReason
                };

                // Ended ones come back as tombstones, not as live addresses (ADR_20260904 §2.1) —
                // restoring one into the live dictionary would re-offer a reply tool for a chat that is
                // gone, and the liveness pass would announce its death a second time after every restart.
                if (restored.IsOpen) _correspondences[(c.Role, c.Topic)] = restored;
                else _ended.Add(restored);
            }
        }

        // Wave 5б's narrowing: built fresh from the runtime's shared, read-only ToolSetRegistry plus
        // this chat's own resolved ToolSets (the role's narrowing of them, or — with no role — null,
        // which ChatToolHost treats as "skip the filter entirely" rather than "filter against
        // nothing"). Nothing here mutates runtime.McpHost or runtime.ToolSets; the narrowing lives
        // entirely in this chat's own ChatToolHost instance. Kept as a field (not built inline for the
        // orchestrator) so AvailableToolNames can inspect the exact same surface without standing up a
        // second one.
        _toolHost = new ChatToolHost(runtime.McpHost, this, runtime.ToolSets, _roleSettings?.ToolSets);
        _orchestrator = new ConversationOrchestrator(runtime.Llm, _toolHost)
        {
            // Live context surface, recomposed on every iteration inside this turn's
            // AgentSessionScope — which is what lets runtime-wide contributors read this chat's
            // active skill and working memory. Settings and plugin edits made since the chat opened
            // apply immediately, and — the reason it is per-iteration — a skill the model activates
            // mid-turn has its procedure in the prompt for the very next LLM call rather than for the
            // next user message.
            Context = runtime.ComposeContext,
            // Split in two because MsgId does not exist yet at drain time — Conversation.Add is what
            // assigns it (see ConversationOrchestrator.OnMessageDelivered's own comment). DrainInbox
            // only remembers WHICH drained messages are a person's own words (by reference — ChatMessage
            // has no value equality, so a HashSet keyed on the instance is exactly "this specific one");
            // OnMessageDelivered fires per message right after conversation.Add has given it a real id,
            // and that is where the echo a directly-sent human message always got finally happens for a
            // queued one too.
            DrainInbox = () =>
            {
                var drained = Inbox.DrainAllWithKinds();
                foreach (var (message, kind) in drained)
                    if (kind is SPLA.Domain.Tools.InboxItemKind.Human or SPLA.Domain.Tools.InboxItemKind.Peer)
                        _pendingEchoes.Add(message);
                return drained.Select(d => d.Message).ToList();
            },
            OnMessageDelivered = message =>
            {
                if (_pendingEchoes.Remove(message)) _activeOnUserMessage?.Invoke(message);
            },
            Checkpoint = _checkpoint,
            // Anti-repeat guard is a per-project setting (agent: loop_guard, default off) — it targets
            // small local models that loop forever, but false-fires on legitimate poll/wait patterns.
            // Only the tool-call guard exists; the error guard waits on a typed ToolResult (debt #4).
            EnableLoopGuard = runtime.Settings.LoopGuard,
            ToolLoopWindow = runtime.Settings.LoopGuardRepeats,
            Logger = runtime.LoggerFactory.CreateLogger<ConversationOrchestrator>()
        };
    }

    /// <summary>The conversation's display messages (system prompt hidden). Hosts project these to
    /// their own wire shapes; persisted sidecar image filenames come from <see cref="ImageFilesFor"/>.</summary>
    public IEnumerable<ChatMessage> DisplayMessages
        => _conversation.Messages.Where(m => m.Role != ChatRole.System);

    /// <summary>Sidecar image filenames persisted for a message, or null when it has none.</summary>
    public IReadOnlyList<string>? ImageFilesFor(ChatMessage message)
        => _imageFiles.TryGetValue(message, out var files) && files.Count > 0 ? files : null;

    /// <summary>Writes the message's data-URL images to sidecar files and records their filenames.</summary>
    private void PersistImages(ChatMessage message, IReadOnlyList<string> dataUrls)
    {
        var project = _runtime.Settings.Project;
        var names = new List<string>();
        foreach (var url in dataUrls)
        {
            try
            {
                var name = ChatImages.WriteDataUrl(project, _chat.Id, url);
                if (name != null) names.Add(name);
            }
            catch { /* a bad image must not break the turn */ }
        }
        if (names.Count > 0) _imageFiles[message] = names;
    }

    /// <summary>
    /// Runs one turn: appends the user message, drives the agent loop, and persists the chat. The
    /// permission and clarify handlers come from the client connection so prompts surface in that
    /// client's UI; <paramref name="callbacks"/> stream the turn's events back to it.
    /// </summary>
    public async Task SendAsync(
        string? text,
        AgentCallbacks callbacks,
        Func<ToolFunctionDefinition, string, Task<PermissionDecision>> permissionHandler,
        Func<ClarifyRequest, Task<string?>> clarifyHandler,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? images = null,
        Action<ChatMessage>? onUserMessage = null)
    {
        // Counted here — synchronously, before the first await — so a caller that hands this task to a
        // host can broadcast "this chat is busy" the instant it starts it, with no window in which the
        // chat still claims to be idle.
        Interlocked.Increment(ref _turnsInFlight);
        try { await _turnGate.WaitAsync(cancellationToken); }
        catch { Interlocked.Decrement(ref _turnsInFlight); throw; }

        try
        {
            _activeOnUserMessage = onUserMessage;

            // The turn's surface, wave-4-style (see RefreshCorrespondences' own comment): a dead
            // correspondent is struck and announced before this turn's context is assembled, so a
            // stale reply_* tool (once wave 5 adds it) never outlives the chat it pointed at by more
            // than one turn.
            RefreshCorrespondences();

            // Registers the turn's tree into the chat-wide hub the moment the orchestrator creates
            // it, without disturbing whatever the caller's own OnProgressTree does with it — both
            // fire on the same handout. A subscriber that only knows this chat's hub (a background
            // task's future sibling) sees the turn's root alongside any other live one.
            var callerOnProgressTree = callbacks.OnProgressTree;
            callbacks = callbacks with
            {
                OnProgressTree = tree =>
                {
                    CurrentTurnTreeId = Progress.Register(tree);
                    callerOnProgressTree?.Invoke(tree);
                }
            };

            // A woken turn (the pump, wave B) adds no user message of its own — its content is
            // whatever SITS in the inbox already, appended by the orchestrator's own drain at the top
            // of its loop (ConversationOrchestrator.cs, DrainInbox). Skipping this whole block for
            // text == null is deliberate: there is nothing here to add, echo, persist as an image, or
            // save early — the turn's own end-of-loop Save() below still runs and picks up whatever
            // the drain appended.
            if (text != null)
            {
                Interlocked.Increment(ref _humanTurnCount);
                // A person spoke — whatever silenced the pump (Stop, or its own self-feeding guard)
                // is over. Same state, different roads in (ADR §2.4): both exist to stop auto-turns
                // until someone is back at the wheel, and a real message is exactly that.
                Volatile.Write(ref _autoWakeSuppressed, 0);

                var userMsg = new ChatMessage
                {
                    Role = ChatRole.User,
                    Content = text,
                    // Data URLs stay in memory for the LLM this turn; the sidecar files below are what persist.
                    Images = images is { Count: > 0 } ? images.ToList() : null
                };
                _conversation.Add(userMsg);
                // MsgId exists only after Add — echo it so the client can anchor rewind/fork on this message.
                onUserMessage?.Invoke(userMsg);
                if (images is { Count: > 0 }) PersistImages(userMsg, images);
                Save();
            }

            // The seeded system message is a placeholder from here on: the orchestrator's SystemPrompt
            // provider re-renders it on every iteration, inside the session scope. Refreshing it here
            // would be both redundant and too early — the scope that carries this chat's active skill
            // opens a few lines below.

            // Live loop-guard setting: a toggle in Settings applies to the very next turn.
            _orchestrator.EnableLoopGuard = _runtime.Settings.LoopGuard;
            _orchestrator.ToolLoopWindow = Math.Max(2, _runtime.Settings.LoopGuardRepeats);

            using var clarifyScope = ClarifyScope.Begin(clarifyHandler);
            using var agentScope = AgentSessionScope.Begin(_agentSession);
            using var permScope = PermissionScope.Begin(permissionHandler);

            // Per-chat resolution: each chat runs against its own connection (endpoint/model) and its
            // own mode, not the project default — mirrors the UI's BuildLlmSettings so the service
            // honours the per-chat autonomy model rather than forcing every chat onto project settings.
            // The reasoning lever is provider-described, and describing it takes a (cached) call —
            // so it is resolved here, once per turn, rather than inside the sync settings fold.
            var llm = ResolveLlmSettings();
            llm.ModelReasoning = await GetReasoningAsync(cancellationToken);

            await _orchestrator.RunAsync(
                _conversation, llm, ResolveMode(), callbacks, cancellationToken);

            // A tool may have injected a synthetic image message mid-turn (see ConversationOrchestrator's
            // pending-image-sink drain). Persist its data URLs to sidecar files exactly like a
            // user-attached image, so the chat YAML stays small and the picture survives reopen.
            foreach (var m in _conversation.Messages)
                if (m.Images is { Count: > 0 } && !_imageFiles.ContainsKey(m))
                    PersistImages(m, m.Images);

            Save();
        }
        finally
        {
            _activeOnUserMessage = null;
            _turnGate.Release();
            Interlocked.Decrement(ref _turnsInFlight);
        }
    }

    /// <summary>
    /// Rewinds the conversation to <paramref name="msgId"/>: with <paramref name="before"/> the
    /// anchor message itself is removed too (user "take back my message"), otherwise everything
    /// after it goes (assistant "return to this point"). Refused while a turn is running.
    /// </summary>
    public bool Rewind(string msgId, bool before)
    {
        if (!_turnGate.Wait(0)) return false;
        try
        {
            var idx = _conversation.Messages.ToList().FindIndex(m => m.MsgId == msgId);
            if (idx < 0) return false;
            _conversation.TruncateTo(before ? idx : idx + 1);
            Save();
            return true;
        }
        finally { _turnGate.Release(); }
    }

    /// <summary>Saves only when no turn is running (fork must not snapshot a half-written
    /// conversation). Returns false when a turn holds the gate.</summary>
    public bool TrySaveIdle()
    {
        if (!_turnGate.Wait(0)) return false;
        try { Save(); return true; }
        finally { _turnGate.Release(); }
    }

    /// <summary>How many persistable messages the chat file keeps up to and including
    /// <paramref name="msgId"/>; -1 if the message is unknown or not persisted. Used by fork to
    /// truncate the duplicated chat file at the anchor.</summary>
    public int PersistedCountUpTo(string msgId)
    {
        var count = 0;
        foreach (var m in _conversation.PersistableWith(_runtime.Settings.SaveToolCalls, _runtime.Settings.SaveAttempts))
        {
            count++;
            if (m.MsgId == msgId) return count;
        }
        return -1;
    }

    /// <summary>Persists the conversation and session KV back to the chat store.</summary>
    public void Save()
    {
        var saveToolCalls = _runtime.Settings.SaveToolCalls;
        var saveAttempts = _runtime.Settings.SaveAttempts;
        _chat.Messages.Clear();
        foreach (var m in _conversation.PersistableWith(saveToolCalls, saveAttempts))
        {
            _chat.Messages.Add(new ChatSessionMessage
            {
                Role = m.Role.ToString().ToLower(),
                Content = m.Content ?? "",
                Reasoning = string.IsNullOrEmpty(m.Reasoning) ? null : m.Reasoning,
                CreatedAt = m.CreatedAt,
                PeerFrom = m.PeerFrom,
                PromptTokens = m.PromptTokens,
                CompletionTokens = m.CompletionTokens,
                Images = _imageFiles.TryGetValue(m, out var files) && files.Count > 0 ? new List<string>(files) : null,
                ToolCalls = saveToolCalls && m.ToolCalls?.Count > 0 ? m.ToolCalls : null,
                ToolCallId = saveToolCalls ? m.ToolCallId : null,
                Attempts = saveAttempts && m.Attempts?.Count > 0
                    ? m.Attempts.Select(a => new ChatSessionAttempt
                    {
                        Index = a.Index,
                        Outcome = a.Outcome.ToString(),
                        Content = a.Content,
                        Reasoning = a.Reasoning,
                        Note = a.Note,
                        Chars = a.Chars,
                        DurationMs = (long)a.Duration.TotalMilliseconds
                    }).ToList()
                    : null
            });
        }
        _chat.Kv = _sessionKv.Snapshot();
        // The flag rides with the history: it only ever goes up, and one that a reload clears is one
        // anybody can clear by closing the window.
        _chat.Doubt = _agentSession.Doubt.Causes
            .Select(c => new ChatSessionDoubt { Zone = c.Origin.Zone, What = c.What, At = c.At.UtcDateTime })
            .ToList();
        // Wave 7б: a correspondence used to die with this ChatRuntime's memory. Persisted the same
        // shape it lives in, with Depth/VolumeEstimate carrying forward the lifetime totals the graph
        // reads back on the other side of a restart.
        // Open ones and tombstones alike (ADR_20260904 §2.1): an ended correspondence that were dropped
        // here would be erased from this side's file on the very next save, which is exactly the loss
        // that ADR exists to stop.
        _chat.Correspondences = _correspondences.Count == 0 && _ended.Count == 0
            ? null
            : _correspondences.Values.Concat(_ended).Select(c => new ChatSessionCorrespondence
            {
                Role = c.Role,
                Topic = c.Topic,
                ChatId = c.ChatId,
                Initiator = c.Initiator == CorrespondenceInitiator.Correspondent ? "correspondent" : "self",
                ToolName = c.ToolName,
                LastReplyAt = c.LastReplyAt,
                Depth = c.Depth,
                VolumeEstimate = c.VolumeEstimate,
                EndedAt = c.EndedAt,
                EndedReason = c.EndedReason
            }).ToList();
        // Carry forward token usage totals from the lifetime history: absence remains absence (null
        // means no message ever reported usage), and presence means at least one reported it (sum over
        // those, treat missing slots as 0). Avoids reparsing the entire message stream on each chat
        // list render — the total only grows.
        var hasUsage = _chat.Messages.Any(m => m.PromptTokens is not null || m.CompletionTokens is not null);
        _chat.PromptTokensTotal = hasUsage ? _chat.Messages.Sum(m => m.PromptTokens ?? 0) : null;
        _chat.CompletionTokensTotal = hasUsage ? _chat.Messages.Sum(m => m.CompletionTokens ?? 0) : null;
        _runtime.ChatManager.SaveChat(_chat);
    }

    /// <summary>The operative context window for this chat's connection (tokens), or null when
    /// unknown. Cached by the runtime with a short TTL; cheap to call once per turn. Lets the turn
    /// path report "prompt tokens vs window" so the UI can warn before the provider rejects.</summary>
    public Task<int?> GetContextLengthAsync(CancellationToken ct = default)
        => _runtime.GetContextLengthAsync(ResolveLlmSettings(), ct);

    /// <summary>What this chat's model will let a caller do with its reasoning channel — what the
    /// status bar draws its lever from, and what gates the wire mapping on a turn.</summary>
    public Task<ReasoningCapability> GetReasoningAsync(CancellationToken ct = default)
    {
        var entry = _runtime.Settings.FindModel(_chat.ModelId) ?? _runtime.Settings.Models.FirstOrDefault();
        return _runtime.GetReasoningAsync(ResolveLlmSettings(), entry?.DeclaredReasoning, ct);
    }

    /// <summary>The chat's effective temperature — its own override, else the project default.</summary>
    public double Temperature => _chat.Model?.Temperature ?? _runtime.Settings.Temperature;

    /// <summary>The chat's effective reasoning selection, in the scalar grammar. Empty = model default.</summary>
    public string ReasoningLevel =>
        string.IsNullOrEmpty(_chat.Model?.ReasoningLevel) ? _runtime.Settings.ReasoningLevel ?? "" : _chat.Model!.ReasoningLevel!;

    /// <summary>The chat's effective LLM settings: its model entry (endpoint/model) layered with its
    /// own behaviour knobs (temperature/reasoning/penalties), falling back to project defaults.</summary>
    private LLMSettings ResolveLlmSettings()
    {
        var entry = _runtime.Settings.FindModel(_chat.ModelId)
                    ?? _runtime.Settings.Models.FirstOrDefault();
        var s = _runtime.Settings.ToLLMSettings(entry);
        var chatModel = _chat.Model;

        s.Mode             = ResolveMode();
        s.Temperature      = chatModel?.Temperature      ?? s.Temperature;
        s.ReasoningLevel   = string.IsNullOrEmpty(chatModel?.ReasoningLevel) ? s.ReasoningLevel : chatModel!.ReasoningLevel;
        s.PresencePenalty  = chatModel?.PresencePenalty  ?? s.PresencePenalty;
        s.FrequencyPenalty = chatModel?.FrequencyPenalty ?? s.FrequencyPenalty;
        s.RepeatPenalty    = chatModel?.RepeatPenalty    ?? s.RepeatPenalty;
        s.MaxTokens        = chatModel?.MaxTokens        ?? s.MaxTokens;
        s.TopP             = chatModel?.TopP             ?? s.TopP;
        s.MinP             = chatModel?.MinP             ?? s.MinP;
        return s;
    }

    /// <summary>
    /// The chat's effective mode: its role's mode when it has one, else its own agent-section
    /// override, else the project default.
    /// <para>
    /// Once a role is named, its resolved mode governs outright — the chat's own per-chat mode
    /// override is deliberately NOT consulted at all, the identical rule <c>SpawnedAgentRunner.RunAsync</c>
    /// applies to the <c>mode</c> argument of a role-carrying spawn (PLAN_20260902 wave 5б: "режим роли
    /// применяется к чату так же, как к прогону"). Letting a chat's own override widen a role after
    /// the fact would make the role no boundary at all. <see cref="ResolvedSettings.Mode"/> already
    /// carries the right fallback for a role that names no <c>mode:</c> of its own — <see cref="SettingsResolver.ResolveForRole"/>
    /// clones the project's own <c>Mode</c> onto it, so this is never a role-shaped guess.
    /// </para>
    /// </summary>
    private AgentMode ResolveMode()
        => _roleSettings is { } role
            ? role.Mode
            : _chat.Agent?.Mode != null && Enum.TryParse<AgentMode>(_chat.Agent.Mode, true, out var m)
                ? m : _runtime.Settings.Mode;

    /// <summary>Wave 6's decay regulator settings (ADR §2.4), resolved the same way every other
    /// role-narrowable number on this chat already is: the role's own value when this chat has a role,
    /// else the project's. <c>ChatPump</c> reads these once, at pump construction — they do not change
    /// for the life of a chat, the same as <see cref="ResolveMode"/>'s own inputs.</summary>
    public TimeSpan PeerDebounceBase => TimeSpan.FromSeconds(
        _roleSettings?.PeerDebounceBaseSeconds ?? _runtime.Settings.PeerDebounceBaseSeconds);

    /// <summary>See <see cref="PeerDebounceBase"/>.</summary>
    public TimeSpan PeerDebounceMax => TimeSpan.FromSeconds(
        _roleSettings?.PeerDebounceMaxSeconds ?? _runtime.Settings.PeerDebounceMaxSeconds);

    /// <summary>See <see cref="PeerDebounceBase"/>.</summary>
    public int PeerDepthCeiling => _roleSettings?.PeerDepthCeiling ?? _runtime.Settings.PeerDepthCeiling;

    /// <summary>See <see cref="PeerDebounceBase"/>.</summary>
    public int PeerHardCap => _roleSettings?.PeerHardCap ?? _runtime.Settings.PeerHardCap;

    /// <summary>
    /// Ends everything this chat holds open. Called when the chat is deleted or the host stops.
    /// <para>
    /// The leak it exists to close was observed rather than imagined: a chat could be gone from the
    /// registry while a shell session it had started was still running, because dropping the runtime
    /// out of a dictionary ends nothing. Today the only such thing is the shell; the frame is here
    /// because the background-task registry is about to be the second, and a chat that could not be
    /// closed had no place to put it.
    /// </para>
    /// <para>Idempotent — a chat may be deleted while a client that had it open is going away too,
    /// and neither caller should have to know about the other.</para>
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

        // Every live background task's own token is linked to this one (BackgroundTaskRegistry) —
        // one cancel here reaches all of them without the registry having to be asked to walk its
        // list. Before the sandbox: a running task may still be mid-shell-command, and its own
        // cancellation is what lets it unwind instead of the shell being pulled out from under it.
        _chatLifetime.Cancel();
        _chatLifetime.Dispose();

        // The chat's own sandbox, and with it the shell sessions this chat started. Never the
        // runtime's: the workspace and gate inside it belong to the project and outlive every chat.
        (_sandbox as IDisposable)?.Dispose();
        _turnGate.Dispose();
    }
}
