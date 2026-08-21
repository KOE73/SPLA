using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools.Resources;

/// <summary><c>delete</c> — remove what is at an address. The clearest case for one tool per verb:
/// this definition declares <see cref="ToolRisk.High"/>, and folding it into a shared tool would make
/// every read ask for the trust this one needs.</summary>
public sealed class ResourceDeleteTool : ResourceToolBase
{
    public ResourceDeleteTool(ResourceRegistry resources) : base(resources) { }

    public override string Name => "resource_delete";

    public override ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Deletes what is at a resource address (scheme://path). Not reversible.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.High,
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
        if (!TryOpen(argumentsJson, ResourceVerb.Delete, out var doc, out var provider, out var uri, out var failure))
            return failure;

        using (doc)
        {
            if (provider is not IResourceRemover remover)
                return ToolResult.Refuse(
                    $"error: scheme '{uri.Scheme}://' does not support 'delete'.", "verb not supported");

            try
            {
                await remover.DeleteAsync(uri, cancellationToken);
                return ToolResult.Text($"ok: deleted {uri}.");
            }
            catch (OperationCanceledException) { throw; }
            catch (PathBoundaryException) { throw; }
            catch (Exception ex) { return Failed($"deleting '{uri}'", ex); }
        }
    }
}
