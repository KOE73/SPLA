using Microsoft.Extensions.Logging;
using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Permissions;

namespace SPLA.Service;

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
public sealed class ChatRuntime
{
    private readonly AgentRuntime _runtime;
    private readonly ChatSession _chat;
    private readonly Conversation _conversation = new();
    private readonly KeyValueStore _sessionKv = new("session");
    private readonly SkillSession _skillSession = new();
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

    /// <summary>This chat's session-scoped working memory entries (for the debug inspector).</summary>
    public IEnumerable<(string Key, string Value)> SessionKvEntries
        => _sessionKv.List().Select(e => (e.Key, e.Value));

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

    /// <summary>The connection this chat points at, if any.</summary>
    public string? ConnectionId => _chat.ConnectionId;

    /// <summary>Changes the chat's mode and/or connection (null = leave as-is) and persists it.</summary>
    public void ApplySettings(string? mode, string? connectionId)
    {
        if (!string.IsNullOrWhiteSpace(mode))
        {
            _chat.Agent ??= new SplaAgentSection();
            _chat.Agent.Mode = mode;
        }
        if (connectionId != null) _chat.ConnectionId = connectionId;
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
                Reasoning = string.IsNullOrEmpty(m.Reasoning) ? null : m.Reasoning
            };
            _conversation.Add(msg);
            // Re-link persisted sidecar image filenames so they survive re-saves and show on reopen.
            if (m.Images is { Count: > 0 }) _imageFiles[msg] = new List<string>(m.Images);
        }

        // Restore this chat's session memory (survives restart) and feed live context:* each turn.
        _sessionKv.LoadFrom(chat.Kv);

        _agentSession = new AgentSession(_sessionKv, _checkpoint, _skillSession);
        _orchestrator = new ConversationOrchestrator(runtime.Llm, runtime.McpHost)
        {
            WorkingMemory = () => CollectWorkingMemory(_sessionKv, runtime.ProjectKv.Store),
            Checkpoint = _checkpoint,
            Logger = runtime.LoggerFactory.CreateLogger<ConversationOrchestrator>()
        };
    }

    /// <summary>The conversation's display messages projected to wire DTOs (system prompt hidden).
    /// Persisted image filenames are surfaced as /chat-image URLs so reopened chats show their pictures.</summary>
    public List<Contracts.ChatMessageDto> SnapshotMessages()
        => _conversation.Messages
            .Where(m => m.Role != ChatRole.System)
            .Select(m =>
            {
                var dto = ProtocolMapper.ToDto(m);
                if (_imageFiles.TryGetValue(m, out var files) && files.Count > 0)
                    dto.Images = files.Select(f => ChatImages.Url(_chat.Id, f)).ToList();
                return dto;
            })
            .ToList();

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
        string text,
        AgentCallbacks callbacks,
        Func<ToolFunctionDefinition, string, Task<PermissionDecision>> permissionHandler,
        Func<ClarifyRequest, Task<string?>> clarifyHandler,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? images = null)
    {
        await _turnGate.WaitAsync(cancellationToken);
        try
        {
            var userMsg = new ChatMessage
            {
                Role = ChatRole.User,
                Content = text,
                // Data URLs stay in memory for the LLM this turn; the sidecar files below are what persist.
                Images = images is { Count: > 0 } ? images.ToList() : null
            };
            _conversation.Add(userMsg);
            if (images is { Count: > 0 }) PersistImages(userMsg, images);
            Save();

            using var clarifyScope = ClarifyScope.Begin(clarifyHandler);
            using var agentScope = AgentSessionScope.Begin(_agentSession);
            using var permScope = PermissionScope.Begin(permissionHandler);

            // Per-chat resolution: each chat runs against its own connection (endpoint/model) and its
            // own mode, not the project default — mirrors the UI's BuildLlmSettings so the service
            // honours the per-chat autonomy model rather than forcing every chat onto project settings.
            await _orchestrator.RunAsync(
                _conversation, ResolveLlmSettings(), ResolveMode(), callbacks, cancellationToken);

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
        }
    }

    /// <summary>Persists the conversation and session KV back to the chat store.</summary>
    public void Save()
    {
        _chat.Messages.Clear();
        foreach (var m in _conversation.Persistable)
        {
            _chat.Messages.Add(new ChatSessionMessage
            {
                Role = m.Role.ToString().ToLower(),
                Content = m.Content ?? "",
                Reasoning = string.IsNullOrEmpty(m.Reasoning) ? null : m.Reasoning,
                Images = _imageFiles.TryGetValue(m, out var files) && files.Count > 0 ? new List<string>(files) : null
            });
        }
        _chat.Kv = _sessionKv.Snapshot();
        _runtime.ChatManager.SaveChat(_chat);
    }

    /// <summary>The operative context window for this chat's connection (tokens), or null when
    /// unknown. Cached by the runtime with a short TTL; cheap to call once per turn. Lets the turn
    /// path report "prompt tokens vs window" so the UI can warn before the provider rejects.</summary>
    public Task<int?> GetContextLengthAsync(CancellationToken ct = default)
        => _runtime.GetContextLengthAsync(ResolveLlmSettings(), ct);

    /// <summary>The chat's effective LLM settings: its connection (endpoint/model) layered with its
    /// own behaviour knobs (temperature/reasoning/penalties), falling back to project defaults.</summary>
    private LLMSettings ResolveLlmSettings()
    {
        var conn = _runtime.Settings.Connections.FirstOrDefault(c => c.Id == _chat.ConnectionId)
                   ?? _runtime.Settings.Connections.FirstOrDefault();
        var s = _runtime.Settings.ToLLMSettings(conn);
        var chatModel = _chat.Model;

        s.Mode             = ResolveMode();
        s.Temperature      = chatModel?.Temperature      ?? s.Temperature;
        s.ReasoningLevel   = string.IsNullOrEmpty(chatModel?.ReasoningLevel) ? s.ReasoningLevel : chatModel!.ReasoningLevel;
        s.PresencePenalty  = chatModel?.PresencePenalty  ?? s.PresencePenalty;
        s.FrequencyPenalty = chatModel?.FrequencyPenalty ?? s.FrequencyPenalty;
        s.RepeatPenalty    = chatModel?.RepeatPenalty    ?? s.RepeatPenalty;
        return s;
    }

    /// <summary>The chat's mode (from its agent section), falling back to the project default.</summary>
    private AgentMode ResolveMode()
        => _chat.Agent?.Mode != null && Enum.TryParse<AgentMode>(_chat.Agent.Mode, true, out var m)
            ? m : _runtime.Settings.Mode;

    private static IReadOnlyList<(string scope, string key, string value)> CollectWorkingMemory(
        IKeyValueStore session, IKeyValueStore project)
        => session.List().Select(kv => (session.Scope, kv.Key, kv.Value))
            .Concat(project.List().Select(kv => (project.Scope, kv.Key, kv.Value)))
            .ToList();
}
