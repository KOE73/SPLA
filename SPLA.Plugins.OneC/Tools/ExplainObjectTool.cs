using System.Text.Json;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>
/// onec.explain_object — aggregate structured context about an object for LLM explanation.
/// Does not generate text itself — assembles data that the model will use to explain.
/// </summary>
public class ExplainObjectTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;

    public string Name => "onec_explain_object";
    public string Description => "Explains the purpose, structure, and usage of a 1C object based on index and LLM analysis.";

    public ExplainObjectTool(OneCIndexDatabase db) => _db = db;

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name        = Name,
            Description = "Gather a compact, structured context about a 1C object — its owned children, " +
                          "what it writes/reads/calls, and its important source files. " +
                          "Feed this context to the LLM so it can explain the object to the user.",
            Scope       = global::SPLA.Domain.Models.ToolScope.Project,
            Effect      = global::SPLA.Domain.Models.ToolEffect.Read,
            Risk        = global::SPLA.Domain.Models.ToolRisk.Low,
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    full_name   = new { type = "string", description = "Fully-qualified object name." },
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

        var obj = _db.GetObjectByFullName(fullName);
        if (obj is null)
            return Task.FromResult($"error: object '{fullName}' not found in index.");

        var allRels = _db.GetRelationsFrom(obj.Id, limit: 500);

        RelationsByType(allRels, out var owns, out var writes, out var reads,
                                 out var calls, out var queries, out var uses);

        // Collect important BSL source files from all relations
        var importantFiles = allRels
            .Where(r => r.SourcePath is not null && r.SourcePath.EndsWith(".bsl", StringComparison.OrdinalIgnoreCase))
            .Select(r => r.SourcePath!)
            .Distinct()
            .OrderBy(p => p)
            .Take(10)
            .ToList();

        var yaml = YamlResponse.Object(b =>
        {
            b.Section("object", o =>
            {
                o.Field("full_name", obj.FullName);
                o.Field("kind",     obj.Kind);
                if (obj.Summary is not null) o.Field("summary", obj.Summary);
            });

            if (owns.Count    > 0) b.List("owns",    owns   .Select(r => r.ToFullName));
            if (writes.Count  > 0) b.List("writes",  writes .Select(r => r.ToFullName));
            if (reads.Count   > 0) b.List("reads",   reads  .Select(r => r.ToFullName));
            if (queries.Count > 0) b.List("queries", queries.Select(r => r.ToFullName));
            if (calls.Count   > 0) b.List("calls",   calls  .Select(r => r.ToFullName));
            if (uses.Count    > 0) b.List("uses",    uses   .Select(r => r.ToFullName));
            if (importantFiles.Count > 0) b.List("important_files", importantFiles);
        });
        var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
        if (target == OutputTarget.Context) return Task.FromResult(yaml);
        var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
        return Task.FromResult(DataChannel.Route(target, BlobPayload.OfText(yaml), $"onec_explain_object: {fullName}", blobName));
    }

    private static void RelationsByType(
        List<RelationRow> all,
        out List<RelationRow> owns,
        out List<RelationRow> writes,
        out List<RelationRow> reads,
        out List<RelationRow> calls,
        out List<RelationRow> queries,
        out List<RelationRow> uses)
    {
        owns    = all.Where(r => r.Type == RelationType.Owns)    .DistinctBy(r => r.ToFullName).ToList();
        writes  = all.Where(r => r.Type == RelationType.Writes)  .DistinctBy(r => r.ToFullName).ToList();
        reads   = all.Where(r => r.Type == RelationType.Reads)   .DistinctBy(r => r.ToFullName).ToList();
        calls   = all.Where(r => r.Type == RelationType.Calls)   .DistinctBy(r => r.ToFullName).ToList();
        queries = all.Where(r => r.Type == RelationType.Queries)  .DistinctBy(r => r.ToFullName).ToList();
        uses    = all.Where(r => r.Type == RelationType.Uses)    .DistinctBy(r => r.ToFullName).ToList();
    }
}

