using SPLA.Domain.Context;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Composition;
using SPLA.Agent.Guards;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Agent;

/// <summary>
/// The single agent loop: call the model, stream the answer, run any tool calls, repeat until
/// the model stops requesting tools. This is the behaviour that used to be copied into
/// <c>ProcessConversationAsync</c> (UI) and the <c>while needToCallLLM</c> block (CLI). It owns
/// the canonical <see cref="ChatMessage"/> history, applies <see cref="ContextAssembler"/> and
/// <see cref="ToolModeFilter"/>, and enforces the anti-loop guards.
/// </summary>
public sealed class ConversationOrchestrator
{
    private readonly ILlmGateway _llm;
    private readonly IToolHost _tools;

    /// <summary>How many suspicious repeats (see <see cref="ToolRepeatTracker"/>) trigger a stage:
    /// first the in-band "are you stuck?" challenge, then — if the streak rebuilds — a hard stop.
    /// Settable so a live settings edit applies to the next turn without recreating the orchestrator.</summary>
    public int ToolLoopWindow { get; set; } = 3;

    /// <summary>When false (default) the tool-call repeat guard is disabled.</summary>
    public bool EnableLoopGuard { get; set; } = false;

    /// <summary>
    /// Optional override for tool gating (e.g. the UI sidebar toggle layered on top of mode rules).
    /// When null, <see cref="ToolModeFilter.Filter"/> is used.
    /// </summary>
    public Func<IEnumerable<ToolDefinition>, AgentMode, IEnumerable<ToolDefinition>>? ToolFilter { get; init; }

    /// <summary>
    /// Optional provider of the assembled agent context. When set, it is invoked on every iteration
    /// of the loop: its system-prompt half replaces the leading system message of the assembled list,
    /// and each of its turn-message items (today: the working-memory snapshot) is inserted after it.
    /// The stored conversation is left untouched — both are per-iteration renderings.
    ///
    /// <para>Per-iteration rather than per-turn because some of what the context reflects is changed
    /// by the model itself, mid-turn: <c>skill_activate</c> is the case that forced this. A context
    /// built once before the loop would inject the activated procedure only from the user's *next*
    /// message, by which point the model has already had to act without it.</para>
    ///
    /// <para>The provider runs inside this turn's <c>AgentSessionScope</c>, which is what lets
    /// contributors resolve per-chat state (the active skill, this chat's working memory) from
    /// process-wide objects.</para>
    ///
    /// <para>One provider rather than two (prompt + memory) because the loop has no business knowing
    /// which sources of context exist — it delivers what the composer produced, addressed by
    /// placement. See <see cref="SPLA.MCP.Core.Composition.IAgentContributor"/>.</para>
    /// </summary>
    public Func<ComposedContext>? Context { get; init; }

    /// <summary>
    /// Optional mark/checkpoint manager. When set, the orchestrator truncates the conversation
    /// whenever a tool signals <see cref="Domain.Agent.MarkManager.RestoreRequested"/>.
    /// </summary>
    public Domain.Agent.MarkManager? Checkpoint { get; init; }

    /// <summary>Optional logger. When set, the loop logs each LLM turn and tool call (start/end).</summary>
    public ILogger? Logger { get; init; }

    /// <summary>
    /// When true, the loop reports into whatever <see cref="ProgressTree"/> is already open on the
    /// calling flow instead of starting one of its own. Set by <c>SpawnedAgentRunner</c>; false for
    /// every chat, where a turn is the unit and a fresh tree per turn is the whole point.
    ///
    /// <para>This is the difference between a sub-agent being observable and being a black box, and it
    /// is one branch rather than an event bus because the tree is already ambient and already nests.
    /// A spawn happens inside the parent's <c>agent_spawn</c> node; left to itself the loop opens a
    /// new tree, which detaches everything below it — the sub-agent's tool calls land somewhere nobody
    /// is subscribed to, and the parent's node sits there saying nothing for minutes. Reusing the
    /// ambient tree makes them children of that node instead, and every surface that already renders
    /// the tree — the CLI status line, the Avalonia and web tool trees, <c>tool.progress</c> over the
    /// service socket, <c>notifications/progress</c> over MCP — shows the sub-agent's work with no
    /// further wiring at all.</para>
    ///
    /// <para>What it does not do is make the sub-agent a chat: nothing here is persisted, and the
    /// nodes live as long as the parent's turn. Retaining the run is a separate question.</para>
    /// </summary>
    public bool NestInAmbientProgress { get; init; }

