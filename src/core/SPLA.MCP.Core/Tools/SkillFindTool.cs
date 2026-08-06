using SPLA.Domain.Models;
using SPLA.Library.Librarians;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Asks the librarian what the fond has on a subject.
///
/// <para><b>Returns cards, never bodies.</b> A stack of annotations: read them, pick one, then go and
/// read that one. Handing back procedures would put the thing this whole design exists to avoid — the
/// full text of skills nobody chose — straight back into the context.</para>
///
/// <para>Read-effect and Skill-scoped, so it does not prompt: asking the catalogue a question commits
/// to nothing, and <c>skill_activate</c> remains the gate that does.</para>
///
/// <para><b>Two librarians, one tool.</b> The ADR calls them layers rather than competitors, and the
/// stage-4 runs are why that matters in practice: a weak model already struggles to call this tool at
/// all, so a second search tool beside it would make selection worse, not better. The deterministic
/// tag pass runs first and free; the model-backed librarian runs only when that found nothing, and
/// only when one is configured.</para>
/// </summary>
public sealed class SkillFindTool : IMcpTool
{
    private const int DefaultLimit = 5;
    private const int MaxLimit = 15;

    private readonly ITagLibrarian _librarian;
    private readonly IAgentLibrarian? _agentLibrarian;

    public SkillFindTool(ITagLibrarian librarian, IAgentLibrarian? agentLibrarian = null)
    {
        _librarian = librarian;
        _agentLibrarian = agentLibrarian;
    }

    public string Name => "skill_find";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Searches the skill catalogue by subject tag and/or free text, and returns matching skill descriptions (not their procedures). Use it when the Skills section lists subjects rather than individual skills, or when no listed skill obviously matches — then call skill_activate with the id you chose.",
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    tags = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Subject words, chosen from the subjects listed in the Skills section. Several tags narrow the result (a skill must carry all of them)."
                    },
                    text = new
                    {
                        type = "string",
                        description = "Free text matched against skill ids and descriptions. Use when no listed subject fits."
                    }
                },
                required = System.Array.Empty<string>()
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        SkillQuery query;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            query = new SkillQuery(
                ToolJson.GetStringArray(doc.RootElement, "tags"),
                ToolJson.GetStringTrimmed(doc.RootElement, "text"));
        }
        catch (JsonException)
        {
            return ToolResult.Fail("error: invalid_json", "invalid json");
        }

        if (query.IsEmpty) return ToolResult.Fail("error: give 'tags', 'text', or both\n" + KnownSubjects(), "empty query");

        // The free, deterministic pass first, always. Set intersection answers most questions at no
        // cost, and paying an LLM call to be told what a dictionary lookup already knew is the kind
        // of latency nobody attributes to the right thing later.
        var matches = _librarian.Find(query, DefaultLimit);

        // Only when it found nothing: the reader used words the fond does not use. That is exactly
        // and only what a librarian who reads the question is for.
        var askedLibrarian = false;
        if (matches.Count == 0 && _agentLibrarian is { IsAvailable: true })
        {
            askedLibrarian = true;
            matches = await _agentLibrarian.AskAsync(
                QuestionFrom(query), DefaultLimit, cancellationToken);
        }

        if (matches.Count == 0) return ToolResult.Text(NothingFound(query));

        var sb = new StringBuilder();
        sb.Append($"{matches.Count} matching skill(s)");
        if (askedLibrarian) sb.Append(" (matched by meaning, not by subject word)");
        sb.Append(". Call skill_activate with the id you want:");
        foreach (var match in matches)
        {
            sb.Append($"\n\n  {match.Card.DisplayId}");
            if (match.Card.Tags.Count > 0) sb.Append($"  [{string.Join(", ", match.Card.Tags)}]");
            if (match.Card.Description.Length > 0) sb.Append($"\n    {match.Card.Description}");
        }

        return ToolResult.Text(sb.ToString());
    }

    /// <summary>The query as a sentence for a librarian that reads rather than matches. Tags carry
    /// real intent even when they matched nothing, so they are not thrown away.</summary>
    private static string QuestionFrom(SkillQuery query)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(query.Text)) parts.Add(query.Text!.Trim());
        if (query.Tags is { Count: > 0 }) parts.Add(string.Join(" ", query.Tags));
        return string.Join(" — ", parts);
    }

    /// <summary>An empty result has two different causes and the model must not confuse them: a
    /// subject nobody has written a skill for, versus a word this fond does not use. The second is
    /// fixable by asking again with a real word, so say which one happened.</summary>
    private string NothingFound(SkillQuery query)
    {
        var unknown = (query.Tags ?? [])
            .Where(t => !_librarian.Vocabulary.Knows(t))
            .ToList();

        return unknown.Count > 0
            ? $"no skills found — these are not subjects in this catalogue: {string.Join(", ", unknown)}\n" + KnownSubjects()
            : "no skills found for that query. Nothing in the catalogue covers it — say so rather than guessing at a skill id.";
    }

    private string KnownSubjects()
    {
        var vocabulary = _librarian.Vocabulary;
        return vocabulary.IsEmpty
            ? "the catalogue has no subjects."
            : "subjects in this catalogue: " + string.Join(", ", vocabulary.Tags());
    }
}
