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

            case MessageTypes.ChatWatch:
            {
                var p = Payload<ChatOpenPayload>(env);
                if (!string.IsNullOrEmpty(p?.ChatId)) _openChats[p.ChatId] = 0;
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

            case MessageTypes.ConnectionsGet:
                await SendAsync(MessageTypes.ConnectionsResult, SettingsOps.GetConnections(_runtime), requestId: env.RequestId);
                // Send cached health immediately so dots render without waiting for the network check.
                await SendAsync(MessageTypes.ConnectionsHealth,
                    ConnectionDiagOps.GetCachedHealth(_runtime.Settings.Connections, _runtime.ConnectionHealth));
                // Re-ping all in background (settings panel just opened) → broadcast to all clients.
                _ = PingAllAndBroadcastAsync();
                break;

            case MessageTypes.ConnectionPing:
                await SendAsync(MessageTypes.ConnectionPingResult,
                    await ConnectionDiagOps.PingAsync(Payload<ConnectionDiagRequest>(env)), requestId: env.RequestId);
                break;

            case MessageTypes.ConnectionModels:
                await SendAsync(MessageTypes.ConnectionModelsResult,
                    await ConnectionDiagOps.GetModelsAsync(Payload<ConnectionDiagRequest>(env)), requestId: env.RequestId);
                break;

            case MessageTypes.ConnectionTest:
                await SendAsync(MessageTypes.ConnectionTestResult,
                    await ConnectionDiagOps.TestChatAsync(Payload<ConnectionDiagRequest>(env)), requestId: env.RequestId);
                break;

            case MessageTypes.ConnectionSwapModel:
            {
                var req = Payload<ConnectionSwapModelRequest>(env);
                if (req == null) break;
                var swapResult = await SwapModelAsync(req, hostStopping);
                await SendAsync(MessageTypes.ConnectionSwapModelResult, swapResult, requestId: env.RequestId);
                if (swapResult.Error == null)
                    await _hub.BroadcastAsync(MessageTypes.ConnectionsResult, SettingsOps.GetConnections(_runtime));
                break;
            }

            case MessageTypes.ConnectionsSave:
            {
                var p = Payload<ConnectionsPayload>(env);
                var result = SettingsOps.SaveConnections(_runtime, p?.Connections ?? new());
                // Everyone refreshes pickers/editors against the new list.
                await _hub.BroadcastAsync(MessageTypes.ConnectionsResult, result);
                // Re-ping after save — new or changed endpoints need a fresh check.
                _ = PingAllAndBroadcastAsync();
                break;
            }

            case MessageTypes.AgentGet:
                await SendAsync(MessageTypes.AgentResult, SettingsOps.GetAgent(_runtime), requestId: env.RequestId);
                break;

            case MessageTypes.PluginsGet:
                await SendAsync(MessageTypes.PluginsResult, SettingsOps.GetPlugins(_runtime), requestId: env.RequestId);
                break;

            case MessageTypes.PluginsSave:
            {
                var p = Payload<PluginsPayload>(env);
                await _hub.BroadcastAsync(MessageTypes.PluginsResult, SettingsOps.SavePlugins(_runtime, p?.Plugins ?? new()));
                break;
            }

            case MessageTypes.PluginAction:
            {
                var p = Payload<PluginActionPayload>(env);
                PluginActionResultPayload result;
                try
                {
                    var value = await _runtime.PluginManager.InvokeActionAsync(p?.PluginId ?? "", p?.Action ?? "", p?.ValueJson);
                    result = new PluginActionResultPayload { Ok = true, ResultJson = System.Text.Json.JsonSerializer.Serialize(value) };
                }
                catch (Exception ex)
                {
                    result = new PluginActionResultPayload { Ok = false, Error = ex.Message };
                }
                await SendAsync(MessageTypes.PluginActionResult, result, requestId: env.RequestId);
                break;
            }

            case MessageTypes.AgentSave:
            {
                var p = Payload<AgentSettingsPayload>(env);
                if (p != null)
                    await _hub.BroadcastAsync(MessageTypes.AgentResult, SettingsOps.SaveAgent(_runtime, p));
                break;
            }

            case MessageTypes.UsageGet:
                await SendAsync(MessageTypes.UsageResult, SettingsOps.GetUsage(_runtime), requestId: env.RequestId);
                break;

            case MessageTypes.SystemRegisterAssociation:
                await SendAsync(MessageTypes.SystemRegisterAssociationResult, SystemOps.RegisterFileAssociation(), requestId: env.RequestId);
                break;

            case MessageTypes.AppearanceSave:
            {
                // Appearance auto-saves on change. SaveAppearance persists + publishes AppearanceChanged;
                // the host's event subscriber fans appearance.changed out to every window (incl. this one).
                var p = Payload<AppearanceChangedPayload>(env);
                if (p != null) SettingsOps.SaveAppearance(_runtime, p.Theme, p.Density);
                break;
            }

            case MessageTypes.DebugRequest:
            {
                var p = Payload<DebugRequestPayload>(env);
                var chat = env.ChatId != null ? _chats.GetOrOpen(env.ChatId) : null;
                var snap = _inspector.Snapshot(p?.Kind ?? "", chat);
                await SendAsync(MessageTypes.DebugSnapshot, snap, env.ChatId, env.RequestId);
                break;
            }

            case MessageTypes.SchemaGet:
            {
                var p = Payload<SchemaGetPayload>(env);
                if (string.IsNullOrWhiteSpace(p?.Name))
                {
                    await SendAsync(MessageTypes.SchemaResult,
                        new SchemaResultPayload { Error = "Name is required." }, requestId: env.RequestId);
                    break;
                }
                await SendAsync(MessageTypes.SchemaResult,
                    SchemaOps.Get(_runtime.SchemaRegistry, p.Name), requestId: env.RequestId);
                break;
            }

            case MessageTypes.FsBrowse:
            {
                var p = Payload<FsBrowsePayload>(env);
                var root = _runtime.Settings.WorkspacePath;
                await SendAsync(MessageTypes.FsBrowseResult,
                    WorkspaceOps.Browse(root, p?.ParentRef), requestId: env.RequestId);
                break;
            }

            case MessageTypes.FsRead:
            {
                var p = Payload<FsReadPayload>(env);
                if (string.IsNullOrWhiteSpace(p?.Ref))
                {
                    await SendAsync(MessageTypes.FsReadResult,
                        new FsReadResultPayload { Error = "Ref is required." }, requestId: env.RequestId);
                    break;
                }
                var root = _runtime.Settings.WorkspacePath;
                await SendAsync(MessageTypes.FsReadResult,
                    WorkspaceOps.Read(root, p.Ref), requestId: env.RequestId);
                break;
            }

            case MessageTypes.FsWrite:
            {
                var p = Payload<FsWritePayload>(env);
                if (string.IsNullOrWhiteSpace(p?.Ref))
                {
                    await SendAsync(MessageTypes.FsWriteResult,
                        new FsWriteResultPayload { Error = "Ref is required." }, requestId: env.RequestId);
                    break;
                }
                var root = _runtime.Settings.WorkspacePath;
                await SendAsync(MessageTypes.FsWriteResult,
                    WorkspaceOps.Write(root, p.Ref, p.Text ?? ""), requestId: env.RequestId);
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
            DefaultMode = _runtime.Settings.Mode.ToString(),
            Theme = _runtime.Settings.Theme,
            Density = _runtime.Settings.Density
        });
    }

    private Task BroadcastChatListAsync()
        => _hub.BroadcastAsync(MessageTypes.ChatListResult, new ChatListResultPayload { Chats = _chats.List() });

    private Task PingAllAndBroadcastAsync() => Task.Run(async () =>
    {
        try
        {
            var health = await ConnectionDiagOps.PingAllAsync(
                _runtime.Settings.Connections, _runtime.ConnectionHealth);
            await _hub.BroadcastAsync(MessageTypes.ConnectionsHealth, health);
        }
        catch { }
    });

    private async Task<ConnectionSwapModelResult> SwapModelAsync(ConnectionSwapModelRequest req, CancellationToken ct)
    {
        var result = new ConnectionSwapModelResult { Id = req.Id };
        try
        {
            var endpoint = req.Endpoint ?? "";
            var apiKey   = req.ApiKey   ?? "lm-studio";

            // Unload every currently loaded model instance before loading the new one.
            var models = await _runtime.ModelManagement.GetModelDetailsAsync(endpoint, apiKey, ct);
            foreach (var m in models.Where(m => m.IsLoaded))
                await _runtime.ModelManagement.UnloadModelAsync(endpoint, apiKey, m.UnloadId, ct);

            await _runtime.ModelManagement.LoadModelAsync(endpoint, apiKey, req.ModelKey, ct);

            // Update the live connection so chats immediately use the new model.
            var conn = _runtime.Settings.Connections.FirstOrDefault(c => c.Id == req.Id);
            if (conn != null) conn.Model = req.ModelKey;

            result.Model = req.ModelKey;
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
        }
        return result;
    }

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
                _ = _hub.BroadcastAsync(MessageTypes.UsageResult, SettingsOps.GetUsage(_runtime));
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
