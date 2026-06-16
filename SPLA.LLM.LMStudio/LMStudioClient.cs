using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPLA.LLM.LMStudio;

public class LMStudioClient : ILLMService, ITokenUsageReporter
{
    /// <summary>LM Studio's OpenAI-compatible endpoint returns a <c>usage</c> block, so real
    /// prompt/completion counts are available (see <see cref="ITokenUsageReporter"/>).</summary>
    public bool SupportsUsage => true;

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public LMStudioClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions 
        { 
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<List<string>> GetModelsAsync(string baseUrl, string apiKey = "lm-studio", CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildEndpointUri(baseUrl, "models"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessWithBodyAsync(response, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        
        var models = new List<string>();
        if (doc.RootElement.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in dataArray.EnumerateArray())
            {
                if (item.TryGetProperty("id", out var idElement))
                {
                    var id = idElement.GetString();
                    if (!string.IsNullOrEmpty(id)) models.Add(id);
                }
            }
        }
        
        return models;
    }

    public async Task<ChatMessage> SendMessageAsync(IEnumerable<ChatMessage> messages, LLMSettings settings, IEnumerable<ToolDefinition>? tools = null, CancellationToken cancellationToken = default)
    {
        var request = CreateRequestMessage(messages, settings, tools, stream: false);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<OpenAIResponse>(_jsonOptions, cancellationToken);
        var msg = responseData?.Choices?.FirstOrDefault()?.Message;

        var (content, reasoning) = SplitReasoning(msg?.Content ?? "", msg?.ReasoningText);

        return new ChatMessage
        {
            Role = ChatRole.Assistant,
            Content = content,
            Reasoning = reasoning,
            ToolCalls = msg?.ToolCalls?.Select(t => new ToolCall
            {
                Id = t.Id,
                Type = t.Type,
                Function = new FunctionCall
                {
                    Name = t.Function.Name,
                    Arguments = t.Function.Arguments
                }
            }).ToList(),
            PromptTokens = responseData?.Usage?.PromptTokens,
            CompletionTokens = responseData?.Usage?.CompletionTokens
        };
    }

    /// <inheritdoc/>
    public async Task<ChatMessage> SendMessageStreamFullAsync(
        IEnumerable<ChatMessage> messages,
        LLMSettings settings,
        IEnumerable<ToolDefinition>? tools,
        Func<string, Task>? onDelta,
        CancellationToken cancellationToken = default,
        Func<string, Task>? onReasoning = null)
    {
        var request = CreateRequestMessage(messages, settings, tools, stream: true);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessWithBodyAsync(response, cancellationToken);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var contentBuilder = new StringBuilder();
        var reasoningBuilder = new StringBuilder();
        // tool_calls accumulator: index → (id, type, name, arguments)
        var toolCallsById = new Dictionary<int, ToolCallAccumulator>();
        int? promptTokens = null;
        int? completionTokens = null;

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data: ")) continue;

            var data = line.Substring(6).Trim();
            if (data == "[DONE]") break;

            OpenAIStreamResponse? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<OpenAIStreamResponse>(data, _jsonOptions);
            }
            catch (JsonException)
            {
                // malformed SSE chunk — skip
                continue;
            }

            var choice = chunk?.Choices?.FirstOrDefault();
            if (choice == null) continue;

            // Usage may appear in the last chunk (some servers send it). Capture the real
            // prompt-token count too — that is the actual size of the request we just sent.
            if (chunk?.Usage != null)
            {
                promptTokens = chunk.Usage.PromptTokens;
                completionTokens = chunk.Usage.CompletionTokens;
            }

            var delta = choice.Delta;
            if (delta == null) continue;

            // --- Reasoning delta (OpenAI reasoning_content / reasoning) ---
            var reasoningChunk = delta.ReasoningText;
            if (!string.IsNullOrEmpty(reasoningChunk))
            {
                reasoningBuilder.Append(reasoningChunk);
                if (onReasoning != null)
                    await onReasoning(reasoningChunk);
            }

            // --- Text delta ---
            if (!string.IsNullOrEmpty(delta.Content))
            {
                contentBuilder.Append(delta.Content);
                if (onDelta != null)
                    await onDelta(delta.Content);
            }

            // --- Tool-call deltas ---
            if (delta.ToolCalls != null)
            {
                foreach (var tc in delta.ToolCalls)
                {
                    var idx = tc.Index;
                    if (!toolCallsById.TryGetValue(idx, out var acc))
                    {
                        acc = new ToolCallAccumulator();
                        toolCallsById[idx] = acc;
                    }

                    if (!string.IsNullOrEmpty(tc.Id))       acc.Id   = tc.Id;
                    if (!string.IsNullOrEmpty(tc.Type))      acc.Type = tc.Type;
                    if (tc.Function != null)
                    {
                        if (!string.IsNullOrEmpty(tc.Function.Name))
                            acc.Name = tc.Function.Name;
                        if (tc.Function.Arguments != null)
                            acc.Arguments.Append(tc.Function.Arguments);
                    }
                }
            }
        }

        // Assemble final ChatMessage
        List<ToolCall>? toolCalls = null;
        if (toolCallsById.Count > 0)
        {
            toolCalls = toolCallsById
                .OrderBy(kv => kv.Key)
                .Select(kv => new ToolCall
                {
                    Id       = kv.Value.Id,
                    Type     = string.IsNullOrEmpty(kv.Value.Type) ? "function" : kv.Value.Type,
                    Function = new FunctionCall
                    {
                        Name      = kv.Value.Name,
                        Arguments = kv.Value.Arguments.ToString()
                    }
                }).ToList();
        }

        // Reasoning may arrive either as a dedicated channel (reasoning_content) or inline
        // as <think>...</think> in the content; reconcile both into a single field.
        var (finalContent, inlineReasoning) = SplitReasoning(contentBuilder.ToString(), reasoningBuilder.ToString());

        return new ChatMessage
        {
            Role             = ChatRole.Assistant,
            Content          = finalContent,
            Reasoning        = inlineReasoning,
            ToolCalls        = toolCalls,
            PromptTokens     = promptTokens,
            CompletionTokens = completionTokens
        };
    }

