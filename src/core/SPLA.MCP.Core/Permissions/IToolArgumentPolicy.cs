using SPLA.Domain.Models;

namespace SPLA.MCP.Core.Permissions;

/// <summary>
/// A domain check on the arguments of one family of tools — "may this caller run <i>this SQL</i>",
/// not merely "may this caller run the SQL tool".
/// <para>
/// Lives inside <see cref="PermissionManager"/> rather than as a second link on
/// <see cref="Pipeline.ToolPipelineStage.Policy"/>, and that is the whole design decision: two links
/// on one stage would be ordered by registration and would answer the same question twice. There is
/// one verdict, and a module is a term inside it.
/// </para>
///
/// <para><b>The contract a module must keep.</b> None of these are style:</para>
/// <list type="number">
///   <item><b>Pure.</b> No I/O, no network, no database round-trip. <see cref="IPermissionManager"/>
///   is documented as safe to call twice — a head may compute the verdict ahead of time to learn
///   whether a human will be asked, without running anything — so a module with a side effect fires
///   twice.</item>
///   <item><b>Stateless.</b> The pipeline is re-entrant: a script's <c>ctx.Run</c> reaches the same
///   module instance from inside a call that is already being evaluated.</item>
///   <item><b>Narrows only.</b> A module may turn <c>Allow</c> into <c>Ask</c> or <c>Deny</c>. It may
///   never turn <c>Deny</c> into <c>Allow</c> — otherwise a plugin gains a way to widen its own
///   rights and "the most restrictive wins" stops being true. Enforced by construction in
///   <see cref="PermissionManager"/>, not left to the module's good manners.</item>
///   <item><b>Abstains rather than guesses.</b> Could not parse the arguments — return <c>null</c>,
///   not a denial. A module that refuses whatever it fails to understand turns a weak parser into a
///   disabled tool, which is worse than having no policy at all.</item>
///   <item><b>Says why.</b> <see cref="PermissionVerdict.Reason"/> reaches the person in the
///   confirmation prompt, where today only the tool's own description appears.</item>
/// </list>
/// </summary>
public interface IToolArgumentPolicy
{
    /// <summary>Whether this module has anything to say about this tool at all. Checked before
    /// <see cref="Evaluate"/> so that a module never has to parse arguments belonging to a tool it
    /// does not know.</summary>
    bool AppliesTo(ToolFunctionDefinition tool);

    /// <summary>
    /// The module's verdict, or <c>null</c> for "no opinion" — which is also the right answer when
    /// the arguments could not be understood.
    /// </summary>
    PermissionVerdict? Evaluate(ToolFunctionDefinition tool, string argumentsJson);
}
