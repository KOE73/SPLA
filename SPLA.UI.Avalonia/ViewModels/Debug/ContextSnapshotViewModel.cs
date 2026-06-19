using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Context;
using SPLA.Domain.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SPLA.UI.Avalonia.ViewModels.Debug;

/// <summary>
/// Holds the exact context that was sent in the most recent LLM request, plus the full
/// conversation history for the "All messages" view. Long-lived (owned by the main view model)
/// and updated each turn from <c>OnLlmTurnStart</c>.
/// </summary>
public partial class ContextSnapshotViewModel : ObservableObject
{
    /// <summary>Lines currently visible (filtered by ShowAll toggle).</summary>
    public ObservableCollection<ContextLineViewModel> Lines { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private int _count;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private int _totalCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private string _capturedAt = "—";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private int _totalWords;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private int _totalTokens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private bool _showAll;

    public string Header => ShowAll
        ? $"ALL MESSAGES  ({TotalCount} total, {Count} in context)  @ {CapturedAt}    |    ~{TotalWords} words    ~{TotalTokens} tokens"
        : $"CONTEXT SENT  ({Count} entries)  @ {CapturedAt}    |    ~{TotalWords} words    ~{TotalTokens} tokens";

    [RelayCommand]
    private void ToggleShowAll() => ShowAll = !ShowAll;

    [RelayCommand]
    private void CopyAll() => CopyLines(Lines);

    [RelayCommand]
    private void CopyNoSystem() => CopyLines(Lines.Where(l => !l.Source.StartsWith("system") && !l.Source.StartsWith("working")));

    private static void CopyLines(IEnumerable<ContextLineViewModel> lines)
    {
        var sb = new StringBuilder();
        foreach (var l in lines)
            sb.AppendLine($"[{l.MsgIdLabel}] [{l.Source}] {l.Full}");
        var text = sb.ToString();
        var clipboard = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow?.Clipboard;
        clipboard?.SetTextAsync(text);
    }

    partial void OnShowAllChanged(bool value) => RebuildLines();

    // ── Snapshot state ────────────────────────────────────────────────────────

    private List<ContextLineViewModel> _allLines = new();
    private List<ContextLineViewModel> _contextLines = new();

    /// <summary>
    /// Captures both the full conversation and the context subset sent to the LLM.
    /// Called from OnLlmTurnStart with the assembled context list; the full conversation
    /// is passed separately so we can show "all messages" mode.
    /// </summary>
    public void Capture(IReadOnlyList<ChatMessage> context, IReadOnlyList<ChatMessage>? allMessages = null)
    {
        void Apply()
        {
            // Build context set for fast lookup.
            var contextSet = new HashSet<string>(context.Select(m => m.MsgId));

            // Context-only lines (what was actually sent).
            _contextLines = new List<ContextLineViewModel>();
            var idx = 0;
            foreach (var m in context)
                _contextLines.Add(new ContextLineViewModel(m) { Index = ++idx, InContext = true });

            // All-messages lines (full history, bold = in context, dim = not).
            if (allMessages != null)
            {
                _allLines = new List<ContextLineViewModel>();
                idx = 0;
                foreach (var m in allMessages)
                    _allLines.Add(new ContextLineViewModel(m)
                    {
                        Index = ++idx,
                        InContext = contextSet.Contains(m.MsgId)
                    });
            }
            else
            {
                _allLines = _contextLines;
            }

            // Stats always reflect context.
            Count = _contextLines.Count;
            TotalCount = _allLines.Count;
            TotalWords = _contextLines.Sum(l => l.WordCount);
            TotalTokens = _contextLines.Sum(l => l.ApproxTokens);
            CapturedAt = System.DateTime.Now.ToString("HH:mm:ss");

            RebuildLines();
        }

        if (Dispatcher.UIThread.CheckAccess()) Apply();
        else Dispatcher.UIThread.Post(Apply);
    }

    private void RebuildLines()
    {
        var source = ShowAll ? _allLines : _contextLines;
        Lines.Clear();
        foreach (var l in source)
            Lines.Add(l);
    }
}
