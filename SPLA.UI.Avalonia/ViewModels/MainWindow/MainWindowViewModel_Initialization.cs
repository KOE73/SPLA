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
    private async Task InitializeAsync()
    {
        // Load from globally resolved settings (App.axaml.cs already parsed .spla + defaults)
        var resolved = App.ResolvedSettings;
        Settings.LoadFromResolved(resolved);

        AvailableChatViews.Clear();
        AvailableChatViews.Add(new SPLA.UI.Avalonia.Views.Chat.ChatViewInfo("bubbles", "Bubbles"));
        AvailableChatViews.Add(new SPLA.UI.Avalonia.Views.Chat.ChatViewInfo("classic", "Document"));
        AvailableChatViews.Add(new SPLA.UI.Avalonia.Views.Chat.ChatViewInfo("diagnostic", "Diagnostic"));
        AvailableChatViews.Add(new SPLA.UI.Avalonia.Views.Chat.ChatViewInfo("web", "Web"));
        SelectedChatView = AvailableChatViews.FirstOrDefault(x => x.Id == Settings.SelectedChatViewId) ?? AvailableChatViews.First();
        UpdateChatViewSelectionFlags();

        Status.Endpoint = resolved.Endpoint;
        Status.Mode = resolved.Mode;
        
        if (resolved.Model == "auto" || string.IsNullOrEmpty(resolved.Model))
        {
            Status.ModelName = "auto (fetching...)";
            _ = Task.Run(async () =>
            {
                try
                {
                    var client = new LMStudioClient(new HttpClient());
                    var models = await client.GetModelsAsync(resolved.Endpoint, resolved.ApiKey);
                    global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (models.Count > 0)
                            Status.ModelName = $"auto ({models[0]})";
                        else
                            Status.ModelName = "auto (no models)";
                    });
                }
                catch
                {
                    global::Avalonia.Threading.Dispatcher.UIThread.Post(() => Status.ModelName = "auto (error)");
                }
            });
        }
        else
        {
            Status.ModelName = resolved.Model;
        }

        App.ChangeTheme(resolved.Theme);
        
        Settings.PropertyChanged += Settings_PropertyChanged;

        _chatManager = new ChatManager(resolved);
        LoadChatsList();
        LoadRecentProjectsList();
        
        // Build system prompt based on active chat later, but for now just load the latest chat or create a new one
        if (Chats.Count > 0)
        {
            SelectChat(Chats.First());
        }
        else
        {
            NewChat();
        }
    }
}

