using SPLA.Domain.Agent;
using SPLA.MCP.Core.Composition;
using SPLA.Library;
using SPLA.Library.Catalog;
using SPLA.Library.Sources;
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
    private readonly SkillLibrary _skills;
    private readonly ISkillSession? _session;
    private readonly int _shelfLimit;

    public SkillsContributor(SkillLibrary skills, ISkillSession? session = null,
        int shelfLimit = CatalogView.DefaultShelfLimit)
    {
        _skills = skills;
        _session = session;
        _shelfLimit = shelfLimit;
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

        // Catalog, not the whole holdings: a skill is offered only when its source vouches for it, it
        // is switched on, and every tool it declared is actually registered right now. Anything else
        // stays in the settings panel with a reason and never reaches the model.
        var available = _skills.Catalog();
        if (available.Count == 0) return AgentContribution.FromContext(items);

        // A preloaded skill from a source the model is not supposed to know about would be the
        // loudest possible way of telling it — the whole body, unasked. Level outranks the flag.
        foreach (var skill in available.Where(s => s.IsPreloaded && Announceable(s)))
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

        // Suppressed while a skill is active, since its body is already injected and a second
        // activation is refused anyway.
        if (session?.ActiveSkillId is not null) return AgentContribution.FromContext(items);

        var view = CatalogView.Build(available.Where(s => !s.IsPreloaded), _shelfLimit);
        if (!view.IsEmpty) items.Add(BuildIndex(view));

        return AgentContribution.FromContext(items);
    }

    private static bool Announceable(SkillCard card) =>
        card.Level is SourceLevel.InCatalog or SourceLevel.OnShelf;

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

    /// <summary>
    /// The skills section: the listed shelf, the tag cloud, or both.
    ///
    /// <para>The cloud is printed with counts. A bare word list would leave the model guessing whether
    /// a subject is one skill or forty, and that is exactly the judgement it needs in order to decide
    /// whether asking is worth a turn.</para>
    /// </summary>
    private static ContextItem BuildIndex(CatalogView view)
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

        if (view.Shelf.Count > 0)
        {
            body.Append("\n\nAvailable skills:");
            foreach (var skill in view.Shelf)
                body.Append($"\n  {skill.Id} — {skill.Description}");
        }

        if (!view.Cloud.IsEmpty)
        {
            body.Append($"\n\nFurther skills are catalogued by subject, not listed here ({view.CloudedSkills.Count} of them). Subjects, with how many skills carry each:");
            foreach (var tag in view.Cloud.All())
                body.Append($"\n  {tag.Tag} ({tag.Count})");

            // The two-step selection is the one new way this can fail: a model that never asks simply
            // does not see most of the fond. Spelling out the sequence, with the words to use, is the
            // cheapest thing that closes it.
            body.Append("\nThose skills are NOT listed above and you cannot name them yet. To use one:");
            body.Append("\n  1. call skill_find with {\"tags\": [\"<subject>\"]} — subjects come from the list just above;");
            body.Append("\n  2. it answers with skill ids and descriptions;");
            body.Append("\n  3. call skill_activate with the id you chose.");
            body.Append("\nMANDATORY: if the request matches one of those subjects, call skill_find BEFORE doing the work yourself or saying no skill exists. Never guess a skill id — activating an id you did not read from skill_find will fail.");
            body.Append("\nIf no subject fits, call skill_find with {\"text\": \"<what the user wants>\"} instead.");
        }

        return new ContextItem
        {
            Source = "index",
            Title = "Skills index",
            Body = body.ToString(),
            Prefix = "\n\n"
        };
    }
}
