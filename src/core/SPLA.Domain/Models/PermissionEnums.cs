namespace SPLA.Domain.Models;

public enum AgentMode
{
    Chat = 0,
    Research = 1,
    Inspect = 2,
    Edit = 3,
    Agent = 4
}

/// <summary>How the project's and role's own AGENTS.md tree reaches the prompt. See
/// <c>ADR_20260911-2_agent_agents-md-scopes.md</c> §2.1. No <c>Inherit</c> member — inheritance is
/// the absence of the key in a layer, not a value.</summary>
public enum AgentsMdMode
{
    /// <summary>Root AGENTS.md (and, as folders are visited, nested ones) is injected into the
    /// prompt. Default — compatible with Codex/Cursor/Copilot's own behavior.</summary>
    Inject = 0,

    /// <summary>SPLA never reads AGENTS.md: no root, no nested, no write-gate. For narrow roles that
    /// do not need project instructions.</summary>
    Ignore = 1
}

public enum ToolScope
{
    Local,      // Files and local resources
    Project,    // Project-specific files
    Shell,      // Command line execution
    Internet,   // Web search, APIs
    Agent,      // Capabilities scoped to the agent itself (memory, info, datetime, context).
                // Fundamental: always available in every mode, bypasses mode/permission gating.
    Skill,      // Skill lifecycle tools (skill.activate, agent.spawn).
                // Deactivation is Agent-scoped (always allowed). Activation is Skill-scoped (mode-gated).
    Foreign     // Executed by a foreign MCP server, which declared none of our axes. The whole
                // server is one basket (ToolSetDescriptor) and the grant is taken on the basket.
}

public enum ToolEffect
{
    Read,       // Safe read-only operations
    Write,      // Modifying local or project files
    Execute,    // Running scripts, commands
    Network     // Outbound network requests
}

public enum ToolRisk
{
    Low,
    Medium,
    High,
    Danger
}

public enum PermissionResult
{
    Allow,
    Deny,
    Ask
}

/// <summary>
/// The full answer to "is this call allowed": the outcome plus why. A caller that only needs the
/// outcome reads <see cref="Result"/>; a caller that wants to explain a refusal — the host applying
/// it, or a head deciding whether to ask a human before ever sending the call — reads
/// <see cref="Reason"/> and <see cref="Category"/> too.
/// <para>
/// Separated from applying the decision on purpose: the same verdict is computed twice for two
/// different audiences — once, ahead of time, by whoever might want to ask a human first, and once,
/// at the point of execution, by the host that actually allows or refuses. Recomputing costs nothing
/// (the function is pure and cheap); trusting an unenforced pre-check would cost the enforcement.
/// </para>
/// </summary>
public sealed record PermissionVerdict
{
    public PermissionResult Result { get; init; }

    /// <summary>Human-readable — why this mode/scope/effect combination landed here. Never empty.</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>The override category this verdict was decided under, when one applied
    /// (read/write/shell/internet), for logs and for a caller building its own explanation.</summary>
    public string? Category { get; init; }

    public static implicit operator PermissionResult(PermissionVerdict verdict) => verdict.Result;

    public static PermissionVerdict Allow(string reason, string? category = null) =>
        new() { Result = PermissionResult.Allow, Reason = reason, Category = category };

    public static PermissionVerdict Deny(string reason, string? category = null) =>
        new() { Result = PermissionResult.Deny, Reason = reason, Category = category };

    public static PermissionVerdict Ask(string reason, string? category = null) =>
        new() { Result = PermissionResult.Ask, Reason = reason, Category = category };
}

public enum PermissionDecision
{
    AllowOnce,
    AllowRemember,
    Deny
}

/// <summary>How long a picture produced by a tool stays in the context sent to the model. See
/// <c>agent.tool_images</c> in <c>agents/spla-file.md</c> and wave 4 of
/// <c>PLAN_20260914_plugins_geometry-workspace.md</c>. No <c>Inherit</c> member — inheritance is the
/// absence of the key in a layer, not a value.</summary>
public enum ToolImagesMode
{
    /// <summary>Every tool picture stays in the context for the rest of the chat. Default — the
    /// historical behaviour, unchanged for anyone who does not opt in.</summary>
    All = 0,

    /// <summary>Only the newest tool picture is assembled into the context, whichever tool produced
    /// it. For iterative look-and-correct loops (geometry, screenshots) where every picture but the
    /// last one is a stale frame that is paid for on every request.</summary>
    Last = 1
}

/// <summary>How long one particular picture stays in the context, as stated by the call that asked
/// for it. This is the per-call counterpart of <see cref="ToolImagesMode"/>: the setting says what
/// happens to pictures nobody spoke for, a value here overrides it for this picture alone.
/// <para>
/// The distinction exists because two kinds of picture travel the same path and want opposite
/// treatment. A frame in a look-and-correct loop is stale the moment the next one arrives; a
/// reference image — the thing the work is measured against — is closer to a system prompt than to a
/// tool result, and must survive every later picture and every compaction.
/// </para></summary>
public enum ImageKeep
{
    /// <summary>The call said nothing; <c>agent.tool_images</c> decides. Default, so every existing
    /// caller keeps the behaviour it had.</summary>
    Unspecified = 0,

    /// <summary>A working frame: the newest tool picture is the only one assembled, whoever made it.
    /// What <see cref="ToolImagesMode.Last"/> does, asked for by one call instead of a setting.</summary>
    Once = 1,

    /// <summary>A reference: stays in the context for the rest of the chat. Not evicted by later
    /// pictures (it is filed under its own name, not the shared one) and not hidden by compaction —
    /// only re-reading the same name replaces it.</summary>
    Pinned = 2
}
