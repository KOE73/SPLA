using SPLA.Domain.Context;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
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
    private readonly ILLMService _llm;
    private readonly IToolHost _tools;

    /// <summary>Window for the "same tool call / same error N times" guards.</summary>
    public int ToolLoopWindow { get; init; } = 3;

    /// <summary>When false (default) the loop guards are disabled entirely.</summary>
    public bool EnableLoopGuard { get; init; } = false;

    /// <summary>
    /// Optional override for tool gating (e.g. the UI sidebar toggle layered on top of mode rules).
    /// When null, <see cref="ToolModeFilter.Filter"/> is used.
    /// </summary>
    public Func<IEnumerable<ToolDefinition>, AgentMode, IEnumerable<ToolDefinition>>? ToolFilter { get; init; }

    /// <summary>
    /// Optional provider of agent working-memory entries (scope, key, value). When set, entries whose
    /// key starts with <see cref="WorkingMemoryInjector.KeyPrefix"/> are rendered fresh and injected
    /// into the prompt on every turn (live, never persisted). See <see cref="WorkingMemoryInjector"/>.
    /// </summary>
    public Func<IReadOnlyList<(string scope, string key, string value)>>? WorkingMemory { get; init; }

    /// <summary>
    /// Optional mark/checkpoint manager. When set, the orchestrator truncates the conversation
    /// whenever a tool signals <see cref="Domain.Agent.MarkManager.RestoreRequested"/>.
    /// </summary>
    public Domain.Agent.MarkManager? Checkpoint { get; init; }

    /// <summary>Optional logger. When set, the loop logs each LLM turn and tool call (start/end).</summary>
    public ILogger? Logger { get; init; }

    public ConversationOrchestrator(ILLMService llm, IToolHost tools)
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
        var recentToolCalls = new Queue<(string name, string args)>();
        var recentToolErrors = new Queue<(string name, string args, bool isError)>();

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
            InjectWorkingMemory(coreMessages);

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
                response = await _llm.SendMessageStreamFullAsync(
                    coreMessages, llmSettings, tools, OnDelta, textLoopCts.Token, callbacks.OnReasoning);
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

            foreach (var tc in response.ToolCalls)
            {
                recentToolCalls.Enqueue((tc.Function.Name, tc.Function.Arguments));
                if (recentToolCalls.Count > ToolLoopWindow) recentToolCalls.Dequeue();
            }

            if (EnableLoopGuard)
            {
                if (LoopGuards.HasToolCallLoop(recentToolCalls, ToolLoopWindow))
                {
                    await Notify(callbacks, "⚠️ Generation stopped: The model is repeating the same tool calls.");
                    return;
                }
                if (LoopGuards.HasErrorLoop(recentToolErrors, ToolLoopWindow))
                {
                    await Notify(callbacks, "⚠️ Generation stopped: The same tool has returned an error too many times in a row.");
                    return;
                }
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

                var isError = result.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)
                              || result.Contains("exception", StringComparison.OrdinalIgnoreCase);
                recentToolErrors.Enqueue((tc.Function.Name, tc.Function.Arguments, isError));
                if (recentToolErrors.Count > ToolLoopWindow) recentToolErrors.Dequeue();

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

            if (!didRollback)
                needToCallLLM = true; // normal tool loop: always continue until model stops calling tools
            didRollback = false;
        }
    }

    /// <summary>
    /// Inserts the live working-memory block (if any) right after the leading system prompt. Operates
    /// on the per-turn assembled list, so it is recomputed every turn and never stored in history.
    /// </summary>
    private void InjectWorkingMemory(List<ChatMessage> coreMessages)
    {
        if (WorkingMemory == null) return;
        var block = WorkingMemoryInjector.Render(WorkingMemory());
        if (block == null) return;

        var msg = new ChatMessage { Role = ChatRole.System, Content = block };
        var firstSystem = coreMessages.FindIndex(m => m.Role == ChatRole.System);
        coreMessages.Insert(firstSystem >= 0 ? firstSystem + 1 : 0, msg);
    }

    private static Task Notify(AgentCallbacks callbacks, string message)
        => callbacks.OnNotice?.Invoke(message) ?? Task.CompletedTask;
}
