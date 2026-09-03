using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Wave 0 of <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c>: <c>ChatSession</c>
/// gains <c>as</c>/<c>parent</c>/<c>origin</c>, declared and round-tripped but written by nobody yet.
/// Two things need proving, per <c>docs/adr/ADR_20260827_core_config-versioning.md</c>'s own test
/// discipline: the new keys survive a real save/load, and a session file that predates them —
/// exactly what everyone's <c>.spla/chats/</c> holds today — still loads without complaint.
/// </summary>
public sealed class ChatSessionActorFieldsTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-session-actor-fields-{Guid.NewGuid():N}")).FullName;

    private static ChatManager NewChatManager(string root) => new(new ResolvedSettings
    {
        WorkspacePath = root,
        ProjectFilePath = Path.Combine(root, "project.spla")
    });

    [Fact]
    public void As_parent_and_origin_round_trip_through_a_real_save_and_load()
    {
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("spawned run");
            session.As = "architect";
            session.Parent = "chat-parent-id";
            session.Origin = "spawned";
            chats.SaveChat(session);

            var reloaded = chats.LoadChat(session.Id);

            Assert.NotNull(reloaded);
            Assert.Equal("architect", reloaded!.As);
            Assert.Equal("chat-parent-id", reloaded.Parent);
            Assert.Equal("spawned", reloaded.Origin);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_session_with_none_of_the_new_keys_set_writes_and_reads_them_back_as_null()
    {
        // Nothing writes these fields yet (that is Wave 2) — an ordinary human chat saved today must
        // not spuriously acquire a role, a parent or an origin.
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var session = chats.CreateNewChat("ordinary chat");
            chats.SaveChat(session);

            var reloaded = chats.LoadChat(session.Id);

            Assert.NotNull(reloaded);
            Assert.Null(reloaded!.As);
            Assert.Null(reloaded.Parent);
            Assert.Null(reloaded.Origin);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_pre_existing_session_file_lacking_the_new_keys_loads_with_no_warning()
    {
        // The first exercise of ADR_20260827_core_config-versioning's "absence of a new key is not an
        // error, it is the first version": a file exactly like the ones already sitting in every
        // .spla/chats/ folder, written before `as:`/`parent:`/`origin:` existed at all.
        var root = TempRoot();
        try
        {
            var chats = NewChatManager(root);
            var chatsDir = Directory.CreateDirectory(Path.Combine(root, ".spla", "chats")).FullName;
            const string id = "legacy-session";
            File.WriteAllText(Path.Combine(chatsDir, id + ".yaml"), """
                version: 1
                id: legacy-session
                title: Old Chat
                created_at: 2026-01-01T00:00:00Z
                updated_at: 2026-01-01T00:00:00Z
                workspace: ""
                messages: []
                """);

            var reloaded = chats.LoadChat(id);

            Assert.NotNull(reloaded);
            Assert.Equal("Old Chat", reloaded!.Title);
            Assert.Null(reloaded.As);
            Assert.Null(reloaded.Parent);
            Assert.Null(reloaded.Origin);
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
