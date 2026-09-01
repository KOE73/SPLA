using System.Text;
using System.Text.Json;

namespace SPLA.Documents.Rendering;

/// <summary>
/// The tree as JSON — the rendering for machine consumption: a caller that wants the third table,
/// or every heading, or the rows under one section, and would otherwise be parsing markdown to
/// find them.
///
/// <para><b>Written by hand rather than serialised.</b> A polymorphic record hierarchy needs either
/// attributes on the model (which would tie the model to one serialiser) or converter registration
/// somewhere far from here. Writing the shape out makes the JSON contract readable in one place —
/// and the contract is what other tools will depend on.</para>
/// </summary>
public sealed class JsonContextRenderer : IContextRenderer
{
    public string TargetType => DocumentContentTypes.Json;

    public string Render(ContextDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = true }))
        {
            json.WriteStartObject();

            json.WriteStartObject("metadata");
            json.WriteString("source_name", document.Metadata.SourceName);
            json.WriteString("source_type", document.Metadata.SourceType);
            WriteOptional(json, "title", document.Metadata.Title);
            WriteOptional(json, "author", document.Metadata.Author);
            WriteOptional(json, "created", document.Metadata.Created);
            WriteOptional(json, "modified", document.Metadata.Modified);
            if (document.Metadata.Extra is { Count: > 0 } extra)
            {
                json.WriteStartObject("extra");
                foreach (var pair in extra) json.WriteString(pair.Key, pair.Value);
                json.WriteEndObject();
            }
            json.WriteEndObject();

            json.WriteStartArray("blocks");
            foreach (var block in document.Blocks) WriteBlock(json, block);
            json.WriteEndArray();

            json.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static void WriteBlock(Utf8JsonWriter json, ContextBlock block)
    {
        json.WriteStartObject();
        switch (block)
        {
            case HeadingBlock heading:
                json.WriteString("type", "heading");
                json.WriteNumber("level", heading.Level);
                json.WriteString("text", heading.Text);
                break;

            case ParagraphBlock paragraph:
                json.WriteString("type", "paragraph");
                json.WriteString("text", paragraph.Text);
                break;

            case ListBlock list:
                json.WriteString("type", "list");
                json.WriteBoolean("ordered", list.Ordered);
                json.WriteStartArray("items");
                foreach (var item in list.Items)
                {
                    json.WriteStartObject();
                    json.WriteNumber("level", item.Level);
                    json.WriteString("text", item.Text);
                    json.WriteEndObject();
                }
                json.WriteEndArray();
                break;

            case TableBlock table:
                json.WriteString("type", "table");
                WriteOptional(json, "caption", table.Caption);
                if (table.Header is not null)
                {
                    json.WriteStartArray("header");
                    foreach (var cell in table.Header) json.WriteStringValue(cell);
                    json.WriteEndArray();
                }
                json.WriteStartArray("rows");
                foreach (var row in table.Rows)
                {
                    json.WriteStartArray();
                    foreach (var cell in row) json.WriteStringValue(cell);
                    json.WriteEndArray();
                }
                json.WriteEndArray();
                break;

            case CodeBlock code:
                json.WriteString("type", "code");
                WriteOptional(json, "language", code.Language);
                json.WriteString("text", code.Text);
                break;

            case ImageBlock image:
                json.WriteString("type", "image");
                WriteOptional(json, "name", image.Name);
                WriteOptional(json, "alt", image.AltText);
                WriteOptional(json, "content_type", image.ContentType);
                if (image.ByteCount is { } bytes) json.WriteNumber("bytes", bytes);
                break;

            case SectionBreak section:
                json.WriteString("type", "section_break");
                WriteOptional(json, "label", section.Label);
                break;
        }
        json.WriteEndObject();
    }

    /// <summary>Absent stays absent. A null-valued property reads as "we looked and there is
    /// nothing", which for document metadata is usually a claim nobody checked.</summary>
    private static void WriteOptional(Utf8JsonWriter json, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) json.WriteString(name, value);
    }
}
