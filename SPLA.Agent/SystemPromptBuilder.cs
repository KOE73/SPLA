using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Plugins;
using System.IO;
using System.Linq;
using System.Text;

namespace SPLA.Agent;

/// <summary>
/// Single, layered builder for the agent system prompt. Replaces the two divergent copies that
/// lived in <c>MainWindowViewModel</c> (UI) and <c>Program.cs</c> (CLI). Both entry points now
/// produce the same prompt: mode preamble → instruction files → custom prompt → skills index →
/// plugin prompts → plugin commands.
/// </summary>
public sealed class SystemPromptBuilder
{
    private readonly SkillManager _skills;
    private readonly PluginManager _plugins;

    public SystemPromptBuilder(SkillManager skills, PluginManager plugins)
    {
        _skills = skills;
        _plugins = plugins;
    }

    public string Build(ResolvedSettings settings, string workingDirectory)
    {
        var sb = new StringBuilder();

        sb.Append(ModePreamble(settings.Mode));
        sb.Append("\nYou have access to tools to interact with the file system and run commands. Your current working directory is: ");
        sb.Append(workingDirectory);
        sb.Append("\n\nTool descriptions are intentionally short. Tool flag: [H] = extended help available. If a [H] tool's details are unclear, call agent.info with the tool name before using it. Do not guess complex argument formats.");
        sb.Append("\n\nMermaid note: when writing Mermaid, use valid quoted labels: `NodeId[\"label\"]`, `subgraph Id[\"title\"]`, and `A -->|\"label\"| B` for text with spaces, punctuation, or non-ASCII characters.");
        sb.Append("\n\nIMPORTANT RULE: You may attempt a specific tool a maximum of 3 times. If it fails 3 times, you MUST stop trying and ask the user for help.");
        sb.Append("\n\nAgent memory (agent.memory): your persistent key/value scratchpad across this conversation. Key convention: namespace:name — context:* keys are auto-injected into this prompt every turn. Actions: get|set|delete|list|count|clear. Use clear (with optional filter=) to bulk-delete instead of many individual deletes. For large stores use count first, then list with filter/top/skip (SQL-style pagination) — never call list without filter on an unknown-size store.");

        AppendInstructions(sb, settings, workingDirectory);

        if (!string.IsNullOrWhiteSpace(settings.CustomPrompt))
            sb.Append("\n\n--- Custom Prompt ---\n").Append(settings.CustomPrompt);

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

    private void AppendSkills(StringBuilder sb)
    {
        var enabledSkills = _skills.GetEnabled().ToList();
        if (enabledSkills.Count == 0) return;

        sb.Append("\n\n--- Skills ---");
        sb.Append("\nRULE: When the user's request matches a skill listed below, you MUST call agent.info FIRST — before calling any other tool or executing any step.");
        sb.Append("\nThis rule overrides any plugin instruction that says to 'start immediately'.");
        sb.Append("\nSkill descriptions are in English. The user may write in any language — translate the intent to English and match semantically.");
        sb.Append("\n");
        sb.Append("\nHow to load a skill — call agent.info with {\"id\": \"<skill-id>\"}");
        sb.Append("\nExample: call agent.info with {\"id\": \"network.range-audit\"}");
        sb.Append("\nagent.info works for both tool help AND skill instructions — use it for any capability lookup.");
        sb.Append("\n\nAvailable skills:");
        foreach (var skill in enabledSkills)
        {
            sb.Append($"\n  {skill.Id} — {skill.Description}");
            if (skill.IsPreloaded)
            {
                var body = _skills.LoadBody(skill.Id);
                if (!string.IsNullOrEmpty(body))
                    sb.Append($"\n\n--- Skill: {skill.Id} (preloaded) ---\n").Append(body);
            }
        }
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

        sb.Append("\n\n--- Plugin Commands ---\nUse tool `plugin.command.run` with `commandId` to run these commands. These are commands, not direct tool names.");
        foreach (var command in pluginCommands)
            sb.Append($"\n- {command.Id}: {command.DisplayName} ({command.Kind})");
    }
}
