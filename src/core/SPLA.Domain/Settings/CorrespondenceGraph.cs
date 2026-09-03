using SPLA.Domain.Models;

namespace SPLA.Domain.Settings;

/// <summary>
/// One "meeting" edge — a connected subtree of correspondences rooted at whoever invited first, per
/// <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.5: "Собрание — производный вид, а не сущность
/// рантайма". There is deliberately no <c>Meeting</c> type: the edge already carries everything a
/// "meeting" would need to show — who called whom, and how much each side said — so a caller wanting
/// the meeting itself follows edges from a root the same way the ADR says to.
/// <para>
/// Both directions are kept apart on purpose. This is the one place the project-wide graph
/// (<see cref="CorrespondenceGraph.Build"/>) is asked to be useful for
/// (ADR §2.5's last row): "перекос" — a role that only ever sends
/// (<see cref="RepliesFromCorrespondent"/> stuck at zero while <see cref="RepliesFromInitiator"/>
/// grows) or a role nobody answers back — is invisible in a single combined total but obvious the
/// moment the two directions sit side by side.
/// </para>
/// </summary>
public sealed record CorrespondenceEdge(
    string FromChatId, string FromRole,
    string ToChatId, string ToRole,
    string Topic,
    int RepliesFromInitiator, int VolumeFromInitiator,
    int RepliesFromCorrespondent, int VolumeFromCorrespondent);

/// <summary>
/// Assembles the project-wide "who talks to whom, and how much" graph purely from sessions on disk
/// (<see cref="ChatManager.ListChats"/>/<see cref="ChatManager.ListSpawnedChats"/>) — never from which
/// <c>ChatRuntime</c>s happen to be open right now. This is the point of wave 7б's decision 3: a
/// derived view must not change depending on which chats a person happens to have open, and
/// <see cref="ChatManager"/> is where sessions are read regardless of runtime state.
/// <para>
/// Each live correspondence is written independently by both sides it connects (the initiator's
/// session and the correspondent's), so the raw data on disk is a set of "half-edges" — one entry per
/// (owning chat, addressed chat, topic). This pairs them back into single <see cref="CorrespondenceEdge"/>
/// records, oriented from whichever side actually opened the correspondence (ADR §2.5: "ребро от
/// инициатора"). A half with no counterpart (the other party's session was deleted, or has simply not
/// saved since) still becomes an edge — a correspondence struck on the correspondent's own file but not
/// yet noticed by the surviving side is exactly the kind of stale, one-sided data a derived view must
/// tolerate rather than hide.
/// </para>
/// </summary>
public static class CorrespondenceGraph
{
    public static List<CorrespondenceEdge> Build(ChatManager manager)
    {
        // Archived chats are excluded on purpose: an archived correspondent cannot receive a reply
        // (RefreshCorrespondences already strikes those the moment a live chat notices), so its stale
        // half-edges would only ever show as a one-sided dead end. Human + spawned sessions are both
        // included — a spawned session's role is exactly what wave 5б's on-demand correspondent chats
        // are, and excluding them would silently drop every correspondence they hold.
        var sessions = manager.ListChats().Concat(manager.ListSpawnedChats()).ToList();
        var roleOf = sessions.ToDictionary(s => s.Id, s => RoleName(s));

        // Half-edges grouped by the unordered chat pair + topic — the same triple both sides of one
        // correspondence necessarily agree on (ChatRuntime.Correspond passes the identical topic string
        // into OpenCorrespondence on both ends).
        var groups = new Dictionary<(string A, string B, string Topic), List<(string OwnerChatId, ChatSessionCorrespondence Entry)>>();
        foreach (var s in sessions)
        {
            if (s.Correspondences is not { Count: > 0 }) continue;
            foreach (var c in s.Correspondences)
            {
                var key = PairKey(s.Id, c.ChatId, c.Topic);
                if (!groups.TryGetValue(key, out var list)) groups[key] = list = new();
                list.Add((s.Id, c));
            }
        }

        var edges = new List<CorrespondenceEdge>();
        foreach (var group in groups.Values)
        {
            var initiatorHalf = group.FirstOrDefault(h =>
                string.Equals(h.Entry.Initiator, "self", StringComparison.OrdinalIgnoreCase));
            var correspondentHalf = group.FirstOrDefault(h =>
                string.Equals(h.Entry.Initiator, "correspondent", StringComparison.OrdinalIgnoreCase));

            if (initiatorHalf.Entry is not null)
            {
                // The common case: this session opened the address itself. The correspondent's own
                // half (if its session still exists and still has it) supplies the reply direction back.
                edges.Add(new CorrespondenceEdge(
                    FromChatId: initiatorHalf.OwnerChatId,
                    FromRole: roleOf.GetValueOrDefault(initiatorHalf.OwnerChatId, "agent"),
                    ToChatId: initiatorHalf.Entry.ChatId,
                    ToRole: initiatorHalf.Entry.Role,
                    Topic: initiatorHalf.Entry.Topic,
                    RepliesFromInitiator: initiatorHalf.Entry.Depth,
                    VolumeFromInitiator: initiatorHalf.Entry.VolumeEstimate,
                    RepliesFromCorrespondent: correspondentHalf.Entry?.Depth ?? 0,
                    VolumeFromCorrespondent: correspondentHalf.Entry?.VolumeEstimate ?? 0));
            }
            else if (correspondentHalf.Entry is not null)
            {
                // Only the addressed side's half survives (the initiator's session vanished, or its
                // save has not caught up yet). Still worth an edge: the correspondent's own record is
                // the only evidence this correspondence ever existed, and dropping it would hide a
                // reply pattern that is real.
                var c = correspondentHalf.Entry;
                edges.Add(new CorrespondenceEdge(
                    FromChatId: c.ChatId,
                    FromRole: c.Role,
                    ToChatId: correspondentHalf.OwnerChatId,
                    ToRole: roleOf.GetValueOrDefault(correspondentHalf.OwnerChatId, "agent"),
                    Topic: c.Topic,
                    RepliesFromInitiator: 0,
                    VolumeFromInitiator: 0,
                    RepliesFromCorrespondent: c.Depth,
                    VolumeFromCorrespondent: c.VolumeEstimate));
            }
            // A group with neither a "self" nor a "correspondent" half cannot occur — every persisted
            // entry carries one of those two strings (ChatSession.Correspondences' own serialization) —
            // so there is no third branch to fall through to.
        }

        return edges;
    }

    private static string RoleName(ChatSession s) => string.IsNullOrWhiteSpace(s.As) ? "agent" : s.As!;

    /// <summary>Order-independent key for "the same correspondence, seen from either side".</summary>
    private static (string, string, string) PairKey(string chatIdA, string chatIdB, string topic)
        => string.CompareOrdinal(chatIdA, chatIdB) <= 0
            ? (chatIdA, chatIdB, topic)
            : (chatIdB, chatIdA, topic);
}
