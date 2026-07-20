using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace SPLA.Domain.Settings;

/// <summary>
/// Replaces one top-level section of a YAML document as a TEXT operation, leaving the rest of the
/// file — comments, blank lines, key order, formatting — untouched. YamlDotNet cannot round-trip
/// comments through the object model, so instead of re-serializing the whole file we locate the
/// section's line span with parser marks and splice in newly serialized text. Comments directly
/// above the replaced key survive (the span starts at the key line); comments between the section's
/// end and the next key survive too.
/// Limitations (fine for SPLA configs): the root must be a block mapping; anchors/aliases and
/// multi-document streams are unsupported — parsing throws and callers fall back to a full rewrite.
/// </summary>
public static class YamlSectionSplicer
{
    /// <summary>
    /// Returns <paramref name="yamlText"/> with the top-level <paramref name="key"/> section replaced
    /// by <paramref name="newSectionText"/> (a complete "key:\n  …" block, or null to remove the
    /// section). A missing key is appended at the end of the document. Throws on YAML the parser
    /// can't handle — the caller decides whether to fall back to a full rewrite.
    /// </summary>
    public static string ReplaceSection(string yamlText, string key, string? newSectionText)
    {
        var span = FindSection(yamlText, key);
        var newline = yamlText.Contains("\r\n") ? "\r\n" : "\n";
        var lines = SplitLines(yamlText);

        var replacement = newSectionText == null
            ? []
            : SplitLines(newSectionText.Replace("\r\n", "\n").Replace("\n", newline).TrimEnd('\r', '\n') + newline);

        List<string> result;
        if (span == null)
        {
            if (newSectionText == null) return yamlText; // nothing to remove
            result = [.. lines];
            // Ensure the existing text ends with a newline before appending a new section.
            if (result.Count > 0 && !result[^1].EndsWith('\n'))
                result[^1] += newline;
            result.AddRange(replacement);
        }
        else
        {
            var (startLine, endLine) = span.Value; // 1-based, inclusive
            result = [.. lines[..(startLine - 1)], .. replacement, .. lines[endLine..]];
        }

        return string.Concat(result);
    }

    /// <summary>Splits text into lines, each keeping its own trailing newline.</summary>
    private static List<string> SplitLines(string text)
    {
        var lines = new List<string>();
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '\n') continue;
            lines.Add(text[start..(i + 1)]);
            start = i + 1;
        }
        if (start < text.Length) lines.Add(text[start..]);
        return lines;
    }

    /// <summary>
    /// Finds the 1-based inclusive line span of a top-level mapping entry (key line through the last
    /// line of its value), or null when the key is absent. Uses parser marks, so block scalars,
    /// nested mappings and flow collections are all handled by the parser itself.
    /// </summary>
    private static (int Start, int End)? FindSection(string yamlText, string key)
    {
        using var reader = new StringReader(yamlText);
        var parser = new Parser(reader);

        parser.Consume<StreamStart>();
        if (!parser.TryConsume<DocumentStart>(out _)) return null;   // empty document
        if (!parser.TryConsume<MappingStart>(out _))
            throw new YamlException("root is not a mapping");

        while (!parser.TryConsume<MappingEnd>(out _))
        {
            var keyScalar = parser.Consume<Scalar>();
            var startLine = (int)keyScalar.Start.Line;
            var endLine = startLine;

            // Consume the value node, tracking the last line any of its events touches.
            var depth = 0;
            do
            {
                var ev = parser.Consume<ParsingEvent>();
                if (ev is MappingStart or SequenceStart) depth++;
                else if (ev is MappingEnd or SequenceEnd) depth--;

                // Zero-width events (block MappingEnd/SequenceEnd, implicit starts) carry the mark of
                // whatever token FOLLOWS the section — often the next key, past any comments above it.
                // Only events with real extent tell us where the value's own text ends.
                if (ev.End.Index <= ev.Start.Index) continue;

                // An End mark at column 1 points at the start of the NEXT line (typical for block
                // scalars) — the value itself ends on the previous line.
                var evEnd = ev.End.Column <= 1 ? (int)ev.End.Line - 1 : (int)ev.End.Line;
                if (evEnd > endLine) endLine = evEnd;
            } while (depth > 0);

            if (string.Equals(keyScalar.Value, key, StringComparison.Ordinal))
                return (startLine, endLine);
        }

        return null;
    }
}
