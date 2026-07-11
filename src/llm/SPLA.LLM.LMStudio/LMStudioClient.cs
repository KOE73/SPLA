using Microsoft.Extensions.Logging;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPLA.LLM.LMStudio;

public sealed partial class LMStudioClient : ILLMService, ITokenUsageReporter
{
    /// <summary>LM Studio's OpenAI-compatible endpoint returns a <c>usage</c> block, so real
    /// prompt/completion counts are available (see <see cref="ITokenUsageReporter"/>).</summary>
    public bool SupportsUsage => true;

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<LMStudioClient> _logger;

    public LMStudioClient(HttpClient httpClient, ILogger<LMStudioClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
        using var response = await SendWithConnectivityAsync(request, settings.BaseUrl, cancellationToken);
        await EnsureSuccessWithBodyAsync(response, cancellationToken);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var contentBuilder = new StringBuilder();
        var reasoningBuilder = new StringBuilder();
        // tool_calls accumulator: index → (id, type, name, arguments)
        var toolCallsById = new Dictionary<int, ToolCallAccumulator>();
        int? promptTokens = null;
        int? completionTokens = null;
        string? finishReason = null;

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

            // The final usage chunk from LM Studio arrives with choices:[] (empty) and only
            // the usage field populated. Must capture it BEFORE the choice==null guard below.
            if (chunk?.Usage != null)
            {
                promptTokens    = chunk.Usage.PromptTokens;
                completionTokens = chunk.Usage.CompletionTokens;
            }

            var choice = chunk?.Choices?.FirstOrDefault();
            if (choice == null) continue;

            if (!string.IsNullOrEmpty(choice.FinishReason))
                finishReason = choice.FinishReason;

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

        // Log raw turn summary for diagnosing finish_reason / tool_call anomalies.
        _logger.LogDebug(
            "LLM turn complete — finish_reason={FinishReason} tool_calls={ToolCallCount} content_len={ContentLen} reasoning_len={ReasoningLen}",
            finishReason ?? "null",
            toolCallsById.Count,
            contentBuilder.Length,
            reasoningBuilder.Length);

        if (finishReason == "stop" && toolCallsById.Count == 0 && contentBuilder.Length > 0)
        {
            // Model stopped with text only — check if it contains XML-style tool call syntax
            // that should have been a real tool call (known issue with some local models).
            var rawContent = contentBuilder.ToString();
            if (rawContent.Contains("<tool_call>", StringComparison.OrdinalIgnoreCase) ||
                rawContent.Contains("<function=", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Model returned finish_reason=stop with tool-call-like syntax in content instead of tool_calls field. " +
                    "This is a model behaviour issue — the tool was NOT executed. Content snippet: {Snippet}",
                    rawContent.Length > 300 ? rawContent[..300] + "…" : rawContent);
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
            ["presence_penalty"]  = settings.PresencePenalty,
            ["frequency_penalty"] = settings.FrequencyPenalty,
            ["repeat_penalty"]    = settings.RepeatPenalty,
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
                    // Vision: a user message with images is sent as the OpenAI parts array
                    // (text + image_url data URLs); otherwise the plain string content.
                    ["content"] = m.Images is { Count: > 0 }
                        ? BuildMultimodalContent(m.Content, m.Images)
                        : (object?)m.Content
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

        // stream_options.include_usage is only meaningful (and valid) for streaming requests.
        // For non-streaming calls usage is returned in the single response body without this flag.
        if (stream)
            payload["stream_options"] = new Dictionary<string, object> { ["include_usage"] = true };

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
            payload["tools"] = tools.Select(t =>
            {
                var func = new Dictionary<string, object?>
                {
                    ["name"]        = t.Function.Name,
                    ["description"] = t.Function.Description,
                    ["parameters"]  = ToolSchemaSerializer.Normalize(t.Function.Parameters)
                };
                if (t.Function.StrictSchema)
                    func["strict"] = true;
                return new Dictionary<string, object?> { ["type"] = t.Type, ["function"] = func };
            }).ToArray();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, BuildEndpointUri(settings.BaseUrl, "chat/completions"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Content = JsonContent.Create(payload, null, _jsonOptions);
        return request;
    }

    /// <summary>Builds the OpenAI vision "content parts" array: a text part (if any) followed by one
    /// image_url part per attached data URL. LM Studio routes these to vision-capable models.</summary>
    private static object[] BuildMultimodalContent(string? text, List<string> images)
    {
        var parts = new List<object>();
        if (!string.IsNullOrEmpty(text))
            parts.Add(new Dictionary<string, object?> { ["type"] = "text", ["text"] = text });
        foreach (var url in images)
            parts.Add(new Dictionary<string, object?>
            {
                ["type"] = "image_url",
                ["image_url"] = new Dictionary<string, object?> { ["url"] = url }
            });
        return parts.ToArray();
    }

    private static Uri BuildEndpointUri(string baseUrl, string relativePath)
    {
        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        return new Uri(new Uri(baseUrl), relativePath);
    }

    /// <summary>Sends the request, converting a transport failure (endpoint down, wrong URL, refused
    /// connection) into a typed <see cref="LlmErrorKind.Unreachable"/> error instead of a raw
    /// <see cref="HttpRequestException"/>. A cancelled request is left to propagate as
    /// <see cref="OperationCanceledException"/>.</summary>
    private async Task<HttpResponseMessage> SendWithConnectivityAsync(HttpRequestMessage request, string baseUrl, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new LlmRequestException(LlmErrorKind.Unreachable,
                $"Could not reach the LLM endpoint at {baseUrl}. Is the server running and the URL correct?",
                ex.StatusCode, ex.ToString());
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A timeout (HttpClient.Timeout), not a user cancel.
            throw new LlmRequestException(LlmErrorKind.Unreachable,
                $"The LLM endpoint at {baseUrl} did not respond in time.", null, ex.ToString());
        }
    }

    private static async Task EnsureSuccessWithBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw ClassifyHttpFailure(response.StatusCode, response.ReasonPhrase, body);
    }

    /// <summary>
    /// Normalizes an unsuccessful HTTP response into a typed <see cref="LlmRequestException"/> with a
    /// short, actionable message. Providers disagree on how they report a full context window — OpenAI
    /// and vLLM return a clean 400 whose JSON <c>error.message</c> names the token limits, while LM
    /// Studio commonly returns a 500 with an HTML page — so we key off both the status and the body
    /// text and collapse them into one <see cref="LlmErrorKind.ContextExhausted"/> signal the UI can act on.
    /// </summary>
    internal static LlmRequestException ClassifyHttpFailure(HttpStatusCode status, string? reason, string? body)
    {
        var providerMessage = ExtractProviderMessage(body);
        var haystack = (providerMessage ?? body ?? string.Empty);

        // OpenAI/vLLM: 400 (or an explicit code) whose message names the context length.
        var looksLikeContext =
            haystack.Contains("context length", StringComparison.OrdinalIgnoreCase) ||
            haystack.Contains("context_length_exceeded", StringComparison.OrdinalIgnoreCase) ||
            haystack.Contains("maximum context", StringComparison.OrdinalIgnoreCase) ||
            haystack.Contains("too many tokens", StringComparison.OrdinalIgnoreCase) ||
            haystack.Contains("reduce the length", StringComparison.OrdinalIgnoreCase);

        if (looksLikeContext)
        {
            var detail = string.IsNullOrWhiteSpace(providerMessage) ? null : providerMessage;
            return new LlmRequestException(LlmErrorKind.ContextExhausted,
                "The conversation is too long for the model's context window. Start a new chat or shorten "
                + "the conversation, then try again."
                + (detail != null ? $" (provider: {Truncate(detail, 200)})" : string.Empty),
                status, body);
        }

        switch ((int)status)
        {
            case 401:
            case 403:
                return new LlmRequestException(LlmErrorKind.AuthFailed,
                    $"The LLM endpoint rejected the request (HTTP {(int)status}). Check the connection's API key.",
                    status, body);
            case 429:
                return new LlmRequestException(LlmErrorKind.RateLimited,
                    "The LLM endpoint is rate-limited or overloaded (HTTP 429). Wait a moment and retry.",
                    status, body);
            case 500:
            case 502:
            case 503:
                // LM Studio (llama.cpp) frequently answers a 500 with an HTML page when the prompt
                // overflows the loaded context or the model faults. We cannot be certain which, so the
                // message names the likeliest cause without over-claiming.
                return new LlmRequestException(LlmErrorKind.ProviderError,
                    $"The LLM server returned HTTP {(int)status}. With local runtimes (e.g. LM Studio) this "
                    + "usually means the context window overflowed or the model errored — try a new chat or a "
                    + "smaller request."
                    + (!string.IsNullOrWhiteSpace(providerMessage) ? $" ({Truncate(providerMessage!, 200)})" : string.Empty),
                    status, body);
            default:
                var tail = !string.IsNullOrWhiteSpace(providerMessage) ? providerMessage! : (reason ?? "unknown error");
                return new LlmRequestException(LlmErrorKind.ProviderError,
                    $"The LLM endpoint returned HTTP {(int)status}: {Truncate(tail, 200)}",
                    status, body);
        }
    }

    /// <summary>Pulls a human message out of an OpenAI/vLLM-style error body (<c>{"error":{"message":…}}</c>
    /// or <c>{"message":…}</c>). Returns null for HTML/empty/unparseable bodies.</summary>
    internal static string? ExtractProviderMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        var trimmed = body.TrimStart();
        if (!trimmed.StartsWith('{')) return null; // HTML error page etc.
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var err))
            {
                if (err.ValueKind == JsonValueKind.String) return err.GetString();
                if (err.ValueKind == JsonValueKind.Object && err.TryGetProperty("message", out var em))
                    return em.GetString();
            }
            if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                return m.GetString();
        }
        catch (JsonException) { /* not JSON we understand */ }
        return null;
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max] + "…";

    // ─── Accumulator helper ───────────────────────────────────────────────────

    private sealed class ToolCallAccumulator
    {
        public string Id        { get; set; } = string.Empty;
        public string Type      { get; set; } = "function";
        public string Name      { get; set; } = string.Empty;
        public StringBuilder Arguments { get; } = new();
    }
}
