using System.Text.Json.Serialization;

namespace SPLA.LLM.LMStudio;

/// <summary>
/// The OpenAI-compatible request/response DTOs the client (de)serializes. Kept in a partial of
/// <see cref="LMStudioClient"/> so they stay private to the client (nothing else should bind to the
/// provider's wire shape) while keeping the main file focused on the request/stream logic.
/// </summary>
public sealed partial class LMStudioClient
{
    // ─── Non-streaming response DTOs ──────────────────────────────────────────

    private class OpenAIResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAIChoice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public OpenAIUsage? Usage { get; set; }
    }

    private class OpenAIUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    private class OpenAIChoice
    {
        [JsonPropertyName("message")]
        public OpenAIMessage? Message { get; set; }
    }

    private class OpenAIMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("reasoning_content")]
        public string? ReasoningContent { get; set; }

        [JsonPropertyName("reasoning")]
        public string? Reasoning { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OpenAIToolCall>? ToolCalls { get; set; }

        /// <summary>First non-empty of the two reasoning aliases used across servers.</summary>
        public string? ReasoningText =>
            !string.IsNullOrEmpty(ReasoningContent) ? ReasoningContent : Reasoning;
    }

    private class OpenAIToolCall
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("function")]
        public OpenAIFunctionCall Function { get; set; } = new();
    }

    private class OpenAIFunctionCall
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = string.Empty;
    }

    // ─── Streaming response DTOs ──────────────────────────────────────────────

    private class OpenAIStreamResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAIStreamChoice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public OpenAIUsage? Usage { get; set; }
    }

    private class OpenAIStreamChoice
    {
        [JsonPropertyName("delta")]
        public OpenAIStreamDelta? Delta { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    private class OpenAIStreamDelta
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("reasoning_content")]
        public string? ReasoningContent { get; set; }

        [JsonPropertyName("reasoning")]
        public string? Reasoning { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OpenAIStreamToolCallDelta>? ToolCalls { get; set; }

        /// <summary>First non-empty of the two reasoning aliases used across servers.</summary>
        public string? ReasoningText =>
            !string.IsNullOrEmpty(ReasoningContent) ? ReasoningContent : Reasoning;
    }

    private class OpenAIStreamToolCallDelta
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("function")]
        public OpenAIStreamFunctionDelta? Function { get; set; }
    }

    private class OpenAIStreamFunctionDelta
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }
}
