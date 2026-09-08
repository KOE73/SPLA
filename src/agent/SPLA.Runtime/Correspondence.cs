namespace SPLA.Runtime;

/// <summary>
/// Who opened a correspondence — this chat, or the correspondent. See
/// <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.2: "переписка помнит своего инициатора" — the one
/// field an entire "meeting" view (wave 7) is later derived from, rather than a dedicated entity with
/// its own owner and membership list.
/// </summary>
public enum CorrespondenceInitiator { Self, Correspondent }

/// <summary>
/// One live correspondence this chat holds with another role's chat — an entry in the set
/// <see cref="ChatRuntime.Correspondences"/> keeps, never a single link back to whoever spawned this
/// chat (ADR §2.2: "собеседники — множество, а не ссылка на породившего" — a reviewer needs both the
/// architect and the writer open at once).
/// <para>
/// Addressed by (<see cref="Role"/>, <see cref="InstanceNo"/>) — a system-issued ordinal, not the
/// caller-supplied <see cref="Purpose"/> text (PLAN_20260906 §2.1/2.3: "имя выдаёт система, а не
/// сочиняет модель"). <see cref="ChatId"/> never reaches the model (ADR §2.3: "идентификатор чата не
/// попадает в контекст модели вообще") — it lives here, in the runtime's own bookkeeping, not in
/// anything the prompt shows.
/// </para>
/// </summary>
public sealed class Correspondence
{
    /// <summary>The correspondent's role name.</summary>
    public required string Role { get; init; }

    /// <summary>The correspondent chat's own instance number — a <b>copy</b> of its
    /// <c>ChatSession.AsInstance</c>, not a number this chat hands out for itself
    /// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.1). That is the whole point of the ADR:
    /// two chats corresponding with the same architect must both call him <c>reply_architect_4</c>,
    /// because the four belongs to him. Together with <see cref="Role"/> it IS the address —
    /// <see cref="ReplyToolNaming.BuildToolName"/> spells it, and the dictionary key in
    /// <see cref="ChatRuntime"/> is exactly this pair.
    /// <para>Consequence, stated plainly because it surprises: the numbers a single chat holds are not
    /// 1, 2, 3. A chat may perfectly well hold only <c>reply_architect_7</c> — and the seven now means
    /// something ("that architect"), identically for everyone looking at him.</para></summary>
    public required int InstanceNo { get; init; }

    /// <summary>Why this correspondence was opened — free text, purely explanatory (PLAN_20260906
    /// §2.3: "адрес — номер, зачем открыли — фраза"). Never part of the tool-name address and never
    /// required: carried into the virtual reply tool's description so the reason survives even though
    /// the number does not say it. Empty when the caller did not say why.</summary>
    public string Purpose { get; init; } = "";

    /// <summary>The correspondent's chat id — the soft link. Resolved through
    /// <c>ChatRegistry.Locate</c>/<c>GetOrOpen</c> on every turn (<see cref="ChatRuntime.RefreshCorrespondences"/>),
    /// never cached as a direct <c>ChatRuntime</c> reference and never watched via a subscription
    /// (trap 3: <c>ChatRegistry.RuntimeClosed</c> fires on sleep too, not just death).</summary>
    public required string ChatId { get; init; }

    /// <summary>Who opened this correspondence — the seed a "meeting" graph (wave 7) is later grown
    /// from by following initiators across correspondences, rather than a dedicated entity (ADR §2.2,
    /// "Собрание — производный вид, а не сущность рантайма").</summary>
    public required CorrespondenceInitiator Initiator { get; init; }

    /// <summary>
    /// Public name of the chat that <i>introduced</i> these two, when a third party did — null for the
    /// ordinary case where one of the two opened the correspondence itself
    /// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.5).
    /// <para><b>Why a separate field and not a third <see cref="CorrespondenceInitiator"/> value.</b>
    /// <c>CorrespondenceGraph.BuildEdges</c> derives an edge's direction from finding exactly one
    /// <c>self</c> half and one <c>correspondent</c> half; a third value written on both halves of an
    /// introduced edge would make the pair match neither test and the edge would vanish from the graph
    /// silently. <see cref="Initiator"/> answers "which end did this edge start from", which stays a
    /// two-valued question even when neither end chose to start it — the introducer names one of them
    /// first, deterministically, and that one is the head.</para>
    /// <para>Persisted, and the group key the "meeting" view (wave 7 of ADR_20260827-2) gets for free:
    /// one meeting is every edge carrying the same <see cref="IntroducedBy"/>. Declared and
    /// round-tripped now; the introduction operation that writes it is the next wave's.</para>
    /// </summary>
    public string? IntroducedBy { get; init; }

