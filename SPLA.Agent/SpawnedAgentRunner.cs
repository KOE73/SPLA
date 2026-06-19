using SPLA.Domain.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Plugins;
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
    private readonly ILLMService _llm;
    private readonly Domain.Interfaces.IToolHost _tools;
    private readonly SkillManager _skills;
    private readonly PluginManager _plugins;
    private readonly ResolvedSettings _settings;

    public SpawnedAgentRunner(
        ILLMService llm,
        Domain.Interfaces.IToolHost tools,
        SkillManager skills,
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
        if (_skills.Find(skillId) is null)
            throw new System.ArgumentException($"Skill '{skillId}' not found.", nameof(skillId));

        // Fresh isolated skill session — does not affect the parent session.
        var session = new SkillSession();
        session.Activate(skillId);

        var promptBuilder = new SystemPromptBuilder(_skills, _plugins, session);
        var systemPrompt = promptBuilder.Build(_settings, System.IO.Directory.GetCurrentDirectory());

        var conversation = new Conversation();
        conversation.Add(new ChatMessage { Role = ChatRole.System, Content = systemPrompt });
        conversation.Add(new ChatMessage { Role = ChatRole.User, Content = input });

        string lastAssistantMessage = string.Empty;

        var orchestrator = new ConversationOrchestrator(_llm, _tools);
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

        await orchestrator.RunAsync(conversation, llmSettings, mode, callbacks, cancellationToken);

        return lastAssistantMessage;
    }
}
