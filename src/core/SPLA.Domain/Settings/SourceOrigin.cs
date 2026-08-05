namespace SPLA.Domain.Settings;

/// <summary>
/// Which layer a skill source entry came from — and therefore how far it may be believed when it
/// vouches for itself.
///
/// <para>The axis is not "how specific" but <b>who can substitute the layer's contents</b>. A project
/// file arrives with somebody else's repository; the machine layer belongs to the person at the
/// keyboard; deployment policy belongs to whoever administers the install. Ordered least privileged
/// first, so a ceiling is a comparison rather than a table lookup.</para>
///
/// <para>This governs <c>trust</c> and nothing else. <c>level</c> — how much of a source reaches the
/// model unasked — is a statement about context economy made by whoever owns the fond, and a project
/// is entitled to say "show my skills only when asked". It is not entitled to say "my skills have
/// been vetted".</para>
/// </summary>
public enum SourceOrigin
{
    /// <summary>Declared in the project's committed <c>.spla</c>. Travels with a repository that may
    /// not be yours, so it may never claim more than <c>untrusted</c> for itself.</summary>
    Project,

    /// <summary>Declared in the machine layer (<c>~/.spla/defaults.yaml</c>) — the person at the
    /// keyboard, who is entitled to say what they trust.</summary>
    Machine,

    /// <summary>Granted at runtime by the user through the UI, into their own store. Same standing as
    /// <see cref="Machine"/> locally; on a server it is the user's private area and the deployment's
    /// ceiling applies over it.</summary>
    Granted,

    /// <summary>Set by whoever administers the installation. Sets the ceiling for everyone else.</summary>
    Deployment
}
