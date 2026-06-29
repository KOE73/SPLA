using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Observability;
using SPLA.UI.Avalonia.Services;

namespace SPLA.UI.Avalonia;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static event EventHandler? VisualResourcesChanged;

    /// <summary>
    /// Resolved settings from defaults.yaml + optional .spla project file.
    /// Set during startup, available globally.
    /// </summary>
    public static ResolvedSettings ResolvedSettings { get; private set; } = new();

    /// <summary>
    /// Path to the .spla project file, if one was loaded.
    /// </summary>
    public static string? ProjectFilePath { get; private set; }


    public override void Initialize()
    {
        SplaTelemetry.ConfigureGlobalLogs();
        Services = ConfigureServices();
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Services.GetRequiredService<ILogger<App>>().LogCritical(args.ExceptionObject as Exception, "Unhandled application exception.");
        TaskScheduler.UnobservedTaskException += (_, args) =>
            Services.GetRequiredService<ILogger<App>>().LogError(args.Exception, "Unobserved task exception.");
        AvaloniaXamlLoader.Load(this);
    }

    private static IServiceProvider ConfigureServices()
    {
        // The shell hosts no agent/plugin services in-process any more — those live in the embedded
        // service (own process). Only logging remains for the shell itself.
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(SplaTelemetry.CreateFileLoggerProvider());
            builder.SetMinimumLevel(LogLevel.Information);
        });
        return services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Check for .spla file in command-line args
            string? splaFile = null;
            if (desktop.Args != null && desktop.Args.Length > 0)
            {
                var candidate = desktop.Args[0];
                if (candidate.EndsWith(".spla", StringComparison.OrdinalIgnoreCase) && File.Exists(candidate))
                {
                    splaFile = candidate;
                }
            }

            // Auto-detect in CWD
            splaFile ??= ConfigLoader.FindProjectFile(Directory.GetCurrentDirectory());

            // Scaffold new/empty project files before resolving so the first resolve
            // already sees the project name and ignore patterns.
            if (splaFile != null) ConfigLoader.ScaffoldIfNew(splaFile);

            ProjectFilePath = splaFile;
            ResolvedSettings = ConfigLoader.LoadAndResolve(splaFile);
            SplaTelemetry.ConfigureProjectLogs(ResolvedSettings.WorkspacePath);
        
            var logger = Services.GetRequiredService<ILogger<App>>();
            logger.LogInformation(
                "Application startup. ProjectFile={ProjectFile} WorkspacePath={WorkspacePath} Mode={Mode}",
                splaFile,
                ResolvedSettings.WorkspacePath,
                ResolvedSettings.Mode);

            if (splaFile != null)
            {
                Directory.SetCurrentDirectory(ResolvedSettings.WorkspacePath);
                ConfigLoader.AddRecentProject(splaFile);
                WindowsShellIntegration.AddRecentProject(splaFile);
            }

            ChangeTheme(ResolvedSettings.Theme);
            ChangeDensity(ResolvedSettings.Density);

            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static readonly HashSet<string> KnownThemes    = ["Dark", "Light", "Cream", "Emerald"];
    private static readonly HashSet<string> KnownDensities = ["Nano", "Mini", "Norm", "Max"];

    public static void ChangeTheme(string themeName)
    {
        var app = Current;
        if (app != null)
        {
            var name = char.ToUpper(themeName[0]) + themeName.Substring(1).ToLower();
            if (!KnownThemes.Contains(name)) name = "Cream";
            var uri = new Uri("avares://SPLA.UI.Avalonia/Themes/Colors/" + name + ".axaml");
            var dict = new global::Avalonia.Markup.Xaml.Styling.ResourceInclude(uri) { Source = uri };

            // Themes is index 0
            if (app.Resources.MergedDictionaries.Count > 0)
                app.Resources.MergedDictionaries[0] = dict;
            else
                app.Resources.MergedDictionaries.Add(dict);

            VisualResourcesChanged?.Invoke(app, EventArgs.Empty);
        }
    }

    // ── Embedded service (one agent, many windows) ───────────────────────────
    // The whole desktop app is a shell over a single SPLA service: every window (main + tear-off
    // surface windows) is a NativeWebView talking the same WebSocket to ONE agent. Started once,
    // lazily, on the project's workspace (or SPLA_SERVICE_URL for a remote agent). No in-process
    // agent stack — the service owns chats, tools, plugins, secrets.
    private static EmbeddedServiceLauncher? _serviceLauncher;
    private static Task<string>? _serviceUrlTask;

    /// <summary>Starts the embedded/remote service once and returns its base URL; subsequent calls
    /// reuse the same running service. All windows navigate WebViews against this URL.</summary>
    public static Task<string> ServiceUrlAsync()
        => _serviceUrlTask ??= StartServiceAsync();

    private static async Task<string> StartServiceAsync()
    {
        _serviceLauncher = new EmbeddedServiceLauncher();
        var remote = Environment.GetEnvironmentVariable("SPLA_SERVICE_URL");
        return await _serviceLauncher.StartAsync(remote, ResolvedSettings.WorkspacePath);
    }

    /// <summary>Stops the local child service (no-op for a remote target). Called when the main
    /// window closes.</summary>
    public static void ShutdownService() => _serviceLauncher?.Dispose();

    public static void ChangeDensity(string densityName)
    {
        var app = Current;
        if (app != null)
        {
            var name = char.ToUpper(densityName[0]) + densityName.Substring(1).ToLower();
            if (!KnownDensities.Contains(name)) name = "Norm";
            var uri = new Uri("avares://SPLA.UI.Avalonia/Themes/Densities/" + name + ".axaml");
            var dict = new global::Avalonia.Markup.Xaml.Styling.ResourceInclude(uri) { Source = uri };
            
            // Densities is index 1
            if (app.Resources.MergedDictionaries.Count > 1)
                app.Resources.MergedDictionaries[1] = dict;
            else if (app.Resources.MergedDictionaries.Count == 1)
                app.Resources.MergedDictionaries.Add(dict);

            VisualResourcesChanged?.Invoke(app, EventArgs.Empty);
        }
    }
}
