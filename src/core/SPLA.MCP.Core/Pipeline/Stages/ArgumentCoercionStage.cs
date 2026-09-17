using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Replaces a top-level argument that arrived as a string with the value the tool's schema declares,
/// when the string reads unambiguously as that type: <c>"290"</c> for a number, <c>"False"</c> for a
/// boolean, <c>"[1, 20]"</c> for an array. A field whose schema admits a string is never touched, and
/// a string that does not parse is passed on as it came — the tool answers for it.
/// <para>
/// Silent towards the model on purpose: the stringified value is the serving stack's doing, so there
/// is nothing for the model to correct. Logged at debug only; the raw arguments are already in the
/// telemetry line outside this link.
/// </para>
/// </summary>
public sealed class ArgumentCoercionStage(ILogger? logger) : IToolMiddleware
{
    public ToolPipelineStage Stage => ToolPipelineStage.Arguments;

    public Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        var (coercedJson, coercedFields) = Coerce(call.ArgumentsJson, call.Tool!.GetDefinition().Function.Parameters);
        if (coercedFields.Count > 0)
        {
            call.ArgumentsJson = coercedJson;
            logger?.LogDebug("Arguments coerced to schema types. Tool={ToolName} Fields={Fields}",
                call.Name, string.Join(",", coercedFields));
        }
        return next(call, ct);
    }

    /// <summary>The arguments with coercible strings replaced, and the names of the fields that were.
    /// Returns the input unchanged when nothing applies, including when either side is not JSON.</summary>
    public static (string ArgumentsJson, IReadOnlyList<string> CoercedFields) Coerce(string argumentsJson, object? parameterSchema)
    {
        if (parameterSchema is null || !HasTopLevelString(argumentsJson))
            return (argumentsJson, []);

        JsonObject? arguments;
        JsonElement properties;
        try
        {
            arguments = JsonNode.Parse(argumentsJson) as JsonObject;
            var schema = JsonSerializer.SerializeToElement(parameterSchema);
            if (arguments is null || schema.ValueKind != JsonValueKind.Object ||
                !schema.TryGetProperty("properties", out properties) || properties.ValueKind != JsonValueKind.Object)
                return (argumentsJson, []);
        }
        catch (JsonException)
        {
            return (argumentsJson, []);
        }

        List<string> coercedFields = [];
        foreach (var (fieldName, fieldValue) in arguments.ToList())
        {
            if (fieldValue is not JsonValue value || !value.TryGetValue<string>(out var text) ||
                !properties.TryGetProperty(fieldName, out var fieldSchema))
                continue;

            if (CoerceText(text, DeclaredTypes(fieldSchema)) is { } coercedValue)
            {
                arguments[fieldName] = coercedValue;
                coercedFields.Add(fieldName);
            }
        }

        return coercedFields.Count > 0 ? (arguments.ToJsonString(), coercedFields) : (argumentsJson, []);
    }

    private static bool HasTopLevelString(string argumentsJson)
    {
        try
        {
            using var document = JsonDocument.Parse(argumentsJson);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                   document.RootElement.EnumerateObject().Any(p => p.Value.ValueKind == JsonValueKind.String);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static HashSet<string> DeclaredTypes(JsonElement fieldSchema)
    {
        if (fieldSchema.ValueKind != JsonValueKind.Object || !fieldSchema.TryGetProperty("type", out var type))
            return [];

        return type.ValueKind switch
        {
            JsonValueKind.String => [type.GetString()!],
            JsonValueKind.Array => [.. type.EnumerateArray().Where(t => t.ValueKind == JsonValueKind.String).Select(t => t.GetString()!)],
            _ => [],
        };
    }

    /// <summary>The value the text stands for under the declared types, or null when it must stay a
    /// string: the schema admits one, declares nothing, or the text is not that type.</summary>
    private static JsonNode? CoerceText(string text, HashSet<string> declaredTypes)
    {
        if (declaredTypes.Count == 0 || declaredTypes.Contains("string"))
            return null;

        var trimmed = text.Trim();
        if (trimmed.Length == 0)
            return null;

        if (declaredTypes.Contains("boolean") && bool.TryParse(trimmed, out var flag))
            return JsonValue.Create(flag);

        JsonNode? parsed;
        try
        {
            parsed = JsonNode.Parse(trimmed);
        }
        catch (JsonException)
        {
            return null;
        }

        return parsed?.GetValueKind() switch
        {
            JsonValueKind.Number when declaredTypes.Contains("number") => parsed,
            JsonValueKind.Number when declaredTypes.Contains("integer") && parsed.AsValue().TryGetValue<long>(out _) => parsed,
            JsonValueKind.Array when declaredTypes.Contains("array") => parsed,
            JsonValueKind.Object when declaredTypes.Contains("object") => parsed,
            _ => null,
        };
    }
}
