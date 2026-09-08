using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SPLA.Agent;

/// <summary>
/// The single registry of core-feature prompt fragments: feature id → embedded <c>prompt.md</c>
/// resource (each file lives under <c>Features/&lt;FeatureFolder&gt;/</c> next to this class).
/// AgentRuntime uses it to fill <c>IAgentFeature.PromptFragment</c> when building the feature
/// catalog; SystemPromptBuilder's no-features-supplied default path uses it too — so there is
/// exactly one place that knows which feature has prompt text.
/// </summary>
public static class CoreFeaturePrompts
{
    /// <summary>Embedded-resource logical name per feature id. Ids absent from this map
    /// (core.files, core.web, core.spawn, core.correspond, core.clarify) are tools-only features —
    /// <see cref="Load"/> returns null for them. core.correspond has no prompt fragment on purpose:
    /// the ADR explicitly rejects a list of open correspondences in the system prompt
    /// (<c>docs/adr/ADR_20260827-2_core_roles.md</c> §3, "Отвергнуто") — the virtual reply tools'
    /// own names and descriptions play that role instead.</summary>
    private static readonly IReadOnlyDictionary<string, string> ResourceNames =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["core.workspace"] = "SPLA.Agent.Features.CoreWorkspace.prompt.md",
            ["core.discipline"] = "SPLA.Agent.Features.CoreDiscipline.prompt.md",
            ["core.memory"] = "SPLA.Agent.Features.CoreMemory.prompt.md",
            ["core.checkpoints"] = "SPLA.Agent.Features.CoreCheckpoints.prompt.md",
            ["core.skills"] = "SPLA.Agent.Features.CoreSkills.prompt.md",
            ["core.blobs"] = "SPLA.Agent.Features.CoreBlobs.prompt.md",
            ["core.toolsets"] = "SPLA.Agent.Features.CoreToolSets.prompt.md",
            ["core.shell"] = "SPLA.Agent.Features.CoreShell.prompt.md",
            ["core.background_tasks"] = "SPLA.Agent.Features.CoreBackgroundTasks.prompt.md",
            // core.roles carries the text spawn and correspond do not: how to address a role at all,
            // and why a manager delegates the introduction instead of relaying messages himself.
            ["core.roles"] = "SPLA.Agent.Features.CoreRoles.prompt.md",
        };

    private static readonly ConcurrentDictionary<string, string?> Cache = new();

    /// <summary>Loads the prompt fragment for a core feature id, or null when the feature has none
    /// (tools-only). Cached — each resource stream is read at most once per process.</summary>
    public static string? Load(string featureId)
    {
        if (!ResourceNames.TryGetValue(featureId, out var resourceName)) return null;

        return Cache.GetOrAdd(featureId, _ =>
        {
            var assembly = typeof(CoreFeaturePrompts).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd().Trim();
        });
    }
}
