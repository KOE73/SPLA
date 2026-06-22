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
    private PermissionMessageViewModel? _pendingPermission;

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
        // Rebind the list explicitly — the declarative ItemsSource binding did not refresh on
        // DataContext change, leaving the previous chat's messages on screen.
        var list = this.FindControl<ItemsControl>("MessagesList");
        if (list != null) list.ItemsSource = vm.Messages;

        vm.Messages.CollectionChanged += OnMessagesCollectionChanged;
        vm.PropertyChanged += OnViewModelPropertyChanged;
        TrackLastStreamingMessage();
        RefreshPendingPermission();
        ScrollToEnd();
    }

    private void DetachViewModel()
    {
        if (_viewModel == null) return;
        _viewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        UntrackStreamingMessage();
        UntrackPendingPermission();

        var list = this.FindControl<ItemsControl>("MessagesList");
        if (list != null) list.ItemsSource = null;

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
        RefreshPendingPermission();
        ScrollToEnd();
    }

    // ── Docked permission bar (native-only) ──────────────────────────────────
    private void RefreshPendingPermission()
    {
        PermissionMessageViewModel? pending = null;
        if (_viewModel != null)
        {
            for (var i = _viewModel.Messages.Count - 1; i >= 0; i--)
            {
                if (_viewModel.Messages[i] is PermissionMessageViewModel p && !p.IsAnswered)
                {
                    pending = p;
                    break;
                }
            }
        }

        if (ReferenceEquals(pending, _pendingPermission)) return;

        UntrackPendingPermission();
        _pendingPermission = pending;

        var dock = this.FindControl<Border>("PermissionDock");
        if (dock != null)
        {
            dock.DataContext = pending;
            dock.IsVisible = pending != null;
        }

        if (pending != null)
            pending.PropertyChanged += OnPendingPermissionPropertyChanged;
    }

    private void UntrackPendingPermission()
    {
        if (_pendingPermission == null) return;
        _pendingPermission.PropertyChanged -= OnPendingPermissionPropertyChanged;
        _pendingPermission = null;
    }

    private void OnPendingPermissionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PermissionMessageViewModel.IsAnswered))
            RefreshPendingPermission();
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
