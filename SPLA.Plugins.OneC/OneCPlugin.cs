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
        // Use the workspace path to initialize the database
        var workspacePath = string.IsNullOrWhiteSpace(settings.WorkspacePath) ? "." : settings.WorkspacePath;
        var dbPath = Path.Combine(workspacePath, ".spla", "onec.sqlite");
        
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