    /// <summary>
    /// Drains whatever a chat queued for delivery that has no turn of its own to arrive on — a
    /// background task's result, first and so far only producer (plan step 1.4). Called at the very
    /// top of the loop, before the next LLM call is assembled: the one point where inserting a
    /// message cannot land between a <c>tool_calls</c> and its matching <c>tool_result</c>s. See
    /// <see cref="SPLA.Domain.Tools.ChatInbox"/> for why nowhere inside a running turn will do.
    /// <para>Null for every caller that has no inbox — <c>SpawnedAgentRunner</c>, tests, anything
    /// that is not a chat — the same shape as <see cref="Context"/>.</para>
    /// </summary>
    public Func<IReadOnlyList<ChatMessage>>? DrainInbox { get; init; }

    /// <summary>Fired right after each drained message is added to the conversation — the earliest
    /// point <see cref="ChatMessage.MsgId"/> exists, since <c>Conversation.Add</c> is what assigns it.
    /// A caller that wants to echo one of these back to a client (PLAN_20260825 wave D: a queued human
    /// message still needs the same "here is its real id" broadcast a directly-sent one gets) cannot
    /// do that from inside <see cref="DrainInbox"/> itself — the id does not exist yet there.</summary>
    public Action<ChatMessage>? OnMessageDelivered { get; init; }

    public ConversationOrchestrator(ILlmGateway llm, IToolHost tools)
    {
        _llm = llm;
        _tools = tools;
    }

