using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SPLA.Documents;
using SPLA.Documents.Rendering;
using SPLA.Domain.Formats;
using SPLA.Domain.Resources;
using SPLA.Domain.Settings;
using SPLA.Plugins.Documents;
using SPLA.Plugins.Documents.Docx;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The Context half of the document plugin: a .docx becomes a block tree, and the tree becomes the
/// three renderings the agent consumes.
///
/// <para>Every test builds its own .docx with the Open XML SDK rather than checking in a binary
/// fixture — the input is then readable in the same file as the expectation, and a failure says
/// which structure produced it.</para>
/// </summary>
public sealed class DocumentContextTests
{
    // ── the tree → renderings ────────────────────────────────────────────────

    private static ContextDocument Sample() => new(
        new DocumentMetadata("request.docx", DocumentContentTypes.Docx, Title: "Заявка", Author: "Ivanov"),
        [
            new HeadingBlock(1, "Заявка"),
            new ParagraphBlock("Просим рассмотреть."),
            new ListBlock(false, [new ListItemLine(0, "первое"), new ListItemLine(1, "вложенное")]),
            new TableBlock(["Дата", "Сумма"], [["2026-08-24", "1250000"]]),
            new ImageBlock("image1.png", "схема", "image/png", 42)
        ]);

    [Fact]
    public void Markdown_keeps_the_outline_list_nesting_and_table_shape()
    {
        var markdown = new MarkdownContextRenderer().Render(Sample());

        Assert.Contains("# Заявка", markdown);
        Assert.Contains("*source: request.docx · author: Ivanov*", markdown);
        Assert.Contains("- первое", markdown);
        Assert.Contains("  - вложенное", markdown);
        Assert.Contains("| Дата | Сумма |", markdown);
        Assert.Contains("| 2026-08-24 | 1250000 |", markdown);
        Assert.Contains("![схема](image1.png)", markdown);
    }

    [Fact]
    public void Markdown_leaves_the_header_row_empty_rather_than_promoting_data()
    {
        var document = new ContextDocument(
            new DocumentMetadata("t.docx", DocumentContentTypes.Docx),
            [new TableBlock(null, [["a", "b"], ["c", "d"]])]);

        var markdown = new MarkdownContextRenderer().Render(document);

        // Both data rows survive: a table with no declared header must not lose its first row to
        // GFM's requirement that a header exists.
        Assert.Contains("| a | b |", markdown);
        Assert.Contains("| c | d |", markdown);
    }

    [Fact]
    public void Markdown_does_not_repeat_a_title_the_document_already_opens_with()
    {
        // Word fills the title property from the visible heading, so the naive rendering opens with
        // the same H1 twice — a rendering artefact a reader has to look past on every document.
        var markdown = new MarkdownContextRenderer().Render(Sample());

        Assert.Equal(1, System.Text.RegularExpressions.Regex.Matches(
            markdown, "^# Заявка\r?$", System.Text.RegularExpressions.RegexOptions.Multiline).Count);
        Assert.Contains("*source: request.docx", markdown);
    }

    [Fact]
    public void Markdown_still_names_a_document_whose_first_heading_is_something_else()
    {
        var document = new ContextDocument(
            new DocumentMetadata("request.docx", DocumentContentTypes.Docx, Title: "Заявка"),
            [new HeadingBlock(1, "Реквизиты")]);

        var markdown = new MarkdownContextRenderer().Render(document);

        Assert.Contains("# Заявка", markdown);
        Assert.Contains("# Реквизиты", markdown);
    }

    [Fact]
    public void Plain_text_carries_the_content_without_markup()
    {
        var text = new PlainTextContextRenderer().Render(Sample());

        Assert.Contains("Просим рассмотреть.", text);
        Assert.Contains("Дата\tСумма", text);
        Assert.DoesNotContain("| Дата", text);
        Assert.DoesNotContain("# ", text);
    }

    [Fact]
    public void Json_names_every_block_by_type()
    {
        var json = new JsonContextRenderer().Render(Sample());

        Assert.Contains("\"type\": \"heading\"", json);
        Assert.Contains("\"type\": \"list\"", json);
        Assert.Contains("\"type\": \"table\"", json);
        Assert.Contains("\"source_name\": \"request.docx\"", json);
        // Absent metadata stays absent rather than becoming a null nobody checked.
        Assert.DoesNotContain("\"created\"", json);
    }

    // ── .docx → the tree ─────────────────────────────────────────────────────

    [Fact]
    public async Task Docx_headings_paragraphs_and_lists_become_typed_blocks()
    {
        var bytes = DocxBuilder.Build(body =>
        {
            body.AppendChild(DocxBuilder.Paragraph("Заявка", styleId: "Heading1"));
            body.AppendChild(DocxBuilder.Paragraph("Просим рассмотреть."));
            body.AppendChild(DocxBuilder.Paragraph("Условия", styleId: "Heading2"));
            body.AppendChild(DocxBuilder.ListItem("первое", level: 0));
            body.AppendChild(DocxBuilder.ListItem("второе", level: 0));
        }, title: "Заявка", bulletList: true);

        var document = await Extract(bytes);

        Assert.Equal("Заявка", document.Metadata.Title);
        Assert.Collection(document.Blocks,
            block => Assert.Equal(new HeadingBlock(1, "Заявка"), block),
            block => Assert.Equal(new ParagraphBlock("Просим рассмотреть."), block),
            block => Assert.Equal(new HeadingBlock(2, "Условия"), block),
            block =>
            {
                // Two consecutive numbered paragraphs are ONE list, and the numbering part said bullet.
                var list = Assert.IsType<ListBlock>(block);
                Assert.False(list.Ordered);
                Assert.Equal(2, list.Items.Count);
            });
    }

