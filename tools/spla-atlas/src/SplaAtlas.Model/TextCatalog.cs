using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>Where a text value came from.</summary>
public enum TextOrigin
{
    /// <summary>Written by a person in this language.</summary>
    Authored,

    /// <summary>Translated from another language, and carrying which one and from what text.</summary>
    Translated,
}

/// <summary>
/// One translatable value, with whatever provenance it was stored with.
/// </summary>
/// <remarks>
/// <para>
/// Read-only by construction, and that is the enforcement of a rule rather than an oversight. The
/// utility translates nothing and authors nothing, so it is not entitled to stamp
/// <see cref="Origin"/>, <see cref="At"/>, <see cref="From"/> or <see cref="FromHash"/> on anything.
/// A value it did not write, it may not call <c>authored</c> — that would put a clean bill of health
/// on a ledger nobody kept. There is therefore no setter to misuse.
/// </para>
/// <para>
/// A v2 catalog stored bare strings. Those load with <see cref="HasProvenance"/> false and every
/// stamp null — not with an invented <c>authored</c>, for the same reason.
/// </para>
/// </remarks>
public sealed class TextValue
{
    private readonly JsonNode? _node;

    internal TextValue(JsonNode? node) => _node = node;

    /// <summary>The text itself: <c>v</c> in v3, the bare string in v2.</summary>
    public string? Value => _node switch
    {
        JsonObject o => o.TryGetPropertyValue("v", out var v) && v is JsonValue jv && jv.TryGetValue<string>(out var s)
            ? s
            : null,
        JsonValue value => value.TryGetValue<string>(out var s) ? s : null,
        _ => null,
    };

    /// <summary>Whether the value carries provenance at all. False for a v2 bare string.</summary>
    public bool HasProvenance => _node is JsonObject;

    /// <summary>ISO-8601 UTC with a <c>Z</c>. Informative only: the hash decides staleness, the date does not.</summary>
    public string? At => Field("at");

    /// <summary>Raw <c>origin</c> token as spelled on disk.</summary>
    public string? OriginToken => Field("origin");

    /// <summary>Parsed origin, or null when absent or unrecognised.</summary>
    public TextOrigin? Origin => OriginToken switch
    {
        "authored" => TextOrigin.Authored,
        "translated" => TextOrigin.Translated,
        _ => null,
    };

    /// <summary>Source language. Only on a translated value.</summary>
    public string? From => Field("from");

    /// <summary>Hash of the normalised source text at the time of translation.</summary>
    public string? FromHash => Field("fromHash");

    private string? Field(string name) =>
        _node is JsonObject o && o.TryGetPropertyValue(name, out var v) && v is JsonValue jv &&
        jv.TryGetValue<string>(out var s)
            ? s
            : null;
}

/// <summary>
/// The text of one named object: an entity, container, relation type, relation or view.
/// </summary>
/// <remarks>
/// Four fields are allowed. <c>description</c> is a line or two, always read; <c>doc</c> is the long
/// text — reasons, invariants, what breaks if they are violated — read on demand. Granularity is the
/// field: editing <c>name</c> does not make <c>doc</c> stale.
/// </remarks>
public sealed class TextEntry
{
    private readonly JsonObject _node;

    internal TextEntry(JsonObject node) => _node = node;

    public TextValue? Name => Get("name");

    public TextValue? Title => Get("title");

    public TextValue? Description => Get("description");

    public TextValue? Doc => Get("doc");

    /// <summary>Every field present on this entry, including any the contract does not define.</summary>
    public IReadOnlyList<string> FieldNames
    {
        get
        {
            var names = new List<string>();
            foreach (var (key, _) in _node)
            {
                names.Add(key);
            }

            return names;
        }
    }

    public TextValue? Get(string field) =>
        _node.TryGetPropertyValue(field, out var value) && value is not null ? new TextValue(value) : null;
}

/// <summary>
/// <c>text.&lt;lang&gt;.json</c> — every named thing's text, with provenance per field.
/// </summary>
/// <remarks>
/// <para>
/// The utility reads this file and never writes into it. <see cref="ModelDocument.Serialize"/> can
/// reproduce the bytes it read — that is what the round-trip test needs — but the model exposes no
/// way to change a value, add an entry or set a stamp, so there is no path by which a sync run could
/// touch a person's text.
/// </para>
/// <para>
/// There is no fallback between languages. A key present in the structure and absent here is a gap
/// to report, not a value to borrow from elsewhere: v2's fallback rule guaranteed that some text
/// stayed untranslated forever while always looking fine.
/// </para>
/// </remarks>
public sealed class TextCatalog : ModelDocument
{
    private TextCatalog(JsonObject node, JsonFormat format, string language)
        : base(node, format) => FileLanguage = language;

    /// <summary>The language taken from the file name, e.g. <c>ru</c> for <c>text.ru.json</c>.</summary>
    public string FileLanguage { get; }

    /// <summary>The language the file declares in its <c>language</c> field.</summary>
    public string? DeclaredLanguage => GetString("language");

    /// <summary>Keys of every entry, in document order.</summary>
    public IReadOnlyList<string> Keys
    {
        get
        {
            var keys = new List<string>();
            if (GetObject("entries") is not { } entries)
            {
                return keys;
            }

            foreach (var (key, _) in entries)
            {
                keys.Add(key);
            }

            return keys;
        }
    }

    public int Count => Keys.Count;

    /// <summary>The entry under <paramref name="key"/>, or null when the language has no text for it.</summary>
    public TextEntry? this[string key] =>
        GetObject("entries") is { } entries && entries.TryGetPropertyValue(key, out var value) &&
        value is JsonObject o
            ? new TextEntry(o)
            : null;

    public static TextCatalog Parse(ReadOnlySpan<byte> bytes, string origin, string language)
    {
        var parsed = JsonFile.Parse(bytes, origin);
        return new TextCatalog(parsed.Root, parsed.Format, language);
    }

    public static TextCatalog Read(string path)
    {
        var parsed = JsonFile.Read(path);
        return new TextCatalog(parsed.Root, parsed.Format, LanguageFromFileName(path));
    }

    /// <summary>Pulls <c>ru</c> out of <c>text.ru.json</c>.</summary>
    public static string LanguageFromFileName(string path)
    {
        var name = Path.GetFileName(path);
        const string prefix = "text.";
        const string suffix = ".json";
        return name.StartsWith(prefix, StringComparison.Ordinal) &&
               name.EndsWith(suffix, StringComparison.Ordinal) &&
               name.Length > prefix.Length + suffix.Length
            ? name[prefix.Length..^suffix.Length]
            : string.Empty;
    }
}