    /// <summary>
    /// Runs the loop against <paramref name="conversation"/>, appending assistant and tool
    /// messages to it in place. Caller is expected to have already appended the user message.
    /// </summary>
    public async Task RunAsync(
        Conversation conversation,
        LLMSettings llmSettings,
        AgentMode mode,
        AgentCallbacks callbacks,
        CancellationToken cancellationToken = default)
    {
        var repeatTracker = new ToolRepeatTracker();

        // Whose turn this is, settled once at the top rather than re-read at each tool call. A turn
        // belongs to one chat and one identity from start to finish; reading that afresh per call
        // would leave the answer at the mercy of whatever flow the call happens to be on.
        var callContext = ToolCallContext.FromAmbient();

        if (Checkpoint != null) Checkpoint.Target = conversation;
        bool didRollback = false;

        // One progress tree per turn. The host opens a node per tool call (see McpHost), so direct
        // calls and tools invoked inside a script both land in here. For backward-compatible single-bar
        // hosts we forward the currently executing top-level call's root progress to OnToolProgress;
        // richer hosts subscribe to the tree itself via OnProgressTree.
        //
        // Unless we were told to nest — see NestInAmbientProgress. Then the tree belongs to whoever is
        // above us and we only add to it: no scope of our own (which would detach the branch we were
        // asked to join), no OnToolProgress forwarding (the roots are that caller's, not ours), and no
        // OnProgressTree (a tree we did not open is not ours to hand out).
        var ambientTree = NestInAmbientProgress ? ProgressScope.CurrentTree : null;
        var progressTree = ambientTree ?? new ProgressTree();
        ToolCall? activeTopLevel = null;
        IDisposable? progressTreeScope = null;

        if (ambientTree is null)
        {
            // The single-bar view: whatever moved last, attributed to the top-level call it happened
            // under. Deliberately not roots only, which is what this was and which had become wrong the
            // moment anything reported below the root — a spawned run and a script's children both do,
            // and both left the bar showing a call that had gone silent for minutes. A host with one
            // line is asking "what is happening now", and the answer to that is the newest tick at any
            // depth; hosts that want the shape subscribe to the tree.
            progressTree.NodeChanged += node =>
            {
                var tc = activeTopLevel;
                if (tc != null && node.Latest != null)
                    callbacks.OnToolProgress?.Invoke(tc, node.Latest);
            };
            callbacks.OnProgressTree?.Invoke(progressTree);
            progressTreeScope = ProgressScope.BeginTree(progressTree);
        }

        using var _progressTreeScope = progressTreeScope;

        bool needToCallLLM = true;
        while (needToCallLLM)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Whatever arrived with nothing of its own to arrive on — see DrainInbox. Ahead of
            // ContextAssembler.Assemble so it rides the very next LLM call rather than waiting an
            // extra round trip, and never inside the tool-call handling below, where a tool_calls/
            // tool_result pairing could be mid-flight.
            var delivered = DrainInbox?.Invoke();
            if (delivered is { Count: > 0 })
                foreach (var message in delivered)
                {
                    conversation.Add(message);
                    OnMessageDelivered?.Invoke(message);
                }

            var coreMessages = ContextAssembler.Assemble(conversation.Messages);
            ApplyComposedContext(coreMessages);

            var tools = (ToolFilter ?? ((t, m) => ToolModeFilter.Filter(t, m)))
                (_tools.GetToolDefinitions(), mode);

            if (callbacks.OnLlmTurnStart != null)
                await callbacks.OnLlmTurnStart(coreMessages);

            Logger?.LogInformation(
                "LLM request → mode={Mode} messages={MessageCount} tools={ToolCount}",
                mode, coreMessages.Count, tools.Count());

            // Attempts this ONE gateway call abandons, collected alongside forwarding to the caller.
            // Scoped to the call (redeclared every loop iteration) because that is the unit they end up
            // attached to: whichever message this call produces — the successful one, or the empty
            // placeholder when every attempt loops — carries exactly the attempts this call reported
            // and none from a call before or after it.
            var callAttempts = new List<GenerationAttempt>();

            // One gateway call = one network call to one model. The agent loop makes N of them
            // per user turn and stays above the gateway — everything the pipeline needs to know
            // about THIS call travels in the context, never in how the pipeline was composed.
            var turn = new LlmTurnContext
            {
                Messages    = coreMessages,
                Tools       = tools as IReadOnlyList<ToolDefinition> ?? tools.ToList(),
                Settings    = llmSettings,
                OnDelta     = callbacks.OnDelta,
                OnReasoning = callbacks.OnReasoning,
                OnAttempt   = attempt =>
                {
                    callAttempts.Add(attempt);
                    callbacks.OnAttempt?.Invoke(attempt);
                }
            };

            var outcome = await _llm.InvokeAsync(turn, cancellationToken);

            // A generation that looped until every allowed attempt was spent. Not an error and not an
            // answer, so it must never be sent to the model — but it still happened, and the abandoned
            // attempts are the only surviving record of it. An empty-content assistant message carries
            // them into the conversation for exactly that reason: ContextAssembler.ShouldSend already
            // drops an empty-content message with no tool calls, so the model never sees it, but a
            // reader still can. (Only the attempts ride along — outcome.Message.Content is the last
            // LOOPING generation's text and must not go anywhere near what gets sent next turn.)
            if (outcome.Status == Domain.Llm.LlmTurnStatus.Degenerate)
            {
                Logger?.LogWarning("LLM generation abandoned — the model repeated itself on every attempt.");
                if (callAttempts.Count > 0)
                    conversation.Add(new ChatMessage
                    {
                        Role = ChatRole.Assistant,
                        Content = string.Empty,
                        Attempts = callAttempts
                    });
                await Notify(callbacks, "⚠️ Generation stopped: the model kept repeating itself.");
                return;
            }

            var response = outcome.Message;
            if (callAttempts.Count > 0) response.Attempts = callAttempts;
            conversation.Add(response);

            // What happened on the wire, told once, whole. Recording it is not done here and not by
            // the host: the tallies and the telemetry meter are fed by TokenAccountingMiddleware,
            // which sits in the pipeline and therefore also covers the callers that never come
            // through this loop. This hook is for showing and reporting.
            callbacks.OnLlmTurn?.Invoke(outcome);

            Logger?.LogInformation(
                "LLM response ← textChars={TextChars} toolCalls={ToolCalls} promptTokens={PromptTokens} completionTokens={CompletionTokens}",
                response.Content?.Length ?? 0, response.ToolCalls?.Count ?? 0,
                response.PromptTokens, response.CompletionTokens);

            if (callbacks.OnAssistantMessage != null)
                await callbacks.OnAssistantMessage(response);

            if (response.ToolCalls == null || response.ToolCalls.Count == 0)
            {
                needToCallLLM = false;
                continue;
            }

            // Tell MarkManager which assistant message is being executed so that
            // checkpoint_save / mark_set know where to insert the label.
            if (Checkpoint != null)
                Checkpoint.CurrentAssistantMsg = response;

            foreach (var tc in response.ToolCalls)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (callbacks.OnToolCallStarted != null)
                    await callbacks.OnToolCallStarted(tc);

                // Mark which top-level call is running so tree progress maps back to it. Tools execute
                // sequentially here, so a single active call is unambiguous; the host opens the node and
                // the tool reports into it via ProgressScope.Report (flowing through parallel work too).
                activeTopLevel = tc;
                Logger?.LogInformation(
                    "Tool call → {ToolName} args={Args}", tc.Function.Name, tc.Function.Arguments);
                // Normalize a null tool result to empty here so the error-detection and the tool
                // message below don't have to null-check (a null result is simply "no output").
                var toolResult = await _tools.ExecuteToolAsync(
                                     mode, tc.Function.Name, tc.Function.Arguments, cancellationToken, callContext)
                             ?? ToolResult.Empty();
                // The conversation carries text; other content is routed below by whoever handles it.
                var result = toolResult.TextContent;
                Logger?.LogInformation(
                    "Tool result ← {ToolName} outcome={Outcome} resultChars={ResultChars}",
                    tc.Function.Name, toolResult.Outcome, result.Length);
                cancellationToken.ThrowIfCancellationRequested();

                repeatTracker.Observe(tc.Function.Name, tc.Function.Arguments, result,
                    hadText: !string.IsNullOrWhiteSpace(response.Content) ||
                             !string.IsNullOrWhiteSpace(response.Reasoning),
                    DateTimeOffset.UtcNow);

                conversation.Add(new ChatMessage
                {
                    Role = ChatRole.Tool,
                    Content = result,
                    ToolCallId = tc.Id
                });

                if (callbacks.OnToolResult != null)
                    await callbacks.OnToolResult(tc, toolResult);

                // Images the tool declared in its result. Deciding how a picture reaches a given
                // model belongs here, not in the tool: tool-result messages cannot reliably carry
                // images to every vision API, so they go out as a synthetic user-role message and
                // the model sees them on its next turn.
                var pendingImages = toolResult.Content.OfType<ToolImage>()
                    .Select(i => $"data:{i.MimeType};base64,{i.Data}")
                    .ToList();
                if (pendingImages is { Count: > 0 })
                {
                    conversation.Add(new ChatMessage
                    {
                        Role = ChatRole.User,
                        Content = $"[Image from {tc.Function.Name}]",
                        Images = pendingImages.ToList()
                    });
                }

                if (Checkpoint?.RestoreRequested == true && Checkpoint.RestoreAnchorId != null)
                {
                    var anchorId = Checkpoint.RestoreAnchorId;
                    var resume   = Checkpoint.RestoreResume;
                    var label    = Checkpoint.RestoreLabel != null
                        ? $"mark '{Checkpoint.RestoreLabel}'"
                        : "checkpoint";
                    Checkpoint.Confirm();

                    if (!conversation.TruncateTo(anchorId))
                    {
                        await Notify(callbacks, $"⚠️ Rollback anchor '{anchorId}' not found — it may have been deleted.");
                        needToCallLLM = false;
                        break;
                    }

                    // Anchor is always a label (L-*), invisible to LLM.
                    // Always inject a synthetic user message as the explicit call-to-action.
                    var content = string.IsNullOrWhiteSpace(resume)
                        ? $"[Rolled back to {label}]"
                        : $"[Rolled back to {label}]\n{resume}";

                    conversation.Add(new ChatMessage
                    {
                        Role = ChatRole.User,
                        Content = content,
                        IsEphemeral = true
                    });

                    needToCallLLM = true;
                    didRollback = true;
                    break;
                }
            }

