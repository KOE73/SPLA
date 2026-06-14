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
    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.Theme))
        {
            App.ChangeTheme(Settings.Theme);
        }
        else if (e.PropertyName == nameof(SettingsViewModel.Density))
        {
            App.ChangeDensity(Settings.Density);
        }
        else if (e.PropertyName == nameof(SettingsViewModel.ActiveProfileId))
        {
            var matched = AvailableProfiles.FirstOrDefault(p => p.Id == Settings.ActiveProfileId);
            if (matched != null && matched != SelectedProfile)
                SelectProfileCommand.Execute(matched);
        }
        else if (e.PropertyName == nameof(SettingsViewModel.ChatRenderMode))
        {
            if (Settings.ChatRenderMode != ActiveRenderMode)
                SelectRenderModeCommand.Execute(Settings.ChatRenderMode);
        }
    }
}

