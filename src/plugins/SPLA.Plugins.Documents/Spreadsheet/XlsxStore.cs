using ClosedXML.Excel;

namespace SPLA.Plugins.Documents.Spreadsheet;

/// <summary>
/// Excel workbooks (.xlsx/.xlsm) through ClosedXML.
///
/// <para><b>An append is an append.</b> The workbook is opened, one block of rows is written under
/// the last used row, and it is saved — every other cell, along with the sheet's formats, formulas,
/// merges and print settings, is left exactly as ClosedXML found it. This is the one property that
/// makes the tool usable on a registry somebody maintains by hand, and it is why the Data half of
/// the API never offers "write the whole sheet".</para>
///
/// <para><b>Values keep their type.</b> A number arrives as a number and a date as a date, because
/// a registry whose Amount column is text stops summing and its Date column stops sorting — a
/// failure that surfaces weeks later, in someone else's report.</para>
/// </summary>
public sealed class XlsxStore : ISpreadsheetStore
{
    public IReadOnlyList<SpreadsheetSheet> Inspect(string hostPath)
    {
        using var workbook = new XLWorkbook(hostPath);

        return workbook.Worksheets
            .Select(sheet => new SpreadsheetSheet(sheet.Name, ReadHeaders(sheet), DataRowCount(sheet)))
            .ToList();
    }

    public SpreadsheetRows ReadRows(string hostPath, string? sheet, int offset, int limit)
    {
        using var workbook = new XLWorkbook(hostPath);
        var worksheet = Resolve(workbook, sheet);
        var headers = ReadHeaders(worksheet);
        var total = DataRowCount(worksheet);

        var rows = new List<IReadOnlyDictionary<string, string>>();
        // Row 1 is the header, so data starts at 2; offset counts data rows, not spreadsheet rows.
        var firstRow = 2 + Math.Max(offset, 0);
        var lastRow = Math.Min(1 + total, firstRow + Math.Max(limit, 0) - 1);

        for (var rowNumber = firstRow; rowNumber <= lastRow; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var column = 0; column < headers.Count; column++)
                values[headers[column]] = row.Cell(column + 1).GetFormattedString();

            rows.Add(values);
        }

        return new SpreadsheetRows(worksheet.Name, headers, rows, total);
    }

    public int AppendRows(
        string hostPath,
        string? sheet,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        bool create)
    {
        if (rows.Count == 0) return 0;

        var exists = File.Exists(hostPath);
        if (!exists && !create)
            throw new SpreadsheetException(
                $"'{Path.GetFileName(hostPath)}' does not exist. Pass create=true to start it, or check the path.");

        using var workbook = exists ? new XLWorkbook(hostPath) : new XLWorkbook();
        var worksheet = exists
            ? Resolve(workbook, sheet)
            : workbook.AddWorksheet(string.IsNullOrWhiteSpace(sheet) ? "Sheet1" : sheet);

        var headers = ReadHeaders(worksheet);

        // A brand-new sheet has no header row to match against, so the first append writes one from
        // the keys it was given. An EXISTING sheet's headers are never touched: a tool that could
        // add a column would also be a tool that could silently split one column into two spellings.
        if (headers.Count == 0)
        {
            headers = rows.SelectMany(row => row.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            for (var column = 0; column < headers.Count; column++)
                worksheet.Cell(1, column + 1).Value = headers[column];
        }

        foreach (var row in rows)
            foreach (var header in row.Keys)
                if (HeaderKey.IndexIn(headers, header) < 0)
                    throw new SpreadsheetException(HeaderKey.Unknown(header, headers, worksheet.Name));

        var nextRow = (worksheet.LastRowUsed()?.RowNumber() ?? 1) + 1;
        foreach (var row in rows)
        {
            foreach (var (header, value) in row)
            {
                var column = HeaderKey.IndexIn(headers, header);
                Write(worksheet.Cell(nextRow, column + 1), value);
            }
            nextRow++;
        }

        if (exists) workbook.Save();
        else workbook.SaveAs(hostPath);

        return rows.Count;
    }

    /// <summary>The named sheet, or the first one. An unknown name is refused with the list of real
    /// ones rather than quietly writing into whatever came first — the wrong sheet is the one
    /// mistake here that looks like success.</summary>
    private static IXLWorksheet Resolve(XLWorkbook workbook, string? sheet)
    {
        if (string.IsNullOrWhiteSpace(sheet))
            return workbook.Worksheets.FirstOrDefault()
                   ?? throw new SpreadsheetException("The workbook has no sheets.");

        return workbook.Worksheets.FirstOrDefault(w => string.Equals(w.Name, sheet, StringComparison.OrdinalIgnoreCase))
               ?? throw new SpreadsheetException(
                   $"Sheet '{sheet}' is not in this workbook. It has: " +
                   string.Join(", ", workbook.Worksheets.Select(w => $"'{w.Name}'")) + ".");
    }

    /// <summary>Row 1, up to the last cell that has anything in it. Blank cells in the middle keep
    /// their position — a column with no title is still a column, and dropping it would shift every
    /// header after it onto the wrong data.</summary>
    private static IReadOnlyList<string> ReadHeaders(IXLWorksheet worksheet)
    {
        var lastCell = worksheet.Row(1).LastCellUsed();
        if (lastCell is null) return [];

        return Enumerable.Range(1, lastCell.Address.ColumnNumber)
            .Select(column => worksheet.Cell(1, column).GetFormattedString().Trim())
            .ToList();
    }

    private static int DataRowCount(IXLWorksheet worksheet)
        => Math.Max((worksheet.LastRowUsed()?.RowNumber() ?? 0) - 1, 0);

    private static void Write(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Clear(XLClearOptions.Contents);
                break;
            case bool flag:
                cell.Value = flag;
                break;
            case double number:
                cell.Value = number;
                break;
            case decimal number:
                cell.Value = (double)number;
                break;
            case long number:
                cell.Value = number;
                break;
            case int number:
                cell.Value = number;
                break;
            case DateTime moment:
                cell.Value = moment;
                break;
            default:
                cell.Value = CellText.Of(value);
                break;
        }
    }
}
