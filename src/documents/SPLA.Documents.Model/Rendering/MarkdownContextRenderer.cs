using System.Text;

namespace SPLA.Documents.Rendering;

/// <summary>
/// The tree as GitHub-flavoured Markdown — the rendering a language model reads best, and the one
/// this whole feature is usually asked for.
///
/// <para><b>Structure survives, decoration does not.</b> Heading levels, list nesting and table
/// shape are the parts a reader (human or model) actually navigates by, so they are rendered
/// exactly; bold/italic runs inside a paragraph are not, because carrying them would mean escaping
/// every stray asterisk in the source for a gain nobody reads.</para>
/// </summary>
public sealed class MarkdownContextRenderer : IContextRenderer
{
    public string TargetType => DocumentContentTypes.Markdown;

    public string Render(ContextDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var output = new StringBuilder();
        AppendPreamble(output, document.Metadata, document.Blocks);

        foreach (var block in document.Blocks)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    output.Append('\n')
                          .Append('#', Math.Clamp(heading.Level, 1, 6))
                          .Append(' ')
                          .AppendLine(Inline(heading.Text));
                    break;

                case ParagraphBlock paragraph when !string.IsNullOrWhiteSpace(paragraph.Text):
                    output.Append('\n').AppendLine(Inline(paragraph.Text));
                    break;

                case ListBlock list:
                    output.Append('\n');
                    var ordinal = 1;
                    foreach (var item in list.Items)
                    {
                        output.Append(' ', Math.Max(item.Level, 0) * 2)
                              .Append(list.Ordered ? $"{ordinal++}. " : "- ")
                              .AppendLine(Inline(item.Text));
                    }
                    break;

                case TableBlock table:
                    AppendTable(output, table);
                    break;

                case CodeBlock code:
                    output.Append("\n```").AppendLine(code.Language ?? string.Empty)
                          .AppendLine(code.Text.TrimEnd())
                          .AppendLine("```");
                    break;

                case ImageBlock image:
                    // A reference, exactly as the tree stores it: the bytes were never carried, so
                    // the link points at a name, not at data the reader could open from here.
                    output.Append("\n![")
                          .Append(Inline(image.AltText ?? image.Name ?? "image"))
                          .Append("](")
                          .Append(image.Name ?? "unnamed")
                          .AppendLine(")");
                    break;

                case SectionBreak section:
                    output.Append("\n---\n");
                    if (!string.IsNullOrWhiteSpace(section.Label))
                        output.Append("<!-- ").Append(section.Label).AppendLine(" -->");
                    break;
            }
        }

        return output.ToString().Trim() + "\n";
    }

    private static void AppendPreamble(
        StringBuilder output, DocumentMetadata metadata, IReadOnlyList<ContextBlock> blocks)
    {
        // Title the document with what it calls itself, and fall back to the file name. A document
        // whose rendering opens with no name at all is one a model has to be told the name of
        // separately, in a sentence that costs more than this line.
        //
        // Unless the document already opens with that very title as its first heading — which is the
        // normal case in Word, where the title property is filled in from the visible heading. Two
        // identical H1s in a row read as a rendering artefact, and that is exactly what they are.
        if (!OpensWithTitle(metadata.Title, blocks))
            output.Append("# ").AppendLine(Inline(metadata.Title ?? metadata.SourceName));

        var facts = new List<string> { $"source: {metadata.SourceName}" };
        if (!string.IsNullOrWhiteSpace(metadata.Author)) facts.Add($"author: {metadata.Author}");
        if (!string.IsNullOrWhiteSpace(metadata.Created)) facts.Add($"created: {metadata.Created}");
        if (!string.IsNullOrWhiteSpace(metadata.Modified)) facts.Add($"modified: {metadata.Modified}");
        if (metadata.Extra is { Count: > 0 })
            facts.AddRange(metadata.Extra.Select(pair => $"{pair.Key}: {pair.Value}"));

        output.Append('\n').Append('*').Append(string.Join(" · ", facts)).AppendLine("*");
    }

    private static bool OpensWithTitle(string? title, IReadOnlyList<ContextBlock> blocks)
        => !string.IsNullOrWhiteSpace(title)
           && blocks.FirstOrDefault(block => block is not SectionBreak) is HeadingBlock first
           && string.Equals(first.Text.Trim(), title.Trim(), StringComparison.CurrentCultureIgnoreCase);

    private static void AppendTable(StringBuilder output, TableBlock table)
    {
        var width = Math.Max(
            table.Header?.Count ?? 0,
            table.Rows.Count == 0 ? 0 : table.Rows.Max(row => row.Count));
        if (width == 0) return;

        output.Append('\n');
        if (!string.IsNullOrWhiteSpace(table.Caption))
            output.Append('*').Append(Inline(table.Caption!)).AppendLine("*\n");

        // GFM has no table without a header row. When the source marked none, the header is emitted
        // empty rather than promoting the first data row: promoting it would silently delete a row
        // of data, which is the kind of loss nobody notices until it is in a spreadsheet.
        var header = Enumerable.Range(0, width)
            .Select(i => table.Header is not null && i < table.Header.Count ? Cell(table.Header[i]) : string.Empty);

        output.Append("| ").Append(string.Join(" | ", header)).AppendLine(" |");
        output.Append('|').Append(string.Concat(Enumerable.Repeat(" --- |", width))).AppendLine();

        foreach (var row in table.Rows)
        {
            var cells = Enumerable.Range(0, width).Select(i => i < row.Count ? Cell(row[i]) : string.Empty);
            output.Append("| ").Append(string.Join(" | ", cells)).AppendLine(" |");
        }
    }

    /// <summary>A cell as one line: pipes escaped, newlines folded — a raw newline inside a cell
    /// ends the row as far as any markdown reader is concerned.</summary>
    private static string Cell(string? value) =>
        (value ?? string.Empty).Replace("|", "\\|").Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Trim();

    /// <summary>Inline text as one line. The tree already holds plain strings, so the only real work
    /// is refusing to let an embedded newline break the block it sits in.</summary>
    private static string Inline(string? value) =>
        (value ?? string.Empty).Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Trim();
}
