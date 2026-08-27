using System;
using System.IO;
using System.Text.Json;

namespace SPLA.Domain.Settings;

/// <summary>
/// The hub's own UI preferences — today just the color scheme picked in the Projects window.
///
/// <para>Kept apart from any project's settings on purpose: the Projects window belongs to the hub,
/// not to a project, so there is no <c>.spla</c> to save it into. It lives next to the project
/// registry itself, in <c>~/.spla/hub-ui.json</c> — a plain file rather than the browser's own
/// storage, because the web view backing a tray window is not guaranteed to keep that storage across
/// runs the way a normal browser profile would.</para>
/// </summary>
public static class HubAppearanceStore
{
    private static string Path_ => System.IO.Path.Combine(ConfigLoader.GetDefaultsDir(), "hub-ui.json");

    private sealed class HubUi
    {
        public string? Theme { get; set; }
    }

    /// <summary>The saved scheme, or null when nothing has been picked yet — callers fall back to
    /// their own default rather than this store inventing one.</summary>
    public static string? LoadTheme()
    {
        try
        {
            if (!File.Exists(Path_)) return null;
            return JsonSerializer.Deserialize<HubUi>(File.ReadAllText(Path_))?.Theme;
        }
        catch { return null; }
    }

    public static void SaveTheme(string theme)
    {
        try
        {
            var dir = ConfigLoader.GetDefaultsDir();
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path_, JsonSerializer.Serialize(new HubUi { Theme = theme }));
        }
        catch { /* best effort — a scheme that fails to persist just needs picking again */ }
    }
}
