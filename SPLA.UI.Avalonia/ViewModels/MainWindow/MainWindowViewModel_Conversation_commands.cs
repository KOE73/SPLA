using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.LLM.LMStudio;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.BasicTools.SystemTools;
using SPLA.Observability;
using SPLA.UI.Avalonia.ViewModels.Messages;
using SPLA.UI.Avalonia.ViewModels.Projects;
using SPLA.UI.Avalonia.ViewModels.Settings;
using SPLA.UI.Avalonia.ViewModels.Status;
using SPLA.UI.Avalonia.Views.Chat;
using SPLA.UI.Avalonia.Services;
using SPLA.Agent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsBusy) return;

        var userText = InputText;
        InputText = "";
        IsBusy = true;

        if (userText.Trim().ToLower() == "/compact")
        {
            await CompactContextAsync();
            IsBusy = false;
            return;
        }

        if (userText.Trim().Equals("/skills", StringComparison.OrdinalIgnoreCase))
        {
            var all = _skillManager.GetAll();
            var lines = all.Count == 0
                ? "No skills available."
                : string.Join("\n", all.Select(s => $"  [{(s.IsEnabled ? "on " : "off")}] {s.Id} — {s.Description}"));
            Messages.Add(new SystemMessageViewModel($"**Skills:**\n{lines}"));
            IsBusy = false;
            return;
        }

        if (userText.TrimStart().StartsWith("/skills load ", StringComparison.OrdinalIgnoreCase))
        {
            var id = userText.TrimStart()["/skills load ".Length..].Trim();
            var body = _skillManager.LoadBody(id);
            if (body == null)
            {
                Messages.Add(new SystemMessageViewModel($"Skill '{id}' not found."));
            }
            else
            {
                Messages.Add(new UserMessageViewModel($"[Skill loaded: {id}]\n\n{body}"));
                SaveCurrentChat();
                Messages.Add(new SystemMessageViewModel($"Skill '{id}' loaded into context."));
            }
            IsBusy = false;
            return;
        }

        Messages.Add(new UserMessageViewModel(userText));
        SaveCurrentChat();

        _currentRequestCts?.Dispose();
        _currentRequestCts = new CancellationTokenSource();

        using var telemetryContext = SplaTelemetry.PushContext(new(
            ConversationId: CurrentChat?.Id,
            RequestId: Guid.NewGuid().ToString("N"),
            ProjectId: App.ResolvedSettings.ProjectName,
            WorkspacePath: App.ResolvedSettings.WorkspacePath));

        await Task.Run(async () => await ProcessConversationAsync(_currentRequestCts.Token));
    }

    [RelayCommand]
    private void Stop()
    {
        if (!IsBusy) return;
        _currentRequestCts?.Cancel();
    }

    private async Task ProcessConversationAsync(CancellationToken cancellationToken)
    {
        StreamingMessageViewModel? streamingVm = null;

        // Throttle state — batch stream chunks and flush to the UI every 80 ms. Recreated each
        // turn in OnLlmTurnStart. The agent loop itself (context, guards, tools) lives in the
        // shared SPLA.Agent.ConversationOrchestrator; this method is now only the UI projection.
        const int FlushIntervalMs = 80;
        var pendingDelta = new System.Text.StringBuilder();
        var pendingReasoning = new System.Text.StringBuilder();
        var lastFlush = DateTime.UtcNow;
        var lastReasoningFlush = DateTime.UtcNow;

        try
        {
            var llmClient = new LMStudioClient(_httpClient);
            var orchestrator = new ConversationOrchestrator(llmClient, _mcpHost)
            {
                // Mode gating + the runtime sidebar toggle stay a UI concern, layered on the core.
                ToolFilter = (_, mode) => GetFilteredToolsForMode(mode),
                // Live working memory: context:* keys are re-injected into the prompt every turn.
                WorkingMemory = CollectWorkingMemory
            };

            // Seed the canonical message list from the current conversation (system prompt +
            // history + the user message just added), already filtered by the domain assembler.
            var conversation = GetRuntimeContext(Messages);

            var callbacks = new AgentCallbacks
            {
                OnLlmTurnStart = async ctx =>
                {
                    // Snapshot the exact context being sent so the debug window can show it.
                    ContextSnapshot.Capture(ctx);
                    pendingDelta = new System.Text.StringBuilder();
                    pendingReasoning = new System.Text.StringBuilder();
                    lastFlush = DateTime.UtcNow;
                    lastReasoningFlush = DateTime.UtcNow;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        StartTokenEstimate(ctx.Select(m => m.Content ?? ""));
                        streamingVm = new StreamingMessageViewModel();
                        Messages.Add(streamingVm);
                    });
                },

                OnReasoning = async chunk =>
                {
                    pendingReasoning.Append(chunk);
                    if ((DateTime.UtcNow - lastReasoningFlush).TotalMilliseconds >= FlushIntervalMs)
                    {
                        var toFlush = pendingReasoning.ToString();
                        pendingReasoning.Clear();
                        lastReasoningFlush = DateTime.UtcNow;
                        var vm = streamingVm;
                        if (vm != null)
                            await Dispatcher.UIThread.InvokeAsync(() => vm.AppendReasoning(toFlush));
                    }
                },

                OnDelta = async chunk =>
                {
                    pendingDelta.Append(chunk);
                    if ((DateTime.UtcNow - lastFlush).TotalMilliseconds >= FlushIntervalMs)
                    {
                        var toFlush = pendingDelta.ToString();
                        pendingDelta.Clear();
                        lastFlush = DateTime.UtcNow;
                        var vm = streamingVm;
                        if (vm != null)
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                vm.Append(toFlush);
                                Status.UpdateActiveCompletionTokens(EstimateTokens(new[] { vm.StreamingContent }));
                            });
                    }
                },

                OnAssistantMessage = async msg =>
                {
                    var vm = streamingVm;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        // Flush any throttled leftovers into the bubble before finalizing.
                        if (vm != null)
                        {
                            if (pendingDelta.Length > 0) { vm.Append(pendingDelta.ToString()); pendingDelta.Clear(); }
                            if (pendingReasoning.Length > 0) { vm.AppendReasoning(pendingReasoning.ToString()); pendingReasoning.Clear(); }
                        }

                        StopTokenEstimate();
                        Status.AddTokens(msg.PromptTokens, msg.CompletionTokens);

                        if (vm != null)
                        {
                            // Sync canonical text/reasoning (streaming may differ in whitespace).
                            if (!string.IsNullOrEmpty(msg.Content)) vm.SetFinal(msg.Content);
                            if (!string.IsNullOrEmpty(msg.Reasoning)) vm.SetFinalReasoning(msg.Reasoning);

                            // Drop the empty bubble when the model returned only tool calls.
                            if (!vm.HasContent && !vm.HasReasoning) Messages.Remove(vm);
                            else SaveCurrentChat();
                        }
                        streamingVm = null;
                    });
                },

                OnToolCallStarted = async tc =>
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Status.SetActiveOperation($"Tool: {tc.Function.Name}");
                        Messages.Add(new MessageViewModel(ChatRole.Assistant, $"[Tool call: {tc.Function.Name}]")
                        {
                            ToolCalls = new List<ToolCall> { tc }
                        });
                    });
                },

                OnToolResult = async (tc, result) =>
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Status.SetActiveOperation($"Tool done: {tc.Function.Name}");
                        Messages.Add(new MessageViewModel(ChatRole.Tool, result) { ToolCallId = tc.Id });
                        SaveCurrentChat();
                    });
                },

                OnNotice = async note =>
                {
                    await Dispatcher.UIThread.InvokeAsync(() => Messages.Add(new SystemMessageViewModel(note)));
                }
            };

            await orchestrator.RunAsync(conversation, Settings.GetSettings(), Settings.Mode, callbacks, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // Preserve any partial text that was streamed before cancellation
                if (streamingVm != null && !streamingVm.HasContent)
                    Messages.Remove(streamingVm);

                Messages.Add(new SystemMessageViewModel("Stopped."));
                SaveCurrentChat();
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (streamingVm != null && !streamingVm.HasContent)
                    Messages.Remove(streamingVm);

                Messages.Add(new SystemMessageViewModel($"Error: {ex.Message}"));
            });
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StopTokenEstimate();
                IsBusy = false;
                _currentRequestCts?.Dispose();
                _currentRequestCts = null;
            });
        }
    }
}

