using System.Text.Json;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>onec_find_object — find objects by name or partial name.</summary>
public class FindObjectTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;

    public string Name => "onec_find_object";
    public string Description => "Finds 1C objects in the index by partial name or full name.";

    public FindObjectTool(OneCIndexDatabase db) => _db = db;

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name        = Name,
            Description = "Find 1C configuration objects by name or partial name. Returns a list of matches with kind and path.",
            Scope       = global::SPLA.Domain.Models.ToolScope.Project,
            Effect      = global::SPLA.Domain.Models.ToolEffect.Read,
            Risk        = global::SPLA.Domain.Models.ToolRisk.Low,
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    query  = new { type = "string",  description = "Name or partial name to search for." },
                    limit       = new { type = "integer", description = "Maximum results to return (default 20)." },
                    offset      = new { type = "integer", description = "Pagination offset (default 0)." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "query" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var doc    = JsonDocument.Parse(argumentsJson);
        var query  = doc.RootElement.TryGetProperty("query",  out var q) ? q.GetString() ?? "" : "";
        var limit  = doc.RootElement.TryGetProperty("limit",  out var l) ? l.GetInt32() : 20;
        var offset = doc.RootElement.TryGetProperty("offset", out var o) ? o.GetInt32() : 0;

        var total   = _db.FindObjects(query, 9999, 0).Count; // cheap for small configs
        var matches = _db.FindObjects(query, limit, offset);

        var yaml = YamlResponse.Object(b =>
        {
            b.Field("query",     query);
            b.Field("total",     total);
            b.Field("returned",  matches.Count);
            b.Field("truncated", matches.Count < total);
            if (matches.Count == 0)
            {
                b.Field("matches", "[]");
                return;
            }
            b.Section("matches", ml =>
            {
                foreach (var obj in matches)
                {
                    ml.Field("- fullName", obj.FullName);
                    ml.Field("  kind",     obj.Kind);
                    if (obj.Path    is not null) ml.Field("  path",    obj.Path);
                    if (obj.Summary is not null) ml.Field("  summary", obj.Summary);
                }
            });
        });
        var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
        if (target == OutputTarget.Context) return Task.FromResult(yaml);
        var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
        return Task.FromResult(DataChannel.Route(target, BlobPayload.OfText(yaml), $"onec_find_object: '{query}' {matches.Count} results", blobName));
    }
}

