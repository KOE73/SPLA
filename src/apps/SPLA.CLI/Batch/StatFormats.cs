using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SPLA.CLI.Batch;

/// <summary>One way of writing a <see cref="RunStats"/> out. A format is an id, a file extension and
/// a renderer over the section list — nothing else, so adding one is adding a line to
/// <see cref="StatFormats.All"/> and never touching what collects the numbers.</summary>
public sealed record StatFormat(string Id, string Extension, Func<RunStats, string> Render);

public static class StatFormats
{
    public static readonly IReadOnlyList<StatFormat> All =
    [
        new("json", ".json", Json),
        new("yaml", ".yaml", Yaml),
        new("md",   ".md",   Markdown)
    ];

    public static StatFormat? Find(string id) =>
        All.FirstOrDefault(f => f.Id.Equals(id.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string Names => string.Join(", ", All.Select(f => f.Id));

    /// <summary>Parses the comma-separated <c>--show-statistic-format</c> list, keeping the caller's
    /// order and refusing unknown ids by name — silently skipping one would produce a run that looks
    /// like it reported and did not.</summary>
    public static bool TryParse(string? spec, out List<StatFormat> formats, out string? error)
    {
        formats = [];
        error = null;
        if (string.IsNullOrWhiteSpace(spec)) { formats = [All[0]]; return true; }

        foreach (var id in spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Find(id) is not { } format) { error = $"unknown statistic format '{id}' — known: {Names}"; return false; }
            if (!formats.Contains(format)) formats.Add(format);
        }
        return true;
    }

    private static string Json(RunStats stats)
    {
        var root = new JsonObject();
        foreach (var section in stats.ToSections())
        {
            var node = new JsonObject();
            foreach (var (key, value) in section.Items) node[key] = ToJsonValue(value);
            root[section.Title] = node;
        }
        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>Boxes a reported value as its concrete JSON type. Handing <see cref="JsonValue.Create{T}"/>
    /// a plain <c>object</c> instead defers the decision to serialization time, where it needs a type
    /// resolver this writer deliberately does not carry — and a report is not worth a serializer
    /// context.</summary>
    private static JsonNode? ToJsonValue(object? value) => value switch
    {
        null      => null,
        string t  => JsonValue.Create(t),
        bool b    => JsonValue.Create(b),
        int i     => JsonValue.Create(i),
        long l    => JsonValue.Create(l),
        double d  => JsonValue.Create(d),
        _         => JsonValue.Create(Text(value))
    };

    private static string Yaml(RunStats stats)
    {
        var sb = new StringBuilder();
        foreach (var section in stats.ToSections())
        {
            sb.Append(section.Title).AppendLine(":");
            foreach (var (key, value) in section.Items)
                sb.Append("  ").Append(key).Append(": ").AppendLine(YamlScalar(value));
        }
        return sb.ToString();
    }

    private static string Markdown(RunStats stats)
    {
        var sb = new StringBuilder();
        foreach (var section in stats.ToSections())
        {
            sb.Append("## ").AppendLine(section.Title).AppendLine();
            sb.AppendLine("| | |").AppendLine("|---|---|");
            foreach (var (key, value) in section.Items)
                sb.Append("| ").Append(key).Append(" | ").Append(Text(value).Replace("|", "\\|")).AppendLine(" |");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>Renders one value for a human or a parser, culture-independently. Public because the
    /// on-screen report renders the same sections and must not drift from the files — a run that says
    /// <c>1,865</c> on screen and <c>1.865</c> in its own companion file is two answers to one question.</summary>
    public static string Text(object? value) => value switch
    {
        null => string.Empty,
        bool b => b ? "true" : "false",
        double d => d.ToString("0.####", CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    /// <summary>Quotes anything a YAML reader could mistake for structure. Deliberately conservative:
    /// the cost of an unnecessary pair of quotes is nothing, the cost of an unquoted <c>: </c> in an
    /// endpoint or a model name is a file that does not parse.</summary>
    private static string YamlScalar(object? value)
    {
        if (value is bool or int or long or double) return Text(value);
        var text = Text(value);
        if (text.Length == 0) return "\"\"";

        var needsQuotes = text.Contains(": ") || text.Contains('#') || text.Contains('\n') ||
                          text.Contains('"') || text.Contains('\'') || text.Contains('\\') ||
                          text[0] is '{' or '[' or '&' or '*' or '!' or '|' or '>' or '%' or '@' or '`' or '-' or '?' ||
                          text.EndsWith(':') || text != text.Trim();
        if (!needsQuotes) return text;

        return "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
    }
}