            // Two-stage repeat guard. Stage 1: the streak of suspicious repeats (identical call,
            // identical result, no commentary, fast rounds — see ToolRepeatTracker) reached the
            // threshold → ask the model in-band whether it is stuck, instead of killing the turn
            // (deliberate polling survives by simply saying so). Stage 2: the streak rebuilt after
            // the challenge → the model really is looping, stop.
            if (EnableLoopGuard && repeatTracker.Streak >= Math.Max(2, ToolLoopWindow))
            {
                if (!repeatTracker.Challenged)
                {
                    conversation.Add(new ChatMessage
                    {
                        Role = ChatRole.User,
                        Content = "[loop-check] You have repeated the same tool call " +
                                  $"{repeatTracker.Streak} times in a row with identical arguments and an identical " +
                                  "result, without saying why. If this is deliberate (e.g. polling), state briefly " +
                                  "what you are waiting for and continue; otherwise change your approach or stop " +
                                  "and summarize."
                    });
                    repeatTracker.BeginChallenge();
                    await Notify(callbacks, "⚠️ Loop check: asked the model to confirm it isn't stuck.");
                }
                else
                {
                    await Notify(callbacks, "⚠️ Generation stopped: the model kept repeating the same tool call after a loop check.");
                    return;
                }
            }

