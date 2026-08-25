using Microsoft.Extensions.Logging;
using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.Library.Catalog;
using SPLA.MCP.Core.Permissions;

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
public sealed class ChatRuntime : IDisposable, SPLA.Domain.Agent.IBackgroundTaskHost
{
    private readonly AgentRuntime _runtime;

    /// <summary>This chat's own boundary — the project's workspace and gate, its own shell. Owned,
    /// and therefore ended in <see cref="Dispose"/>.</summary>
    private readonly SPLA.Domain.Host.ISandbox _sandbox;

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

    private readonly ChatSession _chat;
    private readonly Conversation _conversation = new();
    private readonly KeyValueStore _sessionKv = new("session");
    private readonly SkillSession _skillSession = new();
    private readonly ToolSetSession _toolSetSession = new();
    private readonly CheckpointManager _checkpoint = new();
    private readonly AgentSession _agentSession;
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

    public ChatRuntime(AgentRuntime runtime, ChatSession chat)
    {
        _runtime = runtime;
        _chat = chat;

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
            background: this);

        // A reopened chat is as doubtful as it was when it closed. Restored rather than recomputed:
        // what raised the flag was an arrival, and arrivals do not happen again on load.
        if (chat.Doubt.Count > 0)
            _agentSession.Doubt.Restore(chat.Doubt.Select(d => new SPLA.Domain.Security.DoubtCause(
                new SPLA.Domain.Security.DataOrigin(d.Zone, OperatorNamed: false),
                d.What,
                new DateTimeOffset(DateTime.SpecifyKind(d.At, DateTimeKind.Utc)))));
        _orchestrator = new ConversationOrchestrator(runtime.Llm, runtime.McpHost)
        {
            // Live context surface, recomposed on every iteration inside this turn's
            // AgentSessionScope — which is what lets runtime-wide contributors read this chat's
            // active skill and working memory. Settings and plugin edits made since the chat opened
            // apply immediately, and — the reason it is per-iteration — a skill the model activates
            // mid-turn has its procedure in the prompt for the very next LLM call rather than for the
            // next user message.
            Context = runtime.ComposeContext,
            DrainInbox = Inbox.DrainAll,
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

    /// <summary>The chat's mode (from its agent section), falling back to the project default.</summary>
    private AgentMode ResolveMode()
        => _chat.Agent?.Mode != null && Enum.TryParse<AgentMode>(_chat.Agent.Mode, true, out var m)
            ? m : _runtime.Settings.Mode;

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
