using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.MCP.BasicTools.FileSystem.Search;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsSearchTextTool : IMcpTool
{
    public string Name => "system_search_text";

    /// <summary>
    /// Chats already told that the fast backend is missing. Kept out of the session KV on purpose:
    /// a diagnostic note must not show up in the working memory the model browses. A weak table keyed
    /// on the session dies with the chat and needs no cleanup.
    /// </summary>
    private static readonly ConditionalWeakTable<IAgentSession, object> FallbackNoticeShown = new();
    private static int _noticeShownWithoutSession;

    /// <summary>Everything about this tool that does not fit its one-line description.
    /// Disclosed together with the tool itself — see <c>ToolFunctionDefinition.Details</c>.</summary>
    private static readonly string DetailsText =
        """
        tool: system_search_text

        summary: |
          Searches the CONTENT of files — every file type, not just code: source, documentation,
          markdown, logs, configuration, mounted reference corpora. Reach for this whenever the thing
          you know is text that appears INSIDE a file, and you do not already know which file holds
          it. Searching contents and locating a file by its name are different axes: this tool is the
          content axis, and guessing a file name to avoid a content search is the slower path.

        arguments:
          query:
            required: true
            formats:
              - literal_text
              - regex_when_regex_true
            examples:
              - AddCustomProperty
              - ToolDescriptor
              - "class\\s+McpHost"
          path:
            required: false
            default: current_workspace
          regex:
            required: false
            default: false
            note: |
              Linear-time engine: lookaround and backreferences are unavailable, by design and on
              every backend. A pattern using them is refused rather than silently treated differently.
          case_sensitive:
            required: false
            default: false
          multiline:
            required: false
            default: false
            note: lets a pattern match across line breaks
          context_lines:
            required: false
            default: 0
            note: lines of surrounding context to include on each side of a match
          mode:
            required: false
            default: content
            values:
              - content            # matching lines, with context when asked
              - files_with_matches # only which files match — cheapest way to narrow a corpus
              - count              # per-file match counts
          max_results:
            required: false
            default: 100
          include_patterns:
            required: false
            examples:
              - ["*.cs"]
              - ["**/*.md"]
          exclude_patterns:
            required: false
            examples:
              - ["bin/*", "obj/*"]

        examples:
          - comment: find every mention of a symbol without knowing the file
            request:
              query: AddCustomProperty
          - comment: narrow a large corpus first, then read only what matched
            request:
              query: extrude
              include_patterns: ["**/*.md"]
              mode: files_with_matches
          - comment: read a match together with the code around it
            request:
              query: ToolDefinition
              include_patterns: ["*.cs"]
              context_lines: 3
        """;

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Details = DetailsText,
            Description = "Search the CONTENT of files (code, docs, any text) for a string or regex. Use this whenever you know text that occurs inside a file but not which file — with context lines, multiline matching, and cheaper files-only/count modes.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    query            = new { type = "string",                      description = "Text or regex to look for inside files." },
                    path             = new { type = new[] { "string",  "null" },   description = "Directory to search. Null = current workspace." },
                    regex            = new { type = new[] { "boolean", "null" },   description = "Treat query as regex. Null = false. No lookaround/backreferences." },
                    case_sensitive   = new { type = new[] { "boolean", "null" },   description = "Case-sensitive match. Null = false." },
                    multiline        = new { type = new[] { "boolean", "null" },   description = "Let the pattern match across line breaks. Null = false." },
                    context_lines    = new { type = new[] { "integer", "null" },   description = "Lines of context on each side of a match. Null = 0." },
                    mode             = new { type = new[] { "string",  "null" },   description = "'content' (default), 'files_with_matches', or 'count'." },
                    max_results      = new { type = new[] { "integer", "null" },   description = "Max results. Null = 100." },
                    include_patterns = new { type = new[] { "array",   "null" }, items = new { type = "string" }, description = "Glob patterns to include, e.g. ['**/*.md']. Null = all files." },
                    exclude_patterns = new { type = new[] { "array",   "null" }, items = new { type = "string" }, description = "Glob patterns to exclude, e.g. ['bin/*']. Null = none." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "query", "path", "regex", "case_sensitive", "multiline", "context_lines", "mode", "max_results", "include_patterns", "exclude_patterns", "output", "output_name" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var query           = ToolJson.GetStringTrimmed(root, "query");
            var path            = ToolJson.GetStringTrimmed(root, "path");
            var regex           = ToolJson.GetBoolean(root, "regex", false);
            var caseSensitive   = ToolJson.GetBoolean(root, "case_sensitive", false);
            var multiline       = ToolJson.GetBoolean(root, "multiline", false);
            var contextLines    = ToolJson.GetInt32Clamped(root, "context_lines", 0, 0, 20);
            var maxResults      = ToolJson.GetInt32Clamped(root, "max_results", 100, 1, 10000);
            var includePatterns = ToolJson.GetStringArray(root, "include_patterns");
            var excludePatterns = ToolJson.GetStringArray(root, "exclude_patterns");

            if (!TryParseMode(ToolJson.GetStringTrimmed(root, "mode"), out var mode, out var modeError))
                return ToolResult.Fail(modeError, "invalid mode");

            if (string.IsNullOrEmpty(query))
                return ToolResult.Text(JsonSerializer.Serialize(new SearchTextResult()));

            // Disk-backed workspaces map the logical root to a real host path and get ripgrep. A
            // virtual workspace returns null from MapPathToHost — there is no disk path to point
            // ripgrep at — so the workspace engine walks the logical API instead. The split is by
            // substrate, not by binary availability: see ADR_20260831 §2.0.
            var ws = HostServices.Sandbox.Workspace;
            var logicalRoot = path ?? ".";
            var hostPath = ws.MapPathToHost(logicalRoot);
            var searchRoot = hostPath ?? logicalRoot;

            if (!ws.DirectoryExists(searchRoot))
                return ToolResult.Fail($"Error: Directory not found at {searchRoot}", "directory not found");

            var request = new SearchRequest(
                searchRoot, query, regex, caseSensitive,
                includePatterns, excludePatterns,
                ContextBefore: contextLines, ContextAfter: contextLines,
                Multiline: multiline, Mode: mode);

            SearchOutcome outcome;
            string? degradedReason = null;

            if (hostPath is null)
            {
                outcome = await new WorkspaceSearchEngine(ws).SearchAsync(request, cancellationToken);
            }
            else
            {
                try
                {
                    outcome = await new RipgrepSearchEngine().SearchAsync(request, cancellationToken);
                }
                catch (RipgrepUnavailableException ex)
                {
                    // The fast path is gone on a substrate that should have had it. Fall back, but say
                    // so once per chat — a silent degradation is what this used to be.
                    degradedReason = ex.Message;
                    outcome = await new WorkspaceSearchEngine(ws).SearchAsync(request, cancellationToken);
                }
            }

            var json = JsonSerializer.Serialize(
                BuildResult(outcome, query, mode, maxResults),
                new JsonSerializerOptions { WriteIndented = true });

            if (degradedReason is not null && ShouldAnnounceFallback())
                json = "note: fast search backend unavailable — using built-in fallback (slower, no .gitignore awareness)\n"
                     + $"reason: {degradedReason}\n\n" + json;

            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            if (target == OutputTarget.Context) return ToolResult.Text(json);
            var blobName = ToolJson.GetStringTrimmed(root, "output_name");
            return ToolResult.Text(DataChannel.Route(target, BlobPayload.OfText(json), $"system_search_text: '{query}'", blobName));
        }
        catch (JsonException)
        {
            return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json");
        }
        catch (UnsupportedPatternException ex)
        {
            return ToolResult.Fail($"Error: {ex.Message}", "unsupported pattern");
        }
        // A boundary refusal is a DECISION and must not be flattened into "error reading file" by
        // the catch below: told it was a fault, the model retries or starts repairing something it
        // was never allowed to touch. The pipeline turns this into a refusal with its reason.
        catch (PathBoundaryException) { throw; }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error performing search: {ex.Message}", "search failed");
        }
    }

    private static SearchTextResult BuildResult(SearchOutcome outcome, string query, SearchOutputMode mode, int maxResults)
    {
        var result = new SearchTextResult { Query = query, Mode = ModeName(mode) };

        if (mode == SearchOutputMode.Content)
        {
            result.TotalMatches = outcome.Matches.Count;
            result.Matches = SearchRanking.RankAndFilter(outcome.Matches, query, maxResults);
            result.ReturnedMatches = result.Matches.Count;
            result.FilesWithMatches = outcome.Files.Count;
            if (result.TotalMatches > result.ReturnedMatches)
                result.Truncated = $"showing {result.ReturnedMatches} of {result.TotalMatches} matches";
            return result;
        }

        result.FilesWithMatches = outcome.Files.Count;
        result.TotalMatches = outcome.Files.Sum(f => f.MatchCount);
        result.Files = outcome.Files.Take(maxResults).ToList();
        result.ReturnedMatches = result.Files.Count;
        if (outcome.Files.Count > result.Files.Count)
            result.Truncated = $"showing {result.Files.Count} of {outcome.Files.Count} files";
        return result;
    }

    /// <summary>True the first time a chat degrades to the fallback engine; false ever after, so the
    /// notice informs without nagging.</summary>
    private static bool ShouldAnnounceFallback()
    {
        var session = AgentSessionScope.Current;
        if (session is null)
            return Interlocked.Exchange(ref _noticeShownWithoutSession, 1) == 0;

        lock (FallbackNoticeShown)
        {
            if (FallbackNoticeShown.TryGetValue(session, out _)) return false;
            FallbackNoticeShown.Add(session, new object());
            return true;
        }
    }

    private static bool TryParseMode(string? raw, out SearchOutputMode mode, out string error)
    {
        error = string.Empty;
        mode = SearchOutputMode.Content;
        if (string.IsNullOrWhiteSpace(raw)) return true;

        switch (raw.Trim().ToLowerInvariant())
        {
            case "content":             mode = SearchOutputMode.Content; return true;
            case "files_with_matches":  mode = SearchOutputMode.FilesWithMatches; return true;
            case "count":               mode = SearchOutputMode.Count; return true;
            default:
                error = $"Error: unknown mode '{raw}'. Use 'content', 'files_with_matches' or 'count'.";
                return false;
        }
    }

    private static string ModeName(SearchOutputMode mode) => mode switch
    {
        SearchOutputMode.FilesWithMatches => "files_with_matches",
        SearchOutputMode.Count            => "count",
        _                                 => "content"
    };
}

public class SearchTextResult
{
    public string Query { get; set; } = string.Empty;
    public string Mode { get; set; } = "content";
    public int TotalMatches { get; set; }
    public int ReturnedMatches { get; set; }
    public int FilesWithMatches { get; set; }

    /// <summary>Set only when the answer is shorter than what was found — an honest marker beats a
    /// silently clipped result the model would read as complete.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Truncated { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SearchMatch>? Matches { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SearchFileHit>? Files { get; set; }
}

public class SearchMatch
{
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public string Preview { get; set; } = string.Empty;

    /// <summary>Context lines around the match. Null unless context_lines was asked for — an always-
    /// present empty array would be pure noise in every result that did not want context.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Before { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? After { get; set; }
}
