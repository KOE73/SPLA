using Microsoft.Extensions.Logging;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPLA.Agent.Composition;

/// <summary>The mode preamble — the first and highest-authority line of the prompt.</summary>
public sealed class ModeContributor : IAgentContributor
{
    public string Id => "mode";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var preamble = Preamble(context.Mode);
        return AgentContribution.FromContext(new ContextItem
        {
            Source = context.Mode.ToString(),
            Title = $"Mode: {context.Mode}",
            Body = preamble
        });
    }

    private static string Preamble(AgentMode mode) => mode switch
    {
        AgentMode.Chat => "You are a helpful local AI assistant named SPLA. You are in Chat mode. You should engage in conversation and answer questions.",
        AgentMode.Research => "You are an AI assistant in Research mode. You can read files and search to answer questions, but you cannot modify any files.",
        AgentMode.Inspect => "You are an AI assistant in Inspect mode. You can read files, inspect the system, and run read-only terminal commands.",
        AgentMode.Edit => "You are an AI coding assistant in Edit mode. You MUST proactively use your tools to edit files and write changes to disk rather than just explaining the code. Do not just chat, apply the changes.",
        AgentMode.Agent => "You are a fully autonomous AI Agent. You can read, write, and execute commands without prompting the user. Proactively complete the requested tasks end-to-end.",
        _ => "You are a helpful local AI assistant named SPLA."
    };
}

/// <summary>
/// One item per built-in capability that carries prompt text and is on for the settings being
/// composed for. The features are the same objects whose tools are registered, and the question "is
/// it on" is asked of the same settings <c>McpHost</c> asks when listing and running those tools — a
/// role's own for a role's session — so a capability's text and its tools are switched on and off
/// together. A feature with no fragment (tools-only, e.g. <c>core.files</c>) contributes nothing.
/// </summary>
public sealed class CoreFeatureContributor : IAgentContributor
{
    private const string WorkingDirectoryPlaceholder = "{{workingDirectory}}";
    private readonly IReadOnlyList<IAgentFeature> _features;

    public CoreFeatureContributor(IReadOnlyList<IAgentFeature> features) => _features = features;

    public string Id => "core";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var enabled = AgentFeatureCatalog.EnabledSet(context.Settings.Capabilities);
        var items = new List<ContextItem>();
        foreach (var feature in _features)
        {
            if (!enabled.Contains(feature.Id)) continue;
            if (string.IsNullOrEmpty(feature.PromptFragment)) continue;

            items.Add(new ContextItem
            {
                Source = feature.Id,
                Title = $"Core: {feature.Id}",
                Body = feature.PromptFragment.Replace(WorkingDirectoryPlaceholder, context.WorkingDirectory),
                Prefix = "\n\n"
            });
        }
        return AgentContribution.FromContext(items);
    }
}

/// <summary>
/// The folders this project mounted from outside its root, with what each is for.
///
/// <para>The description is the load-bearing part and is why the manifest refuses a mount without
/// one: an address with no purpose beside it is an invitation to open the folder and find out. Access
/// and availability are here for the same reason — a model that knows a mount is read-only does not
/// spend a turn discovering it, and one that knows a mount is unplugged reports that instead of
/// hunting for a missing file.</para>
///
/// <para>Contributes nothing when the project declared none, which is most projects.</para>
/// </summary>
public sealed class MountsContributor : IAgentContributor
{
    public string Id => "mounts";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var mounts = context.Settings.Mounts;
        if (mounts.Count == 0) return AgentContribution.FromContext([]);

        var lines = mounts.Select(m =>
        {
            var notes = m.Access == MountAccess.Write ? "writable" : "read-only";
            if (m.Trust == MountTrust.Untrusted) notes += ", untrusted — others write here";
            if (!m.IsAvailable) notes += ", NOT CONNECTED on this machine";
            return $"- {m.Address()}/ ({notes}) — {m.Description}";
        });

        return AgentContribution.FromContext([new ContextItem
        {
            Source = "mounts",
            Title = "Mounted folders",
            Prefix = "\n\n",
            Body =
                "Folders outside this project, declared in its manifest and addressed under " +
                $"`{ProjectMount.Prefix}/`. Use these addresses with the file tools exactly as you use " +
                "project paths; never substitute a path from this machine, because the address is what " +
                "is portable and the machine path is not.\n\n" +
                string.Join("\n", lines)
        }]);
    }
}

