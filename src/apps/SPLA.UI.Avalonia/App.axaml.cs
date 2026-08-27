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
using SPLA.Platform;
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
        // Make the DPAPI secret backend selectable before any settings are resolved.
        SPLA.Secrets.Dpapi.DpapiSecrets.Register();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // The tray shell is a different program wearing the same binary: no project, no window,
            // no service — just the machine's notification-area presence. Taken before any project
            // resolution, because it must not touch one.
            if (Program.Launch.Hub)
            {
                RunAsHubShell(desktop);
                base.OnFrameworkInitializationCompleted();
                return;
            }

            // The manifest now arrives parsed (see Program.LaunchSettings) rather than being fished
            // out of a raw argument array here.
            string? splaFile = null;
            if (Program.Launch.Project is { Length: > 0 } candidate &&
                candidate.EndsWith(".spla", StringComparison.OrdinalIgnoreCase) &&
                File.Exists(candidate))
            {
                splaFile = candidate;
            }

            // Auto-detect in CWD. Two manifests in one folder is now a refusal rather than an
            // arbitrary pick — but a window that fails to open says nothing to anybody, so the app
            // starts project-less and the person can pick the one they meant from the project menu.
            try
            {
                splaFile ??= ConfigLoader.FindProjectFile(Directory.GetCurrentDirectory());
            }
            catch (InvalidOperationException)
            {
                splaFile = null;
            }

            // Scaffold new/empty project files before resolving so the first resolve
            // already sees the project name and ignore patterns.
            if (splaFile != null) ConfigLoader.ScaffoldIfNew(splaFile);

            ProjectFilePath = splaFile;
            ResolvedSettings = ConfigLoader.LoadAndResolve(splaFile);
            SplaTelemetry.ConfigureProjectLogs(
                ResolvedSettings.Project.GetBucket("logs").MapToHostDirectory());
        
            var logger = Services.GetRequiredService<ILogger<App>>();
            logger.LogInformation(
                "Application startup. ProjectFile={ProjectFile} WorkspacePath={WorkspacePath} Mode={Mode}",
                splaFile,
                ResolvedSettings.WorkspacePath,
                ResolvedSettings.Mode);

            if (splaFile != null)
            {
                // Deliberately no chdir. A process has exactly one working directory, so the moment
                // this shell can hold two windows on two projects, a cwd set from whichever one
                // started first is wrong for the other — and wrong silently, in whatever resolves a
                // relative path. The working directory belongs to the serve instance, which holds
                // exactly one project by construction; the shell passes the workspace explicitly to
                // the child it spawns and otherwise never relies on where it was launched from.
                ConfigLoader.AddRecentProject(splaFile);
                WindowsShellIntegration.AddRecentProject(splaFile);
            }

            ChangeTheme(ResolvedSettings.Theme);
            ChangeDensity(ResolvedSettings.Density);

            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>The tray shell's own lifetime, held for as long as this process is the session's.</summary>
    private static HubShell? _hubShell;

    /// <summary>
    /// Runs this process as the session's tray shell: claim the role, find or start the hub, show the
    /// tray, and hold no window at all.
    ///
    /// <para>Losing the claim is the ordinary outcome — every project window tries to start one of
    /// these, and all but the first find it already there — so it exits without a word rather than
    /// reporting a failure nobody asked about.</para>
    /// </summary>
    private static async void RunAsHubShell(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _hubShell = HubShell.Claim();
        if (_hubShell is null)
        {
            desktop.Shutdown();
            return;
        }

        // Never quit just because no window is open — there is deliberately never one here.
        desktop.ShutdownMode = global::Avalonia.Controls.ShutdownMode.OnExplicitShutdown;

        HubUrl = await HubLauncher.ResolveAsync();
        if (HubUrl is null)
        {
            // Without a hub there is nothing for a tray to show. Better to leave than to sit there
            // drawing an empty menu that will never fill.
            desktop.Shutdown();
            return;
        }

        _trayIcon = TrayIconService.StartIfHubAvailable(HubUrl, exit: () => desktop.Shutdown());
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

    /// <summary>The machine's registry hub, started (or joined) once. Everything the shell knows about
    /// projects other than its own comes through it — this process never hosts it, see
    /// <see cref="HubLauncher"/>.</summary>
    private static readonly HubLauncher HubLauncher = new();

    /// <summary>Where the machine hub is, or null when none could be reached or started. Null is not
    /// an error state: the shell works, it just has no view past its own project.</summary>
    public static string? HubUrl { get; private set; }

    /// <summary>The machine-wide tray icon, started once alongside the hub connection. Null when
    /// there is no hub — see <see cref="TrayIconService.StartIfHubAvailable"/>.</summary>
    private static TrayIconService? _trayIcon;

    /// <summary>This window's registration with the machine hub, so the hub can raise or close it.
    /// Null when there is no hub — see <see cref="WindowRegistration"/>.</summary>
    private static WindowRegistration? _windowRegistration;

    /// <summary>Starts the embedded/remote service once and returns its base URL; subsequent calls
    /// reuse the same running service. All windows navigate WebViews against this URL.</summary>
    public static Task<string> ServiceUrlAsync()
        => _serviceUrlTask ??= StartServiceAsync();

    private static async Task<string> StartServiceAsync()
    {
        _serviceLauncher = new EmbeddedServiceLauncher();
        var remote = Environment.GetEnvironmentVariable("SPLA_SERVICE_URL");

        // Resolved before the service starts so the child can be told where to register. Failure is
        // silent on purpose: a machine without a hub still runs agents perfectly well.
        HubUrl = await HubLauncher.ResolveAsync();

        // A window no longer draws a tray icon. The tray is a machine-wide view, so it belongs to one
        // process per session — otherwise three open projects meant three identical icons, which is
        // what this replaces. Every window asks for that shell; the session's mutex means only the
        // first launch becomes it and the rest exit at once, so asking is free.
        if (HubUrl is not null) SelfInvocationLauncher.TryLaunch("SPLA.UI.Avalonia.exe", "--hub");
        // The window already answered "is there a project here" during startup. The child service
        // must not ask again — it cannot, being headless — so when the answer was no, it is told to
        // run project-less explicitly. Same behaviour as before; no longer an accident.
        var serviceUrl = await _serviceLauncher.StartAsync(
            remote, ResolvedSettings.WorkspacePath, noProject: ProjectFilePath is null, hubUrl: HubUrl);

        // Registered after the service is up so the record carries the address this window is actually
        // looking at. The agent behind it registers itself separately — one participant per thing that
        // exists, rather than one speaking for another.
        _windowRegistration = WindowRegistration.StartIfHubAvailable(
            HubUrl, ProjectFilePath, ResolvedSettings.ProjectName, serviceUrl,
            Services.GetRequiredService<ILogger<App>>());

        return serviceUrl;
    }

    /// <summary>
    /// Brings a service back after the one this window used has gone, and re-points every window at it.
    ///
    /// <para><b>Reattach or start, never "always start".</b> The launcher first looks for a live
    /// instance through the project's lock file, so if the agent is actually fine and only this
    /// socket broke, this rejoins it instead of racing a second writer at the same project.</para>
    ///
    /// <para>Raised as an event rather than the window polling: <see cref="ServiceUrlAsync"/> caches
    /// its task by design (one service per process), so windows have to be told the cached answer has
    /// been replaced.</para>
    /// </summary>
    public static async Task RestartServiceAsync()
    {
        _serviceLauncher?.Dispose();
        _serviceLauncher = null;
        _serviceUrlTask = null;

        if (_windowRegistration is not null)
        {
            await _windowRegistration.DisposeAsync();
            _windowRegistration = null;
        }

        try
        {
            var url = await ServiceUrlAsync();
            ServiceUrlChanged?.Invoke(null, url);
        }
        catch (Exception ex)
        {
            Services.GetRequiredService<ILogger<App>>()
                .LogError(ex, "Could not bring the service back up on request.");
        }
    }

    /// <summary>Raised with the new base URL once the service has been restarted, so open windows can
    /// re-navigate. Windows subscribe; nothing here knows how many there are.</summary>
    public static event EventHandler<string>? ServiceUrlChanged;

    /// <summary>Stops the local child service (no-op for a remote target). Called when the main
    /// window closes.</summary>
    public static void ShutdownService()
    {
        _serviceLauncher?.Dispose();
        // Lets go of the hub without stopping it: other windows and hand-started serve processes may
        // still be registered with it. See HubLauncher.Dispose.
        HubLauncher.Dispose();
        if (_trayIcon is not null)
        {
            _ = _trayIcon.DisposeAsync();
            _trayIcon = null;
        }
        // Dropping the registration is how the hub learns this window is gone — the socket closing is
        // the signal, exactly as it is for an agent.
        if (_windowRegistration is not null)
        {
            _ = _windowRegistration.DisposeAsync();
            _windowRegistration = null;
        }
    }

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
