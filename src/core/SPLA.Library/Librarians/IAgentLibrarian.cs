using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Library.Librarians;

/// <summary>
/// The librarian that reads the question rather than matching its words.
///
/// <para><b>Where the catalog goes.</b> Into this librarian's own system prompt, for one throwaway
/// call. The 13k tokens a hundred skills cost do not disappear — they stop being paid on every
/// iteration of the main conversation and get paid once, somewhere else. The chat pays for the
/// question and five cards.</para>
///
/// <para><b>What it buys over <see cref="ITagLibrarian"/>.</b> Synonyms and intent: "the server keeps
/// dropping connections" finds an ssh skill that nobody tagged <c>connection</c>. What it costs is an
/// LLM call before any work starts, which is why it is off unless configured and why it runs only
/// after the free deterministic pass has found nothing.</para>
/// </summary>
public interface IAgentLibrarian
{
    /// <summary>False when no model is configured. Callers skip the whole layer rather than paying a
    /// call to be told nothing.</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Cards the librarian judges relevant, best first, never more than <paramref name="limit"/>.
    ///
    /// <para>Returns empty rather than throwing when the model is unreachable or answers nonsense: a
    /// librarian who is out to lunch must degrade the search, not fail the turn.</para>
    /// </summary>
    Task<IReadOnlyList<SkillMatch>> AskAsync(string question, int limit = 5, CancellationToken ct = default);
}
