using SPLA.Domain.Agent;
using SPLA.MCP.Core.Composition;
using SPLA.MCP.Core.Skills;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPLA.Agent.Composition;

/// <summary>
/// Everything the skill system puts in front of the model: the running procedure, the bodies of
/// preloaded skills, and the index of what else could be loaded.
///
/// <para>Gated as a whole by <c>core.skills</c> — the composer simply leaves this contributor out
/// when the capability is off, so the prompt never mentions <c>skill_activate</c> without the tool
/// behind it.</para>
/// </summary>
public sealed class SkillsContributor : IAgentContributor
{
    private readonly SkillManager _skills;
    private readonly ISkillSession? _session;

    public SkillsContributor(SkillManager skills, ISkillSession? session = null)
    {
        _skills = skills;
        _session = session;
    }

    public string Id => "skills";

    /// <summary>
    /// The skill session this contribution reflects: the one handed to the constructor, else the
    /// ambient one of the chat currently running.
    ///
    /// <para>The fallback exists because this object is process-wide while a skill session belongs to
    /// a chat — they cannot be tied together by constructor. Resolving through
    /// <see cref="AgentSessionScope"/> is the same ambient pattern the skill tools use, and it is
    /// what makes an activation visible to the very next LLM call: the agent loop recomposes the
    /// surface inside that scope on every iteration. Passing one explicitly still wins, which is how
    /// a spawned sub-agent keeps describing its own skill while running inside the parent's async
    /// flow.</para>
    /// </summary>
    private ISkillSession? Session => _session ?? AgentSessionScope.Current?.Skills;

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var items = new List<ContextItem>();
        var session = Session;

        AppendActiveSkill(items, session);

        // GetAvailable, not "all known": a skill is offered only when its source vouches for it, it
        // is switched on, and every tool it declared is actually registered right now. Anything else
        // stays in the settings panel with a reason and never reaches the model.
        var available = _skills.GetAvailable();
        if (available.Count == 0) return AgentContribution.FromContext(items);

        foreach (var skill in available.Where(s => s.IsPreloaded))
        {
            var body = _skills.LoadBody(skill.Id);
            if (string.IsNullOrEmpty(body)) continue;

            items.Add(new ContextItem
            {
                Source = skill.Id,
                Title = $"Skill: {skill.Id}",
                Body = body,
                Prefix = $"\n\n--- Skill: {skill.Id} ---\n"
            });
        }

        // On-demand index — suppressed while a skill is active, since its body is already injected.
        var onDemand = available.Where(s => !s.IsPreloaded).ToList();
        if (onDemand.Count > 0 && session?.ActiveSkillId is null)
            items.Add(BuildIndex(onDemand));

        return AgentContribution.FromContext(items);
    }

    private void AppendActiveSkill(List<ContextItem> items, ISkillSession? session)
    {
        var activeId = session?.ActiveSkillId;
        if (string.IsNullOrEmpty(activeId)) return;

        // The pinned snapshot, not a fresh read: a skill mid-run keeps the procedure it started with
        // even while its file is edited and the source hot-reloads around it.
        var body = session!.ActiveBody;
        if (string.IsNullOrEmpty(body)) return;

        items.Add(new ContextItem
        {
            Source = activeId,
            Title = $"Active skill: {activeId}",
            Body = body + BuildResourceList(session.ActiveResources),
            Prefix = $"\n\n=== ACTIVE SKILL: {activeId} ===\n",
            Suffix = $"\n=== END ACTIVE SKILL: {activeId} ==="
        });
    }

    /// <summary>
    /// The attachments that came with the active skill, listed by name only.
    ///
    /// <para>Names, not contents: a procedure that opens two references out of fourteen files should
    /// not carry the other twelve in every iteration. But it cannot ask for what it does not know
    /// exists, so the list itself is not optional — it is the catalogue card for the vkladyshi.</para>
    /// </summary>
    private static string BuildResourceList(IReadOnlyList<string> resources)
    {
        if (resources.Count == 0) return string.Empty;

        var body = new StringBuilder();
        body.Append("\n\n--- Resources of this skill ---");
        body.Append("\nRead any of these with skill_read_resource {\"path\": \"<path>\"}. They are available only while this skill is active.");
        foreach (var path in resources) body.Append($"\n  {path}");

        return body.ToString();
    }

    private static ContextItem BuildIndex(IReadOnlyList<SkillMeta> onDemand)
    {
        var body = new StringBuilder();
        body.Append("--- Skills ---");
        body.Append("\nRULE: When the user's request matches a skill listed below, you MUST call skill_activate with its id FIRST — before calling any other tool or executing any step. This is MANDATORY: the full procedure arrives in your next message, and you plan from it.");
        body.Append("\nIf the user asks what you will use, first check whether a listed skill applies, then mention that skill and the relevant tools from its procedure without activating unless execution is requested.");
        body.Append("\nThis rule overrides any plugin instruction that says to 'start immediately'.");
        body.Append("\nSkill descriptions are in English. The user may write in any language — translate the intent to English and match semantically.");
        body.Append("\n");
        body.Append("\nHow to load a skill — call skill_activate with {\"id\": \"<skill-id>\"}");
        body.Append("\nExample: call skill_activate with {\"id\": \"network.range-audit\"}");
        body.Append("\n\nAvailable skills:");
        foreach (var skill in onDemand)
            body.Append($"\n  {skill.Id} — {skill.Description}");

        return new ContextItem
        {
            Source = "index",
            Title = "Skills index",
            Body = body.ToString(),
            Prefix = "\n\n"
        };
    }
}
