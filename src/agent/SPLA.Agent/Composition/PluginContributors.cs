using SPLA.MCP.Core.Composition;
using SPLA.MCP.Core.Plugins;
using System.Collections.Generic;
using System.Text;

namespace SPLA.Agent.Composition;

/// <summary>Each enabled plugin's own prompt text, one item per plugin.</summary>
public sealed class PluginPromptContributor : IAgentContributor
{
    private readonly PluginManager _plugins;

    public PluginPromptContributor(PluginManager plugins) => _plugins = plugins;

    public string Id => "plugins";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var items = new List<ContextItem>();
        foreach (var plugin in _plugins.GetActivePlugins())
        {
            // Live gating: a plugin disabled after startup keeps its assembly loaded but must not
            // speak in the prompt (McpHost filters its tools the same way).
            if (!_plugins.IsPluginEnabled(plugin.Meta.Id)) continue;
            if (string.IsNullOrWhiteSpace(plugin.EffectivePrompt)) continue;

            items.Add(new ContextItem
            {
                Source = plugin.Meta.Id,
                Title = $"Plugin: {plugin.Meta.Id}",
                Body = plugin.EffectivePrompt,
                Prefix = $"\n\n--- Plugin: {plugin.Meta.Id} ---\n"
            });
        }
        return AgentContribution.FromContext(items);
    }
}

/// <summary>The list of plugin UI commands the model may run through <c>plugin_run_command</c>.</summary>
public sealed class PluginCommandContributor : IAgentContributor
{
    private readonly PluginManager _plugins;

    public PluginCommandContributor(PluginManager plugins) => _plugins = plugins;

    public string Id => "plugin-commands";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var commands = _plugins.GetUiCommands();
        if (commands.Count == 0) return AgentContribution.None;

        var body = new StringBuilder();
        body.Append("--- Plugin Commands ---\nUse tool `plugin_run_command` with `commandId` to run these commands. These are commands, not direct tool names.");
        foreach (var command in commands)
            body.Append($"\n- {command.Id}: {command.DisplayName} ({command.Kind})");

        return AgentContribution.FromContext(new ContextItem
        {
            Source = "commands",
            Title = "Plugin commands",
            Body = body.ToString(),
            Prefix = "\n\n"
        });
    }
}
