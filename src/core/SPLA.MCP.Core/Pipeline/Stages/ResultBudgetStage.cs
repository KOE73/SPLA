using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using Microsoft.Extensions.Logging;
using System.Text;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// The <see cref="ToolPipelineStage.Post"/> link: the last place a result can still be cut down
/// before it becomes part of a request nobody can send.
///
/// <para><b>The case it exists for.</b> A ten-minute <c>ssh_session_wait</c> came back with 460 000
/// characters. The turn was assembled, sent, and refused by the endpoint — <c>request (396067
/// tokens) exceeds the available context size (98304)</c> — and the turn died with it. The model
/// never saw the result, the work was lost, and the provider had told us something we could have
/// told ourselves.</para>
///
/// <para><b>Why the tool cannot decide this.</b> A tool knows how many characters it produced. It
/// does not know which model is reading them, how large that model's window is, or how much of it
/// the conversation has already spent — and the same tool serves a chat with 8k and a chat with
/// 200k. So the tool produces its result in full, and the cut is made here.</para>
///
/// <para>
/// <b>A standing ceiling, not a calculation.</b> Bulk is cut ALWAYS — at
/// <see cref="CeilingChars"/> — whether or not anything is known about the model. The budget
/// (<see cref="IContextBudgetHost"/>) may only lower that ceiling, never raise it.
/// </para>
/// <para>
/// This started as the reverse — the budget decided everything, and an unknown budget meant "pass it
/// through and hope". Two things killed that design. First, a model can emit several tool calls in
/// ONE answer, and the background inbox delivers several results at once on a turn boundary; the
/// occupancy figure only refreshes when the model answers, so every result in such a batch was
/// weighed against the same stale remainder and each was granted a share of it. The arithmetic did
/// not add up, and no amount of accounting inside the link could fix a limit that was per-result by
/// construction. A standing ceiling has no such failure mode: it does not care how many results
/// there were. Second, "unknown" is the normal state in a CLI run, a spawned agent, and the first
/// turn of every chat — which made the safe-looking default the common one.
/// </para>
///
/// <para><b>The outcomes</b>, in the order they are decided:</para>
/// <list type="number">
///   <item>The tool already routed its payload to the data channel — nothing to do. A result that
///   carries a <see cref="ToolResource"/> has made its own arrangement and this link stays out of
///   it.</item>
///   <item>It fits under the limit — passed through untouched. This is the overwhelming majority and
///   it must stay free: a link that rewrites every small result is a link that will eventually
///   rewrite the wrong one.</item>
///   <item>It does not fit — the tail is kept (the end of a log is where the answer is), the whole
///   is put in the blob store, and the model is told both facts. <b>Without having been asked.</b>
///   An opt-in flag cannot help here: by the time anyone knows the output was too big, the call that
///   would have carried the flag has already happened. The stored blob is searchable — see
///   <c>blob_grep</c>, without which this would be a grave rather than a store.</item>
///   <item>It does not fit and there is no blob store (no chat behind this call) — the tail is kept
///   and the rest is stated as lost. Losing the middle of one result is bad; losing the turn is
///   worse.</item>
/// </list>
///
/// <para><b>What this link deliberately does not do:</b> refuse the call, retry it, or decide that a
/// result is uninteresting. It never turns a successful call into a failure — a trimmed result keeps
/// its <see cref="ToolResult.Outcome"/>, because what happened to the call and what happened to its
/// text are different facts.</para>
/// </summary>
public sealed class ResultBudgetStage : IToolMiddleware
{
    /// <summary>
    /// Characters per token, for turning a budget in tokens into a limit in characters.
    /// <para>
    /// Deliberately pessimistic. Real tokenisers average nearer 4 on English prose, but tool output
    /// is not prose — paths, hex ids, JSON and Cyrillic all tokenise far worse, and the docker log
    /// that started this weighed in around 1.2. Erring low means the occasional result is trimmed
    /// that would have fitted; erring high means the failure this link exists to prevent still
    /// happens. Those two mistakes are not symmetrical.
    /// </para>
    /// </summary>
    private const double CharsPerToken = 2.0;

    /// <summary>
    /// The share of the remaining window one tool result may occupy.
    /// <para>
    /// Not the whole of it: the result is not the only thing joining the next request — the turn also
    /// carries the model's own reply, the other tool calls of the same round, and room for the answer
    /// it is about to write. A result allowed to fill the entire remainder would leave the model
    /// unable to say anything about it, which is a different way to waste the same turn.
    /// </para>
    /// </summary>
    private const double ShareOfRemaining = 0.5;

    /// <summary>Never trim below this, whatever the arithmetic says. A budget that has almost run
    /// out would otherwise reduce results to nothing, and a model shown nothing cannot even work out
    /// that it should compact the conversation.</summary>
    private const int FloorChars = 4_000;

    /// <summary>
    /// The standing limit: no tool result exceeds this, whatever is or is not known about the model.
    /// <para>
    /// A single half-megabyte result inside a large window is still a result nobody reads and a turn
    /// nobody can afford to repeat. Roughly a screenful of a book — enough for a stack trace, a build
    /// log's error section, or a directory listing, and past the point where more text answers more
    /// questions. What does not fit is not lost: it is one <c>blob_grep</c> away.
    /// </para>
    /// </summary>
    private const int CeilingChars = 60_000;

