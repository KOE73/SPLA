using SPLA.Domain.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.Agent.Composition;
using SPLA.MCP.Core.Composition;
using SPLA.MCP.Core.Plugins;
using SPLA.Library;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Agent;

/// <summary>
/// Runs a task headlessly in a fresh agent instance (new Conversation, new SkillSession).
/// Same code as the interactive agent — different entry point only.
/// <para>Two shapes, one loop. With a skill id the procedure is pinned up front — skill_activate /
/// skill_deactivate are bypassed, the sub-agent runs that one procedure, and the run ends when the
/// orchestrator loop finishes. Without one it is a plain delegated task: the base prompt, the seed
/// message, and the same tools. Delegation is the point of a sub-agent and a curated procedure is
/// only one way to describe the work, so requiring a skill for every spawn meant every ad-hoc
/// sub-task needed a file written for it first.</para>
/// </summary>
public sealed class SpawnedAgentRunner : Domain.Interfaces.IAgentSpawner
{
    private readonly Domain.Llm.ILlmGateway _llm;
    private readonly Domain.Interfaces.IToolHost _tools;
    private readonly SkillLibrary _skills;
    private readonly PluginManager _plugins;
    private readonly ResolvedSettings _settings;

    /// <summary>
    /// Asks what window the model actually has, when config did not say. Optional because the runner
    /// is constructible without a runtime (tests, a worker entry point) and a missing window costs
    /// only the percentage, not the figure.
    /// </summary>
    private readonly Func<LLMSettings, CancellationToken, Task<int?>>? _contextWindow;

    /// <summary>
    /// How many spawns deep the current async flow already is. A sub-agent reaches tools through the
    /// same <see cref="Domain.Interfaces.IToolHost"/> as its parent, so nothing stops it from calling
    /// agent_spawn again; the orchestrator's loop guard watches repeated tool calls, which is a
    /// different thing from recursion between agents.
    /// <para><see cref="AsyncLocal{T}"/> rather than a field on purpose: several chats spawn
    /// concurrently, and a shared counter would add their depths together.</para>
    /// </summary>
    private static readonly AsyncLocal<int> _depth = new();

    /// <summary>Spawns allowed below the top-level chat. Chosen above anything in use today, so this
    /// limit refuses runaway recursion without changing how existing skills behave.</summary>
    private const int MaxDepth = 3;

    /// <summary>How long a run will wait to learn the model's context window before starting without
    /// it. Short because this is a status figure, not the work: the run is worth more than the
    /// percentage on its label. Paid at most once a minute — the runtime caches the answer, and caches
    /// a failure as one too.</summary>
    private static readonly TimeSpan WindowLookupBudget = TimeSpan.FromSeconds(2);

    /// <summary>Where a finished run's conversation goes. Optional for the same reason
    /// <see cref="_contextWindow"/> is — the runner is constructible without a runtime, and a run
    /// nobody can read back is still a run that happened.</summary>
    private readonly SpawnedRunLog? _runLog;

    public SpawnedAgentRunner(
        Domain.Llm.ILlmGateway llm,
        Domain.Interfaces.IToolHost tools,
        SkillLibrary skills,
        PluginManager plugins,
        ResolvedSettings settings,
        Func<LLMSettings, CancellationToken, Task<int?>>? contextWindow = null,
        SpawnedRunLog? runLog = null)
    {
        _llm = llm;
        _tools = tools;
        _skills = skills;
        _plugins = plugins;
        _settings = settings;
        _contextWindow = contextWindow;
        _runLog = runLog;
    }

