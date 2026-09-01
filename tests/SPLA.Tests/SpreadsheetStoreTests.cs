using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using SPLA.Plugins.Documents.Spreadsheet;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The Data half of the document plugin: rows addressed by column header, in the two formats a
/// registry actually arrives in.
///
/// <para>The property under test throughout is the one an append has to keep: <b>everything already
/// in the file survives</b> — other rows, other columns, the delimiter, the encoding — because the
/// file belongs to a person who did not ask for it to be rewritten.</para>
/// </summary>
public sealed class SpreadsheetStoreTests : IDisposable
{
    private readonly string _directory =
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "spla-sheets-" + Guid.NewGuid().ToString("N"))).FullName;

    public void Dispose()
    {
        try { Directory.Delete(_directory, recursive: true); } catch { /* a temp dir left behind is not a test failure */ }
    }

    private string Path_(string name) => Path.Combine(_directory, name);

    private static IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows(
        params IReadOnlyDictionary<string, object?>[] rows) => rows;

    private static Dictionary<string, object?> Row(params (string Key, object? Value)[] cells)
        => cells.ToDictionary(cell => cell.Key, cell => cell.Value);

    // ── csv ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Csv_append_creates_the_file_with_a_header_row_from_the_keys()
    {
        var path = Path_("registry.csv");
        var store = SpreadsheetStores.For(path);

        var appended = store.AppendRows(path, null, Rows(Row(("Date", "2026-08-24"), ("Amount", 1250000L))), create: true);

        Assert.Equal(1, appended);
        var sheet = Assert.Single(store.Inspect(path));
        Assert.Equal(["Date", "Amount"], sheet.Headers);
        Assert.Equal(1, sheet.RowCount);

        // A BOM, so the file opens as UTF-8 in Excel rather than as the local code page.
        var bytes = File.ReadAllBytes(path);
        Assert.Equal([0xEF, 0xBB, 0xBF], bytes[..3]);
    }

    [Fact]
    public void Csv_append_preserves_a_semicolon_delimiter_it_found()
    {
        var path = Path_("registry.csv");
        File.WriteAllText(path, "Дата;Организация\r\n2026-08-20;ООО Ромашка\r\n", new UTF8Encoding(true));

        SpreadsheetStores.For(path).AppendRows(
            path, null, Rows(Row(("Дата", "2026-08-24"), ("Организация", "ООО Василёк"))), create: false);

        var text = File.ReadAllText(path);
        Assert.Contains("2026-08-24;ООО Василёк", text);
        // The row that was already there is still there, and still first.
        Assert.Contains("2026-08-20;ООО Ромашка", text);
        Assert.DoesNotContain("2026-08-24,", text);
    }

    [Fact]
    public void Csv_append_refuses_an_unknown_column_and_names_the_real_ones()
    {
        var path = Path_("registry.csv");
        File.WriteAllText(path, "Date,Amount\r\n2026-08-20,10\r\n");

        var failure = Assert.Throws<SpreadsheetException>(() =>
            SpreadsheetStores.For(path).AppendRows(path, null, Rows(Row(("Total", 5L))), create: false));

        Assert.Contains("'Total'", failure.Message);
        Assert.Contains("'Date'", failure.Message);
        Assert.Contains("'Amount'", failure.Message);

        // And nothing was written: a refused append leaves the file exactly as it was.
        Assert.Equal("Date,Amount\r\n2026-08-20,10\r\n", File.ReadAllText(path));
    }

    [Fact]
    public void Csv_reads_quoted_fields_holding_the_delimiter()
    {
        var path = Path_("registry.csv");
        File.WriteAllText(path, "Company,Note\r\n\"Romashka, LLC\",\"said \"\"yes\"\"\"\r\n");

        var page = SpreadsheetStores.For(path).ReadRows(path, null, 0, 10);

        var row = Assert.Single(page.Rows);
        Assert.Equal("Romashka, LLC", row["Company"]);
        Assert.Equal("said \"yes\"", row["Note"]);
    }

    [Fact]
    public void Csv_matches_a_header_written_with_different_spacing_or_case()
    {
        var path = Path_("registry.csv");
        File.WriteAllText(path, "Date ,AMOUNT\r\n");

        SpreadsheetStores.For(path).AppendRows(
            path, null, Rows(Row(("date", "2026-08-24"), ("Amount", 7L))), create: false);

        Assert.Contains("2026-08-24,7", File.ReadAllText(path));
    }

    // ── xlsx ─────────────────────────────────────────────────────────────────

    private string BuildWorkbook(string name = "registry.xlsx", string sheet = "Requests")
    {
        var path = Path_(name);
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet(sheet);
        worksheet.Cell(1, 1).Value = "Дата";
        worksheet.Cell(1, 2).Value = "Организация";
        worksheet.Cell(1, 3).Value = "Сумма";
        worksheet.Cell(2, 1).Value = new DateTime(2026, 8, 20);
        worksheet.Cell(2, 2).Value = "ООО Ромашка";
        worksheet.Cell(2, 3).Value = 123000;
        workbook.SaveAs(path);
        return path;
    }

    [Fact]
    public void Xlsx_inspect_reports_sheets_headers_and_data_row_counts()
    {
        var path = BuildWorkbook();

        var sheet = Assert.Single(SpreadsheetStores.For(path).Inspect(path));

        Assert.Equal("Requests", sheet.Name);
        Assert.Equal(["Дата", "Организация", "Сумма"], sheet.Headers);
        Assert.Equal(1, sheet.RowCount);
    }

    [Fact]
    public void Xlsx_append_adds_under_the_last_row_and_keeps_numbers_numeric()
    {
        var path = BuildWorkbook();

        var appended = SpreadsheetStores.For(path).AppendRows(
            path, "Requests",
            Rows(Row(("Дата", "2026-08-24"), ("Организация", "ООО Василёк"), ("Сумма", 1250000L))),
            create: false);

        Assert.Equal(1, appended);

        using var workbook = new XLWorkbook(path);
        var worksheet = workbook.Worksheet("Requests");

        Assert.Equal("ООО Ромашка", worksheet.Cell(2, 2).GetString());   // untouched
        Assert.Equal("ООО Василёк", worksheet.Cell(3, 2).GetString());
        Assert.Equal(1250000d, worksheet.Cell(3, 3).GetDouble());        // a number, not text
    }

    [Fact]
    public void Xlsx_append_leaves_columns_it_was_not_given_empty()
    {
        var path = BuildWorkbook();

        SpreadsheetStores.For(path).AppendRows(
            path, "Requests", Rows(Row(("Организация", "ООО Василёк"))), create: false);

        using var workbook = new XLWorkbook(path);
        var worksheet = workbook.Worksheet("Requests");
        Assert.True(worksheet.Cell(3, 1).IsEmpty());
        Assert.Equal("ООО Василёк", worksheet.Cell(3, 2).GetString());
    }

    [Fact]
    public void Xlsx_append_refuses_an_unknown_sheet_and_names_the_real_ones()
    {
        var path = BuildWorkbook();

        var failure = Assert.Throws<SpreadsheetException>(() => SpreadsheetStores.For(path).AppendRows(
            path, "Заявки", Rows(Row(("Дата", "2026-08-24"))), create: false));

        Assert.Contains("'Requests'", failure.Message);
    }

    [Fact]
    public void Xlsx_read_rows_pages_and_reports_the_total()
    {
        var path = BuildWorkbook();
        SpreadsheetStores.For(path).AppendRows(
            path, "Requests", Rows(Row(("Организация", "ООО Василёк"))), create: false);

        var page = SpreadsheetStores.For(path).ReadRows(path, "Requests", offset: 1, limit: 10);

        Assert.Equal(2, page.TotalRows);
        var row = Assert.Single(page.Rows);
        Assert.Equal("ООО Василёк", row["Организация"]);
    }

    [Fact]
    public void An_unsupported_extension_is_refused_with_the_list_of_supported_ones()
    {
        var failure = Assert.Throws<SpreadsheetException>(() => SpreadsheetStores.For(Path_("report.docx")));
        Assert.Contains(".xlsx", failure.Message);
    }
}
