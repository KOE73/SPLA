using Avalonia;
using System;

namespace SPLA.UI.Avalonia;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            WindowsShellIntegration.Initialize();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