    /// <summary>
    /// Reconciles reasoning that may come from a dedicated channel and/or inline
    /// <c>&lt;think&gt;...&lt;/think&gt;</c> tags embedded in the content. Returns the cleaned
    /// content (tags removed) and the combined reasoning text (null when empty).
    /// </summary>
    private static (string Content, string? Reasoning) SplitReasoning(string content, string? channelReasoning)
    {
        content ??= string.Empty;
        var reasoningParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(channelReasoning))
            reasoningParts.Add(channelReasoning.Trim());

        if (content.Contains("<think>", StringComparison.OrdinalIgnoreCase))
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(
                content, "<think>(.*?)</think>",
                System.Text.RegularExpressions.RegexOptions.Singleline |
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                var inner = m.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(inner)) reasoningParts.Add(inner);
            }

            // Strip the think blocks (and any unterminated trailing one) from the visible content.
            content = System.Text.RegularExpressions.Regex.Replace(
                content, "<think>.*?</think>",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.Singleline |
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            content = System.Text.RegularExpressions.Regex.Replace(
                content, "<think>.*$",
                string.Empty,
                System.Text.RegularExpressions.RegexOptions.Singleline |
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            content = content.Trim();
        }

        var reasoning = reasoningParts.Count > 0 ? string.Join("\n\n", reasoningParts) : null;
        return (content, reasoning);
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(IEnumerable<ChatMessage> messages, LLMSettings settings, IEnumerable<ToolDefinition>? tools = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = CreateRequestMessage(messages, settings, tools, stream: true);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessWithBodyAsync(response, cancellationToken);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

            var data = line.Substring(6);
            if (data == "[DONE]") break;

            var chunk = JsonSerializer.Deserialize<OpenAIStreamResponse>(data, _jsonOptions);
            var content = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
            
            if (!string.IsNullOrEmpty(content))
            {
                yield return content;
            }
        }
    }

    private HttpRequestMessage CreateRequestMessage(IEnumerable<ChatMessage> messages, LLMSettings settings, IEnumerable<ToolDefinition>? tools, bool stream)
    {
        var payload = new Dictionary<string, object>
        {
            ["model"] = settings.ModelName,
            ["temperature"] = settings.Temperature,
            ["stream"] = stream,
            ["messages"] = messages.Select(m => 
            {
                if (m.Role == ChatRole.Tool)
                {
                    return (object)new Dictionary<string, object?>
                    {
                        ["role"] = "tool",
                        ["content"] = m.Content,
                        ["tool_call_id"] = m.ToolCallId
                    };
                }
                
                var msgDict = new Dictionary<string, object?>
                {
                    ["role"] = m.Role.ToString().ToLowerInvariant(),
                    ["content"] = m.Content
                };

                if (m.Role == ChatRole.Assistant && m.ToolCalls?.Any() == true)
                {
                    msgDict["tool_calls"] = m.ToolCalls.Select(t => new Dictionary<string, object?>
                    {
                        ["id"] = t.Id,
                        ["type"] = t.Type,
                        ["function"] = new Dictionary<string, object?>
                        {
                            ["name"] = t.Function.Name,
                            ["arguments"] = t.Function.Arguments
                        }
                    }).ToArray();
                }

                return msgDict;
            }).ToArray()
        };

        // Reasoning lever for the OpenAI-compatible endpoint. on/off maps to the chat-template
        // kwarg (Qwen3/Gemma-style); graded values map to reasoning_effort (gpt-oss-style).
        // Empty = leave the model's own default. Unknown fields are ignored by LM Studio.
        var level = settings.ReasoningLevel?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(level))
        {
            if (level is "off" or "on")
                payload["chat_template_kwargs"] = new Dictionary<string, object?> { ["enable_thinking"] = level == "on" };
            else
                payload["reasoning_effort"] = level;
        }

        if (tools?.Any() == true)
        {
            payload["tools"] = tools.Select(t => new Dictionary<string, object>
            {
                ["type"] = t.Type,
                ["function"] = new Dictionary<string, object?>
                {
                    ["name"] = t.Function.Name,
                    ["description"] = t.Function.Description,
                    ["parameters"] = t.Function.Parameters ?? new { type = "object", properties = new { } }
                }
            }).ToArray();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, BuildEndpointUri(settings.BaseUrl, "chat/completions"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Content = JsonContent.Create(payload, null, _jsonOptions);
        return request;
    }

    private static Uri BuildEndpointUri(string baseUrl, string relativePath)
    {
        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        return new Uri(new Uri(baseUrl), relativePath);
    }

    private static async Task EnsureSuccessWithBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            response.EnsureSuccessStatusCode();
        }

        throw new HttpRequestException(
            $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}",
            null,
            response.StatusCode);
    }

    // ─── Accumulator helper ───────────────────────────────────────────────────

    private sealed class ToolCallAccumulator
    {
        public string Id        { get; set; } = string.Empty;
        public string Type      { get; set; } = "function";
        public string Name      { get; set; } = string.Empty;
        public StringBuilder Arguments { get; } = new();
    }

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
