using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SPLA.UI.Avalonia.ViewModels.Debug;

namespace SPLA.UI.Avalonia;

public partial class KvDebugWindow : Window
{
    public KvDebugWindow()
    {
        InitializeComponent();
        ExtendClientAreaTitleBarHeightHint = 30;
    }

    public KvDebugWindow(KvDebugWindowViewModel vm) : this()
    {
        DataContext = vm;
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e) => BeginMoveDrag(e);
    private void MinimizeButton_Click(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void CloseButton_Click(object? sender, RoutedEventArgs e) => Close();
}
