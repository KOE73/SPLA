using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.OneC.Indexing;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>
/// onec.index_configuration — trigger indexing of a 1C configuration dump directory.
/// This is the entry-point tool that the agent (or user) calls first.
/// </summary>
public class IndexConfigurationTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;
    public string Name => "onec.index.build";
    public string Description => "Parses 1C source files and updates the local SQLite index. Run this before doing any OneC object search or explanation.";

    public IndexConfigurationTool(OneCIndexDatabase db) => _db = db;

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name        = Name,
            Description = "Index a 1C configuration dump directory. " +
                          "Builds (or incrementally updates) the local SQLite index of objects and relations. " +
                          "Must be called before using any other onec.* tools on a new configuration.",
            Scope       = global::SPLA.Domain.Models.ToolScope.Project,
            Effect      = global::SPLA.Domain.Models.ToolEffect.Write,    // writes to .spla/index/onec.sqlite
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
            b.Field("configPath",    absPath);
            b.Field("objectsAdded",  report.ObjectsAdded);
            b.Field("objectsUpdated",report.ObjectsUpdated);
            b.Field("relationsAdded",report.RelationsAdded);
            b.Field("filesSkipped",  report.FilesSkipped);
            b.Field("filesWithErrors",report.FilesWithErrors);
            b.Field("elapsedSeconds", Math.Round(report.Elapsed.TotalSeconds, 2));
            if (report.Errors.Count > 0)
                b.List("errors", report.Errors.Take(20));
        });
        return yaml;
    }
}

