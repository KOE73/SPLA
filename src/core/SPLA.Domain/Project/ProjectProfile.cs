using System;
using System.Collections.Generic;

namespace SPLA.Domain.Project;

/// <summary>
/// How much a newly created project is allowed to do on the day it is created.
///
/// <para>A profile is a <b>template applied once</b>, not a field of the manifest. What it writes —
/// an explicit capability list, an explicit plugin gate — is ordinary configuration afterwards, and
/// nothing at load time ever asks "which profile was this?". Storing the profile instead would put a
/// second source of truth beside <c>agent.capabilities</c> and immediately raise the question of
/// which one wins; storing only its result cannot.</para>
///
/// <para>The default is <see cref="Minimal"/>. Running in a folder without a manifest used to inherit
/// the machine's whole <c>defaults.yaml</c> AND drop the path boundary — more authority than anyone
/// asked for. A profile is how authority becomes something granted rather than something inherited
/// by accident.</para>
/// </summary>
public enum ProjectProfile
{
    /// <summary>Nothing built in: no <c>core.*</c> features, no plugins. The LLM connection is still
    /// inherited — it is not a capability, and without it there is nothing to run at all.</summary>
    Minimal,

    /// <summary>Everything built in, plus whatever plugins and skills the machine layer offers.</summary>
    Standard,

    /// <summary>Create nothing; run against <c>defaults.yaml</c> with no project and no boundary.
    /// The historical behaviour, kept reachable but never arrived at by default.</summary>
    Inherit
}

/// <summary>Parsing and naming for <see cref="ProjectProfile"/> — the one place the spelling exists.</summary>
public static class ProjectProfiles
{
    public const ProjectProfile Default = ProjectProfile.Minimal;

    public static string Name(ProjectProfile profile) => profile switch
    {
        ProjectProfile.Minimal => "minimal",
        ProjectProfile.Standard => "standard",
        ProjectProfile.Inherit => "inherit",
        _ => throw new ArgumentOutOfRangeException(nameof(profile))
    };

    public static IReadOnlyList<string> AllNames { get; } = ["minimal", "standard", "inherit"];

    /// <summary>One line per profile, for a CLI error message or a dialog's radio buttons.</summary>
    public static string Describe(ProjectProfile profile) => profile switch
    {
        ProjectProfile.Minimal => "no built-in features and no plugins; the LLM connection is inherited",
        ProjectProfile.Standard => "every built-in feature, plus the machine's plugins and skills",
        ProjectProfile.Inherit => "no manifest at all: machine defaults, and no project boundary",
        _ => throw new ArgumentOutOfRangeException(nameof(profile))
    };

    /// <summary>No guessing and no default on failure: an unknown name is an error with the legal
    /// names in it, the same rule secret scopes follow.</summary>
    public static bool TryParse(string? name, out ProjectProfile profile)
    {
        switch (name?.Trim().ToLowerInvariant())
        {
            case "minimal": profile = ProjectProfile.Minimal; return true;
            case "standard": profile = ProjectProfile.Standard; return true;
            case "inherit": profile = ProjectProfile.Inherit; return true;
            default: profile = Default; return false;
        }
    }
}
