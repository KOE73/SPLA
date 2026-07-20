using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.OneC.Indexing;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>
/// onec_build_index — index a 1C configuration dump directory.
/// </summary>
public class IndexConfigurationTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;
    public string Name => "onec_build_index";
    public string Description => "Parses 1C source files and updates the local SQLite index used by the OneC analysis tools.";

    public IndexConfigurationTool(OneCIndexDatabase db) => _db = db;

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name        = Name,
            Description = "Builds or incrementally updates the local SQLite index of objects and relations from a 1C configuration dump directory.",
            Scope       = global::SPLA.Domain.Models.ToolScope.Project,
            Effect      = global::SPLA.Domain.Models.ToolEffect.Write,    // writes to .spla/onec.sqlite
            Risk        = global::SPLA.Domain.Models.ToolRisk.Medium,
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    path = new { type = "string", description = "Absolute or relative path to the configuration dump directory." },
                },
                required = new[] { "path" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var doc  = JsonDocument.Parse(argumentsJson);
        var path = doc.RootElement.TryGetProperty("path", out var p) ? p.GetString() ?? "" : "";

        if (string.IsNullOrWhiteSpace(path))
            return "error: 'path' parameter is required.";

        // Resolve relative to CWD
        var absPath = Path.GetFullPath(path);
        if (!Directory.Exists(absPath))
            return $"error: directory not found: {absPath}";

        var indexer = new OneCIndexer(_db);
        var progress = new List<string>();

        var report = await Task.Run(() =>
            indexer.Index(absPath, new Progress<string>(msg => progress.Add(msg))),
            cancellationToken);

        var yaml = YamlResponse.Object(b =>
        {
            b.Field("config_path", absPath);
            b.Field("objects_added", report.ObjectsAdded);
            b.Field("objects_updated", report.ObjectsUpdated);
            b.Field("relations_added", report.RelationsAdded);
            b.Field("files_skipped", report.FilesSkipped);
            b.Field("files_with_errors", report.FilesWithErrors);
            b.Field("elapsed_seconds", Math.Round(report.Elapsed.TotalSeconds, 2));
            if (report.Errors.Count > 0)
                b.List("errors", report.Errors.Take(20));
        });
        return yaml;
    }
}

