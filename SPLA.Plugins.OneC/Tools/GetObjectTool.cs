using System.Text.Json;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>onec.get_object — full card for a single object including its relations.</summary>
public class GetObjectTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;
    public string Name => "onec_get_object";
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
                    full_name        = new { type = "string",  description = "Fully-qualified object name, e.g. Document.РеализацияТоваров." },
                    include_snippets = new { type = "boolean", description = "Include source path and line (default false)." },
                    output           = SchemaParts.Output,
                    output_name      = SchemaParts.OutputName
                },
                required = new[] { "full_name" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var doc      = JsonDocument.Parse(argumentsJson);
        var fullName = ToolJson.GetString(doc.RootElement, "full_name") ?? "";
        var snippets = ToolJson.GetBoolean(doc.RootElement, "include_snippets", false);

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
                o.Field("full_name", obj.FullName);
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
        var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
        if (target == OutputTarget.Context) return Task.FromResult(yaml);
        var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
        return Task.FromResult(DataChannel.Route(target, BlobPayload.OfText(yaml), $"onec_get_object: {fullName}", blobName));
    }
}

