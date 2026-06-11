using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>onec.get_object — full card for a single object including its relations.</summary>
public class GetObjectTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;
    public string Name => "onec.object.get";
    public string Description => "Gets detailed information about a specific 1C object by its full name.";

    public GetObjectTool(OneCIndexDatabase db) => _db = db;

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name        = Name,
            Description = "Get the full card for a 1C configuration object: metadata + all outgoing relations grouped by type.",
            Scope       = global::SPLA.Domain.Models.ToolScope.Project,
            Effect      = global::SPLA.Domain.Models.ToolEffect.Read,
            Risk        = global::SPLA.Domain.Models.ToolRisk.Low,
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    fullName       = new { type = "string",  description = "Fully-qualified object name, e.g. Document.РеализацияТоваров." },
                    includeSnippets = new { type = "boolean", description = "Include source path and line (default false)." },
                },
                required = new[] { "fullName" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var doc      = JsonDocument.Parse(argumentsJson);
        var fullName = doc.RootElement.TryGetProperty("fullName",       out var fn) ? fn.GetString() ?? "" : "";
        var snippets = doc.RootElement.TryGetProperty("includeSnippets", out var sn) && sn.GetBoolean();

        var obj = _db.GetObjectByFullName(fullName);
        if (obj is null)
            return Task.FromResult($"error: object '{fullName}' not found in index.");

        var rels = _db.GetRelationsFrom(obj.Id, limit: 500);

        var grouped = rels
            .GroupBy(r => r.Type)
            .ToDictionary(g => g.Key, g => g.ToList());

        var yaml = YamlResponse.Object(b =>
        {
            b.Section("object", o =>
            {
                o.Field("fullName", obj.FullName);
                o.Field("kind",     obj.Kind);
                if (obj.Path    is not null) o.Field("path",    obj.Path);
                if (obj.Summary is not null) o.Field("summary", obj.Summary);
            });

            b.Section("relations", r =>
            {
                foreach (var relType in new[] {
                    RelationType.Owns, RelationType.Writes, RelationType.Reads,
                    RelationType.Calls, RelationType.Queries, RelationType.Uses })
                {
                    if (grouped.TryGetValue(relType, out var rows))
                        r.RelationList(relType, rows, snippets);
                }
            });
        });
        return Task.FromResult(yaml);
    }
}

