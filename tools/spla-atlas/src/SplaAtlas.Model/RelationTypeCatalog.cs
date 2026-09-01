using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>One record of <c>relation-types.json</c>.</summary>
/// <remarks>
/// The id stays the bare word — <c>call</c>, <c>data-flow</c> — rather than gaining an <c>rt_</c>
/// prefix, because <c>styles.json</c> matches a style by that exact string. The <c>rt_</c> prefix
/// belongs to the type's <em>text</em> key and nowhere else.
/// </remarks>
public sealed class RelationTypeDefinition(JsonObject node) : JsonBacked(node)
{
    public string? Id => GetString("id");

    public string? OriginToken
    {
        get => GetString("origin");
        set => SetString("origin", value);
    }

    public Origin? Origin => Model.OriginToken.Parse(OriginToken);

    /// <summary>Whether a person owns this row.</summary>
    public bool IsAuthored => Origin == Model.Origin.Authored;

    public string? StyleId
    {
        get => GetString("styleId");
        set => SetString("styleId", value);
    }

    /// <summary>Key under which this type's name and description live in a text catalog.</summary>
    public string? TextKey => Id is { } id ? $"rt_{id}" : null;
}

/// <summary>
/// <c>relation-types.json</c> — the dictionary of relation types.
/// </summary>
/// <remarks>
/// Mostly a person's file. The utility may add a structural type it emits (<c>extends</c>,
/// <c>implements</c>, <c>composes</c>) when the dictionary lacks it, and may do nothing else here:
/// it does not edit an existing row and it never removes one. A type it did not author is a
/// statement about what a kind of relation guarantees, and that is not the utility's to revise.
/// </remarks>
public sealed class RelationTypeCatalog : ModelDocument
{
    /// <summary>The types the extractor produces and may therefore register itself.</summary>
    public static readonly IReadOnlyList<string> StructuralTypes = ["extends", "implements", "composes"];

    private RelationTypeCatalog(JsonObject node, JsonFormat format)
        : base(node, format)
    {
    }

    public JsonBackedList<RelationTypeDefinition> RelationTypes =>
        field ??= List("relationTypes", n => new RelationTypeDefinition(n));

    public static RelationTypeCatalog Parse(ReadOnlySpan<byte> bytes, string origin)
    {
        var parsed = JsonFile.Parse(bytes, origin);
        return new RelationTypeCatalog(parsed.Root, parsed.Format);
    }

    public static RelationTypeCatalog Read(string path)
    {
        var parsed = JsonFile.Read(path);
        return new RelationTypeCatalog(parsed.Root, parsed.Format);
    }

    public static RelationTypeCatalog CreateEmpty(JsonFormat? format = null) =>
        new(new JsonObject { ["contractVersion"] = 3, ["relationTypes"] = new JsonArray() },
            format ?? JsonFormat.Default);

    public RelationTypeDefinition? ById(string id)
    {
        foreach (var type in RelationTypes)
        {
            if (type.Id == id)
            {
                return type;
            }
        }

        return null;
    }

    /// <summary>
    /// Registers a structural type if the dictionary does not already carry it. Returns the row, and
    /// whether it had to be created.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The type is not one the extractor produces. Flow types — <c>call</c>, <c>data-flow</c>,
    /// <c>event</c>, <c>security</c>, <c>storage</c> and whatever comes next — are a person's to
    /// define, and the utility asking for one here is a bug rather than a permission question.
    /// </exception>
    public (RelationTypeDefinition Type, bool Created) EnsureStructuralType(string id)
    {
        if (!StructuralTypes.Contains(id, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                $"'{id}' is not a structural type; only {string.Join(", ", StructuralTypes)} are the utility's to register.",
                nameof(id));
        }

        if (ById(id) is { } existing)
        {
            return (existing, false);
        }

        var created = RelationTypes.Add(new JsonObject
        {
            ["id"] = id,
            ["origin"] = OriginToken.Code,
        });

        return (created, true);
    }
}