    /// <summary>
    /// Runs <paramref name="input"/> in a fresh conversation, optionally pinned to
    /// <paramref name="skillId"/>. Returns the last assistant message produced by the run.
    /// Throws <see cref="System.ArgumentException"/> if a named skill is not found.
    /// </summary>
    public async Task<string> RunAsync(
        string? skillId,
        string input,
        AgentMode mode,
        CancellationToken cancellationToken = default)
    {
        // Refused as text, not as an exception: the caller is a model reading a tool result, and a
        // sentence it can act on beats a stack trace it cannot.
        if (_depth.Value >= MaxDepth)
            return $"error: spawn depth limit reached ({MaxDepth}). " +
                   "A spawned agent cannot keep spawning; do the remaining work in this run.";

        // Identifies this run wherever it travels — on every progress tick, and as the key a reader
        // later hands back to find it in the log. Generated up front, before anything can fail, so a
        // run recorded in the finally block always has the same id its ticks already carried.
        var runId = "r-" + System.Guid.NewGuid().ToString("N")[..8];
        var startedAt = DateTimeOffset.UtcNow;

        // Fresh isolated agent state — own skill session, working memory, and checkpoint manager.
        // Opening an AgentSessionScope keeps the sub-agent's tool calls (memory, marks, skills) off
        // the parent chat's state, even though the spawn happens inside the parent's async flow.
        var skillSession = new SkillSession();

        // A free-form spawn leaves the session idle rather than pinned. That is not the same as an
        // agent without skills: the session is the sub-agent's own, so if the work turns out to match
        // one, it can find and activate it for itself without touching the parent's.
        if (!string.IsNullOrWhiteSpace(skillId))
        {
            var lookup = _skills.Resolve(skillId!);
            if (lookup.IsAmbiguous)
                throw new System.ArgumentException(
                    $"Skill '{skillId}' is held by more than one source — name one of: " +
                    string.Join(", ", lookup.Candidates.Select(c => c.Address)), nameof(skillId));

            var meta = lookup.Card;
            if (meta is null)
                throw new System.ArgumentException($"Skill '{skillId}' not found.", nameof(skillId));

            var body = _skills.LoadBody(meta.Address);
            if (string.IsNullOrWhiteSpace(body))
                throw new System.ArgumentException(
                    $"Skill '{skillId}' has no readable procedure.", nameof(skillId));

            // Same loan slip as an in-chat activation: a sub-agent running a skill needs that skill's
            // references as much as the parent would, and its own session is the only place to hold them.
            skillSession.Activate(meta.DisplayId, body, meta.SourceId, meta.Ref, _skills.ListResources(meta.Address));
        }

        var checkpoint = new CheckpointManager();
        // Everything else here is deliberately fresh, but the sandbox is inherited: it is the host's
        // boundary, not the agent's state. Left to its default a sub-agent would come out of its
        // parent's sandbox — a way out of the box by spawning, which matters the moment a chat runs
        // with a real one. No parent (a CLI or worker entry point) keeps the constructor's default.
        var agentSession = new AgentSession(
            new KeyValueStore("session"), checkpoint, skillSession,
            sandbox: AgentSessionScope.Current?.Sandbox);

        // The session is passed explicitly rather than resolved ambiently — the spawn happens inside
        // the parent's async flow, and the sub-agent must describe its own skill, not the parent's.
        var composer = new AgentContextComposer(
            AgentContributors.Default(_skills, _plugins, skillSession));
        var systemPrompt = composer.Compose(_settings, _settings.WorkspacePath).SystemPrompt;

        var conversation = new Conversation();
        conversation.Add(new ChatMessage { Role = ChatRole.System, Content = systemPrompt });
        conversation.Add(new ChatMessage { Role = ChatRole.User, Content = input });

        string lastAssistantMessage = string.Empty;

        // A pinned run keeps the prompt frozen: it has one procedure, cannot activate another, and a
        // stray skill_deactivate must not be able to delete the very instructions it was spawned to
        // follow. A free-form run gets the chat's per-iteration recomposition instead — with no skill
        // pinned, activating one mid-run is a legitimate move, and it is worth nothing if the
        // procedure never reaches the prompt.
        var context = skillSession.ActiveSkillId is null
            ? () => composer.Compose(_settings, _settings.WorkspacePath)
            : (Func<ComposedContext>?)null;

        // Spawned sub-agents are the most prone to tool-call loops; guard them too (tool-call only).
        //
        // NestInAmbientProgress is what stops the run being a black box. The spawn is happening inside
        // the caller's agent_spawn node; joining that node's tree rather than opening a new one makes
        // every tool the sub-agent runs a child of it, visible wherever the caller's progress already
        // is. Nothing is forwarded, subscribed or relayed — the tree is ambient, so not detaching from
        // it is the whole mechanism.
        var orchestrator = new ConversationOrchestrator(_llm, _tools)
        {
            Checkpoint = checkpoint,
            EnableLoopGuard = true,
            Context = context,
            NestInAmbientProgress = true
        };

        // Tool activity arrives on its own as child nodes. What only the runner can say is what happens
        // *between* the tools — that the run is on its fourth turn, that the model is thinking, what it
        // last concluded — and without it a sub-agent spending a minute inside the model looks exactly
        // like one that has hung.
        var llmSettings = _settings.ToLLMSettings();
        llmSettings.Mode = mode;

        // How full the run's context is, as the last call reported it. Carried on **every** tick rather
        // than announced in one of its own, for two reasons. It is the only quantity in an agent run
        // that means anything on a bar — there is no "percent done" for an agent, but there is a
        // ceiling it is walking towards. And a tick that carried it alone would be gone seconds later:
        // ticks are coalesced and the freshest message wins, so anything that must persist has to ride
        // along with whatever is being said now.
        //
        // This is also the number that makes a runaway run legible before it is expensive. A sub-agent
        // filling its window is doing something different from one working; until now neither was
        // visible, and there is still nothing that stops either (see the plan's turn-budget note).
        var promptTokens = 0;
        var contextWindow = llmSettings.ContextLength ?? 0;

        // Config rarely declares the window — a local runtime just loads whatever it loads — so ask
        // the provider when it did not.
        //
        // Waited for, briefly, rather than left to land on its own. Fire-and-forget was the first
        // attempt and it was wrong in a way a test caught: a short run finishes before the answer
        // arrives, so the percentage was missing precisely when the run was cheap and present when it
        // was slow — the opposite of a figure you can rely on. The bound is what keeps that honest: a
        // provider that is slow or gone costs this much once and no more, because the runtime caches
        // the answer either way, failures included.
        if (contextWindow <= 0 && _contextWindow is not null)
        {
            try
            {
                contextWindow = await _contextWindow(llmSettings, cancellationToken)
                    .WaitAsync(WindowLookupBudget, cancellationToken) ?? 0;
            }
            catch (TimeoutException) { /* unknown; the count still travels without a percentage */ }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { }
        }

        var turn = 0;
        ToolProgress Tick(string message) => new()
        {
            Message = message + Context(promptTokens, contextWindow),
            // Only as a pair, and only for the bar. Current on its own draws nothing — there is no
            // fraction without a denominator — and would put the same number on the line twice, since
            // the message already carries it in a form a person reads.
            Current = promptTokens > 0 && contextWindow > 0 ? promptTokens : null,
            Total   = promptTokens > 0 && contextWindow > 0 ? contextWindow : null,
            // On every tick, not just the first: ticks are coalesced and the freshest one wins (see
            // promptTokens above), so anything that must survive to the last tick has to ride along
            // with whatever is being said now. The run id is how a reader later points at this branch
            // in the log — it has nowhere else to live once the tree has moved past the first tick.
            Details = new[] { new ToolProgressDetail("run", runId) }
        };

        var callbacks = new AgentCallbacks
        {
            OnLlmTurnStart = _ =>
            {
                ProgressScope.Report(Tick($"turn {++turn}, thinking"));
                return Task.CompletedTask;
            },
            // Fires once per model call, after it returns. PromptTokens is what the provider counted
            // for the context it was sent — the honest figure, not our estimate of it.
            OnLlmTurn = result =>
            {
                if (result.Message.PromptTokens is int used and > 0) promptTokens = used;
            },
            OnAssistantMessage = msg =>
            {
                lastAssistantMessage = msg.Content ?? string.Empty;
                var said = Excerpt(msg.Content, 56);
                if (said is not null)
                    ProgressScope.Report(Tick($"turn {turn}: {said}"));
                return Task.CompletedTask;
            }
        };

        // What the run will be called in the tree. The tool's own node says "agent_spawn" — the same
        // line for every delegation — so this is the only place the task can be named.
        var label = skillSession.ActiveSkillId ?? Excerpt(input, 40) ?? "task";

        // Counted around the run itself, and restored rather than decremented — the same shape the
        // ambient scopes in this codebase use, so an exception cannot leave the depth raised.
        var previousDepth = _depth.Value;
        _depth.Value = previousDepth + 1;

        // A node of the run's own, under the tool's. Without it a batch is a lie: agent_spawn_batch
        // runs its tasks on parallel flows that all inherit the same current node, so three sub-agents
        // would hang their tool calls off the batch as one undifferentiated row of siblings — the tree
        // would show what was done and lose who did it. With it each run is a branch that says which
        // task it is, and the flattened line reads agent_spawn_batch › audit ports › port_scan.
        using var runNode = ProgressScope.BeginNode(label);
        // Defaults to "failed" rather than left unassigned: an exception the two catches below do not
        // name (a StackOverflowException, say) still has to leave the log with an honest outcome, not
        // whatever the compiler would have to invent to let this compile.
        var outcome = "failed";
        string? error = null;
        try
        {
            using (AgentSessionScope.Begin(agentSession))
                await orchestrator.RunAsync(conversation, llmSettings, mode, callbacks, cancellationToken);
            outcome = "completed";
        }
        catch (OperationCanceledException)
        {
            runNode.Fail();
            outcome = "cancelled";
            throw;
        }
        catch (Exception ex)
        {
            runNode.Fail();
            outcome = "failed";
            error = ex.Message;
            throw;
        }
        finally
        {
            _depth.Value = previousDepth;

            // Skipped without a log rather than falling back to some other store: a runner built for
            // a worker or a test has nowhere honest to put a transcript, and pretending otherwise would
            // mean inventing a second retention policy nobody asked for.
            _runLog?.Record(new SpawnedRun
            {
                Id = runId,
                Label = label,
                SkillId = skillSession.ActiveSkillId,
                Mode = mode.ToString(),
                StartedAt = startedAt,
                FinishedAt = DateTimeOffset.UtcNow,
                Outcome = outcome,
                Error = error,
                Result = lastAssistantMessage,
                Messages = conversation.Messages.ToList()
            });
        }

        return lastAssistantMessage;
    }

