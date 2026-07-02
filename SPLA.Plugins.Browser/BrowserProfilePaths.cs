using System.IO;
using SPLA.Domain.Project;

namespace SPLA.Plugins.Browser;

/// <summary>The one place that knows where the project-local persistent browser profile lives:
/// the project's "browser-profile" bucket (locally the historical <c>.spla/browser-profile</c>).</summary>
internal static class BrowserProfilePaths
{
    public const string ProjectProfileLabel = "Project profile (persistent)";

    public static string ProjectProfileDir(IProject project)
        => project.GetBucket("browser-profile").MapToHostDirectory()
           ?? throw new InvalidOperationException(
               "A persistent browser profile needs a disk-backed project backend.");

    public static bool ProjectProfileHasState(IProject project)
    {
        var dir = ProjectProfileDir(project);
        return Directory.Exists(dir) && Directory.EnumerateFileSystemEntries(dir).Any();
    }
}
