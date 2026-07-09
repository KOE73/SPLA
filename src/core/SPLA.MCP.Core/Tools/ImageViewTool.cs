using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Lets the model pull a previously stored blob image into its own context as a real picture.
/// Any tool that stores image bytes in the chat's <see cref="IBlobStore"/> (content type
/// <c>image/*</c>) makes that picture viewable this way — not just the producing tool's own
/// turn. Pushes a data URL into the chat's <see cref="IPendingImageSink"/>; the conversation
/// loop injects it as a synthetic user-image message on the next turn (same mechanism a
/// screenshot tool uses directly). Agent-scoped: works the same in every mode.
/// </summary>
public sealed class ImageViewTool : IMcpTool
{
    public string Name => "image_view";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Loads a stored image (by blob: handle) into context so you can actually see it on your " +
                          "next turn. Use this to view a screenshot or other image a tool saved to the blob store " +
                          "instead of inlining it into the conversation.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    handle = new { type = "string", description = "The blob: handle of a stored image (e.g. from browser_screenshot)." }
                },
                required = new[] { "handle" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? handle;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            handle = ToolJson.GetStringTrimmed(doc.RootElement, "handle");
        }
        catch (JsonException) { return Task.FromResult("error: invalid_json"); }

        if (handle is null) return Task.FromResult("error: 'handle' is required");

        var session = AgentSessionScope.Current;
        if (session is null) return Task.FromResult("error: no active chat session");

        var payload = session.Blobs.Get(handle);
        if (payload is null) return Task.FromResult($"error: no blob found for handle '{handle}'");
        if (payload.Kind != BlobKind.Bytes || payload.Bytes is null)
            return Task.FromResult($"error: blob '{handle}' is not an image (kind={payload.Kind})");

        var contentType = string.IsNullOrWhiteSpace(payload.ContentType) ? "image/png" : payload.ContentType;
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult($"error: blob '{handle}' content type '{contentType}' is not an image");

        var dataUrl = $"data:{contentType};base64,{Convert.ToBase64String(payload.Bytes)}";
        session.Images.Push(dataUrl);
        return Task.FromResult($"ok: queued image from '{handle}' ({payload.Size} bytes) — visible on your next turn.");
    }
}
