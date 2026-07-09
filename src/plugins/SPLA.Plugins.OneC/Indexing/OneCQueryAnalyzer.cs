using System.Text.RegularExpressions;
using SPLA.Plugins.OneC.Models;

namespace SPLA.Plugins.OneC.Indexing;

/// <summary>
/// Extracts 1C query texts from BSL files and identifies data sources
/// referenced in FROM / JOIN clauses.
///
/// 1C queries are embedded as string literals in BSL code:
///
///   Запрос = Новый Запрос;
///   Запрос.Текст = "ВЫБРАТЬ ...
///                  |ИЗ РегистрНакопления.ОстаткиТоваров КАК Остатки";
///
/// Multi-line queries use the pipe-continuation convention (|).
/// The analyzer reconstructs multi-line strings and then scans for
/// table references in ИЗ / СОЕДИНЕНИЕ clauses.
/// Confidence: heuristic.
/// </summary>
public class OneCQueryAnalyzer
{
    // Matches the start of a query string: «"ВЫБРАТЬ» or «"SELECT» (case-insensitive)
    private static readonly Regex RxQueryStart = new(
        @"""(?:\s*\|?\s*)(?:ВЫБРАТЬ|SELECT)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // A table reference after ИЗ or after a JOIN keyword
    // Captures the dotted identifier, e.g. «РегистрНакопления.ОстаткиТоваров»
    private static readonly Regex RxFromClause = new(
        @"(?:ИЗ|FROM|ЛЕВОЕ\s+СОЕДИНЕНИЕ|ПРАВОЕ\s+СОЕДИНЕНИЕ|ПОЛНОЕ\s+СОЕДИНЕНИЕ|ВНУТРЕННЕЕ\s+СОЕДИНЕНИЕ|СОЕДИНЕНИЕ|JOIN)\s+" +
        @"([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*\.[А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_.]*)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Maps 1C query table prefixes to metadata kinds
    private static readonly Dictionary<string, string> TablePrefixToKind = new(StringComparer.OrdinalIgnoreCase)
    {
        ["РегистрНакопления"]         = "AccumulationRegister",
        ["AccumulationRegister"]       = "AccumulationRegister",
        ["РегистрСведений"]           = "InformationRegister",
        ["InformationRegister"]        = "InformationRegister",
        ["РегистрБухгалтерии"]        = "AccountingRegister",
        ["AccountingRegister"]         = "AccountingRegister",
        ["Справочник"]                = "Catalog",
        ["Catalog"]                    = "Catalog",
        ["Документ"]                  = "Document",
        ["Document"]                   = "Document",
        ["ПланВидовХарактеристик"]    = "ChartOfCharacteristicTypes",
        ["ПланСчетов"]                = "ChartOfAccounts",
        ["Перечисление"]              = "Enum",
        ["РегистрРасчета"]            = "CalculationRegister",
    };

    /// <summary>
    /// Scan a BSL file for embedded query strings and extract
    /// table references as <see cref="CodeRelation"/> entries with type «queries».
    /// </summary>
    public List<CodeRelation> Analyze(string filePath, string relativeFilePath)
    {
        string content;
        try { content = File.ReadAllText(filePath); }
        catch { return []; }

        var results = new List<CodeRelation>();

        // Find all BSL string literals that look like 1C queries
        // Strategy: locate each ВЫБРАТЬ occurrence and grab surrounding text
        int searchFrom = 0;
        while (true)
        {
            var m = RxQueryStart.Match(content, searchFrom);
            if (!m.Success) break;

            // Estimate the line number of this match
            int lineNo = CountLines(content, m.Index);

            // Extract a reasonable chunk of the query (up to 4000 chars)
            int end    = Math.Min(m.Index + 4000, content.Length);
            var chunk  = content[m.Index..end];

            // Scan for table references in FROM / JOIN clauses
            foreach (Match fm in RxFromClause.Matches(chunk))
            {
                var tableRef = fm.Groups[1].Value.Trim();
                // tableRef looks like «РегистрНакопления.ОстаткиТоваров» or
                // «РегистрНакопления.ОстаткиТоваров.Остатки» (virtual table)

                var parts = tableRef.Split('.');
                if (parts.Length < 2) continue;

                var prefix    = parts[0];
                var tableName = parts[1]; // strip virtual table suffix if any

                if (!TablePrefixToKind.TryGetValue(prefix, out var kind)) continue;

                results.Add(new CodeRelation
                {
                    TargetKind   = kind,
                    TargetName   = tableName,
                    RelationType = RelationType.Queries,
                    SourcePath   = relativeFilePath,
                    SourceLine   = lineNo,
                    Confidence   = RelationConfidence.Heuristic,
                });
            }

            searchFrom = m.Index + 1;
        }

        return results;
    }

    private static int CountLines(string text, int upToIndex)
    {
        int count = 1;
        for (int i = 0; i < upToIndex && i < text.Length; i++)
            if (text[i] == '\n') count++;
        return count;
    }
}

