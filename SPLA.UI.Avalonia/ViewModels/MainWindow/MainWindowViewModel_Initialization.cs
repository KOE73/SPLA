using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPLA.Agent;
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
    private async Task InitializeAsync()
    {
        // Load from globally resolved settings (App.axaml.cs already parsed .spla + defaults)
        var resolved = App.ResolvedSettings;
        Settings.LoadFromResolved(resolved);

        // Load profiles: built-ins + any user-defined overrides from config
        AvailableProfiles.Clear();
        foreach (var profile in resolved.EffectiveProfiles)
            AvailableProfiles.Add(new SPLA.UI.Avalonia.ViewModels.Chat.ChatProfileViewModel(profile));

        App.ChangeTheme(resolved.Theme);

        Settings.PropertyChanged += Settings_PropertyChanged;

        _chatManager = new ChatManager(resolved);
        _services = new SPLA.UI.Avalonia.ViewModels.Chat.ChatServices
        {
            Llm           = _llmClient,
            McpHost       = _mcpHost,
            SkillManager  = _skillManager,
            PluginManager = _pluginManager,
            ChatManager   = _chatManager,
            ProjectKv     = _projectKv.Store,
            ToolFilter    = GetFilteredToolsForMode,
            ModelCatalog  = () => Settings.ModelDetails,
            PersistPermission = SaveToolPermissionDecision,
            Logger        = App.Services?.GetService<ILogger<ConversationOrchestrator>>()
        };

        SyncChatsFromDisk();
        LoadRecentProjectsList();

        // Activate the most recent chat (loads it), or create a new one.
        if (Chats.Count > 0)
            ActiveChat = Chats.First();
        else
            NewChat();

        await Task.CompletedTask;
    }
}

