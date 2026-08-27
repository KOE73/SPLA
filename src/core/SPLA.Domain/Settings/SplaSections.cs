using YamlDotNet.Serialization;

namespace SPLA.Domain.Settings;

/// <summary>
/// Agent behavior section — mode and instruction files.
/// </summary>
public class SplaAgentSection
{
    [YamlMember(Alias = "mode")]
    public string? Mode { get; set; }

    [YamlMember(Alias = "instructions")]
    public List<string>? Instructions { get; set; }

    [YamlMember(Alias = "compact_tail_messages")]
    public int? CompactTailMessages { get; set; }

    [YamlMember(Alias = "custom_prompt")]
    public string? CustomPrompt { get; set; }

    /// <summary>Guard against machine-gun tool loops (a small-local-model failure mode). Only
    /// rapid identical calls with identical results and no commentary count; the first trip asks
    /// the model in-band whether it is stuck, a rebuilt streak stops the turn. **On by default** —
    /// deliberate polling (ssh_session_wait etc.) changes the result or takes longer than the ten-second
    /// window, so it does not trip. Set false here for a project where it proves otherwise.</summary>
    [YamlMember(Alias = "loop_guard")]
    public bool? LoopGuard { get; set; }

    /// <summary>How many suspicious consecutive repeats trigger each stage (default 3).</summary>
    [YamlMember(Alias = "loop_guard_repeats")]
    public int? LoopGuardRepeats { get; set; }

    /// <summary>
    /// Whether the resource-address abstraction (<c>file://</c>, <c>sftp://</c>, …) is announced to
    /// the model at all. **Off by default, deliberately** — the whole point of a switch here is to be
    /// able to run the project with and without the feature and compare, and a default of true would
    /// make that comparison impossible to reproduce months later when "off" quietly became the
    /// unusual case. See <c>ResourceSchemesContributor</c> and <c>SplaSections.SplaProject.Resources</c>
    /// for the per-scheme switches this master flag gates.
    /// </summary>
    [YamlMember(Alias = "unified_resources")]
    public bool? UnifiedResources { get; set; }

    /// <summary>Minutes an unanswered permission/clarify question is kept before it is denied.
    /// Generous on purpose — a person who walked away should be able to come back and answer —
    /// and 0 means no limit at all. See <c>PendingAskStore</c>.</summary>
    [YamlMember(Alias = "ask_timeout_minutes")]
    public int? AskTimeoutMinutes { get; set; }

    /// <summary>Enabled built-in agent capabilities (dotted "core.*" feature ids — see
    /// <c>SPLA.MCP.Core.Agent.AgentFeatureCatalog</c>). Null (key absent) = every feature enabled,
    /// the historical behaviour. Empty list = no built-in feature (only the mode preamble,
    /// instructions, custom_prompt, and plugins remain). Unknown ids are ignored with a warning;
    /// a feature's dependencies are auto-enabled.</summary>
    [YamlMember(Alias = "capabilities")]
    public List<string>? Capabilities { get; set; }

    /// <summary>
    /// Domains the operator vouches for. Naming a source is what makes it a named one, so content
    /// fetched from these stops being part of the open web and stops raising the chat's doubt flag —
    /// an internal wiki is not a stranger's page.
    ///
    /// <para>Matched on the host, with subdomains included: <c>corp.local</c> covers
    /// <c>wiki.corp.local</c>. Accumulates across layers rather than being overridden, because a
    /// project vouching for its own wiki must not silently drop what the machine layer vouched
    /// for.</para>
    /// </summary>
    [YamlMember(Alias = "trusted_domains")]
    public List<string>? TrustedDomains { get; set; }

    /// <summary>Persist the full tool-call/tool-result trace (arguments and outputs) alongside the
    /// human-readable chat, not just the final text. Off by default — most of that trace is
    /// diagnostic noise nobody re-reads, and it bloats the chat file. Turn on when you actually need
    /// to reconstruct exactly what an agent did, not just what it said.</summary>
    [YamlMember(Alias = "save_tool_calls")]
    public bool? SaveToolCalls { get; set; }

