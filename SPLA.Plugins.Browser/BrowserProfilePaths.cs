using System.IO;

namespace SPLA.Plugins.Browser;

/// <summary>The one place that knows where the project-local persistent browser profile lives.</summary>
internal static class BrowserProfilePaths
{
    public const string ProjectProfileLabel = "Project profile (persistent)";

    public static string ProjectProfileDir(string workspacePath)
        => Path.Combine(workspacePath, ".spla", "browser-profile");

    public static bool ProjectProfileHasState(string workspacePath)
    {
        var dir = ProjectProfileDir(workspacePath);
        return Directory.Exists(dir) && Directory.EnumerateFileSystemEntries(dir).Any();
    }
}
