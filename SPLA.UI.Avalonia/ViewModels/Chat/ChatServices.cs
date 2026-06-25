using Microsoft.Extensions.Logging;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.LLM.LMStudio;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Plugins;
using SPLA.Domain.Settings;
using System;
using System.Collections.Generic;

namespace SPLA.UI.Avalonia.ViewModels.Chat;

/// <summary>
/// The singletons shared by every chat in the window. A <see cref="ChatSessionViewModel"/> owns all
/// of its <em>own</em> state (messages, input, run-state, working memory, checkpoint, skill session,
/// connection choice); only the genuinely-shared infrastructure lives here and is handed to each chat.
/// </summary>
public sealed class ChatServices
{
    public required LMStudioClient Llm { get; init; }
    public required McpHost McpHost { get; init; }
    public required SkillManager SkillManager { get; init; }
    public required PluginManager PluginManager { get; init; }
    public required ChatManager ChatManager { get; init; }

    /// <summary>Shared, always-persistent project working memory (project-kv.yaml).</summary>
    public required IKeyValueStore ProjectKv { get; init; }

    /// <summary>Mode + sidebar tool gating, layered on the agent's own mode rules.</summary>
    public required Func<AgentMode, List<ToolDefinition>> ToolFilter { get; init; }

    /// <summary>Shared fetched model catalogue (used to derive reasoning options for a chat's model).</summary>
    public required Func<IReadOnlyList<ModelInfo>> ModelCatalog { get; init; }

    /// <summary>Persists a remembered tool-permission decision to project config + the manager.</summary>
    public required Action<ToolFunctionDefinition, string, PermissionDecision> PersistPermission { get; init; }

    /// <summary>Logger for the per-chat conversation loop (LLM turns, tool calls).</summary>
    public ILogger? Logger { get; init; }
}
