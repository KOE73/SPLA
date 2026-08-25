using SPLA.Runtime;
using Microsoft.Extensions.Logging;
using SPLA.Agent;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Runs turns for one chat, independent of any WebSocket connection. One instance per chat, wired up
/// from <see cref="ChatRegistry.RuntimeOpened"/> the same way <c>SplaServiceHost.WireChatProgress</c>
/// wires the chat's progress fan-out — both live and die with the <see cref="ChatRuntime"/>, not with
/// whichever socket happened to be attached when the chat was opened.
///
/// <para>
/// This used to be <c>ClientConnection.RunTurnAsync</c>, a private instance method run "from the
/// socket" as fire-and-forget. Investigation for PLAN_20260825 wave A found it was already almost
/// connection-independent: <c>runtime.Asks</c>/<c>runtime.Turns</c> are chat-addressed registries, not
/// per-connection, and <see cref="ConnectionHub.BroadcastToWatchersAsync"/> already fans out to every
/// watcher rather than to the connection that happened to call it. The one genuine per-connection
/// dependency was the acting user's key for telemetry attribution — that is why it is now a parameter
/// (<paramref name="userKey"/> below on <see cref="RunTurnAsync"/>) instead of a captured field: the
/// only thing that made a turn "belong" to a connection was who was asking, and now that comes in on
/// each call, so a future non-socket caller (the pump in wave B) can supply its own.
/// </para>
/// </summary>
internal sealed class ChatTurnDriver
{
    private readonly ConnectionHub _hub;
    private readonly AgentRuntimeRegistry _registry;
    private readonly AgentRuntime _runtime;
    private readonly string _projectId;
    private readonly ChatRuntime _chat;
    private readonly ILogger _log;

    public ChatTurnDriver(
        ConnectionHub hub, AgentRuntimeRegistry registry, AgentRuntime runtime, string projectId,
        ChatRuntime chat, ILogger log)
    {
        _hub = hub;
        _registry = registry;
        _runtime = runtime;
        _projectId = projectId;
        _chat = chat;
        _log = log;
    }

    /// <summary>
    /// Runs one turn to completion (or cancellation/failure), broadcasting the same events this used
    /// to send from the connection: the streamed turn itself, the busy/idle chat-list marks either
    /// side of it, and the end-of-turn <see cref="MessageTypes.TurnComplete"/> /
    /// <see cref="MessageTypes.ChatToolSetState"/> pair. Behaviour is unchanged from the old
    /// <c>ClientConnection.RunTurnAsync</c> — only who owns the call moved.
    /// </summary>
    public async Task RunTurnAsync(string? text, IReadOnlyList<string>? images, string userKey, CancellationToken hostStopping)
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

        var ctx = new TurnContext();

        // Resolve the model's operative context window up front (cached; one cheap local call) so
        // every token.usage broadcast this turn can carry "prompt vs window" for the UI's budget bar.
        try { ctx.ContextLength = await chat.GetContextLengthAsync(turnCts.Token); }
        catch { /* unknown window — usage still reports raw counts */ }

        var callbacks = BuildCallbacks(runtime, projectId, chat, ctx);

        string? error = null;
        try
        {
            var send = chat.SendAsync(
                text,
                callbacks,
                (def, args) => runtime.Asks.AskPermissionAsync(chat.ChatId, def, args, turnCts.Token),
                req => runtime.Asks.AskClarifyAsync(chat.ChatId, req, turnCts.Token),
                turnCts.Token,
                images,
                onUserMessage: m => _ = _hub.BroadcastToWatchersAsync(chat.ChatId, MessageTypes.UserMessage,
                    new UserMessagePayload
                    {
                        MsgId = m.MsgId,
                        CreatedAt = m.CreatedAt.ToString("o"),
                        Text = text
                    }));

            // SendAsync counts the turn before its first await, so the chat already reports itself
            // busy here — the sidebar marks where work is happening, including for clients that are
            // not watching this chat and would otherwise never learn of it.
            await BroadcastChatListAsync(projectId);
            await send;
        }
        catch (OperationCanceledException) { /* cancelled turn — reported below */ }
        catch (Exception ex)
        {
            error = ex.Message;
            _log.LogError(ex, "Turn failed for chat {ChatId}.", chat.ChatId);
        }
        finally
        {
            runtime.Turns.Remove(chat.ChatId, turnCts);
        }

        await _hub.BroadcastToWatchersAsync(chat.ChatId, MessageTypes.TurnComplete,
            new TurnCompletePayload
            {
                Cancelled = turnCts.IsCancellationRequested,
                Error = error,
                ActiveSkillId = chat.ActiveSkillId
            });

        // The sets the model reached for during the turn. Sent at the same moment as the active skill
        // and for the same reason: end of turn is when the person can see, and act on, what is still up.
        await _hub.BroadcastToWatchersAsync(chat.ChatId, MessageTypes.ChatToolSetState,
            new ChatToolSetStatePayload { ChatId = chat.ChatId, Sets = ChatHandlers.ToolSetDtos(_registry.Open(projectId), chat) });

