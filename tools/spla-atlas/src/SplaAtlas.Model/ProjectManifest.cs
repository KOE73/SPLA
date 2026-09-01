using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>
/// <c>project.json</c> — the manifest. Read by the utility, written by a person.
/// </summary>
public sealed class ProjectManifest : JsonBacked
{
    private ProjectManifest(JsonObject node, JsonFormat format)
        : base(node) => Format = format;

    /// <summary>The formatting this file arrived in.</summary>
    public JsonFormat Format { get; }

    /// <summary>Matches the directory name.</summary>
    public string? Id
    {
        get => GetString("id");
        set => SetString("id", value);
    }

    public string? Title
    {
        get => GetString("title");
        set => SetString("title", value);
    }

    public string? Subtitle
    {
        get => GetString("subtitle");
        set => SetString("subtitle", value);
    }

    /// <summary>Contract version. 3 for everything currently on disk.</summary>
    public int? ContractVersion
    {
        get => GetInt("contractVersion");
        set => SetInt("contractVersion", value);
    }

    public string? DefaultView
    {
        get => GetString("defaultView");
        set => SetString("defaultView", value);
    }

    /// <summary>The axis a view inherits when it declares none of its own.</summary>
    public string? DefaultAxis
    {
        get => GetString("defaultAxis");
        set => SetString("defaultAxis", value);
    }

    /// <summary>Which text catalogs to load. The first is what the editor shows; no data privilege.</summary>
    public IReadOnlyList<string> Languages => GetStringArray("languages");

    /// <summary>Declared views. Nothing reads this today — views open through <c>catalog.json</c>.</summary>
    public IReadOnlyList<string> Views => GetStringArray("views");

    /// <summary>Relative path to the shared style sheet.</summary>
    public string? Styles
    {
        get => GetString("styles");
        set => SetString("styles", value);
    }

    /// <summary>
    /// Repository-relative roots the extractor should walk, from <c>sources.include</c>.
    /// </summary>
    /// <remarks>
    /// Empty for five of the six live projects, and that is not a defect: those projects were drawn
    /// by hand and never claimed to describe a slice of the tree. A <c>sync</c> against one of them
    /// has nothing to compare with, and must say so rather than fail or invent a root.
    /// </remarks>
    public IReadOnlyList<string> SourceIncludes =>
        GetObject("sources") is { } sources
            ? new SourcesView(sources).Include
            : [];

    /// <summary>Whether the manifest declares source roots at all.</summary>
    public bool HasSourceIncludes => SourceIncludes.Count > 0;

    public static ProjectManifest Parse(ReadOnlySpan<byte> bytes, string origin)
    {
        var parsed = JsonFile.Parse(bytes, origin);
        return new ProjectManifest(parsed.Root, parsed.Format);
    }

    public static ProjectManifest Read(string path)
    {
        var parsed = JsonFile.Read(path);
        return new ProjectManifest(parsed.Root, parsed.Format);
    }

    public byte[] Serialize() => JsonFile.Serialize(Node, Format);

    public void Write(string path) => JsonFile.Write(path, Node, Format);

    private sealed class SourcesView(JsonObject node) : JsonBacked(node)
    {
        public IReadOnlyList<string> Include => GetStringArray("include");
    }
}
