using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SPLA.Documents;
using Drawings = DocumentFormat.OpenXml.Drawing;
using WordDrawings = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace SPLA.Plugins.Documents.Docx;

/// <summary>
/// Word (.docx) → <see cref="ContextDocument"/>.
///
/// <para><b>This is the normalisation the Open XML SDK deliberately does not do.</b> The SDK hands
/// back the file's own model — runs, style ids, numbering references, drawing anchors — which is
/// the right level for editing a document and the wrong one for understanding it. Turning
/// <c>pStyle=Heading2</c> into "a level-2 heading" is a judgement, and judgements belong in a
/// backend, not in a general-purpose parser.</para>
///
/// <para><b>What is deliberately dropped:</b> fonts, sizes, colours, alignment, spacing, borders,
/// bold/italic runs, tracked changes, comments, headers and footers. What is kept is what a reader
/// navigates by: outline, prose, list structure, tables, page boundaries, and the fact that a
/// picture was there.</para>
/// </summary>
public sealed class DocxExtractor : IDocumentExtractor
{
    public IReadOnlyCollection<string> SourceTypes { get; } = [DocumentContentTypes.Docx];

    public Task<ContextDocument> ExtractAsync(Stream source, string sourceName, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ct.ThrowIfCancellationRequested();

        // The SDK needs a seekable stream and will not take a forward-only one; a caller that read
        // the file over a network handed us exactly that. Copying here costs one buffer and removes
        // a failure mode from every call site.
        var seekable = source.CanSeek ? source : CopyToMemory(source, ct);

        using var package = WordprocessingDocument.Open(seekable, false);
        var mainPart = package.MainDocumentPart
                       ?? throw new InvalidOperationException(
                           $"'{sourceName}' is not a readable Word document: it has no main document part.");

        var styles = DocxStyles.From(mainPart);
        var numbering = DocxNumbering.From(mainPart);
        var blocks = new List<ContextBlock>();

        foreach (var element in mainPart.Document?.Body?.Elements() ?? [])
        {
            ct.ThrowIfCancellationRequested();

            switch (element)
            {
                case Paragraph paragraph:
                    AppendParagraph(blocks, paragraph, mainPart, styles, numbering);
                    break;

                case Table table:
                    blocks.Add(ReadTable(table));
                    break;
            }
        }

        var document = new ContextDocument(ReadMetadata(package, sourceName), blocks);
        return Task.FromResult(document);
    }

    private static MemoryStream CopyToMemory(Stream source, CancellationToken ct)
    {
        var buffer = new MemoryStream();
        source.CopyTo(buffer);
        ct.ThrowIfCancellationRequested();
        buffer.Position = 0;
        return buffer;
    }

