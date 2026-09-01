using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>
/// <c>entities.json</c> — the durable catalog of everything that can become a node on a view.
/// </summary>
/// <remarks>
/// Durable is the operative word: the utility reconciles this file, it does not regenerate it. A
/// type that leaves the code keeps its record and its id, because texts, relations and placements
/// all point at that id and would otherwise break silently.
/// </remarks>
public sealed class EntityCatalog : ModelDocument
{
    private EntityCatalog(JsonObject node, JsonFormat format)
        : base(node, format)
    {
    }

    public JsonBackedList<Entity> Entities => field ??= List("entities", n => new Entity(n));

    public static EntityCatalog Parse(ReadOnlySpan<byte> bytes, string origin)
    {
        var parsed = JsonFile.Parse(bytes, origin);
        return new EntityCatalog(parsed.Root, parsed.Format);
    }

    public static EntityCatalog Read(string path)
    {
        var parsed = JsonFile.Read(path);
        return new EntityCatalog(parsed.Root, parsed.Format);
    }

    /// <summary>An empty catalog, for a project that has none yet.</summary>
    public static EntityCatalog CreateEmpty(JsonFormat? format = null) =>
        new(new JsonObject { ["entities"] = new JsonArray() }, format ?? JsonFormat.Default);

    /// <summary>First entity with the given id, or null.</summary>
    public Entity? ById(string id)
    {
        foreach (var entity in Entities)
        {
            if (entity.Id == id)
            {
                return entity;
            }
        }

        return null;
    }

    /// <summary>
    /// Ids that appear on more than one record.
    /// </summary>
    /// <remarks>
    /// A <c>сломано</c> finding: the id is the only handle every other file has on an entity, so a
    /// duplicate makes at least one of those references mean two things at once.
    /// </remarks>
    public IReadOnlyList<string> DuplicateIds()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var duplicates = new List<string>();
        foreach (var entity in Entities)
        {
            if (entity.Id is { } id && !seen.Add(id) && !duplicates.Contains(id, StringComparer.Ordinal))
            {
                duplicates.Add(id);
            }
        }

        return duplicates;
    }

    /// <summary>
    /// Rewrites the retired <c>manual</c> origin token as <c>authored</c>. Returns how many changed.
    /// </summary>
    /// <remarks>
    /// Deliberately not part of reading. If merely opening a project rewrote these, the v2-to-v3
    /// rename would land scattered across whichever unrelated runs happened to touch each file, and
    /// every <c>--dry-run</c> would report a change it did not come to make.
    /// </remarks>
    public int MigrateLegacyOrigins()
    {
        var changed = 0;
        foreach (var entity in Entities)
        {
            if (!OriginToken.IsLegacy(entity.OriginToken))
            {
                continue;
            }

            entity.OriginToken = OriginToken.Authored;
            changed++;
        }

        return changed;
    }
}
