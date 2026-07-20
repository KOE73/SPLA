using SPLA.Runtime;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SPLA.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Project;
using SPLA.Domain.Tools;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// One connected client over a WebSocket. Owns only the connection's own concerns: transport
/// (framing, the receive loop, the send lock), the cross-cutting auth gate, and the correlation
/// tables that let a running turn ask the initiating client a permission/clarify question and await
/// the answer without blocking the socket. Business logic for each message type lives in a
/// per-module <see cref="IMessageHandler"/> reached through <see cref="MessageRouter"/>; the
/// connection exposes exactly what handlers need via <see cref="IClientSession"/>, so a new message
/// type is a new handler and never edits this class.
/// <para>
/// Multi-project: there is no "current project" stored on the connection. Every project- or
/// chat-scoped envelope carries its own <see cref="ProtocolEnvelope.ProjectId"/> (null meaning the
/// connection's default project), and each message resolves the runtime it needs fresh via
/// <see cref="Resolve"/> — a socket that touches several projects never has ambiguous state to drift.
/// </para>
/// </summary>
public sealed class ClientConnection : IClientSession
{
    private readonly WebSocket _socket;
    private readonly AgentRuntimeRegistry _registry;
    private readonly ConnectionHub _hub;
    private readonly AuthGate _auth;
    private readonly ILogger _log;
    private readonly SPLA.Domain.Identity.IIdentity _identity;
    private readonly MessageRouter _router;
    private readonly InitialChatRequest? _initialChat;

    /// <summary>This connection's live SSH terminals (phase B). Created here, torn down in the
    /// receive-loop finally so a dropped socket closes every SSH session it opened.</summary>
    private readonly SshTerminalManager _terminals;
    private readonly PluginPanelManager _pluginPanels;

    /// <summary>Per-user project scope (server mode): this connection lists/defaults to the projects in
    /// the authenticated user's own area, not the process-global set. Null in local/embedded mode.</summary>
    private readonly IProjectProvider? _userProvider;
    private readonly string? _userDefaultProjectId;
    /// <summary>The user's own storage area root (server mode) — new projects are created inside it
    /// from just a name, so a browser client never picks a server filesystem path.</summary>
    private readonly string? _userArea;

    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private readonly ConcurrentDictionary<string, TaskCompletionSource<PermissionDecision>> _pendingPermissions = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string?>> _pendingClarifies = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeTurns = new();
    private readonly ConcurrentDictionary<string, byte> _openChats = new();

    /// <summary>Set once a successful <see cref="MessageTypes.Hello"/> handshake has run. When the
    /// server requires a connect token (<see cref="AuthGate.RequiresToken"/>), every non-Hello frame
    /// received before this is rejected and the socket is closed — otherwise a client could skip the
    /// handshake and drive the agent past the token check (the token was only ever validated inside
    /// <see cref="HandleHelloAsync"/>).</summary>
    private bool _authenticated;

    /// <summary>Every project id this connection has touched — the project-level analogue of
    /// <see cref="_openChats"/>, used to scope broadcasts so a client only watching project A never
    /// receives project B's settings/usage/chat-list results.</summary>
    private readonly ConcurrentDictionary<string, byte> _openProjects = new();

    /// <summary>True when this connection currently has <paramref name="chatId"/> open (a watcher of it).</summary>
    public bool IsWatching(string chatId) => _openChats.ContainsKey(chatId);

    /// <summary>True when this connection has touched <paramref name="projectId"/> at least once.</summary>
    public bool IsWatchingProject(string projectId) => _openProjects.ContainsKey(projectId);

    public ClientConnection(
        WebSocket socket, AgentRuntimeRegistry registry, ConnectionHub hub, AuthGate auth, ILogger log,
        SPLA.Domain.Identity.IIdentity? identity = null,
        IProjectProvider? userProvider = null, string? userDefaultProjectId = null, string? userArea = null)
        : this(socket, registry, hub, auth, log, identity, userProvider, userDefaultProjectId, userArea, null)
    {
    }

    internal ClientConnection(
        WebSocket socket, AgentRuntimeRegistry registry, ConnectionHub hub, AuthGate auth, ILogger log,
        SPLA.Domain.Identity.IIdentity? identity,
        IProjectProvider? userProvider, string? userDefaultProjectId, string? userArea,
        InitialChatRequest? initialChat)
    {
        _socket = socket;
        _registry = registry;
        _hub = hub;
        _auth = auth;
        _log = log;
        _identity = identity ?? SPLA.Domain.Identity.LocalIdentity.Single;
        _userProvider = userProvider;
        _userDefaultProjectId = userDefaultProjectId;
        _userArea = userArea;
        _initialChat = initialChat;
        _router = MessageRouter.Default;
        _terminals = new SshTerminalManager((type, payload) => SendAsync(type, payload), log);
        _pluginPanels = new PluginPanelManager((type, payload) => SendAsync(type, payload));
    }

