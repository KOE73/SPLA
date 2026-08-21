using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Formats;
using SPLA.Domain.Resources;
using YamlDotNet.Serialization;

namespace SPLA.MCP.Core.Formats;

/// <summary>
/// JSON to YAML — the same values, reshaped.
///
/// <para><b>Why this one is in the day-one set.</b> Identity moves nothing and the UTF-8 decoder only
/// relabels; neither exercises a real reshape. This one parses, builds a different document, and emits
/// a payload of a different size, with a parse failure of its own that has nothing to do with encoding.
/// After it, <c>docx → md</c> is one class and one line of registration, with no change anywhere
/// else.</para>
///
/// <para>A projection like every other member: comments are not in JSON to lose, but key order,
/// number formatting and the distinction between an integer and a float are all at the mercy of the
/// round trip. Nothing here promises the YAML re-parses to a byte-identical JSON.</para>
/// </summary>
public sealed class JsonToYamlConverter : IFormatConverter
{
    public string SourceType => "application/json";
    public string TargetType => "application/yaml";

    public string Summary =>
        "JSON to YAML — the same values in block style; key order and number formatting are the " +
        "serialiser's, and comments do not survive a round trip back";

    public Task<ResourceContent> ConvertAsync(
        ResourceContent source,
        IReadOnlyDictionary<string, object?>? options,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var bytes = source.Bytes ?? Array.Empty<byte>();

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(bytes);
        }
        catch (JsonException ex)
        {
            // Say WHERE. "Invalid JSON" sends the caller back to reading the whole payload; a line and
            // a position is the difference between a fix and a search.
            throw new InvalidOperationException(
                $"Cannot convert to YAML: the source is not valid JSON — {ex.Message} " +
                $"(line {ex.LineNumber?.ToString(CultureInfo.InvariantCulture) ?? "?"}, " +
                $"position {ex.BytePositionInLine?.ToString(CultureInfo.InvariantCulture) ?? "?"}).",
                ex);
        }

        using (doc)
        {
            var graph = ToGraph(doc.RootElement);
            var yaml = new SerializerBuilder().Build().Serialize(graph);
            return Task.FromResult(new ResourceContent(Encoding.UTF8.GetBytes(yaml), TargetType));
        }
    }

    /// <summary>
    /// The JSON document as plain CLR objects, which is the only shape YamlDotNet's serialiser can be
    /// handed without a type. Numbers keep their JSON spelling where it matters: an integer stays an
    /// integer instead of becoming <c>1</c> spelled <c>1.0</c>.
    /// </summary>
    private static object? ToGraph(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var map = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var property in element.EnumerateObject())
                    map[property.Name] = ToGraph(property.Value);
                return map;

            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                    list.Add(ToGraph(item));
                return list;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt64(out var i)) return i;
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            default:
                return null;
        }
    }
}
