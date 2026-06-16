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
    private void StartTokenEstimate(IEnumerable<string> promptContents)
    {
        var promptEstimate = EstimateTokens(promptContents);
        Status.BeginActiveTokens(promptEstimate);

        _tokenEstimateStartedAt = DateTime.UtcNow;
        _tokenEstimateTimer?.Stop();
        _tokenEstimateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _tokenEstimateTimer.Tick += TokenEstimateTimer_Tick;
        _tokenEstimateTimer.Start();
    }

    private void TokenEstimateTimer_Tick(object? sender, EventArgs e)
    {
        if (!IsBusy) return;

        var elapsed = DateTime.UtcNow - _tokenEstimateStartedAt;
        Status.UpdateActiveElapsed(elapsed);
    }

    private void StopTokenEstimate()
    {
        if (_tokenEstimateTimer != null)
        {
            _tokenEstimateTimer.Stop();
            _tokenEstimateTimer.Tick -= TokenEstimateTimer_Tick;
            _tokenEstimateTimer = null;
        }
        Status.ClearActiveTokens();
    }

    private static int EstimateTokens(IEnumerable<string> contents)
    {
        var chars = contents.Sum(x => string.IsNullOrEmpty(x) ? 0 : x.Length);
        return Math.Max(1, (int)Math.Ceiling(chars / 4.0));
    }
}

