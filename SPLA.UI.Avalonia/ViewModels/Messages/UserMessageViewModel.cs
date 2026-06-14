using SPLA.Domain.Models;

namespace SPLA.UI.Avalonia.ViewModels.Messages;

public sealed class UserMessageViewModel(string content) : MessageViewModel(ChatRole.User, content)
{
    public override bool IsMarkdown => true;
    public override bool IsPlainText => false;
    public override bool IsError => false;
    public override bool IsToolCallNotice => false;
}
