using SPLA.Domain.Context;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Agent.Guards;
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

    /// <summary>
    /// Optional override for tool gating (e.g. the UI sidebar toggle layered on top of mode rules).
    /// When null, <see cref="ToolModeFilter.Filter"/> is used.
    /// </summary>
    public Func<IEnumerable<ToolDefinition>, AgentMode, IEnumerable<ToolDefinition>>? ToolFilter { get; init; }

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
        List<ChatMessage> conversation,
        LLMSettings llmSettings,
        AgentMode mode,
        AgentCallbacks callbacks,
        CancellationToken cancellationToken = default)
    {
        var recentToolCalls = new Queue<(string name, string args)>();
        var recentToolErrors = new Queue<(string name, string args, bool isError)>();

        bool needToCallLLM = true;
        while (needToCallLLM)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var coreMessages = ContextAssembler.Assemble(conversation);
            var tools = (ToolFilter ?? ((t, m) => ToolModeFilter.Filter(t, m)))
                (_tools.GetToolDefinitions(), mode);

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

            foreach (var tc in response.ToolCalls)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (callbacks.OnToolCallStarted != null)
                    await callbacks.OnToolCallStarted(tc);

                var result = await _tools.ExecuteToolAsync(mode, tc.Function.Name, tc.Function.Arguments, cancellationToken);
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
            }

            needToCallLLM = true;
        }
    }

    private static Task Notify(AgentCallbacks callbacks, string message)
        => callbacks.OnNotice?.Invoke(message) ?? Task.CompletedTask;
}
