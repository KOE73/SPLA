using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SPLA.Domain.Settings;
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
        // ApplyProjectTitle is the single source of truth for BOTH this in-window text and the OS
        // Window.Title — it used to only touch Window.Title, so switching projects via the web
        // client's in-page ProjectPicker left this custom title bar text stuck on the stale path
        // while the OS taskbar correctly updated. WebViewBridge re-invokes it whenever that happens.
        var startupLabel = App.ResolvedSettings.ProjectName
            ?? (App.ProjectFilePath is { } p ? Path.GetFileNameWithoutExtension(p) : null);
        ApplyProjectTitle(startupLabel);
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        var settings = App.ResolvedSettings;
        var icon = IconGenerator.GenerateIcon(settings.WorkspacePath, settings.ProjectName);
        if (icon != null) Icon = icon;

        try
        {
            Helpers.WebViewBridge.Attach(Browser, ApplyProjectTitle);
            _url = await App.ServiceUrlAsync();
            Browser.Navigate(new Uri(_url));
        }
        catch (Exception ex)
        {
            TitleText.Text = "— web failed: " + ex.Message;
        }
    }

    // ── New / Open / Recent project ───────────────────────────────────────────
    // Each project = its own window/process (matches the one-embedded-service-per-process model —
    // see App.ServiceUrlAsync). Opening/creating a project therefore launches a fresh instance of
    // this same executable pointed at that .spla file, rather than switching in place.
    private void OpenProjectMenu_Click(object? sender, RoutedEventArgs e)
    {
        var flyout = new MenuFlyout();

        var newItem = new MenuItem { Header = "New Project…" };
        newItem.Click += async (_, _) => await NewProjectAsync();
        flyout.Items.Add(newItem);

        var openItem = new MenuItem { Header = "Open Project…" };
        openItem.Click += async (_, _) => await OpenProjectAsync();
        flyout.Items.Add(openItem);

        var recent = ConfigLoader.LoadRecentProjects();
        if (recent.Count > 0)
        {
            flyout.Items.Add(new Separator());
            foreach (var path in recent.Take(8))
            {
                var item = new MenuItem { Header = Path.GetFileNameWithoutExtension(path) };
                ToolTip.SetTip(item, path);
                item.Click += (_, _) => LaunchProject(path);
                flyout.Items.Add(item);
            }
        }

        flyout.ShowAt((Control)sender!);
    }

    private async Task NewProjectAsync()
    {
        var storage = StorageProvider;
        if (storage == null) return;

        var folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose a folder for the new project", AllowMultiple = false
        });
        var dir = folders.FirstOrDefault()?.TryGetLocalPath();
        if (dir == null) return;

        var name = Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar));
        var manifestPath = Path.Combine(dir, name + ".spla");
        if (!File.Exists(manifestPath))
        {
            ConfigLoader.SaveProject(
                new SplaProject { Name = name, Ignore = [.. ConfigLoader.DefaultIgnorePatterns] },
                manifestPath);
        }
        LaunchProject(manifestPath);
    }

    private async Task OpenProjectAsync()
    {
        var storage = StorageProvider;
        if (storage == null) return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open project",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("SPLA project") { Patterns = ["*.spla"] }]
        });
        var path = files.FirstOrDefault()?.TryGetLocalPath();
        if (path != null) LaunchProject(path);
    }

    private void LaunchProject(string manifestPath)
    {
        try
        {
            var (exe, args) = ResolveSelfInvocation();
            var psi = new ProcessStartInfo { FileName = exe, UseShellExecute = false };
            foreach (var a in args) psi.ArgumentList.Add(a);
            psi.ArgumentList.Add(manifestPath);
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            TitleText.Text = "— launch failed: " + ex.Message;
        }
    }

    /// <summary>Finds this same app to relaunch: a published SPLA.UI.Avalonia.exe next to us, or the
    /// dll run via dotnet (dev tree) — mirrors <c>EmbeddedServiceLauncher.ResolveCliInvocation</c>.</summary>
    private static (string Exe, string[] Args) ResolveSelfInvocation()
    {
        var baseDir = AppContext.BaseDirectory;

        var exe = Path.Combine(baseDir, "SPLA.UI.Avalonia.exe");
        if (File.Exists(exe)) return (exe, []);

        var dll = Path.Combine(baseDir, "SPLA.UI.Avalonia.dll");
        if (File.Exists(dll)) return ("dotnet", [dll]);

        throw new FileNotFoundException("Could not locate SPLA.UI.Avalonia to relaunch.");
    }

    private void OpenDebugSurface_Click(object? sender, RoutedEventArgs e)
        => new SurfaceWindow("debug", "Debug").Show(this);

    private void OpenSettingsSurface_Click(object? sender, RoutedEventArgs e)
        => new SurfaceWindow("settings", "Settings").Show(this);

    private void OpenWireSurface_Click(object? sender, RoutedEventArgs e)
        => new SurfaceWindow("wire", "Wire").Show(this);

    private void OpenInBrowser_Click(object? sender, RoutedEventArgs e)
    {
        var currentUrl = Browser.Source?.AbsoluteUri ?? _url;
        if (currentUrl is null) return;

        try
        {
            Process.Start(new ProcessStartInfo(currentUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            TitleText.Text = "— browser launch failed: " + ex.Message;
        }
    }

    /// <summary>Sets BOTH the OS window title (taskbar/Alt+Tab) and the custom in-window title bar
    /// text from a project display name, or the generic app name when null/blank — the two must stay
    /// in lockstep or one silently goes stale (that was the actual bug: only Window.Title was wired
    /// up at first). Called at startup and whenever the web client's in-page focus changes (see
    /// <see cref="Helpers.WebViewBridge"/>).</summary>
    private void ApplyProjectTitle(string? projectLabel)
    {
        Title = string.IsNullOrWhiteSpace(projectLabel) ? "SPLA - Local AI Assistant" : $"SPLA — {projectLabel}";
        TitleText.Text = string.IsNullOrWhiteSpace(projectLabel) ? "no project" : projectLabel;
    }

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
