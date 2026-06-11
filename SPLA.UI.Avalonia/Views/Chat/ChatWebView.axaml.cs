using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPLA.UI.Avalonia.Services.Chat;
using SPLA.UI.Avalonia.ViewModels;
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
    private readonly DispatcherTimer _renderTimer;
    private readonly Dictionary<MessageViewModel, PropertyChangedEventHandler> _messageHandlers = [];
    private readonly ILogger<ChatWebView>? _logger;
    private MainWindowViewModel? _viewModel;

    public ChatWebView()
    {
        InitializeComponent();

        _browser = this.FindControl<NativeWebView>("ChatBrowser")!;
        _logger = App.Services.GetService<ILogger<ChatWebView>>();
        _renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
        _renderTimer.Tick += (_, _) =>
        {
            _renderTimer.Stop();
            RenderNow();
        };

        DataContextChanged += (_, _) => AttachViewModel(DataContext as MainWindowViewModel);
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        _browser.WebMessageReceived += (_, args) => _ = HandleWebMessageAsync(args);
        App.VisualResourcesChanged += OnVisualResourcesChanged;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        AttachViewModel(DataContext as MainWindowViewModel);
        QueueRender();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        AttachViewModel(null);
        _renderTimer.Stop();
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        App.VisualResourcesChanged -= OnVisualResourcesChanged;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnVisualResourcesChanged(object? sender, EventArgs e) => QueueRender();

    private void AttachViewModel(MainWindowViewModel? viewModel)
    {
        if (ReferenceEquals(_viewModel, viewModel)) return;

        if (_viewModel != null)
        {
            _viewModel.Messages.CollectionChanged -= MessagesCollectionChanged;
            ClearMessageSubscriptions();
        }

        _viewModel = viewModel;

        if (_viewModel != null)
        {
            _viewModel.Messages.CollectionChanged += MessagesCollectionChanged;
            RefreshMessageSubscriptions();
        }

        QueueRender();
    }

    private void MessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var message in e.OldItems.OfType<MessageViewModel>())
            {
                RemoveMessageSubscription(message);
            }
        }

        if (e.NewItems != null)
        {
            foreach (var message in e.NewItems.OfType<MessageViewModel>())
            {
                AddMessageSubscription(message);
            }
        }

        if (e.Action is NotifyCollectionChangedAction.Reset)
        {
            RefreshMessageSubscriptions();
        }

        QueueRender();
    }

    private void RefreshMessageSubscriptions()
    {
        ClearMessageSubscriptions();
        if (_viewModel == null) return;

        foreach (var message in _viewModel.Messages)
        {
            AddMessageSubscription(message);
        }
    }

    private void AddMessageSubscription(MessageViewModel message)
    {
        if (_messageHandlers.ContainsKey(message)) return;

        PropertyChangedEventHandler handler = (_, e) =>
        {
            if (e.PropertyName is nameof(MessageViewModel.Content)
                or nameof(MessageViewModel.RetentionIcon)
                or nameof(MessageViewModel.RetentionDescription)
                or nameof(MessageViewModel.IsMarkdown)
                or nameof(MessageViewModel.IsPlainText))
            {
                QueueRender();
            }
        };

        _messageHandlers[message] = handler;
        message.PropertyChanged += handler;
    }

    private void RemoveMessageSubscription(MessageViewModel message)
    {
        if (!_messageHandlers.Remove(message, out var handler)) return;
        message.PropertyChanged -= handler;
    }

    private void ClearMessageSubscriptions()
    {
        foreach (var (message, handler) in _messageHandlers)
        {
            message.PropertyChanged -= handler;
        }

        _messageHandlers.Clear();
    }

    private void QueueRender()
    {
        if (!IsLoaded) return;

        _renderTimer.Stop();
        _renderTimer.Start();
    }

    private void RenderNow()
    {
        if (_viewModel == null) return;

        try
        {
            var renderedChat = ChatHtmlRenderer.RenderToFile(_viewModel.Messages, CreateTheme());
            _browser.Navigate(renderedChat.DocumentUri);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to render web chat view.");
        }
    }

    private async Task HandleWebMessageAsync(object args)
    {
        try
        {
            var messageJson = ExtractWebMessage(args);
            if (string.IsNullOrWhiteSpace(messageJson)) return;

            using var document = JsonDocument.Parse(messageJson);
            var root = document.RootElement;
            var action = root.TryGetProperty("action", out var actionProperty) ? actionProperty.GetString() : null;
            var index = root.TryGetProperty("index", out var indexProperty) ? indexProperty.GetInt32() : -1;

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                switch (action)
                {
                    case "copy":
                        await CopyMessageAsync(index);
                        break;
                    case "delete":
                        DeleteMessage(index);
                        break;
                    case "copyText":
                        var text = root.TryGetProperty("text", out var textProperty) ? textProperty.GetString() : null;
                        await CopyTextAsync(text);
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
        var message = GetMessageByIndex(index);
        if (message == null) return;

        await CopyTextAsync(message.Content);
    }

    private void DeleteMessage(int index)
    {
        var message = GetMessageByIndex(index);
        if (message == null || _viewModel == null) return;

        if (_viewModel.DeleteMessageCommand.CanExecute(message))
        {
            _viewModel.DeleteMessageCommand.Execute(message);
        }
    }

    private MessageViewModel? GetMessageByIndex(int index)
    {
        if (_viewModel == null || index < 0 || index >= _viewModel.Messages.Count) return null;
        return _viewModel.Messages[index];
    }

    private async Task CopyTextAsync(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(text);
        }
    }

    private static string? ExtractWebMessage(object args)
    {
        if (args is string text) return text;

        foreach (var propertyName in new[] { "Body", "Message", "Data", "WebMessageAsJson", "WebMessageAsString", "Source" })
        {
            var property = args.GetType().GetProperty(propertyName);
            var value = property?.GetValue(args)?.ToString();
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }

        return args.ToString();
    }

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
            ISolidColorBrush solidColorBrush => ToCss(solidColorBrush.Color),
            Color color => ToCss(color),
            _ => fallback
        };
    }

    private static string ToCss(Color color) => color.A == byte.MaxValue
        ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
        : $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255d:0.###})";

    private static string ReadThickness(string key, string fallback)
    {
        if (Application.Current?.TryGetResource(key, null, out var value) != true) return fallback;

        return value switch
        {
            Thickness thickness => ToCss(thickness),
            double number => $"{number:0.###}px",
            _ => fallback
        };
    }

    private static string ReadCornerRadius(string key, string fallback)
    {
        if (Application.Current?.TryGetResource(key, null, out var value) != true) return fallback;

        return value switch
        {
            CornerRadius radius => $"{radius.TopLeft:0.###}px",
            double number => $"{number:0.###}px",
            _ => fallback
        };
    }

    private static string ReadDouble(string key, string fallback)
    {
        if (Application.Current?.TryGetResource(key, null, out var value) != true) return fallback;
        return value is double number ? $"{number:0.###}px" : fallback;
    }

    private static string ToCss(Thickness thickness) =>
        $"{thickness.Top:0.###}px {thickness.Right:0.###}px {thickness.Bottom:0.###}px {thickness.Left:0.###}px";
}
