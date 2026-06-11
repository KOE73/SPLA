using Avalonia.Controls;
using SPLA.UI.Avalonia.ViewModels;
using System.Collections.Specialized;

namespace SPLA.UI.Avalonia.Views.Chat;

public partial class ChatDiagnosticView : UserControl
{
    private MainWindowViewModel? _attachedVm;

    public ChatDiagnosticView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (_attachedVm != null)
        {
            _attachedVm.Messages.CollectionChanged -= Messages_CollectionChanged;
            _attachedVm = null;
        }

        if (DataContext is MainWindowViewModel vm)
        {
            _attachedVm = vm;
            _attachedVm.Messages.CollectionChanged += Messages_CollectionChanged;
            ScrollToEnd();
        }
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ScrollToEnd();
    }

    private void ScrollToEnd()
    {
        var scroll = this.FindControl<ScrollViewer>("ChatScrollViewer");
        if (scroll != null)
        {
            global::Avalonia.Threading.Dispatcher.UIThread.Post(() => scroll.ScrollToEnd(), global::Avalonia.Threading.DispatcherPriority.Background);
        }
    }
}
