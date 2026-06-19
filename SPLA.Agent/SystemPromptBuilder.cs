using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Plugins;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SPLA.Agent;

/// <summary>
/// Single, layered builder for the agent system prompt. Assembly order (top = highest authority):
/// mode preamble → global rules → instruction files → custom prompt → active skill body →
/// plugin prompts → plugin commands → skills index (on-demand list).
/// </summary>
public sealed class SystemPromptBuilder
{
    private const string MainPromptResourceName = "SPLA.Agent.main_sys_prompt.md";
    private const string WorkingDirectoryPlaceholder = "{{workingDirectory}}";
    private static readonly Lazy<string> MainPromptTemplate = new(LoadMainPromptTemplate);

    private readonly SkillManager _skills;
    private readonly PluginManager _plugins;
    private readonly ISkillSession? _skillSession;

    public SystemPromptBuilder(SkillManager skills, PluginManager plugins, ISkillSession? skillSession = null)
    {
        _skills = skills;
        _plugins = plugins;
        _skillSession = skillSession;
    }

    public string Build(ResolvedSettings settings, string workingDirectory)
    {
        var sb = new StringBuilder();

        sb.Append(ModePreamble(settings.Mode));
        sb.Append('\n').Append(BuildMainPrompt(workingDirectory));

        AppendInstructions(sb, settings, workingDirectory);

        if (!string.IsNullOrWhiteSpace(settings.CustomPrompt))
            sb.Append("\n\n--- Custom Prompt ---\n").Append(settings.CustomPrompt);

        AppendActiveSkill(sb);
        AppendSkills(sb);
        AppendPluginPrompts(sb);
        AppendPluginCommands(sb);

        return sb.ToString();
    }

    private static string ModePreamble(AgentMode mode) => mode switch
    {
        AgentMode.Chat => "You are a helpful local AI assistant named SPLA. You are in Chat mode. You should engage in conversation and answer questions.",
        AgentMode.Research => "You are an AI assistant in Research mode. You can read files and search to answer questions, but you cannot modify any files.",
        AgentMode.Inspect => "You are an AI assistant in Inspect mode. You can read files, inspect the system, and run read-only terminal commands.",
        AgentMode.Edit => "You are an AI coding assistant in Edit mode. You MUST proactively use your tools to edit files and write changes to disk rather than just explaining the code. Do not just chat, apply the changes.",
        AgentMode.Agent => "You are a fully autonomous AI Agent. You can read, write, and execute commands without prompting the user. Proactively complete the requested tasks end-to-end.",
        _ => "You are a helpful local AI assistant named SPLA."
    };

    private static string BuildMainPrompt(string workingDirectory)
        => MainPromptTemplate.Value.Replace(WorkingDirectoryPlaceholder, workingDirectory);

    private static string LoadMainPromptTemplate()
    {
        var assembly = typeof(SystemPromptBuilder).Assembly;
        using var stream = assembly.GetManifestResourceStream(MainPromptResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{MainPromptResourceName}' was not found.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd().Trim();
    }

    private static void AppendInstructions(StringBuilder sb, ResolvedSettings settings, string workingDirectory)
    {
        foreach (var instrPath in settings.Instructions)
        {
            var fullPath = Path.GetFullPath(Path.Combine(workingDirectory, instrPath));
            if (File.Exists(fullPath))
            {
                var content = File.ReadAllText(fullPath);
                sb.Append($"\n\n--- Instructions from {instrPath} ---\n").Append(content);
            }
        }
    }

    private void AppendActiveSkill(StringBuilder sb)
    {
        var activeId = _skillSession?.ActiveSkillId;
        if (string.IsNullOrEmpty(activeId)) return;

        var body = _skills.LoadBody(activeId);
        if (string.IsNullOrEmpty(body)) return;

        sb.Append($"\n\n=== ACTIVE SKILL: {activeId} ===\n");
        sb.Append(body);
        sb.Append($"\n=== END ACTIVE SKILL: {activeId} ===");
    }

    private void AppendSkills(StringBuilder sb)
    {
        var enabledSkills = _skills.GetEnabled().ToList();
        if (enabledSkills.Count == 0) return;

        var preloaded    = enabledSkills.Where(s => s.IsPreloaded).ToList();
        var onDemand     = enabledSkills.Where(s => !s.IsPreloaded).ToList();

        // ── Preloaded skills: inject body directly, no agent_info needed ──
        foreach (var skill in preloaded)
        {
            var body = _skills.LoadBody(skill.Id);
            if (!string.IsNullOrEmpty(body))
                sb.Append($"\n\n--- Skill: {skill.Id} ---\n").Append(body);
        }

        // ── On-demand skills: list + agent_info load rule ──
        // Suppressed while a skill is active — the active skill body is already injected above.
        if (onDemand.Count == 0 || _skillSession?.ActiveSkillId is not null) return;

        sb.Append("\n\n--- Skills ---");
        sb.Append("\nRULE: When the user's request matches a skill listed below, you MUST call agent_info FIRST — before calling any other tool or executing any step.");
        sb.Append("\nRULE: After agent_info, you MUST call skill_activate with the skill id BEFORE any task tool call. This is MANDATORY — do NOT skip it, do NOT proceed to task tools without it. agent_info alone only previews/loads instructions and does not make the skill active.");
        sb.Append("\nIf the user asks what you will use, first check whether a listed skill applies, then mention that skill and the relevant tools from its procedure without activating unless execution is requested.");
        sb.Append("\nThis rule overrides any plugin instruction that says to 'start immediately'.");
        sb.Append("\nSkill descriptions are in English. The user may write in any language — translate the intent to English and match semantically.");
        sb.Append("\n");
        sb.Append("\nHow to load a skill — call agent_info with {\"id\": \"<skill-id>\"}");
        sb.Append("\nExample: call agent_info with {\"id\": \"network.range-audit\"}");
        sb.Append("\nagent_info works for both tool help AND skill instructions — use it for any capability lookup.");
        sb.Append("\n\nAvailable skills:");
        foreach (var skill in onDemand)
            sb.Append($"\n  {skill.Id} — {skill.Description}");
    }

    private void AppendPluginPrompts(StringBuilder sb)
    {
        foreach (var plugin in _plugins.GetActivePlugins())
        {
            if (!string.IsNullOrWhiteSpace(plugin.EffectivePrompt))
                sb.Append($"\n\n--- Plugin: {plugin.Meta.Id} ---\n").Append(plugin.EffectivePrompt);
        }
    }

    private void AppendPluginCommands(StringBuilder sb)
    {
        var pluginCommands = _plugins.GetUiCommands();
        if (pluginCommands.Count == 0) return;

        sb.Append("\n\n--- Plugin Commands ---\nUse tool `plugin_run_command` with `commandId` to run these commands. These are commands, not direct tool names.");
        foreach (var command in pluginCommands)
            sb.Append($"\n- {command.Id}: {command.DisplayName} ({command.Kind})");
    }
}
