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
using SPLA.UI.Avalonia.ViewModels.Messages;
using SPLA.UI.Avalonia.ViewModels.Projects;
using SPLA.UI.Avalonia.ViewModels.Settings;
using SPLA.UI.Avalonia.ViewModels.Status;
using SPLA.UI.Avalonia.Views.Chat;
using SPLA.UI.Avalonia.Services;
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
    private async Task CompactContextAsync()
    {
        if (_chatManager == null || CurrentChat == null) return;

        Messages.Add(new MessageViewModel(ChatRole.System, "Compacting context..."));

        try
        {
            _chatManager.SaveBackup(BuildVisibleChatSnapshot(), "before_compact");

            var llmClient = new LMStudioClient(_httpClient);
            
            var compressionPrompt = new ChatMessage
            {
                Role = ChatRole.System,
                Content = "Compress the conversation into a working summary to continue development.\nKeep only facts, decisions, constraints, open questions, modified files, commands, and the next step.\nDo not keep chit-chat. Respond in Markdown format."
            };

            var coreMessages = Messages.Where(m => !m.IsSystemOrTool && !m.IsToolCallNotice && m.HasContent).Select(m => new ChatMessage 
            { 
                Role = m.Role, 
                Content = m.Content 
            }).ToList();
            
            coreMessages.Insert(0, compressionPrompt);

            // Temporarily set a higher timeout and max tokens for summarization if needed
            var responseMsg = await llmClient.SendMessageAsync(coreMessages, Settings.GetSettings(), new List<ToolDefinition>());
            
            var summary = responseMsg.Content ?? "Compression failed.";
            _chatManager.SaveSummary(CurrentChat.Id, summary);

            var resolved = App.ResolvedSettings;
            var tailCount = resolved.CompactTailMessages;

            // Retain system prompts and the summary, plus tail messages
            var tailMessages = Messages.Where(m => m.IsUser || m.IsAssistant).TakeLast(tailCount).ToList();
            
            ClearMessagesSafely();
            if (resolved.ProjectName != null)
                Messages.Add(new MessageViewModel(ChatRole.System, $"Project: {resolved.ProjectName} | Mode: {resolved.Mode}"));
            
            Messages.Add(new MessageViewModel(ChatRole.System, "--- Compressed Context (Summary) ---\n" + summary));

            foreach (var tm in tailMessages) Messages.Add(tm);

            Messages.Add(new MessageViewModel(ChatRole.System, "Context compacted. Full history saved in YAML. The agent will use the summary and recent messages going forward."));
            
            SaveCurrentChat();
        }
        catch (Exception ex)
        {
            Messages.Add(new MessageViewModel(ChatRole.System, $"Compression error: {ex.Message}"));
        }
    }

    private ChatSession BuildVisibleChatSnapshot()
    {
        var snapshot = new ChatSession
        {
            Id = CurrentChat?.Id ?? "",
            Title = CurrentChat?.Title ?? "Chat",
            CreatedAt = CurrentChat?.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Workspace = CurrentChat?.Workspace ?? App.ResolvedSettings.WorkspacePath,
            Model = CurrentChat?.Model,
            Agent = CurrentChat?.Agent,
            Context = CurrentChat?.Context
        };

        foreach (var message in Messages.Where(m => m.HasContent && m is not PermissionMessageViewModel))
        {
            snapshot.Messages.Add(new ChatSessionMessage
            {
                Role = message.Role.ToString().ToLowerInvariant(),
                Content = message.Content
            });
        }

        return snapshot;
    }
}