    /// <summary>Persist abandoned-generation records — the repetition guard's discarded attempts,
    /// captured text included — alongside the chat. Off by default: each one can run to several kB,
    /// and most chats never trip the guard at all. Deliberately a bool, not a set of levels: the
    /// generation that succeeded IS the final message and is always stored regardless of this flag,
    /// so there is no third thing to choose between — this only decides whether the ones that were
    /// thrown away are kept too.</summary>
    [YamlMember(Alias = "save_attempts")]
    public bool? SaveAttempts { get; set; }
}

/// <summary>
/// LLM connection section.
/// </summary>
public class SplaLlmSection
{
    [YamlMember(Alias = "provider")]
    public string? Provider { get; set; }

    [YamlMember(Alias = "endpoint")]
    public string? Endpoint { get; set; }

    [YamlMember(Alias = "api_key")]
    public string? ApiKey { get; set; }

    [YamlMember(Alias = "model")]
    public string? Model { get; set; }

    [YamlMember(Alias = "temperature")]
    public double? Temperature { get; set; }

    [YamlMember(Alias = "reasoning_level")]
    public string? ReasoningLevel { get; set; }

    [YamlMember(Alias = "presence_penalty")]
    public double? PresencePenalty { get; set; }

    [YamlMember(Alias = "frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    [YamlMember(Alias = "repeat_penalty")]
    public double? RepeatPenalty { get; set; }

    [YamlMember(Alias = "max_tokens")]
    public int? MaxTokens { get; set; }

    [YamlMember(Alias = "top_p")]
    public double? TopP { get; set; }

    [YamlMember(Alias = "min_p")]
    public double? MinP { get; set; }
}

/// <summary>
/// A connection to one provider account: transport and credentials, plus the models selected under
/// it. It is a container — a chat never points here, it points at one of the <see cref="Models"/>.
/// <para>
/// The split exists because a key and an endpoint are shared by every model reached through them:
/// five OpenRouter models under one key must not mean five copies of that key. Everything that is a
/// property of the <i>account</i> (credentials, rate limits, balance, reachability) belongs here;
/// everything that is a property of the <i>model</i> belongs on the leaf.
/// </para>
/// </summary>
public class SplaConnectionSection
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Display label for the connection. Falls back to <see cref="Id"/> — never to the
    /// provider name, which cannot distinguish two connections to the same provider.</summary>
    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    [YamlMember(Alias = "provider")]
    public string? Provider { get; set; }

    [YamlMember(Alias = "endpoint")]
    public string? Endpoint { get; set; }

    /// <summary>The inference credential. A <c>secret:</c> reference in practice.</summary>
    [YamlMember(Alias = "api_key")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Optional account-management credential, separate from <see cref="ApiKey"/> because providers
    /// keep them separate: OpenRouter's balance needs a management key, OpenAI's cost endpoints an
    /// admin key, Anthropic's an <c>sk-ant-admin</c>. Null for every local provider and for anyone
    /// who does not need account figures — inference never uses it.
    /// </summary>
    [YamlMember(Alias = "admin_key")]
    public string? AdminKey { get; set; }

    /// <summary>When true picking a different model triggers LM Studio unload+load via the management API.</summary>
    [YamlMember(Alias = "swap_model")]
    public bool SwapModel { get; set; }

    /// <summary>The models selected under this connection. Each is what a chat can point at.</summary>
    [YamlMember(Alias = "models")]
    public List<SplaModelSection> Models { get; set; } = new();

    /// <summary>Display label for the connection tree. Computed, never persisted.</summary>
    [YamlIgnore]
    public string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name! : Id;
}

/// <summary>
/// One model selected under a connection — the leaf a chat points at by <see cref="Id"/>.
/// <para>
/// <see cref="Id"/> is ours and must be globally unique across the project (chats reference it flat,
/// without the owning connection); <see cref="Model"/> is the provider's own string and goes on the
/// wire verbatim. Two entries may carry the same <see cref="Model"/> under different connections —
/// that is the point: "opus on the work key" and "opus on the personal key" are different choices.
/// </para>
/// </summary>
public class SplaModelSection
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    /// <summary>The model identifier sent to the provider (<c>anthropic/claude-opus-4</c>, an LM
    /// Studio key, …). "auto" or empty = let the provider decide.</summary>
    [YamlMember(Alias = "model")]
    public string? Model { get; set; }

    /// <summary>
    /// Manual context-window override in tokens. When set it wins over any auto-detected value
    /// (LM Studio native API / vLLM <c>max_model_len</c>) — for providers that report nothing, or
    /// when the user knows better. Null/0 = auto-detect.
    /// </summary>
    [YamlMember(Alias = "context_length")]
    public int? ContextLength { get; set; }

    /// <summary>
    /// Manual declaration of the model's reasoning options, in the provider's own words
    /// (<c>["off","low","medium","high"]</c>). Same role as <see cref="ContextLength"/> and the same
    /// precedence: when set it wins over anything the provider advertises, and it is the only way to
    /// get the lever for a server that describes nothing — most OpenAI-compatible endpoints, LocalAI
    /// and plain vLLM among them. Null = take the provider's word, or leave the lever unavailable.
    /// </summary>
    [YamlMember(Alias = "reasoning_options")]
    public List<string>? ReasoningOptions { get; set; }

    /// <summary>The option this model uses when asked for nothing. Only read alongside
    /// <see cref="ReasoningOptions"/>.</summary>
    [YamlMember(Alias = "reasoning_default")]
    public string? ReasoningDefault { get; set; }

    /// <summary>Display label for the picker — falls back to the wire model string, then the id.</summary>
    [YamlIgnore]
    public string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name!
        : !string.IsNullOrWhiteSpace(Model) ? Model!
        : Id;
}

/// <summary>
/// One declared mount: a folder outside the project root, given a name and addressed under
/// <c>mnt/&lt;name&gt;/...</c>. See <c>agents/spla-file.md</c> and
/// <c>docs/adr/ADR_20260814_core_project-mounts.md</c>.
///
/// <para>Everything here is what the file said. Validation and resolution happen once, at load, in
/// <see cref="MountResolver"/> — never in a tool and never at use.</para>
/// </summary>
public class SplaMountSection
{
    /// <summary>The address segment. Travels in git; it is what instructions and prompts name.</summary>
    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    /// <summary>Mount kind. Only <c>file-system</c> exists; the key is here so a second one would be
    /// an addition rather than a break.</summary>
    [YamlMember(Alias = "type")]
    public string? Type { get; set; }

