using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Skills;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Reads one attachment of the CURRENTLY ACTIVE skill — a <c>references/</c> page, an <c>assets/</c>
/// template, anything that shipped beside its SKILL.md.
///
/// <para>This is a loan slip, not file access. Two conditions, both structural rather than advisory:
/// a skill must be active, and the path must belong to that skill. There is no argument for naming
/// another skill, which is why the model cannot ask for one; and the source resolves the path itself,
/// so escaping the skill's folder fails inside the source rather than being checked here.</para>
///
/// <para>The address comes from the session's pinned loan slip (source id + ref), not from a fresh
/// lookup by id. A rebuild that shadows the skill therefore cannot redirect a running procedure to a
/// different edition's appendices, exactly as the pinned body cannot be swapped mid-run.</para>
/// </summary>
public sealed class SkillReadResourceTool : IMcpTool
{
    private readonly SkillLibrary _skills;

    public SkillReadResourceTool(SkillLibrary skills) => _skills = skills;

    public string Name => "skill_read_resource";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Reads one resource file belonging to the currently active skill (its references/, assets/ and other attachments). Only works while a skill is active, and only for that skill's own resources — the available paths are listed in the ACTIVE SKILL block.",
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new
                    {
                        type = "string",
                        description = "Resource path relative to the active skill, exactly as listed in the ACTIVE SKILL block (e.g. references/package-contract.md)."
                    }
                },
                required = new[] { "path" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var session = AgentSessionScope.Current?.Skills;
        if (session is null) return Task.FromResult("error: no active chat session");

        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var path = ToolJson.GetStringTrimmed(doc.RootElement, "path");

            if (string.IsNullOrEmpty(path))
                return Task.FromResult("error: 'path' parameter is required");

            if (session.ActiveSkillId is null)
                return Task.FromResult(
                    "error: no skill is active — resources are only readable while their skill is active; call skill_activate first");

            if (session.ActiveSourceId is null || session.ActiveRef is null)
                return Task.FromResult(
                    $"error: skill '{session.ActiveSkillId}' was activated without a source address — its resources are not reachable");

            // Normalised the way the listing is, so "references\x.md" and "./references/x.md" are not
            // three different answers to the same question. A ".." is NOT normalised away: quietly
            // reinterpreting an escape as a local path would answer a question nobody asked.
            var wanted = path.Replace('\\', '/');
            if (wanted.StartsWith("./", StringComparison.Ordinal)) wanted = wanted[2..];

            var known = session.ActiveResources;
            if (!known.Any(r => r.Equals(wanted, StringComparison.OrdinalIgnoreCase)))
            {
                var msg = known.Count == 0
                    ? $"error: skill '{session.ActiveSkillId}' has no resources"
                    : $"error: '{path}' is not a resource of skill '{session.ActiveSkillId}'\navailable:\n" +
                      string.Join("\n", known.Select(r => $"  - {r}"));
                return Task.FromResult(msg);
            }

            var text = _skills.ReadResource(session.ActiveSourceId, session.ActiveRef, wanted);
            if (text is null)
                return Task.FromResult(
                    $"error: resource '{wanted}' could not be read — its source '{session.ActiveSourceId}' returned nothing");

            return Task.FromResult(text);
        }
        catch (JsonException)
        {
            return Task.FromResult("error: invalid_json");
        }
    }
}
