using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools.Resources;

/// <summary><c>mkdir</c> — make a container. Meaningless for stores whose containers are implied by
/// their keys, which is exactly why it is an extended verb and why a scheme that does not serve it
/// says so up front rather than at call time.</summary>
public sealed class ResourceMakeDirTool : ResourceToolBase
{
    public ResourceMakeDirTool(ResourceRegistry resources) : base(resources) { }

    public override string Name => "resource_mkdir";

    public override ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Creates a container (directory) at a resource address (scheme://path).",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
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
        if (!TryOpen(argumentsJson, ResourceVerb.MakeDir, out var doc, out var provider, out var uri, out var failure))
            return failure;

        using (doc)
        {
            if (provider is not IResourceContainerMaker maker)
                return ToolResult.Refuse(
                    $"error: scheme '{uri.Scheme}://' does not support 'mkdir'.", "verb not supported");

            try
            {
                await maker.MakeDirAsync(uri, cancellationToken);
                return ToolResult.Text($"ok: created {uri}.");
            }
            catch (OperationCanceledException) { throw; }
            catch (PathBoundaryException) { throw; }
            catch (Exception ex) { return Failed($"creating '{uri}'", ex); }
        }
    }
}
