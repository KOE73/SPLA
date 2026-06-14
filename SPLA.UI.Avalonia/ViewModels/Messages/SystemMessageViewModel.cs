using SPLA.Domain.Models;

namespace SPLA.UI.Avalonia.ViewModels.Messages;

public sealed class SystemMessageViewModel(string content) : MessageViewModel(ChatRole.System, content)
{
    public override bool IsMarkdown => false;
    public override bool IsPlainText => true;
    public override bool IsError => false;
    public override bool IsToolCallNotice => false;
}
