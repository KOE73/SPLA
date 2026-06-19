using System;
using System.Text.Json;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Glob matching and value-filter helpers for KV list operations.
/// </summary>
internal static class KvGlob
{
    /// <summary>
    /// Matches <paramref name="text"/> against <paramref name="pattern"/>.
    /// If pattern contains no '*', falls back to case-insensitive Contains (backward compat).
    /// '*' matches any sequence of characters including none.
    /// </summary>
    internal static bool KeyMatch(string text, string pattern)
    {
        if (!pattern.Contains('*'))
            return text.Contains(pattern, StringComparison.OrdinalIgnoreCase);

        var parts = pattern.Split('*');
        int pos = 0;

        for (int i = 0; i < parts.Length; i++)
        {
            var seg = parts[i];
            if (seg.Length == 0) continue;

            if (i == 0)
            {
                if (!text.StartsWith(seg, StringComparison.OrdinalIgnoreCase)) return false;
                pos = seg.Length;
            }
            else if (i == parts.Length - 1)
            {
                if (parts[^1].Length == 0) break; // trailing * — anything goes
                if (!text.EndsWith(seg, StringComparison.OrdinalIgnoreCase)) return false;
                if (text.Length - seg.Length < pos) return false;
            }
            else
            {
                int idx = text.IndexOf(seg, pos, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return false;
                pos = idx + seg.Length;
            }
        }
        return true;
    }

    /// <summary>
    /// Evaluates a single where clause against a KV value string.
    /// Syntax: "field=pattern"  — extracts field from JSON value, glob-matches against pattern.
    ///         "=pattern"       — matches the raw value string against pattern (no field extraction).
    /// Returns true if the clause matches, or if the clause is empty/unparseable.
    /// </summary>
    internal static bool WhereMatch(string value, string? where)
    {
        if (string.IsNullOrWhiteSpace(where)) return true;

        var eq = where.IndexOf('=');
        if (eq < 0) return true; // malformed — don't filter out

        var field   = where[..eq].Trim();
        var pattern = where[(eq + 1)..].Trim();

        string candidate;
        if (field.Length == 0)
        {
            candidate = value;
        }
        else
        {
            candidate = ExtractField(value, field) ?? string.Empty;
        }

        return KeyMatch(candidate, pattern);
    }

    private static string? ExtractField(string json, string field)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
            if (!doc.RootElement.TryGetProperty(field, out var prop)) return null;
            return prop.ValueKind == JsonValueKind.String
                ? prop.GetString()
                : prop.GetRawText();
        }
        catch
        {
            return null;
        }
    }
}
