using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Context;
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
    private void ClearMessagesSafely()
    {
        foreach (var m in Messages)
        {
            if (m is StreamingMessageViewModel sm)
                sm.SetFinal(string.Empty);
            else
                m.Content = string.Empty;
        }
        Messages.Clear();
    }

    [RelayCommand]
    private async Task DeleteMessageAsync(MessageViewModel? message)
    {
        if (message == null) return;
        if (!Messages.Contains(message)) return;

        if (message is StreamingMessageViewModel streamingMessage)
        {
            streamingMessage.SetFinal(string.Empty);
        }
        else
        {
            message.Content = string.Empty;
        }

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        if (!Messages.Remove(message)) return;

        SaveCurrentChat();
    }

    private void SaveCurrentChat()
    {
        if (_chatManager == null || CurrentChat == null) return;
        
        CurrentChat.Messages.Clear();
        // Skip the system prompt(s) at the start
        foreach (var m in Messages.Where(ShouldPersistMessage))
        {
            if (m is PermissionMessageViewModel) continue;
            CurrentChat.Messages.Add(new ChatSessionMessage
            {
                Role = m.Role.ToString().ToLower(),
                Content = m.Content,
                Reasoning = m.HasReasoning ? m.Reasoning : null
            });
        }
        
        _chatManager.SaveChat(CurrentChat);
        LoadChatsList(); // Refresh titles/dates
    }

    private static bool ShouldPersistMessage(MessageViewModel message)
    {
        if (!message.HasContent) return false;
        if (message is PermissionMessageViewModel) return false;
        if (message is SystemMessageViewModel) return false;
        if (message.Content.StartsWith("You are a helpful local AI assistant")) return false;
        if (message.Content.StartsWith("Project: ")) return false;
        if (message.Role == ChatRole.Tool) return false;
        if (message.IsToolCallNotice) return false;

        return true;
    }

    /// <summary>
    /// UI-only reasons a bubble must never reach the model. These are the bits that genuinely
    /// depend on the render type (a status notice, a permission card, an error bubble); the rest
    /// of the decision — empty content, orphan tool results, retention — lives in the domain
    /// <see cref="ContextAssembler"/>.
    /// </summary>
    private static bool IsEphemeral(MessageViewModel m)
    {
        if (m is SystemMessageViewModel sm && !sm.IsSystemPrompt) return true;
        if (m is PermissionMessageViewModel) return true;
        if (m.IsError) return true;
        if (m.Content.StartsWith("Сжатие контекста")) return true;
        if (m.Content.StartsWith("Ошибка сжатия:")) return true;
        return false;
    }

    private List<ChatMessage> GetRuntimeContext(IEnumerable<MessageViewModel> source)
    {
        // Project view-models to domain messages, then let the shared ContextAssembler decide
        // what the model sees (send-filter + retention). No render-type checks beyond IsEphemeral.
        var domain = source.Select(m => new ChatMessage
        {
            Role = m.Role,
            Content = m.IsToolCallNotice ? "" : m.Content,
            ToolCallId = m.ToolCallId,
            ToolCalls = m.ToolCalls,
            RetentionPolicy = m.RetentionPolicy,
            ReplacementKey = m.ReplacementKey,
            IsEphemeral = IsEphemeral(m)
        });

        return ContextAssembler.Assemble(domain);
    }
}

