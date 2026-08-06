using SPLA.Domain.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Agent.Composition;
using SPLA.MCP.Core.Composition;
using SPLA.MCP.Core.Plugins;
using SPLA.Library;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Agent;

/// <summary>
/// Runs a skill headlessly in a fresh agent instance (new Conversation, new SkillSession).
/// Same code as the interactive agent — different entry point only.
/// The spawned agent follows the skill body to completion without UI or clarify interaction.
/// skill_activate / skill_deactivate are bypassed: the skill is activated directly and the
/// run ends when the orchestrator loop finishes (no more tool calls from the model).
/// </summary>
public sealed class SpawnedAgentRunner : Domain.Interfaces.IAgentSpawner
{
    private readonly Domain.Llm.ILlmGateway _llm;
    private readonly Domain.Interfaces.IToolHost _tools;
    private readonly SkillLibrary _skills;
    private readonly PluginManager _plugins;
    private readonly ResolvedSettings _settings;

    /// <summary>
    /// How many spawns deep the current async flow already is. A sub-agent reaches tools through the
    /// same <see cref="Domain.Interfaces.IToolHost"/> as its parent, so nothing stops it from calling
    /// agent_spawn again; the orchestrator's loop guard watches repeated tool calls, which is a
    /// different thing from recursion between agents.
    /// <para><see cref="AsyncLocal{T}"/> rather than a field on purpose: several chats spawn
    /// concurrently, and a shared counter would add their depths together.</para>
    /// </summary>
    private static readonly AsyncLocal<int> _depth = new();

    /// <summary>Spawns allowed below the top-level chat. Chosen above anything in use today, so this
    /// limit refuses runaway recursion without changing how existing skills behave.</summary>
    private const int MaxDepth = 3;

    public SpawnedAgentRunner(
        Domain.Llm.ILlmGateway llm,
        Domain.Interfaces.IToolHost tools,
        SkillLibrary skills,
        PluginManager plugins,
        ResolvedSettings settings)
    {
        _llm = llm;
        _tools = tools;
        _skills = skills;
        _plugins = plugins;
        _settings = settings;
    }

    /// <summary>
    /// Runs <paramref name="skillId"/> against <paramref name="input"/> in a fresh conversation.
    /// Returns the last assistant message produced by the run.
    /// Throws <see cref="System.ArgumentException"/> if the skill is not found.
    /// </summary>
    public async Task<string> RunSkillAsync(
        string skillId,
        string input,
        AgentMode mode,
        CancellationToken cancellationToken = default)
    {
        // Refused as text, not as an exception: the caller is a model reading a tool result, and a
        // sentence it can act on beats a stack trace it cannot.
        if (_depth.Value >= MaxDepth)
            return $"error: spawn depth limit reached ({MaxDepth}). " +
                   "A spawned agent cannot keep spawning; do the remaining work in this run.";

        var lookup = _skills.Resolve(skillId);
        if (lookup.IsAmbiguous)
            throw new System.ArgumentException(
                $"Skill '{skillId}' is held by more than one source — name one of: " +
                string.Join(", ", lookup.Candidates.Select(c => c.Address)), nameof(skillId));

        var meta = lookup.Card;
        if (meta is null)
            throw new System.ArgumentException($"Skill '{skillId}' not found.", nameof(skillId));

        // Fresh isolated agent state — own skill session, working memory, and checkpoint manager.
        // Opening an AgentSessionScope keeps the sub-agent's tool calls (memory, marks, skills) off
        // the parent chat's state, even though the spawn happens inside the parent's async flow.
        var body = _skills.LoadBody(meta.Address);
        if (string.IsNullOrWhiteSpace(body))
            throw new System.ArgumentException(
                $"Skill '{skillId}' has no readable procedure.", nameof(skillId));

        var skillSession = new SkillSession();
        // Same loan slip as an in-chat activation: a sub-agent running a skill needs that skill's
        // references as much as the parent would, and its own session is the only place to hold them.
        skillSession.Activate(meta.DisplayId, body, meta.SourceId, meta.Ref, _skills.ListResources(meta.Address));
        var checkpoint = new CheckpointManager();
        // Everything else here is deliberately fresh, but the sandbox is inherited: it is the host's
        // boundary, not the agent's state. Left to its default a sub-agent would come out of its
        // parent's sandbox — a way out of the box by spawning, which matters the moment a chat runs
        // with a real one. No parent (a CLI or worker entry point) keeps the constructor's default.
        var agentSession = new AgentSession(
            new KeyValueStore("session"), checkpoint, skillSession,
            sandbox: AgentSessionScope.Current?.Sandbox);

        // Composed once, up front: a sub-agent runs one pinned procedure and cannot activate another,
        // so there is nothing for a per-iteration recomposition to pick up. The session is passed
        // explicitly rather than resolved ambiently — the spawn happens inside the parent's async
        // flow, and the sub-agent must describe its own skill, not the parent's.
        var composer = new AgentContextComposer(
            AgentContributors.Default(_skills, _plugins, skillSession));
        var systemPrompt = composer.Compose(_settings, _settings.WorkspacePath).SystemPrompt;

        var conversation = new Conversation();
        conversation.Add(new ChatMessage { Role = ChatRole.System, Content = systemPrompt });
        conversation.Add(new ChatMessage { Role = ChatRole.User, Content = input });

        string lastAssistantMessage = string.Empty;

        // Spawned sub-agents are the most prone to tool-call loops; guard them too (tool-call only).
        var orchestrator = new ConversationOrchestrator(_llm, _tools) { Checkpoint = checkpoint, EnableLoopGuard = true };
        var callbacks = new AgentCallbacks
        {
            OnAssistantMessage = msg =>
            {
                lastAssistantMessage = msg.Content ?? string.Empty;
                return Task.CompletedTask;
            }
        };

        var llmSettings = _settings.ToLLMSettings();
        llmSettings.Mode = mode;

        // Counted around the run itself, and restored rather than decremented — the same shape the
        // ambient scopes in this codebase use, so an exception cannot leave the depth raised.
        var previousDepth = _depth.Value;
        _depth.Value = previousDepth + 1;
        try
        {
            using (AgentSessionScope.Begin(agentSession))
                await orchestrator.RunAsync(conversation, llmSettings, mode, callbacks, cancellationToken);
        }
        finally
        {
            _depth.Value = previousDepth;
        }

        return lastAssistantMessage;
    }
}
