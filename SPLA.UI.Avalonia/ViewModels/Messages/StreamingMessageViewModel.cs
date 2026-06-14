using CommunityToolkit.Mvvm.ComponentModel;
using SPLA.Domain.Models;

namespace SPLA.UI.Avalonia.ViewModels.Messages;

public partial class StreamingMessageViewModel : AssistantMessageViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Content))]
    [NotifyPropertyChangedFor(nameof(HasContent))]
    [NotifyPropertyChangedFor(nameof(IsVisibleMessage))]
    private string _streamingContent = string.Empty;

    public override string Content => StreamingContent;
    public override bool HasContent => !string.IsNullOrWhiteSpace(StreamingContent);

    public StreamingMessageViewModel() : base(string.Empty) { }

    public void Append(string delta) => StreamingContent += delta;

    public void SetFinal(string text) => StreamingContent = text;
}