            if (!didRollback)
                needToCallLLM = true; // normal tool loop: always continue until model stops calling tools
            didRollback = false;
        }
    }

    /// <summary>
    /// Applies this iteration's composed context to the assembled list: the system prompt replaces
    /// the leading system message, then every turn-message item is inserted after it, in composition
    /// order.
    ///
    /// <para>Both replace rather than mutate: the assembled list holds the very objects the
    /// conversation stores, and writing into one would put a per-iteration rendering into persisted
    /// history.</para>
    /// </summary>
    private void ApplyComposedContext(List<ChatMessage> coreMessages)
    {
        if (Context == null) return;

        var composed = Context();

        var prompt = composed.SystemPrompt;
        var firstSystem = coreMessages.FindIndex(m => m.Role == ChatRole.System);
        if (!string.IsNullOrEmpty(prompt))
        {
            var message = new ChatMessage { Role = ChatRole.System, Content = prompt };
            if (firstSystem >= 0)
            {
                coreMessages[firstSystem] = message;
            }
            else
            {
                coreMessages.Insert(0, message);
                firstSystem = 0;
            }
        }

        var insertAt = firstSystem >= 0 ? firstSystem + 1 : 0;
        foreach (var item in composed.TurnMessages)
            coreMessages.Insert(insertAt++, new ChatMessage { Role = ChatRole.System, Content = item.Text });
    }

    private static Task Notify(AgentCallbacks callbacks, string message)
        => callbacks.OnNotice?.Invoke(message) ?? Task.CompletedTask;
}
