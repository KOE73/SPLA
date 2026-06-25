using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Observability;
using SPLA.Plugins.Host.Avalonia;
using SPLA.UI.Avalonia.Services;
using SPLA.UI.Avalonia.Services.Plugins;

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

    /// <summary>
    /// Persistent project-lifetime token tally. Created once the workspace is resolved at startup,
    /// backed by <c>&lt;workspace&gt;/.spla/token-usage.json</c>. Every chat folds its real per-turn
    /// usage in here, so the count is cumulative across the whole project, ultimate and ever-growing.
    /// </summary>
    public static SPLA.Domain.Interfaces.ITokenUsageStore TokenUsage { get; private set; } =
        new SPLA.Domain.Agent.FileTokenUsageStore(
            Path.Combine(Directory.GetCurrentDirectory(), ".spla", "token-usage.json"));

    /// <summary>
    /// Machine-global token tally, shared across every project on this machine. Backed by
    /// <c>~/.spla/token-usage.json</c>. Every turn folds into both this and the per-project
    /// <see cref="TokenUsage"/>.
    /// </summary>
    public static SPLA.Domain.Interfaces.ITokenUsageStore TokenUsageGlobal { get; } =
        new SPLA.Domain.Agent.FileTokenUsageStore(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".spla", "token-usage.json"));

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
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(SplaTelemetry.CreateFileLoggerProvider());
            builder.SetMinimumLevel(LogLevel.Information);
        });
        services.AddSingleton<IActiveConversationAccessor, ActiveConversationAccessor>();
        services.AddSingleton<IPluginInteractionService, PluginInteractionService>();
        services.AddSingleton<IAvaloniaPluginPanelRegistry, AvaloniaPluginPanelRegistry>();
        services.AddSingleton<IAvaloniaPluginSettingsRegistry, AvaloniaPluginSettingsRegistry>();
        services.AddSingleton<IPluginPanelHostService, PluginPanelHostService>();
        services.AddSingleton<IAvaloniaPluginLoader, AvaloniaPluginLoader>();
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
            TokenUsage = new SPLA.Domain.Agent.FileTokenUsageStore(
                Path.Combine(ResolvedSettings.WorkspacePath, ".spla", "token-usage.json"));

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

    public static void ChangeTheme(string themeName)
    {
        var app = Current;
        if (app != null)
        {
            var uri = new Uri("avares://SPLA.UI.Avalonia/Themes/Colors/" + themeName + ".axaml");
            var dict = new global::Avalonia.Markup.Xaml.Styling.ResourceInclude(uri) { Source = uri };
            
            // Themes is index 0
            if (app.Resources.MergedDictionaries.Count > 0)
                app.Resources.MergedDictionaries[0] = dict;
            else
                app.Resources.MergedDictionaries.Add(dict);

            VisualResourcesChanged?.Invoke(app, EventArgs.Empty);
        }
    }

    public static void ReloadResolvedSettings()
    {
        ResolvedSettings = ConfigLoader.LoadAndResolve(ProjectFilePath);
    }

    public static void ChangeDensity(string densityName)
    {
        var app = Current;
        if (app != null)
        {
            // Normalize name (e.g. "norm" -> "Norm")
            densityName = char.ToUpper(densityName[0]) + densityName.Substring(1).ToLower();
            var uri = new Uri("avares://SPLA.UI.Avalonia/Themes/Densities/" + densityName + ".axaml");
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
