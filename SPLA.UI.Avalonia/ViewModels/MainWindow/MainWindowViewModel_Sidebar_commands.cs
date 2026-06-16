using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
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
using SPLA.UI.Avalonia.Services.Plugins;
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
    private void LoadRecentProjectsList()
    {
        RecentProjects.Clear();
        foreach (var p in ConfigLoader.LoadRecentProjects())
        {
            RecentProjects.Add(new RecentProjectViewModel(p));
        }
        OnPropertyChanged(nameof(HasRecentProjects));
    }

    private void LoadPluginUiCommands()
    {
        PluginUiCommands.Clear();
        foreach (var command in _pluginManager.GetUiCommands())
        {
            PluginUiCommands.Add(command);
        }
        OnPropertyChanged(nameof(HasPluginUiCommands));
    }

    [RelayCommand]
    private void OpenRecentProject(RecentProjectViewModel? project)
    {
        if (project != null && File.Exists(project.Path))
        {
            ProjectFileLauncher.OpenNewInstance(project.Path);
        }
    }

    [RelayCommand]
    private async Task ExecutePluginUiCommandAsync(SplaPluginUiCommand? command)
    {
        if (command == null) return;

        try
        {
            if (command.Kind == SplaPluginUiCommandKind.OpenPanel)
            {
                var opened = await App.Services.GetRequiredService<IPluginPanelHostService>().OpenPanelAsync(command.Target);
                if (!opened)
                {
                    Messages.Add(new MessageViewModel(ChatRole.System, $"Plugin command failed: panel not found: {command.Target}"));
                }
                return;
            }

            if (command.Kind == SplaPluginUiCommandKind.OpenFile && !File.Exists(command.Target))
            {
                Messages.Add(new MessageViewModel(ChatRole.System, $"Plugin command failed: file not found: {command.Target}"));
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = command.Target,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Messages.Add(new MessageViewModel(ChatRole.System, $"Plugin command failed: {ex.Message}"));
        }
    }

    [RelayCommand]
    private void SelectProfile(SPLA.UI.Avalonia.ViewModels.Chat.ChatProfileViewModel? profile)
    {
        if (profile == null || profile == SelectedProfile) return;
        if (SelectedProfile != null) SelectedProfile.IsSelected = false;
        profile.IsSelected = true;
        SelectedProfile = profile;
        OnPropertyChanged(nameof(ActiveProfile));
    }

    [RelayCommand]
    private void SelectRenderMode(string? mode)
    {
        if (string.IsNullOrEmpty(mode) || mode == ActiveRenderMode) return;
        ActiveRenderMode = mode;
    }

    [RelayCommand]
    private void NewChat()
    {
        if (_chatManager == null) return;
        var chat = _chatManager.CreateNewChat();
        Chats.Insert(0, chat);
        SelectChat(chat);
    }

    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        if (SelectProjectFolderAsync == null || IsBusy) return;

        var folder = await SelectProjectFolderAsync();
        if (string.IsNullOrWhiteSpace(folder)) return;

        var workspacePath = Path.GetFullPath(folder);
        Directory.CreateDirectory(workspacePath);

        var folderName = Path.GetFileName(workspacePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var projectFilePath = Path.Combine(workspacePath, $"{folderName}.spla");
        if (!File.Exists(projectFilePath))
        {
            var project = new SplaProject
            {
                Version = 1,
                Name = Path.GetFileName(workspacePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                Workspace = ".",
                Agent = new SplaAgentSection
                {
                    Mode = Settings.Mode.ToString(),
                    Instructions = File.Exists(Path.Combine(workspacePath, "AGENTS.md"))
                        ? new List<string> { "AGENTS.md" }
                        : null
                },
                Permissions = new SplaPermissionsSection
                {
                    Read = "allow",
                    Write = "ask",
                    Shell = "ask",
                    Internet = "allow"
                },
                Ignore = new List<string> { "bin/", "obj/", ".git/", "node_modules/" }
            };

            ConfigLoader.SaveProject(project, projectFilePath);
        }

        WindowsShellIntegration.AddRecentProject(projectFilePath);
        ConfigLoader.AddRecentProject(projectFilePath);
        LoadRecentProjectsList();
        ProjectFileLauncher.OpenNewInstance(projectFilePath);
    }
}