    /// <summary>Where it points on this machine. Relative paths are relative to the directory holding
    /// the manifest — a property of the machine, which is exactly why it is the half that may differ
    /// between two checkouts.</summary>
    [YamlMember(Alias = "path")]
    public string? Path { get; set; }

    /// <summary><c>read</c> (default) or <c>write</c>. Read-only unless opted in, the same way an SSH
    /// host is.</summary>
    [YamlMember(Alias = "access")]
    public string? Access { get; set; }

    /// <summary><c>trusted</c> (default) or <c>untrusted</c>. A mount is a source the operator named,
    /// so it is trusted like one; <c>untrusted</c> is for a folder other people write into.</summary>
    [YamlMember(Alias = "trust")]
    public string? Trust { get; set; }

    /// <summary>Required. Goes into the system prompt — without a line saying what the folder is for,
    /// the model opens it to find out.</summary>
    [YamlMember(Alias = "description")]
    public string? Description { get; set; }
}

/// <summary>
/// Secret-store backend selection. Machine-level only (<c>~/.spla/defaults.yaml</c>) — the backend
/// is a property of the machine, never of a committable project. To run a second instance on a
/// different backend without editing this file, point it at an isolated home via <c>SPLA_HOME</c>.
/// </summary>
public class SplaSecretsSection
{
    /// <summary><c>file</c> (default, plaintext) or <c>dpapi</c> (Windows-encrypted). Unknown/empty = file.</summary>
    [YamlMember(Alias = "backend")]
    public string? Backend { get; set; }
}

/// <summary>
/// The <c>mcp:</c> section — deliberately both halves of the wire in one place. <see cref="Enabled"/>
/// and <see cref="Port"/> are the outward half: whether <c>spla serve</c> maps <c>POST /mcp</c> at all
/// and what fixed port to bind, so the address is predictable instead of the usual ephemeral one (see
/// <c>SplaServiceHost.HandleMcpAsync</c>). <see cref="Servers"/> is the inward half: which foreign MCP
/// servers this project consumes. They read as unrelated features, but they are the same wire read in
/// opposite directions — one section that says "who we are as an MCP peer" beats two sections that
/// each have to be found and kept in sync separately.
/// </summary>
public class SplaMcpSection
{
    /// <summary>Null/absent = off (the default) — the project's writer-lease model stays strict, no
    /// second head over HTTP or otherwise. Set true to opt in and offer <c>POST /mcp</c>.</summary>
    [YamlMember(Alias = "enabled")]
    public bool? Enabled { get; set; }

