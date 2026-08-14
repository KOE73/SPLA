using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Composition;
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
        var preamble = Preamble(context.Settings.Mode);
        return AgentContribution.FromContext(new ContextItem
        {
            Source = context.Settings.Mode.ToString(),
            Title = $"Mode: {context.Settings.Mode}",
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
/// One item per enabled built-in capability that carries prompt text. The features are the same
/// objects whose tools were registered, so a capability's text and its tools are switched on and off
/// together — a feature with no fragment (tools-only, e.g. <c>core.files</c>) contributes nothing.
/// </summary>
public sealed class CoreFeatureContributor : IAgentContributor
{
    private const string WorkingDirectoryPlaceholder = "{{workingDirectory}}";
    private readonly IReadOnlyList<IAgentFeature> _features;

    public CoreFeatureContributor(IReadOnlyList<IAgentFeature> features) => _features = features;

    public string Id => "core";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var items = new List<ContextItem>();
        foreach (var feature in _features)
        {
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
/// there contributes nothing — the list is a wish, not a manifest.</summary>
public sealed class InstructionsContributor : IAgentContributor
{
    public string Id => "instructions";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var items = new List<ContextItem>();
        foreach (var instructionPath in context.Settings.Instructions)
        {
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
