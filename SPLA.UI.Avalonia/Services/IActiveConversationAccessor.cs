namespace SPLA.UI.Avalonia.Services;

public interface IActiveConversationAccessor
{
    IConversationInput? CurrentInput { get; set; }
}

public interface IConversationInput
{
    string Text { get; set; }
    void InsertText(string text);
}

public sealed class ActiveConversationAccessor : IActiveConversationAccessor
{
    public IConversationInput? CurrentInput { get; set; }
}
