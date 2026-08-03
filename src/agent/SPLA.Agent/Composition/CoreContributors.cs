using SPLA.Domain.Models;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Composition;
using System.Collections.Generic;
using System.IO;

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
