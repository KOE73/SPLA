using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.MCP.Core.Agent;

/// <summary>
/// Canonical order and dependency graph of the built-in agent capability ids ("core.*"). Pure
/// metadata — no tool or prompt content — so both the tool-registration side (AgentRuntime, which
/// knows how to build each feature's concrete <see cref="IMcpTool"/> instances) and the prompt side
/// (SystemPromptBuilder, which knows each feature's prompt fragment text) resolve the same
/// <c>agent.capabilities</c> setting into the same enabled-id set, in the same order, once.
/// </summary>
public static class AgentFeatureCatalog
{
    /// <summary>All known feature ids, in the order features are registered/rendered.</summary>
    public static readonly IReadOnlyList<string> Order = new[]
    {
        "core.workspace",
        "core.discipline",
        "core.files",
        "core.shell",
        "core.web",
        "core.memory",
        "core.checkpoints",
        "core.skills",
        "core.toolsets",
        "core.spawn",
        "core.clarify",
        "core.blobs",
        "core.background_tasks",
    };

    private static readonly Dictionary<string, string[]> RequiresMap = new(System.StringComparer.Ordinal)
    {
        ["core.checkpoints"] = new[] { "core.memory" },
    };

    /// <summary>Ids another feature depends on, or empty when it has none.</summary>
    public static IReadOnlyList<string> RequiresOf(string id)
        => RequiresMap.TryGetValue(id, out var r) ? r : System.Array.Empty<string>();

    /// <summary>Short A2-English blurb plus the literal tool names, for the settings panel. Kept here
    /// next to <see cref="Order"/> so a new tool added to a feature is a one-line reminder to update
    /// this too.</summary>
    private static readonly Dictionary<string, string> DescriptionMap = new(System.StringComparer.Ordinal)
    {
        ["core.workspace"] = "Tells the agent about the project and the current date and time.\nget_context get_current_date_time",
        ["core.discipline"] = "Ground rules for how the agent behaves. No tools of its own.",
        ["core.files"] = "Read, search, and change files on disk.\nfs_list fs_read fs_search_text fs_find_files fs_create fs_patch fs_write fs_delete image_view",
        ["core.shell"] = "Run a shell command, and answer it if it asks something.\nsystem_run_shell system_resume_shell system_kill_shell",
        ["core.web"] = "Fetch a web page.\nweb_fetch",
        ["core.memory"] = "Save, read, list, and delete small pieces of memory for this project.\nagent_memory_set agent_memory_get agent_memory_delete agent_memory_list agent_memory_clear",
        ["core.checkpoints"] = "Save and restore a point in the conversation to go back to.\ncontext_checkpoint_set context_checkpoint_restore mark_set mark_rollback",
        ["core.skills"] = "Turn skills on or off and read their files.\nskill_activate skill_deactivate skill_read_resource skill_find",
        ["core.toolsets"] = "Turn a group of tools on or off.\ntoolset_activate toolset_deactivate",
        ["core.spawn"] = "Start one or many sub-agents to do a task.\nagent_spawn agent_spawn_batch",
        ["core.clarify"] = "Ask the user a question and wait for the answer.\nagent_clarify",
        ["core.blobs"] = "Look at a piece of data stored outside the chat.\nblob_peek",
        ["core.background_tasks"] = "See, read, and cancel calls running detached from the turn (background: true).\ntask_list task_output task_cancel",
    };

    /// <summary>Human-readable blurb for a feature id, or null if none is defined.</summary>
    public static string? DescriptionOf(string id)
        => DescriptionMap.TryGetValue(id, out var d) ? d : null;

    /// <summary>
    /// Resolves the <c>agent.capabilities</c> project setting against the canonical catalog:
    /// <list type="bullet">
    /// <item><c>null</c> configured list → every known feature is enabled (full backward compatibility).</item>
    /// <item>empty list → no feature is enabled.</item>
    /// <item>unknown id → dropped, with a warning logged.</item>
    /// <item>a feature's <see cref="RequiresOf"/> deps are auto-included transitively, with an info log.</item>
    /// </list>
    /// Returns the enabled ids in canonical <see cref="Order"/>, regardless of the input order.
    /// </summary>
    public static IReadOnlyList<string> Resolve(IReadOnlyList<string>? configured, ILogger? logger = null)
    {
        if (configured == null) return Order;

        var known = new HashSet<string>(Order, System.StringComparer.Ordinal);
        var enabled = new HashSet<string>(System.StringComparer.Ordinal);
        var queue = new Queue<string>();

        foreach (var id in configured)
        {
            if (!known.Contains(id))
            {
                logger?.LogWarning("Unknown agent capability id ignored: {Id}", id);
                continue;
            }
            if (enabled.Add(id)) queue.Enqueue(id);
        }

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            foreach (var dep in RequiresOf(id))
            {
                if (!enabled.Add(dep)) continue;
                logger?.LogInformation("Auto-enabling required capability {Dep} for {Id}", dep, id);
                queue.Enqueue(dep);
            }
        }

        return Order.Where(enabled.Contains).ToList();
    }
}
