using System.Text;
using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Documents.Spreadsheet;

namespace SPLA.Plugins.Documents.Tools;

/// <summary>
/// <c>spreadsheet_inspect</c> — sheets, column names, row counts.
///
/// <para><b>The call that makes the append safe.</b> Rows are addressed by column header, so a
/// caller that has not seen the headers is guessing at their spelling; one cheap read removes the
/// guess and, with it, the class of failure where a row lands in the wrong column.</para>
/// </summary>
public sealed class SpreadsheetInspectTool : IMcpTool
{
    public string Name => "spreadsheet_inspect";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Lists the sheets of an .xlsx/.csv file with their column headers and row counts. " +
                "Call this before appending: the exact column names are what rows are keyed by.",
            Details = DetailsText,
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Path to the .xlsx/.xlsm/.csv/.tsv file." }
                },
                required = new[] { "path" }
            }
        }
    };

    private const string DetailsText = """
        tool: spreadsheet_inspect

        summary: sheets, column headers and data-row counts of a workbook or delimited file.

        arguments:
          path: workspace path. .xlsx, .xlsm, .csv, .tsv.

        notes:
          - Headers are read from row 1. Row counts exclude that header row.
          - A .csv/.tsv file has exactly one sheet, named after the file.

        examples:
          - request: { path: "registry.xlsx" }
        """;

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = JsonDocument.Parse(argumentsJson);
            var path = ToolJson.GetStringTrimmed(document.RootElement, "path");

            if (!DocumentsToolPaths.TryHostPath(path, mustExist: true, out var hostPath, out var error))
                return Task.FromResult(ToolResult.Fail($"Error: {error}", "path not readable"));

            var sheets = SpreadsheetStores.For(hostPath).Inspect(hostPath);

            var report = new StringBuilder();
            report.AppendLine(Path.GetFileName(hostPath));
            foreach (var sheet in sheets)
            {
                report.Append("- sheet '").Append(sheet.Name).Append("': ")
                      .Append(sheet.RowCount).Append(" data row(s); columns: ")
                      .AppendLine(sheet.Headers.Count == 0
                          ? "(none — row 1 is empty)"
                          : string.Join(", ", sheet.Headers));
            }

            return Task.FromResult(ToolResult.Text(report.ToString().TrimEnd()));
        }
        catch (JsonException) { return Task.FromResult(ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json")); }
        catch (SpreadsheetException ex) { return Task.FromResult(ToolResult.Fail($"Error: {ex.Message}", "spreadsheet")); }
        catch (Exception ex) { return Task.FromResult(ToolResult.Fail($"Error: {ex.Message}", ex.GetType().Name)); }
    }
}
