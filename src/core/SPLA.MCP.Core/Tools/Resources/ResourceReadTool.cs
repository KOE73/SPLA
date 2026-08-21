using SPLA.Domain.Agent;
using SPLA.Domain.Formats;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using SPLA.MCP.Core.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools.Resources;

/// <summary>
/// <c>read</c> — content out of an address, optionally projected onto another type on the way out.
///
/// <para><b>The requested type picks the basket, not the detected one.</b> The same docx read as
/// bytes is a handle the model may pass on without ever seeing it; read as text it belongs in the
/// conversation whole. One address, one verb, two entirely different intentions — and only the caller
/// knows which one this call is. So <c>as</c> is what decides where the result lands, and the
/// detected type only answers "what have I got, and what can it become".</para>
///
/// <para><b>The default has to be safe by context.</b> With no <c>as</c>, textual content goes into
/// the conversation and binary content does NOT: it is stored through <see cref="DataChannel"/> and
/// answered with a handle. An unlabelled 200 MB mp4 read without a target must not be able to fill
/// the window, and a default that inlines whatever came back is exactly how it would.</para>
/// </summary>
public sealed class ResourceReadTool : ResourceToolBase
{
    private readonly FormatConverterRegistry _converters;

    public ResourceReadTool(ResourceRegistry resources, FormatConverterRegistry converters)
        : base(resources)
        => _converters = converters ?? throw new ArgumentNullException(nameof(converters));

    public override string Name => "resource_read";

    public override ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Reads the content at a resource address (scheme://path). Text comes back as text; " +
                "binary is stored as a blob:<handle> instead of being inlined. Set 'as' to a target " +
                "MIME type to have the content projected first (e.g. application/yaml from JSON, " +
                "text/plain from bytes) — the refusal lists what the source can actually reach.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    uri = UriParameter,
                    @as = new
                    {
                        type = new[] { "string", "null" },
                        description = "Optional target MIME type to project the content onto before it is " +
                                      "returned (e.g. 'text/plain', 'application/yaml', 'image/png'). Omit to " +
                                      "take the content as it is. An image result is shown to you as a picture; " +
                                      "text is inlined; anything else is stored as a blob handle."
                    },
                    output = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "uri", "as", "output", "output_name" }
            }
        }
    };

    public override async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        if (!TryOpen(argumentsJson, ResourceVerb.Read, out var doc, out var provider, out var uri, out var failure))
            return failure;

        using (doc)
        {
            var requested = ToolJson.GetStringTrimmed(doc.RootElement, "as");
            var forced = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
            var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");

            ResourceContent content;
            try
            {
                content = await provider.ReadAsync(uri, cancellationToken);
            }
            catch (OperationCanceledException) { throw; }
            catch (PathBoundaryException) { throw; }
            catch (Exception ex) { return Failed($"reading '{uri}'", ex); }

            var detected = string.IsNullOrWhiteSpace(content.ContentType) ? ContentTypes.Unknown : content.ContentType;

            if (requested is not null)
            {
                // Even 'as' equal to the detected type goes through the registry. A shortcut here
                // would leave the commonest projection — the identity one — as the single call that
                // never exercises the lookup, and a lookup only rare calls travel is a lookup nobody
                // notices is broken.
                if (!_converters.TryResolve(detected, requested, out var converter, out var error))
                    return ToolResult.Fail($"error: {error}", "no conversion");

                try
                {
                    content = await converter.ConvertAsync(content, null, cancellationToken);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    return ToolResult.Fail(
                        $"error: converting '{uri}' from {detected} to {requested} failed — {ex.Message}",
                        "conversion failed");
                }
            }

            var produced = string.IsNullOrWhiteSpace(content.ContentType) ? detected : content.ContentType;
            var bytes = content.Bytes ?? Array.Empty<byte>();

            // The basket comes off what is in hand NOW — the result type when a projection ran, the
            // detected type otherwise. That is the same rule stated once: an image is shown, text is
            // inlined, everything else is kept out of the conversation behind a handle.
            // A picture goes into the conversation only when it was ASKED for. Without 'as' the
            // caller stated no intention, and the safe reading of "read this address" is not "spend
            // a vision payload on whatever happened to be there".
            if (requested is not null && forced == OutputTarget.Context && ContentTypes.IsViewableImage(produced))
                return ToolResult.From(
                    new ToolText($"ok: {uri} as {produced} ({bytes.Length} bytes) — shown below."),
                    new ToolImage(Convert.ToBase64String(bytes), produced));

            if (forced == OutputTarget.Context && IsTextual(produced, bytes))
                return ToolResult.Text(Encoding.UTF8.GetString(Strip(bytes)));

            var target = forced == OutputTarget.Context ? OutputTarget.Blob : forced;
            var payload = IsTextual(produced, bytes)
                ? BlobPayload.OfText(Encoding.UTF8.GetString(Strip(bytes)), produced)
                : BlobPayload.OfBytes(bytes, produced);

            var note = DataChannel.Route(
                target,
                payload,
                $"resource_read: {uri} as {produced} ({bytes.Length} bytes) — kept out of the conversation " +
                "because it is not text; pass the handle on, or re-read with as='text/plain' to see it.",
                blobName);

            return ToolResult.From(
                new ToolText(note),
                new ToolResource(uri.ToString(), produced, $"{bytes.Length} bytes"));
        }
    }

    /// <summary>Whether content of this type belongs in the conversation verbatim. The label decides,
    /// with the bytes consulted only for the type that means "nobody said" — a declared
    /// <c>application/octet-stream</c> that reads as UTF-8 is text somebody forgot to name, while a
    /// declared <c>image/png</c> is not text no matter what its bytes round-trip as.</summary>
    private static bool IsTextual(string contentType, byte[] bytes)
    {
        if (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)) return true;

        if (contentType.StartsWith(ContentTypes.Unknown, StringComparison.OrdinalIgnoreCase))
            return ContentTypes.LooksLikeUtf8Text(bytes);

        foreach (var known in new[] { "application/json", "application/xml", "application/yaml", "application/x-yaml", "application/javascript" })
            if (contentType.StartsWith(known, StringComparison.OrdinalIgnoreCase)) return true;

        return contentType.Contains("+json", StringComparison.OrdinalIgnoreCase)
               || contentType.Contains("+xml", StringComparison.OrdinalIgnoreCase)
               || contentType.Contains("+yaml", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>A BOM is a marker, not content: carried into the text it would put an invisible
    /// U+FEFF at the front of every file the model reads.</summary>
    private static byte[] Strip(byte[] bytes)
        => bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? bytes[3..] : bytes;
}