    // ── IClientSession (the surface handlers act through) ─────────────────────

    SPLA.Domain.Identity.IIdentity IClientSession.Identity => _identity;
    ConnectionHub IClientSession.Hub => _hub;
    AgentRuntimeRegistry IClientSession.Registry => _registry;
    IProjectProvider? IClientSession.UserProvider => _userProvider;
    string? IClientSession.UserArea => _userArea;

    /// <summary>The project a bare (no ProjectId) message means for THIS connection — the user's own
    /// default in server mode, else the registry's shared default.</summary>
    public string DefaultProjectId => _userDefaultProjectId ?? _registry.DefaultProjectId;

    public IReadOnlyList<ProjectDescriptor> ListProjects() => _userProvider?.List() ?? _registry.List();
    public IReadOnlyList<ProjectDescriptor> RecentProjects() => _userProvider?.Recent() ?? _registry.Recent();

    /// <summary>Resolves the runtime an envelope means: its explicit ProjectId, or the connection's
    /// default when omitted. Marks the resolved project as "touched" so later broadcasts reach it.</summary>
    public (RuntimeEntry Entry, string ProjectId) Resolve(ProtocolEnvelope env)
    {
        var projectId = string.IsNullOrEmpty(env.ProjectId) ? DefaultProjectId : env.ProjectId;
        _openProjects[projectId] = 0;
        return (_registry.Open(projectId), projectId);
    }

    SshTerminalManager IClientSession.Terminals => _terminals;
    PluginPanelManager IClientSession.PluginPanels => _pluginPanels;

    void IClientSession.MarkProjectOpen(string projectId) => _openProjects[projectId] = 0;
    void IClientSession.MarkChatOpen(string chatId) => _openChats[chatId] = 0;

    void IClientSession.StartTurn(
        AgentRuntime runtime, string projectId, ChatRuntime chat, string text, List<string>? images,
        CancellationToken hostStopping)
        // Run the turn in the background so the receive loop keeps serving this turn's
        // permission/clarify answers and its cancel request.
        => _ = RunTurnAsync(runtime, projectId, chat, text, images, hostStopping);

    bool IClientSession.TryCancelTurn(string chatId)
    {
        if (_activeTurns.TryGetValue(chatId, out var cts)) { cts.Cancel(); return true; }
        return false;
    }

    bool IClientSession.CompletePermission(string requestId, PermissionDecision decision)
        => _pendingPermissions.TryRemove(requestId, out var tcs) && tcs.TrySetResult(decision);

    bool IClientSession.CompleteClarify(string requestId, string? choice)
        => _pendingClarifies.TryRemove(requestId, out var tcs) && tcs.TrySetResult(choice);

    // ── Connection lifecycle ─────────────────────────────────────────────────

