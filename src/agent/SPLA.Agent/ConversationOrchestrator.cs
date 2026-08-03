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
using System.Text;
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

        if (Checkpoint != null) Checkpoint.Target = conversation;
        bool didRollback = false;

        // One progress tree per turn. The host opens a node per tool call (see McpHost), so direct
        // calls and tools invoked inside a script both land in here. For backward-compatible single-bar
        // hosts we forward the currently executing top-level call's root progress to OnToolProgress;
        // richer hosts subscribe to the tree itself via OnProgressTree.
        var progressTree = new ProgressTree();
        ToolCall? activeTopLevel = null;
        progressTree.NodeChanged += node =>
        {
            var tc = activeTopLevel;
            if (tc != null && node.ParentId == null && node.Latest != null)
                callbacks.OnToolProgress?.Invoke(tc, node.Latest);
        };
        callbacks.OnProgressTree?.Invoke(progressTree);
        using var _progressTreeScope = ProgressScope.BeginTree(progressTree);

        bool needToCallLLM = true;
        while (needToCallLLM)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var coreMessages = ContextAssembler.Assemble(conversation.Messages);
            ApplyComposedContext(coreMessages);

            var tools = (ToolFilter ?? ((t, m) => ToolModeFilter.Filter(t, m)))
                (_tools.GetToolDefinitions(), mode);

            if (callbacks.OnLlmTurnStart != null)
                await callbacks.OnLlmTurnStart(coreMessages);

            // Text-repeat guard: accumulate streamed text and cancel via a linked token if it loops.
            using var textLoopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var streamed = new StringBuilder();
            bool textLoopTripped = false;

            // onDelta is always set: even with no UI sink we still need to watch for a text loop.
            async Task OnDelta(string chunk)
            {
                streamed.Append(chunk);
                if (callbacks.OnDelta != null) await callbacks.OnDelta(chunk);
                if (!textLoopTripped && LoopGuards.HasTextLoop(streamed.ToString()))
                {
                    textLoopTripped = true;
                    textLoopCts.Cancel();
                }
            }

            Logger?.LogInformation(
                "LLM request → mode={Mode} messages={MessageCount} tools={ToolCount}",
                mode, coreMessages.Count, tools.Count());

            ChatMessage response;
            try
            {
                // One gateway call = one network call to one model. The agent loop makes N of them
                // per user turn and stays above the gateway — everything the pipeline needs to know
                // about THIS call travels in the context, never in how the pipeline was composed.
                var turn = new LlmTurnContext
                {
                    Messages    = coreMessages,
                    Tools       = tools as IReadOnlyList<ToolDefinition> ?? tools.ToList(),
                    Settings    = llmSettings,
                    OnDelta     = OnDelta,
                    OnReasoning = callbacks.OnReasoning
                };

                response = (await _llm.InvokeAsync(turn, textLoopCts.Token)).Message;
            }
            catch (OperationCanceledException) when (textLoopTripped && !cancellationToken.IsCancellationRequested)
            {
                await Notify(callbacks, "⚠️ Generation stopped: Text repetition detected.");
                return;
            }

            conversation.Add(response);

            // Real token accounting lives in the core loop, not the UI: every turn — answer or
            // tool-call, success or not — reports the provider's figures exactly once. Hosts fold
            // these into per-chat and persistent project-lifetime tallies; the same figures are
            // recorded on the telemetry meter here so they reach both the local stats view and any
            // OTLP backend from this single choke point.
            callbacks.OnTokenUsage?.Invoke(response.PromptTokens, response.CompletionTokens);
            if (response.PromptTokens is { } promptTokens and > 0)
                Observability.SplaTelemetry.PromptTokens.Add(promptTokens);
            if (response.CompletionTokens is { } completionTokens and > 0)
                Observability.SplaTelemetry.CompletionTokens.Add(completionTokens);

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
                var result = await _tools.ExecuteToolAsync(mode, tc.Function.Name, tc.Function.Arguments, cancellationToken)
                             ?? string.Empty;
                Logger?.LogInformation(
                    "Tool result ← {ToolName} resultChars={ResultChars}", tc.Function.Name, result.Length);
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
                    await callbacks.OnToolResult(tc, result);

                // A tool may have pushed image(s) into the pending sink (e.g. a browser screenshot).
                // Tool-result messages cannot reliably carry images to every vision API, so drain the
                // sink into a synthetic user-role message — the model sees the picture on its next turn.
                var pendingImages = Domain.Agent.AgentSessionScope.Current?.Images.DrainAll();
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
