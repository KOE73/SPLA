using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

/// <summary>What shape of answer the caller wants — the grep <c>-l</c>/<c>-c</c> axis.</summary>
public enum SearchOutputMode
{
    /// <summary>Matching lines with optional surrounding context.</summary>
    Content,
    /// <summary>Only the files that contain a match. Lets an engine stop at the first hit per file.</summary>
    FilesWithMatches,
    /// <summary>Per-file match counts, no line text.</summary>
    Count
}

/// <summary>
/// One search, fully described. A record rather than a parameter list because both engines take the
/// same options and the list had already grown past the point where positional arguments are safe.
/// </summary>
public sealed record SearchRequest(
    string RootPath,
    string Query,
    bool IsRegex,
    bool CaseSensitive,
    string[]? IncludePatterns,
    string[]? ExcludePatterns,
    int ContextBefore,
    int ContextAfter,
    bool Multiline,
    SearchOutputMode Mode);

/// <summary>
/// What an engine found. <see cref="Files"/> is filled in every mode; <see cref="Matches"/> only in
/// <see cref="SearchOutputMode.Content"/> — that is what lets the other two modes exit early instead
/// of collecting line text nobody asked for.
/// </summary>
public sealed class SearchOutcome
{
    public List<SearchMatch> Matches { get; } = new();
    public List<SearchFileHit> Files { get; } = new();
}

/// <summary>A file that contained at least one match, with its count when the mode computed one.</summary>
public sealed class SearchFileHit
{
    public string File { get; set; } = string.Empty;
    public int MatchCount { get; set; }
}

public interface ISearchEngine
{
    Task<SearchOutcome> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
}
