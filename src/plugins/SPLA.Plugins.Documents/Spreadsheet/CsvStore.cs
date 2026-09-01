using System.Text;

namespace SPLA.Plugins.Documents.Spreadsheet;

/// <summary>
/// Delimited text (.csv/.tsv) with the same header-keyed contract as the Excel store, so a caller
/// writes the same call either way and the file's format stays the file's business.
///
/// <para><b>The delimiter is read, not assumed.</b> A registry exported by a Russian-locale Excel is
/// semicolon-separated, and appending comma-separated rows to it produces a file that still opens,
/// still looks like a table, and has every appended row in one column. Detected on read, preserved
/// on write.</para>
///
/// <para><b>A BOM is written on creation</b> for the same reason: Excel opens a UTF-8 file without
/// one as the local ANSI code page, and Cyrillic becomes mojibake at the point where a person finally
/// looks at the result.</para>
/// </summary>
public sealed class CsvStore : ISpreadsheetStore
{
    public IReadOnlyList<SpreadsheetSheet> Inspect(string hostPath)
    {
        var file = Read(hostPath);
        return [new SpreadsheetSheet(SheetName(hostPath), file.Headers, file.Rows.Count)];
    }

    public SpreadsheetRows ReadRows(string hostPath, string? sheet, int offset, int limit)
    {
        RequireSheet(hostPath, sheet);
        var file = Read(hostPath);

        var page = file.Rows.Skip(Math.Max(offset, 0)).Take(Math.Max(limit, 0));
        var rows = page
            .Select(cells =>
            {
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var column = 0; column < file.Headers.Count; column++)
                    values[file.Headers[column]] = column < cells.Count ? cells[column] : string.Empty;
                return (IReadOnlyDictionary<string, string>)values;
            })
            .ToList();