    /// <summary>Of what is kept, how much comes from the END. Tool output is chronological and its
    /// conclusion lives at the bottom: the exit status, the error, the summary line. The head is
    /// worth keeping only to say what was being done.</summary>
    private const double TailShare = 0.8;

    private readonly ILogger? _logger;

    public ResultBudgetStage(ILogger? logger = null) => _logger = logger;

    public ToolPipelineStage Stage => ToolPipelineStage.Post;

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        var result = await next(call, ct);

        // Rule 1: the tool already sent its bulk down the data channel. Its own summary is the
        // arrangement it chose, and second-guessing it here would trim a summary.
        if (result.Content.OfType<ToolResource>().Any()) return result;

        var text = result.TextContent;
        if (text.Length == 0) return result;

        var limit = LimitChars(AgentSessionScope.Current?.ContextBudget?.Budget);

        // Rule 2: it fits. The overwhelming majority of calls end here.
        if (text.Length <= limit) return result;

        return Trim(call, result, text, limit);
    }

    /// <summary>
    /// How many characters this result may occupy. <see cref="CeilingChars"/> is the answer when
    /// nothing is known about the model — an honest default, not a guess, because the ceiling is a
    /// statement about what is worth reading rather than about what happens to fit.
    /// <para>
    /// A known budget can only make it stricter. Never the other way round: a large window is not a
    /// reason to hand the model more text than it can use, and the mistake of trimming something
    /// that would have fitted costs a <c>blob_grep</c>, while the mistake of not trimming costs the
    /// turn.
    /// </para>
    /// </summary>
    private static int LimitChars(ContextBudget? budget)
    {
        if (budget is not { WindowTokens: > 0 } b) return CeilingChars;
        var chars = (long)(b.RemainingTokens * ShareOfRemaining * CharsPerToken);
        return (int)Math.Clamp(chars, FloorChars, CeilingChars);
    }

    private ToolResult Trim(ToolCallInvocation call, ToolResult result, string text, int limit)
    {
        var store = AgentSessionScope.Current?.Blobs;
        string? handle = null;
        if (store is not null)
        {
            try
            {
                handle = store.Put(
                    BlobPayload.OfText(text, ContentTypes.Text),
                    name: null,
                    origin: null);
            }
            catch (Exception ex)
            {
                // A blob store that refuses is not a reason to lose the turn: fall through and keep
                // the tail with an honest note about what went missing.
                _logger?.LogWarning(ex, "result budget: could not store the full output of {Tool}", call.Name);
            }
        }

        var (head, tail, cutLines) = Split(text, limit);

        var sb = new StringBuilder();
        sb.Append(head);
        sb.Append("\n\n…[").Append(cutLines).Append(" lines cut: this result was ")
          .Append(text.Length).Append(" characters and the context has room for about ")
          .Append(limit).Append(". ");
        sb.Append(handle is null
            ? "The full output could not be stored and is gone — re-run narrower (grep, tail, a smaller range) if you need the middle."
            : $"NOTHING IS LOST: the full output is at {handle}. Search it with blob_grep (a pattern, with " +
              "surrounding lines), look at a slice with blob_peek, or pass the handle to a tool that takes one. " +
              "The END of the output is kept below, which is usually where the answer is.");
        sb.Append("]…\n\n");
        sb.Append(tail);

        var content = new List<ToolContent> { new ToolText(sb.ToString()) };
        // Everything that was not text (an image, a resource) travels on untouched: this link
        // budgets prose, and a screenshot is not made smaller by cutting a log.
        content.AddRange(result.Content.Where(c => c is not ToolText));
        if (handle is not null)
            content.Add(new ToolResource(handle, ContentTypes.Text, $"full output of {call.Name}"));

        _logger?.LogInformation(
            "result budget: {Tool} returned {Size} chars, limit {Limit}, kept {Kept}{Stored}",
            call.Name, text.Length, limit, head.Length + tail.Length,
            handle is null ? ", not stored" : $", stored as {handle}");

        // The outcome is the tool's, not ours. A successful call whose output was long is still a
        // successful call, and a failure trimmed to its last lines is still a failure.
        return new ToolResult { Outcome = result.Outcome, Reason = result.Reason, Content = content };
    }

    /// <summary>Cuts on line boundaries so neither half starts or ends mid-line — a truncated line
    /// reads as data that was never there, which is worse than an obviously missing chunk.</summary>
    private static (string Head, string Tail, int CutLines) Split(string text, int limit)
    {
        var tailBudget = (int)(limit * TailShare);
        var headBudget = limit - tailBudget;

        var headEnd = text.LastIndexOf('\n', Math.Min(headBudget, text.Length - 1));
        if (headEnd <= 0) headEnd = Math.Min(headBudget, text.Length);

        var tailStart = text.Length - tailBudget;
        if (tailStart < headEnd) tailStart = headEnd;
        var nl = text.IndexOf('\n', tailStart);
        if (nl >= 0 && nl + 1 < text.Length) tailStart = nl + 1;

        var head = text[..headEnd];
        var tail = text[tailStart..];
        var cutLines = text.AsSpan(headEnd, Math.Max(0, tailStart - headEnd)).Count('\n');
        return (head, tail, cutLines);
    }
}
