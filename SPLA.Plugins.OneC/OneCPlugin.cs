using System.Collections.Generic;
using System.IO;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.OneC.Storage;
using SPLA.Plugins.OneC.Tools;

namespace SPLA.Plugins.OneC;

public class OneCPlugin : ISplaPlugin
{
    private OneCIndexDatabase? _db;

    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        // The index lives in the project's runtime area (root bucket keeps the historical
        // .spla/onec.sqlite location, which the Avalonia-side panel also expects).
        var runtimeDir = settings.Project
            .GetBucket(SPLA.Domain.Project.IProjectBackend.RootBucket)
            .MapToHostDirectory()
            ?? throw new InvalidOperationException("OneC index needs a disk-backed project backend.");
        var dbPath = Path.Combine(runtimeDir, "onec.sqlite");

        _db = new OneCIndexDatabase(dbPath);
        _db.EnsureCreated();

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
}

