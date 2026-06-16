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
    private List<ToolDefinition> GetFilteredToolsForMode(AgentMode mode)
    {
        return _tools.Where(t =>
        {
            // Runtime sidebar toggle
            if (!Sidebar.IsToolEnabled(t.Function.Name)) return false;

            // Agent-scoped fundamental tools (memory, info, datetime, context) are always
            // available, in every mode including Chat — they only touch the agent's own state.
            if (t.Function.Scope == ToolScope.Agent) return true;

            if (mode == AgentMode.Chat) return false;

            // Web/Internet tools are for Research, Edit, and Agent
            if (t.Function.Scope == ToolScope.Internet)
            {
                return mode == AgentMode.Research || mode == AgentMode.Edit || mode == AgentMode.Agent;
            }

            // Local read tools (system.fs.list, system.fs.read, system.fs.search, get_context, get_current_date_time) are for Inspect, Edit, and Agent
            if (t.Function.Effect == ToolEffect.Read)
            {
                return mode == AgentMode.Inspect || mode == AgentMode.Edit || mode == AgentMode.Agent;
            }

            // Local write tools (system.fs.create, system.fs.patch, system.fs.write, system.fs.delete) are for Edit and Agent
            if (t.Function.Effect == ToolEffect.Write)
            {
                return mode == AgentMode.Edit || mode == AgentMode.Agent;
            }

            // Command execution tools (run_command) are only for Agent
            if (t.Function.Effect == ToolEffect.Execute)
            {
                return mode == AgentMode.Agent;
            }

            return false;
        }).ToList();
    }

    /// <summary>
    /// Snapshot of both working-memory stores for the orchestrator's per-turn context:* injection.
    /// </summary>
    private IReadOnlyList<(string scope, string key, string value)> CollectWorkingMemory()
        => _sessionKv.List().Select(kv => (_sessionKv.Scope, kv.Key, kv.Value))
            .Concat(_projectKv.Store.List().Select(kv => (_projectKv.Store.Scope, kv.Key, kv.Value)))
            .ToList();
}

