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
    private void Status_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StatusViewModel.Mode))
        {
            Settings.Mode = Status.Mode;
            _ = Settings.SaveSettingsAsync();
        }
    }

    private static PermissionManager CreatePermissionManager()
    {
        var remembered = App.ResolvedSettings.ToolPermissionRules
            .Select(rule => new RememberedToolPermission
            {
                Tool = rule.Tool ?? "",
                Arguments = string.IsNullOrWhiteSpace(rule.Arguments) ? "*" : rule.Arguments,
                Decision = string.Equals(rule.Decision, "allow", StringComparison.OrdinalIgnoreCase)
                    ? PermissionDecision.AllowRemember
                    : PermissionDecision.Deny
            });

        return new PermissionManager(remembered);
    }

    private async Task<PermissionDecision> HandlePermissionRequestAsync(ToolFunctionDefinition def, string args)
    {
        var tcs = new TaskCompletionSource<PermissionDecision>();
        
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Messages.Add(new PermissionMessageViewModel(def, args, tcs));
        });

        var decision = await tcs.Task;
        if (decision is PermissionDecision.AllowRemember or PermissionDecision.Deny)
        {
            SaveToolPermissionDecision(def, args, decision);
        }

        return decision;
    }

    private void SaveToolPermissionDecision(ToolFunctionDefinition def, string args, PermissionDecision decision)
    {
        if (string.IsNullOrWhiteSpace(App.ProjectFilePath)) return;

        var project = ConfigLoader.LoadProjectRaw(App.ProjectFilePath);
        project.Permissions ??= new SplaPermissionsSection();
        project.Permissions.Tools ??= new List<SplaToolPermissionRule>();

        project.Permissions.Tools.RemoveAll(x =>
            string.Equals(x.Tool, def.Name, StringComparison.OrdinalIgnoreCase) &&
            (x.Arguments == "*" || string.Equals(x.Arguments, args, StringComparison.Ordinal)));

        project.Permissions.Tools.Add(new SplaToolPermissionRule
        {
            Tool = def.Name,
            Arguments = "*",
            Decision = decision == PermissionDecision.AllowRemember ? "allow" : "deny"
        });

        ConfigLoader.SaveProject(project, App.ProjectFilePath);
        _permissionManager.Remember(new RememberedToolPermission
        {
            Tool = def.Name,
            Arguments = "*",
            Decision = decision
        });
        App.ReloadResolvedSettings();
    }
}

