using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using SPLA.Library.Catalog;
using SPLA.Library.Sources;

namespace SPLA.Library.Format;

/// <summary>
/// Splits a skill document into its YAML frontmatter (metadata) and its body (the procedure).
///
/// <para>Deliberately NOT part of <see cref="ISkillSource"/>: a source only knows how to fetch text
/// by ref. Keeping the format knowledge here means a future source serving a different kind of
/// prompt asset reuses the same source implementation with a different parser on top.</para>
/// </summary>
public static class SkillFrontmatter
{
    private static readonly IDeserializer Yaml = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>Frontmatter fields, all optional. Shape mirrors the documented skill file format.</summary>
    private sealed class Raw
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public bool? Enabled { get; set; }
        public List<string>? Tags { get; set; }
        public RawRequirements? Requires { get; set; }
        public RawRequirements? Uses { get; set; }
    }

    private sealed class RawRequirements
    {
        public List<string>? Tools { get; set; }
        public List<string>? Features { get; set; }
    }

    /// <summary>
    /// Parses <paramref name="text"/> into an entry. <paramref name="fallbackId"/> is used when the
    /// document declares no id — a bare .md file dropped into a skills folder is a valid skill.
    /// Malformed YAML is never fatal: the skill degrades to id + empty description rather than
    /// vanishing, because a silently missing skill is far harder to diagnose than a described one.
    /// </summary>
    public static SkillEntry Parse(string text, string fallbackId, string skillRef)
    {
        Raw? raw = null;
        if (TrySplit(text, out var header, out _))
        {
            try { raw = Yaml.Deserialize<Raw>(header); }
            catch (YamlDotNet.Core.YamlException) { raw = ParseLeniently(header); }
        }

        return new SkillEntry(
            Id: Trimmed(raw?.Id) ?? fallbackId,
            Description: Trimmed(raw?.Description) ?? string.Empty,
            Ref: skillRef,
            Requires: ToRequirements(raw?.Requires),
            Uses: ToRequirements(raw?.Uses),
            DefaultEnabled: raw?.Enabled ?? true,
            // Normalised here, at the only door tags come through, so nothing downstream ever has to
            // wonder whether it is holding "SSH" or "ssh".
            Tags: SkillTag.NormalizeAll(raw?.Tags));
    }

    /// <summary>The procedure text — everything after the frontmatter block, or the whole document
    /// when there is none.</summary>
    public static string StripHeader(string text)
        => TrySplit(text, out _, out var body) ? body : text;

    /// <summary>
    /// Splits on a leading "---" block. Only a line that is exactly "---" closes it, so a "---"
    /// appearing inside a description value does not truncate the header — the previous hand-rolled
    /// IndexOf("---") parser got that wrong.
    /// </summary>
    private static bool TrySplit(string text, out string header, out string body)
    {
        header = string.Empty;
        body = text;

        if (!text.StartsWith("---", StringComparison.Ordinal)) return false;

        var firstBreak = text.IndexOf('\n');
        if (firstBreak < 0) return false;
        if (text[3..firstBreak].Trim().Length != 0) return false;   // "---extra" is not a delimiter

        var cursor = firstBreak + 1;
        var headerStart = cursor;

        while (cursor < text.Length)
        {
            var lineEnd = text.IndexOf('\n', cursor);
            var hasBreak = lineEnd >= 0;
            if (!hasBreak) lineEnd = text.Length;

            if (text[cursor..lineEnd].Trim() == "---")
            {
                header = text[headerStart..cursor];
                body = hasBreak ? text[(lineEnd + 1)..] : string.Empty;
                return true;
            }

            cursor = lineEnd + 1;
        }

        return false;   // unterminated header — treat the whole document as body
    }

    /// <summary>
    /// Salvage pass for a header that is not valid YAML. The common cause is an unquoted description
    /// containing ": " — "Trigger on: DNS not resolving" is how most existing skills are written, and
    /// YAML reads that as a nested mapping and gives up on the whole document.
    ///
    /// <para>Losing the id and description over a colon would make a skill silently disappear, so the
    /// scalar fields are recovered by hand: first colon splits, the rest of the line is the value.
    /// Requirements are NOT recovered — they are structured, and guessing at their shape would be
    /// worse than treating the skill as requirement-free and letting the author fix the quoting.</para>
    /// </summary>
    private static Raw ParseLeniently(string header)
    {
        var raw = new Raw();

        foreach (var line in header.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (trimmed.Length == 0 || char.IsWhiteSpace(trimmed[0]) || trimmed[0] == '#') continue;

            var colon = trimmed.IndexOf(':');
            if (colon <= 0) continue;

            var key = trimmed[..colon].Trim();
            var value = trimmed[(colon + 1)..].Trim().Trim('"', '\'');

            switch (key.ToLowerInvariant())
            {
                case "id": raw.Id = value; break;
                case "description": raw.Description = value; break;
                case "enabled" when bool.TryParse(value, out var e): raw.Enabled = e; break;
            }
        }

        return raw;
    }

    private static SkillRequirements ToRequirements(RawRequirements? raw)
    {
        if (raw is null) return SkillRequirements.None;

        var tools = Clean(raw.Tools);
        var features = Clean(raw.Features);
        return tools.Count == 0 && features.Count == 0
            ? SkillRequirements.None
            : new SkillRequirements(tools, features);
    }

    private static IReadOnlyList<string> Clean(List<string>? values) =>
        values is null
            ? []
            : values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToList();

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
