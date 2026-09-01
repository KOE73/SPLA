using System.Text.Json.Nodes;

namespace SplaAtlas.Model.Json;

/// <summary>
/// Base for every typed model object in this assembly. Each one is a window onto a
/// <see cref="JsonObject"/> that was parsed from disk, not a copy of it.
/// </summary>
/// <remarks>
/// <para>
/// Reading a property looks into the underlying node; writing one edits it in place. Two properties
/// the utility must not lose therefore need no code at all: a key nobody modelled is still in the
/// node, and the order the keys arrived in is the order they go back out. That order is not
/// cosmetic here — the six live manifests carry the same keys in four different orders, and any
/// writer with an opinion about ordering rewrites all six on its first run.
/// </para>
/// <para>
/// Assigning <c>null</c> to a string property removes the key rather than writing a JSON null. The
/// two are different statements in this contract — an absent <c>parent</c> and
/// <c>"parent": null</c> both occur in <c>containers.json</c> — so where the difference matters,
/// use <see cref="Remove"/> and <see cref="SetNull"/> explicitly.
/// </para>
/// </remarks>
public abstract class JsonBacked
{
    protected JsonBacked(JsonObject node) => Node = node;

    /// <summary>The underlying tree. Editing it directly is legitimate; that is where fidelity lives.</summary>
    public JsonObject Node { get; }

    /// <summary>Whether the key is present at all, regardless of its value.</summary>
    public bool Has(string name) => Node.ContainsKey(name);

    /// <summary>Whether the key is present and holds a JSON null.</summary>
    public bool IsNull(string name) => Node.TryGetPropertyValue(name, out var value) && value is null;

    /// <summary>Drops the key.</summary>
    public void Remove(string name) => Node.Remove(name);

    /// <summary>Writes an explicit JSON null.</summary>
    public void SetNull(string name) => Node[name] = null;

    protected string? GetString(string name) =>
        Node.TryGetPropertyValue(name, out var value) && value is JsonValue v && v.TryGetValue<string>(out var s)
            ? s
            : null;

    protected void SetString(string name, string? value)
    {
        if (value is null)
        {
            Node.Remove(name);
            return;
        }

        Node[name] = JsonValue.Create(value);
    }

    protected int? GetInt(string name) =>
        Node.TryGetPropertyValue(name, out var value) && value is JsonValue v && v.TryGetValue<int>(out var i)
            ? i
            : null;

    protected void SetInt(string name, int? value)
    {
        if (value is null)
        {
            Node.Remove(name);
            return;
        }

        Node[name] = JsonValue.Create(value.Value);
    }

    /// <summary>
    /// Reads an array of strings. A missing key and an empty array both read as empty — the
    /// difference is preserved in the node and visible through <see cref="Has"/>.
    /// </summary>
    protected IReadOnlyList<string> GetStringArray(string name)
    {
        if (!Node.TryGetPropertyValue(name, out var value) || value is not JsonArray array)
        {
            return [];
        }

        var result = new List<string>(array.Count);
        foreach (var item in array)
        {
            if (item is JsonValue v && v.TryGetValue<string>(out var s))
            {
                result.Add(s);
            }
        }

        return result;
    }

    /// <summary>Returns the object under <paramref name="name"/>, or null if absent or not an object.</summary>
    protected JsonObject? GetObject(string name) =>
        Node.TryGetPropertyValue(name, out var value) ? value as JsonObject : null;

    /// <summary>
    /// Returns the object under <paramref name="name"/>, creating it if it is absent.
    /// </summary>
    /// <remarks>
    /// Only for write paths. Calling this to answer a question would add the key to a document that
    /// did not have it, which is how a lossless codec quietly stops being one — see
    /// <see cref="JsonBackedList{T}"/> for the same hazard and the same rule.
    /// </remarks>
    protected JsonObject EnsureObject(string name)
    {
        if (Node.TryGetPropertyValue(name, out var value) && value is JsonObject existing)
        {
            return existing;
        }

        var created = new JsonObject();
        Node[name] = created;
        return created;
    }

    /// <summary>A typed, lazily materialised list view over an array-valued property.</summary>
    protected JsonBackedList<T> List<T>(string name, Func<JsonObject, T> wrap)
        where T : JsonBacked => new(Node, name, wrap);
}
