using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SPLA.UI.Avalonia;

public partial class ConfirmDeleteChatWindow : Window
{
    public ConfirmDeleteChatWindow() : this(string.Empty) { }

    public ConfirmDeleteChatWindow(string chatTitle)
    {
        InitializeComponent();
        ExtendClientAreaTitleBarHeightHint = 30;
        ChatTitleBlock.Text = chatTitle;
        CancelBtn.Click += (_, _) => Close(false);
        DeleteBtn.Click += (_, _) => Close(true);
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e) => BeginMoveDrag(e);
    private void TitleBarClose_Click(object? sender, RoutedEventArgs e) => Close(false);
}
