using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPLA.UI.Avalonia.Services;

namespace SPLA.UI.Avalonia;

/// <summary>
/// A thin client: hosts a WebView pointed at an embedded <c>SPLA.CLI serve</c> instance on loopback.
/// Proves the end-state architecture inside the native shell — the same web UI a browser would load,
/// driving the same agent over a WebSocket. Opt-in and additive; it does not replace the existing
/// in-process chat UI. The child service is started on open and killed on close.
/// </summary>
public partial class ThinClientWindow : Window
{
    private readonly EmbeddedServiceLauncher _launcher = new();
    private readonly ILogger<ThinClientWindow>? _logger;
    private string? _url;

    public ThinClientWindow()
    {
        InitializeComponent();
        ExtendClientAreaTitleBarHeightHint = 30;
        _logger = App.Services.GetService<ILogger<ThinClientWindow>>();

        Opened += OnOpened;
        Closed += OnClosed;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        try
        {
            // Run the embedded service in the current project's workspace so it resolves the same
            // .spla settings/connections/chats as the native app.
            var workspace = App.ResolvedSettings.WorkspacePath;
            _url = await _launcher.StartAsync(workspace);
            StatusText.Text = "— " + _url;
            Browser.Navigate(new Uri(_url));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start embedded service for thin client.");
            StatusText.Text = "— failed: " + ex.Message;
        }
    }

    private void OnClosed(object? sender, EventArgs e) => _launcher.Dispose();

    private void ReloadButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_url != null) Browser.Navigate(new Uri(_url));
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e) => BeginMoveDrag(e);
    private void MinimizeButton_Click(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void CloseButton_Click(object? sender, RoutedEventArgs e) => Close();
}