        await BroadcastChatListAsync(projectId);   // the chat is idle again — clear its mark
    }

    private Task BroadcastChatListAsync(string projectId)
        => _hub.BroadcastToProjectAsync(projectId, MessageTypes.ChatListResult,
            new ChatListResultPayload { Chats = _registry.Open(projectId).Chats.List() });

    /// <summary>
    /// Turn events fan out to every connection watching the chat (via the hub), not just whoever asked
    /// for the turn — so two windows on one chat both see the live stream. Permission/clarify are not
    /// here either, but for a different reason: they are raised by the project runtime's
    /// <c>PendingAskStore</c> and fanned out by the host (see
    /// <c>SplaServiceHost.WireRuntimeEvents</c>), so they survive whichever connection asked for the
    /// turn going away — and, since this class exists, survive there having been no connection at all.
    /// </summary>
    private AgentCallbacks BuildCallbacks(AgentRuntime runtime, string projectId, ChatRuntime chat, TurnContext ctx)
    {
        var chatId = chat.ChatId;
        DateTime lastProgress = DateTime.MinValue;
        Task ToWatchers(string type, object payload)
        {
            // Every turn event is also proof the turn is moving. A registered turn that has gone
            // silent for a long time is a model that stopped halfway — the state the instance must
            // never be evicted out of, and the one a person comes back to and pokes forward.
            runtime.Turns.Touch(chatId);
            return _hub.BroadcastToWatchersAsync(chatId, type, payload);
        }

        return new AgentCallbacks
        {
            OnLlmTurnStart = context =>
            {
                chat.CaptureLastContext(context);   // for the context.last debug snapshot
                // Indices come from the CHAT, not from this turn: see ChatRuntime.NextBubbleIndex.
                ctx.CurrentMsgIndex = chat.NextBubbleIndex();
                return ToWatchers(MessageTypes.LlmTurnStart, new DeltaPayload
                {
                    MsgIndex = ctx.CurrentMsgIndex, Text = "", ProgressTreeId = chat.CurrentTurnTreeId
                });
            },
            OnDelta = chunk => ToWatchers(MessageTypes.Delta, new DeltaPayload { MsgIndex = ctx.CurrentMsgIndex, Text = chunk }),
            OnReasoning = chunk => ToWatchers(MessageTypes.Reasoning, new ReasoningPayload { MsgIndex = ctx.CurrentMsgIndex, Text = chunk }),
            OnAttempt = attempt => _ = ToWatchers(MessageTypes.Attempt, new AttemptPayload
            {
                MsgIndex = ctx.CurrentMsgIndex,
                Index = attempt.Index,
                Outcome = attempt.Outcome.ToString(),
                Note = attempt.Note,
                Chars = attempt.Chars,
                DurationMs = (long)attempt.Duration.TotalMilliseconds,
                Content = attempt.Content,
                Reasoning = attempt.Reasoning
            }),
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
            // No OnProgressTree here anymore: the nested picture (script children, a sub-agent's
            // whole run, and — since PLAN_20260824-2 wave 1 — a background task's ticks) is delivered
            // by a chat-level subscription now, wired once for the chat's whole life in
            // SplaServiceHost.WireChatProgress. A per-turn subscription here would see only the
            // turn's own tree and, worse, double-deliver every node this same tree already reports
            // through that chat-level path (ChatRuntime.SendAsync registers the turn's tree into
            // ChatRuntime.Progress additively — see its own comment).
            OnToolResult = (tc, result) => ToWatchers(MessageTypes.ToolResult,
                new ToolResultPayload
                {
                    ToolCallId = tc.Id,
                    ToolName = tc.Function.Name,
                    Result = result.TextContent,
                    Outcome = result.Outcome.ToString(),
                    Reason = result.Reason
                }),
            OnNotice = note => ToWatchers(MessageTypes.Notice, new NoticePayload { Text = note }),
            // Tells the windows what the call cost; recording it happened in the pipeline, so
            // SettingsOps.GetUsage already reflects this turn by the time the broadcast is built.
            OnLlmTurn = turn =>
            {
                _ = ToWatchers(MessageTypes.TokenUsage, new TokenUsagePayload
                {
                    PromptTokens = turn.Message.PromptTokens,
                    CompletionTokens = turn.Message.CompletionTokens,
                    ContextLength = ctx.ContextLength
                });
                _ = _hub.BroadcastToProjectAsync(projectId, MessageTypes.UsageResult, SettingsOps.GetUsage(runtime));
            }
        };
    }

    /// <summary>Per-turn mutable bookkeeping for streaming assistant messages. The bubble index itself
    /// is the chat's (<see cref="ChatRuntime.NextBubbleIndex"/>); only "which bubble is streaming right
    /// now" is per turn.</summary>
    private sealed class TurnContext
    {
        public int CurrentMsgIndex;
        /// <summary>The model's operative context window (tokens) resolved at turn start; null = unknown.</summary>
        public int? ContextLength;
    }
}
