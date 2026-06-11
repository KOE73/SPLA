namespace SPLA.Plugins.OneC.Indexing;

/// <summary>
/// Classifies what kind of content a file contains.
/// </summary>
public enum FileContentType
{
    Metadata,   // XML / .mdo — object definition
    Module,     // .bsl — BSL source code
    Form,       // Forms/<Name>/ subtree root
    Unknown,
}

/// <summary>
/// A file found during directory scan, annotated with its 1C metadata role.
/// </summary>
public class ScannedFile
{
    /// <summary>Absolute path to the file.</summary>
    public string AbsolutePath { get; set; } = string.Empty;

    /// <summary>Path relative to the configuration dump root.</summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>1C metadata kind inferred from directory position.</summary>
    public string ObjectKind { get; set; } = string.Empty;

    /// <summary>Object name inferred from directory name.</summary>
    public string ObjectName { get; set; } = string.Empty;

    /// <summary>Fully-qualified object name, e.g. «Document.РеализацияТоваров».</summary>
    public string ObjectFullName => $"{ObjectKind}.{ObjectName}";

    public FileContentType ContentType { get; set; }
}

/// <summary>
/// Walks a 1C configuration dump directory and classifies every relevant file.
/// 
/// Expected directory structure (standard 1C file-based configuration):
/// 
///   Documents/
///     РеализацияТоваров/
///       РеализацияТоваров.mdo          — metadata XML
///       Ext/
///         ObjectModule.bsl             — object module
///         ManagerModule.bsl            — manager module
///       Forms/
///         ФормаДокумента/
///           ФормаДокумента.mdo
///           Ext/Form/Module.bsl        — form module
///   Catalogs/
///   AccumulationRegisters/
///   InformationRegisters/
///   AccountingRegisters/
///   CommonModules/
///   Reports/
///   DataProcessors/
///   ChartsOfAccounts/
///   ChartsOfCalculationTypes/
///   ChartsOfCharacteristicTypes/
///   BusinessProcesses/
///   Tasks/
///   ExchangePlans/
///   Constants/
///   Sequences/
///   DocumentJournals/
///   Enums/
///   Subsystems/
/// </summary>
public class OneCFileScanner
{
    /// <summary>
    /// Maps top-level directory names (case-insensitive) to 1C metadata kind strings.
    /// </summary>
    private static readonly Dictionary<string, string> DirToKind = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Documents"]                    = "Document",
        ["Catalogs"]                     = "Catalog",
        ["AccumulationRegisters"]        = "AccumulationRegister",
        ["InformationRegisters"]         = "InformationRegister",
        ["AccountingRegisters"]          = "AccountingRegister",
        ["CalculationRegisters"]         = "CalculationRegister",
        ["CommonModules"]                = "CommonModule",
        ["Reports"]                      = "Report",
        ["DataProcessors"]               = "Processing",
        ["ChartsOfAccounts"]             = "ChartOfAccounts",
        ["ChartsOfCalculationTypes"]     = "ChartOfCalculationTypes",
        ["ChartsOfCharacteristicTypes"]  = "ChartOfCharacteristicTypes",
        ["BusinessProcesses"]            = "BusinessProcess",
        ["Tasks"]                        = "Task",
        ["ExchangePlans"]                = "ExchangePlan",
        ["Constants"]                    = "Constant",
        ["DocumentJournals"]             = "DocumentJournal",
        ["Enums"]                        = "Enum",
        ["Subsystems"]                   = "Subsystem",
        ["Sequences"]                    = "Sequence",
        ["ScheduledJobs"]                = "ScheduledJob",
        ["HTTPServices"]                 = "HTTPService",
        ["WebServices"]                  = "WebService",
        ["Roles"]                        = "Role",
        ["FilterCriteria"]               = "FilterCriterion",
        ["EventSubscriptions"]           = "EventSubscription",
        ["ExternalDataSources"]          = "ExternalDataSource",
    };

    private static readonly HashSet<string> MetadataExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".xml", ".mdo" };

    private static readonly HashSet<string> ModuleExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".bsl" };

    /// <summary>
    /// Scans the directory and returns all relevant files with their classification.
    /// Skips binary files, hidden directories, and .git folders.
    /// </summary>
    public List<ScannedFile> Scan(string rootPath)
    {
        var result = new List<ScannedFile>();
        var root   = new DirectoryInfo(rootPath);
        if (!root.Exists)
            throw new DirectoryNotFoundException($"Configuration dump not found: {rootPath}");

        // Top-level directories correspond to metadata kinds
        foreach (var topDir in root.EnumerateDirectories())
        {
            if (!DirToKind.TryGetValue(topDir.Name, out var kind))
                continue;

            // Second level: individual objects
            foreach (var objectDir in topDir.EnumerateDirectories())
            {
                var objectName = objectDir.Name;
                ScanObjectDirectory(objectDir, root.FullName, kind, objectName, result);
            }
        }

        return result;
    }

    private void ScanObjectDirectory(
        DirectoryInfo objectDir,
        string        rootPath,
        string        kind,
        string        objectName,
        List<ScannedFile> result)
    {
        // Recurse all files under this object directory
        foreach (var file in objectDir.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            var ext         = file.Extension;
            var relPath     = Path.GetRelativePath(rootPath, file.FullName);
            var contentType = FileContentType.Unknown;

            if (MetadataExtensions.Contains(ext))
                contentType = FileContentType.Metadata;
            else if (ModuleExtensions.Contains(ext))
                contentType = FileContentType.Module;
            else
                continue; // skip non-text / non-relevant files

            result.Add(new ScannedFile
            {
                AbsolutePath = file.FullName,
                RelativePath = relPath,
                ObjectKind   = kind,
                ObjectName   = objectName,
                ContentType  = contentType,
            });
        }
    }
}