    /// <summary>Reads and dispatches frames until the socket closes or the host stops.</summary>
    public async Task RunAsync(CancellationToken hostStopping)
    {
        _hub.Add(this);
        try
        {
            await ReceiveLoopAsync(hostStopping);
        }
        finally
        {
            _hub.Remove(this);
            // Unblock any handlers waiting on this client.
            foreach (var tcs in _pendingPermissions.Values) tcs.TrySetResult(PermissionDecision.Deny);
            foreach (var tcs in _pendingClarifies.Values) tcs.TrySetResult(null);
            foreach (var cts in _activeTurns.Values) cts.Cancel();
            // Tear down any live SSH terminals this connection opened.
            await _terminals.DisposeAsync();
            await _pluginPanels.DisposeAsync();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken hostStopping)
    {
        var buffer = new byte[64 * 1024];
        var assembled = new MemoryStream();

        while (_socket.State == WebSocketState.Open && !hostStopping.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await _socket.ReceiveAsync(buffer, hostStopping);
            }
            catch (OperationCanceledException) { break; }
            catch (WebSocketException) { break; }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                break;
            }

            assembled.Write(buffer, 0, result.Count);
            if (!result.EndOfMessage) continue;

            var text = Encoding.UTF8.GetString(assembled.GetBuffer(), 0, (int)assembled.Length);
            assembled.SetLength(0);

            try
            {
                await DispatchAsync(text, hostStopping);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to handle client message.");
                await SendAsync(MessageTypes.Error, new ErrorPayload { Message = ex.Message });
            }
        }
    }

    /// <summary>
    /// The connection's own responsibilities per frame — deserialize, the two cross-cutting concerns
    /// (token gate + handshake), then hand off to the module handler via <see cref="_router"/>.
    /// </summary>
    private async Task DispatchAsync(string json, CancellationToken hostStopping)
    {
        var env = JsonSerializer.Deserialize<ProtocolEnvelope>(json, ServiceJson.Options);
        if (env == null) return;

        // Token gate: when a connect token is required, nothing but the handshake is honoured until
        // that handshake has succeeded. Without this, only Hello validated the token — every other
        // message type was dispatched unchecked, so a client could omit Hello entirely and still act.
        if (_auth.RequiresToken && !_authenticated && env.Type != MessageTypes.Hello)
        {
            await SendAsync(MessageTypes.Error, new ErrorPayload { Message = "Unauthorized: handshake required." });
            await _socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "unauthenticated", CancellationToken.None);
            return;
        }

        // Handshake is a connection concern (it flips authentication and describes the default
        // project), so it stays here rather than in a handler.
        if (env.Type == MessageTypes.Hello)
        {
            await HandleHelloAsync(env, hostStopping);
            return;
        }

        var ctx = new RequestContext { Session = this, Env = env, HostStopping = hostStopping };
        if (!await _router.TryDispatchAsync(ctx))
            _log.LogWarning("Unknown message type: {Type}", env.Type);
    }

    private async Task HandleHelloAsync(ProtocolEnvelope env, CancellationToken hostStopping)
    {
        var decision = _auth.Authorize(env.Auth);
        if (!decision.Ok)
        {
            await SendAsync(MessageTypes.Error, new ErrorPayload { Message = "Unauthorized." });
            await _socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "unauthorized", CancellationToken.None);
            return;
        }

        // Handshake accepted — subsequent frames may be dispatched (see the token gate above).
        _authenticated = true;

        // The handshake always describes the connection's default project — a single-project client
        // never needs to know projects exist at all. In server mode this is the user's OWN default
        // project (in their area), so a domain user lands in their space, not the server's.
        var projectId = DefaultProjectId;
        _openProjects[projectId] = 0;
        var ctx = ProtocolProjection.ToContext(projectId, _registry.Open(projectId).Runtime);

        _log.LogInformation("Client connected as {User} ({Key}, {Groups} groups).",
            _identity.DisplayName, _identity.UserKey, _identity.Groups.Count);

        // Only a really-authenticated user populates the identity fields; the local/embedded sentinel
        // stays blank so the client's identity badge collapses (no "who" chrome in single-user mode).
        var authed = !ReferenceEquals(_identity, SPLA.Domain.Identity.LocalIdentity.Single);

        await SendAsync(MessageTypes.Welcome, new WelcomePayload
        {
            ActorId = decision.ActorId,
            Capabilities = decision.Capabilities,
            UserKey = authed ? _identity.UserKey : "",
            UserName = authed ? _identity.DisplayName : "",
            ProjectId = ctx.ProjectId,
            ProjectName = ctx.ProjectName,
            WorkspacePath = ctx.WorkspacePath,
            Connections = ctx.Connections,
            Modes = ctx.Modes,
            DefaultMode = ctx.DefaultMode,
            Theme = ctx.Theme,
            Density = ctx.Density
        });

        await TryStartInitialChatAsync(hostStopping);
    }

    /// <summary>Consumes the process-wide startup request once, opens the resulting chat in this
    /// client, refreshes project chat lists, then runs the first turn through this connection so all
    /// interactive prompts and streamed events have a real Web UI owner.</summary>
    private async Task TryStartInitialChatAsync(CancellationToken hostStopping)
    {
        var request = _initialChat?.Take();
        if (request == null) return;

        var projectId = DefaultProjectId;
        _openProjects[projectId] = 0;
        var entry = _registry.Open(projectId);
        var chat = entry.Chats.CreateNew(request.Title);

        await SendOpenedAsync(chat);
        await _hub.BroadcastToProjectAsync(projectId, MessageTypes.ChatListResult,
            new ChatListResultPayload { Chats = entry.Chats.List() });

        _ = RunTurnAsync(entry.Runtime, projectId, chat, request.Message, null, hostStopping);
    }

    // ── Turn execution + outbound streaming ───────────────────────────────────

    /// <summary>Hub-facing send that never throws — used for broadcasts to all/watching connections.</summary>
    public async Task TrySendAsync(string type, object? payload, string? chatId = null)
    {
        try { await SendAsync(type, payload, chatId); }
        catch { /* a single dead client must not break a broadcast */ }
    }

    public async Task SendOpenedAsync(ChatRuntime chat)
    {
        _openChats[chat.ChatId] = 0;   // this connection now watches this chat
        await SendAsync(MessageTypes.ChatOpened, new ChatOpenedPayload
        {
            ChatId = chat.ChatId,
            Title = chat.Title,
            Messages = chat.SnapshotMessages(),
            Mode = chat.ModeName,
            ConnectionId = chat.ConnectionId
        }, chat.ChatId);
    }

    private async Task RunTurnAsync(
        AgentRuntime runtime, string projectId, ChatRuntime chat, string text, List<string>? images,
        CancellationToken hostStopping)
    {
        using var turnCts = CancellationTokenSource.CreateLinkedTokenSource(hostStopping);
        _activeTurns[chat.ChatId] = turnCts;

        // Make the acting user (and chat/project) ambient for this turn's telemetry, so tool-call and
        // token measurements the collector taps can be attributed to this user for the per-user stats
        // slice. AsyncLocal flows into the orchestrator/McpHost on this same async path.
        using var telemetryScope = SPLA.Observability.SplaTelemetry.PushContext(
            new SPLA.Observability.SplaTelemetryContext(
                ConversationId: chat.ChatId, ProjectId: projectId, UserKey: _identity.UserKey));

        var ctx = new TurnContext();

        // Resolve the model's operative context window up front (cached; one cheap local call) so
        // every token.usage broadcast this turn can carry "prompt vs window" for the UI's budget bar.
        try { ctx.ContextLength = await chat.GetContextLengthAsync(turnCts.Token); }
        catch { /* unknown window — usage still reports raw counts */ }

        var callbacks = BuildCallbacks(runtime, projectId, chat, ctx);

        string? error = null;
        try
        {
            await chat.SendAsync(
                text,
                callbacks,
                (def, args) => RequestPermissionAsync(chat.ChatId, def, args, turnCts.Token),
                req => RequestClarifyAsync(chat.ChatId, req, turnCts.Token),
                turnCts.Token,
                images,
                onUserMessage: m => _ = _hub.BroadcastToWatchersAsync(chat.ChatId, MessageTypes.UserMessage,
                    new UserMessagePayload
                    {
                        MsgId = m.MsgId,
                        CreatedAt = m.CreatedAt.ToString("o"),
                        Text = text
                    }));
        }
        catch (OperationCanceledException) { /* cancelled turn — reported below */ }
        catch (Exception ex)
        {
            error = ex.Message;
            _log.LogError(ex, "Turn failed for chat {ChatId}.", chat.ChatId);
        }
        finally
        {
            _activeTurns.TryRemove(chat.ChatId, out _);
        }

        await _hub.BroadcastToWatchersAsync(chat.ChatId, MessageTypes.TurnComplete,
            new TurnCompletePayload { Cancelled = turnCts.IsCancellationRequested, Error = error });
    }

    /// <summary>
    /// Turn events fan out to every connection watching the chat (via the hub), not just the sender —
    /// so two windows on one chat both see the live stream. Permission/clarify are NOT here: those go
    /// only to the initiating connection (see RequestPermissionAsync/RequestClarifyAsync).
    /// </summary>
    private AgentCallbacks BuildCallbacks(AgentRuntime runtime, string projectId, ChatRuntime chat, TurnContext ctx)
    {
        var chatId = chat.ChatId;
        DateTime lastProgress = DateTime.MinValue;
        Task ToWatchers(string type, object payload) => _hub.BroadcastToWatchersAsync(chatId, type, payload);

        return new AgentCallbacks
        {
            OnLlmTurnStart = context =>
            {
                chat.CaptureLastContext(context);   // for the context.last debug snapshot
                ctx.CurrentMsgIndex = ctx.NextIndex++;
                return ToWatchers(MessageTypes.LlmTurnStart, new DeltaPayload { MsgIndex = ctx.CurrentMsgIndex, Text = "" });
            },
            OnDelta = chunk => ToWatchers(MessageTypes.Delta, new DeltaPayload { MsgIndex = ctx.CurrentMsgIndex, Text = chunk }),
            OnReasoning = chunk => ToWatchers(MessageTypes.Reasoning, new ReasoningPayload { MsgIndex = ctx.CurrentMsgIndex, Text = chunk }),
            OnAssistantMessage = msg => ToWatchers(MessageTypes.AssistantMessage,
                new AssistantMessagePayload { MsgIndex = ctx.CurrentMsgIndex, Message = ProtocolMapper.ToDto(msg) }),
            OnToolCallStarted = tc => ToWatchers(MessageTypes.ToolStarted, new ToolStartedPayload { ToolCall = ProtocolMapper.ToDto(tc) }),
            OnToolProgress = (tc, progress) =>
            {
                var now = DateTime.UtcNow;
                if ((now - lastProgress).TotalMilliseconds < 120 && (progress.Fraction ?? 0) < 1.0) return;
                lastProgress = now;
                _ = ToWatchers(MessageTypes.ToolProgress, new ToolProgressPayload
                {
                    ToolCallId = tc.Id,
                    ToolName = tc.Function.Name,
                    Current = progress.Current ?? 0,
                    Total = progress.Total ?? 0,
                    Fraction = progress.Fraction,
                    Message = progress.Message,
                    Details = progress.Details?.Select(d => new ToolProgressDetailDto { Label = d.Label, Value = d.Value }).ToList()
                });
            },
            OnToolResult = (tc, result) => ToWatchers(MessageTypes.ToolResult,
                new ToolResultPayload { ToolCallId = tc.Id, ToolName = tc.Function.Name, Result = result }),
            OnNotice = note => ToWatchers(MessageTypes.Notice, new NoticePayload { Text = note }),
            OnTokenUsage = (prompt, completion) =>
            {
                runtime.TokenUsageProject.Record(prompt, completion);
                runtime.TokenUsageGlobal.Record(prompt, completion);
                _ = ToWatchers(MessageTypes.TokenUsage, new TokenUsagePayload
                {
                    PromptTokens = prompt,
                    CompletionTokens = completion,
                    ContextLength = ctx.ContextLength
                });
                _ = _hub.BroadcastToProjectAsync(projectId, MessageTypes.UsageResult, SettingsOps.GetUsage(runtime));
            }
        };
    }

    private async Task<PermissionDecision> RequestPermissionAsync(
        string chatId, ToolFunctionDefinition def, string args, CancellationToken ct)
    {
        var reqId = Guid.NewGuid().ToString("N");
        var tcs = new TaskCompletionSource<PermissionDecision>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingPermissions[reqId] = tcs;

        await SendAsync(MessageTypes.PermissionRequest,
            new PermissionRequestPayload { ToolName = def.Name, Arguments = args }, chatId, reqId);

        using (ct.Register(() => { if (_pendingPermissions.TryRemove(reqId, out var t)) t.TrySetResult(PermissionDecision.Deny); }))
            return await tcs.Task;
    }

    private async Task<string?> RequestClarifyAsync(string chatId, ClarifyRequest req, CancellationToken ct)
    {
        var reqId = Guid.NewGuid().ToString("N");
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingClarifies[reqId] = tcs;

        await SendAsync(MessageTypes.ClarifyRequest, new ClarifyRequestPayload
        {
            Question = req.Question,
            Options = req.Options.Select(o => new ClarifyOptionDto { Label = o.Label, Description = o.Description }).ToList()
        }, chatId, reqId);

        using (ct.Register(() => { if (_pendingClarifies.TryRemove(reqId, out var t)) t.TrySetResult(null); }))
            return await tcs.Task;
    }

    // ── Framing ────────────────────────────────────────────────────────────────

    public async Task SendAsync(string type, object? payload, string? chatId = null, string? requestId = null)
    {
        var env = new ProtocolEnvelope
        {
            Type = type,
            ChatId = chatId,
            RequestId = requestId,
            Payload = payload == null ? null : JsonSerializer.SerializeToElement(payload, ServiceJson.Options)
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(env, ServiceJson.Options);

        await _sendLock.WaitAsync();
        try
        {
            if (_socket.State == WebSocketState.Open)
                await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>Per-turn mutable index bookkeeping for streaming assistant messages.</summary>
    private sealed class TurnContext
    {
        public int NextIndex;
        public int CurrentMsgIndex;
        /// <summary>The model's operative context window (tokens) resolved at turn start; null = unknown.</summary>
        public int? ContextLength;
    }
}
