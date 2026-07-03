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
/// One connected client over a WebSocket. Translates between the wire protocol and the agent core:
/// inbound envelopes become agent actions; the orchestrator's <see cref="AgentCallbacks"/> become
/// outbound envelopes. Permission and clarify round-trips are correlated by RequestId through
/// <see cref="TaskCompletionSource{T}"/>s completed by the receive loop, so a running turn can ask
/// the very client that started it and await the answer without blocking the socket.
/// <para>
/// Multi-project: there is no "current project" stored on the connection. Every project- or
/// chat-scoped envelope carries its own <see cref="ProtocolEnvelope.ProjectId"/> (null meaning the
/// connection's default project — see <see cref="AgentRuntimeRegistry.DefaultProjectId"/>), and each
/// message resolves the runtime it needs fresh via <see cref="Resolve"/>. This mirrors the earlier
/// choice for chat scoping (sessionId per message, no mutable state) one level up: a socket that
/// touches several projects never has ambiguous "which one did I mean" state to drift.
/// </para>
/// </summary>
public sealed class ClientConnection
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly WebSocket _socket;
    private readonly AgentRuntimeRegistry _registry;
    private readonly ConnectionHub _hub;
    private readonly AuthGate _auth;
    private readonly ILogger _log;
    private readonly SPLA.Domain.Identity.IIdentity _identity;

    /// <summary>Per-user project scope (server mode): this connection lists/defaults to the projects in
    /// the authenticated user's own area, not the process-global set. Null in local/embedded mode,
    /// where the connection falls back to the shared registry's provider and default.</summary>
    private readonly IProjectProvider? _userProvider;
    private readonly string? _userDefaultProjectId;
    /// <summary>The user's own storage area root (server mode) — new projects are created inside it
    /// from just a name, so a browser client never picks a server filesystem path.</summary>
    private readonly string? _userArea;

    /// <summary>The project a bare (no ProjectId) message means for THIS connection — the user's own
    /// default in server mode, else the registry's shared default.</summary>
    private string DefaultProjectId => _userDefaultProjectId ?? _registry.DefaultProjectId;

    private IReadOnlyList<ProjectDescriptor> ListProjects()
        => _userProvider?.List() ?? _registry.List();
    private IReadOnlyList<ProjectDescriptor> RecentProjects()
        => _userProvider?.Recent() ?? _registry.Recent();
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private readonly ConcurrentDictionary<string, TaskCompletionSource<PermissionDecision>> _pendingPermissions = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string?>> _pendingClarifies = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeTurns = new();
    private readonly ConcurrentDictionary<string, byte> _openChats = new();

    /// <summary>Every project id this connection has touched — the project-level analogue of
    /// <see cref="_openChats"/>, used to scope broadcasts (see <see cref="ConnectionHub.BroadcastToProjectAsync"/>)
    /// so a client only watching project A never receives project B's settings/usage/chat-list results.</summary>
    private readonly ConcurrentDictionary<string, byte> _openProjects = new();

    /// <summary>True when this connection currently has <paramref name="chatId"/> open (a watcher of it).</summary>
    public bool IsWatching(string chatId) => _openChats.ContainsKey(chatId);

    /// <summary>True when this connection has touched <paramref name="projectId"/> at least once.</summary>
    public bool IsWatchingProject(string projectId) => _openProjects.ContainsKey(projectId);

    public ClientConnection(
        WebSocket socket, AgentRuntimeRegistry registry, ConnectionHub hub, AuthGate auth, ILogger log,
        SPLA.Domain.Identity.IIdentity? identity = null,
        IProjectProvider? userProvider = null, string? userDefaultProjectId = null, string? userArea = null)
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
    }

    /// <summary>Resolves the runtime an envelope means: its explicit ProjectId, or the connection's
    /// default when omitted. Marks the resolved project as "touched" by this connection so later
    /// project-scoped broadcasts reach it.</summary>
    private (RuntimeEntry Entry, string ProjectId) Resolve(ProtocolEnvelope env)
    {
        var projectId = string.IsNullOrEmpty(env.ProjectId) ? DefaultProjectId : env.ProjectId;
        _openProjects[projectId] = 0;
        return (_registry.Open(projectId), projectId);
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

            // ── Project browser ──────────────────────────────────────────
            case MessageTypes.ProjectList:
                await SendAsync(MessageTypes.ProjectListResult,
                    new ProjectListResultPayload { Projects = ListProjects().Select(ToDto).ToList() },
                    requestId: env.RequestId);
                break;

            case MessageTypes.ProjectRecent:
                await SendAsync(MessageTypes.ProjectListResult,
                    new ProjectListResultPayload { Projects = RecentProjects().Select(ToDto).ToList() },
                    requestId: env.RequestId);
                break;

            case MessageTypes.ProjectOpen:
            {
                var p = Payload<ProjectOpenPayload>(env);
                if (string.IsNullOrWhiteSpace(p?.ProjectId))
                {
                    await SendAsync(MessageTypes.Error, new ErrorPayload { Message = "ProjectId is required." }, requestId: env.RequestId);
                    break;
                }
                _openProjects[p.ProjectId] = 0;
                var opened = _registry.Open(p.ProjectId);
                await SendAsync(MessageTypes.ProjectContext, ToContext(p.ProjectId, opened.Runtime), requestId: env.RequestId);
                break;
            }

            case MessageTypes.ProjectCreate:
            {
                var p = Payload<ProjectCreatePayload>(env);
                string manifestPath;
                if (_userProvider != null && _userArea != null)
                {
                    // Server mode: the user gives only a NAME; the project is created inside their own
                    // area and registered in their per-user provider (so it shows up on refresh).
                    if (string.IsNullOrWhiteSpace(p?.Name))
                    {
                        await SendAsync(MessageTypes.Error, new ErrorPayload { Message = "Project name is required." }, requestId: env.RequestId);
                        break;
                    }
                    var safe = SanitizeName(p.Name);
                    manifestPath = System.IO.Path.Combine(_userArea, safe, safe + ".spla");
                    _userProvider.Create(new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath, Name = p.Name });
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(p?.ManifestPath))
                    {
                        await SendAsync(MessageTypes.Error, new ErrorPayload { Message = "ManifestPath is required." }, requestId: env.RequestId);
                        break;
                    }
                    manifestPath = p.ManifestPath;
                    _registry.Create(new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath, Name = p.Name });
                }
                _openProjects[manifestPath] = 0;
                var opened = _registry.Open(manifestPath);
                await SendAsync(MessageTypes.ProjectContext, ToContext(manifestPath, opened.Runtime), requestId: env.RequestId);
                break;
            }

            // ── Chats ─────────────────────────────────────────────────────
            case MessageTypes.ChatList:
            {
                var (entry, _) = Resolve(env);
                await SendAsync(MessageTypes.ChatListResult, new ChatListResultPayload { Chats = entry.Chats.List() },
                    requestId: env.RequestId);
                break;
            }

            case MessageTypes.ChatNew:
            {
                var (entry, projectId) = Resolve(env);
                var p = Payload<ChatNewPayload>(env) ?? new ChatNewPayload();
                var chat = entry.Chats.CreateNew(p.Title);
                await SendOpenedAsync(chat);
                await BroadcastChatListAsync(projectId, entry.Chats);
                break;
            }

            case MessageTypes.ChatRename:
            {
                var (entry, projectId) = Resolve(env);
                var p = Payload<ChatRenamePayload>(env);
                if (p != null) { entry.Chats.Rename(p.ChatId, p.Title); await BroadcastChatListAsync(projectId, entry.Chats); }
                break;
            }

            case MessageTypes.ChatDelete:
            {
                var (entry, projectId) = Resolve(env);
                var p = Payload<ChatDeletePayload>(env);
                if (p != null) { entry.Chats.Delete(p.ChatId); await BroadcastChatListAsync(projectId, entry.Chats); }
                break;
            }

            case MessageTypes.ChatOpen:
            {
                var (entry, _) = Resolve(env);
                var p = Payload<ChatOpenPayload>(env);
                var chat = p != null ? entry.Chats.GetOrOpen(p.ChatId) : null;
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
                // Registers this connection as a watcher of both the chat (for turn events) and the
                // project (for settings/usage broadcasts) without the side effects of ChatOpen.
                Resolve(env);
                var p = Payload<ChatOpenPayload>(env);
                if (!string.IsNullOrEmpty(p?.ChatId)) _openChats[p.ChatId] = 0;
                break;
            }

            case MessageTypes.ChatSend:
            {
                var (entry, projectId) = Resolve(env);
                var p = Payload<ChatSendPayload>(env);
                if (p == null) break;
                var chat = entry.Chats.GetOrOpen(p.ChatId);
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
                _ = RunTurnAsync(entry.Runtime, projectId, chat, p.Text, p.Images, hostStopping);
                break;
            }

            case MessageTypes.ChatSettings:
            {
                var (entry, _) = Resolve(env);
                var p = Payload<ChatSettingsPayload>(env);
                if (p != null)
                {
                    var chat = entry.Chats.GetOrOpen(p.ChatId);
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
                // Connection-level, not project-scoped — a debug/tear-off window may follow focus
                // regardless of which project it is itself looking at.
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

            // ── Settings / connections ────────────────────────────────────
            case MessageTypes.ConnectionsGet:
            {
                var (entry, projectId) = Resolve(env);
                await SendAsync(MessageTypes.ConnectionsResult, SettingsOps.GetConnections(entry.Runtime), requestId: env.RequestId);
                // Send cached health immediately so dots render without waiting for the network check.
                await SendAsync(MessageTypes.ConnectionsHealth,
                    ConnectionDiagOps.GetCachedHealth(entry.Runtime.Settings.Connections, entry.Runtime.ConnectionHealth));
                // Re-ping all in background (settings panel just opened) → broadcast to this project's clients.
                _ = PingAllAndBroadcastAsync(entry.Runtime, projectId);
                break;
            }

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
                var (entry, projectId) = Resolve(env);
                var req = Payload<ConnectionSwapModelRequest>(env);
                if (req == null) break;
                var swapResult = await SwapModelAsync(entry.Runtime, req, hostStopping);
                await SendAsync(MessageTypes.ConnectionSwapModelResult, swapResult, requestId: env.RequestId);
                if (swapResult.Error == null)
                    await _hub.BroadcastToProjectAsync(projectId, MessageTypes.ConnectionsResult, SettingsOps.GetConnections(entry.Runtime));
                break;
            }

            case MessageTypes.ConnectionsSave:
            {
                var (entry, projectId) = Resolve(env);
                var p = Payload<ConnectionsPayload>(env);
                var result = SettingsOps.SaveConnections(entry.Runtime, p?.Connections ?? new());
                // Everyone on this project refreshes pickers/editors against the new list.
                await _hub.BroadcastToProjectAsync(projectId, MessageTypes.ConnectionsResult, result);
                // Re-ping after save — new or changed endpoints need a fresh check.
                _ = PingAllAndBroadcastAsync(entry.Runtime, projectId);
                break;
            }

            case MessageTypes.AgentGet:
            {
                var (entry, _) = Resolve(env);
                await SendAsync(MessageTypes.AgentResult, SettingsOps.GetAgent(entry.Runtime), requestId: env.RequestId);
                break;
            }

            case MessageTypes.PluginsGet:
            {
                var (entry, _) = Resolve(env);
                await SendAsync(MessageTypes.PluginsResult, SettingsOps.GetPlugins(entry.Runtime), requestId: env.RequestId);
                break;
            }

            case MessageTypes.PluginsSave:
            {
                var (entry, projectId) = Resolve(env);
                var p = Payload<PluginsPayload>(env);
                await _hub.BroadcastToProjectAsync(projectId, MessageTypes.PluginsResult,
                    SettingsOps.SavePlugins(entry.Runtime, p?.Plugins ?? new()));
                break;
            }

            case MessageTypes.PluginAction:
            {
                var (entry, _) = Resolve(env);
                var p = Payload<PluginActionPayload>(env);
                PluginActionResultPayload result;
                try
                {
                    var value = await entry.Runtime.PluginManager.InvokeActionAsync(p?.PluginId ?? "", p?.Action ?? "", p?.ValueJson);
                    result = new PluginActionResultPayload { Ok = true, ResultJson = JsonSerializer.Serialize(value) };
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
                var (entry, projectId) = Resolve(env);
                var p = Payload<AgentSettingsPayload>(env);
                if (p != null)
                    await _hub.BroadcastToProjectAsync(projectId, MessageTypes.AgentResult, SettingsOps.SaveAgent(entry.Runtime, p));
                break;
            }

            case MessageTypes.UsageGet:
            {
                var (entry, _) = Resolve(env);
                await SendAsync(MessageTypes.UsageResult, SettingsOps.GetUsage(entry.Runtime), requestId: env.RequestId);
                break;
            }

            case MessageTypes.SystemRegisterAssociation:
                await SendAsync(MessageTypes.SystemRegisterAssociationResult, SystemOps.RegisterFileAssociation(), requestId: env.RequestId);
                break;

            case MessageTypes.AppearanceSave:
            {
                // Appearance auto-saves on change. SaveAppearance persists + publishes AppearanceChanged;
                // the host's event subscriber fans appearance.changed out to this project's windows.
                var (entry, _) = Resolve(env);
                var p = Payload<AppearanceChangedPayload>(env);
                if (p != null) SettingsOps.SaveAppearance(entry.Runtime, p.Theme, p.Density);
                break;
            }

            case MessageTypes.DebugRequest:
            {
                var (entry, _) = Resolve(env);
                var p = Payload<DebugRequestPayload>(env);
                var chat = env.ChatId != null ? entry.Chats.GetOrOpen(env.ChatId) : null;
                var snap = new LiveAgentInspector(entry.Runtime).Snapshot(p?.Kind ?? "", chat);
                await SendAsync(MessageTypes.DebugSnapshot, snap, env.ChatId, env.RequestId);
                break;
            }

            case MessageTypes.SchemaGet:
            {
                var (entry, _) = Resolve(env);
                var p = Payload<SchemaGetPayload>(env);
                if (string.IsNullOrWhiteSpace(p?.Name))
                {
                    await SendAsync(MessageTypes.SchemaResult,
                        new SchemaResultPayload { Error = "Name is required." }, requestId: env.RequestId);
                    break;
                }
                await SendAsync(MessageTypes.SchemaResult,
                    SchemaOps.Get(entry.Runtime.SchemaRegistry, p.Name), requestId: env.RequestId);
                break;
            }

            case MessageTypes.FsBrowse:
            {
                var (entry, _) = Resolve(env);
                var p = Payload<FsBrowsePayload>(env);
                var root = entry.Runtime.Settings.WorkspacePath;
                await SendAsync(MessageTypes.FsBrowseResult,
                    WorkspaceOps.Browse(root, p?.ParentRef), requestId: env.RequestId);
                break;
            }

            case MessageTypes.FsRead:
            {
                var (entry, _) = Resolve(env);
                var p = Payload<FsReadPayload>(env);
                if (string.IsNullOrWhiteSpace(p?.Ref))
                {
                    await SendAsync(MessageTypes.FsReadResult,
                        new FsReadResultPayload { Error = "Ref is required." }, requestId: env.RequestId);
                    break;
                }
                var root = entry.Runtime.Settings.WorkspacePath;
                await SendAsync(MessageTypes.FsReadResult,
                    WorkspaceOps.Read(root, p.Ref), requestId: env.RequestId);
                break;
            }

            case MessageTypes.FsWrite:
            {
                var (entry, _) = Resolve(env);
                var p = Payload<FsWritePayload>(env);
                if (string.IsNullOrWhiteSpace(p?.Ref))
                {
                    await SendAsync(MessageTypes.FsWriteResult,
                        new FsWriteResultPayload { Error = "Ref is required." }, requestId: env.RequestId);
                    break;
                }
                var root = entry.Runtime.Settings.WorkspacePath;
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

        // The handshake always describes the connection's default project — a single-project
        // client never needs to know projects exist at all. In server mode this is the user's OWN
        // default project (in their area), so a domain user lands in their space, not the server's.
        var projectId = DefaultProjectId;
        _openProjects[projectId] = 0;
        var ctx = ToContext(projectId, _registry.Open(projectId).Runtime);

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
    }

    private static ProjectDescriptorDto ToDto(ProjectDescriptor d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        ManifestPath = d.ManifestPath,
        LastOpened = d.LastOpened?.ToString("o")
    };

    private static ProjectContextPayload ToContext(string projectId, AgentRuntime runtime) => new()
    {
        ProjectId = projectId,
        ProjectName = runtime.Settings.ProjectName,
        WorkspacePath = runtime.Settings.WorkspacePath,
        Connections = runtime.Settings.Connections
            .Select(c => new ConnectionDto { Id = c.Id, Name = c.DisplayName }).ToList(),
        Modes = Enum.GetNames<AgentMode>(),
        DefaultMode = runtime.Settings.Mode.ToString(),
        Theme = runtime.Settings.Theme,
        Density = runtime.Settings.Density
    };

    private Task BroadcastChatListAsync(string projectId, ChatRegistry chats)
        => _hub.BroadcastToProjectAsync(projectId, MessageTypes.ChatListResult, new ChatListResultPayload { Chats = chats.List() });

    private Task PingAllAndBroadcastAsync(AgentRuntime runtime, string projectId) => Task.Run(async () =>
    {
        try
        {
            var health = await ConnectionDiagOps.PingAllAsync(runtime.Settings.Connections, runtime.ConnectionHealth);
            await _hub.BroadcastToProjectAsync(projectId, MessageTypes.ConnectionsHealth, health);
        }
        catch { }
    });

    private static async Task<ConnectionSwapModelResult> SwapModelAsync(
        AgentRuntime runtime, ConnectionSwapModelRequest req, CancellationToken ct)
    {
        var result = new ConnectionSwapModelResult { Id = req.Id };
        try
        {
            var endpoint = req.Endpoint ?? "";
            var apiKey   = req.ApiKey   ?? "lm-studio";

            // Unload every currently loaded model instance before loading the new one.
            var models = await runtime.ModelManagement.GetModelDetailsAsync(endpoint, apiKey, ct);
            foreach (var m in models.Where(m => m.IsLoaded))
                await runtime.ModelManagement.UnloadModelAsync(endpoint, apiKey, m.UnloadId, ct);

            await runtime.ModelManagement.LoadModelAsync(endpoint, apiKey, req.ModelKey, ct);

            // Update the live connection so chats immediately use the new model.
            var conn = runtime.Settings.Connections.FirstOrDefault(c => c.Id == req.Id);
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

    private async Task RunTurnAsync(
        AgentRuntime runtime, string projectId, ChatRuntime chat, string text, List<string>? images,
        CancellationToken hostStopping)
    {
        using var turnCts = CancellationTokenSource.CreateLinkedTokenSource(hostStopping);
        _activeTurns[chat.ChatId] = turnCts;

        var ctx = new TurnContext();
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
                _ = ToWatchers(MessageTypes.TokenUsage, new TokenUsagePayload { PromptTokens = prompt, CompletionTokens = completion });
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

    /// <summary>Turns a user-typed project name into a filesystem-safe folder/file name.</summary>
    private static string SanitizeName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        var s = new string(chars).Trim();
        return string.IsNullOrEmpty(s) ? "project" : s;
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
