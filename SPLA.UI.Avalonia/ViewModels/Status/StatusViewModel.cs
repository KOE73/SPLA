using CommunityToolkit.Mvvm.ComponentModel;
using SPLA.Domain.Models;
using System;

namespace SPLA.UI.Avalonia.ViewModels.Status;

public partial class StatusViewModel : ViewModelBase
{
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
    }

    public void ClearActiveTokens()
    {
        ActivePromptTokens = 0;
        ActiveCompletionTokens = 0;
        ActiveElapsedSeconds = 0;
        ActiveOperation = "Idle";
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
