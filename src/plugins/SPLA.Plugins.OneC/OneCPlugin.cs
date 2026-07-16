using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.OneC.Storage;
using SPLA.Plugins.OneC.Tools;
using SPLA.Plugins.OneC.Web;

namespace SPLA.Plugins.OneC;

public class OneCPlugin : ISplaPlugin, ISplaPluginAction
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private OneCIndexDatabase? _db;
    private OneCBrowserActions? _browser;

    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        // The index lives in the project's runtime area (root bucket keeps the historical
        // .spla/onec.sqlite location, which the web browser panel also reads).
        var runtimeDir = settings.Project
            .GetBucket(SPLA.Domain.Project.IProjectBackend.RootBucket)
            .MapToHostDirectory()
            ?? throw new InvalidOperationException("OneC index needs a disk-backed project backend.");
        var dbPath = Path.Combine(runtimeDir, "onec.sqlite");

        _db = new OneCIndexDatabase(dbPath);
        _db.EnsureCreated();
        _browser = new OneCBrowserActions(_db);

        return
        [
            new ExplainObjectTool(_db),
            new FindObjectTool(_db),
            new FindReadersTool(_db),
            new FindReferencesTool(_db),
            new FindWritersTool(_db),
            new GetDependenciesTool(_db),
            new GetObjectTool(_db),
            new GetReverseDependenciesTool(_db),
            new IndexConfigurationTool(_db)
        ];
    }

    /// <summary>
    /// Handles actions invoked from the "1C Configuration Browser" web panel via <c>plugin.action</c>.
    /// These are human-triggered browsing/rebuild operations, not agent tools — they replace the
    /// event handlers that used to live in the Avalonia panel's code-behind.
    /// </summary>
    public async Task<object?> InvokeActionAsync(string action, string? valueJson, CancellationToken ct = default)
    {
        if (_browser is null)
            throw new InvalidOperationException("OneC plugin is not initialized.");

        using var doc = string.IsNullOrWhiteSpace(valueJson)
            ? JsonDocument.Parse("{}")
            : JsonDocument.Parse(valueJson);
        var p = doc.RootElement;

        return action switch
        {
            "overview" => _browser.Overview(),
            "search" => _browser.Search(Str(p, "query")),
            "object" => _browser.Object(Str(p, "fullName")),
            "graph" => _browser.Graph(Str(p, "fullName"), Str(p, "mode"), Int(p, "depth", 3), Int(p, "limit", 400)),
            "formatters" => _browser.Formatters(),
            "format" => _browser.Format(Str(p, "formatterId"), Str(p, "fullName"), Str(p, "mode"), Int(p, "depth", 3), Int(p, "limit", 400)),
            "rebuild" => await _browser.RebuildAsync(Str(p, "path"), ct),
            _ => throw new InvalidOperationException($"Unknown onec plugin action: {action}")
        };
    }

    private static string? Str(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    private static int Int(JsonElement e, string name, int fallback) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.TryGetInt32(out var i)
            ? i
            : fallback;
}
