using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>
/// A whole model file: its parsed tree plus the formatting it arrived in.
/// </summary>
/// <remarks>
/// <see cref="Serialize"/> on an untouched document reproduces the input bytes exactly. That is the
/// property the whole assembly is built around, and the one the round-trip test over the six live
/// projects exists to keep honest — it is what catches a lost provenance stamp, a reordered key, an
/// eaten optional field, or an <c>authored</c> quietly turned into <c>code</c>.
/// </remarks>
public abstract class ModelDocument : JsonBacked
{
    protected ModelDocument(JsonObject node, JsonFormat format)
        : base(node) => Format = format;

    /// <summary>The formatting this file arrived in, reproduced on write.</summary>
    public JsonFormat Format { get; }

    /// <summary>
    /// Declared contract version. Absent from several live files, which is legal: the version is
    /// carried by the project, and a catalog that omits it is not thereby a different format.
    /// </summary>
    public int? ContractVersion
    {
        get => GetInt("contractVersion");
        set => SetInt("contractVersion", value);
    }

    /// <summary>Renders the document back to bytes.</summary>
    public byte[] Serialize() => JsonFile.Serialize(Node, Format);

    /// <summary>Writes the document to disk.</summary>
    public void Write(string path) => JsonFile.Write(path, Node, Format);
}
