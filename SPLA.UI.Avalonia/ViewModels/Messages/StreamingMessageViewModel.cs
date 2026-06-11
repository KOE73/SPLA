using CommunityToolkit.Mvvm.ComponentModel;
using SPLA.Domain.Models;

namespace SPLA.UI.Avalonia.ViewModels.Messages;

/// <summary>
/// A mutable message that accumulates streaming text deltas.
/// Notifies the UI via <see cref="ObservableProperty"/> on each append.
/// </summary>
public partial class StreamingMessageViewModel : MessageViewModel
{
    [ObservableProperty]
    private string _streamingContent = string.Empty;

    // Expose as the polymorphic Content so templates bind automatically
    public override string Content => StreamingContent;
    public override bool HasContent => !string.IsNullOrWhiteSpace(StreamingContent);

    public StreamingMessageViewModel(ChatRole role) : base(role, string.Empty) { }

    /// <summary>Appends a delta chunk. MUST be called from the UI thread.</summary>
    public void Append(string delta)
    {
        StreamingContent += delta;
        OnPropertyChanged(nameof(Content));
        OnPropertyChanged(nameof(HasContent));
        OnPropertyChanged(nameof(IsVisibleMessage));
        OnPropertyChanged(nameof(IsMarkdown));
        OnPropertyChanged(nameof(IsPlainText));
    }

    /// <summary>Replaces the whole text at once (e.g. on Stop).</summary>
    public void SetFinal(string text)
    {
        StreamingContent = text;
        OnPropertyChanged(nameof(Content));
        OnPropertyChanged(nameof(HasContent));
        OnPropertyChanged(nameof(IsVisibleMessage));
        OnPropertyChanged(nameof(IsMarkdown));
        OnPropertyChanged(nameof(IsPlainText));
    }
}
