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
    Internet    // Web search, APIs
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

public enum PermissionDecision
{
    AllowOnce,
    AllowRemember,
    Deny
}
