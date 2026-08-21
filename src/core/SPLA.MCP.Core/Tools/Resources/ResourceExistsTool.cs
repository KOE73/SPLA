using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools.Resources;

/// <summary><c>exists</c> — whether anything is at this address. Its own tool for the reason stated
/// on <see cref="ResourceToolBase"/>: the permission verdict is read off the definition, and this one
/// is the cheapest question in the set.</summary>
public sealed class ResourceExistsTool : ResourceToolBase
{
    public ResourceExistsTool(ResourceRegistry resources) : base(resources) { }

    public override string Name => "resource_exists";

    public override ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Answers whether anything is at a resource address (scheme://path). Cheaper than " +
                          "reading, and does not put content into your context.",
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
        if (!TryOpen(argumentsJson, ResourceVerb.Exists, out var doc, out var provider, out var uri, out var failure))
            return failure;

        using (doc)
        {
            try
            {
                var exists = await provider.ExistsAsync(uri, cancellationToken);
                return ToolResult.Text(exists ? $"yes: {uri} exists." : $"no: nothing is at {uri}.");
            }
            catch (OperationCanceledException) { throw; }
            catch (PathBoundaryException) { throw; }
            catch (Exception ex) { return Failed($"testing '{uri}'", ex); }
        }
    }
}
