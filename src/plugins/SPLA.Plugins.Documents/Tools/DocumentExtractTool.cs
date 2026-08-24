using System.Text.Json;
using SPLA.Documents;
using SPLA.Documents.Rendering;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;

namespace SPLA.Plugins.Documents.Tools;

/// <summary>
/// <c>document_extract</c> — a Word file as meaning rather than as a file.
///
/// <para><b>Why this is not "read the file".</b> A .docx read with a filesystem tool is a zip
/// archive: the model gets binary, or a blob handle it cannot open, and the usual next move is to
/// ask the user to convert it. This collapses that whole exchange into one call — which is the test
/// a tool in this project has to pass.</para>
/// </summary>
public sealed class DocumentExtractTool(IDocumentExtractor extractor) : IMcpTool
{
    public string Name => "document_extract";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Reads a Word (.docx) file and returns its content: headings, paragraphs, lists, " +
                "tables and image references. Formatting is dropped. Use this instead of " +
                "system_read_file for .docx — as a file, a .docx is a zip archive.",
            Details = DetailsText,
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new
                    {
                        type = "string",
                        description = "Path to the .docx file, or a blob:<handle> holding its bytes."
                    },
                    @as = new
                    {
                        type = new[] { "string", "null" },
                        @enum = new[] { "markdown", "text", "json" },
                        description = "Rendering of the extracted content: 'markdown' (default), 'text', or " +
                                      "'json' (a typed block tree, for picking out one table or section)."
                    },
                    output = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "path" }
            }
        }
    };

    private const string DetailsText = """
        tool: document_extract

        summary: Word (.docx) file -> its semantic content.

        arguments:
          path: workspace path to a .docx, or a blob:<handle> from another tool.
          as:
            markdown: default. Headings as #, lists as -/1., tables as GFM tables.
            text:     no markup; tables become tab-separated lines.
            json:     {metadata, blocks[]} with typed blocks (heading/paragraph/list/table/code/image/section_break).

        kept:    outline (heading levels), paragraphs, list nesting, tables, page breaks, image names.
        dropped: fonts, sizes, colours, alignment, bold/italic, tracked changes, comments, headers/footers.

        notes:
          - Tables keep a header row only when the document marked one; otherwise every row is data.
          - Images are references (name, type, size), never bytes.
          - Only .docx. Legacy .doc, .pdf and .xlsx are not read by this tool.
          - Large document: set output='blob' and pass the handle on instead of filling the context.

        examples:
          - request: { path: "docs/request.docx" }
          - request: { path: "docs/request.docx", as: "json", output: "blob", output_name: "request" }
        """;

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = JsonDocument.Parse(argumentsJson);
            var root = document.RootElement;

            var path = ToolJson.GetStringTrimmed(root, "path");
            var (ok, bytes, name, error) = await DocumentsToolPaths.TryReadAsync(path, cancellationToken);
            if (!ok) return ToolResult.Fail($"Error: {error}", "path not readable");

            var renderer = RendererFor(ToolJson.GetStringTrimmed(root, "as"));

            ContextDocument extracted;
            using (var stream = new MemoryStream(bytes))
            {
                try
                {
                    extracted = await extractor.ExtractAsync(stream, name, cancellationToken);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    return ToolResult.Fail(
                        $"Error: '{name}' could not be read as a Word document — {ex.Message}", "not a docx");
                }
            }

            var rendered = renderer.Render(extracted);
            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            if (target == OutputTarget.Context) return ToolResult.Text(rendered);

            var summary = $"document_extract: {name} — {extracted.Blocks.Count} block(s), {rendered.Length} chars";
            return ToolResult.Text(DataChannel.Route(
                target,
                BlobPayload.OfText(rendered, renderer.TargetType),
                summary,
                ToolJson.GetStringTrimmed(root, "output_name")));
        }
        catch (JsonException) { return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json"); }
        catch (Exception ex) { return ToolResult.Fail($"Error: {ex.Message}", ex.GetType().Name); }
    }

    private static IContextRenderer RendererFor(string? requested) => requested?.ToLowerInvariant() switch
    {
        "json" => new JsonContextRenderer(),
        "text" or "plain" or "txt" => new PlainTextContextRenderer(),
        _ => new MarkdownContextRenderer()
    };
}
