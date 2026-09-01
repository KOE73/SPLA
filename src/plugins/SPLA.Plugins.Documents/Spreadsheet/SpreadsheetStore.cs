using System.Globalization;

namespace SPLA.Plugins.Documents.Spreadsheet;

/// <summary>One sheet, as an inspection answers for it.</summary>
/// <param name="RowCount">Data rows, header excluded — the number a caller reasons about when
/// deciding whether to read or to append.</param>
public sealed record SpreadsheetSheet(string Name, IReadOnlyList<string> Headers, int RowCount);

/// <summary>Rows keyed by column header, which is the only key a caller can state without having
/// seen the file's geometry.</summary>
public sealed record SpreadsheetRows(
    string Sheet,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyDictionary<string, string>> Rows,
    int TotalRows);

/// <summary>A refusal a caller can act on: it names what was wrong AND what was available.</summary>
public sealed class SpreadsheetException(string message) : InvalidOperationException(message);

/// <summary>
/// Tabular data, addressed by sheet and column header.
///
/// <para><b>This is the Data half of the split</b> (see
/// docs/adr/ADR_20260824_plugins_document-context.md): values, rows and headers. Styles, formulas,
/// merges, column widths and print areas are the Artifact half and are not reachable through here —
/// an append leaves whatever formatting the sheet already had, and never asks about it.</para>
///
/// <para>Implementations take HOST paths, already resolved and bounds-checked by the calling tool.
/// Nothing in here consults the workspace or the sandbox: a store that could also decide what it is
/// allowed to open would be a second place where that question is answered.</para>
/// </summary>
public interface ISpreadsheetStore
{
    IReadOnlyList<SpreadsheetSheet> Inspect(string hostPath);

    SpreadsheetRows ReadRows(string hostPath, string? sheet, int offset, int limit);

    /// <param name="rows">Values are strings, numbers, booleans or null, as they arrived from JSON.
    /// A store writes a number as a number where its format can hold one — a date typed as text is
    /// the classic way a registry stops sorting.</param>
    /// <returns>How many rows were appended.</returns>
    int AppendRows(
        string hostPath,
        string? sheet,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        bool create);
}

/// <summary>Which store serves a path. Extension, not content: the file may not exist yet — an
/// append that creates it is a normal call.</summary>
public static class SpreadsheetStores
{
    public static ISpreadsheetStore For(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" or ".xlsm" => new XlsxStore(),
            ".csv" or ".tsv" => new CsvStore(),
            _ => throw new SpreadsheetException(
                $"'{Path.GetFileName(path)}' is not a spreadsheet this tool can open. " +
                "Supported: .xlsx, .xlsm, .csv, .tsv.")
        };
    }
}

/// <summary>
/// How a column header written by a model is matched against one written by a person.
///
/// <para>Case, surrounding space, doubled inner spaces and non-breaking spaces all differ between
/// the two without meaning anything. Matching them literally is how "Сумма " fails to find "Сумма"
/// and a row lands in the wrong column — or, worse, is refused for a reason nobody can see by
/// looking at the screen.</para>
/// </summary>
public static class HeaderKey
{
    public static string Of(string? header)
    {
        if (string.IsNullOrWhiteSpace(header)) return string.Empty;

        var collapsed = string.Join(' ',
            header.Replace('\u00A0', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return collapsed.ToLowerInvariant();
    }

    /// <summary>Position of a header in a row of headers, or -1. Used by every store, so that
    /// "which column is this" cannot be answered two different ways in one product.</summary>
    public static int IndexIn(IReadOnlyList<string> headers, string? header)
    {
        var key = Of(header);
        for (var i = 0; i < headers.Count; i++)
            if (Of(headers[i]) == key) return i;
        return -1;
    }

    /// <summary>The refusal text for an unknown column, listing what the sheet does have. Written
    /// once because the whole value of the refusal is that the caller can fix the call from it.</summary>
    public static string Unknown(string header, IReadOnlyList<string> headers, string sheet) =>
        $"Column '{header}' is not in sheet '{sheet}'. Its columns are: " +
        (headers.Count == 0 ? "(none — the sheet has no header row)" : string.Join(", ", headers.Select(h => $"'{h}'"))) +
        ". Use the exact column names, or add the column to the sheet first.";
}

/// <summary>Turning a JSON-shaped value into the text a text format stores, without a locale
/// surprising anyone: an invariant round-trip, never the current culture's decimal comma.</summary>
public static class CellText
{
    public static string Of(object? value) => value switch
    {
        null => string.Empty,
        string text => text,
        bool flag => flag ? "TRUE" : "FALSE",
        double number => number.ToString("R", CultureInfo.InvariantCulture),
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        long number => number.ToString(CultureInfo.InvariantCulture),
        int number => number.ToString(CultureInfo.InvariantCulture),
        DateTime moment => moment.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
    };
}
