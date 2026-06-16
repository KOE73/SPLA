using SPLA.Domain.Models;

namespace SPLA.UI.Avalonia.ViewModels.Messages;

public sealed class SystemMessageViewModel(string content, bool isSystemPrompt = false) : MessageViewModel(ChatRole.System, content)
{
    public override bool IsMarkdown => false;
    public override bool IsPlainText => true;
    public override bool IsError => false;
    public override bool IsToolCallNotice => false;

    /// <summary>
    /// True for the actual system prompt injected into every request; false for UI-only notices.
    /// </summary>
    public bool IsSystemPrompt { get; } = isSystemPrompt;
}
