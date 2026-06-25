using SPLA.Domain.Models;

namespace SPLA.MCP.Core.Permissions;

public interface IPermissionManager
{
    PermissionResult CheckPermission(AgentMode mode, ToolFunctionDefinition toolMetadata, string argumentsJson);
}

public class PermissionManager : IPermissionManager
{
    private readonly List<RememberedToolPermission> _rememberedPermissions;

    public PermissionManager(IEnumerable<RememberedToolPermission>? rememberedPermissions = null)
    {
        _rememberedPermissions = rememberedPermissions?.ToList() ?? new List<RememberedToolPermission>();
    }

    public void Remember(RememberedToolPermission permission)
    {
        _rememberedPermissions.RemoveAll(x =>
            string.Equals(x.Tool, permission.Tool, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Arguments, permission.Arguments, StringComparison.Ordinal));
        _rememberedPermissions.Add(permission);
    }

    public PermissionResult CheckPermission(AgentMode mode, ToolFunctionDefinition toolMetadata, string argumentsJson)
    {
        // Agent-scoped capabilities (memory, info, datetime, context) are fundamental: always
        // allowed, in every mode, regardless of remembered rules. They never touch project/system.
        if (toolMetadata.Scope == ToolScope.Agent)
            return PermissionResult.Allow;

        // Skill-scoped tools: activation requires user confirmation in interactive modes.
        // Deactivation uses ToolScope.Agent (always allowed) and never reaches this branch.
        if (toolMetadata.Scope == ToolScope.Skill)
        {
            return mode switch
            {
                AgentMode.Chat    => PermissionResult.Ask,
                AgentMode.Inspect => PermissionResult.Ask,
                AgentMode.Edit    => PermissionResult.Allow,
                AgentMode.Agent   => PermissionResult.Allow,
                _                 => PermissionResult.Deny   // Research and unknown modes
            };
        }

        // Agent mode: mode-based rules are authoritative; remembered denies must not override them.
        // Remembered allows are also redundant here (everything is already allowed), so skip entirely.
        if (mode != AgentMode.Agent)
        {
            var remembered = _rememberedPermissions.FirstOrDefault(x =>
                string.Equals(x.Tool, toolMetadata.Name, StringComparison.OrdinalIgnoreCase) &&
                (x.Arguments == "*" || string.Equals(x.Arguments, argumentsJson, StringComparison.Ordinal)));

            if (remembered != null)
            {
                return remembered.Decision == PermissionDecision.AllowRemember
                    ? PermissionResult.Allow
                    : PermissionResult.Deny;
            }
        }

        if (mode == AgentMode.Chat)
            return PermissionResult.Deny;
            
        if (mode == AgentMode.Research)
        {
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Local && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Internet) return PermissionResult.Allow;
            return PermissionResult.Deny;
        }

        if (mode == AgentMode.Inspect)
        {
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Local && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Internet) return PermissionResult.Ask;
            return PermissionResult.Deny;
        }

        if (mode == AgentMode.Edit)
        {
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Write) return PermissionResult.Ask;
            
            if (toolMetadata.Scope == ToolScope.Local && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            
            if (toolMetadata.Scope == ToolScope.Shell)
            {
                if (toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
                if (toolMetadata.Risk == ToolRisk.Danger) return PermissionResult.Deny;
                return PermissionResult.Ask; 
            }
            if (toolMetadata.Scope == ToolScope.Internet) return PermissionResult.Allow;
            
            return PermissionResult.Deny;
        }

        if (mode == AgentMode.Agent)
        {
            if (toolMetadata.Scope == ToolScope.Project) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Shell)
            {
                if (toolMetadata.Risk == ToolRisk.Danger) return PermissionResult.Ask;
                return PermissionResult.Allow;
            }
            if (toolMetadata.Scope == ToolScope.Internet) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Local)
            {
                if (toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
                return PermissionResult.Ask;
            }
            
            return PermissionResult.Ask;
        }

        return PermissionResult.Deny;
    }
}

public sealed class RememberedToolPermission
{
    public string Tool { get; init; } = "";
    public string Arguments { get; init; } = "";
    public PermissionDecision Decision { get; init; }
}
