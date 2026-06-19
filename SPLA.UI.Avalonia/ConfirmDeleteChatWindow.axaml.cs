using Avalonia.Controls;

namespace SPLA.UI.Avalonia;

public partial class ConfirmDeleteChatWindow : Window
{
    public ConfirmDeleteChatWindow() : this(string.Empty) { }

    public ConfirmDeleteChatWindow(string chatTitle)
    {
        InitializeComponent();
        ChatTitleBlock.Text = chatTitle;
        CancelBtn.Click += (_, _) => Close(false);
        DeleteBtn.Click += (_, _) => Close(true);
    }
}
