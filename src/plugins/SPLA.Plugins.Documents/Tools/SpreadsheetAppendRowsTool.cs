using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Documents.Spreadsheet;

namespace SPLA.Plugins.Documents.Tools;

/// <summary>
/// <c>spreadsheet_append_rows</c> — rows onto the end of a sheet, addressed by column header.
///
/// <para><b>Headers, not cells.</b> The model states <c>{"Date": …, "Company": …}</c> and never
/// <c>B27</c>. That is the whole reasoning chain this tool takes off the model: find the last row,
/// map each value to a column letter, avoid overwriting, keep the types. Every one of those steps is
/// deterministic and every one of them is a place a small model would slip.</para>
///
/// <para><b>Nothing existing is rewritten.</b> The append writes under the last used row; formats,
/// formulas and every other cell stay as they were. An unknown column is refused with the list of
/// real ones rather than silently added, because a sheet that grows a second spelling of one column
/// is broken in a way nobody spots for weeks.</para>
/// </summary>
public sealed class SpreadsheetAppendRowsTool : IMcpTool
{
    public string Name => "spreadsheet_append_rows";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Appends rows to a sheet (.xlsx/.csv). Each row is an object keyed by COLUMN HEADER — " +
                "{\"Date\": \"2026-08-24\", \"Amount\": 1250000} — never by cell address. Existing rows, " +
                "formats and formulas are left untouched; an unknown column is refused.",
            Details = DetailsText,
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
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
                    rows = new
                    {
                        type = "array",
                        description = "Rows to append. Each item is an object whose keys are column headers " +
                                      "as they appear in row 1 of the sheet.",
                        items = new { type = "object" }
                    },
                    create = new
                    {
                        type = new[] { "boolean", "null" },
                        description = "Create the file (and its header row from the given keys) if it does not " +
                                      "exist. Default false."
                    }
                },
                required = new[] { "path", "rows" }
            }
        }
    };

    private const string DetailsText = """
        tool: spreadsheet_append_rows

        summary: append rows to a sheet, keyed by column header.

        arguments:
          path:   workspace path. .xlsx, .xlsm, .csv, .tsv.
          sheet:  sheet name; omit for the first sheet.
          rows:   array of objects. Keys are COLUMN HEADERS from row 1, not cell addresses.
                  Values may be strings, numbers, booleans or null.
          create: true to create a missing file and write the header row from the keys. Default false.

        rules:
          - Call spreadsheet_inspect first to learn the exact column names.
          - An unknown column header is refused, with the sheet's real columns listed. Columns are
            never added to an existing sheet.
          - A column not mentioned in a row is left empty; column order does not matter.
          - Numbers are written as numbers and dates as dates where the format allows — do not
            pre-format them as text.
          - Existing cells, formats and formulas are untouched; rows go under the last used row.

        examples:
          - request:
              path: "registry.xlsx"
              sheet: "Requests"
              rows:
                - { "Date": "2026-08-24", "Company": "Romashka LLC", "Amount": 1250000 }
        """;

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = JsonDocument.Parse(argumentsJson);
            var root = document.RootElement;

            var path = ToolJson.GetStringTrimmed(root, "path");
            if (!DocumentsToolPaths.TryHostPath(path, mustExist: false, out var hostPath, out var error))
                return Task.FromResult(ToolResult.Fail($"Error: {error}", "path not writable"));

            if (!TryReadRows(root, out var rows, out var rowsError))
                return Task.FromResult(ToolResult.Fail($"Error: {rowsError}", "bad rows"));

            if (rows.Count == 0)
                return Task.FromResult(ToolResult.Fail("Error: 'rows' is empty — nothing to append.", "no rows"));

            var sheet = ToolJson.GetStringTrimmed(root, "sheet");
            var appended = SpreadsheetStores.For(hostPath)
                .AppendRows(hostPath, sheet, rows, ToolJson.GetBoolean(root, "create", false));

            return Task.FromResult(ToolResult.Text(
                $"ok: appended {appended} row(s) to {Path.GetFileName(hostPath)}" +
                (string.IsNullOrWhiteSpace(sheet) ? "." : $" (sheet '{sheet}').")));
        }
        catch (JsonException) { return Task.FromResult(ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json")); }
        catch (SpreadsheetException ex) { return Task.FromResult(ToolResult.Fail($"Error: {ex.Message}", "spreadsheet")); }
        catch (Exception ex) { return Task.FromResult(ToolResult.Fail($"Error: {ex.Message}", ex.GetType().Name)); }
    }

    /// <summary>
    /// The rows argument as CLR values, keeping JSON's own types: a number stays a number so the
    /// store can write it as one. A nested object or array is refused rather than stringified —
    /// there is no cell shape for it, and a cell reading "System.Object[]" is worse than a refusal.
    /// </summary>
    private static bool TryReadRows(
        JsonElement root,
        out IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        out string? error)
    {
        rows = [];
        error = null;

        if (!ToolJson.TryGetProperty(root, "rows", out var array) || array.ValueKind != JsonValueKind.Array)
        {
            error = "'rows' must be an array of objects keyed by column header.";
            return false;
        }

        var parsed = new List<IReadOnlyDictionary<string, object?>>();
        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                error = $"Each row must be an object keyed by column header; got {item.ValueKind}.";
                return false;
            }

            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in item.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        row[property.Name] = property.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                        row[property.Name] = property.Value.TryGetInt64(out var whole)
                            ? whole
                            : property.Value.GetDouble();
                        break;
                    case JsonValueKind.True or JsonValueKind.False:
                        row[property.Name] = property.Value.GetBoolean();
                        break;
                    case JsonValueKind.Null or JsonValueKind.Undefined:
                        row[property.Name] = null;
                        break;
                    default:
                        error = $"Column '{property.Name}' holds {property.Value.ValueKind}; a cell takes a " +
                                "string, number, boolean or null.";
                        return false;
                }
            }

            parsed.Add(row);
        }

        rows = parsed;
        return true;
    }
}
