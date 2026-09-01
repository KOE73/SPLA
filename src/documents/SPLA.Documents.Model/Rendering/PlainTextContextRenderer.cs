using System.Text;

namespace SPLA.Documents.Rendering;

/// <summary>
/// The tree as plain text — for consumers that will not read markup: embeddings, keyword search,
/// and anything that would otherwise have to strip the markup back off again.
///
/// <para>Tables become tab-separated lines rather than aligned columns: alignment is padding, and
/// padding is the thing a plain-text consumer has to remove before it can use the values.</para>
/// </summary>
public sealed class PlainTextContextRenderer : IContextRenderer
{
    public string TargetType => DocumentContentTypes.PlainText;

    public string Render(ContextDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var output = new StringBuilder();
        output.AppendLine(document.Metadata.Title ?? document.Metadata.SourceName);

        foreach (var block in document.Blocks)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    output.AppendLine().AppendLine(heading.Text.Trim());
                    break;

                case ParagraphBlock paragraph when !string.IsNullOrWhiteSpace(paragraph.Text):
                    output.AppendLine().AppendLine(paragraph.Text.Trim());
                    break;

                case ListBlock list:
                    output.AppendLine();
                    foreach (var item in list.Items)
                        output.Append(' ', Math.Max(item.Level, 0) * 2).Append("- ").AppendLine(item.Text.Trim());
                    break;

                case TableBlock table:
                    output.AppendLine();
                    if (table.Header is { Count: > 0 })
                        output.AppendLine(string.Join('\t', table.Header));
                    foreach (var row in table.Rows)
                        output.AppendLine(string.Join('\t', row));
                    break;

                case CodeBlock code:
                    output.AppendLine().AppendLine(code.Text.TrimEnd());
                    break;

                case ImageBlock image:
                    output.AppendLine().Append("[image: ").Append(image.AltText ?? image.Name ?? "unnamed").AppendLine("]");
                    break;

                case SectionBreak:
                    output.AppendLine();
                    break;
            }
        }

        return output.ToString().Trim() + "\n";
    }
}
