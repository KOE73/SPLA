using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Library.Catalog;
using SPLA.Library.Sources;

namespace SPLA.Library.Librarians;

/// <summary>
/// One LLM call with the whole catalog in its system prompt.
///
/// <para><b>Not built on agent_spawn</b>, despite the plan's wording. A spawned sub-agent is a full
/// agent loop — tools, conversation, checkpoints — and it runs a <i>skill</i>, which a lookup is not.
/// A librarian needs exactly one call to one model, and <see cref="ILlmGateway"/> is that, with
/// accounting and quotas already in the path.</para>
///
/// <para><b>The answer is never trusted as text.</b> The model returns ids; every id is looked up in
/// the holdings and anything that does not resolve is dropped. A hallucinated skill id is the obvious
/// failure of this whole approach, and mapping back through the library is what makes it impossible
/// rather than merely unlikely.</para>
/// </summary>
public sealed class AgentLibrarian : IAgentLibrarian
{
    private readonly SkillLibrary _library;
    private readonly ILlmGateway _llm;
    private readonly Func<ResolvedSettings> _settings;
    private readonly ILogger? _logger;

    public AgentLibrarian(SkillLibrary library, ILlmGateway llm, Func<ResolvedSettings> settings,
        ILogger? logger = null)
    {
        _library = library;
        _llm = llm;
        _settings = settings;
        _logger = logger;
    }

    private SplaLibrarianSection? Config => _settings().SkillLibrarian;

    public bool IsAvailable => Config?.Enabled == true;

    /// <summary>The same boundary the tag librarian keeps: a source the model is not told about must
    /// not become discoverable by asking, and that includes asking in words.</summary>
    private IReadOnlyList<SkillCard> Searchable() =>
        _library.Catalog().Where(c => c.Level != SourceLevel.OutOfCatalog).ToList();

    public async Task<IReadOnlyList<SkillMatch>> AskAsync(
        string question, int limit = 5, CancellationToken ct = default)
    {
        if (!IsAvailable || string.IsNullOrWhiteSpace(question) || limit <= 0) return [];

        var cards = Searchable();
        if (cards.Count == 0) return [];

        try
        {
            var settings = _settings();
            var turn = new LlmTurnContext
            {
                Messages =
                [
                    new ChatMessage { Role = ChatRole.System, Content = SystemPrompt(cards, limit) },
                    new ChatMessage { Role = ChatRole.User, Content = question.Trim() }
                ],
                // No tools, and no sinks: a librarian answers, it does not act, and nobody is watching
                // it type. Streaming a lookup into the user's chat would be noise.
                Settings = settings.ToLLMSettings(EntryFor(settings)),
                ModelId = Config?.Model
            };

            var result = await _llm.InvokeAsync(turn, ct);
            return Resolve(result.Message?.Content, cards, limit);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            // Degrade the search, never the turn: the caller has already tried the free pass and will
            // simply report nothing found.
            _logger?.LogWarning(ex, "Agent librarian failed; falling back to no matches.");
            return [];
        }
    }

    private ResolvedModelEntry? EntryFor(ResolvedSettings settings) =>
        Config?.Model is { } id
            ? settings.Models.FirstOrDefault(m => m.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
              ?? settings.Models.FirstOrDefault()
            : settings.Models.FirstOrDefault();

    /// <summary>
    /// The catalog, plus the narrowest possible instruction. Ids only in the answer: a librarian that
    /// is asked to explain itself spends tokens on prose the caller throws away, and every extra
    /// sentence is another chance to answer in a shape that will not parse.
    /// </summary>
    private static string SystemPrompt(IReadOnlyList<SkillCard> cards, int limit)
    {
        var sb = new StringBuilder();
        sb.Append("You are a library catalogue. You are given every skill this system has and one request from a user.\n");
        sb.Append($"Answer with the ids of up to {limit} skills that would help with that request, most relevant first, ONE PER LINE, and nothing else.\n");
        sb.Append("Copy ids exactly as written below. Do not invent ids. Do not explain. Do not number the lines.\n");
        sb.Append("If nothing here fits the request, answer with the single word NONE.\n");
        sb.Append("Match on meaning, not on wording: the user will not use the same words as the descriptions.\n\n");
        sb.Append("SKILLS:\n");

        foreach (var card in cards)
        {
            sb.Append($"{card.Id}");
            if (card.Tags.Count > 0) sb.Append($" [{string.Join(", ", card.Tags)}]");
            if (card.Description.Length > 0) sb.Append($" — {card.Description}");
            sb.Append('\n');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Maps the answer back onto real cards. Everything unrecognised is dropped silently — the model
    /// is a selector here, not a source of truth, and the library is the only thing that can say a
    /// skill exists.
    /// </summary>
    private static IReadOnlyList<SkillMatch> Resolve(string? answer, IReadOnlyList<SkillCard> cards, int limit)
    {
        if (string.IsNullOrWhiteSpace(answer)) return [];

        var byId = cards.ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
        var matches = new List<SkillMatch>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in answer.Split('\n'))
        {
            // Tolerate the shapes a model reaches for anyway: "- id", "1. id", "`id`", "id — why".
            // Models decorate lists whatever the instruction says; refusing the decoration would mean
            // dropping a correct answer over punctuation.
            var line = raw.Trim().TrimStart('-', '*', '•', ' ');
            line = StripEnumerator(line).Trim('`', ' ');
            var cut = line.IndexOfAny([' ', '\t', ':', '—']);
            if (cut > 0) line = line[..cut];
            line = line.TrimEnd('.', ',', ')');
            if (line.Length == 0 || line.Equals("NONE", StringComparison.OrdinalIgnoreCase)) continue;

            if (byId.TryGetValue(line, out var card) && seen.Add(card.Id))
            {
                // Rank is the order the librarian gave, expressed as a descending score so a caller
                // can merge these with tag matches without knowing where they came from.
                matches.Add(new SkillMatch(card, [], limit - matches.Count));
                if (matches.Count == limit) break;
            }
        }

        return matches;
    }

    /// <summary>Removes a leading "1." or "2)" — but only when a digit run is followed by one, so a
    /// skill id that legitimately starts with a number survives.</summary>
    private static string StripEnumerator(string line)
    {
        var i = 0;
        while (i < line.Length && char.IsAsciiDigit(line[i])) i++;
        if (i == 0 || i >= line.Length || (line[i] != '.' && line[i] != ')')) return line;

        return line[(i + 1)..].TrimStart();
    }
}
