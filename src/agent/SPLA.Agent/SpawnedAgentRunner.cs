using SPLA.Domain.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.Agent.Composition;
using SPLA.MCP.Core.Composition;
using SPLA.MCP.Core.Plugins;
using SPLA.MCP.Core.ToolSets;
using SPLA.Library;
using System;
using System.Collections.Generic;
using System.IO;
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

    /// <summary>
    /// What turns a spawn into a real session — see
    /// <c>docs/adr/ADR_20260902_core_session-unification.md</c> §2.1. Optional for the same reason
    /// <see cref="_contextWindow"/> is: the runner is constructible without a runtime (tests, a worker
    /// entry point), and without one a run keeps the pre-wave-2 behaviour — an isolated in-memory
    /// conversation nobody can read back afterwards, but a run that still happens.
    /// <para>Settable rather than a constructor parameter: <c>AgentRuntime</c> builds this runner
    /// before its project's <c>ChatRegistry</c> exists (see <c>AgentRuntime.cs</c>'s own comment on
    /// the <see cref="_contextWindow"/> callback for the identical reason), so the registry attaches
    /// itself in its own constructor — every way of building one, not whichever caller remembered.</para>
    /// </summary>
    private Domain.Interfaces.ISpawnSessionHost? _sessionHost;

    /// <summary>Attaches the real session host once it exists. See <see cref="_sessionHost"/>.</summary>
    public void AttachSessionHost(Domain.Interfaces.ISpawnSessionHost host) => _sessionHost = host;

    /// <summary>
    /// The project's tool-set catalogue — what exists and which set each tool name belongs to. Used
    /// only to build a per-run tool filter (see <see cref="RunAsync"/>): this runner never mutates it
    /// and never asks it to gate anything on its own settings, because those settings are the
    /// project's, not this run's. Optional for the same reason <see cref="_contextWindow"/> is: a
    /// runner built for a test or a worker entry point has no registry, and without one a run simply
    /// applies mode-only gating, exactly as before roles narrowed tool sets at all.
    /// </summary>
    private readonly ToolSetRegistry? _toolSets;

    /// <summary>The manifest, for its <c>roles:</c> list. Read off the settings the runner already
    /// holds rather than attached afterwards: an attach-later hook is called by one caller and
    /// silently not by the rest, and that had already happened here — roles resolved in tests and
    /// nowhere else, so every real spawn naming one would have been refused.</summary>
    private SplaProject? Manifest => _settings.Manifest;

    public IReadOnlyList<string> GetAvailableRoles() =>
        Manifest?.Roles is { Count: > 0 } roles ? roles.AsReadOnly() : [];

    public SpawnedAgentRunner(
        Domain.Llm.ILlmGateway llm,
        Domain.Interfaces.IToolHost tools,
        SkillLibrary skills,
        PluginManager plugins,
        ResolvedSettings settings,
        Func<LLMSettings, CancellationToken, Task<int?>>? contextWindow = null,
        Domain.Interfaces.ISpawnSessionHost? sessionHost = null,
        ToolSetRegistry? toolSets = null)
    {
        _llm = llm;
        _tools = tools;
        _skills = skills;
        _plugins = plugins;
        _settings = settings;
        _contextWindow = contextWindow;
        _sessionHost = sessionHost;
        _toolSets = toolSets;
    }

    /// <summary>
    /// Runs <paramref name="input"/> in a fresh conversation, optionally pinned to
    /// <paramref name="skillId"/> and/or run under a <paramref name="role"/>.
    /// Returns the last assistant message produced by the run.
    /// Throws <see cref="System.ArgumentException"/> if a named skill is not found.
    /// Throws <see cref="System.InvalidOperationException"/> if a named role is not declared or cannot be loaded.
    /// </summary>
    public async Task<string> RunAsync(
        string? skillId,
        string input,
        AgentMode mode,
        string? role = null,
        CancellationToken cancellationToken = default)
    {
        // Refused as text, not as an exception: the caller is a model reading a tool result, and a
        // sentence it can act on beats a stack trace it cannot.
        if (_depth.Value >= MaxDepth)
            return $"error: spawn depth limit reached ({MaxDepth}). " +
                   "A spawned agent cannot keep spawning; do the remaining work in this run.";

        var startedAt = DateTimeOffset.UtcNow;

        // Which chat this spawn is under — read ambiently, the same way the sandbox already is a few
        // lines below, so a spawn from a human chat AND a spawn from a spawned session (recursion,
        // trap 9) both get the right parent: whichever session actually made the call.
        var parentChatId = AgentSessionScope.Current?.ChatId;

        // Validate and resolve role if one is named. If a role is declared but its body cannot be
        // loaded, validation throws InvalidOperationException and we never reach the session open.
        ResolvedSettings runSettings = _settings;
        if (!string.IsNullOrWhiteSpace(role))
        {
            if (Manifest is null || _settings.ProjectFilePath is null)
                throw new InvalidOperationException(
                    $"Role '{role}' was named, but this run has no project manifest to declare it in.");

            // Beside the manifest, not beside the workspace: a project may point its workspace
            // somewhere else entirely, and roles travel with the manifest in git (ADR_20260827-2).
            var manifestDirectory = Path.GetDirectoryName(_settings.ProjectFilePath)!;
            var roleSection = ConfigLoader.LoadRole(manifestDirectory, role!);
            runSettings = SettingsResolver.ResolveForRole(_settings, Manifest, role!, roleSection);
        }

        // Which mode this run actually executes in. A role picks its own mode the way `agent:` does
        // (ADR_20260827-2 §2.1: "the role chooses the mode and narrows within it, rather than
        // becoming a second axis of rights beside modes") — so once a role is named, its resolved
        // mode governs, whether that came from the role's own `mode:` or (the role saying nothing)
        // from the project's own. The `mode` ARGUMENT is what a role-less, ad-hoc spawn uses; it is
        // deliberately NOT consulted at all once a role is named. Letting a caller widen a role by
        // passing a different mode alongside it would make the role no boundary at all — the exact
        // failure this closes.
        var effectiveMode = !string.IsNullOrWhiteSpace(role) ? runSettings.Mode : mode;

        // A real session when a host is attached (chatId, file, ChatInbox, progress tree — see
        // ISpawnSessionHost); the pre-wave-2 in-memory-only shape otherwise (tests, a worker entry
        // point). Wave 3 wires a real role through agent_spawn.
        var session = _sessionHost?.OpenSpawnedSession(parentChatId, role);

        // Identifies this run wherever it travels — on every progress tick, and (with a session) as
        // the id subagent.get resolves back to this session's file. Generated up front, before
        // anything can fail, so a run whose finally block never gets to open a session still ticks
        // with a stable id for its whole life.
        var runId = session?.ChatId ?? "r-" + System.Guid.NewGuid().ToString("N")[..8];

        // Fresh isolated agent state — own skill session, working memory, and checkpoint manager.
        // Opening an AgentSessionScope keeps the sub-agent's tool calls (memory, marks, skills) off
        // the parent chat's state, even though the spawn happens inside the parent's async flow.
        // With a session this comes from it (so its Background/ChatId are the real ones); without one
        // it is built fresh here, inheriting only the sandbox — the host's boundary, not agent state,
        // and the one thing a sub-agent must not escape by spawning.
        var agentSession = session?.AgentSession
            ?? new AgentSession(new KeyValueStore("session"), new CheckpointManager(), new SkillSession(),
                sandbox: AgentSessionScope.Current?.Sandbox);
        var skillSession = agentSession.Skills;

        // A free-form spawn leaves the session idle rather than pinned. That is not the same as an
        // agent without skills: the session is the sub-agent's own, so if the work turns out to match
        // one, it can find and activate it for itself without touching the parent's.
        //
        // Wrapped: a session's file is already written on disk by the time we get here (OpenSpawnedSession
        // saved it eagerly), so a validation failure that throws past this point must still close that
        // file out as failed rather than leaving it stuck "in progress" forever — nothing else would
        // ever call Finish for a run that never reached the try block below.
        try
        {
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
        }
        catch (System.ArgumentException ex)
        {
            if (session != null)
            {
                session.Finish(System.Array.Empty<ChatMessage>(), skillId, effectiveMode.ToString(), startedAt,
                    "failed", ex.Message);
                _sessionHost?.TrimSpawnedRetention(_settings.SpawnedRetention);
                session.Dispose();
            }
            throw;
        }

        // The session is passed explicitly rather than resolved ambiently — the spawn happens inside
        // the parent's async flow, and the sub-agent must describe its own skill, not the parent's.
        var composer = new AgentContextComposer(
            AgentContributors.Default(_skills, _plugins, skillSession));
        var systemPrompt = composer.Compose(runSettings, runSettings.WorkspacePath).SystemPrompt;

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
            ? () => composer.Compose(runSettings, runSettings.WorkspacePath)
            : (Func<ComposedContext>?)null;

        // Narrows the tool surface to what this run's settings actually select — see
        // <see cref="_toolSets"/>. Composed with (never instead of) mode gating: the mode filter runs
        // first, exactly as it would with no role at all, and this only ever removes tools mode
        // gating would have kept. Built fresh per run from the runner's own registry, so nothing here
        // mutates the shared <c>McpHost</c>/<c>ToolSetRegistry</c> the project and every other chat
        // share — the narrowing lives entirely in this closure and dies with this run.
        //
        // A role's own <c>toolsets:</c> entries win when it names a set (already layered onto
        // <c>runSettings.ToolSets</c> by <see cref="SettingsResolver.ResolveForRole"/>); a set the
        // role never mentions falls back to <c>_toolSets.LevelOf</c>, i.e. whatever the project itself
        // decided — inheritance, not an empty set. This is why a role with no tool selection at all
        // must narrow nothing: <c>runSettings.ToolSets</c> is then byte-for-byte the project's own
        // dictionary (see <c>SettingsResolver.CloneForRole</c>), so every lookup below falls through
        // to the exact same answer the registry would have given a role-less run.
        Func<IEnumerable<ToolDefinition>, AgentMode, IEnumerable<ToolDefinition>>? toolFilter =
            _toolSets is null
                ? null
                : (defs, m) => ToolModeFilter.Filter(defs, m)
                    .Where(t => IsDisclosedForRun(t.Function.Name, _toolSets, runSettings.ToolSets));

        // Spawned sub-agents are the most prone to tool-call loops; guard them too (tool-call only).
        //
        // NestInAmbientProgress is what stops the run being a black box. The spawn is happening inside
        // the caller's agent_spawn node; joining that node's tree rather than opening a new one makes
        // every tool the sub-agent runs a child of it, visible wherever the caller's progress already
        // is. Nothing is forwarded, subscribed or relayed — the tree is ambient, so not detaching from
        // it is the whole mechanism.
        var orchestrator = new ConversationOrchestrator(_llm, _tools)
        {
            Checkpoint = agentSession.Checkpoint,
            EnableLoopGuard = true,
            Context = context,
            NestInAmbientProgress = true,
            ToolFilter = toolFilter
        };

        // Tool activity arrives on its own as child nodes. What only the runner can say is what happens
        // *between* the tools — that the run is on its fourth turn, that the model is thinking, what it
        // last concluded — and without it a sub-agent spending a minute inside the model looks exactly
        // like one that has hung.
        var llmSettings = runSettings.ToLLMSettings();
        llmSettings.Mode = effectiveMode;

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
                await orchestrator.RunAsync(conversation, llmSettings, effectiveMode, callbacks, cancellationToken);
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

            // Skipped without a session rather than falling back to some other store: a runner built
            // for a worker or a test has nowhere honest to put a transcript, and pretending otherwise
            // would mean inventing a second retention policy nobody asked for.
            if (session != null)
            {
                session.Finish(conversation.Messages, skillSession.ActiveSkillId, effectiveMode.ToString(),
                    startedAt, outcome, error);
                // Only after Finish: trimming looks at what is on disk right now, and this run's own
                // file must already carry a non-null outcome or it would immediately be the newest
                // "in progress" session on the ring's wrong side (trap 5).
                _sessionHost?.TrimSpawnedRetention(_settings.SpawnedRetention);
                session.Dispose();
            }
        }

        return lastAssistantMessage;
    }

    /// <summary>
    /// Whether the model should see <paramref name="toolName"/> in this run, given this run's own
    /// tool-set levels (<paramref name="runToolSets"/> — the project's, or a role's narrowing of them)
    /// rather than whatever the shared <paramref name="registry"/> was built to answer for the chat
    /// that spawned this run. This deliberately re-implements the shape of
    /// <c>McpHost.IsDisclosed</c>/<c>ToolSetRefusal</c> rather than calling into it: those read
    /// <c>ToolSetRegistry</c>'s own captured <c>ResolvedSettings</c>, which is the project's baseline
    /// and is exactly what a role must be able to narrow — asking the registry itself would answer for
    /// the wrong settings.
    /// <para>
    /// A tool no set claims (<see cref="ToolSetRegistry.SetOfTool"/> returns null) is nobody's to gate
    /// and is always kept, same rule the registry itself uses. For a claimed tool, an explicit entry
    /// in <paramref name="runToolSets"/> wins; absent one, <paramref name="registry"/>'s own
    /// <see cref="ToolSetRegistry.LevelOf"/> answers — which is how a role that named no selection for
    /// a set inherits the project's standing decision instead of silently losing it.
    /// </para>
    /// <para><see cref="ToolSetLevel.SkillDemand"/> and <see cref="ToolSetLevel.AgentDemand"/> are
    /// "not disclosed unless raised" — same as a chat, checked against this run's own
    /// <see cref="AgentSessionScope.Current"/>, which is the spawned run's <c>AgentSession</c> for the
    /// whole time this is called (the caller opens that scope around the entire orchestrator run).
    /// </para>
    /// </summary>
    private static bool IsDisclosedForRun(
        string toolName, ToolSetRegistry registry, IReadOnlyDictionary<string, string> runToolSets)
    {
        var setId = registry.SetOfTool(toolName);
        if (setId is null) return true;

        var level = runToolSets.TryGetValue(setId, out var configured)
                    && ToolSetRegistry.TryParseLevel(configured, out var parsed)
            ? parsed
            : registry.LevelOf(setId);

        return level switch
        {
            ToolSetLevel.Enabled => true,
            ToolSetLevel.Disabled => false,
            _ => AgentSessionScope.Current?.ToolSets.IsActive(setId) == true
        };
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
