using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SPLA.Agent;

/// <summary>Origin of a single system-prompt segment. Drives the colour grouping in the prompt preview.</summary>
public enum PromptSegmentKind
{
    Mode,
    Core,
    Instructions,
    Custom,
    ActiveSkill,
    Skill,
    SkillsIndex,
    Plugin,
    PluginCommands
}

/// <summary>One layer of the assembled system prompt. <see cref="Body"/> is the clean content for display;
/// <see cref="Text"/> is the exact contribution to the final prompt (separators + machine header included),
/// so concatenating every segment's <see cref="Text"/> reproduces the prompt byte-for-byte.</summary>
public sealed record PromptSegment(PromptSegmentKind Kind, string Title, string Body, string Text);

/// <summary>
/// Single, layered builder for the agent system prompt. Assembly order (top = highest authority):
/// mode preamble → core → instruction files → custom prompt → active skill body →
/// preloaded skills → skills index → plugin prompts → plugin commands.
///
/// <para>The builder produces an intermediate <see cref="PromptSegment"/> list — the single source of truth.
/// <see cref="Build"/> flattens it for the LLM; the UI renders the same segments as coloured blocks.</para>
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

    /// <summary>Flatten the segments into the final system prompt string sent to the LLM.</summary>
    public string Build(ResolvedSettings settings, string workingDirectory)
        => string.Concat(BuildSegments(settings, workingDirectory).Select(s => s.Text));

    /// <summary>The intermediate, source-tagged segments. Both <see cref="Build"/> and the prompt preview
    /// derive from this list, so what you see is exactly what is sent.</summary>
    public IReadOnlyList<PromptSegment> BuildSegments(ResolvedSettings settings, string workingDirectory)
    {
        var segments = new List<PromptSegment>();

        var preamble = ModePreamble(settings.Mode);
        segments.Add(new(PromptSegmentKind.Mode, $"Mode: {settings.Mode}", preamble, preamble));

        var core = BuildMainPrompt(workingDirectory);
        segments.Add(new(PromptSegmentKind.Core, "Core instructions", core, "\n" + core));

        AppendInstructions(segments, settings, workingDirectory);

        if (!string.IsNullOrWhiteSpace(settings.CustomPrompt))
            segments.Add(new(PromptSegmentKind.Custom, "Custom prompt", settings.CustomPrompt,
                "\n\n--- Custom Prompt ---\n" + settings.CustomPrompt));

        AppendActiveSkill(segments);
        AppendSkills(segments);
        AppendPluginPrompts(segments);
        AppendPluginCommands(segments);

        return segments;
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

    private static void AppendInstructions(List<PromptSegment> segments, ResolvedSettings settings, string workingDirectory)
    {
        foreach (var instrPath in settings.Instructions)
        {
            var fullPath = Path.GetFullPath(Path.Combine(workingDirectory, instrPath));
            if (File.Exists(fullPath))
            {
                var content = File.ReadAllText(fullPath);
                segments.Add(new(PromptSegmentKind.Instructions, $"Instructions: {instrPath}", content,
                    $"\n\n--- Instructions from {instrPath} ---\n" + content));
            }
        }
    }

    private void AppendActiveSkill(List<PromptSegment> segments)
    {
        var activeId = _skillSession?.ActiveSkillId;
        if (string.IsNullOrEmpty(activeId)) return;

        var body = _skills.LoadBody(activeId);
        if (string.IsNullOrEmpty(body)) return;

        var text = $"\n\n=== ACTIVE SKILL: {activeId} ===\n" + body + $"\n=== END ACTIVE SKILL: {activeId} ===";
        segments.Add(new(PromptSegmentKind.ActiveSkill, $"Active skill: {activeId}", body, text));
    }

    private void AppendSkills(List<PromptSegment> segments)
    {
        var enabledSkills = _skills.GetEnabled().ToList();
        if (enabledSkills.Count == 0) return;

        var preloaded = enabledSkills.Where(s => s.IsPreloaded).ToList();
        var onDemand  = enabledSkills.Where(s => !s.IsPreloaded).ToList();

        // ── Preloaded skills: inject body directly, no agent_info needed ──
        foreach (var skill in preloaded)
        {
            var body = _skills.LoadBody(skill.Id);
            if (!string.IsNullOrEmpty(body))
                segments.Add(new(PromptSegmentKind.Skill, $"Skill: {skill.Id}", body,
                    $"\n\n--- Skill: {skill.Id} ---\n" + body));
        }

        // ── On-demand skills: list + agent_info load rule ──
        // Suppressed while a skill is active — the active skill body is already injected above.
        if (onDemand.Count == 0 || _skillSession?.ActiveSkillId is not null) return;

        var sb = new StringBuilder();
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

        var text = sb.ToString();
        segments.Add(new(PromptSegmentKind.SkillsIndex, "Skills index", text.TrimStart('\n'), text));
    }

    private void AppendPluginPrompts(List<PromptSegment> segments)
    {
        foreach (var plugin in _plugins.GetActivePlugins())
        {
            if (!string.IsNullOrWhiteSpace(plugin.EffectivePrompt))
                segments.Add(new(PromptSegmentKind.Plugin, $"Plugin: {plugin.Meta.Id}", plugin.EffectivePrompt,
                    $"\n\n--- Plugin: {plugin.Meta.Id} ---\n" + plugin.EffectivePrompt));
        }
    }

    private void AppendPluginCommands(List<PromptSegment> segments)
    {
        var pluginCommands = _plugins.GetUiCommands();
        if (pluginCommands.Count == 0) return;

        var sb = new StringBuilder();
        sb.Append("\n\n--- Plugin Commands ---\nUse tool `plugin_run_command` with `commandId` to run these commands. These are commands, not direct tool names.");
        foreach (var command in pluginCommands)
            sb.Append($"\n- {command.Id}: {command.DisplayName} ({command.Kind})");

        var text = sb.ToString();
        segments.Add(new(PromptSegmentKind.PluginCommands, "Plugin commands", text.TrimStart('\n'), text));
    }
}
