using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.UI.Avalonia.ViewModels.Status;

public partial class StatusViewModel : ViewModelBase
{
    private ISkillSession? _skillSession;

    /// <summary>Wires up skill session observation. Call once from MainWindowViewModel.</summary>
    public void AttachSkillSession(ISkillSession session)
    {
        if (_skillSession != null)
            _skillSession.Changed -= OnSkillSessionChanged;
        _skillSession = session;
        _skillSession.Changed += OnSkillSessionChanged;
        SyncSkillState();
    }

    private void OnSkillSessionChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
            SyncSkillState();
        else
            Dispatcher.UIThread.Post(SyncSkillState);
    }

    private void SyncSkillState()
    {
        ActiveSkillId = _skillSession?.ActiveSkillId;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveSkill))]
    private string? _activeSkillId;

    public bool HasActiveSkill => ActiveSkillId is not null;

    [RelayCommand(CanExecute = nameof(HasActiveSkill))]
    private void UnloadSkill() => _skillSession?.Deactivate();

    partial void OnActiveSkillIdChanged(string? value)
    {
        UnloadSkillCommand.NotifyCanExecuteChanged();
    }


    [ObservableProperty]
    private string _endpoint = "http://127.0.0.1:1234/v1/";

    [ObservableProperty]
    private string _modelName = "Not selected";

    [ObservableProperty]
    private AgentMode _mode = AgentMode.Edit;

    public ModeDisplayItem[] AvailableModeDescriptions { get; } = new ModeDisplayItem[]
    {
        new ModeDisplayItem(AgentMode.Chat, "Chat - Basic conversation without tool access"),
        new ModeDisplayItem(AgentMode.Research, "Research - Read files and search web, no modifications"),
        new ModeDisplayItem(AgentMode.Inspect, "Inspect - Run read-only terminal commands"),
        new ModeDisplayItem(AgentMode.Edit, "Edit - Modify files, prompts for terminal"),
        new ModeDisplayItem(AgentMode.Agent, "Agent - Full autonomy (read/write/execute)")
    };

    public ModeDisplayItem SelectedModeDescription
    {
        get => System.Linq.Enumerable.FirstOrDefault(AvailableModeDescriptions, x => x.Mode == Mode) ?? AvailableModeDescriptions[0];
        set
        {
            if (value != null && Mode != value.Mode)
            {
                Mode = value.Mode;
                OnPropertyChanged(nameof(SelectedModeDescription));
            }
        }
    }

    partial void OnModeChanged(AgentMode value)
    {
        OnPropertyChanged(nameof(SelectedModeDescription));
    }

    [ObservableProperty]
    private int _promptTokens;

    [ObservableProperty]
    private int _completionTokens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPromptTokens))]
    private int _activePromptTokens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentCompletionTokens))]
    private int _activeCompletionTokens;

    [ObservableProperty]
    private string _activeOperation = "Idle";

    /// <summary>Second status line: live counts / percent / interesting findings from the running tool.</summary>
    [ObservableProperty]
    private string? _activeOperationDetail;

    [ObservableProperty]
    private int _activeElapsedSeconds;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPromptTokens))]
    private int _lastPromptTokens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentCompletionTokens))]
    private int _lastCompletionTokens;

    public int CurrentPromptTokens => ActivePromptTokens > 0 ? ActivePromptTokens : LastPromptTokens;
    public int CurrentCompletionTokens => ActiveCompletionTokens > 0 ? ActiveCompletionTokens : LastCompletionTokens;

    // ── Project-lifetime cumulative totals (persisted via ITokenUsageStore) ──────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalTokens))]
    private long _totalPromptTokens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalTokens))]
    private long _totalCompletionTokens;

    public long TotalTokens => TotalPromptTokens + TotalCompletionTokens;

    private SPLA.Domain.Interfaces.ITokenUsageStore? _usageStore;

    /// <summary>
    /// Binds this status bar to the persistent project-lifetime token store. Reflects the current
    /// total immediately and keeps it live as turns are recorded. Call once from the chat VM.
    /// </summary>
    public void AttachUsageStore(SPLA.Domain.Interfaces.ITokenUsageStore store)
    {
        if (_usageStore != null) _usageStore.Changed -= OnUsageStoreChanged;
        _usageStore = store;
        _usageStore.Changed += OnUsageStoreChanged;
        SyncTotals();
    }

    private void OnUsageStoreChanged(object? sender, EventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess()) SyncTotals();
        else Dispatcher.UIThread.Post(SyncTotals);
    }

    private void SyncTotals()
    {
        var t = _usageStore?.Total;
        if (t == null) return;
        TotalPromptTokens = t.PromptTokens;
        TotalCompletionTokens = t.CompletionTokens;
    }

    /// <summary>
    /// Records the real usage reported by the provider for the last turn. Null arguments mean the
    /// provider did not report that figure, so we leave the running totals / "last" values untouched
    /// rather than counting a misleading 0.
    /// </summary>
    public void AddTokens(int? prompt, int? completion)
    {
        if (prompt is int p)
        {
            PromptTokens += p;
            LastPromptTokens = p;
        }
        if (completion is int c)
        {
            CompletionTokens += c;
            LastCompletionTokens = c;
        }
    }

    partial void OnPromptTokensChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentPromptTokens));
    }

    partial void OnCompletionTokensChanged(int value)
    {
        OnPropertyChanged(nameof(CurrentCompletionTokens));
    }

    public void BeginActiveTokens(int promptEstimate)
    {
        ActivePromptTokens = promptEstimate;
        ActiveCompletionTokens = 0;
        ActiveElapsedSeconds = 0;
        ActiveOperation = "LLM thinking...";
    }

    public void UpdateActiveCompletionTokens(int completionEstimate)
    {
        ActiveCompletionTokens = completionEstimate;
    }

    public void UpdateActiveElapsed(TimeSpan elapsed)
    {
        ActiveElapsedSeconds = Math.Max(0, (int)elapsed.TotalSeconds);
    }

    public void SetActiveOperation(string operation)
    {
        ActiveOperation = string.IsNullOrWhiteSpace(operation) ? "Idle" : operation;
        ActiveOperationDetail = null;
    }

    /// <summary>
    /// Renders a <see cref="ToolProgress"/> tick into the two status lines, generically: the header
    /// shows the tool and percent (when known); the detail line shows count, message, and any extra
    /// findings. Knows nothing tool-specific — every tool that reports gets the same treatment.
    /// </summary>
    public void ReportProgress(string toolName, ToolProgress p)
    {
        var pct = p.Fraction is double f ? $"  {f * 100:0}%" : "";
        ActiveOperation = $"⏳ {toolName}{pct}";

        var parts = new List<string>();
        if (p.Current is long c && p.Total is long t) parts.Add($"{c:N0} / {t:N0}");
        if (!string.IsNullOrWhiteSpace(p.Message)) parts.Add(p.Message!);
        if (p.Details is { Count: > 0 })
            parts.AddRange(p.Details.Select(d => $"{d.Label}: {d.Value}"));

        ActiveOperationDetail = parts.Count > 0 ? string.Join("  ·  ", parts) : null;
    }

    public void ClearActiveTokens()
    {
        ActivePromptTokens = 0;
        ActiveCompletionTokens = 0;
        ActiveElapsedSeconds = 0;
        ActiveOperation = "Idle";
        ActiveOperationDetail = null;
    }
}

public class ModeDisplayItem
{
    public AgentMode Mode { get; }
    public string Description { get; }

    public ModeDisplayItem(AgentMode mode, string description)
    {
        Mode = mode;
        Description = description;
    }

    public override string ToString() => Description;
}