    [Fact]
    public async Task Docx_table_takes_a_header_row_only_when_the_document_marked_one()
    {
        var withHeader = await Extract(DocxBuilder.Build(body =>
            body.AppendChild(DocxBuilder.Table([["Дата", "Сумма"], ["2026-08-24", "10"]], boldFirstRow: true))));

        var headed = Assert.IsType<TableBlock>(withHeader.Blocks.Single());
        Assert.Equal(["Дата", "Сумма"], headed.Header);
        Assert.Single(headed.Rows);

        var withoutHeader = await Extract(DocxBuilder.Build(body =>
            body.AppendChild(DocxBuilder.Table([["a", "b"], ["c", "d"]], boldFirstRow: false))));

        var plain = Assert.IsType<TableBlock>(withoutHeader.Blocks.Single());
        Assert.Null(plain.Header);
        Assert.Equal(2, plain.Rows.Count);
    }

    [Fact]
    public async Task Docx_reports_a_page_break_as_a_section_boundary()
    {
        var document = await Extract(DocxBuilder.Build(body =>
        {
            body.AppendChild(DocxBuilder.Paragraph("первая страница"));
            body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
            body.AppendChild(DocxBuilder.Paragraph("вторая страница"));
        }));

        Assert.Contains(document.Blocks, block => block is SectionBreak);
    }

    [Fact]
    public async Task Docx_that_is_not_a_word_document_fails_with_a_named_reason()
    {
        var extractor = new DocxExtractor();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("this is not a docx"));

        await Assert.ThrowsAnyAsync<Exception>(() => extractor.ExtractAsync(stream, "broken.docx"));
    }

    // ── the pairs in the core registry ───────────────────────────────────────

    [Fact]
    public async Task Plugin_registers_its_pairs_and_they_convert_through_the_registry()
    {
        var settings = new ResolvedSettings();
        var tools = new DocumentsPlugin().Initialize(settings).ToList();
        var registry = FormatConverterRegistry.For(settings);

        Assert.Contains(tools, tool => tool.Name == "document_extract");
        Assert.Contains(tools, tool => tool.Name == "spreadsheet_append_rows");

        Assert.Equal(
            ["application/json", "text/markdown", "text/plain"],
            registry.TargetsFor(DocumentContentTypes.Docx).OrderBy(t => t, StringComparer.Ordinal));

        var bytes = DocxBuilder.Build(body => body.AppendChild(DocxBuilder.Paragraph("Просим рассмотреть.")));
        Assert.True(registry.TryResolve(DocumentContentTypes.Docx, "text/markdown", out var converter, out _));

        var converted = await converter.ConvertAsync(new ResourceContent(bytes, DocumentContentTypes.Docx), null);

        Assert.Equal("text/markdown", converted.ContentType);
        Assert.Contains("Просим рассмотреть.", Encoding.UTF8.GetString(converted.Bytes));
    }

    private static async Task<ContextDocument> Extract(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return await new DocxExtractor().ExtractAsync(stream, "test.docx");
    }
}

/// <summary>A .docx assembled in memory: the fixture is the code that built it.</summary>
internal static class DocxBuilder
{
    public static byte[] Build(Action<Body> compose, string? title = null, bool bulletList = false)
    {
        var buffer = new MemoryStream();

        using (var package = WordprocessingDocument.Create(buffer, WordprocessingDocumentType.Document))
        {
            var mainPart = package.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());

            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles(
                HeadingStyle("Heading1", "heading 1"),
                HeadingStyle("Heading2", "heading 2"));

            var numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
            numberingPart.Numbering = new Numbering(
                new AbstractNum(
                    new Level(new NumberingFormat
                    {
                        Val = bulletList ? NumberFormatValues.Bullet : NumberFormatValues.Decimal
                    })
                    { LevelIndex = 0 })
                { AbstractNumberId = 1 },
                new NumberingInstance(new AbstractNumId { Val = 1 }) { NumberID = 1 });

            compose(mainPart.Document.Body!);
            mainPart.Document.Save();

            if (title is not null) package.PackageProperties.Title = title;
        }

        return buffer.ToArray();
    }

    public static Paragraph Paragraph(string text, string? styleId = null)
    {
        var paragraph = new Paragraph();
        if (styleId is not null)
            paragraph.AppendChild(new ParagraphProperties(new ParagraphStyleId { Val = styleId }));
        paragraph.AppendChild(new Run(new Text(text)));
        return paragraph;
    }

    public static Paragraph ListItem(string text, int level)
    {
        var properties = new ParagraphProperties(new NumberingProperties(
            new NumberingLevelReference { Val = level },
            new NumberingId { Val = 1 }));

        return new Paragraph(properties, new Run(new Text(text)));
    }

    public static Table Table(IReadOnlyList<IReadOnlyList<string>> rows, bool boldFirstRow)
    {
        var table = new Table();

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = new TableRow();
            foreach (var value in rows[rowIndex])
            {
                var run = new Run(new Text(value));
                if (boldFirstRow && rowIndex == 0) run.PrependChild(new RunProperties(new Bold()));
                row.AppendChild(new TableCell(new Paragraph(run)));
            }
            table.AppendChild(row);
        }

        return table;
    }

    private static Style HeadingStyle(string styleId, string name) => new(new StyleName { Val = name })
    {
        Type = StyleValues.Paragraph,
        StyleId = styleId
    };
}
