namespace SPLA.Plugins.OneC.Models;

/// <summary>
/// Summary of a completed indexing run.
/// </summary>
public class IndexingReport
{
    /// <summary>New objects added to the index.</summary>
    public int ObjectsAdded { get; set; }

    /// <summary>Objects updated (file changed since last run).</summary>
    public int ObjectsUpdated { get; set; }

    /// <summary>Files skipped because their hash matched.</summary>
    public int FilesSkipped { get; set; }

    /// <summary>New relations inserted.</summary>
    public int RelationsAdded { get; set; }

    /// <summary>Files that failed to parse (error details in <see cref="Errors"/>).</summary>
    public int FilesWithErrors { get; set; }

    /// <summary>List of error messages collected during indexing.</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Total wall-clock duration of the indexing run.</summary>
    public TimeSpan Elapsed { get; set; }

    public override string ToString() =>
        $"Objects: +{ObjectsAdded} ~{ObjectsUpdated} | Relations: +{RelationsAdded} | Skipped: {FilesSkipped} | Errors: {FilesWithErrors} | {Elapsed.TotalSeconds:F1}s";
}

