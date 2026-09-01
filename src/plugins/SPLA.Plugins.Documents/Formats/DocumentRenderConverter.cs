using SPLA.Documents;
using SPLA.Documents.Rendering;
using SPLA.Domain.Formats;
using SPLA.Domain.Resources;

namespace SPLA.Plugins.Documents.Formats;

/// <summary>
/// One (document format → rendering) pair, as the core registry understands pairs: extract the
/// meaning with a backend, write it out with a renderer.
///
/// <para><b>Why the same class serves three registrations.</b> docx→markdown, docx→text and
/// docx→json differ only in the renderer; writing three classes would put the same extraction call
/// in three places and invite them to drift. The registry keys on the pair, not on the type, so one
/// implementation on three pairs is ordinary registration.</para>
///
/// <para>Everything crossing this boundary is bytes plus a MIME type. <see cref="ContextDocument"/>
/// itself never leaves the plugin — which is exactly what allows the model assembly to live inside
/// each backend's own load context instead of in the host's.</para>
/// </summary>
public sealed class DocumentRenderConverter(
    string sourceType,
    IDocumentExtractor extractor,
    IContextRenderer renderer,
    string summary) : IFormatConverter
{
    public string SourceType => sourceType;

    public string TargetType => renderer.TargetType;

    public string Summary => summary;

    public async Task<ResourceContent> ConvertAsync(
        ResourceContent source,
        IReadOnlyDictionary<string, object?>? options,
        CancellationToken ct = default)
    {
        using var stream = new MemoryStream(source.Bytes ?? []);
        var document = await extractor.ExtractAsync(stream, "document", ct);
        var rendered = renderer.Render(document);

        return new ResourceContent(System.Text.Encoding.UTF8.GetBytes(rendered), renderer.TargetType);
    }
}
