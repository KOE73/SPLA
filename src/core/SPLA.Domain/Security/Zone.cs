using System;

namespace SPLA.Domain.Security;

/// <summary>
/// One perimeter, as an instance rather than a category. <c>sql:prod</c> and <c>sql:test</c> are two
/// zones, not two uses of one — a permission given for the test database must not cover the live
/// one, and a grant that is broader than the promise a person made is a grant they will switch off.
///
/// <para>Two unrelated relations put zones where they are, and mixing them is what broke the single
/// <c>ToolScope</c> axis: <b>containment</b> (the project is physically inside the machine) and
/// <b>reachability</b> (an island is reached only through a configured connection, and is contained
/// by nothing).</para>
/// </summary>
/// <param name="Kind">Family: <c>project</c>, <c>machine</c>, <c>context</c>, <c>agent</c>,
/// <c>web</c>, <c>sql</c>, <c>ssh</c>, <c>llm</c>, <c>any</c>.</param>
/// <param name="Instance">Which one, for kinds that have more than one. Empty for the singletons.</param>
public readonly record struct Zone(string Kind, string Instance = "")
{
    /// <summary>Inside the project root. What the agent is normally for.</summary>
    public static readonly Zone Project = new("project");

    /// <summary>The filesystem outside the root. Still this machine, still the operator's.</summary>
    public static readonly Zone Machine = new("machine");

    /// <summary>The model's context. Not a place data rests but one it passes through — and
    /// everything that passes through it has been shown to whichever model island is answering.</summary>
    public static readonly Zone Context = new("context");

    /// <summary>The agent's own state: working memory, checkpoints, marks.</summary>
    public static readonly Zone Agent = new("agent");

    /// <summary>The open web: the island with no name and no credential.</summary>
    public static readonly Zone Web = new("web");

    /// <summary>Everywhere. What a shell reaches, and an honest admission rather than a category —
    /// it is why execution needs an isolator and not a rule.</summary>
    public static readonly Zone Any = new("any");

    /// <summary>Nothing could be determined. Recorded as itself rather than guessed at: a wrong
    /// guess here becomes a wrong verdict later.</summary>
    public static readonly Zone Unknown = new("unknown");

    public static Zone Island(IslandIdentity island) => new(island.Kind, island.DisplayName);

    /// <summary>A named site stands apart from the open web — the operator vouched for it.</summary>
    public static Zone Site(string domain) => new("web", domain);

    public bool IsKnown => Kind != "unknown";

    public override string ToString() => Instance.Length == 0 ? Kind : $"{Kind}:{Instance}";
}

/// <summary>
/// One movement: out of <paramref name="Source"/>, into <paramref name="Sink"/>.
///
/// <para>The unit of decision, and the reason a single scope could never express one. The sink sets
/// the damage — a write into the project is reversible, a write onto a live server is not. The
/// source sets the trust in what is moving. Neither answers for the other, which is why
/// <c>Проект → SSH</c> (configuring a server) and <c>SQL:prod → SSH</c> (posting the payroll to it)
/// are different acts with the same destination.</para>
/// </summary>
public readonly record struct ZoneEdge(Zone Source, Zone Sink, ZoneEffect Effect)
{
    public override string ToString() => $"{Source} → {Sink}";

    /// <summary>How a grant names this movement. Effect is part of it: permission to read a database
    /// is not permission to write to it.</summary>
    public string Key => $"{Source}→{Sink}#{Effect}".ToLowerInvariant();
}

/// <summary>What the movement does at the far end.</summary>
public enum ZoneEffect
{
    /// <summary>Content comes out of the source.</summary>
    Read,

    /// <summary>Content goes in, or the sink's state changes. The two are deliberately one: telling
    /// them apart costs more than it is worth, and both are things the operator has to allow.</summary>
    Write,

    /// <summary>Arbitrary code. Its reach is <see cref="Zone.Any"/> by nature, so no rule bounds it —
    /// only an isolator can.</summary>
    Execute
}
