namespace SPLA.Plugins.OneC.Models;

/// <summary>
/// Tracks which files have been indexed and their content hash,
/// enabling incremental re-indexing on subsequent runs.
/// </summary>
public class OneCFileEntry
{
    public long Id { get; set; }

    /// <summary>Relative path from the configuration dump root.</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>SHA-256 hex digest of the file content.</summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>ISO-8601 UTC timestamp of when this file was last indexed.</summary>
    public string LastIndexedAt { get; set; } = string.Empty;
}

