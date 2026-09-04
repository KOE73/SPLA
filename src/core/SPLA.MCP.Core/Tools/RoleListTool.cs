using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// The project's role directory: who exists to be tasked (<c>agent_spawn</c>) or written to
/// (<c>agent_correspond</c>). Without it addressing a role is guesswork — both of those tools take a
/// role name and refuse anything the manifest does not declare, and until now nothing told a chat
/// what those names were.
///
/// <para><b>Name, mode and description only — never the role's prompt, and there is no flag that
/// would add it.</b> A role body carries its character together with its capabilities, islands and
/// trusted domains, i.e. the shape of what it may reach; a tool that handed that body to any chat
/// that asked would publish a security decision sideways, around the zone model
/// (<c>ADR_20260811_core_security-zones</c>). Reading a role's prompt stays an ordinary file read of
/// <c>roles/&lt;name&gt;.yaml</c>, where the file tools' own permissions decide whether it happens.
/// See <see cref="SplaRoleSection.Description"/> for the outward/inward split this rests on.</para>
///
/// <para>The manifest is the authority on which roles act (<see cref="SplaProject.Roles"/> —
/// "nothing acts that nobody named"), so the listing is driven by it and the files under
/// <c>roles/</c> only fill in the detail. A declared name whose body will not load is therefore
/// still listed, marked as unreadable: it is a real, addressable role with a broken file, and
/// dropping it — or failing the whole call over it — would hide from the caller exactly the name
/// they are about to be refused on.</para>
/// </summary>
public sealed class RoleListTool : IMcpTool
{
    private readonly ResolvedSettings _settings;

    public RoleListTool(ResolvedSettings settings) => _settings = settings;

    public string Name => "role_list";

    /// <summary>Everything about this tool that does not fit its one-line description.
    /// Disclosed together with the tool itself — see <c>ToolFunctionDefinition.Details</c>.</summary>
    private static readonly string DetailsText =
        """
        tool: role_list

        summary: Lists the roles this project declares — the names agent_spawn and agent_correspond
                 accept. Call it before addressing anyone, rather than guessing a role name and
                 being refused.

        arguments:
          none.

        returns:
          One line per role: its name, its agent mode, and a one-sentence description when the role
          gives one. A declared role whose roles/<name>.yaml cannot be read is still listed, marked
          "(body unreadable)" — the name is addressable either way.
          "(none)" when the project declares no roles at all.

        notes:
          - Only name, mode and description are returned. A role's system prompt is deliberately NOT
            available here, and no argument turns it on: the role body also carries that role's
            capabilities, islands and trusted domains, and handing it out on request would leak a
            security decision to any chat that asked.
          - If you actually need the text of a role's prompt, read roles/<name>.yaml with the file
            tools — where the ordinary permission rules apply to you.
          - The project's own agent: block is "role zero" and is not a role in this list; you are
            already it (or a role of your own) and cannot address it.
        """;

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Details = DetailsText,
            Description = "Lists the roles declared in this project — name, mode and a short description — " +
                          "so you can address one with agent_spawn or agent_correspond. Role prompts are " +
                          "not disclosed.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new { },
                required = Array.Empty<string>()
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var declared = _settings.Manifest?.Roles ?? new List<string>();
        var names = declared
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count == 0)
            return Task.FromResult(ToolResult.Text(
                "(none) This project declares no roles — there is nobody to spawn or correspond with."));

        // Role files sit next to the manifest, never relative to the current directory (ConfigLoader
        // .LoadRole's own contract). No manifest path ⇒ nothing to be next to, so bodies stay unread
        // and the names alone are still worth returning.
        var projectDir = _settings.ProjectFilePath is { } path ? Path.GetDirectoryName(path) : null;

        var body = new StringBuilder($"{names.Count} role(s) declared in this project:");
        foreach (var name in names)
        {
            SplaRoleSection? role = null;
            if (projectDir != null)
            {
                // Broad on purpose: unreadable is unreadable, whether the file is missing, locked
                // or malformed YAML, and one bad file must not cost the caller the rest of the
                // directory. The name still goes out, marked.
                try { role = ConfigLoader.LoadRole(projectDir, name); }
                catch { }
            }

            if (role is null)
            {
                body.Append($"\n- {name}  (body unreadable — roles/{name}.yaml is missing or malformed)");
                continue;
            }

            var mode = string.IsNullOrWhiteSpace(role.Mode) ? "(inherits project mode)" : role.Mode!.Trim();
            body.Append($"\n- {name}  [{mode}]");
            if (!string.IsNullOrWhiteSpace(role.Description))
                body.Append($"  — {role.Description!.Trim()}");
        }

        return Task.FromResult(ToolResult.Text(body.ToString()));
    }
}
