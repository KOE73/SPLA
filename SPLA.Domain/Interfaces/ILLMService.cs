using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Interfaces;

public interface ILLMService
{
    Task<ChatMessage> SendMessageAsync(IEnumerable<ChatMessage> messages, LLMSettings settings, IEnumerable<ToolDefinition>? tools = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> SendMessageStreamAsync(IEnumerable<ChatMessage> messages, LLMSettings settings, IEnumerable<ToolDefinition>? tools = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a chat request in streaming mode, invoking <paramref name="onDelta"/> for each text chunk
    /// as it arrives, and returns the fully assembled <see cref="ChatMessage"/> (including tool_calls)
    /// when the stream is finished.
    /// </summary>
    Task<ChatMessage> SendMessageStreamFullAsync(
        IEnumerable<ChatMessage> messages,
        LLMSettings settings,
        IEnumerable<ToolDefinition>? tools,
        Func<string, Task>? onDelta,
        CancellationToken cancellationToken = default);
}
