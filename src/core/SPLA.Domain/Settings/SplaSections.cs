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
