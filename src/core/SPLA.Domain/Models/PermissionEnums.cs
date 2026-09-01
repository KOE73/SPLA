namespace SPLA.Domain.Models;

public enum AgentMode
{
    Chat = 0,
    Research = 1,
    Inspect = 2,
    Edit = 3,
    Agent = 4
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
