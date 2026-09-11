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
using System.Text;

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
public sealed class ChatRuntime : IDisposable, SPLA.Domain.Agent.IBackgroundTaskHost, SPLA.Domain.Agent.ICorrespondenceHost, SPLA.Domain.Agent.IContextBudgetHost, IReplyToolSource, IChatFeedSession
{
    private readonly AgentRuntime _runtime;

    /// <summary>Used only for the correspondence-address log line (PLAN_20260906 wave 0 §3) — every
    /// other diagnostic in this class predates a logger field, so this stays narrowly scoped rather
    /// than becoming a general-purpose one nothing else uses.</summary>
    private readonly ILogger<ChatRuntime> _logger;

    /// <summary>This chat's own boundary — the project's workspace and gate, its own shell. Owned,
    /// and therefore ended in <see cref="Dispose"/>.</summary>
    private readonly SPLA.Domain.Host.ISandbox _sandbox;

    /// <summary>The project's chat directory — what lets this chat resolve a correspondent's chat id
    /// to a live runtime (waking a sleeping one) or find out it went away. Null for a
    /// <see cref="ChatRuntime"/> built outside a registry (a bare CLI chat) — correspondence simply
    /// does not work there, the same way roles and spawning degrade gracefully without their own
    /// optional collaborators elsewhere in this codebase.</summary>
    private readonly ChatRegistry? _registry;

    /// <summary>This chat's live correspondences, keyed by (role, instance number) — the same pair
    /// wave 5's/wave 0's virtual <c>reply_&lt;role&gt;[_&lt;n&gt;]</c> tool name is built from
    /// (PLAN_20260906 wave 0 §2.1: not (role, topic) any more). See <see cref="Correspondences"/>.</summary>
    private readonly Dictionary<(string Role, int InstanceNo), Correspondence> _correspondences = new();

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

    /// <summary>The settings this chat acts under: its role's, else the project's (live). Everything
    /// per-chat reads this — never <c>_runtime.Settings</c> directly, which is how a role's
    /// declarations used to stop at the resolver.</summary>
    private ResolvedSettings EffectiveSettings => _roleSettings ?? _runtime.Settings;

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

    /// <summary>Maps a user message to the sidecar images persisted for it — the file each picture was
    /// written to, and the name it was sent under. The binary lives on disk under
    /// <c>.spla/chat-images/&lt;chatId&gt;/</c>; only the file name and the label ride in the chat YAML.</summary>
    private readonly Dictionary<ChatMessage, List<ChatSessionImage>> _imageFiles = new();

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
    // Formerly a stashed per-turn onUserMessage callback (see the caller-supplied parameter this class
    // used to accept before wave 0 of ADR_20260910-2). Now OnMessageDelivered below just publishes to
    // this chat's own Feed unconditionally — every human/peer message is chat-owned news, not a
    // caller's — so nothing needs to be stashed at turn start any more.

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
    public int NextBubbleIndex()
    {
        var index = Interlocked.Increment(ref _bubbleSeq);
        // A new bubble is a new live partial: whatever the previous one had streamed is either
        // already in the conversation (OnAssistantMessage put it there) or was abandoned.
        lock (_liveGate)
        {
            _liveIndex = index;
            _liveContent.Clear();
            _liveReasoning.Clear();
        }
        return index;
    }

    private readonly Lock _liveGate = new();
    private readonly StringBuilder _liveContent = new();
    private readonly StringBuilder _liveReasoning = new();
    private int? _liveIndex;

    /// <summary>
    /// What the model has streamed into the current bubble and has not yet finished saying, or null
    /// when nothing is in flight.
    ///
    /// <para>Exists because a chat's history is the only thing an opening client is handed, and the
    /// sentence being generated right now is not in it: a window opened mid-turn saw an empty log
    /// until the turn ended. The stream itself reaches only connections that were already watching,
    /// so this is the one way a latecomer can be told where the answer had got to.</para>
    ///
    /// <para>Held here rather than in a host because every host streams the same turn and none of
    /// them outlives the chat — see <see cref="SendAsync"/>, which wraps the caller's own delta sinks
    /// to feed it. Cleared as soon as the assembled message reaches the conversation, so it is never
    /// a second copy of something the history already carries.</para>
    /// </summary>
    public LivePartial? Live
    {
        get
        {
            lock (_liveGate)
            {
                if (_liveIndex is not { } index) return null;
                if (_liveContent.Length == 0 && _liveReasoning.Length == 0) return null;
                return new LivePartial(index, _liveContent.ToString(), _liveReasoning.ToString());
            }
        }
    }

    /// <summary>An unfinished bubble: which one, and how far it has got.</summary>
    /// <param name="MsgIndex">The streaming-bubble index the chunks belong to — the same one the live
    /// stream's own events carry, so a client that later receives both cannot double-render.</param>
    public sealed record LivePartial(int MsgIndex, string Content, string Reasoning);

    private void ClearLive()
    {
        lock (_liveGate)
        {
            _liveIndex = null;
            _liveContent.Clear();
            _liveReasoning.Clear();
        }
    }

    /// <summary>The skill running in this chat, or null when idle.</summary>
    public string? ActiveSkillId => _skillSession.ActiveSkillId;

    /// <summary>This chat's own event stream (ADR_20260910-2, wave 0) — every turn this chat runs
    /// publishes here, whoever started it (see <see cref="SendAsync"/>'s adapter). The concrete type is
    /// exposed (not just <see cref="IChatFeed"/>) because <see cref="ChatFeed.Publish"/> is this class's
    /// own way of writing to it, and only the writer needs that; every other reader only ever needs
    /// <see cref="IChatFeed.Subscribe"/>, which this already satisfies.</summary>
    public ChatFeed Feed { get; } = new();

    private readonly Action<string, SPLA.Domain.Models.ProgressNode> _onProgressNodeChanged;
    private readonly Action<SPLA.Domain.Tools.BackgroundTaskRecord> _onTaskChanged;
    private readonly Action<PendingAsk> _onAskRaised;
    private readonly Action<PendingAsk, AskResolution> _onAskResolved;

    /// <summary>Publishes one event onto this chat's own <see cref="Feed"/> — every emitter below funnels
    /// through this one call so the "who is this chat's stream for" question has one answer.</summary>
    private void Emit(ChatEvent e) => Feed.Publish(e);

    /// <summary>Same as <see cref="Emit(ChatEvent)"/>, but folds the state change the event announces
    /// into the same critical section as its sequence number and delivery — see
    /// <see cref="ChatFeed.Publish"/>'s own comment on <c>mutateUnderGate</c> for why that matters for
    /// a subscriber taking a snapshot concurrently (ADR_20260910-2 §4.4, wave 1).</summary>
    private void Emit(ChatEvent e, Action mutate) => Feed.Publish(e, mutate);

    /// <summary>
    /// Atomically subscribes to this chat's feed and captures its current in-memory state — wave 1's
    /// "снимок плюс поток" (ADR_20260910-2 §4.4). See <see cref="ChatFeed.SubscribeWithSnapshot{T}"/>
    /// for the atomicity guarantee; <see cref="BuildSnapshot"/> is the capture function it runs under
    /// the same lock <see cref="Emit(ChatEvent, Action)"/> uses for a mutating event.
    /// </summary>
    public (ChatFeedSnapshot Snapshot, long Sequence, IDisposable Subscription) SubscribeWithSnapshot(
        Action<ChatEvent> handler)
        => Feed.SubscribeWithSnapshot(BuildSnapshot, handler);

    /// <summary>
    /// Atomically captures this chat's snapshot alongside <paramref name="attach"/> — the wire
    /// path's own use of §4.4: a chat has one permanently registered <c>ChatFeedWireSubscriber</c>
    /// (constructed from <c>ChatRegistry.RuntimeOpened</c>) that fans events to whichever connections
    /// are marked watching, so opening a chat needs no new per-connection <see cref="IChatFeed"/>
    /// subscription — only the mark ("this connection watches now") and the snapshot to agree on
    /// exactly which events fall on which side. See <see cref="ChatFeed.SnapshotUnderGate{T}"/>.
    /// </summary>
    public ChatFeedSnapshot SnapshotForOpen(Action attach) => Feed.SnapshotUnderGate(BuildSnapshot, attach);

