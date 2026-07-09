using System.Text.RegularExpressions;
using SPLA.Plugins.OneC.Models;

namespace SPLA.Plugins.OneC.Indexing;

/// <summary>
/// Lexical analyzer for BSL (1C:Enterprise scripting language) source files.
///
/// Extracts three relation categories from source code:
///
/// 1. calls — CommonModule.Method() or CommonModule.SubObject.Method()
///            Pattern: word followed by dot, where the word matches a known
///            common module name (resolved against the object index at call time).
///            Confidence: heuristic
///
/// 2. writes — References to movement/write operations on registers:
///             Движения.<RegisterName>.Добавить|Записать|ВыполнитьДвижения
///             РегистрНакопления.<Name>.СоздатьНаборЗаписей
///             Confidence: heuristic
///
/// 3. reads / uses — References to other metadata objects by their type prefix:
///             Справочники.<Name>  → Catalog         → reads
///             Документы.<Name>    → Document        → uses
///             РегистрыНакопления  → AccumulationReg → reads
///             etc.
///            Confidence: heuristic
///
/// All patterns are compiled once as static fields.
/// Line numbers are captured for source navigation.
/// </summary>
public class OneCCodeAnalyzer
{
    // ── CommonModule call: word.word( or word.word.word(
    // Captures the module name (group 1)
    private static readonly Regex RxCallModule = new(
        @"\b([А-ЯёЁA-Za-zА-я][А-ЯёЁA-Za-zА-я0-9_]*)\.[А-ЯёЁA-Za-zА-я][А-ЯёЁA-Za-zА-я0-9_.]*\s*\(",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // ── Register movement write detection
    // Matches «Движения.ИмяРегистра» or «РегистрНакопления.ИмяРегистра.СоздатьНаборЗаписей»
    private static readonly Regex RxMovements = new(
        @"\bДвижения\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)",
        RegexOptions.Compiled);

    private static readonly Regex RxRegWrite = new(
        @"\b(?:РегистрНакопления|РегистрБухгалтерии|РегистрРасчета)\." +
        @"([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)\.(?:СоздатьНаборЗаписей|Записать)",
        RegexOptions.Compiled);

    // ── Object read / use references
    private static readonly (Regex Pattern, string Kind, string RelationType)[] RxReads =
    [
        (new Regex(@"\bСправочники\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)",     RegexOptions.Compiled), "Catalog",             RelationType.Reads),
        (new Regex(@"\bДокументы\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)",       RegexOptions.Compiled), "Document",            RelationType.Uses),
        (new Regex(@"\bРегистрыНакопления\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)", RegexOptions.Compiled), "AccumulationRegister", RelationType.Reads),
        (new Regex(@"\bРегистрыСведений\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)",  RegexOptions.Compiled), "InformationRegister",  RelationType.Reads),
        (new Regex(@"\bРегистрыБухгалтерии\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)", RegexOptions.Compiled), "AccountingRegister", RelationType.Reads),
        (new Regex(@"\bПланыВидовХарактеристик\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)", RegexOptions.Compiled), "ChartOfCharacteristicTypes", RelationType.Uses),
        (new Regex(@"\bПланыСчетов\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)",      RegexOptions.Compiled), "ChartOfAccounts",     RelationType.Reads),
        (new Regex(@"\bОтчеты\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)",           RegexOptions.Compiled), "Report",              RelationType.Uses),
        (new Regex(@"\bОбработки\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)",        RegexOptions.Compiled), "Processing",          RelationType.Uses),
        (new Regex(@"\bПеречисления\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)",     RegexOptions.Compiled), "Enum",                RelationType.Uses),
        (new Regex(@"\bКонстанты\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)",        RegexOptions.Compiled), "Constant",            RelationType.Reads),
        (new Regex(@"\bЖурналыДокументов\.([А-ЯёЁа-яA-Za-z][А-ЯёЁа-яA-Za-z0-9_]*)", RegexOptions.Compiled), "DocumentJournal",    RelationType.Uses),
    ];

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Analyse a BSL file and return all detected code-level relations.
    /// </summary>
    /// <param name="filePath">Absolute path of the .bsl file.</param>
    /// <param name="relativeFilePath">Relative path stored in <see cref="CodeRelation.SourcePath"/>.</param>
    /// <param name="knownCommonModules">Set of known CommonModule names for call resolution.</param>
    public List<CodeRelation> Analyze(
        string      filePath,
        string      relativeFilePath,
        HashSet<string> knownCommonModules)
    {
        string[] lines;
        try { lines = File.ReadAllLines(filePath); }
        catch { return []; }

        var results = new List<CodeRelation>();
        var lineNo  = 0;

        foreach (var rawLine in lines)
        {
            lineNo++;

            // Strip single-line comments (//)
            var line = StripComment(rawLine);
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 1. calls — CommonModule references
            foreach (Match m in RxCallModule.Matches(line))
            {
                var moduleName = m.Groups[1].Value;
                if (knownCommonModules.Contains(moduleName))
                {
                    results.Add(new CodeRelation
                    {
                        TargetKind   = "CommonModule",
                        TargetName   = moduleName,
                        RelationType = RelationType.Calls,
                        SourcePath   = relativeFilePath,
                        SourceLine   = lineNo,
                    });
                }
            }

            // 2. writes — Движения.<Register>
            foreach (Match m in RxMovements.Matches(line))
            {
                results.Add(new CodeRelation
                {
                    TargetKind   = "AccumulationRegister",
                    TargetName   = m.Groups[1].Value,
                    RelationType = RelationType.Writes,
                    SourcePath   = relativeFilePath,
                    SourceLine   = lineNo,
                });
            }

            // 3. writes — РегистрНакопления.<Name>.СоздатьНаборЗаписей
            foreach (Match m in RxRegWrite.Matches(line))
            {
                results.Add(new CodeRelation
                {
                    TargetKind   = "AccumulationRegister",
                    TargetName   = m.Groups[1].Value,
                    RelationType = RelationType.Writes,
                    SourcePath   = relativeFilePath,
                    SourceLine   = lineNo,
                });
            }

            // 4. reads / uses — object manager references
            foreach (var (pattern, kind, relType) in RxReads)
            {
                foreach (Match m in pattern.Matches(line))
                {
                    results.Add(new CodeRelation
                    {
                        TargetKind   = kind,
                        TargetName   = m.Groups[1].Value,
                        RelationType = relType,
                        SourcePath   = relativeFilePath,
                        SourceLine   = lineNo,
                    });
                }
            }
        }

        return results;
    }

    private static string StripComment(string line)
    {
        var idx = line.IndexOf("//", StringComparison.Ordinal);
        return idx >= 0 ? line[..idx] : line;
    }
}

/// <summary>
/// A code-level relation detected in a BSL file before object IDs are resolved.
/// </summary>
public class CodeRelation
{
    public string  TargetKind   { get; set; } = string.Empty;
    public string  TargetName   { get; set; } = string.Empty;
    public string  RelationType { get; set; } = string.Empty;
    public string  SourcePath   { get; set; } = string.Empty;
    public int     SourceLine   { get; set; }
    public string  Confidence   { get; set; } = RelationConfidence.Heuristic;
}

