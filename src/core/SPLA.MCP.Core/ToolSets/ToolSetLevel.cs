namespace SPLA.MCP.Core.ToolSets;

/// <summary>
/// How much of a tool set reaches the model before it is needed. This replaces the plugin's
/// two-state on/off flag as the unit of exposure: the question a level answers is not "may these
/// tools run" but "does the model know this set exists, and at what price in context".
///
/// <para>Two axes are folded into one ladder here — <b>who activates</b> and <b>how much context</b>
/// — and they happen to order the same way. <see cref="SkillDemand"/> and <see cref="AgentDemand"/>
/// are not "more and less of the same thing": a skill carries the set in its own requirements, so
/// nothing needs to be announced to the model at all, while the agent picks the set itself and
/// therefore has to be told the set exists. A fifth level (say, "raised by the user from the UI")
/// would split the two axes apart again — see IDEA_20260802_core §6.2 before adding one.</para>
/// </summary>
public enum ToolSetLevel
{
    /// <summary>Forbidden. The set does not exist for the model — no declaration, no definitions,
    /// no execution, and nothing describes it.</summary>
    Disabled,

    /// <summary>Allowed, but only for a foreseen need: a skill that declares the set in its
    /// requirements raises it on activation. Costs zero context until that happens.</summary>
    SkillDemand,

    /// <summary>Allowed, and the agent may raise it itself. Costs one declaration line — the price
    /// of the case nobody wrote a skill for.</summary>
    AgentDemand,

    /// <summary>Always disclosed: full definitions of every tool in the set, in every request.</summary>
    Enabled
}

/// <summary>Where a tool set came from. Provenance only — it takes no part in the decision to show
/// the set to the model (that is the level's job, see <see cref="ToolSetLevel"/>).</summary>
public enum ToolSetOrigin
{
    /// <summary>A built-in agent capability (<c>IAgentFeature</c>).</summary>
    Core,

    /// <summary>Shipped by a plugin assembly.</summary>
    Plugin
}
