using Avalonia;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using SPLA.Platform;
using Spectre.Console.Cli;

namespace SPLA.UI.Avalonia;

/// <summary>How this process was asked to run. Filled by the parser before any Avalonia type is
/// touched, so <see cref="App"/> never has to re-read a raw argument list.</summary>
public sealed class LaunchSettings : CommandSettings
{
    [CommandArgument(0, "[project]")]
    [Description("Path to a .spla manifest to open. Defaults to the one in the current directory.")]
    public string? Project { get; init; }

    [CommandOption("--hub")]
    [Description("Run as the machine's tray shell instead of a project window. Only one per session.")]
    public bool Hub { get; init; }
}

class Program
{
    /// <summary>The parsed launch options, available to <see cref="App"/> once startup begins.</summary>
    public static LaunchSettings Launch { get; private set; } = new();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // A GUI-subsystem process gets no console and does not inherit the terminal's, so Spectre's
        // help and error text would render into a discarded writer and the app would appear to exit
        // for no reason. Attaching the parent's console — when it has one — puts that text where the
        // person who typed the command is looking. Fails harmlessly (error 6) when launched from
        // Explorer or the tray, which is exactly the case with nobody to tell.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) AttachConsole(ATTACH_PARENT_PROCESS);

        try
        {
            WindowsShellIntegration.Initialize();

            var app = new CommandApp<LaunchCommand>();
            app.Configure(config =>
            {
                config.SetApplicationName("spla-ui");
                config.UseStrictParsing();
                // Pinned English for the same reason as the CLI — see ApplyCommonCliConventions there.
                config.SetApplicationCulture(CultureInfo.GetCultureInfo("en"));
            });
            Environment.ExitCode = app.Run(args);
        }
        catch (Exception ex)
        {
            // Best-effort crash dump next to the executable (portable, not a hardcoded dev path).
            try
            {
                var path = System.IO.Path.Combine(AppContext.BaseDirectory, "spla-crash.txt");
                System.IO.File.WriteAllText(path, ex.ToString());
            }
            catch { /* nothing we can do */ }
            throw;
        }
    }

    /// <summary>The whole application, as a command. Running Avalonia from inside the parser rather
    /// than after it keeps one path: arguments that do not parse never reach a window, and
    /// <c>--help</c> prints and exits without starting a UI at all.</summary>
    private sealed class LaunchCommand : Command<LaunchSettings>
    {
        protected override int Execute(
            CommandContext context, LaunchSettings settings, CancellationToken cancellationToken)
        {
            Launch = settings;
            BuildAvaloniaApp().StartWithClassicDesktopLifetime([]);
            return 0;
        }
    }

    private const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(uint dwProcessId);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
