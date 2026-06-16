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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Reasoning))]
    [NotifyPropertyChangedFor(nameof(HasReasoning))]
    private string _streamingReasoning = string.Empty;

    public override string Content => StreamingContent;
    public override bool HasContent => !string.IsNullOrWhiteSpace(StreamingContent);

    public override string? Reasoning => StreamingReasoning;
    public override bool HasReasoning => !string.IsNullOrWhiteSpace(StreamingReasoning);

    public StreamingMessageViewModel() : base(string.Empty) { }

    public void Append(string delta) => StreamingContent += delta;

    public void SetFinal(string text) => StreamingContent = text;

    public void AppendReasoning(string delta) => StreamingReasoning += delta;

    public void SetFinalReasoning(string text) => StreamingReasoning = text;
}
