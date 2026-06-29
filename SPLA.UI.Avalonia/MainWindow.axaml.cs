using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SPLA.UI.Avalonia.Helpers;

namespace SPLA.UI.Avalonia;

/// <summary>
/// The main window is a thin shell: Avalonia manages only the OS frame and chrome; the entire content
/// is the web client (one renderer) hosted in a <c>NativeWebView</c> over the shared embedded service
/// (<see cref="App.ServiceUrlAsync"/>). There is no in-process agent stack — the service owns chats,
/// tools, plugins and secrets. Auxiliary panels (debug, etc.) open as separate <see cref="SurfaceWindow"/>
/// frames so they can be dragged around the screen, while still rendering the same web surfaces.
/// </summary>
public partial class MainWindow : Window
{
    private string? _url;

    public MainWindow()
    {
        InitializeComponent();
        TitleText.Text = string.IsNullOrWhiteSpace(App.ProjectFilePath) ? "no project" : App.ProjectFilePath;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        var settings = App.ResolvedSettings;
        var icon = IconGenerator.GenerateIcon(settings.WorkspacePath, settings.ProjectName);
        if (icon != null) Icon = icon;

        try
        {
            _url = await App.ServiceUrlAsync();
            Browser.Navigate(new Uri(_url));
        }
        catch (Exception ex)
        {
            TitleText.Text = "— web failed: " + ex.Message;
        }
    }

    private void OpenDebugSurface_Click(object? sender, RoutedEventArgs e)
        => new SurfaceWindow("debug", "Debug").Show(this);

    private void OpenSettingsSurface_Click(object? sender, RoutedEventArgs e)
        => new SurfaceWindow("settings", "Settings").Show(this);

    private void Reload_Click(object? sender, RoutedEventArgs e)
    {
        if (_url != null) Browser.Navigate(new Uri(_url));
    }

    // ── Window chrome ─────────────────────────────────────────────────────────
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        if (MaximizeButton != null)
            MaximizeButton.Content = WindowState == WindowState.Maximized ? "❐" : "□";
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        App.ShutdownService();
        base.OnClosed(e);
    }
}