        return new SpreadsheetRows(SheetName(hostPath), file.Headers, rows, file.Rows.Count);
    }

    public int AppendRows(
        string hostPath,
        string? sheet,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        bool create)
    {
        if (rows.Count == 0) return 0;
        RequireSheet(hostPath, sheet);

        var exists = File.Exists(hostPath);
        if (!exists && !create)
            throw new SpreadsheetException(
                $"'{Path.GetFileName(hostPath)}' does not exist. Pass create=true to start it, or check the path.");

        var file = exists
            ? Read(hostPath)
            : new CsvFile(
                [.. rows.SelectMany(row => row.Keys).Distinct(StringComparer.OrdinalIgnoreCase)],
                [],
                DefaultDelimiter(hostPath),
                "\r\n",
                true);

        foreach (var row in rows)
            foreach (var header in row.Keys)
                if (HeaderKey.IndexIn(file.Headers, header) < 0)
                    throw new SpreadsheetException(HeaderKey.Unknown(header, file.Headers, SheetName(hostPath)));

        var text = new StringBuilder();
        text.Append(Line(file.Headers, file.Delimiter)).Append(file.NewLine);
        foreach (var existing in file.Rows)
            text.Append(Line(Pad(existing, file.Headers.Count), file.Delimiter)).Append(file.NewLine);

        foreach (var row in rows)
        {
            var cells = new string[file.Headers.Count];
            for (var column = 0; column < cells.Length; column++) cells[column] = string.Empty;
            foreach (var (header, value) in row)
                cells[HeaderKey.IndexIn(file.Headers, header)] = CellText.Of(value);

            text.Append(Line(cells, file.Delimiter)).Append(file.NewLine);
        }

        File.WriteAllText(hostPath, text.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: file.HasBom));
        return rows.Count;
    }

    /// <summary>A delimited file has exactly one sheet, and it is the file. Naming a different one
    /// is a call that would otherwise appear to succeed while ignoring an argument the caller
    /// clearly meant.</summary>
    private static void RequireSheet(string hostPath, string? sheet)
    {
        if (string.IsNullOrWhiteSpace(sheet)) return;
        if (string.Equals(sheet, SheetName(hostPath), StringComparison.OrdinalIgnoreCase)) return;

        throw new SpreadsheetException(
            $"'{Path.GetFileName(hostPath)}' is a delimited text file: it has one sheet, '{SheetName(hostPath)}'. " +
            $"Omit 'sheet', or use an .xlsx file if you need several.");
    }

    private static string SheetName(string hostPath) => Path.GetFileNameWithoutExtension(hostPath);

    private sealed record CsvFile(
        IReadOnlyList<string> Headers,
        IReadOnlyList<IReadOnlyList<string>> Rows,
        char Delimiter,
        string NewLine,
        bool HasBom);

    private static CsvFile Read(string hostPath)
    {
        if (!File.Exists(hostPath))
            throw new SpreadsheetException($"'{Path.GetFileName(hostPath)}' does not exist.");

        var bytes = File.ReadAllBytes(hostPath);
        var hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        var content = new UTF8Encoding(false).GetString(hasBom ? bytes[3..] : bytes);

        var newLine = content.Contains("\r\n") ? "\r\n" : "\n";
        var delimiter = DetectDelimiter(hostPath, content);
        var table = Parse(content, delimiter);

        return table.Count == 0
            ? new CsvFile([], [], delimiter, newLine, hasBom)
            : new CsvFile(table[0].Select(header => header.Trim()).ToList(), table.Skip(1).ToList(), delimiter, newLine, hasBom);
    }

    private static char DefaultDelimiter(string hostPath)
        => Path.GetExtension(hostPath).Equals(".tsv", StringComparison.OrdinalIgnoreCase) ? '\t' : ',';

    private static char DetectDelimiter(string hostPath, string content)
    {
        if (Path.GetExtension(hostPath).Equals(".tsv", StringComparison.OrdinalIgnoreCase)) return '\t';

        var firstLine = content.Split('\n', 2)[0];
        var candidates = new[] { ',', ';', '\t' };
        var best = candidates
            .Select(candidate => (Candidate: candidate, Count: firstLine.Count(character => character == candidate)))
            .OrderByDescending(pair => pair.Count)
            .First();

        return best.Count > 0 ? best.Candidate : ',';
    }

    /// <summary>RFC 4180 in the only two respects that matter in practice: a quoted field may hold
    /// the delimiter and a newline, and a doubled quote inside one is a literal quote.</summary>
    private static List<List<string>> Parse(string content, char delimiter)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;

        for (var i = 0; i < content.Length; i++)
        {
            var character = content[i];

            if (quoted)
            {
                if (character != '"') { field.Append(character); continue; }
                if (i + 1 < content.Length && content[i + 1] == '"') { field.Append('"'); i++; continue; }
                quoted = false;
                continue;
            }

            switch (character)
            {
                case '"':
                    quoted = true;
                    break;
                case '\r':
                    break;
                case '\n':
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = [];
                    break;
                default:
                    if (character == delimiter) { row.Add(field.ToString()); field.Clear(); }
                    else field.Append(character);
                    break;
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        // A trailing newline produces one empty trailing row in every naive parser. Dropping it here
        // keeps "how many rows does this file have" from depending on whether someone's editor adds
        // a final newline.
        if (rows.Count > 0 && rows[^1].Count == 1 && rows[^1][0].Length == 0) rows.RemoveAt(rows.Count - 1);

        return rows;
    }

    private static IReadOnlyList<string> Pad(IReadOnlyList<string> cells, int width)
        => cells.Count >= width ? cells : [.. cells, .. Enumerable.Repeat(string.Empty, width - cells.Count)];

    private static string Line(IEnumerable<string> cells, char delimiter)
        => string.Join(delimiter, cells.Select(cell => Quote(cell, delimiter)));

    private static string Quote(string cell, char delimiter)
        => cell.Contains(delimiter) || cell.Contains('"') || cell.Contains('\n') || cell.Contains('\r')
            ? '"' + cell.Replace("\"", "\"\"") + '"'
            : cell;
}
