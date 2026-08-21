using SPLA.Domain.Agent;
using SPLA.Domain.Formats;
using SPLA.Domain.Resources;
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
/// turn. The picture rides out in the result as a <see cref="ToolImage"/>; the conversation loop
/// injects it as a synthetic user-image message on the next turn (same mechanism a screenshot
/// tool uses). Agent-scoped: works the same in every mode.
/// </summary>
public sealed class ImageViewTool : IMcpTool
{
    private readonly FormatConverterRegistry _converters;

    /// <param name="converters">The project's format projections. Required rather than optional: an
    /// optional registry would grow a fallback path, and a fallback path is how a seam ends up running
    /// empty in production while looking wired in the type system.</param>
    public ImageViewTool(FormatConverterRegistry converters)
        => _converters = converters ?? throw new ArgumentNullException(nameof(converters));

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
            ConversationBound = true,
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

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? handle;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            handle = ToolJson.GetStringTrimmed(doc.RootElement, "handle");
        }
        catch (JsonException) { return ToolResult.Fail("error: invalid_json", "invalid json"); }

        if (handle is null) return ToolResult.Fail("error: 'handle' is required", "missing handle");

        var session = AgentSessionScope.Current;
        if (session is null) return ToolResult.Refuse("error: no active chat session", "no chat session");

        var payload = session.Blobs.Get(handle);
        if (payload is null) return ToolResult.Fail($"error: no blob found for handle '{handle}'", "unknown handle");
        if (payload.Kind != BlobKind.Bytes || payload.Bytes is null)
            return ToolResult.Fail(
                $"error: blob '{handle}' holds text, not an image — read it with a tool that takes text, or blob_peek it.",
                "not an image");

        // Never assume. A byte blob whose type nobody recorded is checked against its own signature:
        // the previous default of "image/png" turned every typeless binary into a data URL of garbage
        // that the model then had to interpret as a picture.
        var contentType = payload.ContentType;
        if (string.IsNullOrWhiteSpace(contentType))
            contentType = ContentTypes.Sniff(payload.Bytes) ?? ContentTypes.Unknown;

        // What we want out is a picture. When the blob already IS one, the pair asked for is
        // source → itself, which the identity converter answers by handing the bytes straight back —
        // an ordinary screenshot therefore travels the same lookup as everything else and comes out
        // byte-for-byte as it went in. When it is not, image/png is the target worth naming, and the
        // registry's refusal says what this source CAN reach instead of dead-ending at "not an image".
        var target = ContentTypes.IsViewableImage(contentType) ? contentType! : "image/png";

        if (!_converters.TryResolve(contentType!, target, out var converter, out var error))
            return ToolResult.Fail(
                $"error: blob '{handle}' cannot be shown as a picture ({payload.Size} bytes). {error} " +
                "Pass the handle to a writing/uploading tool to move it, or use blob_peek to inspect its bytes.",
                "no conversion");

        ResourceContent picture;
        try
        {
            picture = await converter.ConvertAsync(
                new ResourceContent(payload.Bytes, contentType!), null, cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return ToolResult.Fail(
                $"error: converting blob '{handle}' from {contentType} to {target} failed — {ex.Message}",
                "conversion failed");
        }

        // The picture rides in the result rather than being pushed into the chat's pending sink: the
        // tool states that it has an image, and the conversation layer decides how a given model gets
        // to see it. The wording still holds — that layer delivers it on the next turn.
        return ToolResult.From(
            new ToolText($"ok: queued image from '{handle}' ({payload.Size} bytes) — visible on your next turn."),
            new ToolImage(Convert.ToBase64String(picture.Bytes), picture.ContentType));
    }
}
