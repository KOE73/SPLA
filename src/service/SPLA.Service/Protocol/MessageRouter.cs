using SPLA.Runtime;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Identity;
using SPLA.Domain.Models;
using SPLA.Domain.Project;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Shared JSON options for the wire protocol (web defaults). One instance so every
/// serialize/deserialize on a connection uses identical settings.</summary>
internal static class ServiceJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

/// <summary>
/// The surface a <see cref="ClientConnection"/> exposes to message handlers. Handlers act on this
/// facade rather than on the connection directly, so adding, moving, or removing a message type
/// touches only its handler — never <see cref="ClientConnection"/> itself. Everything a handler
/// legitimately needs (send, project/chat resolution, turn start, correlation completion) is here;
/// transport (framing, the receive loop, the send lock) and the correlation tables stay private to
/// the connection.
/// </summary>
internal interface IClientSession
{
    IIdentity Identity { get; }
    ConnectionHub Hub { get; }
    AgentRuntimeRegistry Registry { get; }

    /// <summary>Per-user project scope (server mode); null locally/embedded.</summary>
    IProjectProvider? UserProvider { get; }

    /// <summary>The user's own storage-area root (server mode); null locally/embedded.</summary>
    string? UserArea { get; }

    /// <summary>The project a bare (no ProjectId) message means for this connection.</summary>
    string DefaultProjectId { get; }

    IReadOnlyList<ProjectDescriptor> ListProjects();
    IReadOnlyList<ProjectDescriptor> RecentProjects();

    /// <summary>Resolves the runtime an envelope means and marks that project as touched.</summary>
    (RuntimeEntry Entry, string ProjectId) Resolve(ProtocolEnvelope env);

    void MarkProjectOpen(string projectId);
    void MarkChatOpen(string chatId);

    Task SendAsync(string type, object? payload, string? chatId = null, string? requestId = null);

    /// <summary>This connection's live SSH terminals (phase B). Per-connection state, torn down with
    /// the socket.</summary>
    SshTerminalManager Terminals { get; }
    PluginPanelManager PluginPanels { get; }

    /// <summary>Sends a chat.opened snapshot and registers this connection as its watcher.</summary>
    Task SendOpenedAsync(ChatRuntime chat);

    /// <summary>Starts a turn in the background (the receive loop keeps serving this turn's
    /// permission/clarify/cancel round-trips).</summary>
    void StartTurn(AgentRuntime runtime, string projectId, ChatRuntime chat, string text,
        List<string>? images, CancellationToken hostStopping);

    /// <summary>Cancels the active turn of <paramref name="chatId"/>, if any.</summary>
    bool TryCancelTurn(string chatId);

    /// <summary>Completes a pending permission round-trip correlated by <paramref name="requestId"/>.</summary>
    bool CompletePermission(string requestId, PermissionDecision decision);

    /// <summary>Completes a pending clarify round-trip correlated by <paramref name="requestId"/>.</summary>
    bool CompleteClarify(string requestId, string? choice);
}

/// <summary>Everything one inbound message needs: the session facade, the parsed envelope, and the
/// host-stopping token. Handlers use its typed <see cref="Payload{T}"/> and <see cref="Reply"/>
/// helpers instead of re-reading connection internals.</summary>
internal sealed class RequestContext
{
    public required IClientSession Session { get; init; }
    public required ProtocolEnvelope Env { get; init; }
    public required CancellationToken HostStopping { get; init; }

    public string? RequestId => Env.RequestId;

    public T? Payload<T>() where T : class
        => Env.Payload is { } el ? el.Deserialize<T>(ServiceJson.Options) : null;

    /// <summary>Sends a message correlated to this request (RequestId echoed back).</summary>
    public Task Reply(string type, object? payload)
        => Session.SendAsync(type, payload, null, Env.RequestId);

    /// <summary>Sends an uncorrelated message on this connection.</summary>
    public Task Send(string type, object? payload, string? chatId = null)
        => Session.SendAsync(type, payload, chatId);

    public Task Error(string message)
        => Reply(MessageTypes.Error, new ErrorPayload { Message = message });
}

/// <summary>One group of related message types. Stateless — all state flows through
/// <see cref="RequestContext"/> — so a single instance serves every connection.</summary>
internal interface IMessageHandler
{
    IEnumerable<string> HandledTypes { get; }
    Task HandleAsync(RequestContext ctx);
}

/// <summary>
/// Routes an inbound message to the handler registered for its type. Built once from the handler set;
/// a message type absent from the table is logged and ignored (same as the old <c>default:</c> arm).
/// This is the seam that keeps <see cref="ClientConnection"/> closed to protocol growth.
/// </summary>
internal sealed class MessageRouter
{
    private readonly Dictionary<string, IMessageHandler> _handlers = new(StringComparer.Ordinal);

    public MessageRouter(IEnumerable<IMessageHandler> handlers)
    {
        foreach (var handler in handlers)
            foreach (var type in handler.HandledTypes)
                _handlers[type] = handler;
    }

    public async Task<bool> TryDispatchAsync(RequestContext ctx)
    {
        if (!_handlers.TryGetValue(ctx.Env.Type, out var handler))
            return false;
        await handler.HandleAsync(ctx);
        return true;
    }

    /// <summary>The default registry: one handler per protocol module.</summary>
    public static MessageRouter Default { get; } = new(
    [
        new ProjectHandlers(),
        new ChatHandlers(),
        new CorrelationHandlers(),
        new ConnectionHandlers(),
        new SettingsHandlers(),
        new SecretHandlers(),
        new WorkspaceHandlers(),
        new TerminalHandlers(),
        new PluginPanelHandlers(),
    ]);
}

/// <summary>Shared projections from domain state to wire DTOs, used by more than one handler and by
/// the handshake. Kept neutral (not on any one handler) so no handler depends on another.</summary>
internal static class ProtocolProjection
{
    public static ProjectDescriptorDto ToDto(ProjectDescriptor d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        ManifestPath = d.ManifestPath,
        LastOpened = d.LastOpened?.ToString("o")
    };

    public static ProjectContextPayload ToContext(string projectId, AgentRuntime runtime) => new()
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

    /// <summary>Turns a user-typed project name into a filesystem-safe folder/file name.</summary>
    public static string SanitizeName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        var s = new string(chars).Trim();
        return string.IsNullOrEmpty(s) ? "project" : s;
    }
}
