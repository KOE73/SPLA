namespace SPLA.Observability;

public static class SplaTelemetryContextExtensions
{
    public static SplaTelemetryContext WithToolCall(this SplaTelemetryContext context, string? toolCallId) =>
        context with { ToolCallId = toolCallId };
}
