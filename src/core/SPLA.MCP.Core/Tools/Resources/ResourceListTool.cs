using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools.Resources;

/// <summary>
/// <c>list</c> — a plain <c>ls</c> of what lies directly under an address. Never recursive and never
/// a filter: "name what lies here" is a verb, "find whatever matches this" is a query language, and
/// the second one is not admitted (see the admission test on <see cref="ResourceVerb"/>).
///
/// <para>This is also the answer for a CONTAINER — a workbook's sheets, an archive's entries — which
/// is why no conversion knob appears here. Which child you want depends on what the listing showed,
/// and a dependency on what was already seen is navigation, not projection.</para>
/// </summary>
public sealed class ResourceListTool : ResourceToolBase
{
    public ResourceListTool(ResourceRegistry resources) : base(resources) { }

    public override string Name => "resource_list";

    public override ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Lists what lies directly under a resource address (scheme://path). Direct children " +
                          "only — not recursive, not a search.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new { uri = UriParameter },
                required = new[] { "uri" }
            }
        }
    };

    public override async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        if (!TryOpen(argumentsJson, ResourceVerb.List, out var doc, out var provider, out var uri, out var failure))
            return failure;

        using (doc)
        {
            try
            {
                var entries = await provider.ListAsync(uri, cancellationToken);
                if (entries.Count == 0) return ToolResult.Text($"(empty) {uri} has no children.");

                var body = new StringBuilder($"{entries.Count} under {uri}:");
                foreach (var entry in entries.OrderBy(e => !e.IsContainer).ThenBy(e => e.Name, StringComparer.Ordinal))
                    body.Append($"\n- {entry.Name}{(entry.IsContainer ? "/" : "")}{(entry.Size is { } s ? $"  ({s} bytes)" : "")}");

                return ToolResult.Text(body.ToString());
            }
            catch (OperationCanceledException) { throw; }
            catch (PathBoundaryException) { throw; }
            catch (Exception ex) { return Failed($"listing '{uri}'", ex); }
        }
    }
}