    /// <summary>A fixed port for <c>spla serve</c> to bind, so a client (or a person configuring one)
    /// can hardcode <c>http://127.0.0.1:&lt;port&gt;/mcp</c> instead of reading the ephemeral one out
    /// of the instance lock file each time. Null = ephemeral, as before this section existed. An
    /// explicit <c>--port</c> on the command line still wins over this — a CLI flag typed for this run
    /// is a stronger statement than whatever the project remembers.</summary>
    [YamlMember(Alias = "port")]
    public int? Port { get; set; }

    /// <summary>Foreign MCP servers this project consumes — see <see cref="SplaMcpServerSection"/>.
    /// Null/absent = none declared. Merged across layers by <see cref="SplaMcpServerSection.Id"/> in
    /// <c>SettingsResolver</c>, the same way <c>connections:</c> merges.</summary>
    [YamlMember(Alias = "servers")]
    public List<SplaMcpServerSection>? Servers { get; set; }
}

/// <summary>
/// One foreign MCP server this project consumes, by stdio or HTTP. Connecting, projecting its tools
/// and enforcing anything about them is later work (see
/// <c>docs/plans/PLAN_20260826_service_mcp-client.md</c> steps 2/3/5) — this type is only the
/// declaration: what the operator wrote down about the server, unresolved and unvalidated.
/// </summary>
public class SplaMcpServerSection
{
    /// <summary>
    /// The prefix every tool of this server gets once connected (<c>ghmcp_create_issue</c>). Must
    /// match <c>^[a-z][a-z0-9_]{0,15}$</c> — enforced where the prefix is actually applied
    /// (<c>McpToolNaming</c>, step 3 of the plan above), not here, so there is exactly one place that
    /// decides what a legal id looks like.
    /// <para>
    /// Changing this later breaks stored grants and chat history: a remembered permission and a
    /// logged tool call both name the prefixed tool, and renaming the server renames every tool it
    /// ever offered out from under them. Treat it as load-bearing, not cosmetic, once a server has
    /// been connected even once.
    /// </para>
    /// </summary>
    [YamlMember(Alias = "id")]
    public string? Id { get; set; }

    /// <summary>Display label. Falls back to <see cref="Id"/> in the UI, never persisted as a
    /// fallback — same convention as <see cref="SplaConnectionSection.Name"/>.</summary>
    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    /// <summary>A disabled server is not connected at all — its tools and its tool set do not exist,
    /// the same way a disabled plugin's toolset is absent rather than merely hidden. Null/absent =
    /// enabled, so an operator who writes a server entry sees it come up without also having to say
    /// <c>enabled: true</c>.</summary>
    [YamlMember(Alias = "enabled")]
    public bool? Enabled { get; set; }

    /// <summary><c>stdio</c> or <c>http</c>. Picks which of the fields below apply.</summary>
    [YamlMember(Alias = "transport")]
    public string? Transport { get; set; }

    /// <summary>stdio only: the executable to launch (<c>npx</c>, <c>uvx</c>, a local binary).</summary>
    [YamlMember(Alias = "command")]
    public string? Command { get; set; }

