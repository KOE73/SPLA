using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.MCP.BasicTools.FileSystem.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace SPLA.MCP.BasicTools.FileSystem;

/// <summary>
/// Directory listing, with a depth axis. The recursive form exists because orientation in an unknown
/// project was costing a call per directory level: the model walked the tree by hand, one round trip
/// at a time. One call now answers "what is the shape of this place".
/// <para>
/// Deliberately a parameter here rather than a separate tree tool — two tools already look at this
/// niche (this one and <c>system_find_files</c>), and a third would not help a model that had
/// trouble choosing between two. See <c>docs/adr/ADR_20260831_mcp_search-and-listing-tools.md</c> §2.5.
/// </para>
/// </summary>
public class FsListTool : IMcpTool
{
    public string Name => "system_list_files";

    private const int DefaultTotalLimit = 500;
    private const int DefaultRecursiveDepth = 3;

    private static readonly string DetailsText =
        """
        tool: system_list_files

        summary: |
          Lists a directory. With max_depth it descends and returns an indented tree, which is how to
          see the shape of an unfamiliar project in ONE call instead of one call per level.

        arguments:
          path:
            required: true
          max_depth:
            required: false
            default: 1
            note: 1 = this directory only. Use 2-4 to see structure without drowning in it.
          pattern:
            required: false
            note: glob filter on files, e.g. '**/*.html'. Directories are kept so the tree stays readable.
          per_dir_limit:
            required: false
            note: max entries shown per directory — with sort, this is "top N in every folder"
          sort:
            required: false
            default: name
            values: [name, size, modified]
          desc:
            required: false
            default: false
            note: reverse the sort — largest/newest first
          total_limit:
            required: false
            default: 500
            note: overall cap; truncation is always reported, never silent

        examples:
          - comment: shape of a project at a glance
            request:
              path: "."
              max_depth: 3
          - comment: the three largest pages in every folder
            request:
              path: "."
              max_depth: 4
              pattern: "**/*.html"
              per_dir_limit: 3
              sort: size
              desc: true
        """;

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Details = DetailsText,
            Description = "Lists files and directories. Set max_depth to descend and get an indented tree of the whole structure in one call, with optional per-directory top-N by size or date.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path          = new { type = "string",                     description = "The path to list." },
                    max_depth     = new { type = new[] { "integer", "null" },  description = "Levels to descend. Null = 1 (this directory only)." },
                    pattern       = new { type = new[] { "string",  "null" },  description = "Glob filter on file names, e.g. '**/*.cs'. Null = all files." },
                    per_dir_limit = new { type = new[] { "integer", "null" },  description = "Max entries shown per directory. Null = no per-directory cap." },
                    sort          = new { type = new[] { "string",  "null" },  description = "'name' (default), 'size', or 'modified'." },
                    desc          = new { type = new[] { "boolean", "null" },  description = "Reverse the sort — largest/newest first. Null = false." },
                    total_limit   = new { type = new[] { "integer", "null" },  description = "Overall cap on entries. Null = 500." },
                    output        = SchemaParts.Output,
                    output_name   = SchemaParts.OutputName
                },
                required = new[] { "path", "max_depth", "pattern", "per_dir_limit", "sort", "desc", "total_limit", "output", "output_name" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var path = ToolJson.GetStringTrimmed(root, "path");
            if (path is null) return Task.FromResult(ToolResult.Fail("Error: Missing 'path' parameter.", "missing path"));

            var maxDepth    = ToolJson.GetInt32Clamped(root, "max_depth", 1, 1, 32);
            var pattern     = ToolJson.GetStringTrimmed(root, "pattern");
            var perDirLimit = ToolJson.GetInt32Clamped(root, "per_dir_limit", int.MaxValue, 1, int.MaxValue);
            var totalLimit  = ToolJson.GetInt32Clamped(root, "total_limit", DefaultTotalLimit, 1, 100000);
            var descending  = ToolJson.GetBoolean(root, "desc", false);

            if (!TryParseSort(ToolJson.GetStringTrimmed(root, "sort"), out var sort, out var sortError))
                return Task.FromResult(ToolResult.Fail(sortError, "invalid sort"));

            var ws = HostServices.Sandbox.Workspace;
            if (!ws.DirectoryExists(path))
                return Task.FromResult(ToolResult.Fail($"Error: Directory not found at {path}", "directory not found"));

            var options = new ListOptions(maxDepth, pattern, perDirLimit, sort, descending, totalLimit);
            var writer = new TreeWriter(ws, options);
            writer.Write(path, cancellationToken);

            var text = writer.ToText(path);
            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            if (target == OutputTarget.Context) return Task.FromResult(ToolResult.Text(text));
            var blobName = ToolJson.GetStringTrimmed(root, "output_name");
            return Task.FromResult(ToolResult.Text(DataChannel.Route(target, BlobPayload.OfText(text), $"system_list_files: {path}", blobName)));
        }
        catch (JsonException)
        {
            return Task.FromResult(ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json"));
        }
        catch (PathBoundaryException) { throw; }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Fail($"Error listing directory: {ex.Message}", "list failed"));
        }
    }

    private static bool TryParseSort(string? raw, out ListSort sort, out string error)
    {
        error = string.Empty;
        sort = ListSort.Name;
        if (string.IsNullOrWhiteSpace(raw)) return true;

        switch (raw.Trim().ToLowerInvariant())
        {
            case "name":     sort = ListSort.Name; return true;
            case "size":     sort = ListSort.Size; return true;
            case "modified": sort = ListSort.Modified; return true;
            default:
                error = $"Error: unknown sort '{raw}'. Use 'name', 'size' or 'modified'.";
                return false;
        }
    }

    private enum ListSort { Name, Size, Modified }

    private sealed record ListOptions(
        int MaxDepth, string? Pattern, int PerDirLimit, ListSort Sort, bool Descending, int TotalLimit);

    /// <summary>
    /// Renders the tree as indentation plus a trailing '/' on directories — the shape everyone reads
    /// without a legend. Box-drawing characters were rejected: they cost tokens on every line and buy
    /// nothing a model needs. Indentation also means a path prefix is written once, not repeated on
    /// every line the way a flat path listing does.
    /// </summary>
    private sealed class TreeWriter
    {
        private readonly IWorkspace _ws;
        private readonly ListOptions _options;
        private readonly StringBuilder _sb = new();
        private readonly System.Text.RegularExpressions.Regex? _filter;

        private int _emitted;
        private bool _hitTotalLimit;
        private int _directories;
        private int _files;

        public TreeWriter(IWorkspace ws, ListOptions options)
        {
            _ws = ws;
            _options = options;
            _filter = options.Pattern is null ? null : SearchPatterns.GlobToRegex(options.Pattern);
        }

        public void Write(string root, CancellationToken ct) => WriteDirectory(root, 1, ct);

        private void WriteDirectory(string dir, int depth, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (_hitTotalLimit) return;

            var dirs = Safe(() => _ws.GetDirectories(dir))
                .Where(d => !SearchPatterns.IgnoreFolders.Contains(LeafName(d)))
                .ToList();

            var files = Safe(() => _ws.GetFiles(dir))
                .Where(MatchesFilter)
                .ToList();

            var entries = Order(dirs.Select(d => new Entry(d, true)).Concat(files.Select(f => new Entry(f, false))));

            var shown = 0;
            foreach (var entry in entries)
            {
                if (_hitTotalLimit) return;
                if (shown >= _options.PerDirLimit)
                {
                    // Honest truncation: a clipped listing the model reads as complete is worse than
                    // a short one that says how much it is hiding.
                    Append(depth, $"… and {entries.Count - shown} more in this directory");
                    return;
                }

                if (!Append(depth, entry.IsDirectory ? LeafName(entry.Path) + "/" : LeafName(entry.Path))) return;
                shown++;

                if (entry.IsDirectory)
                {
                    _directories++;
                    if (depth < _options.MaxDepth) WriteDirectory(entry.Path, depth + 1, ct);
                }
                else _files++;
            }
        }

        private List<Entry> Order(IEnumerable<Entry> entries)
        {
            // Directories first keeps the tree readable at any sort; the requested key orders within
            // each group, so "top 3 by size" means top 3 files, not three folders.
            var byGroup = entries.OrderBy(e => e.IsDirectory ? 0 : 1);

            return (_options.Sort, _options.Descending) switch
            {
                (ListSort.Size, false)     => byGroup.ThenBy(SizeOf).ToList(),
                (ListSort.Size, true)      => byGroup.ThenByDescending(SizeOf).ToList(),
                (ListSort.Modified, false) => byGroup.ThenBy(ModifiedOf).ToList(),
                (ListSort.Modified, true)  => byGroup.ThenByDescending(ModifiedOf).ToList(),
                (_, true)                  => byGroup.ThenByDescending(e => LeafName(e.Path), StringComparer.OrdinalIgnoreCase).ToList(),
                _                          => byGroup.ThenBy(e => LeafName(e.Path), StringComparer.OrdinalIgnoreCase).ToList()
            };
        }

        private bool Append(int depth, string text)
        {
            if (_emitted >= _options.TotalLimit)
            {
                _hitTotalLimit = true;
                return false;
            }
            _sb.Append(' ', (depth - 1) * 2).AppendLine(text);
            _emitted++;
            return true;
        }

        private bool MatchesFilter(string file)
        {
            if (_filter is null) return true;
            var leaf = LeafName(file);
            return _filter.IsMatch(file.Replace('\\', '/')) || _filter.IsMatch(leaf);
        }

        private static long SizeOf(Entry e)
        {
            if (e.IsDirectory) return 0;
            try { return new FileInfo(e.Path).Length; } catch { return 0; }
        }

        private static DateTime ModifiedOf(Entry e)
        {
            try { return File.GetLastWriteTimeUtc(e.Path); } catch { return DateTime.MinValue; }
        }

        public string ToText(string root)
        {
            var header = new StringBuilder();
            header.Append("Directory tree of ").Append(root)
                  .Append(" (").Append(_directories).Append(" dirs, ").Append(_files).Append(" files");
            if (_hitTotalLimit) header.Append("; TRUNCATED at total_limit — raise total_limit or narrow with pattern/max_depth");
            header.AppendLine(")");
            return header.Append(_sb).ToString();
        }

        private static IReadOnlyList<string> Safe(Func<IReadOnlyList<string>> get)
        {
            try { return get(); }
            catch { return Array.Empty<string>(); }   // inaccessible directory — skip, don't abort
        }

        private static string LeafName(string path)
        {
            var trimmed = path.TrimEnd('/', '\\');
            var slash = trimmed.LastIndexOfAny(new[] { '/', '\\' });
            return slash < 0 ? trimmed : trimmed.Substring(slash + 1);
        }

        private readonly record struct Entry(string Path, bool IsDirectory);
    }
}
