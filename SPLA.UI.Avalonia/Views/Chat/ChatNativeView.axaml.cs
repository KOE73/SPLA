using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ChatNativeView>? _logger;

    private static int _instanceCounter;
    private readonly int _instanceId = System.Threading.Interlocked.Increment(ref _instanceCounter);

    public ChatNativeView()
    {
        InitializeComponent();
        _logger = App.Services?.GetService<ILogger<ChatNativeView>>();
        _logger?.LogInformation("[CHATBIND] Native CTOR #{Inst} (total={Total})", _instanceId, _instanceCounter);
        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _logger?.LogInformation("[CHATBIND] Native #{Inst} ATTACHED to tree (vm={Id})", _instanceId, _viewModel?.Id);
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _logger?.LogInformation("[CHATBIND] Native #{Inst} DETACHED from tree (vm={Id})", _instanceId, _viewModel?.Id);
        base.OnDetachedFromVisualTree(e);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        var newId = (DataContext as ChatSessionViewModel)?.Id;
        _logger?.LogInformation("[CHATBIND] Native #{Inst} DataContextChanged: {Old} -> {New} (visible={Vis})",
            _instanceId, _viewModel?.Id, newId, IsVisible);

        // Defer the actual rebind off the property-change notification: the ItemsSource swap tears down
        // markdown containers and can throw from inside Avalonia's property pipeline; running it on the
        // dispatcher isolates that from the binding system. The ReferenceEquals guard coalesces duplicates.
        Dispatcher.UIThread.Post(SyncToDataContext, DispatcherPriority.Render);
    }

    private void SyncToDataContext()
    {
        var vm = DataContext as ChatSessionViewModel;
        if (ReferenceEquals(vm, _viewModel)) return;

        _logger?.LogInformation("[CHATBIND] Native #{Inst} SyncToDataContext: {Old} -> {New}",
            _instanceId, _viewModel?.Id, vm?.Id);

        // Unsubscribe from the outgoing chat (cheap, no container teardown).
        if (_viewModel != null)
        {
            _viewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            UntrackStreamingMessage();
            UntrackPendingPermission();
        }
        _viewModel = vm;

        // Swap the list contents in ONE assignment (old -> new), not old -> null -> new: that halves
        // the container teardowns. Wrapped because Markdown.Avalonia 12.0.0-a1's ColorTextBlock throws
        // InvalidCastException while tearing down markdown containers (theme-variant re-eval fires on
        // logical-parent clear). ItemsSource is assigned before the teardown notifications run, so even
        // if the third-party renderer throws mid-teardown the list still targets the correct chat — we
        // swallow the error to keep the app alive instead of aborting the rebind (which left the
        // previous chat on screen and, once deferred, crashed the process).
        //
        // TEMPORARY — defensive workaround for the Markdown.Avalonia ALPHA (12.0.0-a1; bumped to a3 as a
        // candidate fix). Once Markdown.Avalonia ships a stable release, RE-VERIFY whether the teardown
        // still throws and, if fixed, REMOVE this try/catch (and the deferred-rebind isolation) and go
        // back to a plain ItemsSource assignment.
        var list = this.FindControl<ItemsControl>("MessagesList");
        if (list != null)
        {
            try
            {
                list.ItemsSource = vm?.Messages;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[CHATBIND] Native #{Inst} ItemsSource swap threw during markdown teardown", _instanceId);
            }
        }

        if (vm != null)
        {
            vm.Messages.CollectionChanged += OnMessagesCollectionChanged;
            vm.PropertyChanged += OnViewModelPropertyChanged;
            _logger?.LogInformation("[CHATBIND] Native #{Inst} attached vm={Id} msgCount={Count}",
                _instanceId, vm.Id, vm.Messages.Count);
            TrackLastStreamingMessage();
            RefreshPendingPermission();
            ScrollToEnd();
        }
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
