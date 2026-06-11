namespace SPLA.Plugins.OneC.Models;

/// <summary>
/// Represents a single metadata object in a 1C configuration
/// (e.g. Document.РеализацияТоваров, Catalog.Номенклатура).
/// </summary>
public class OneCObject
{
    public long Id { get; set; }

    /// <summary>
    /// Metadata type: Document, Catalog, AccumulationRegister,
    /// InformationRegister, CommonModule, Form, Report, Processing, etc.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>Short name, e.g. «РеализацияТоваров».</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Fully-qualified name, e.g. «Document.РеализацияТоваров».</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Relative path inside the configuration dump directory.</summary>
    public string? Path { get; set; }

    /// <summary>Human-readable summary extracted from Synonym metadata field.</summary>
    public string? Summary { get; set; }
}

