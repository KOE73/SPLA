using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SPLA.MCP.Core.Json;

/// <summary>
/// Centralised helpers for reading tool arguments from a <see cref="JsonElement"/>.
/// All methods are null-safe: a missing field and a JSON-null field both return the
/// method's "absent" result (null / defaultValue). Tools never need to check ValueKind
/// manually for the common scalar cases.
/// See agents/tool-args.md for usage conventions and patterns.
/// </summary>
public static class ToolJson
{
    // ── pass-through ─────────────────────────────────────────────────────────

    /// <summary>Thin wrapper kept for call-sites that need the raw JsonElement.</summary>
    public static bool TryGetProperty(JsonElement root, string name, out JsonElement value)
        => root.TryGetProperty(name, out value);

    // ── string ────────────────────────────────────────────────────────────────
    #region String

    /// <summary>
    /// Returns the string value of <paramref name="name"/>, or null if the property is
    /// absent or JSON-null.
    /// </summary>
    public static string? GetString(JsonElement root, string name)
        => root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    /// <summary>
    /// Same as <see cref="GetString"/> but calls <see cref="string.Trim()"/> on the result.
    /// Returns null when the trimmed value is empty.
    /// Use for host names, paths, identifiers — anywhere leading/trailing whitespace is noise.
    /// </summary>
    public static string? GetStringTrimmed(JsonElement root, string name)
    {
        var s = GetString(root, name)?.Trim();
        return string.IsNullOrEmpty(s) ? null : s;
    }

    #endregion

    // ── integer ───────────────────────────────────────────────────────────────
    #region Integer

    /// <summary>
    /// Returns the integer value of <paramref name="name"/>, or null if the property is
    /// absent, JSON-null, or not a number.
    /// </summary>
    public static int? GetInt32(JsonElement root, string name)
        => root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number
                                                && v.TryGetInt32(out var i)
            ? i
            : null;

    /// <summary>
    /// Returns the integer value of <paramref name="name"/>, or <paramref name="defaultValue"/>
    /// if absent, JSON-null, or not a number.
    /// </summary>
    public static int GetInt32(JsonElement root, string name, int defaultValue)
        => GetInt32(root, name) ?? defaultValue;

    /// <summary>
    /// Returns the integer value of <paramref name="name"/> clamped to
    /// [<paramref name="min"/>, <paramref name="max"/>], or <paramref name="defaultValue"/>
    /// if absent, JSON-null, or not a number.
    /// Use for timeouts, counts, ports — any bounded numeric parameter.
    /// </summary>
    public static int GetInt32Clamped(JsonElement root, string name, int defaultValue, int min, int max)
        => GetInt32(root, name) is { } v ? Math.Clamp(v, min, max) : defaultValue;

    #endregion

    // ── boolean ───────────────────────────────────────────────────────────────
    #region Boolean

    /// <summary>
    /// Returns the boolean value of <paramref name="name"/>, or <paramref name="defaultValue"/>
    /// if absent, JSON-null, or not a boolean token.
    /// </summary>
    public static bool GetBoolean(JsonElement root, string name, bool defaultValue)
        => root.TryGetProperty(name, out var v) && v.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? v.GetBoolean()
            : defaultValue;

    #endregion

    // ── array ─────────────────────────────────────────────────────────────────
    #region Array

    /// <summary>
    /// Returns all non-null string elements of the JSON array at <paramref name="name"/>,
    /// or null if the property is absent, JSON-null, or not an array.
    /// Empty array → returns an empty (non-null) array.
    /// </summary>
    public static string[]? GetStringArray(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var v) || v.ValueKind != JsonValueKind.Array)
            return null;

        var result = new List<string>(v.GetArrayLength());
        foreach (var el in v.EnumerateArray())
            if (el.ValueKind == JsonValueKind.String && el.GetString() is { } s)
                result.Add(s);
        return result.ToArray();
    }

    #endregion

    // ── object / dictionary ───────────────────────────────────────────────────
    #region Dictionary

    /// <summary>
    /// Returns a flat string→string dictionary from a JSON object at <paramref name="name"/>.
    /// Only properties whose values are JSON strings are included; others are silently skipped.
    /// Returns null if the property is absent, JSON-null, or not an object.
    /// Use for HTTP headers, metadata maps, and similar key/value payloads.
    /// </summary>
    public static Dictionary<string, string>? GetStringDictionary(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var v) || v.ValueKind != JsonValueKind.Object)
            return null;

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in v.EnumerateObject())
            if (prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() is { } s)
                result[prop.Name] = s;
        return result;
    }

    #endregion
}
