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
/// Runs a task headlessly in a fresh agent instance (new Conversation, new SkillSession).
/// Same code as the interactive agent — different entry point only.
/// <para>Two shapes, one loop. With a skill id the procedure is pinned up front — skill_activate /
/// skill_deactivate are bypassed, the sub-agent runs that one procedure, and the run ends when the
/// orchestrator loop finishes. Without one it is a plain delegated task: the base prompt, the seed
/// message, and the same tools. Delegation is the point of a sub-agent and a curated procedure is
/// only one way to describe the work, so requiring a skill for every spawn meant every ad-hoc
/// sub-task needed a file written for it first.</para>
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
    /// Runs <paramref name="input"/> in a fresh conversation, optionally pinned to
    /// <paramref name="skillId"/>. Returns the last assistant message produced by the run.
    /// Throws <see cref="System.ArgumentException"/> if a named skill is not found.
    /// </summary>
    public async Task<string> RunAsync(
        string? skillId,
        string input,
        AgentMode mode,
        CancellationToken cancellationToken = default)
    {
        // Refused as text, not as an exception: the caller is a model reading a tool result, and a
        // sentence it can act on beats a stack trace it cannot.
        if (_depth.Value >= MaxDepth)
            return $"error: spawn depth limit reached ({MaxDepth}). " +
                   "A spawned agent cannot keep spawning; do the remaining work in this run.";

        // Fresh isolated agent state — own skill session, working memory, and checkpoint manager.
        // Opening an AgentSessionScope keeps the sub-agent's tool calls (memory, marks, skills) off
        // the parent chat's state, even though the spawn happens inside the parent's async flow.
        var skillSession = new SkillSession();

        // A free-form spawn leaves the session idle rather than pinned. That is not the same as an
        // agent without skills: the session is the sub-agent's own, so if the work turns out to match
        // one, it can find and activate it for itself without touching the parent's.
        if (!string.IsNullOrWhiteSpace(skillId))
        {
            var lookup = _skills.Resolve(skillId!);
            if (lookup.IsAmbiguous)
                throw new System.ArgumentException(
                    $"Skill '{skillId}' is held by more than one source — name one of: " +
                    string.Join(", ", lookup.Candidates.Select(c => c.Address)), nameof(skillId));

            var meta = lookup.Card;
            if (meta is null)
                throw new System.ArgumentException($"Skill '{skillId}' not found.", nameof(skillId));

            var body = _skills.LoadBody(meta.Address);
            if (string.IsNullOrWhiteSpace(body))
                throw new System.ArgumentException(
                    $"Skill '{skillId}' has no readable procedure.", nameof(skillId));

            // Same loan slip as an in-chat activation: a sub-agent running a skill needs that skill's
            // references as much as the parent would, and its own session is the only place to hold them.
            skillSession.Activate(meta.DisplayId, body, meta.SourceId, meta.Ref, _skills.ListResources(meta.Address));
        }

        var checkpoint = new CheckpointManager();
        // Everything else here is deliberately fresh, but the sandbox is inherited: it is the host's
        // boundary, not the agent's state. Left to its default a sub-agent would come out of its
        // parent's sandbox — a way out of the box by spawning, which matters the moment a chat runs
        // with a real one. No parent (a CLI or worker entry point) keeps the constructor's default.
        var agentSession = new AgentSession(
            new KeyValueStore("session"), checkpoint, skillSession,
            sandbox: AgentSessionScope.Current?.Sandbox);

        // The session is passed explicitly rather than resolved ambiently — the spawn happens inside
        // the parent's async flow, and the sub-agent must describe its own skill, not the parent's.
        var composer = new AgentContextComposer(
            AgentContributors.Default(_skills, _plugins, skillSession));
        var systemPrompt = composer.Compose(_settings, _settings.WorkspacePath).SystemPrompt;

        var conversation = new Conversation();
        conversation.Add(new ChatMessage { Role = ChatRole.System, Content = systemPrompt });
        conversation.Add(new ChatMessage { Role = ChatRole.User, Content = input });

        string lastAssistantMessage = string.Empty;

        // A pinned run keeps the prompt frozen: it has one procedure, cannot activate another, and a
        // stray skill_deactivate must not be able to delete the very instructions it was spawned to
        // follow. A free-form run gets the chat's per-iteration recomposition instead — with no skill
        // pinned, activating one mid-run is a legitimate move, and it is worth nothing if the
        // procedure never reaches the prompt.
        var context = skillSession.ActiveSkillId is null
            ? () => composer.Compose(_settings, _settings.WorkspacePath)
            : (Func<ComposedContext>?)null;

        // Spawned sub-agents are the most prone to tool-call loops; guard them too (tool-call only).
        var orchestrator = new ConversationOrchestrator(_llm, _tools)
        {
            Checkpoint = checkpoint,
            EnableLoopGuard = true,
            Context = context
        };
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
