namespace SPLA.Plugins.OneC.Models;

/// <summary>
/// Directed relationship between two 1C configuration objects.
/// </summary>
public class OneCRelation
{
    public long Id { get; set; }

    public long FromObjectId { get; set; }
    public long ToObjectId { get; set; }

    /// <summary>
    /// Semantic type of the relation:
    /// owns | uses | calls | reads | writes | queries
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Relative path of the source file where the relation was found.</summary>
    public string? SourcePath { get; set; }

    /// <summary>1-based line number in the source file.</summary>
    public int? SourceLine { get; set; }

    /// <summary>
    /// How confident we are in this relation:
    /// exact     — determined from XML metadata structure
    /// heuristic — inferred by lexical analysis of BSL/query text
    /// llm       — suggested by a language model (not used in v1)
    /// </summary>
    public string Confidence { get; set; } = "heuristic";
}

/// <summary>All valid relation type strings.</summary>
public static class RelationType
{
    /// <summary>Object structurally contains another (form, table part, module).</summary>
    public const string Owns = "owns";

    /// <summary>Object references / mentions another object.</summary>
    public const string Uses = "uses";

    /// <summary>Module calls a procedure or function from a common module.</summary>
    public const string Calls = "calls";

    /// <summary>Object reads data from a register, catalog, or document.</summary>
    public const string Reads = "reads";

    /// <summary>Object writes / posts data to a register or other storage.</summary>
    public const string Writes = "writes";

    /// <summary>Object uses a table as a query data source.</summary>
    public const string Queries = "queries";
}

/// <summary>Confidence level for a detected relation.</summary>
public static class RelationConfidence
{
    public const string Exact     = "exact";
    public const string Heuristic = "heuristic";
    public const string Llm       = "llm";
}

