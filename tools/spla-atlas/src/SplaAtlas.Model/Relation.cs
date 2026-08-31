using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>One record of <c>relations.json</c>.</summary>
/// <remarks>
/// There is no <c>label</c> here and there must never be one again: v3 moved every relation text
/// into the text catalogs under the relation's own id, without a fallback, so that a relation nobody
/// has described renders unlabelled and the gap stays visible.
/// </remarks>
public sealed class Relation(JsonObject node) : JsonBacked(node)
{
    /// <summary>
    /// Prefixed <c>r_</c>. A derived relation is <c>r_{from}_{to}_{type}</c> with the <c>e_</c>
    /// prefixes stripped; a hand-written one carries an id its author chose.
    /// </summary>
    public string? Id => GetString("id");

    /// <summary>Id of the source entity.</summary>
    public string? From
    {
        get => GetString("from");
        set => SetString("from", value);
    }

    /// <summary>Id of the target entity.</summary>
    public string? To
    {
        get => GetString("to");
        set => SetString("to", value);
    }

    /// <summary>Id of a record in <c>relation-types.json</c>.</summary>
    public string? Type
    {
        get => GetString("type");
        set => SetString("type", value);
    }

    /// <summary>Raw <c>origin</c> token, as spelled on disk. 21 live records still say <c>manual</c>.</summary>
    public string? OriginToken
    {
        get => GetString("origin");
        set => SetString("origin", value);
    }

    /// <summary>Parsed origin, or null if the token is one the contract does not define.</summary>
    public Origin? Origin => Model.OriginToken.Parse(OriginToken);

    /// <summary>Whether a person keeps this record. The utility must leave those entirely alone.</summary>
    public bool IsAuthored => Origin == Model.Origin.Authored;

    public string? Status
    {
        get => GetString("status");
        set => SetString("status", value);
    }

    /// <summary>
    /// Checkable grounds for the relation. When a file or symbol named here is gone, that is a
    /// <c>лишнее</c> finding — the point of recording evidence is that logical relations rot audibly.
    /// </summary>
    public JsonBackedList<RelationEvidence> Evidence =>
        field ??= List("evidence", n => new RelationEvidence(n));

    /// <summary>
    /// A duplicate of <see cref="Type"/> left behind by the v1 layout tool, on 3083 live records.
    /// </summary>
    /// <remarks>
    /// Modelled so it survives a rewrite untouched, and named plainly so nobody mistakes it for part
    /// of the contract. Whether to drop it is the owner's call, not a side effect of a sync run.
    /// </remarks>
    public string? LegacyRelationField
    {
        get => GetString("relation");
        set => SetString("relation", value);
    }

    /// <summary>
    /// Edge geometry left behind by the v1 layout tool, on 25 live records — always empty.
    /// </summary>
    /// <remarks>
    /// Geometry has no business in a registry; under v3 it lives in views, which the utility never
    /// writes. Preserved here for the same reason as <see cref="LegacyRelationField"/>: removing it
    /// is a decision, not a cleanup this codec is entitled to make on its own.
    /// </remarks>
    public bool HasLegacyPoints => Has("points");
}