    /// <summary>stdio only: arguments passed to <see cref="Command"/>.</summary>
    [YamlMember(Alias = "args")]
    public List<string>? Args { get; set; }

    /// <summary>stdio only: working directory for the launched process. Null = inherit.</summary>
    [YamlMember(Alias = "cwd")]
    public string? Cwd { get; set; }

    /// <summary>
    /// stdio only: environment variables for the launched process. Values are <c>secret:</c>/<c>env:</c>
    /// references, resolved at connect time — never plaintext in this file and never sent to a client.
    /// See <c>agents/secrets.md</c> (§1 invariants 1–4, §3 reference forms): config holds a pointer,
    /// the store holds the material, and resolution happens host-side, at the point of use, into a
    /// value that is never stored back onto this object.
    /// </summary>
    [YamlMember(Alias = "env")]
    public Dictionary<string, string>? Env { get; set; }

    /// <summary>http only: the server's endpoint URL.</summary>
    [YamlMember(Alias = "url")]
    public string? Url { get; set; }

    /// <summary>
    /// http only: request headers. Values are <c>secret:</c>/<c>env:</c> references, resolved at
    /// connect time — same rule and same citation as <see cref="Env"/>: never plaintext here, never
    /// sent to a client, materialized host-side and dropped.
    /// </summary>
    [YamlMember(Alias = "headers")]
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>Free-text description, shown in the UI and folded into the tool descriptions this
    /// server's tools get so the model sees the source in the card itself.</summary>
    [YamlMember(Alias = "description")]
    public string? Description { get; set; }

    /// <summary>
    /// <c>unnamed</c> (default) or <c>named</c>. The operator named the *pipe*, not what flows through
    /// it: declaring a server in config is an act of configuration, not an act of vouching for the
    /// content its tools return. Default <c>unnamed</c> means results from this server raise the
    /// chat's doubt flag (<c>ChatDoubt</c>/<c>DataOrigin</c>), the same way an untrusted fetch does.
    /// <c>named</c> is the explicit opt-out — the same deliberate act as adding a host to
    /// <c>trusted_domains</c>, not a side effect of typing a URL or a command.
    /// </summary>
    [YamlMember(Alias = "origin")]
    public string? Origin { get; set; }

    /// <summary>Optional convenience mirroring this server's entry in the <c>toolsets:</c> section
    /// (the tool set id is this server's <see cref="Id"/>). Lets an operator set the disclosure level
    /// for the whole server right next to its declaration instead of in a second section; equivalent
    /// to writing the same value under <c>toolsets: {&lt;id&gt;: ...}</c>.</summary>
    [YamlMember(Alias = "level")]
    public string? Level { get; set; }
}

/// <summary>
/// UI preferences section.
/// </summary>
public class SplaUiSection
{
    [YamlMember(Alias = "theme")]
    public string? Theme { get; set; }

    [YamlMember(Alias = "density")]
    public string? Density { get; set; }
}

/// <summary>
/// Per-effect permission overrides. Values: "allow", "ask", "deny".
/// </summary>
public class SplaPermissionsSection
{
    [YamlMember(Alias = "read")]
    public string? Read { get; set; }

    [YamlMember(Alias = "write")]
    public string? Write { get; set; }

    [YamlMember(Alias = "shell")]
    public string? Shell { get; set; }

    [YamlMember(Alias = "internet")]
    public string? Internet { get; set; }

    /// <summary>Override for foreign MCP-server tools (ToolScope.Foreign).</summary>
    [YamlMember(Alias = "foreign")]
    public string? Foreign { get; set; }

    [YamlMember(Alias = "tools")]
    public List<SplaToolPermissionRule>? Tools { get; set; }
}

public class SplaToolPermissionRule
{
    [YamlMember(Alias = "tool")]
    public string? Tool { get; set; }

    [YamlMember(Alias = "arguments")]
    public string? Arguments { get; set; }

    [YamlMember(Alias = "decision")]
    public string? Decision { get; set; }
}
