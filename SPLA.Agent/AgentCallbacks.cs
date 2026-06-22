using SPLA.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPLA.Agent;

/// <summary>
/// Output hooks the orchestrator calls as a turn progresses. Every entry point (CLI console,
/// Avalonia UI, a future server) implements only these — the loop itself stays I/O-agnostic.
/// All hooks are optional; null means "ignore this event".
/// </summary>
public sealed class AgentCallbacks
{
    /// <summary>
    /// Fires before each LLM call, carrying the exact context that will be sent. Entry points use
    /// it to spin up a streaming sink (UI bubble) and prime token estimation before deltas arrive.
    /// </summary>
    public Func<IReadOnlyList<ChatMessage>, Task>? OnLlmTurnStart { get; init; }

    /// <summary>A chunk of assistant answer text as it streams.</summary>
    public Func<string, Task>? OnDelta { get; init; }

    /// <summary>A chunk of reasoning/chain-of-thought text as it streams.</summary>
    public Func<string, Task>? OnReasoning { get; init; }

    /// <summary>The fully assembled assistant message (text + reasoning + tool calls), once per LLM call.</summary>
    public Func<ChatMessage, Task>? OnAssistantMessage { get; init; }

    /// <summary>A tool is about to execute.</summary>
    public Func<ToolCall, Task>? OnToolCallStarted { get; init; }

    /// <summary>
    /// A running tool reported progress (a <see cref="ToolProgress"/> tick). Deliberately a
    /// fire-and-forget <see cref="Action"/>, not an async hook: a progress tick must never block or
    /// back-pressure the tool, and may fire thousands of times — the consumer is expected to throttle
    /// and marshal to its UI thread itself. Wired from <see cref="SPLA.Domain.Tools.ProgressScope"/>;
    /// carries the progress of the currently executing top-level tool call (the matching tree root).
    /// </summary>
    public Action<ToolCall, ToolProgress>? OnToolProgress { get; init; }

    /// <summary>
    /// Hands the consumer the <see cref="SPLA.Domain.Tools.ProgressTree"/> for the turn, once, before
    /// any tool runs. Consumers that want the full nested/parallel picture (a tool tree, scripts and
    /// their children) subscribe to <see cref="SPLA.Domain.Tools.ProgressTree.NodeChanged"/> here.
    /// Plain and framework-agnostic: a CLI, Avalonia, or Blazor host all consume it the same way.
    /// <see cref="OnToolProgress"/> remains the simple single-bar view for hosts that don't need the tree.
    /// </summary>
    public Action<SPLA.Domain.Tools.ProgressTree>? OnProgressTree { get; init; }

    /// <summary>A tool finished; carries the raw result string.</summary>
    public Func<ToolCall, string, Task>? OnToolResult { get; init; }

    /// <summary>An ephemeral notice for the user (guard tripped, loop stopped). Never sent to the model.</summary>
    public Func<string, Task>? OnNotice { get; init; }
}
