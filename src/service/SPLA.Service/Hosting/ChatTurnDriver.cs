using SPLA.Runtime;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;

namespace SPLA.Service;

/// <summary>
/// Runs turns for one chat, independent of any WebSocket connection. Reduced to exactly what
/// ADR_20260910-2 wave 0 leaves it (§8/"Не решено" — <c>ChatTurnDriver</c>'s fate at large is still
/// open, but wave 0 already strips it to cancellation, telemetry attribution, and starting the turn):
/// every broadcast this class used to build (<c>BuildCallbacks</c>) now happens in
/// <see cref="ChatFeedWireSubscriber"/>, wired once per chat from <c>ChatRegistry.RuntimeOpened</c>
/// rather than once per turn here — a chat's own <see cref="ChatRuntime.Feed"/> carries every one of
/// those events regardless of who started the turn, including a pump-woken one with no connection at
/// all.
/// </summary>
internal sealed class ChatTurnDriver
{
    private readonly AgentRuntime _runtime;
    private readonly string _projectId;
    private readonly ChatRuntime _chat;
    private readonly ILogger _log;

    public ChatTurnDriver(
        ConnectionHub hub, AgentRuntimeRegistry registry, AgentRuntime runtime, string projectId,
        ChatRuntime chat, ILogger log)
    {
        // hub/registry are no longer used by this class directly — every broadcast moved to
        // ChatFeedWireSubscriber — but the constructor signature is left unchanged so every call site
        // (ChatHandlers, ChatPump wiring in SplaServiceHost) keeps working without a second edit pass.
        _ = hub;
        _ = registry;
        _runtime = runtime;
        _projectId = projectId;
        _chat = chat;
        _log = log;
    }

    /// <summary>
    /// Runs one turn to completion (or cancellation/failure). Registers/telemetry-scopes/starts the
    /// turn and nothing else — see this class's own comment for where the rest went.
    /// </summary>
    public async Task RunTurnAsync(string? text, IReadOnlyList<ImageAttachment>? images, string userKey, CancellationToken hostStopping)
    {
        var chat = _chat;
        var runtime = _runtime;
        var projectId = _projectId;

        using var turnCts = CancellationTokenSource.CreateLinkedTokenSource(hostStopping);
        runtime.Turns.Register(chat.ChatId, turnCts);

        // Make the acting user (and chat/project) ambient for this turn's telemetry, so tool-call and
        // token measurements the collector taps can be attributed to this user for the per-user stats
        // slice. AsyncLocal flows into the orchestrator/McpHost on this same async path.
        using var telemetryScope = SPLA.Observability.SplaTelemetry.PushContext(
            new SPLA.Observability.SplaTelemetryContext(
                ConversationId: chat.ChatId, ProjectId: projectId, UserKey: userKey));

        try
        {
            await chat.SendAsync(
                text,
                (def, args) => runtime.Asks.AskPermissionAsync(chat.ChatId, def, args, turnCts.Token),
                req => runtime.Asks.AskClarifyAsync(chat.ChatId, req, turnCts.Token),
                turnCts.Token,
                images);
        }
        catch (OperationCanceledException) { /* cancelled turn — chat.Feed already published ChatTurnCompleted */ }
        catch (Exception ex)
        {
            // chat.SendAsync's own finally already published ChatTurnCompleted with this same message
            // (ChatRuntime.SendAsync's turnError) before rethrowing — logged here only for the process
            // log, not re-reported to the wire a second time.
            _log.LogError(ex, "Turn failed for chat {ChatId}.", chat.ChatId);
        }
        finally
        {
            runtime.Turns.Remove(chat.ChatId, turnCts);
        }
    }
}
