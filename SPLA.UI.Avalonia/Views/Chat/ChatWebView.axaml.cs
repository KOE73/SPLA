using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPLA.UI.Avalonia.Services.Chat;
using SPLA.UI.Avalonia.ViewModels.Chat;
using SPLA.MCP.Core.Permissions;
using SPLA.UI.Avalonia.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.Views.Chat;

public partial class ChatWebView : UserControl
{
    private readonly NativeWebView _browser;
    private readonly ILogger<ChatWebView>? _logger;

    // Full-render debounce (theme changes, resets)
    private readonly DispatcherTimer _fullRenderTimer;

    // Track which messages we're subscribed to, and their list index
    private readonly Dictionary<MessageViewModel, (PropertyChangedEventHandler Handler, int Index)> _messageHandlers = [];

    private ChatSessionViewModel? _viewModel;

    // True once the page is fully loaded and JS API is available
    private bool _pageReady;

    // Queued incremental ops waiting for page ready
    private readonly List<Func<Task>> _pendingOps = [];

    public ChatWebView()
    {
        InitializeComponent();

        _browser = this.FindControl<NativeWebView>("ChatBrowser")!;
        _logger = App.Services.GetService<ILogger<ChatWebView>>();

        _fullRenderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _fullRenderTimer.Tick += (_, _) => { _fullRenderTimer.Stop(); DoFullRender(); };

        DataContextChanged += (_, _) => AttachViewModel(DataContext as ChatSessionViewModel);
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        _browser.WebMessageReceived += (_, args) => _ = HandleWebMessageAsync(args);
        App.VisualResourcesChanged += OnVisualResourcesChanged;
    }

