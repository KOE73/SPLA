using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using SPLA.MCP.Core.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools.Resources;

/// <summary>
/// <c>write</c> — content in, creating or replacing.
///
/// <para><b>Content may be a <c>blob:</c> handle</b>, resolved through
/// <see cref="DataChannel.ResolveBytes"/> exactly as the filesystem tools resolve theirs. That is what
/// makes "read there, write here" possible without the payload passing through the conversation on
/// the way — the whole point of the data channel, and the reason a resource read of binary answers
/// with a handle in the first place.</para>
/// </summary>
public sealed class ResourceWriteTool : ResourceToolBase
{
    public ResourceWriteTool(ResourceRegistry resources) : base(resources) { }

    public override string Name => "resource_write";

    public override ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Writes content to a resource address (scheme://path). 'content' may be a literal " +
                          "string or a blob:<handle> from another tool, so bulk data never has to pass through " +
                          "your context. Set overwrite=false to refuse an address that already exists.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    uri = UriParameter,
                    content = new
                    {
                        type = "string",
                        description = "The content to write, or a blob:<handle> naming stored data to write as-is."
                    },
                    overwrite = new
                    {
                        type = new[] { "boolean", "null" },
                        description = "True (default) replaces whatever is there; false means create-only and " +
                                      "refuses an existing address rather than replacing it."
                    }
                },
                required = new[] { "uri", "content", "overwrite" }
            }
        }
    };

    public override async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        if (!TryOpen(argumentsJson, ResourceVerb.Write, out var doc, out var provider, out var uri, out var failure))
            return failure;

        using (doc)
        {
            var raw = ToolJson.GetString(doc.RootElement, "content");
            if (raw is null) return ToolResult.Fail("error: 'content' is required.", "missing content");

            if (!DataChannel.ResolveBytes(raw, out var bytes, out var error))
                return ToolResult.Fail($"error: {error}", "unresolved blob");

            var overwrite = ToolJson.GetBoolean(doc.RootElement, "overwrite", true);

            if (provider is not IResourceWriter writer)
                return ToolResult.Refuse(
                    $"error: scheme '{uri.Scheme}://' does not support 'write'.", "verb not supported");

            try
            {
                await writer.WriteAsync(uri, bytes, overwrite, cancellationToken);
                return ToolResult.Text($"ok: wrote {bytes.Length} bytes to {uri}.");
            }
            catch (OperationCanceledException) { throw; }
            catch (PathBoundaryException) { throw; }
            catch (Exception ex) { return Failed($"writing '{uri}'", ex); }
        }
    }
}
