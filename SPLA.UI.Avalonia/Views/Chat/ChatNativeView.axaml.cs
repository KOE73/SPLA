using Avalonia.Controls;
using Avalonia.Threading;
using SPLA.UI.Avalonia.ViewModels.Chat;
using SPLA.UI.Avalonia.ViewModels.Messages;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SPLA.UI.Avalonia.Views.Chat;

public partial class ChatNativeView : UserControl
{
    private ChatSessionViewModel? _viewModel;
    private StreamingMessageViewModel? _activeStreaming;

    public ChatNativeView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        DetachViewModel();
        if (DataContext is ChatSessionViewModel vm)
            AttachViewModel(vm);
    }

    private void AttachViewModel(ChatSessionViewModel vm)
    {
        _viewModel = vm;
        vm.Messages.CollectionChanged += OnMessagesCollectionChanged;
        vm.PropertyChanged += OnViewModelPropertyChanged;
        TrackLastStreamingMessage();
        ScrollToEnd();
    }

    private void DetachViewModel()
    {
        if (_viewModel == null) return;
        _viewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        UntrackStreamingMessage();
        _viewModel = null;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ChatSessionViewModel.ActiveProfile))
            ScrollToEnd();
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UntrackStreamingMessage();
        TrackLastStreamingMessage();
        ScrollToEnd();
    }

    private void TrackLastStreamingMessage()
    {
        if (_viewModel?.Messages.Count > 0 &&
            _viewModel.Messages[^1] is StreamingMessageViewModel streaming)
        {
            _activeStreaming = streaming;
            streaming.PropertyChanged += OnStreamingPropertyChanged;
        }
    }

    private void UntrackStreamingMessage()
    {
        if (_activeStreaming == null) return;
        _activeStreaming.PropertyChanged -= OnStreamingPropertyChanged;
        _activeStreaming = null;
    }

    private void OnStreamingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(StreamingMessageViewModel.StreamingContent)
                            or nameof(StreamingMessageViewModel.Content))
            ScrollToEnd();
    }

    private void ScrollToEnd()
    {
        var scroll = this.FindControl<ScrollViewer>("ChatScrollViewer");
        if (scroll != null)
            Dispatcher.UIThread.Post(() => scroll.ScrollToEnd(), DispatcherPriority.Background);
    }
}
