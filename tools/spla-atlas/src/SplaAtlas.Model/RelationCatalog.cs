using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>
/// <c>relations.json</c> — the catalog of relations. Shared ownership: the utility keeps the
/// <c>code</c> ones, a person keeps the <c>authored</c> ones, and neither writes the other's.
/// </summary>
public sealed class RelationCatalog : ModelDocument
{
    private RelationCatalog(JsonObject node, JsonFormat format)
        : base(node, format)
    {
    }

    public JsonBackedList<Relation> Relations => field ??= List("relations", n => new Relation(n));

    public static RelationCatalog Parse(ReadOnlySpan<byte> bytes, string origin)
    {
        var parsed = JsonFile.Parse(bytes, origin);
        return new RelationCatalog(parsed.Root, parsed.Format);
    }

    public static RelationCatalog Read(string path)
    {
        var parsed = JsonFile.Read(path);
        return new RelationCatalog(parsed.Root, parsed.Format);
    }

    public static RelationCatalog CreateEmpty(JsonFormat? format = null) =>
        new(new JsonObject { ["contractVersion"] = 3, ["relations"] = new JsonArray() },
            format ?? JsonFormat.Default);

    public Relation? ById(string id)
    {
        foreach (var relation in Relations)
        {
            if (relation.Id == id)
            {
                return relation;
            }
        }

        return null;
    }

    /// <summary>
    /// Builds the derived id for a relation between two entities.
    /// </summary>
    /// <remarks>
    /// From the entity ids, never from type names: an id survives a rename and a name does not, and
    /// deriving from names is precisely how v1 lost a layout every time a class was renamed. All call
    /// sites of A to B collapse into this one fact, which is also what keeps the id free of
    /// collisions by construction.
    /// </remarks>
    public static string DeriveId(string fromEntityId, string toEntityId, string type) =>
        $"r_{StripPrefix(fromEntityId)}_{StripPrefix(toEntityId)}_{type}";

    /// <summary>
    /// Rewrites the retired <c>manual</c> origin token as <c>authored</c>. Returns how many changed.
    /// </summary>
    public int MigrateLegacyOrigins()
    {
        var changed = 0;
        foreach (var relation in Relations)
        {
            if (!OriginToken.IsLegacy(relation.OriginToken))
            {
                continue;
            }

            relation.OriginToken = OriginToken.Authored;
            changed++;
        }

        return changed;
    }

    private static string StripPrefix(string entityId) =>
        entityId.StartsWith("e_", StringComparison.Ordinal) ? entityId[2..] : entityId;
}
