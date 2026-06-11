using System.Text;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>
/// Minimal YAML serialization helpers for tool responses.
/// Avoids a heavy YAML library dependency while producing readable output.
/// </summary>
internal static class YamlResponse
{
    public static string Object(Action<YamlBuilder> build)
    {
        var sb = new StringBuilder();
        var builder = new YamlBuilder(sb, 0);
        build(builder);
        return sb.ToString();
    }
}

internal class YamlBuilder
{
    private readonly StringBuilder _sb;
    private readonly int           _indent;
    private string Pad => new string(' ', _indent * 2);

    public YamlBuilder(StringBuilder sb, int indent)
    {
        _sb    = sb;
        _indent = indent;
    }

    public YamlBuilder Field(string key, object? value)
    {
        if (value is null) return this;
        _sb.AppendLine($"{Pad}{key}: {Escape(value.ToString()!)}");
        return this;
    }

    public YamlBuilder Field(string key, bool value)
    {
        _sb.AppendLine($"{Pad}{key}: {(value ? "true" : "false")}");
        return this;
    }

    public YamlBuilder Field(string key, int value)
    {
        _sb.AppendLine($"{Pad}{key}: {value}");
        return this;
    }

    public YamlBuilder Section(string key, Action<YamlBuilder> nested)
    {
        _sb.AppendLine($"{Pad}{key}:");
        nested(new YamlBuilder(_sb, _indent + 1));
        return this;
    }

    public YamlBuilder List(string key, IEnumerable<string> items)
    {
        var list = items.ToList();
        if (list.Count == 0) return this;
        _sb.AppendLine($"{Pad}{key}:");
        foreach (var item in list)
            _sb.AppendLine($"{Pad}  - {Escape(item)}");
        return this;
    }

    public YamlBuilder RelationList(string key, IEnumerable<RelationRow> rows, bool includeSource = false)
    {
        var list = rows.ToList();
        if (list.Count == 0) return this;
        _sb.AppendLine($"{Pad}{key}:");
        foreach (var r in list)
        {
            _sb.AppendLine($"{Pad}  - fullName: {r.ToFullName}");
            _sb.AppendLine($"{Pad}    kind: {r.ToKind}");
            if (includeSource && r.SourcePath is not null)
            {
                _sb.AppendLine($"{Pad}    sourcePath: {r.SourcePath}");
                if (r.SourceLine.HasValue)
                    _sb.AppendLine($"{Pad}    sourceLine: {r.SourceLine}");
            }
            _sb.AppendLine($"{Pad}    confidence: {r.Confidence}");
        }
        return this;
    }

    public YamlBuilder ReverseRelationList(string key, IEnumerable<RelationRow> rows, bool includeSource = false)
    {
        var list = rows.ToList();
        if (list.Count == 0) return this;
        _sb.AppendLine($"{Pad}{key}:");
        foreach (var r in list)
        {
            _sb.AppendLine($"{Pad}  - relation: {r.Type}");
            _sb.AppendLine($"{Pad}    from: {r.FromFullName}");
            _sb.AppendLine($"{Pad}    fromKind: {r.FromKind}");
            if (includeSource && r.SourcePath is not null)
            {
                _sb.AppendLine($"{Pad}    sourcePath: {r.SourcePath}");
                if (r.SourceLine.HasValue)
                    _sb.AppendLine($"{Pad}    sourceLine: {r.SourceLine}");
            }
        }
        return this;
    }

    private static string Escape(string s)
    {
        if (s.Contains(':') || s.Contains('#') || s.Contains('"') || s.Contains('\''))
            return $"\"{s.Replace("\"", "\\\"")}\"";
        return s;
    }
}