    private static DocumentMetadata ReadMetadata(WordprocessingDocument package, string sourceName)
    {
        var properties = package.PackageProperties;
        return new DocumentMetadata(
            SourceName: sourceName,
            SourceType: DocumentContentTypes.Docx,
            Title: Blank(properties.Title),
            Author: Blank(properties.Creator),
            Created: properties.Created?.ToString("yyyy-MM-dd"),
            Modified: properties.Modified?.ToString("yyyy-MM-dd"));

        static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// One paragraph, in the four shapes a paragraph can actually take: a heading, a list item, a
    /// picture, or prose. List items are merged into the block that precedes them when they belong
    /// to the same numbering — a list of seven bullets is one list, not seven one-item lists.
    /// </summary>
    private static void AppendParagraph(
        List<ContextBlock> blocks,
        Paragraph paragraph,
        MainDocumentPart mainPart,
        DocxStyles styles,
        DocxNumbering numbering)
    {
        var properties = paragraph.ParagraphProperties;

        // A page break is a real boundary and is often the ONLY marker of one, so it is emitted
        // even when the paragraph carrying it is otherwise empty.
        if (paragraph.Descendants<Break>().Any(b => b.Type?.Value == BreakValues.Page))
            blocks.Add(new SectionBreak());

        foreach (var image in ReadImages(paragraph, mainPart))
            blocks.Add(image);

        var text = ReadText(paragraph);
        if (string.IsNullOrWhiteSpace(text)) return;

        var styleId = properties?.ParagraphStyleId?.Val?.Value;
        if (styles.HeadingLevel(styleId, properties?.OutlineLevel?.Val?.Value) is { } level)
        {
            blocks.Add(new HeadingBlock(level, text));
            return;
        }

        var numberingProperties = properties?.NumberingProperties;
        var numberingId = numberingProperties?.NumberingId?.Val?.Value;
        if (numberingId is not null)
        {
            var indent = numberingProperties?.NumberingLevelReference?.Val?.Value ?? 0;
            var item = new ListItemLine(indent, text);

            if (blocks.Count > 0 && blocks[^1] is ListBlock previous &&
                previous.Ordered == numbering.IsOrdered(numberingId.Value, indent))
            {
                blocks[^1] = previous with { Items = [.. previous.Items, item] };
                return;
            }

            blocks.Add(new ListBlock(numbering.IsOrdered(numberingId.Value, indent), [item]));
            return;
        }

        blocks.Add(new ParagraphBlock(text));
    }

    /// <summary>
    /// The paragraph's text, with the parts that carry meaning kept and the parts that carry
    /// appearance discarded. Hyperlink text is included inline; the target is not, because a URL in
    /// the middle of a sentence costs a model more attention than it repays — and the address is
    /// still in the file for a caller that wants it.
    /// </summary>
    private static string ReadText(OpenXmlElement paragraph)
    {
        var text = new StringBuilder();

        foreach (var node in paragraph.Descendants())
        {
            switch (node)
            {
                case Text run:
                    text.Append(run.Text);
                    break;
                case TabChar:
                    text.Append(' ');
                    break;
                case Break lineBreak when lineBreak.Type?.Value != BreakValues.Page:
                    text.Append(' ');
                    break;
            }
        }

        // A non-breaking space is a layout device that reaches consumers as a character they do
        // not split on: a value read out of such a run keeps it and then matches nothing.
        return text.ToString().Replace('\u00A0', ' ').Trim();
    }

    /// <summary>Pictures as references: name, alt text, type and size. The bytes stay in the file —
    /// see <see cref="ImageBlock"/> for why an extraction never carries them.</summary>
    private static IEnumerable<ImageBlock> ReadImages(Paragraph paragraph, MainDocumentPart mainPart)
    {
        foreach (var drawing in paragraph.Descendants<Drawing>())
        {
            var relationshipId = drawing.Descendants<Drawings.Blip>().FirstOrDefault()?.Embed?.Value;
            var description = drawing.Descendants<WordDrawings.DocProperties>().FirstOrDefault();

            string? name = description?.Name?.Value;
            string? contentType = null;
            long? byteCount = null;

            if (!string.IsNullOrWhiteSpace(relationshipId) &&
                mainPart.GetPartById(relationshipId) is ImagePart imagePart)
            {
                contentType = imagePart.ContentType;
                name ??= Path.GetFileName(imagePart.Uri.OriginalString);

                // The part's own stream is the only honest size: the drawing's extent is display
                // geometry, which is exactly the class of fact this extraction drops.
                using var stream = imagePart.GetStream();
                byteCount = stream.CanSeek ? stream.Length : null;
            }

            yield return new ImageBlock(name, Blank(description?.Description?.Value), contentType, byteCount);
        }

        static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// A table, with a header row only when the document actually marked one — by
    /// <c>&lt;w:tblHeader/&gt;</c> (the row Word repeats on every page) or by the whole first row
    /// being bold, which is how a header is marked in practice by people who never open the table
    /// properties dialog.
    ///
    /// <para>When neither holds, the header is null and every row is data. Promoting the first row
    /// on a guess would silently delete a row — and these rows are on their way into a spreadsheet,
    /// where a missing one is not noticed until someone reconciles totals.</para>
    /// </summary>
    private static TableBlock ReadTable(Table table)
    {
        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count == 0) return new TableBlock(null, []);

        var cells = rows
            .Select(row => (IReadOnlyList<string>)row.Elements<TableCell>().Select(CellText).ToList())
            .ToList();

        var declaresHeader = rows[0].TableRowProperties?.Elements<TableHeader>().Any() == true;
        var looksLikeHeader = cells[0].Count > 0
                              && cells[0].All(value => !string.IsNullOrWhiteSpace(value))
                              && rows[0].Descendants<Run>().Any()
                              && rows[0].Descendants<Run>().All(IsBold);

        return declaresHeader || looksLikeHeader
            ? new TableBlock(cells[0], cells.Skip(1).ToList())
            : new TableBlock(null, cells);

        static string CellText(TableCell cell) =>
            string.Join(' ', cell.Elements<Paragraph>().Select(ReadText).Where(t => t.Length > 0));

        static bool IsBold(Run run)
        {
            var bold = run.RunProperties?.Bold;
            return bold is not null && bold.Val?.Value != false;
        }
    }
}
