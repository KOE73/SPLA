using System.Text.Json;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>onec_find_writers — who writes to this register?</summary>
public class FindWritersTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;
    public string Name => "onec_find_writers";
    public string Description => "Finds objects that WRITE to the specified 1C object (e.g. write to a Register).";

    public FindWritersTool(OneCIndexDatabase db) => _db = db;

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name        = Name,
            Description = "Find all objects that write data to the specified register or storage object. " +
                          "Matches relations of type 'writes'.",
            Scope       = global::SPLA.Domain.Models.ToolScope.Project,
            Effect      = global::SPLA.Domain.Models.ToolEffect.Read,
            Risk        = global::SPLA.Domain.Models.ToolRisk.Low,
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    full_name = new { type = "string",  description = "Fully-qualified register name, e.g. AccumulationRegister.ОстаткиТоваров." },
                    limit       = new { type = "integer", description = "Max results (default 50)." },
                    offset      = new { type = "integer", description = "Pagination offset (default 0)." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
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

        var total = _db.CountRelationsTo(obj.Id, [RelationType.Writes]);
        var rows  = _db.GetRelationsTo(obj.Id, [RelationType.Writes], limit + offset)
                       .Skip(offset).Take(limit).ToList();

        var yaml = YamlResponse.Object(b =>
        {
            b.Field("register",  fullName);
            b.Field("total",     total);
            b.Field("returned",  rows.Count);
            b.Field("truncated", offset + rows.Count < total);
            b.ReverseRelationList("writers", rows, includeSource: true);
        });
        var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
        if (target == OutputTarget.Context) return Task.FromResult(yaml);
        var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
        return Task.FromResult(DataChannel.Route(target, BlobPayload.OfText(yaml), $"onec_find_writers: {fullName}", blobName));
    }
}