    /// <inheritdoc cref="IChatFeedSession.ResubscribeQueuedWithSnapshot"/>
    public (ChatFeedSnapshot Snapshot, IDisposable Subscription) ResubscribeQueuedWithSnapshot(
        Func<ChatEvent, Task> handler, Action onDetached)
        => Feed.SubscribeQueuedWithSnapshot(handler, onDetached, BuildSnapshot);

    /// <summary>Reads every piece of state a reconnecting subscriber needs: the conversation, the
    /// in-flight bubble, every progress node still running (turn tree and background-task trees alike
    /// — <see cref="Progress"/> holds both), every pending ask, every running background task. Called
    /// only from inside <see cref="ChatFeed.SubscribeWithSnapshot{T}"/>'s lock — see that method's own
    /// comment for why running it anywhere else would not be atomic.</summary>
    private ChatFeedSnapshot BuildSnapshot() => new(
        _conversation.Messages.ToList(),
        Live,
        Progress.Trees.SelectMany(kv => kv.Value.Nodes
                .Where(n => n.State == SPLA.Domain.Models.ProgressState.Running)
                .Select(n => new ChatFeedProgressNode(kv.Key, n)))
            .ToList(),
        _runtime.Asks.List(ChatId),
        Tasks.All.Where(t => t.State == SPLA.Domain.Tools.BackgroundTaskState.Running).ToList());

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
        return _runtime.ComposeContext(ResolveMode(), _roleSettings);
    }

    /// <summary>
    /// This chat's own tool surface — before mode gating, exactly as <see cref="ChatToolHost"/> hands
    /// it to the orchestrator (see <see cref="_toolHost"/>). For inspection and tests: proves a role's
    /// narrowing (or its absence) the same way <c>ComposeContext</c> above proves the prompt surface,
    /// without needing to drive a whole turn through a fake LLM to observe what reached it. Asked
    /// inside this chat's own session scope, as a turn asks: the surface depends on whose settings the
    /// session carries, and outside the scope the project's would answer.
    /// </summary>
    public IEnumerable<string> AvailableToolNames()
    {
        using var scope = AgentSessionScope.Begin(_agentSession);
        return _toolHost.GetToolDefinitions().Select(d => d.Function.Name).ToList();
    }

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
    /// Registers (or returns the existing default) correspondence for <paramref name="role"/> — the
    /// machinery <c>agent_correspond</c> opens a correspondence through
    /// (PLAN_20260906 wave 0 §2.2/2.3: the address is (role, instance number), never (role, topic)).
    /// Idempotent by default: without <paramref name="another"/>, opening the same role twice returns
    /// this chat's existing lowest-numbered instance rather than resetting its depth or initiator;
    /// opening the same <paramref name="correspondentChatId"/> twice returns that one either way,
    /// <paramref name="another"/> included — "another" means another correspondent, and this is the
    /// same one.
    /// <para><b>The number is not computed here.</b> It is read off the correspondent's own chat
    /// (<c>ChatSession.AsInstance</c>) by <see cref="ResolveCorrespondentInstance"/>
    /// (ADR_20260906 §2.1). The signature deliberately did not grow a parameter for it: the
    /// correspondent's chat id is already sitting in the argument list, the number is a fact about
    /// that chat, and a caller allowed to pass its own number is a caller able to make two chats
    /// disagree about who <c>architect_4</c> is. Minting, unlike reading, does stay with the owner —
    /// see <see cref="EnsureAsInstance"/>.</para>
    /// </summary>
    /// <param name="introducedBy">Public name of the third chat that introduced these two, or null for
    /// the ordinary case where one of the two opened this itself (ADR_20260906 §2.4/§2.5). Written on
    /// both halves of an introduced edge; it is a fact about the edge, not a third
    /// <see cref="CorrespondenceInitiator"/> value, precisely so that
    /// <c>CorrespondenceGraph.BuildEdges</c> still finds one <c>self</c> half and one
    /// <c>correspondent</c> half and keeps the edge.</param>
    public Correspondence OpenCorrespondence(
        string role, string purpose, string correspondentChatId, CorrespondenceInitiator initiator,
        bool another = false, string? introducedBy = null)
    {
        // Same chat already on the books — return it whatever `another` says. Not politeness: since
        // the ordinal now belongs to the correspondent, a second open against the same chat would
        // compute the same (role, instance) key and quietly overwrite the first record, losing its
        // depth, its volume and its persisted tool name.
        var sameChat = _correspondences.Values.FirstOrDefault(
            c => string.Equals(c.ChatId, correspondentChatId, StringComparison.Ordinal));
        if (sameChat is not null) return sameChat;

        var sameRole = _correspondences.Values
            .Where(c => string.Equals(c.Role, role, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.InstanceNo)
            .ToList();
        if (!another && sameRole.Count > 0) return sameRole[0];

        var instanceNo = ResolveCorrespondentInstance(role, correspondentChatId, sameRole);
        var toolName = ReplyToolNaming.BuildToolName(role, instanceNo);

        // Two different roles can still normalise to the same spelling (transliteration collisions,
        // or the "x" fallback for a script Normalize does not cover), which would otherwise hand two
        // correspondences the same ToolName — and ChatToolHost.ExecuteToolAsync's
        // FirstOrDefault(c => c.ToolName == name) would then silently deliver every reply to whichever
        // one it finds first. Never let that happen: de-suffix with a plain ordinal until the name is
        // free among still-open correspondences (plan PLAN_20260906 wave 0's "минимальная починка").
        if (_correspondences.Values.Any(c => c.ToolName == toolName))
        {
            var n = 2;
            string candidate;
            do { candidate = $"{toolName}_{n++}"; }
            while (_correspondences.Values.Any(c => c.ToolName == candidate));
            toolName = candidate;
        }

        var correspondence = new Correspondence
        {
            Role = role, Purpose = purpose, InstanceNo = instanceNo,
            ChatId = correspondentChatId, Initiator = initiator, IntroducedBy = introducedBy,
            ToolName = toolName
        };
        _correspondences[(role, instanceNo)] = correspondence;

        // Wave 0 §3's logging bullet: the address alone ("reply_architect_2") says nothing about who
        // is on the other end once more than a couple of correspondents pile up — the next log-based
        // post-mortem needs the correspondent's chat title sitting right next to it. Peek, never
        // GetOrOpen: logging a freshly-created correspondent (this chat's own doing, seconds old) is
        // the common case, but this must never wake an unrelated sleeping chat as a side effect of a
        // log line.
        var correspondentTitle = _registry?.Peek(correspondentChatId)?.Title ?? "(not open)";
        _logger.LogInformation(
            "Correspondence opened: {ToolName} -> role={Role} instance={InstanceNo} chat={ChatId} ({Title})",
            toolName, role, instanceNo, correspondentChatId, correspondentTitle);

        return correspondence;
    }

    /// <summary>
    /// The correspondent's public ordinal — the number that makes <c>reply_architect_4</c> mean the
    /// same architect in every chat that holds him (ADR_20260906 §2.1).
    /// <list type="number">
    /// <item><description>The live runtime, if that chat is already open — the freshest answer, and
    /// the usual one, since the correspondent was very often just created by the call above.
    /// <c>Peek</c>, never <c>GetOrOpen</c>: reading a number must not wake a sleeping chat.</description></item>
    /// <item><description>Otherwise the session on disk — which also mints a number for a chat old
    /// enough not to have one, since <c>ChatManager.LoadChat</c> does that on first load.</description></item>
    /// <item><description>Otherwise the old, purely local scheme. Reached when the correspondent has
    /// no public name at all — a spawned run, or a chat this runtime has no directory to look up —
    /// and the honest answer is then "the next free slot in my own list", which is exactly what a
    /// name that nobody else can pronounce is worth.</description></item>
    /// </list>
    /// </summary>
    private int ResolveCorrespondentInstance(
        string role, string correspondentChatId, List<Correspondence> sameRole)
    {
        var live = _registry?.Peek(correspondentChatId)?.Session.AsInstance;
        if (live is > 0) return live.Value;

        var stored = _runtime.ChatManager.LoadChat(correspondentChatId)?.AsInstance;
        if (stored is > 0) return stored.Value;

        return sameRole.Count > 0 ? sameRole[^1].InstanceNo + 1 : 1;
    }

    /// <summary>
    /// This chat's own public ordinal, minted on the spot if it does not have one yet
    /// (ADR_20260906 §2.1, "роли нет → лениво, при первой адресации").
    /// <para>A chat with a role was numbered when it was created — it exists <i>because</i> someone
    /// asked for that role. A chat without one is normally a human's, and numbering every such chat at
    /// creation would burn the <c>agent</c> sequence on the dozens that never speak to anybody. So the
    /// number appears at the one moment it starts to matter: when this chat first becomes somebody's
    /// correspondent and needs a name for their tool list to say.</para>
    /// <para>Written through <see cref="Save"/> immediately, not left in memory: the number is about to
    /// be spelled into another chat's persisted <c>tool_name</c>, and a name that survives on one side
    /// of a restart but not the other is worse than no name. Idempotent — once set, never re-minted, so
    /// the second correspondent gets the same number as the first (trap 11).</para>
    /// </summary>
    public int EnsureAsInstance()
    {
        if (_chat.AsInstance is > 0) return _chat.AsInstance.Value;

        // A spawned run has no public address by design (ChatManager.CreateSpawnedChat): its only
        // address is the link to its parent, so minting here would spend a number on a name nobody
        // can ever say. Answer 1 so callers have something to spell, and store nothing.
        if (SPLA.Domain.Settings.ChatManager.IsSpawned(_chat)) return 1;

        _chat.AsInstance = _runtime.ChatManager.NextInstanceNumber(_chat.As);
        Save();
        return _chat.AsInstance.Value;
    }

    /// <summary>This chat's public name — <c>&lt;role&gt;_&lt;n&gt;</c>, the one string that stands for
    /// it outside itself (ADR_20260906 §2.2). Minting the number if needed, because being asked for a
    /// name is exactly the "first addressing" that mints it.</summary>
    public string PublicName() =>
        ReplyToolNaming.BuildPublicName(PublicRole, EnsureAsInstance());

    /// <summary>The role this chat is publicly named under: its own, or <c>agent</c> — role zero —
    /// when it has none. One constant, shared with <c>ChatManager</c>, so that the fallback cannot
    /// drift between the place that mints the number and the place that spells it.</summary>
    private string PublicRole =>
        string.IsNullOrWhiteSpace(_chat.As) ? SPLA.Domain.Settings.ChatManager.DefaultPublicRole : _chat.As!;

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
    private void StrikeCorrespondence((string Role, int InstanceNo) key, Correspondence correspondence, bool archived)
    {
        // Moved to the tombstone list, never dropped (ADR_20260904 §2.1). Removing it from the live
        // dictionary is still what stops the reply tool being offered and what keeps the liveness pass
        // from re-announcing this every turn — the record itself survives to be persisted.
        _correspondences.Remove(key);
        correspondence.EndedAt = DateTimeOffset.UtcNow;
        correspondence.EndedReason = archived ? "archived" : "deleted";
        _ended.Add(correspondence);
        var topicSuffix = string.IsNullOrEmpty(correspondence.Purpose) ? "" : $" ({correspondence.Purpose})";
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
    /// <c>reply_&lt;role&gt;[_&lt;n&gt;]</c> tool calls into. A reply is an edge source→sink
    /// (ADR §2.2), and this chat is the source: its own <see cref="ISandbox.Gate"/> is what gets
    /// asked, not the recipient's and not some separate correspondence-only permission (ADR §2.4:
    /// "гранты те же" — the same gate every other call already goes through).
    /// </summary>
    public ReplyResult SendReply(string role, int instanceNo, string text)
    {
        if (!_correspondences.TryGetValue((role, instanceNo), out var correspondence))
            return new ReplyResult(ReplyOutcome.UnknownCorrespondence,
                $"no open correspondence with '{role}' (instance {instanceNo})");

        if (!_sandbox.Gate.CanCorrespond())
            return new ReplyResult(ReplyOutcome.Denied, "correspondence is not permitted for this chat");

        if (_registry is null)
            return new ReplyResult(ReplyOutcome.CorrespondentGone, "this chat has no directory to reach a correspondent through");

        var location = _registry.Locate(correspondence.ChatId);
        if (location != SPLA.Domain.Settings.ChatLocation.Active)
        {
            var archived = location == SPLA.Domain.Settings.ChatLocation.Archived;
            StrikeCorrespondence((role, instanceNo), correspondence, archived);
            return new ReplyResult(ReplyOutcome.CorrespondentGone,
                archived ? "their chat was archived" : "their chat was deleted");
        }

        // A spawned correspondent mid-run is alive but not reachable yet: GetOrOpen refuses it (its
        // file is still empty — ADR_20260910-2), and treating that as "unreachable" below would
        // tombstone a correspondence that becomes deliverable the moment the run ends. Refused
        // for now, correspondence kept.
        if (_registry.PeekSpawned(correspondence.ChatId) is not null)
            return new ReplyResult(ReplyOutcome.Denied,
                "their run is still in progress; reply again after it finishes");

        var target = _registry.GetOrOpen(correspondence.ChatId);
        if (target is null)
        {
            // Tombstoned rather than dropped, like every other ending (ADR_20260904 §2.1). Deliberately
            // NOT routed through StrikeCorrespondence: that one also queues a notice, and this path
            // never did — the caller is already being told, in the return value, on this very turn.
            _correspondences.Remove((role, instanceNo));
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
        var ownRoleForPeer = PublicRole;
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
    /// <c>agent_correspond</c> (PLAN_20260902 wave 5; PLAN_20260906 waves 0 and 4) calls into.
    /// Delivers <paramref name="text"/> through the same <see cref="SendReply"/> an ordinary
    /// <c>reply_&lt;role&gt;_&lt;n&gt;</c> call would use, so the very first message and every one
    /// after it go through one edge, one gate check, one depth counter. <paramref name="purpose"/> is
    /// free text, never part of the address (§2.3).
    /// <para>
    /// <paramref name="role"/> names <b>either</b> a declared role — "give me an architect", which
    /// creates a fresh chat under that role, as it always did — <b>or</b> an existing chat's public
    /// name, <c>architect_2</c>: "put me in touch with that one" (ADR_20260906 §2.3). Before this,
    /// addressing and creating were a single act with no way to ask for the first without the second,
    /// which is why nobody could ever be introduced to an existing correspondent.
    /// </para>
    /// </summary>
    public SPLA.Domain.Agent.CorrespondResult Correspond(
        string role, string purpose, string text, bool another = false)
    {
        role = role?.Trim() ?? "";
        purpose = purpose?.Trim() ?? "";
        text = text?.Trim() ?? "";

        if (role.Length == 0)
            return new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.InvalidArgument, "error: 'role' is required");
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
        //
        // A declared role wins over a public name, always: the role is the older and the broader
        // meaning, and this order is exactly why a public name always carries its number
        // (ReplyToolNaming.BuildPublicName) — were the first architect spelled bare `architect`, this
        // branch would swallow him and he would be the one chat unaddressable by name.
        var availableRoles = _runtime.Settings.Manifest?.Roles ?? new List<string>();
        var correspondent = availableRoles.Contains(role, StringComparer.OrdinalIgnoreCase)
            ? OpenByRole(role, purpose, another)
            : OpenByPublicName(role, purpose);

        if (correspondent.Error is { } error) return error;

        var target = correspondent.Correspondence!;
        var reply = SendReply(target.Role, target.InstanceNo, text);

        return reply.Outcome switch
        {
            ReplyOutcome.Delivered => new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.Delivered,
                // Named by the correspondent's public name, not by whatever the caller typed: the
                // receipt is where the model reads the address it will use next, and ADR_20260906 §2.2
                // has exactly one name for that — the same one the tool list, the directory, the log
                // and the graph show.
                $"delivered: correspondence with '{ReplyToolNaming.BuildPublicName(target.Role, target.InstanceNo)}' is open — this is a delivery receipt, not " +
                $"their answer. Their reply will arrive on its own; keep working or wait for it. " +
                $"Use '{target.ToolName}' to send your next message."),
            ReplyOutcome.Denied => new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.Denied, $"error: {reply.Reason}"),
            ReplyOutcome.CorrespondentGone => new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.CorrespondentGone, $"error: {reply.Reason}"),
            _ => new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.CorrespondentGone, $"error: {reply.Reason}")
        };
    }

    /// <summary>Either the correspondence to deliver through, or the refusal to hand back untouched —
    /// the two ways <see cref="OpenByRole"/> and <see cref="OpenByPublicName"/> can end. A local pair
    /// rather than an out-parameter or an exception: both branches must be able to fail with their own
    /// wording, and the wording IS the useful part of the failure.</summary>
    private readonly record struct CorrespondentLookup(
        Correspondence? Correspondence, SPLA.Domain.Agent.CorrespondResult? Error);

    /// <summary>
    /// "Give me an architect" — the older half of <c>agent_correspond</c>, unchanged in meaning:
    /// reuse this chat's existing correspondence with the role, or create the correspondent's chat on
    /// demand (ADR_20260827-2 §2.2, "чат собеседника создаётся по требованию") and open the address on
    /// both sides.
    /// <para><paramref name="another"/> still means "give me one more of these", but it no longer picks
    /// the number: the new chat brings its own from the project counter (ADR_20260906 §2.1), so this
    /// method only decides <i>whether</i> a new chat is made, never what it will be called — which
    /// is why it is consumed entirely here and never passed on to <see cref="Link"/>.</para>
    /// </summary>
    private CorrespondentLookup OpenByRole(string role, string purpose, bool another)
    {
        var existing = _correspondences.Values
            .Where(c => string.Equals(c.Role, role, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.InstanceNo)
            .ToList();
        if (existing.Count > 0 && !another) return new CorrespondentLookup(existing[0], null);

        // The role travels into CreateNew itself (wave 5б) rather than being patched onto Session.As
        // afterward: the correspondent's ChatRuntime constructor resolves its role's settings once,
        // right there, so a role stamped on only AFTER that constructor already ran would narrow
        // nothing for this chat's whole life.
        var correspondentChat = _registry!.CreateNew(purpose.Length > 0 ? $"{role}: {purpose}" : role, role);
        return new CorrespondentLookup(Link(correspondentChat, role, purpose), null);
    }

    /// <summary>
    /// "Put me in touch with that one" — <c>agent_correspond</c> given a public name
    /// (<c>architect_2</c>) instead of a role, the half ADR_20260906 §2.3 adds. Goes past
    /// <c>CreateNew</c> entirely: the chat already exists, and creating a second one would be the very
    /// confusion the ADR was written about.
    /// <para><b>The name is matched, never parsed.</b> Splitting <c>fool_ru_2</c> on the last
    /// underscore is a guess about which part is the role, and a project with roles like <c>fool_ru</c>
    /// and <c>comedian_en</c> (the demo project has exactly those) makes the guess wrong. So every
    /// candidate chat spells its own name the one way <see cref="ReplyToolNaming.BuildPublicName"/>
    /// spells names, and the strings are compared. No parse, nothing to get wrong.</para>
    /// </summary>
    private CorrespondentLookup OpenByPublicName(string name, string purpose)
    {
        var session = FindByPublicName(name);

        if (session is null) return new CorrespondentLookup(null, UnknownAddressee(name));

        if (string.Equals(session.Id, ChatId, StringComparison.Ordinal))
            return new CorrespondentLookup(null, new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.InvalidArgument,
                $"error: '{name}' is this chat — you cannot correspond with yourself"));

        // GetOrOpen, not Peek: the addressee is very likely asleep, and waking him is the point.
        var correspondentChat = _registry!.GetOrOpen(session.Id);
        if (correspondentChat is null)
            return new CorrespondentLookup(null, new SPLA.Domain.Agent.CorrespondResult(
                SPLA.Domain.Agent.CorrespondOutcome.CorrespondentGone,
                $"error: '{name}' could not be reached — that chat is archived or gone"));

        // His ROLE, not the name that was typed: the address is (role, instance), and the instance is
        // his own number, so a chat with no role of its own is corresponded with as `agent`.
        var peerRole = string.IsNullOrWhiteSpace(session.As)
            ? SPLA.Domain.Settings.ChatManager.DefaultPublicRole : session.As!;

        return new CorrespondentLookup(Link(correspondentChat, peerRole, purpose), null);
    }

    /// <summary>
    /// The active session whose public name is <paramref name="name"/>, or null. The one place a public
    /// name is turned back into a chat — shared by <see cref="OpenByPublicName"/> and
    /// <see cref="Introduce"/>, so "what <c>architect_2</c> means" cannot come to mean two things.
    /// <para>Matched, never parsed (see <see cref="OpenByPublicName"/>), and matched only against
    /// active, numbered sessions: a spawned run has no public name by construction
    /// (<see cref="EnsureAsInstance"/>) and an archived chat is not in <c>ListChats</c> at all, so both
    /// answer null here and are refused by wording their callers choose.</para>
    /// </summary>
    private SPLA.Domain.Models.ChatSession? FindByPublicName(string name) =>
        _runtime.ChatManager.ListChats().FirstOrDefault(
            s => s.AsInstance is > 0 &&
                 string.Equals(
                     ReplyToolNaming.BuildPublicName(
                         string.IsNullOrWhiteSpace(s.As) ? SPLA.Domain.Settings.ChatManager.DefaultPublicRole : s.As!,
                         s.AsInstance!.Value),
                     name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Opens both halves of one correspondence — this chat's address for <paramref name="correspondentChat"/>
    /// and his for this one — and returns ours. Both sides always, and here rather than in two places:
    /// an edge with only one half written is invisible to <c>CorrespondenceGraph.BuildEdges</c>, which
    /// pairs a <c>self</c> half with a <c>correspondent</c> half (ADR_20260906 §2.5).
    /// <para><see cref="EnsureAsInstance"/> first, and this is the moment ADR §2.1's lazy minting
    /// actually happens: the correspondent is about to write down what to call us, and until now a
    /// chat with no role had nothing to be called.</para>
    /// <para>Both opens ask for a <i>distinct</i> address (<c>another: true</c>), and that is not the
    /// caller's <c>another</c> flag leaking through — it is the difference between addressing a
    /// kind and addressing a chat. This method is only ever reached with one particular chat in hand:
    /// the fresh one just created, or the one a public name resolved to. Reusing "some correspondence
    /// I already have with that role" here would point the new edge at the wrong chat — and on the
    /// way back it always would, since the correspondent's second visitor is a different chat wearing
    /// the same role as his first. Idempotency comes from <see cref="OpenCorrespondence"/>'s
    /// same-chat check instead, which is the honest test for "already linked".</para>
    /// </summary>
    /// <param name="introducedBy">Null for the ordinary case — this chat opened the edge itself. Set to
    /// a third chat's public name when <see cref="Introduce"/> is driving (ADR_20260906 §2.4), in which
    /// case <c>this</c> is the head the introducer named first and the same value goes on both halves,
    /// making one introduction one group key for the "meeting" view (§2.5).</param>
    private Correspondence Link(
        ChatRuntime correspondentChat, string peerRole, string purpose, string? introducedBy = null)
    {
        var ownRole = PublicRole;
        EnsureAsInstance();

        var ours = OpenCorrespondence(
            peerRole, purpose, correspondentChat.ChatId, CorrespondenceInitiator.Self,
            another: true, introducedBy);
        correspondentChat.OpenCorrespondence(
            ownRole, purpose, ChatId, CorrespondenceInitiator.Correspondent,
            another: true, introducedBy);
        return ours;
    }

    /// <summary>The refusal for a name that is neither a declared role nor anybody's public name. Lists
    /// both alphabets, because the caller cannot tell from the error which of the two he mistyped —
    /// and a model that is shown only the roles will keep inventing roles.</summary>
    private SPLA.Domain.Agent.CorrespondResult UnknownAddressee(string name)
    {
        var roles = _runtime.Settings.Manifest?.Roles ?? new List<string>();
        var rolesList = roles.Count == 0
            ? "none declared"
            : string.Join(", ", roles.OrderBy(r => r, StringComparer.OrdinalIgnoreCase));

        var names = _runtime.ChatManager.ListChats()
            .Where(s => s.AsInstance is > 0 && !string.Equals(s.Id, ChatId, StringComparison.Ordinal))
            .Select(s => ReplyToolNaming.BuildPublicName(
                string.IsNullOrWhiteSpace(s.As) ? SPLA.Domain.Settings.ChatManager.DefaultPublicRole : s.As!,
                s.AsInstance!.Value))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var namesList = names.Count == 0 ? "none yet" : string.Join(", ", names);

        return new SPLA.Domain.Agent.CorrespondResult(
            SPLA.Domain.Agent.CorrespondOutcome.UnknownRole,
            $"error: '{name}' is neither a role nor a chat. Roles (a new correspondent is created): " +
            $"{rolesList}. Existing chats (an address is opened to one that is already there): {namesList}");
    }

    /// <summary>
    /// <see cref="SPLA.Domain.Agent.ICorrespondenceHost.Introduce"/> — knowing two chats and putting
    /// them in touch with each other, while staying off the edge yourself
    /// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.4). Mechanically it is exactly
    /// <see cref="Link"/>, the same pair of <see cref="OpenCorrespondence"/> calls
    /// <see cref="Correspond"/> makes; what is new is only that <c>this</c> chat is neither end.
    /// <para><b>Why an introduction must deliver something.</b> Opening a correspondence touches
    /// nobody's mailbox, and a turn is born from the mailbox (ADR_20260825). Two chats handed each
    /// other's address and nothing else would both go on standing exactly as they were, and the
    /// introducer would read a success receipt for a conversation that never started. So the head of
    /// the edge — <paramref name="first"/>, deterministically, §2.5 — is written to, and speaks first.
    /// The other party gets no message of its own on purpose: the next thing it receives is the first
    /// party's real reply, and a second synthetic one would only wake it to say nothing yet.</para>
    /// <para><b>Who carries the words.</b> Delivery goes through <see cref="SendReply"/> on the second
    /// party's own half of the new edge, so the introduction travels the same one edge, one gate and one
    /// depth counter every later message will (never a direct <c>Inbox.Enqueue</c> past all three). The
    /// consequence is that the recipient sees it attributed to the party it is now corresponding with,
    /// which is why the text is wrapped naming the introducer out loud rather than passed through bare:
    /// the words are the introducer's, and nothing here may quietly put them in somebody else's mouth.</para>
    /// </summary>
    public SPLA.Domain.Agent.IntroduceResult Introduce(
        string first, string second, string purpose, string text)
    {
        first = first?.Trim() ?? "";
        second = second?.Trim() ?? "";
        purpose = purpose?.Trim() ?? "";
        text = text?.Trim() ?? "";

        if (first.Length == 0 || second.Length == 0)
            return Refuse(SPLA.Domain.Agent.IntroduceOutcome.InvalidArgument,
                "error: both 'first' and 'second' are required");
        if (text.Length == 0)
            return Refuse(SPLA.Domain.Agent.IntroduceOutcome.InvalidArgument, "error: 'text' is required");
        if (_registry is null)
            return Refuse(SPLA.Domain.Agent.IntroduceOutcome.AddresseeGone,
                "error: this chat has no project chat directory to reach anybody through");

        // The introducer's own gate, before anything is opened: an introduction is this chat acting on
        // the correspondence graph, and it is refused here for the same reason a reply it could not
        // send is refused there (ADR_20260827-2 §2.4, "гранты те же").
        if (!_sandbox.Gate.CanCorrespond())
            return Refuse(SPLA.Domain.Agent.IntroduceOutcome.Denied,
                "error: correspondence is not permitted for this chat");

        var firstSession = FindByPublicName(first);
        if (firstSession is null) return Unknown(first);
        var secondSession = FindByPublicName(second);
        if (secondSession is null) return Unknown(second);

        if (string.Equals(firstSession.Id, secondSession.Id, StringComparison.Ordinal))
            return Refuse(SPLA.Domain.Agent.IntroduceOutcome.SameChat,
                $"error: '{first}' and '{second}' are the same chat — there is nobody to introduce it to");

        // Naming yourself is not a smaller introduction, it is a different operation with a different
        // result (you end up ON the edge), so it is refused by name rather than silently redirected.
        if (string.Equals(firstSession.Id, ChatId, StringComparison.Ordinal) ||
            string.Equals(secondSession.Id, ChatId, StringComparison.Ordinal))
            return Refuse(SPLA.Domain.Agent.IntroduceOutcome.IntroducerIsParty,
                "error: an introduction connects two OTHER chats — you named yourself as one of them. " +
                "To correspond with somebody yourself, use agent_correspond.");

        // GetOrOpen, not Peek: both are very likely asleep, and one of them is about to be written to.
        var firstChat = _registry.GetOrOpen(firstSession.Id);
        var secondChat = _registry.GetOrOpen(secondSession.Id);
        if (firstChat is null || secondChat is null)
            return Refuse(SPLA.Domain.Agent.IntroduceOutcome.AddresseeGone,
                $"error: '{(firstChat is null ? first : second)}' could not be reached — that chat is archived or gone");

        // Already acquainted: nothing would be created (OpenCorrespondence returns the existing address
        // for the same chat), but delivering onto their edge would drop a third party's words into a
        // conversation he was never part of, attributed to one of them. Refused, and the caller is told
        // the address they already hold so he can stop trying.
        var existing = firstChat.Correspondences.FirstOrDefault(
            c => string.Equals(c.ChatId, secondChat.ChatId, StringComparison.Ordinal));
        if (existing is not null)
            return Refuse(SPLA.Domain.Agent.IntroduceOutcome.AlreadyLinked,
                $"error: '{first}' and '{second}' already correspond ('{existing.ToolName}' on {first}'s side) — " +
                "nothing to introduce. Say what you wanted to say to one of them yourself.");

        var introducer = PublicName();
        var firstName = firstChat.PublicName();
        var secondName = secondChat.PublicName();

        // Head = first, deterministically (§2.5): its half is `self`, the other's is `correspondent`,
        // which is the only pairing CorrespondenceGraph.BuildEdges can orient — and it orients the same
        // way after every save, since the direction is decided here once and then persisted.
        var firstHalf = firstChat.Link(secondChat, secondChat.PublicRole, purpose, introducedBy: introducer);
        var secondHalf = secondChat.Correspondences.First(
            c => string.Equals(c.ChatId, firstChat.ChatId, StringComparison.Ordinal));

        var carried =
            $"{introducer} has introduced you to {secondName} and asked me to pass this on:\n\n{text}\n\n" +
            $"(Use '{firstHalf.ToolName}' to answer {secondName} directly. {introducer} is not part of " +
            $"this conversation and will not see it.)";

        var delivered = secondChat.SendReply(secondHalf.Role, secondHalf.InstanceNo, carried);
        if (!delivered.Delivered)
            return Refuse(
                delivered.Outcome == ReplyOutcome.Denied
                    ? SPLA.Domain.Agent.IntroduceOutcome.Denied
                    : SPLA.Domain.Agent.IntroduceOutcome.AddresseeGone,
                $"error: the address is open on both sides, but the introduction could not be delivered " +
                $"to '{firstName}': {delivered.Reason}");

        return new SPLA.Domain.Agent.IntroduceResult(
            SPLA.Domain.Agent.IntroduceOutcome.Introduced,
            $"introduced: '{firstName}' and '{secondName}' now correspond, and your message reached " +
            $"{firstName}. This is a receipt, not a conversation — you are not on that edge, you will " +
            $"not see what they say, and neither of them is answering you. Carry on with your own work.");

        SPLA.Domain.Agent.IntroduceResult Refuse(SPLA.Domain.Agent.IntroduceOutcome outcome, string message)
            => new(outcome, message);

        // Same two alphabets agent_correspond lists (see UnknownAddressee) minus the roles: a role is
        // not an answer here, since an introduction takes two chats that already exist.
        SPLA.Domain.Agent.IntroduceResult Unknown(string name)
        {
            var names = _runtime.ChatManager.ListChats()
                .Where(s => s.AsInstance is > 0 && !string.Equals(s.Id, ChatId, StringComparison.Ordinal))
                .Select(s => ReplyToolNaming.BuildPublicName(
                    string.IsNullOrWhiteSpace(s.As) ? SPLA.Domain.Settings.ChatManager.DefaultPublicRole : s.As!,
                    s.AsInstance!.Value))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var archived = _runtime.ChatManager.ListArchivedChats().Any(
                s => s.AsInstance is > 0 &&
                     string.Equals(
                         ReplyToolNaming.BuildPublicName(
                             string.IsNullOrWhiteSpace(s.As) ? SPLA.Domain.Settings.ChatManager.DefaultPublicRole : s.As!,
                             s.AsInstance!.Value),
                         name, StringComparison.OrdinalIgnoreCase));

            return archived
                ? Refuse(SPLA.Domain.Agent.IntroduceOutcome.AddresseeGone,
                    $"error: '{name}' is archived — an archived chat cannot take up a new correspondence")
                : Refuse(SPLA.Domain.Agent.IntroduceOutcome.UnknownAddressee,
                    $"error: '{name}' is not a chat you can introduce. An introduction takes two existing " +
                    $"chats named by their public names, not roles. Chats: " +
                    $"{(names.Count == 0 ? "none yet" : string.Join(", ", names))}");
        }
    }

    public ChatRuntime(AgentRuntime runtime, ChatSession chat, ChatRegistry? registry = null)
    {
        _runtime = runtime;
        _chat = chat;
        _registry = registry;
        _logger = runtime.LoggerFactory.CreateLogger<ChatRuntime>();

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
                ScopeMarker = m.ScopeMarker,
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
            // Re-link the persisted sidecar images so they survive re-saves and show on reopen. The
            // labels come back onto the live message too: a reopened chat that keeps talking must send
            // its earlier pictures under the same names the model already answered about, or every
            // "Image 2" in the history quietly loses its referent.
            if (m.Images is { Count: > 0 })
            {
                _imageFiles[msg] = m.Images.Select(i => i.Clone()).ToList();
                // Read back from the sidecar as data URLs — the picture itself, not the /chat-image
                // address a browser fetches, which would mean nothing to the provider. An image whose
                // file has gone stays out of the turn rather than travelling as a broken reference.
                var restored = m.Images
                    .Select(i => (Data: ChatImages.ReadDataUrl(_runtime.Settings.Project, chat.Id, i.File), i.Label))
                    .Where(i => i.Data != null)
                    .Select(i => new ImageAttachment(i.Data!, i.Label))
                    .ToList();
                if (restored.Count > 0) msg.Images = restored;
            }
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
        // A role's own shell_timeout_seconds, on this chat's own shell. Only when it differs: a live
        // edit of the project's value reaches plain chats through the runtime, and a role that says
        // nothing inherited the same number anyway.
        if (_roleSettings is { } role && role.ShellTimeoutSeconds != runtime.Settings.ShellTimeoutSeconds &&
            _sandbox is SPLA.Domain.Host.PassthroughSandbox chatSandbox)
            chatSandbox.SetShellSilentIdle(role.ShellTimeoutSeconds > 0
                ? TimeSpan.FromSeconds(role.ShellTimeoutSeconds) : Timeout.InfiniteTimeSpan);
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
            correspondence: this,
            // And once more: the Post link of the tool pipeline asks "how much room is left" through
            // the same ambient session, and this chat is the only thing that can answer.
            contextBudget: this,
            // A role's settings, carried where every per-call decision reads them — tools, tool-set
            // levels, trusted domains, question timeouts. Null for a plain chat = the project's, live.
            settings: _roleSettings);

        // A reopened chat is as doubtful as it was when it closed. Restored rather than recomputed:
        // what raised the flag was an arrival, and arrivals do not happen again on load.
        if (chat.Doubt.Count > 0)
            _agentSession.Doubt.Restore(chat.Doubt.Select(d => new SPLA.Domain.Security.DoubtCause(
                new SPLA.Domain.Security.DataOrigin(d.Zone, OperatorNamed: false),
                d.What,
                new DateTimeOffset(DateTime.SpecifyKind(d.At, DateTimeKind.Utc)))));

        // Wave 7б/wave 0: restore correspondences straight into the live dictionary rather than
        // through OpenCorrespondence — that method decides ToolName (and InstanceNo) fresh from "does
        // a same-role correspondent already exist", which is exactly wrong here: the persisted
        // ToolName/InstanceNo were decided once, in the past, and must come back unchanged (plan trap
        // 11) even if today's in-memory logic would compute something different.
        if (chat.Correspondences is { Count: > 0 })
        {
            // Migration for a session written before wave 0: InstanceNo/Purpose did not exist yet, so
            // a missing InstanceNo is assigned by order of appearance in the file, counted per role
            // (PLAN_20260906 §3 wave 0's migration note) — the same order ToolName's own topic-joined
            // naming was already decided in, so this reproduces the numbering that spelling implied.
            var nextInstanceByRole = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in chat.Correspondences)
            {
                var instanceNo = c.InstanceNo ?? (nextInstanceByRole.TryGetValue(c.Role, out var n) ? n + 1 : 1);
                nextInstanceByRole[c.Role] = instanceNo;

                var restored = new Correspondence
                {
                    Role = c.Role,
                    Purpose = string.IsNullOrEmpty(c.Purpose) ? c.Topic : c.Purpose,
                    InstanceNo = instanceNo,
                    ChatId = c.ChatId,
                    Initiator = string.Equals(c.Initiator, "correspondent", StringComparison.OrdinalIgnoreCase)
                        ? CorrespondenceInitiator.Correspondent : CorrespondenceInitiator.Self,
                    IntroducedBy = c.IntroducedBy,
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
                if (restored.IsOpen) _correspondences[(restored.Role, restored.InstanceNo)] = restored;
                else _ended.Add(restored);
            }
        }

        // A role's narrowing and widening both come from _agentSession.Settings, read by the shared host
        // itself (see ChatToolHost's own comment). Kept as a field (not built inline for the
        // orchestrator) so AvailableToolNames can inspect the exact same surface without standing up a
        // second one.
        _toolHost = new ChatToolHost(runtime.McpHost, this);
        _orchestrator = new ConversationOrchestrator(runtime.Llm, _toolHost)
        {
            // Live context surface, recomposed on every iteration inside this turn's
            // AgentSessionScope — which is what lets runtime-wide contributors read this chat's
            // active skill and working memory. Settings and plugin edits made since the chat opened
            // apply immediately, and — the reason it is per-iteration — a skill the model activates
            // mid-turn has its procedure in the prompt for the very next LLM call rather than for the
            // next user message. Goes through this chat's own ComposeContext (not runtime.ComposeContext
            // directly) so the mode preamble names THIS chat's resolved mode — its own override, or a
            // role's — rather than always the project default.
            Context = ComposeContext,
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
                if (_pendingEchoes.Remove(message)) Emit(new ChatUserMessage(message) { ChatId = ChatId });
            },
            Checkpoint = _checkpoint,
            // Anti-repeat guard is a per-project setting (agent: loop_guard, default off) — it targets
            // small local models that loop forever, but false-fires on legitimate poll/wait patterns.
            // Only the tool-call guard exists; the error guard waits on a typed ToolResult (debt #4).
            EnableLoopGuard = EffectiveSettings.LoopGuard,
            ToolLoopWindow = EffectiveSettings.LoopGuardRepeats,
            Logger = runtime.LoggerFactory.CreateLogger<ConversationOrchestrator>()
        };

        // Chat-lifetime feed subscriptions — wave 0's "чат сам публикует... узлы прогресса..., изменения
        // задач" (ADR §4.1). One subscription each, for the chat's whole life, exactly the shape
        // SplaServiceHost.WireChatProgress and its Tasks.Changed/Asks sibling used to set up per chat
        // from the outside; now the chat does it for itself, so a wire subscriber (or any other
        // observer) only has to subscribe to ONE stream to see all of it.
        _onProgressNodeChanged = (treeId, node) => Emit(new ChatProgressNode(treeId, node) { ChatId = ChatId });
        Progress.NodeChanged += _onProgressNodeChanged;

        _onTaskChanged = record => Emit(new ChatTaskChanged(record) { ChatId = ChatId });
        Tasks.Changed += _onTaskChanged;

        // Asks live on the PROJECT's runtime (PendingAskStore's own comment: a question belongs to the
        // chat, not the connection, but the store itself is shared by every chat of the project) — so
        // this chat filters the project-wide stream down to its own chat id before republishing.
        _onAskRaised = ask => { if (ask.ChatId == ChatId) Emit(new ChatAskRaised(ask) { ChatId = ChatId }); };
        _onAskResolved = (ask, reason) => { if (ask.ChatId == ChatId) Emit(new ChatAskResolved(ask, reason) { ChatId = ChatId }); };
        runtime.Asks.Asked += _onAskRaised;
        runtime.Asks.Resolved += _onAskResolved;
    }

    /// <summary>The conversation's display messages (system prompt hidden). Hosts project these to
    /// their own wire shapes; persisted sidecar image filenames come from <see cref="ImageFilesFor"/>.</summary>
    public IEnumerable<ChatMessage> DisplayMessages
        => _conversation.Messages.Where(m => m.Role != ChatRole.System && m.ScopeMarker == null);

    /// <summary>Sidecar images persisted for a message — file name and label — or null when it has none.</summary>
    public IReadOnlyList<ChatSessionImage>? ImageFilesFor(ChatMessage message)
        => _imageFiles.TryGetValue(message, out var files) && files.Count > 0 ? files : null;

    /// <summary>Writes the message's data-URL images to sidecar files, keeping each one's name.</summary>
    private void PersistImages(ChatMessage message, IReadOnlyList<ImageAttachment> images)
    {
        var project = _runtime.Settings.Project;
        var stored = new List<ChatSessionImage>();
        foreach (var image in images)
        {
            try
            {
                var name = ChatImages.WriteDataUrl(project, _chat.Id, image.Url);
                if (name != null) stored.Add(new ChatSessionImage(name, image.Label));
            }
            catch { /* a bad image must not break the turn */ }
        }
        if (stored.Count > 0) _imageFiles[message] = stored;
    }

    /// <summary>
    /// Runs one turn: appends the user message, drives the agent loop, and persists the chat. The
    /// permission and clarify handlers come from the client connection so prompts surface in that
    /// client's UI; the turn's events reach observers only through <see cref="Feed"/> — ADR_20260910-2
    /// wave 3: no caller-supplied <see cref="AgentCallbacks"/> anymore, every subscriber (console,
    /// <c>chat run</c>, the wire) attaches to the feed instead.
    /// </summary>
    public async Task SendAsync(
        string? text,
        Func<ToolFunctionDefinition, string, Task<PermissionDecision>> permissionHandler,
        Func<ClarifyRequest, Task<string?>> clarifyHandler,
        CancellationToken cancellationToken,
        IReadOnlyList<ImageAttachment>? images = null)
    {
        // Counted here — synchronously, before the first await — so a caller that hands this task to a
        // host can broadcast "this chat is busy" the instant it starts it, with no window in which the
        // chat still claims to be idle.
        Interlocked.Increment(ref _turnsInFlight);
        try { await _turnGate.WaitAsync(cancellationToken); }
        catch { Interlocked.Decrement(ref _turnsInFlight); throw; }

        var cancelled = false;
        string? turnError = null;
        try
        {
            Emit(new ChatTurnStarted(text) { ChatId = ChatId });

            // The turn's surface, wave-4-style (see RefreshCorrespondences' own comment): a dead
            // correspondent is struck and announced before this turn's context is assembled, so a
            // stale reply_* tool (once wave 5 adds it) never outlives the chat it pointed at by more
            // than one turn.
            RefreshCorrespondences();

            // The one adapter "AgentCallbacks -> feed" ADR_20260910-2 wave 0 asks for: this chat's own
            // bookkeeping (tree registration, usage recording, the live partial) happens here as the
            // chat's own work, unconditionally, and every hook publishes onto Feed. Wave 3: no more
            // caller-supplied AgentCallbacks folded in on top — every observer (console, `chat run`,
            // the wire) is a Feed subscriber now, so this adapter is the only place that still builds
            // one, purely to satisfy ConversationOrchestrator.RunAsync's signature.
            var currentMsgIndex = 0;

            var adapter = new AgentCallbacks
            {
                OnLlmTurnStart = context =>
                {
                    CaptureLastContext(context);
                    // Indices come from the CHAT (see NextBubbleIndex's own comment) — not from this
                    // one turn — so two live bubbles in the same chat never collide.
                    currentMsgIndex = NextBubbleIndex();
                    Emit(new ChatLlmCallStarted(currentMsgIndex, context, CurrentTurnTreeId) { ChatId = ChatId });
                    return Task.CompletedTask;
                },
                OnDelta = chunk =>
                {
                    Emit(new ChatDelta(currentMsgIndex, chunk) { ChatId = ChatId },
                        () => { lock (_liveGate) _liveContent.Append(chunk); });
                    return Task.CompletedTask;
                },
                OnReasoning = chunk =>
                {
                    Emit(new ChatReasoning(currentMsgIndex, chunk) { ChatId = ChatId },
                        () => { lock (_liveGate) _liveReasoning.Append(chunk); });
                    return Task.CompletedTask;
                },
                OnAssistantMessage = msg =>
                {
                    Emit(new ChatAssistantMessage(currentMsgIndex, msg) { ChatId = ChatId }, ClearLive);
                    return Task.CompletedTask;
                },
                OnAttempt = attempt => Emit(new ChatAttempt(currentMsgIndex, attempt) { ChatId = ChatId }),
                OnToolCallStarted = tc =>
                {
                    Emit(new ChatToolStarted(tc) { ChatId = ChatId });
                    return Task.CompletedTask;
                },
                OnToolProgress = (tc, progress) => Emit(new ChatToolProgress(tc, progress) { ChatId = ChatId }),
                // Registers the turn's tree into the chat-wide hub the moment the orchestrator creates
                // it. Node changes themselves are NOT emitted from here — this chat already republishes
                // every Progress.NodeChanged tick onto Feed for the chat's whole life (see the
                // constructor), turn tree and background task tree alike, so doing it again per-turn
                // would double-deliver every node this same tree already reports through that path.
                OnProgressTree = tree => CurrentTurnTreeId = Progress.Register(tree),
                OnToolResult = (tc, result) =>
                {
                    Emit(new ChatToolResult(tc, result) { ChatId = ChatId });
                    return Task.CompletedTask;
                },
                OnNotice = note =>
                {
                    Emit(new ChatNotice(note) { ChatId = ChatId });
                    return Task.CompletedTask;
                },
                OnLlmTurn = turn =>
                {
                    RecordUsage(turn);
                    Emit(new ChatLlmTurn(turn, _budgetWindow) { ChatId = ChatId });
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
                // MsgId exists only after Add — the Add itself runs inside Emit's gate (wave 1) so a
                // subscriber snapshotting concurrently sees this message and this event as one atomic
                // change, never one without the other. Publishing lets a subscriber anchor rewind/fork
                // on this message; replaces the onUserMessage parameter this method used to take (wave 0).
                Emit(new ChatUserMessage(userMsg) { ChatId = ChatId }, () => _conversation.Add(userMsg));
                if (images is { Count: > 0 }) PersistImages(userMsg, images);
                Save();
            }

            // The seeded system message is a placeholder from here on: the orchestrator's SystemPrompt
            // provider re-renders it on every iteration, inside the session scope. Refreshing it here
            // would be both redundant and too early — the scope that carries this chat's active skill
            // opens a few lines below.

            // Live loop-guard setting: a toggle in Settings applies to the very next turn.
            _orchestrator.EnableLoopGuard = EffectiveSettings.LoopGuard;
            _orchestrator.ToolLoopWindow = Math.Max(2, EffectiveSettings.LoopGuardRepeats);

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

            // The window, once per turn (cached with a TTL by the runtime), and the occupancy on
            // every model answer inside it. Together they are what the tool pipeline's Post link
            // needs to know whether a result will fit — see IContextBudgetHost. Failing to resolve
            // the window is not an error: the budget then stays null and results pass through
            // untrimmed, which is the honest behaviour when nothing is known.
            try { _budgetWindow = await GetContextLengthAsync(cancellationToken); }
            catch { _budgetWindow = null; }

            try
            {
                await _orchestrator.RunAsync(
                    _conversation, llm, ResolveMode(), adapter, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
                throw;
            }
            catch (Exception ex)
            {
                turnError = ex.Message;
                throw;
            }

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
            // A turn that was cancelled or failed mid-stream never reached OnAssistantMessage, so its
            // half-said sentence would otherwise be offered to every client opening this chat from
            // now on, as if the model were still speaking it. Cleared inside Emit's gate (wave 1) —
            // same reasoning as the user-message Add above — so a concurrent snapshot never sees a
            // live partial that "turn complete" already says is gone.
            // Wave 0: the chat announces its own end of turn — this used to be ChatTurnDriver's job
            // (MessageTypes.TurnComplete), built from the outside after SendAsync returned or threw.
            // Emitted from the SAME finally that clears Live/releases the gate, so "turn complete" on
            // the feed and "chat no longer busy" (IsTurnRunning, one line below) become true together —
            // no window in which a late subscriber sees one but not the other.
            Emit(new ChatTurnCompleted(cancelled, turnError, ActiveSkillId) { ChatId = ChatId }, ClearLive);
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
        foreach (var m in _conversation.PersistableWith(EffectiveSettings.SaveToolCalls, EffectiveSettings.SaveAttempts))
        {
            count++;
            if (m.MsgId == msgId) return count;
        }
        return -1;
    }

    /// <summary>Persists the conversation and session KV back to the chat store.</summary>
    public void Save()
    {
        var saveToolCalls = EffectiveSettings.SaveToolCalls;
        var saveAttempts = EffectiveSettings.SaveAttempts;
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
                ScopeMarker = m.ScopeMarker,
                PromptTokens = m.PromptTokens,
                CompletionTokens = m.CompletionTokens,
                Images = _imageFiles.TryGetValue(m, out var files) && files.Count > 0
                    ? files.Select(f => f.Clone()).ToList()
                    : null,
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
                        DurationMs = (long)a.Duration.TotalMilliseconds,
                        WaitMs = a.Wait is { } w ? (long)w.TotalMilliseconds : null,
                        WaitStated = a.WaitStated
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
                // Mirrored into the legacy "topic" field too (not just "purpose"/"instance_no") so
                // anything still reading it — CorrespondenceGraph's display field — keeps working
                // unchanged (ChatSessionCorrespondence.Topic's own comment).
                Topic = c.Purpose,
                Purpose = c.Purpose,
                InstanceNo = c.InstanceNo,
                ChatId = c.ChatId,
                Initiator = c.Initiator == CorrespondenceInitiator.Correspondent ? "correspondent" : "self",
                IntroducedBy = c.IntroducedBy,
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

    // ── Context budget (IContextBudgetHost) ────────────────────────────────────
    //
    // Two numbers, both measured, both refreshed once per model call: the window this chat's
    // connection actually has, and what the last request actually occupied in it. Nothing here is
    // estimated — see ContextBudget's own note on why that matters.

    private int? _budgetWindow;
    private int? _budgetUsed;

    /// <summary><see cref="SPLA.Domain.Agent.IContextBudgetHost.Budget"/> — what the Post link of the
    /// tool pipeline reads to decide whether a result will fit. Null until BOTH numbers exist: before
    /// the first answer of a turn there is no measured occupancy, and inventing one would make every
    /// early result look either free or doomed.</summary>
    public SPLA.Domain.Agent.ContextBudget? Budget =>
        _budgetWindow is int window and > 0 && _budgetUsed is int used
            ? new SPLA.Domain.Agent.ContextBudget(window, used)
            : null;

    /// <summary>Records what the provider counted for the call that just returned. The occupancy of
    /// the NEXT request is at least this — the conversation only grows within a turn — which is
    /// exactly the question a tool result about to be appended raises.</summary>
    private void RecordUsage(SPLA.Domain.Llm.LlmTurnResult turn)
    {
        if (turn.Message.PromptTokens is int prompt and > 0)
            _budgetUsed = prompt + (turn.Message.CompletionTokens ?? 0);
    }

    /// <summary>What this chat's model will let a caller do with its reasoning channel — what the
    /// status bar draws its lever from, and what gates the wire mapping on a turn.</summary>
    public Task<ReasoningCapability> GetReasoningAsync(CancellationToken ct = default)
    {
        var settings = _roleSettings ?? _runtime.Settings;
        var entry = settings.FindModel(_chat.ModelId) ?? settings.DefaultModel;
        return _runtime.GetReasoningAsync(ResolveLlmSettings(), entry?.DeclaredReasoning, ct);
    }

    /// <summary>The chat's effective temperature — its own override, else the role's, else the
    /// resolved model's own default, else the project/machine default. See
    /// <see cref="ResolvedSettings.ToLLMSettings(ResolvedModelEntry?)"/> for the same precedence.</summary>
    public double Temperature => ResolveLlmSettings().Temperature;

    /// <summary>The chat's effective reasoning selection, in the scalar grammar. Empty = model default.</summary>
    public string ReasoningLevel =>
        string.IsNullOrEmpty(_chat.Model?.ReasoningLevel)
            ? (_roleSettings ?? _runtime.Settings).ReasoningLevel ?? ""
            : _chat.Model!.ReasoningLevel!;

    /// <summary>The chat's effective LLM settings: its model entry (endpoint/model) layered with its
    /// own behaviour knobs (temperature/reasoning/penalties), falling back to the role's settings when
    /// this chat has one (<see cref="_roleSettings"/>), then the project defaults.</summary>
    private LLMSettings ResolveLlmSettings()
    {
        var settings = _roleSettings ?? _runtime.Settings;
        var entry = settings.FindModel(_chat.ModelId) ?? settings.DefaultModel;
        var s = settings.ToLLMSettings(entry);
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

    /// <summary>See <see cref="PeerDebounceBase"/>. Null means disabled — see
    /// <see cref="SPLA.Domain.Settings.SplaAgentSection.SelfFeedingCap"/>.</summary>
    public int? SelfFeedingCap => _roleSettings?.SelfFeedingCap ?? _runtime.Settings.SelfFeedingCap;

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

        // The project-wide Asks store outlives this chat, so its subscriptions must be dropped
        // explicitly — the same leak Progress/Tasks don't have (both are owned by this instance and
        // die with it either way, but unsubscribing them too costs nothing and reads the same).
        _runtime.Asks.Asked -= _onAskRaised;
        _runtime.Asks.Resolved -= _onAskResolved;
        Progress.NodeChanged -= _onProgressNodeChanged;
        Tasks.Changed -= _onTaskChanged;

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