/// <summary>The project's instruction files, in the order the settings list them. A file that is not
/// there contributes nothing — the list is a wish, not a manifest.
///
/// <para>An entry named <c>AGENTS.md</c> (any casing, any subdirectory) is skipped with a logged
/// warning instead of being read: that file's own mechanism is <see cref="ProjectAgentsContributor"/>
/// (<c>agent.agents_md</c>), and reading it here too would put its content in the prompt twice. See
/// <c>ADR_20260911-2_agent_agents-md-scopes.md</c> §2.2.</para>
/// </summary>
public sealed class InstructionsContributor(ILogger<InstructionsContributor>? logger = null) : IAgentContributor
{
    public string Id => "instructions";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var items = new List<ContextItem>();
        foreach (var instructionPath in context.Settings.Instructions)
        {
            if (string.Equals(Path.GetFileName(instructionPath), "AGENTS.md", StringComparison.OrdinalIgnoreCase))
            {
                logger?.LogWarning(
                    "instructions: lists '{Path}', which is an AGENTS.md file. It is skipped here — " +
                    "AGENTS.md reaches the prompt only through the agent.agents_md mechanism, never " +
                    "through instructions:, to avoid including it twice.", instructionPath);
                continue;
            }

            var fullPath = Path.GetFullPath(Path.Combine(context.WorkingDirectory, instructionPath));
            if (!File.Exists(fullPath)) continue;

            items.Add(new ContextItem
            {
                Source = instructionPath,
                Title = $"Instructions: {instructionPath}",
                Body = File.ReadAllText(fullPath),
                Prefix = $"\n\n--- Instructions from {instructionPath} ---\n"
            });
        }
        return AgentContribution.FromContext(items);
    }
}

/// <summary>The project's root <c>AGENTS.md</c> (the file at <c>&lt;project root&gt;/AGENTS.md</c>,
/// where project root is the directory containing <c>.spla</c> — <see cref="ResolvedSettings.WorkspacePath"/>,
/// the same root the zones model uses). Nested, per-folder AGENTS.md files are a later wave
/// (<c>ResolvedScopedAgents</c> in the ADR) and are not this contributor's job.
///
/// <para>Active only when <see cref="AgentsMdMode.Inject"/> is resolved. When active it always emits
/// the <c>&lt;agents&gt;</c> semantics declaration, even with no root file to show — the declaration
/// must be present from the first turn so a later nested block does not change more than the tail of
/// the prompt (ADR §2.4, note 1). When <see cref="AgentsMdMode.Ignore"/> is resolved, this contributor
/// emits nothing at all, not even the declaration.</para>
///
/// <para>See <c>ADR_20260911-2_agent_agents-md-scopes.md</c> §2.2/§2.4.</para>
/// </summary>
public sealed class ProjectAgentsContributor : IAgentContributor
{
    public string Id => "project-agents";

    private const string Declaration =
        "Project instructions may appear below as <agents> blocks.\n" +
        "Each block is authoritative for its declared scope (a folder and everything under it).\n" +
        "A narrower scope overrides a broader one on conflict.\n" +
        "Sibling scopes are independent: a block for one folder says nothing about another.";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        if (context.Settings.AgentsMd != AgentsMdMode.Inject) return AgentContribution.None;

        var items = new List<ContextItem>
        {
            new()
            {
                Source = "agents-md-semantics",
                Title = "AGENTS.md semantics",
                Body = Declaration,
                Prefix = "\n\n"
            }
        };

        var rootAgentsPath = Path.GetFullPath(Path.Combine(context.Settings.WorkspacePath, "AGENTS.md"));
        if (File.Exists(rootAgentsPath))
        {
            items.Add(new ContextItem
            {
                Source = "AGENTS.md",
                Title = "AGENTS.md (project root)",
                Prefix = "\n\n<agents scope=\"\" source=\"AGENTS.md\">\n",
                Body = File.ReadAllText(rootAgentsPath),
                Suffix = "\n</agents>"
            });
        }

        return AgentContribution.FromContext(items);
    }
}

/// <summary>The user's own prompt text from settings.</summary>
public sealed class CustomPromptContributor : IAgentContributor
{
    public string Id => "custom-prompt";

    public AgentContribution Contribute(AgentContributionContext context)
        => string.IsNullOrWhiteSpace(context.Settings.CustomPrompt)
            ? AgentContribution.None
            : AgentContribution.FromContext(new ContextItem
            {
                Source = "custom",
                Title = "Custom prompt",
                Body = context.Settings.CustomPrompt,
                Prefix = "\n\n--- Custom Prompt ---\n"
            });
}
