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
    private void LoadChatsList()
    {
        if (_chatManager == null) return;
        
        var activeId = CurrentChat?.Id;
        _isReloadingChats = true;
        try
        {
            Chats.Clear();
            foreach (var c in _chatManager.ListChats())
            {
                Chats.Add(c);
            }
            
            if (activeId != null)
            {
                var matched = Chats.FirstOrDefault(c => c.Id == activeId);
                if (matched != null)
                {
                    CurrentChat = matched;
                }
            }
        }
        finally
        {
            _isReloadingChats = false;
        }
    }

    partial void OnCurrentChatChanged(ChatSession? value)
    {
        if (_chatManager == null) return;
        if (_isReloadingChats) return;

        if (value == null)
        {
            _loadedChatId = null;
            ClearMessagesSafely();
            return;
        }

        if (_loadedChatId == value.Id && Messages.Count > 0)
        {
            return;
        }

        _loadedChatId = value.Id;
        ClearMessagesSafely();

        var loaded = _chatManager.LoadChat(value.Id);
        if (loaded != null)
        {
            value.Messages = loaded.Messages;
            value.Title = loaded.Title;
            value.UpdatedAt = loaded.UpdatedAt;
        }

        // Restore this chat's session working memory (replaces the previous chat's).
        _sessionKv.LoadFrom(loaded?.Kv ?? new Dictionary<string, string>());

        var resolved = App.ResolvedSettings;
        if (resolved.ProjectName != null)
        {
            Messages.Add(new SystemMessageViewModel($"Project: {resolved.ProjectName} | Mode: {resolved.Mode}"));
        }

        // Build system prompt
        var modePrompt = resolved.Mode switch
        {
            AgentMode.Chat => "You are a helpful local AI assistant named SPLA. You are in Chat mode. You should engage in conversation and answer questions.",
            AgentMode.Research => "You are an AI assistant in Research mode. You can read files and search to answer questions, but you cannot modify any files.",
            AgentMode.Inspect => "You are an AI assistant in Inspect mode. You can read files, inspect the system, and run read-only terminal commands.",
            AgentMode.Edit => "You are an AI coding assistant in Edit mode. You MUST proactively use your tools to edit files and write changes to disk rather than just explaining the code. Do not just chat, apply the changes.",
            AgentMode.Agent => "You are a fully autonomous AI Agent. You can read, write, and execute commands without prompting the user. Proactively complete the requested tasks end-to-end.",
            _ => "You are a helpful local AI assistant named SPLA."
        };

        var currentDir = Directory.GetCurrentDirectory();
        var systemPrompt = $"{modePrompt}\nYou have access to tools to interact with the file system and run commands. Your current working directory is: {currentDir}\n\nTool descriptions are intentionally short. Tool flag: [H] = extended help available. If a [H] tool's details are unclear, call agent.info with the tool name before using it. Do not guess complex argument formats.\n\nMermaid note: when writing Mermaid, use valid quoted labels: `NodeId[\"label\"]`, `subgraph Id[\"title\"]`, and `A -->|\"label\"| B` for text with spaces, punctuation, or non-ASCII characters.\n\nIMPORTANT RULE: You may attempt a specific tool a maximum of 3 times. If it fails 3 times, you MUST stop trying and ask the user for help.";

        foreach (var instrPath in resolved.Instructions)
        {
            var fullPath = Path.GetFullPath(Path.Combine(currentDir, instrPath));
            if (File.Exists(fullPath))
            {
                var content = File.ReadAllText(fullPath);
                systemPrompt += $"\n\n--- Instructions from {instrPath} ---\n{content}";
            }
        }
        
        if (!string.IsNullOrWhiteSpace(resolved.CustomPrompt))
        {
            systemPrompt += $"\n\n--- Custom Prompt ---\n{resolved.CustomPrompt}";
        }

        // Inject skill index BEFORE plugin prompts so the rule takes precedence
        var enabledSkills = _skillManager.GetEnabled().ToList();
        if (enabledSkills.Count > 0)
        {
            systemPrompt += "\n\n--- Skills ---";
            systemPrompt += "\nRULE: When the user's request matches a skill listed below, you MUST call agent.info FIRST — before calling any other tool or executing any step.";
            systemPrompt += "\nThis rule overrides any plugin instruction that says to 'start immediately'.";
            systemPrompt += "\nSkill descriptions are in English. The user may write in any language — translate the intent to English and match semantically.";
            systemPrompt += "\n";
            systemPrompt += "\nHow to load a skill — call agent.info with {\"id\": \"<skill-id>\"}";
            systemPrompt += "\nExample: call agent.info with {\"id\": \"network.range-audit\"}";
            systemPrompt += "\nagent.info works for both tool help AND skill instructions — use it for any capability lookup.";
            systemPrompt += "\n\nAvailable skills:";
            foreach (var skill in enabledSkills)
            {
                systemPrompt += $"\n  {skill.Id} — {skill.Description}";
                if (skill.IsPreloaded)
                {
                    var body = _skillManager.LoadBody(skill.Id);
                    if (!string.IsNullOrEmpty(body))
                        systemPrompt += $"\n\n--- Skill: {skill.Id} (preloaded) ---\n{body}";
                }
            }
        }

        // Inject Plugin Prompts
        foreach (var plugin in _pluginManager.GetActivePlugins())
        {
            if (!string.IsNullOrWhiteSpace(plugin.EffectivePrompt))
            {
                systemPrompt += $"\n\n--- Plugin: {plugin.Meta.Id} ---\n{plugin.EffectivePrompt}";
            }
        }

        var pluginCommands = _pluginManager.GetUiCommands();
        if (pluginCommands.Count > 0)
        {
            systemPrompt += "\n\n--- Plugin Commands ---\nUse tool `plugin.command.run` with `commandId` to run these commands. These are commands, not direct tool names.";
            foreach (var command in pluginCommands)
            {
                systemPrompt += $"\n- {command.Id}: {command.DisplayName} ({command.Kind})";
            }
        }
        
        Messages.Add(new SystemMessageViewModel(systemPrompt, isSystemPrompt: true));

        // Load existing messages — use typed VMs so DataTemplates match correctly
        foreach (var m in value.Messages)
        {
            MessageViewModel vm = m.Role.ToLower() switch
            {
                "user"      => new UserMessageViewModel(m.Content),
                "assistant" => new AssistantMessageViewModel(m.Content),
                "tool"      => new MessageViewModel(ChatRole.Tool, m.Content),
                _           => new SystemMessageViewModel(m.Content)
            };
            if (!string.IsNullOrEmpty(m.Reasoning))
                vm.Reasoning = m.Reasoning;
            Messages.Add(vm);
        }
    }

    [RelayCommand]
    private async Task DeleteChatAsync(ChatSession? chat)
    {
        if (_chatManager == null || chat == null || IsBusy) return;

        if (ConfirmDeleteChatAsync != null)
        {
            var confirmed = await ConfirmDeleteChatAsync(chat);
            if (!confirmed) return;
        }

        var deletingCurrent = CurrentChat?.Id == chat.Id;
        _chatManager.DeleteChat(chat.Id);
        Chats.Remove(chat);

        if (!deletingCurrent) return;

        CurrentChat = null;
        _loadedChatId = null;
        ClearMessagesSafely();

        LoadChatsList();
        if (Chats.Count > 0)
        {
            SelectChat(Chats.First());
        }
        else
        {
            NewChat();
        }
    }

    [RelayCommand]
    private void SelectChat(ChatSession session)
    {
        if (_chatManager == null) return;
        if (_isReloadingChats) return;
        
        if (CurrentChat != session)
        {
            CurrentChat = session;
        }
        else if (_loadedChatId != session.Id)
        {
            _loadedChatId = null;
            OnCurrentChatChanged(session);
        }
    }


    [RelayCommand]
    private void ForkChat()
    {
        if (_chatManager == null || CurrentChat == null) return;
        var newChat = _chatManager.DuplicateChat(CurrentChat.Id);
        LoadChatsList();
        SelectChat(newChat);
    }
}

