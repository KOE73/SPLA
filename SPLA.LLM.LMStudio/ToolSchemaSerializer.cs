using System.Text.Json;
using System.Text.Json.Nodes;

namespace SPLA.LLM.LMStudio;

/// <summary>
/// Normalizes a tool's Parameters object before it is sent to an OpenAI-compatible endpoint.
/// Adds <c>additionalProperties: false</c> to every JSON Schema object node so the model
/// cannot hallucinate extra fields. When the tool opts into strict mode the caller should
/// also pass <c>strict: true</c> at the function level.
/// </summary>
internal static class ToolSchemaSerializer
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonNode _empty =
        JsonNode.Parse("{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}")!;

    public static JsonNode Normalize(object? parameters)
    {
        if (parameters is null) return _empty;

        var json = JsonSerializer.Serialize(parameters, _opts);
        var node = JsonNode.Parse(json) ?? _empty;
        AddAdditionalPropertiesFalse(node);
        return node;
    }

    private static void AddAdditionalPropertiesFalse(JsonNode node)
    {
        if (node is not JsonObject obj) return;

        if (obj["type"] is JsonValue typeVal && typeVal.TryGetValue<string>(out var typeStr) && typeStr == "object")
        {
            obj.Remove("additionalProperties");
            obj["additionalProperties"] = false;
        }

        // Snapshot to avoid modifying the collection while enumerating
        var keys = obj.Select(kv => kv.Key).ToArray();
        foreach (var key in keys)
        {
            if (obj[key] is JsonNode child)
                AddAdditionalPropertiesFalse(child);
        }
    }
}
