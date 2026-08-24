using System.Text;
using System.Text.Json;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.Documents.Spreadsheet;

namespace SPLA.Plugins.Documents.Tools;

/// <summary>
/// <c>spreadsheet_read_rows</c> — a page of rows, keyed by column header.
///
/// <para>Paged by default rather than whole-file, for the same reason <c>sql_query</c> is: a
/// registry with forty thousand rows answers a question about its shape just as well with twenty of
/// them, and the difference is the entire context window.</para>
/// </summary>
public sealed class SpreadsheetReadRowsTool : IMcpTool
{
    private const int DefaultLimit = 20;

    public string Name => "spreadsheet_read_rows";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Reads rows of a sheet as a table keyed by column header. Returns the first 20 rows " +
                "unless limit/offset say otherwise; set output='blob' to capture a large range.",
            Details = DetailsText,
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Path to the .xlsx/.xlsm/.csv/.tsv file." },
                    sheet = new
                    {
                        type = new[] { "string", "null" },
                        description = "Sheet name. Omit for the first sheet (a .csv has only one)."
                    },
                    offset = new { type = new[] { "integer", "null" }, description = "Data rows to skip (default 0)." },
                    limit = new { type = new[] { "integer", "null" }, description = "Rows to return (default 20)." },
                    output = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "path" }
            }
        }
    };

    private const string DetailsText = """
        tool: spreadsheet_read_rows

        summary: rows of one sheet, as a table with the sheet's own column headers.

        arguments:
          path:   workspace path. .xlsx, .xlsm, .csv, .tsv.
          sheet:  sheet name; omit for the first sheet.
          offset: data rows to skip; 0 is the first row under the header.
          limit:  rows to return; default 20.

        notes:
          - Values come back as displayed (formatted), not as raw numbers or formulas.
          - The reply ends with the range and the total, so paging needs no second call to plan.

        examples:
          - request: { path: "registry.xlsx", sheet: "Requests", limit: 5 }
          - request: { path: "registry.xlsx", offset: 100, limit: 50, output: "blob" }
        """;

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = JsonDocument.Parse(argumentsJson);
            var root = document.RootElement;

            var path = ToolJson.GetStringTrimmed(root, "path");
            if (!DocumentsToolPaths.TryHostPath(path, mustExist: true, out var hostPath, out var error))
                return Task.FromResult(ToolResult.Fail($"Error: {error}", "path not readable"));

            var offset = Math.Max(ToolJson.GetInt32(root, "offset", 0), 0);
            var limit = Math.Max(ToolJson.GetInt32(root, "limit", DefaultLimit), 0);

            var page = SpreadsheetStores.For(hostPath)
                .ReadRows(hostPath, ToolJson.GetStringTrimmed(root, "sheet"), offset, limit);

            var table = Render(page, offset);
            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            if (target == OutputTarget.Context) return Task.FromResult(ToolResult.Text(table));

            var summary = $"spreadsheet_read_rows: {page.Rows.Count} row(s) of '{page.Sheet}'";
            return Task.FromResult(ToolResult.Text(DataChannel.Route(
                target, BlobPayload.OfText(table), summary, ToolJson.GetStringTrimmed(root, "output_name"))));
        }
        catch (JsonException) { return Task.FromResult(ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json")); }
        catch (SpreadsheetException ex) { return Task.FromResult(ToolResult.Fail($"Error: {ex.Message}", "spreadsheet")); }
        catch (Exception ex) { return Task.FromResult(ToolResult.Fail($"Error: {ex.Message}", ex.GetType().Name)); }
    }

    private static string Render(SpreadsheetRows page, int offset)
    {
        if (page.Headers.Count == 0) return $"'{page.Sheet}' has no header row — row 1 is empty.";
        if (page.Rows.Count == 0) return $"'{page.Sheet}': no rows in range (total {page.TotalRows}).";

        var table = new StringBuilder();
        table.Append("| ").Append(string.Join(" | ", page.Headers)).AppendLine(" |");
        table.Append('|').Append(string.Concat(Enumerable.Repeat(" --- |", page.Headers.Count))).AppendLine();

        foreach (var row in page.Rows)
            table.Append("| ")
                 .Append(string.Join(" | ", page.Headers.Select(header => Cell(row.GetValueOrDefault(header)))))
                 .AppendLine(" |");

        table.Append($"(rows {offset + 1}-{offset + page.Rows.Count} of {page.TotalRows} in '{page.Sheet}')");
        return table.ToString();

        static string Cell(string? value) =>
            (value ?? string.Empty).Replace("|", "\\|").Replace('\n', ' ').Replace('\r', ' ').Trim();
    }
}
