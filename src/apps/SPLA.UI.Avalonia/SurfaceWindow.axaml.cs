using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SPLA.UI.Avalonia;

/// <summary>
/// A standalone OS window hosting one web <em>surface</em> (e.g. <c>debug</c>, <c>memory</c>) over the
/// shared embedded service: <c>&lt;serviceUrl&gt;/?surface=&lt;name&gt;</c>. This is how the multi-window
/// "tear off panels into the corners" capability survives the move to a single web renderer — Avalonia
/// provides the frame, the web client provides the content. Same agent, same renderer, many windows.
/// </summary>
public partial class SurfaceWindow : Window
{
    private string _surface = "debug";
    private string? _query;
    private string? _url;

    public SurfaceWindow()
    {
        InitializeComponent();
        Opened += OnOpened;
    }

    /// <param name="query">Extra pre-encoded query params for the surface (e.g. "host=x&amp;project=y"),
    /// used by tear-offs that need context — an SSH terminal's host, the focused project.</param>
    public SurfaceWindow(string surface, string? title = null, string? query = null) : this()
    {
        _surface = surface;
        _query = query;
        var label = title ?? surface;
        Title = "SPLA — " + label;
        TitleText.Text = "— " + label;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        try
        {
            Helpers.WebViewBridge.Attach(Browser);
            var baseUrl = await App.ServiceUrlAsync();
            _url = baseUrl.TrimEnd('/') + "/?surface=" + Uri.EscapeDataString(_surface)
                 + (string.IsNullOrEmpty(_query) ? "" : "&" + _query);
            Browser.Navigate(new Uri(_url));
        }
        catch (Exception ex)
        {
            TitleText.Text = "— failed: " + ex.Message;
        }
    }

    private void Reload_Click(object? sender, RoutedEventArgs e)
    {
        if (_url != null) Browser.Navigate(new Uri(_url));
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }

    private void Min_Click(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Max_Click(object? sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void Close_Click(object? sender, RoutedEventArgs e) => Close();
}