    /// <summary>When a reply last crossed this correspondence, either direction. Null before the
    /// first one.</summary>
    public DateTimeOffset? LastReplyAt { get; set; }

    /// <summary>Consecutive replies exchanged on this correspondence — what wave 6's debounce/ceiling
    /// regulator (ADR §2.4: "с ростом глубины обмена растёт дебаунс") will read once it exists. Wave 4
    /// only counts it on send; nothing yet resets it on external energy (<c>Human</c>/<c>TaskResult</c>)
    /// or acts on its value.
    /// <para>
    /// Never reset — <c>ChatPump</c>'s own <c>_peerDepth</c> (the debounce counter ADR §2.4 actually
    /// describes) is a separate, per-chat number that DOES reset on external energy; this field is the
    /// lifetime count of replies THIS side has sent through this one address, which is exactly what
    /// wave 7б's graph wants for "how many replies crossed this edge" (ADR §2.5's last row). Two
    /// different questions, two different counters, same class name-ish concept — do not merge them.
    /// </para></summary>
    public int Depth { get; set; }

    /// <summary>
    /// Lifetime estimated size (<see cref="SPLA.MCP.Core.Composition.TokenEstimate.Of"/>) of every
    /// reply THIS side has sent through this correspondence — wave 7б's answer to "how much" (ADR
    /// §2.5's last row: "объём считается по репликам и токенам на переписку").
    /// <para>
    /// Deliberately NOT a slice of a turn's real provider usage: a turn does many things besides send
    /// this one reply (other tool calls, other correspondences, the model's own thinking), so
    /// attributing the whole turn's token count to the edge that woke it would be a confident lie. This
    /// is instead a plain, honestly-named estimate of exactly what crossed this edge — the text of the
    /// reply itself, nothing else — computed the same provider-free way the composition layer already
    /// estimates context size.
    /// </para></summary>
    public int VolumeEstimate { get; set; }

    /// <summary>
    /// The virtual <c>reply_&lt;role&gt;[_&lt;n&gt;]</c> tool name this correspondence answers to,
    /// decided once — by <see cref="ChatRuntime.OpenCorrespondence"/> — at the moment this record is
    /// created, and never recomputed afterwards (plan trap 11: "имя виртуального инструмента
    /// стабильно"; a persisted <see cref="InstanceNo"/> is restored as-is on reload for the same
    /// reason). A second correspondent of the same role arriving later gets its OWN, higher ordinal —
    /// it never reaches back and renames this one.
    /// </summary>
    public required string ToolName { get; init; }

    /// <summary>
    /// When this correspondence ended, or null while it is still open. Set — never a removal — when the
    /// correspondent's chat is archived or deleted: see
    /// <c>docs/adr/ADR_20260904_core_history-vs-current.md</c> §2.1, "архивация — надгробие, а не
    /// удаление". Striking the record instead would erase the only machine-readable evidence that these
    /// two ever corresponded, on the surviving side, permanently — the replies themselves stay in
    /// <c>messages:</c>, but "who, on what address" does not survive in prose.
    /// </summary>
    public DateTimeOffset? EndedAt { get; set; }

    /// <summary>Why it ended — <c>archived</c> or <c>deleted</c>. Kept apart from
    /// <see cref="EndedAt"/> because the two are different news to a reader of the history (the same
    /// distinction the notice text already makes), and null while <see cref="IsOpen"/>.</summary>
    public string? EndedReason { get; set; }

    /// <summary>Whether this correspondence can still carry a reply. The one gate the live tool surface
    /// asks: <c>ChatRuntime.Correspondences</c> exposes only open ones, so an ended correspondence stops
    /// offering its <c>reply_*</c> tool without anything else having to know it exists.</summary>
    public bool IsOpen => EndedAt is null;
}
