using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SPLA.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// One connected client over a WebSocket. Translates between the wire protocol and the agent core:
/// inbound envelopes become agent actions; the orchestrator's <see cref="AgentCallbacks"/> become
/// outbound envelopes. Permission and clarify round-trips are correlated by RequestId through
/// <see cref="TaskCompletionSource{T}"/>s completed by the receive loop, so a running turn can ask
/// the very client that started it and await the answer without blocking the socket.
/// </summary>
public sealed class ClientConnection
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly WebSocket _socket;
    private readonly AgentRuntime _runtime;
    private readonly ChatRegistry _chats;
    private readonly ConnectionHub _hub;
    private readonly AuthGate _auth;
    private readonly IAgentInspector _inspector;
    private readonly ILogger _log;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private readonly ConcurrentDictionary<string, TaskCompletionSource<PermissionDecision>> _pendingPermissions = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string?>> _pendingClarifies = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeTurns = new();
    private readonly ConcurrentDictionary<string, byte> _openChats = new();

    /// <summary>True when this connection currently has <paramref name="chatId"/> open (a watcher of it).</summary>
    public bool IsWatching(string chatId) => _openChats.ContainsKey(chatId);

    public ClientConnection(
        WebSocket socket, AgentRuntime runtime, ChatRegistry chats, ConnectionHub hub,
        AuthGate auth, IAgentInspector inspector, ILogger log)
    {
        _socket = socket;
        _runtime = runtime;
        _chats = chats;
        _hub = hub;
        _auth = auth;
        _inspector = inspector;
        _log = log;
    }

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

    private async Task DispatchAsync(string json, CancellationToken hostStopping)
    {
        var env = JsonSerializer.Deserialize<ProtocolEnvelope>(json, Json);
        if (env == null) return;

        switch (env.Type)
        {
            case MessageTypes.Hello:
                await HandleHelloAsync(env);
                break;

            case MessageTypes.ChatList:
                await SendAsync(MessageTypes.ChatListResult, new ChatListResultPayload { Chats = _chats.List() });
                break;

            case MessageTypes.ChatNew:
            {
                var p = Payload<ChatNewPayload>(env) ?? new ChatNewPayload();
                var chat = _chats.CreateNew(p.Title);
                await SendOpenedAsync(chat);
                await BroadcastChatListAsync();
                break;
            }

            case MessageTypes.ChatRename:
            {
                var p = Payload<ChatRenamePayload>(env);
                if (p != null) { _chats.Rename(p.ChatId, p.Title); await BroadcastChatListAsync(); }
                break;
            }

            case MessageTypes.ChatDelete:
            {
                var p = Payload<ChatDeletePayload>(env);
                if (p != null) { _chats.Delete(p.ChatId); await BroadcastChatListAsync(); }
                break;
            }

            case MessageTypes.ChatOpen:
            {
                var p = Payload<ChatOpenPayload>(env);
                var chat = p != null ? _chats.GetOrOpen(p.ChatId) : null;
                if (chat == null)
                {
                    await SendAsync(MessageTypes.Error, new ErrorPayload { Message = $"Chat not found: {p?.ChatId}" });
                    break;
                }
                await SendOpenedAsync(chat);
                break;
            }

            case MessageTypes.ChatSend:
            {
                var p = Payload<ChatSendPayload>(env);
                if (p == null) break;
                var chat = _chats.GetOrOpen(p.ChatId);
                if (chat == null)
                {
                    await SendAsync(MessageTypes.Error, new ErrorPayload { Message = $"Chat not found: {p.ChatId}" });
                    break;
                }
                // The sender must be a watcher of this chat, otherwise the turn's stream (which now
                // fans out to watchers only) would never reach the very client that started it.
                _openChats[p.ChatId] = 0;
                // Run the turn in the background so the receive loop keeps serving permission/clarify
                // answers and cancel requests for this very turn.
                _ = RunTurnAsync(chat, p.Text, p.Images, hostStopping);
                break;
            }

            case MessageTypes.ChatSettings:
            {
                var p = Payload<ChatSettingsPayload>(env);
                if (p != null)
                {
                    var chat = _chats.GetOrOpen(p.ChatId);
                    if (chat != null)
                    {
                        chat.ApplySettings(p.Mode, p.ConnectionId);
                        await SendOpenedAsync(chat);   // echo back the applied settings
                    }
                }
                break;
            }

            case MessageTypes.FocusSet:
            {
                // A window focused a chat; echo to everyone so tear-off windows follow the active chat.
                var p = Payload<FocusPayload>(env);
                if (p != null && !string.IsNullOrEmpty(p.ChatId))
                    await _hub.BroadcastAsync(MessageTypes.FocusChanged, new FocusPayload { ChatId = p.ChatId });
                break;
            }

            case MessageTypes.Cancel:
            {
                if (env.ChatId != null && _activeTurns.TryGetValue(env.ChatId, out var cts))
                    cts.Cancel();
                break;
            }

            case MessageTypes.PermissionDecision:
            {
                var p = Payload<PermissionDecisionPayload>(env);
                if (env.RequestId != null && _pendingPermissions.TryRemove(env.RequestId, out var tcs))
                    tcs.TrySetResult(ProtocolMapper.ParseDecision(p?.Decision));
                break;
            }

            case MessageTypes.ClarifyChoice:
            {
                var p = Payload<ClarifyChoicePayload>(env);
                if (env.RequestId != null && _pendingClarifies.TryRemove(env.RequestId, out var tcs))
                    tcs.TrySetResult(p?.Choice);
                break;
            }

            case MessageTypes.DebugRequest:
            {
                var p = Payload<DebugRequestPayload>(env);
                var chat = env.ChatId != null ? _chats.GetOrOpen(env.ChatId) : null;
                var snap = _inspector.Snapshot(p?.Kind ?? "", chat);
                await SendAsync(MessageTypes.DebugSnapshot, snap, env.ChatId);
                break;
            }

            default:
                _log.LogWarning("Unknown message type: {Type}", env.Type);
                break;
        }
    }

    private async Task HandleHelloAsync(ProtocolEnvelope env)
    {
        var decision = _auth.Authorize(env.Auth);
        if (!decision.Ok)
        {
            await SendAsync(MessageTypes.Error, new ErrorPayload { Message = "Unauthorized." });
            await _socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "unauthorized", CancellationToken.None);
            return;
        }

        await SendAsync(MessageTypes.Welcome, new WelcomePayload
        {
            ActorId = decision.ActorId,
            Capabilities = decision.Capabilities,
            ProjectName = _runtime.Settings.ProjectName,
            WorkspacePath = _runtime.Settings.WorkspacePath,
            Connections = _runtime.Settings.Connections
                .Select(c => new ConnectionDto { Id = c.Id, Name = c.DisplayName }).ToList(),
            Modes = Enum.GetNames<AgentMode>(),
            DefaultMode = _runtime.Settings.Mode.ToString()
        });
    }

    private Task BroadcastChatListAsync()
        => _hub.BroadcastAsync(MessageTypes.ChatListResult, new ChatListResultPayload { Chats = _chats.List() });

    /// <summary>Hub-facing send that never throws — used for broadcasts to all/watching connections.</summary>
    public async Task TrySendAsync(string type, object? payload, string? chatId = null)
    {
        try { await SendAsync(type, payload, chatId); }
        catch { /* a single dead client must not break a broadcast */ }
    }

    private async Task SendOpenedAsync(ChatRuntime chat)
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

    private async Task RunTurnAsync(ChatRuntime chat, string text, List<string>? images, CancellationToken hostStopping)
    {
        using var turnCts = CancellationTokenSource.CreateLinkedTokenSource(hostStopping);
        _activeTurns[chat.ChatId] = turnCts;

        var ctx = new TurnContext();
        var callbacks = BuildCallbacks(chat, ctx);

        string? error = null;
        try
        {
            await chat.SendAsync(
                text,
                callbacks,
                (def, args) => RequestPermissionAsync(chat.ChatId, def, args, turnCts.Token),
                req => RequestClarifyAsync(chat.ChatId, req, turnCts.Token),
                turnCts.Token,
                images);
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
    private AgentCallbacks BuildCallbacks(ChatRuntime chat, TurnContext ctx)
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
                _runtime.TokenUsageProject.Record(prompt, completion);
                _runtime.TokenUsageGlobal.Record(prompt, completion);
                _ = ToWatchers(MessageTypes.TokenUsage, new TokenUsagePayload { PromptTokens = prompt, CompletionTokens = completion });
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

    private static T? Payload<T>(ProtocolEnvelope env) where T : class
        => env.Payload is { } el ? el.Deserialize<T>(Json) : null;

    private async Task SendAsync(string type, object? payload, string? chatId = null, string? requestId = null)
    {
        var env = new ProtocolEnvelope
        {
            Type = type,
            ChatId = chatId,
            RequestId = requestId,
            Payload = payload == null ? null : JsonSerializer.SerializeToElement(payload, Json)
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(env, Json);

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
    }
}
