using System.Collections;
using System.Text.Json.Nodes;

namespace SplaAtlas.Model.Json;

/// <summary>
/// A typed list view over an array-valued property of a <see cref="JsonObject"/>.
/// </summary>
/// <remarks>
/// <para>
/// The array is resolved on demand and <em>created only when something is added to it</em>. Reading
/// an absent list yields an empty one and leaves the document untouched. That is not a nicety: five
/// live entities carry no <c>members</c> key at all, and a view that materialised the array just to
/// answer "how many?" would write <c>"members": []</c> into each of them — the codec would fail its
/// own losslessness by being asked a question.
/// </para>
/// <para>
/// Wrappers are cached per array slot, so the same element read twice is the same instance and edits
/// through one are visible through the other. Elements that are not JSON objects are skipped rather
/// than rejected: a malformed element belongs in the report, not in an exception.
/// </para>
/// </remarks>
public sealed class JsonBackedList<T> : IReadOnlyList<T>
    where T : JsonBacked
{
    private readonly JsonObject _owner;
    private readonly string _property;
    private readonly Func<JsonObject, T> _wrap;
    private readonly Dictionary<JsonObject, T> _cache = [];

    internal JsonBackedList(JsonObject owner, string property, Func<JsonObject, T> wrap)
    {
        _owner = owner;
        _property = property;
        _wrap = wrap;
    }

    /// <summary>Whether the underlying property exists on the document.</summary>
    public bool IsMaterialised => Resolve() is not null;

    public int Count
    {
        get
        {
            if (Resolve() is not { } array)
            {
                return 0;
            }

            var count = 0;
            foreach (var item in array)
            {
                if (item is JsonObject)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public T this[int index]
    {
        get
        {
            if (Resolve() is { } array)
            {
                var seen = 0;
                foreach (var item in array)
                {
                    if (item is not JsonObject o)
                    {
                        continue;
                    }

                    if (seen++ == index)
                    {
                        return Wrap(o);
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    /// <summary>Appends a new empty object, creating the array if this is the first element.</summary>
    public T Add() => Add(new JsonObject());

    /// <summary>Appends an already-built object, creating the array if this is the first element.</summary>
    public T Add(JsonObject node)
    {
        Materialise().Add(node);
        return Wrap(node);
    }

    /// <summary>Removes an item from the underlying array. Returns whether it was there.</summary>
    public bool Remove(T item)
    {
        if (Resolve() is not { } array)
        {
            return false;
        }

        for (var i = 0; i < array.Count; i++)
        {
            if (!ReferenceEquals(array[i], item.Node))
            {
                continue;
            }

            array.RemoveAt(i);
            _cache.Remove(item.Node);
            return true;
        }

        return false;
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (Resolve() is not { } array)
        {
            yield break;
        }

        foreach (var item in array)
        {
            if (item is JsonObject o)
            {
                yield return Wrap(o);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private JsonArray? Resolve() =>
        _owner.TryGetPropertyValue(_property, out var value) ? value as JsonArray : null;

    private JsonArray Materialise()
    {
        if (Resolve() is { } existing)
        {
            return existing;
        }

        var created = new JsonArray();
        _owner[_property] = created;
        return created;
    }

    private T Wrap(JsonObject node)
    {
        if (_cache.TryGetValue(node, out var existing))
        {
            return existing;
        }

        var created = _wrap(node);
        _cache[node] = created;
        return created;
    }
}
