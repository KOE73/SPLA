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
    private OneCIndexDatabase? _db;
    private OneCWebActions? _webActions;

    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        // The index lives in the project's runtime area. The root bucket keeps the historical
        // .spla/onec.sqlite location.
        var runtimeDir = settings.Project
            .GetBucket(SPLA.Domain.Project.IProjectBackend.RootBucket)
            .MapToHostDirectory()
            ?? throw new InvalidOperationException("OneC index needs a disk-backed project backend.");
        var dbPath = Path.Combine(runtimeDir, "onec.sqlite");

        _db = new OneCIndexDatabase(dbPath);
        _db.EnsureCreated();
        _webActions = new(dbPath, settings.WorkspacePath);

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

    public async Task<object?> InvokeActionAsync(
        string action,
        string? valueJson,
        CancellationToken ct = default)
    {
        if (_webActions is null)
            throw new InvalidOperationException("OneC plugin is not initialized.");

        using var payload = JsonDocument.Parse(string.IsNullOrWhiteSpace(valueJson) ? "{}" : valueJson);
        return await _webActions.InvokeAsync(action, payload.RootElement, ct);
    }
}
