using SPLA.Documents;
using SPLA.Documents.Rendering;
using SPLA.Domain.Formats;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Documents.Docx;
using SPLA.Plugins.Documents.Formats;
using SPLA.Plugins.Documents.Tools;

namespace SPLA.Plugins.Documents;

/// <summary>
/// The native document backend: Open XML for Word, ClosedXML for Excel, and nothing installed on
/// the machine.
///
/// <para><b>It contributes twice, and deliberately so.</b> The conversion pairs go into the core
/// <see cref="FormatConverterRegistry"/>, which is what lets <c>resource_read … as: text/markdown</c>
/// work on a docx address and what puts the projection in the system prompt. The tools are returned
/// the ordinary way, because the registry side only speaks when <c>agent.unified_resources</c> is
/// on — and reading a Word file is not an experimental capability.</para>
///
/// <para><b>Registration happens from inside the plugin's own load context</b>, into the host's
/// registry instance, through <c>ResolvedSettings.SharedServices</c>. That rendezvous is why a
/// second backend (pandoc, Aspose) is a folder to drop in rather than a change to the host — see
/// docs/adr/ADR_20260824_plugins_document-context.md.</para>
/// </summary>
public sealed class DocumentsPlugin : ISplaPlugin
{
    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        var extractor = new DocxExtractor();

        RegisterConversions(settings, extractor);

        return
        [
            new DocumentExtractTool(extractor),
            new SpreadsheetInspectTool(),
            new SpreadsheetReadRowsTool(),
            new SpreadsheetAppendRowsTool()
        ];
    }

    private static void RegisterConversions(ResolvedSettings settings, IDocumentExtractor extractor)
    {
        var registry = FormatConverterRegistry.For(settings);

        registry.Register(new DocumentRenderConverter(
            DocumentContentTypes.Docx, extractor, new MarkdownContextRenderer(),
            "Word document to Markdown — headings, paragraphs, lists, tables and image references; " +
            "fonts, colours, tracked changes and layout are dropped"));

        registry.Register(new DocumentRenderConverter(
            DocumentContentTypes.Docx, extractor, new PlainTextContextRenderer(),
            "Word document to plain text — the same content with no markup, tables as tab-separated lines"));

        registry.Register(new DocumentRenderConverter(
            DocumentContentTypes.Docx, extractor, new JsonContextRenderer(),
            "Word document to a JSON block tree — headings, lists and tables as typed nodes, for " +
            "picking out one table or one section rather than reading the whole document"));
    }
}
