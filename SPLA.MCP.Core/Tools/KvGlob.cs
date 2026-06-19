using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Glob matching and value-filter helpers for KV list operations.
/// </summary>
internal static class KvGlob
{
    /// <summary>
    /// Matches <paramref name="text"/> against <paramref name="pattern"/>.
    /// glob mode (default): '*' matches any sequence; no '*' falls back to Contains.
    /// regex mode: full .NET regex; '.' is NOT auto-escaped — caller writes literal patterns.
    /// </summary>
    internal static bool KeyMatch(string text, string pattern, bool regexMode = false)
    {
        if (regexMode)
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);

        if (!pattern.Contains('*'))
            return text.Contains(pattern, StringComparison.OrdinalIgnoreCase);

        return Regex.IsMatch(text, GlobToRegex(pattern), RegexOptions.IgnoreCase);
    }

    private static string GlobToRegex(string glob)
    {
        var sb = new StringBuilder("^");
        foreach (char c in glob)
        {
            if (c == '*') sb.Append(".*");
            else if (c is '.' or '+' or '?' or '(' or ')' or '[' or ']' or '{' or '}' or '^' or '$' or '|' or '\\')
                sb.Append('\\').Append(c);
            else sb.Append(c);
        }
        sb.Append('$');
        return sb.ToString();
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
