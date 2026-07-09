using System.Text.Json;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>onec.find_references — find all objects that reference the given object.</summary>
public class FindReferencesTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;
    public string Name => "onec_find_references";
    public string Description => "Finds all incoming references to a 1C object (where this object is used).";

    public FindReferencesTool(OneCIndexDatabase db) => _db = db;

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name        = Name,
            Description = "Find all objects that reference (use, call, read, write, or query) the specified object. " +
                          "Useful for impact analysis: 'who uses this register / module?'",
            Scope       = global::SPLA.Domain.Models.ToolScope.Project,
            Effect      = global::SPLA.Domain.Models.ToolEffect.Read,
            Risk        = global::SPLA.Domain.Models.ToolRisk.Low,
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    full_name = new { type = "string",  description = "Fully-qualified target object name." },
                    relation_types = new { type = "array",   items = new { type = "string" }, description = "Filter by relation types (owns, uses, calls, reads, writes, queries). Omit for all." },
                    limit          = new { type = "integer", description = "Max items returned (default 50)." },
                    offset         = new { type = "integer", description = "Pagination offset (default 0)." },
                    include_snippets = new { type = "boolean", description = "Include source file and line number (default true)." },
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
        var limit    = doc.RootElement.TryGetProperty("limit",          out var l)  ? l.GetInt32() : 50;
        var offset   = doc.RootElement.TryGetProperty("offset",         out var o)  ? o.GetInt32() : 0;
        var snippets = ToolJson.GetBoolean(doc.RootElement, "include_snippets", true);

        string[]? types = null;
        if (ToolJson.TryGetProperty(doc.RootElement, "relation_types", out var rt) && rt.ValueKind == System.Text.Json.JsonValueKind.Array)
            types = rt.EnumerateArray().Select(e => e.GetString()!).ToArray();

        var obj = _db.GetObjectByFullName(fullName);
        if (obj is null)
            return Task.FromResult($"error: object '{fullName}' not found in index.");

        var total = _db.CountRelationsTo(obj.Id, types);
        var rows  = _db.GetRelationsTo(obj.Id, types, limit + offset).Skip(offset).Take(limit).ToList();

        var yaml = YamlResponse.Object(b =>
        {
            b.Field("object",    fullName);
            b.Field("total",     total);
            b.Field("returned",  rows.Count);
            b.Field("truncated", offset + rows.Count < total);
            b.Section("references", r => r.ReverseRelationList("items", rows, snippets));
        });
        var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
        if (target == OutputTarget.Context) return Task.FromResult(yaml);
        var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
        return Task.FromResult(DataChannel.Route(target, BlobPayload.OfText(yaml), $"onec_find_references: {fullName}", blobName));
    }
}

