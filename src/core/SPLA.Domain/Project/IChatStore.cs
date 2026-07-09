using SPLA.Domain.Models;

namespace SPLA.Domain.Project;

/// <summary>
/// Storage form for chat history — deals in <see cref="ChatSession"/> domain objects, NOT in
/// serialized text. That distinction is the whole point of chats being their own store form: a
/// backend decides HOW to persist (a YAML file per chat today, DB rows / a JSON column tomorrow)
/// and can order/filter natively. If this took pre-serialized text we'd lose exactly the
/// query-friendliness that motivates putting chats in a database.
///
/// The disk-bound seam (<see cref="IBucket.MapToHostDirectory"/>) is an implementation detail of
/// <see cref="FileChatStore"/>, not part of this contract — a server backend has no host directory.
/// Business logic (id generation, title auto-derivation, duplication) stays in
/// <see cref="Settings.ChatManager"/>; this is purely persistence.
/// </summary>
public interface IChatStore
{
    /// <summary>Persist (insert or replace) a chat by its <see cref="ChatSession.Id"/>.</summary>
    void SaveChat(ChatSession session);

    /// <summary>Load a chat by id, or null if it does not exist.</summary>
    ChatSession? LoadChat(string id);

    /// <summary>All chats, newest-updated first. A DB backend does this with ORDER BY.</summary>
    IReadOnlyList<ChatSession> ListChats();

    /// <summary>Remove a chat and its summary. No-op if absent.</summary>
    void DeleteChat(string id);

    /// <summary>Persist the markdown summary for a chat.</summary>
    void SaveSummary(string id, string markdownContent);

    /// <summary>Load the markdown summary for a chat, or null if none.</summary>
    string? LoadSummary(string id);

    /// <summary>Persist a point-in-time backup copy of a chat, tagged with a human label.</summary>
    void SaveBackup(ChatSession session, string label);
}
