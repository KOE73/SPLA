using Avalonia.Controls;
using SPLA.UI.Avalonia.ViewModels.Debug;

namespace SPLA.UI.Avalonia;

public partial class ContextDebugWindow : Window
{
    public ContextDebugWindow()
    {
        InitializeComponent();
    }

    public ContextDebugWindow(ContextSnapshotViewModel vm) : this()
    {
        DataContext = vm;
    }
}
