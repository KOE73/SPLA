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
using SPLA.UI.Avalonia.Services.Guards;
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
        // Circular buffer tracking the last N tool calls for loop detection
        const int ToolLoopWindow = 3;
        var recentToolCalls = new Queue<(string name, string args)>();
        var recentToolErrors = new Queue<(string name, string args, bool isError)>();

        StreamingMessageViewModel? streamingVm = null;

        try
        {
            var llmClient = new LMStudioClient(_httpClient);
            bool needToCallLLM = true;

            while (needToCallLLM)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var coreMessages = GetRuntimeContext(Messages);

                await Dispatcher.UIThread.InvokeAsync(() => StartTokenEstimate(coreMessages.Select(m => m.Content ?? "")));

                // Create the streaming bubble up front so the user sees it appear immediately
                streamingVm = new StreamingMessageViewModel();
                await Dispatcher.UIThread.InvokeAsync(() => Messages.Add(streamingVm));

                // --- Throttle: batch deltas, flush to UI every 80 ms ---
                var pendingDelta = new System.Text.StringBuilder();
                var lastFlush   = DateTime.UtcNow;
                const int FlushIntervalMs = 80;

                async Task OnDelta(string chunk)
                {
                    pendingDelta.Append(chunk);
                    if ((DateTime.UtcNow - lastFlush).TotalMilliseconds >= FlushIntervalMs)
                    {
                        var toFlush = pendingDelta.ToString();
                        pendingDelta.Clear();
                        lastFlush = DateTime.UtcNow;
                        
                        await Dispatcher.UIThread.InvokeAsync(() => 
                        {
                            streamingVm.Append(toFlush);
                            var tokenEstimate = EstimateTokens(new[] { streamingVm.StreamingContent });
                            Status.UpdateActiveCompletionTokens(tokenEstimate);
                        });

                        // --- Text repeat guard ---
                        var currentText = streamingVm.StreamingContent;
                        if (StreamingRepeatGuard.HasTextLoop(currentText))
                        {
                            _currentRequestCts?.Cancel();
                            await Dispatcher.UIThread.InvokeAsync(() =>
                                Messages.Add(new MessageViewModel(ChatRole.System, "⚠️ Generation stopped: Text repetition detected."))
                            );
                        }
                    }
                }

                ChatMessage responseMsg;
                try
                {
                    var filteredTools = GetFilteredToolsForMode(App.ResolvedSettings.Mode);
                    responseMsg = await llmClient.SendMessageStreamFullAsync(
                        coreMessages, Settings.GetSettings(), filteredTools, OnDelta, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Flush any leftover pending delta before re-throwing
                    if (pendingDelta.Length > 0)
                    {
                        var leftover = pendingDelta.ToString();
                        await Dispatcher.UIThread.InvokeAsync(() => 
                        {
                            streamingVm.Append(leftover);
                            var tokenEstimate = EstimateTokens(new[] { streamingVm.StreamingContent });
                            Status.UpdateActiveCompletionTokens(tokenEstimate);
                        });
                    }
                    throw;
                }

                // Flush remaining delta after stream ends
                if (pendingDelta.Length > 0)
                {
                    var leftover = pendingDelta.ToString();
                    await Dispatcher.UIThread.InvokeAsync(() => 
                    {
                        streamingVm.Append(leftover);
                        var tokenEstimate = EstimateTokens(new[] { streamingVm.StreamingContent });
                        Status.UpdateActiveCompletionTokens(tokenEstimate);
                    });
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StopTokenEstimate();
                    Status.AddTokens(responseMsg.PromptTokens, responseMsg.CompletionTokens);

                    // Sync final text (streaming may have slightly different whitespace)
                    if (!string.IsNullOrEmpty(responseMsg.Content))
                        streamingVm.SetFinal(responseMsg.Content);

                    // Remove empty streaming bubble if model returned only tool calls
                    if (!streamingVm.HasContent)
                        Messages.Remove(streamingVm);
                    else
                        SaveCurrentChat();

                    streamingVm = null;
                });

                if (responseMsg.ToolCalls != null && responseMsg.ToolCalls.Any())
                {
                    // --- Tool-call loop guard ---
                    foreach (var tc in responseMsg.ToolCalls)
                    {
                        var key = (tc.Function.Name, tc.Function.Arguments);
                        recentToolCalls.Enqueue(key);
                        if (recentToolCalls.Count > ToolLoopWindow)
                            recentToolCalls.Dequeue();
                    }

                    if (ToolCallLoopGuard.HasToolCallLoop(recentToolCalls, ToolLoopWindow))
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                            Messages.Add(new SystemMessageViewModel(
                                "⚠️ Generation stopped: The model is repeating the same tool calls.")));
                        needToCallLLM = false;
                    }
                    else if (ToolCallLoopGuard.HasErrorLoop(recentToolErrors, ToolLoopWindow))
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                            Messages.Add(new SystemMessageViewModel(
                                "⚠️ Generation stopped: The same tool has returned an error too many times in a row.")));
                        needToCallLLM = false;
                    }
                    else
                    {
                        foreach (var tc in responseMsg.ToolCalls)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Status.SetActiveOperation($"Tool: {tc.Function.Name}");
                                Messages.Add(new MessageViewModel(ChatRole.Assistant, $"[Tool call: {tc.Function.Name}]")
                                {
                                    ToolCalls = responseMsg.ToolCalls
                                });
                            });

                            using var toolTelemetryContext = SplaTelemetry.PushContext((SplaTelemetry.CurrentContext ?? new SplaTelemetryContext()).WithToolCall(tc.Id));
                            var result = await _mcpHost.ExecuteToolAsync(Settings.Mode, tc.Function.Name, tc.Function.Arguments, cancellationToken);
                            cancellationToken.ThrowIfCancellationRequested();
                            
                            var isError = result.StartsWith("Error:", StringComparison.OrdinalIgnoreCase) || 
                                          result.Contains("exception", StringComparison.OrdinalIgnoreCase);
                            recentToolErrors.Enqueue((tc.Function.Name, tc.Function.Arguments, isError));
                            if (recentToolErrors.Count > ToolLoopWindow) recentToolErrors.Dequeue();
                            
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Status.SetActiveOperation($"Tool done: {tc.Function.Name}");
                                Messages.Add(new MessageViewModel(ChatRole.Tool, result) { ToolCallId = tc.Id });
                                SaveCurrentChat();
                            });
                        }
                        needToCallLLM = true;
                    }
                }
                else
                {
                    needToCallLLM = false;
                }
            }
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

