using System.IO;
using SPLA.Domain.Settings;

namespace SPLA.Domain.Project;

/// <summary>
/// Local backend: runtime state lives in the <c>.spla/</c> folder next to the manifest, buckets
/// are its subfolders. Without a project the global <c>~/.spla/</c> plays the runtime area —
/// the same fallback every consumer used to hand-roll; this class is now the single source of
/// that decision.
/// </summary>
public sealed class LocalProjectBackend : IProjectBackend
{
    private readonly string _runtimeDir;

    public LocalProjectBackend(string projectId, string runtimeDir)
    {
        ProjectId = projectId;
        _runtimeDir = runtimeDir;
    }

    public string ProjectId { get; }

    public IBucket GetBucket(string name)
        => new FileBucket(name, name.Length == 0 ? _runtimeDir : Path.Combine(_runtimeDir, name));

    /// <summary>Runtime-dir decision previously duplicated by ChatManager/ProjectKvStore/etc.</summary>
    public static LocalProjectBackend For(ResolvedSettings settings)
    {
        var runtimeDir = settings.ProjectFilePath != null
            ? Path.Combine(settings.WorkspacePath, ".spla")
            : ConfigLoader.GetDefaultsDir();
        return new LocalProjectBackend(settings.ProjectFilePath ?? "(no-project)", runtimeDir);
    }
}
