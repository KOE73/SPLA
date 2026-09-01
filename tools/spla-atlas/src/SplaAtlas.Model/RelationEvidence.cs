using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>One entry of a relation's <c>evidence</c> array: where the relation can be checked.</summary>
public sealed class RelationEvidence(JsonObject node) : JsonBacked(node)
{
    /// <summary>Repository-relative path.</summary>
    public string? CodeRef
    {
        get => GetString("codeRef");
        set => SetString("codeRef", value);
    }

    /// <summary>Member name within that file, when the evidence is narrower than the file.</summary>
    public string? Symbol
    {
        get => GetString("symbol");
        set => SetString("symbol", value);
    }

    /// <summary>Line number. Defined by the contract; absent from all live data.</summary>
    public int? Line
    {
        get => GetInt("line");
        set => SetInt("line", value);
    }
}