    // ── Lifecycle ──────────────────────────────────────────────────────────

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        AttachViewModel(DataContext as ChatSessionViewModel);
        QueueFullRender();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        AttachViewModel(null);
        _fullRenderTimer.Stop();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        App.VisualResourcesChanged -= OnVisualResourcesChanged;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnVisualResourcesChanged(object? sender, EventArgs e) => QueueFullRender();

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ChatSessionViewModel.ActiveProfile))
            QueueFullRender();
        else if (e.PropertyName is nameof(ChatSessionViewModel.ActiveClarify))
            EnqueueOp(SyncClarifyAsync);
    }

    private async Task SyncClarifyAsync()
    {
        var clarify = _viewModel?.ActiveClarify;
        if (clarify == null)
        {
            await InvokeJsAsync("window.__splaClearClarify()");
            return;
        }

        var data = new
        {
            question = clarify.Question,
            options = clarify.Options.Select(o => new { label = o.Label, description = o.Description }).ToArray()
        };
        var json = JsonSerializer.Serialize(data, JsonSerializerOptions.Web);
        var escaped = JsonSerializer.Serialize(json);
        await InvokeJsAsync($"window.__splaClarify({escaped})");
    }

    // ── ViewModel attachment ───────────────────────────────────────────────

    private void AttachViewModel(ChatSessionViewModel? viewModel)
    {
        if (ReferenceEquals(_viewModel, viewModel)) return;

        if (_viewModel != null)
        {
            _viewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            ClearAllMessageSubscriptions();
        }

        _viewModel = viewModel;

        if (_viewModel != null)
        {
            _viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            ResubscribeAllMessages();
        }

        // Chat session changed → full re-render
        QueueFullRender();
    }

    // ── Collection change handling ─────────────────────────────────────────

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewItems != null:
                foreach (MessageViewModel msg in e.NewItems)
                {
                    var index = _viewModel!.Messages.IndexOf(msg);
                    SubscribeMessage(msg, index);
                    EnqueueOp(() => AppendMessageAsync(msg, index));
                }
                break;

            case NotifyCollectionChangedAction.Remove when e.OldItems != null:
                foreach (MessageViewModel msg in e.OldItems)
                {
                    if (_messageHandlers.Remove(msg, out var entry))
                    {
                        msg.PropertyChanged -= entry.Handler;
                        EnqueueOp(() => RemoveMessageAsync(entry.Index));
                    }
                }
                break;

            default:
                // Reset, Replace, Move → full re-render
                ResubscribeAllMessages();
                QueueFullRender();
                break;
        }
    }

    // ── Message property subscriptions ────────────────────────────────────

    private void SubscribeMessage(MessageViewModel msg, int index)
    {
        if (_messageHandlers.ContainsKey(msg)) return;

        PropertyChangedEventHandler handler = (_, e) => OnMessagePropertyChanged(msg, e);
        _messageHandlers[msg] = (handler, index);
        msg.PropertyChanged += handler;
    }

    private void OnMessagePropertyChanged(MessageViewModel msg, PropertyChangedEventArgs e)
    {
        if (!_messageHandlers.TryGetValue(msg, out var entry)) return;

        if (msg is StreamingMessageViewModel streaming)
        {
            if (e.PropertyName == nameof(StreamingMessageViewModel.StreamingContent))
                EnqueueOp(() => AppendDeltaAsync(entry.Index, streaming));
            else if (e.PropertyName == nameof(StreamingMessageViewModel.StreamingReasoning))
                EnqueueOp(() => AppendReasoningAsync(entry.Index, streaming));
        }
        else if (e.PropertyName is nameof(MessageViewModel.Content)
                                 or nameof(MessageViewModel.RetentionIcon)
                                 or nameof(MessageViewModel.RetentionDescription)
                                 or nameof(PermissionMessageViewModel.IsAnswered))
        {
            EnqueueOp(() => AppendMessageAsync(msg, entry.Index));
        }
    }

    private void ResubscribeAllMessages()
    {
        ClearAllMessageSubscriptions();
        if (_viewModel == null) return;
        for (var i = 0; i < _viewModel.Messages.Count; i++)
            SubscribeMessage(_viewModel.Messages[i], i);
    }

    private void ClearAllMessageSubscriptions()
    {
        foreach (var (msg, entry) in _messageHandlers)
            msg.PropertyChanged -= entry.Handler;
        _messageHandlers.Clear();
    }

    // ── Incremental JS ops ─────────────────────────────────────────────────

    // Track last-seen content to avoid redundant delta calls during rapid streaming
    private readonly Dictionary<int, string> _lastStreamedContent = [];

    // Indices whose shell has been created (so reasoning can attach before any content arrives)
    private readonly HashSet<int> _shellEnsured = [];

    private async Task AppendMessageAsync(MessageViewModel msg, int index)
    {
        if (!msg.HasContent && !msg.HasReasoning) return;
        var json = ChatHtmlRenderer.SerializeMessage(msg, index);
        var escaped = JsonSerializer.Serialize(json); // JSON-encodes the string for JS
        await InvokeJsAsync($"window.__splaAppendMessage({escaped})");
        _shellEnsured.Add(index);
    }

    private async Task AppendReasoningAsync(int index, StreamingMessageViewModel streaming)
    {
        var current = streaming.StreamingReasoning;
        if (string.IsNullOrEmpty(current)) return;

        // Reasoning may stream before any content — make sure the message shell exists.
        if (!_shellEnsured.Contains(index) && !_lastStreamedContent.ContainsKey(index))
            await AppendMessageAsync(streaming, index);

        var escapedReasoning = JsonSerializer.Serialize(current);
        await InvokeJsAsync($"window.__splaSetReasoning({index}, {escapedReasoning})");
    }

    private async Task AppendDeltaAsync(int index, StreamingMessageViewModel streaming)
    {
        var current = streaming.StreamingContent;
        _lastStreamedContent.TryGetValue(index, out var prev);

        if (current == prev) return;

        if (string.IsNullOrEmpty(prev))
        {
            // First chunk — create the message shell first
            await AppendMessageAsync(streaming, index);
            _lastStreamedContent[index] = current;
            return;
        }

        var delta = current.Length > prev.Length ? current[prev.Length..] : string.Empty;
        _lastStreamedContent[index] = current;

        if (string.IsNullOrEmpty(delta)) return;

        var escapedIndex = index.ToString();
        var escapedDelta = JsonSerializer.Serialize(delta);
        await InvokeJsAsync($"window.__splaAppendDelta({escapedIndex}, {escapedDelta})");
    }

    private async Task RemoveMessageAsync(int index)
    {
        _lastStreamedContent.Remove(index);
        _shellEnsured.Remove(index);
        await InvokeJsAsync($"window.__splaRemoveMessage({index})");
    }

    // ── Full render (theme / session change) ──────────────────────────────

    private void QueueFullRender()
    {
        if (!IsLoaded) return;
        _fullRenderTimer.Stop();
        _fullRenderTimer.Start();
    }

    private void DoFullRender()
    {
        if (_viewModel == null) return;
        try
        {
            _pageReady = false;
            _pendingOps.Clear();
            _lastStreamedContent.Clear();
            _shellEnsured.Clear();
            var rendered = ChatHtmlRenderer.RenderToFile(_viewModel.Messages, CreateTheme(), _viewModel.ActiveProfile?.Id);
            _browser.Navigate(rendered.DocumentUri);
            // Page is considered ready immediately after navigation in NativeWebView
            _pageReady = true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to render web chat view.");
        }
    }

    // ── JS invocation ──────────────────────────────────────────────────────

    private void EnqueueOp(Func<Task> op)
    {
        if (!_pageReady)
        {
            _pendingOps.Add(op);
            return;
        }
        _ = op();
    }

    private async Task InvokeJsAsync(string script)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() => _browser.InvokeScript(script));
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "JS invocation failed: {Script}", script[..Math.Min(80, script.Length)]);
        }
    }

    // ── Web → host messages ────────────────────────────────────────────────

    private async Task HandleWebMessageAsync(object args)
    {
        try
        {
            var messageJson = ExtractWebMessage(args);
            if (string.IsNullOrWhiteSpace(messageJson)) return;

            using var document = JsonDocument.Parse(messageJson);
            var root = document.RootElement;
            var action = root.TryGetProperty("action", out var actionProp) ? actionProp.GetString() : null;
            var index = root.TryGetProperty("index", out var indexProp) ? indexProp.GetInt32() : -1;

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                switch (action)
                {
                    case "copy": await CopyMessageAsync(index); break;
                    case "delete": DeleteMessage(index); break;
                    case "copyText":
                        var text = root.TryGetProperty("text", out var textProp) ? textProp.GetString() : null;
                        await CopyTextAsync(text);
                        break;
                    case "permission":
                        var decision = root.TryGetProperty("decision", out var decisionProp) ? decisionProp.GetString() : null;
                        HandlePermissionDecision(index, decision);
                        break;
                    case "clarify":
                        string? choice = null;
                        if (root.TryGetProperty("choice", out var choiceProp) && choiceProp.ValueKind != JsonValueKind.Null)
                            choice = choiceProp.GetString();
                        HandleClarifyChoice(choice);
                        break;
                }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to handle web chat command.");
        }
    }

    private async Task CopyMessageAsync(int index)
    {
        if (_viewModel == null || index < 0 || index >= _viewModel.Messages.Count) return;
        await CopyTextAsync(_viewModel.Messages[index].Content);
    }

    private void DeleteMessage(int index)
    {
        if (_viewModel == null || index < 0 || index >= _viewModel.Messages.Count) return;
        var msg = _viewModel.Messages[index];
        if (_viewModel.DeleteMessageCommand.CanExecute(msg))
            _viewModel.DeleteMessageCommand.Execute(msg);
    }

    private void HandlePermissionDecision(int index, string? decision)
    {
        if (_viewModel == null || index < 0 || index >= _viewModel.Messages.Count) return;
        if (_viewModel.Messages[index] is not PermissionMessageViewModel perm) return;
        switch (decision)
        {
            case "allowRemember": perm.AllowRememberCommand.Execute(null); break;
            case "allowOnce":     perm.AllowOnceCommand.Execute(null);     break;
            case "deny":          perm.DenyCommand.Execute(null);          break;
        }
    }

    private void HandleClarifyChoice(string? choice)
    {
        var clarify = _viewModel?.ActiveClarify;
        if (clarify == null) return;
        if (choice == null)
        {
            clarify.DismissCommand.Execute(null);
            return;
        }
        var option = clarify.Options.FirstOrDefault(o => o.Label == choice);
        option?.ChooseCommand.Execute(null);
    }

    private async Task CopyTextAsync(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null) await clipboard.SetTextAsync(text);
    }

    private static string? ExtractWebMessage(object args)
    {
        if (args is string text) return text;
        foreach (var prop in new[] { "Body", "Message", "Data", "WebMessageAsJson", "WebMessageAsString", "Source" })
        {
            var value = args.GetType().GetProperty(prop)?.GetValue(args)?.ToString();
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }
        return args.ToString();
    }

    // ── Theme reading ──────────────────────────────────────────────────────

    private static WebChatTheme CreateTheme() => new(
        ReadBrush("AppBackgroundBrush", "#FDFBF7"),
        ReadBrush("PanelBackgroundBrush", "#F5F2EB"),
        ReadBrush("PanelBorderBrush", "#E8E2D2"),
        ReadBrush("TextForegroundBrush", "#4A3B2C"),
        ReadBrush("SubTextForegroundBrush", "#8A7B6C"),
        ReadBrush("AccentBrush", "#E18F2F"),
        ReadBrush("MessageBackgroundBrush", "#FFFDF9"),
        ReadBrush("UserBubbleBackgroundBrush", "#EFEBE0"),
        ReadBrush("AiBubbleBackgroundBrush", "#FFF2DC"),
        ReadBrush("SystemBubbleBackgroundBrush", "#EAF2E0"),
        ReadBrush("ToolBubbleBackgroundBrush", "#F5EBEF"),
        ReadBrush("ErrorBubbleBackgroundBrush", "#FBECE9"),
        ReadThickness("StandardMargin", "10px"),
        ReadThickness("StandardPadding", "15px"),
        ReadThickness("CompactMargin", "5px"),
        ReadDouble("StandardSpacing", "10px"),
        ReadThickness("ChatBubbleMargin", "3px 10px"),
        ReadThickness("UserChatBubbleMargin", "3px 10px 3px 28px"),
        ReadThickness("AiChatBubbleMargin", "3px 28px 3px 10px"),
        ReadCornerRadius("StandardCornerRadius", "8px"),
        ReadDouble("HeaderFontSize", "18px"),
        ReadDouble("BaseFontSize", "14px"),
        ReadDouble("SmallFontSize", "12px"));

    private static string ReadBrush(string key, string fallback)
    {
        if (Application.Current?.TryGetResource(key, null, out var value) != true) return fallback;
        return value switch
        {
            ISolidColorBrush b => ToCss(b.Color),
            Color c => ToCss(c),
            _ => fallback
        };
    }

    private static string ToCss(Color c) => c.A == byte.MaxValue
        ? $"#{c.R:X2}{c.G:X2}{c.B:X2}"
        : $"rgba({c.R}, {c.G}, {c.B}, {c.A / 255d:0.###})";

    private static string ReadThickness(string key, string fallback)
    {
        if (Application.Current?.TryGetResource(key, null, out var value) != true) return fallback;
        return value switch
        {
            Thickness t => $"{t.Top:0.###}px {t.Right:0.###}px {t.Bottom:0.###}px {t.Left:0.###}px",
            double d => $"{d:0.###}px",
            _ => fallback
        };
    }

    private static string ReadCornerRadius(string key, string fallback)
    {
        if (Application.Current?.TryGetResource(key, null, out var value) != true) return fallback;
        return value switch
        {
            CornerRadius r => $"{r.TopLeft:0.###}px",
            double d => $"{d:0.###}px",
            _ => fallback
        };
    }

    private static string ReadDouble(string key, string fallback)
    {
        if (Application.Current?.TryGetResource(key, null, out var value) != true) return fallback;
        return value is double d ? $"{d:0.###}px" : fallback;
    }
}