    /// <summary>
    /// How much of the window the run is using, as a suffix — or nothing at all before the first call
    /// has reported.
    /// <para>
    /// The sentence says only what the numbers <b>mean</b>, because when the window is known the
    /// numbers themselves travel in <see cref="ToolProgress.Current"/> and <c>Total</c>, and every
    /// renderer that has those appends them in its own way. Saying them here as well printed the same
    /// figure twice on one line — visible the moment this went out over MCP. With no window there are
    /// no fields to set, so the count has nowhere else to be and the sentence carries it.
    /// </para>
    /// </summary>
    private static string Context(int promptTokens, int contextWindow)
    {
        if (promptTokens <= 0) return string.Empty;

        return contextWindow > 0
            ? $" · ctx {100L * promptTokens / contextWindow}%"
            : $" · ctx {Tokens(promptTokens)}";
    }

    /// <summary>
    /// Token counts as a person reads them: 850, 12.4k, 128k.
    /// <para>Invariant on purpose. The line this lands in is English, and on a machine with a comma
    /// decimal separator the count came out "1,2k" — the same half-translated screen the CLI help had,
    /// and worse here because the string also goes out over MCP to a reader whose locale is not this
    /// machine's.</para>
    /// </summary>
    private static string Tokens(int count) => count switch
    {
        < 1000 => count.ToString(System.Globalization.CultureInfo.InvariantCulture),
        < 100_000 => (count / 1000.0).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "k",
        _ => (count / 1000).ToString(System.Globalization.CultureInfo.InvariantCulture) + "k"
    };

    /// <summary>
    /// The first line of <paramref name="text"/>, no longer than <paramref name="limit"/>. Null when
    /// there is nothing to show — a turn whose only output was tool calls has no narrative, and
    /// repeating the previous one would be worse than saying nothing.
    /// </summary>
    private static string? Excerpt(string? text, int limit)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var line = text.AsSpan().Trim();
        var br = line.IndexOfAny('\r', '\n');
        if (br >= 0) line = line[..br].TrimEnd();
        if (line.IsEmpty) return null;

        return line.Length <= limit ? line.ToString() : string.Concat(line[..limit].TrimEnd(), "…");
    }
}
