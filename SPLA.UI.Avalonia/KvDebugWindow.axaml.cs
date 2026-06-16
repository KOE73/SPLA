using Avalonia.Controls;
using SPLA.UI.Avalonia.ViewModels.Debug;

namespace SPLA.UI.Avalonia;

public partial class KvDebugWindow : Window
{
    public KvDebugWindow()
    {
        InitializeComponent();
    }

    public KvDebugWindow(KvDebugWindowViewModel vm) : this()
    {
        DataContext = vm;
    }
}
