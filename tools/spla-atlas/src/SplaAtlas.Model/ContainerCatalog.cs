using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>The <c>match</c> block of a container: the rules that draw entities into it.</summary>
public sealed class ContainerMatch(JsonObject node) : JsonBacked(node)
{
    /// <summary>Exact type names.</summary>
    public IReadOnlyList<string> Names => GetStringArray("name");

    /// <summary>Regular expressions over the type name.</summary>
    public IReadOnlyList<string> NameRegexes => GetStringArray("nameRegex");

    /// <summary>Repository-relative path prefixes.</summary>
    public IReadOnlyList<string> Paths => GetStringArray("path");

    /// <summary>Whether this block states any rule at all.</summary>
    public bool IsEmpty => Names.Count == 0 && NameRegexes.Count == 0 && Paths.Count == 0;
}

/// <summary>One record of <c>containers.json</c>.</summary>
public sealed class Container(JsonObject node) : JsonBacked(node)
{
    /// <summary>Prefixed <c>c_</c>. The container's name lives in a text catalog under this same key.</summary>
    public string? Id => GetString("id");

    /// <summary>Enclosing container, or null at the top. Both an absent key and an explicit null occur.</summary>
    public string? Parent => GetString("parent");

    /// <summary>Whether the record says <c>"parent": null</c> rather than omitting the key.</summary>
    public bool HasExplicitNullParent => IsNull("parent");

    public string? Theme
    {
        get => GetString("theme");
        set => SetString("theme", value);
    }

    /// <summary>
    /// Which axis this container belongs to. Not part of the contract — the axis is declared by a
    /// view — but <c>spla_system</c> writes it as a note to the reader, and 27 records carry it.
    /// Nothing reads it, and nothing should start to without an ADR saying so.
    /// </summary>
    public string? Axis => GetString("axis");

    /// <summary>The autofill rules, or null when the container is populated only by overrides.</summary>
    public ContainerMatch? Match => GetObject("match") is { } m ? new ContainerMatch(m) : null;
}

/// <summary>
/// <c>containers.json</c> — membership, not geometry. A person's file; the utility only reads it.
/// </summary>
/// <remarks>
/// <para>
/// The utility applies <see cref="Container.Match"/> in the contract's order — name, then nameRegex,
/// then path by longest prefix, then file neighbour — respects <see cref="Overrides"/> above all of
/// it, and reports rules that caught nothing. It places nothing: membership is a hint for the base
/// panel and the report, and has no bearing on any view's geometry.
/// </para>
/// <para>
/// Membership need not be unique. One entity in several containers is ordinary — on different axes
/// it belongs in different frames — and is not a finding.
/// </para>
/// <para>
/// The file is absent from four of the six live projects, and that is normal rather than missing.
/// </para>
/// </remarks>
public sealed class ContainerCatalog : ModelDocument
{
    private ContainerCatalog(JsonObject node, JsonFormat format)
        : base(node, format)
    {
    }

    public JsonBackedList<Container> Containers => field ??= List("containers", n => new Container(n));

    /// <summary>
    /// Explicit entity-to-container assignments made by a person. Always beat a rule.
    /// </summary>
    public IReadOnlyDictionary<string, string> Overrides
    {
        get
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (GetObject("overrides") is not { } node)
            {
                return result;
            }

            foreach (var (key, value) in node)
            {
                if (value is JsonValue v && v.TryGetValue<string>(out var container))
                {
                    result[key] = container;
                }
            }

            return result;
        }
    }

    public static ContainerCatalog Parse(ReadOnlySpan<byte> bytes, string origin)
    {
        var parsed = JsonFile.Parse(bytes, origin);
        return new ContainerCatalog(parsed.Root, parsed.Format);
    }

    public static ContainerCatalog Read(string path)
    {
        var parsed = JsonFile.Read(path);
        return new ContainerCatalog(parsed.Root, parsed.Format);
    }

    public static ContainerCatalog CreateEmpty(JsonFormat? format = null) =>
        new(new JsonObject { ["contractVersion"] = 3, ["containers"] = new JsonArray() },
            format ?? JsonFormat.Default);

    public Container? ById(string id)
    {
        foreach (var container in Containers)
        {
            if (container.Id == id)
            {
                return container;
            }
        }

        return null;
    }
}
