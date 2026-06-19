using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>onec.find_readers — who reads from this register/catalog?</summary>
public class FindReadersTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;
    public string Name => "onec_find_readers";
    public string Description => "Finds objects that READ from the specified 1C object (e.g. read from a Register).";

    public FindReadersTool(OneCIndexDatabase db) => _db = db;

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name        = Name,
            Description = "Find all objects that read data from the specified register, catalog, or document. " +
                          "Matches relations of types 'reads' and 'queries'.",
            Scope       = global::SPLA.Domain.Models.ToolScope.Project,
            Effect      = global::SPLA.Domain.Models.ToolEffect.Read,
            Risk        = global::SPLA.Domain.Models.ToolRisk.Low,
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    full_name = new { type = "string",  description = "Fully-qualified object name." },
                    limit    = new { type = "integer", description = "Max results (default 50)." },
                    offset   = new { type = "integer", description = "Pagination offset (default 0)." },
                },
                required = new[] { "full_name" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var doc      = JsonDocument.Parse(argumentsJson);
        var fullName = ToolJson.GetString(doc.RootElement, "full_name") ?? "";
        var limit    = doc.RootElement.TryGetProperty("limit",    out var l)  ? l.GetInt32() : 50;
        var offset   = doc.RootElement.TryGetProperty("offset",   out var o)  ? o.GetInt32() : 0;

        var obj = _db.GetObjectByFullName(fullName);
        if (obj is null)
            return Task.FromResult($"error: object '{fullName}' not found in index.");

        string[] readTypes = [RelationType.Reads, RelationType.Queries];
        var total = _db.CountRelationsTo(obj.Id, readTypes);
        var rows  = _db.GetRelationsTo(obj.Id, readTypes, limit + offset)
                       .Skip(offset).Take(limit).ToList();

        var yaml = YamlResponse.Object(b =>
        {
            b.Field("object",    fullName);
            b.Field("total",     total);
            b.Field("returned",  rows.Count);
            b.Field("truncated", offset + rows.Count < total);
            b.ReverseRelationList("readers", rows, includeSource: true);
        });
        return Task.FromResult(yaml);
    }
}

