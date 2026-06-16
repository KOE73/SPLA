using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SPLA.Domain.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SPLA.UI.Avalonia.ViewModels.Debug;

/// <summary>
/// Holds the exact context that was sent in the most recent LLM request. Long-lived (owned by the
/// main view model) and updated each turn from <c>OnLlmTurnStart</c>, so an open debug window always
/// reflects the latest request. Window binds directly to <see cref="Lines"/>.
/// </summary>
public partial class ContextSnapshotViewModel : ObservableObject
{
    /// <summary>One entry per message actually sent, in order.</summary>
    public ObservableCollection<ContextLineViewModel> Lines { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private int _count;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Header))]
    private string _capturedAt = "—";

    public string Header => $"CONTEXT SENT  ({Count} entries)  @ {CapturedAt}";

    /// <summary>Replaces the snapshot with the context of the latest request. UI-thread safe.</summary>
    public void Capture(IReadOnlyList<ChatMessage> context)
    {
        void Apply()
        {
            Lines.Clear();
            foreach (var m in context)
                Lines.Add(new ContextLineViewModel(m));
            Count = Lines.Count;
            CapturedAt = System.DateTime.Now.ToString("HH:mm:ss");
        }

        if (Dispatcher.UIThread.CheckAccess()) Apply();
        else Dispatcher.UIThread.Post(Apply);
    }
}
