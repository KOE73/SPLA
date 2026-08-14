using SPLA.MCP.Core.Composition;

namespace SPLA.CLI.Batch;

/// <summary>
/// Everything a CLI invocation adds to the prompt on top of the project's own surface — <c>--sys-prompt</c>,
/// the <c>--md-clean</c> directive, and anything else a future flag wants to say. Purely additive: it
/// never touches <c>settings.CustomPrompt</c>, so the project's own prompt (there for a reason) is
/// never clobbered by a one-off batch run.
/// <para>
/// Handed to <c>AgentRuntime</c> at construction as a host contributor; the slot it lands in — right
/// after <c>custom-prompt</c>, before skills/toolsets/plugins/memory — is chosen by
/// <c>AgentContributors.Default</c>, which owns prompt order. This class supplies text, never a
/// position: see its <c>hostExtras</c> parameter for why the split matters.
/// </para>
/// </summary>
public sealed class CliContributor : IAgentContributor
{
    private readonly List<ContextItem> _items = [];

    public string Id => "cli";

    public void AddText(string source, string title, string body) =>
        _items.Add(new ContextItem
        {
            Source = source,
            Title = title,
            Body = body,
            Prefix = $"\n\n--- {title} ---\n"
        });

    public AgentContribution Contribute(AgentContributionContext context) =>
        AgentContribution.FromContext(_items);
}
