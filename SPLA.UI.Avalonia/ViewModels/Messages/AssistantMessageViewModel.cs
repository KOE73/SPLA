using SPLA.Domain.Models;

namespace SPLA.UI.Avalonia.ViewModels.Messages;

public class AssistantMessageViewModel(string content) : MessageViewModel(ChatRole.Assistant, content)
{
    public override bool IsMarkdown => true;
    public override bool IsPlainText => false;
    public override bool IsError => false;
    public override bool IsToolCallNotice => false;
}
