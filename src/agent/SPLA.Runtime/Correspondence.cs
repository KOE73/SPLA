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
/// Addressed by (<see cref="Role"/>, <see cref="Topic"/>) — the same pair wave 5's virtual
/// <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c> tool name is built from. <see cref="ChatId"/> never
/// reaches the model (ADR §2.3: "идентификатор чата не попадает в контекст модели вообще") — it lives
/// here, in the runtime's own bookkeeping, not in anything the prompt shows.
/// </para>
/// </summary>
public sealed class Correspondence
{
    /// <summary>The correspondent's role name.</summary>
    public required string Role { get; init; }

    /// <summary>Why this correspondence was opened. Required by the ADR precisely so two
    /// correspondents holding the same role can still be told apart in the tool-name address;
    /// wave 5's name normalisation is what actually enforces "non-empty" on the way in — this type
    /// stores whatever it is given.</summary>
    public required string Topic { get; init; }

    /// <summary>The correspondent's chat id — the soft link. Resolved through
    /// <c>ChatRegistry.Locate</c>/<c>GetOrOpen</c> on every turn (<see cref="ChatRuntime.RefreshCorrespondences"/>),
    /// never cached as a direct <c>ChatRuntime</c> reference and never watched via a subscription
    /// (trap 3: <c>ChatRegistry.RuntimeClosed</c> fires on sleep too, not just death).</summary>
    public required string ChatId { get; init; }

    /// <summary>Who opened this correspondence — the seed a "meeting" graph (wave 7) is later grown
    /// from by following initiators across correspondences, rather than a dedicated entity (ADR §2.2,
    /// "Собрание — производный вид, а не сущность рантайма").</summary>
    public required CorrespondenceInitiator Initiator { get; init; }

    /// <summary>When a reply last crossed this correspondence, either direction. Null before the
    /// first one.</summary>
    public DateTimeOffset? LastReplyAt { get; set; }

    /// <summary>Consecutive replies exchanged on this correspondence — what wave 6's debounce/ceiling
    /// regulator (ADR §2.4: "с ростом глубины обмена растёт дебаунс") will read once it exists. Wave 4
    /// only counts it on send; nothing yet resets it on external energy (<c>Human</c>/<c>TaskResult</c>)
    /// or acts on its value.</summary>
    public int Depth { get; set; }

    /// <summary>
    /// The virtual <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c> tool name this correspondence answers to,
    /// decided once — by <see cref="ChatRuntime.OpenCorrespondence"/> — at the moment this record is
    /// created, and never recomputed afterwards (plan trap 11: "имя виртуального инструмента
    /// стабильно"). This is what keeps a name from shifting under an already-issued call: a second
    /// correspondent of the same role arriving later gets the topic folded into ITS OWN name, but does
    /// not retroactively rename this one, even though the "more than one correspondent of this role"
    /// condition (ADR §2.3) has since become true for both.
    /// </summary>
    public required string ToolName { get; init; }
}
