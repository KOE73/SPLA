using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using System;
using System.IO;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 4 of <c>docs/plans/PLAN_20260906_core_chat-directory-and-await.md</c>
/// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.1): the instance number belongs to the chat,
/// is minted once by a project-wide counter, and only ever goes up — because it is a public name, and
/// a name that comes back is a name two different chats answer to.
/// </summary>
public sealed class RoleInstanceNumberTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-instance-no-{Guid.NewGuid():N}")).FullName;

    /// <summary>A manager over a bare workspace — the same shape <c>CorrespondenceGraphTests</c> uses
    /// to exercise <see cref="ChatManager"/> without standing up a whole runtime.</summary>
    private static ChatManager Manager(string root) => new(new ResolvedSettings
    {
        WorkspacePath = root,
        ProjectFilePath = Path.Combine(root, "test.spla")
    });

    [Fact]
    public void A_deleted_chat_does_not_give_its_number_back()
    {
        var root = TempRoot();
        try
        {
            var chats = Manager(root);
            var first = chats.CreateNewChat("A", "architect");
            var second = chats.CreateNewChat("B", "architect");
            var third = chats.CreateNewChat("C", "architect");

            Assert.Equal(1, first.AsInstance);
            Assert.Equal(2, second.AsInstance);
            Assert.Equal(3, third.AsInstance);

            // The one in the middle goes away — its name, however, is already written down elsewhere.
            chats.DeleteChat(second.Id);

            Assert.Equal(4, chats.CreateNewChat("D", "architect").AsInstance);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Counters_are_per_role_and_case_insensitive()
    {
        var root = TempRoot();
        try
        {
            var chats = Manager(root);
            Assert.Equal(1, chats.CreateNewChat("A", "architect").AsInstance);
            Assert.Equal(1, chats.CreateNewChat("B", "reviewer").AsInstance);
            Assert.Equal(2, chats.CreateNewChat("C", "Architect").AsInstance);

            // A chat with no role is NOT numbered at creation (ADR_20260906 §2.1): it is normally a
            // human's and will never be addressed, and numbering it here would spend the `agent`
            // sequence on dozens of such chats. Its number is minted on first address instead — see
            // A_role_less_chat_is_numbered_only_when_it_becomes_a_correspondent in CorrespondenceTests.
            Assert.Null(chats.CreateNewChat("D").AsInstance);
            Assert.Null(chats.CreateNewChat("E").AsInstance);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_spawned_chat_gets_no_number()
    {
        var root = TempRoot();
        try
        {
            var chats = Manager(root);
            var spawned = chats.CreateSpawnedChat("parent-id", "architect", null, "Edit");

            Assert.Null(spawned.AsInstance);
            // And it did not spend one either: the next real architect is still the first.
            Assert.Equal(1, chats.CreateNewChat("A", "architect").AsInstance);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void An_old_session_is_minted_on_first_load_and_keeps_that_number()
    {
        var root = TempRoot();
        try
        {
            var chats = Manager(root);
            // A session as it was written before as_instance existed: a role, no number.
            var legacy = chats.CreateNewChat("Legacy", "architect");
            legacy.AsInstance = null;
            chats.SaveChat(legacy);

            var loaded = chats.LoadChat(legacy.Id)!;
            Assert.Equal(2, loaded.AsInstance); // 1 was spent by CreateNewChat above

            // Struck on disk, not just in memory — otherwise every load would hand out a fresh one.
            var again = chats.LoadChat(legacy.Id)!;
            Assert.Equal(2, again.AsInstance);
            Assert.Equal(2, Manager(root).LoadChat(legacy.Id)!.AsInstance);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_copy_is_a_new_chat_and_gets_its_own_number()
    {
        var root = TempRoot();
        try
        {
            var chats = Manager(root);
            var source = chats.CreateNewChat("A", "architect");

            var copy = chats.DuplicateChat(source.Id);

            Assert.NotEqual(source.Id, copy.Id);
            Assert.Equal(2, copy.AsInstance);
            Assert.Equal(1, chats.LoadChat(source.Id)!.AsInstance);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void The_counter_survives_a_restart()
    {
        var root = TempRoot();
        try
        {
            Assert.Equal(1, Manager(root).CreateNewChat("A", "architect").AsInstance);

            // A second manager over the same project — nothing shared in memory, only the file.
            Assert.Equal(2, Manager(root).CreateNewChat("B", "architect").AsInstance);
            Assert.True(File.Exists(Path.Combine(root, ".spla", RoleInstanceCounters.FileName)));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_corrupt_counter_file_falls_back_to_the_numbers_already_on_disk()
    {
        var root = TempRoot();
        try
        {
            var chats = Manager(root);
            chats.CreateNewChat("A", "architect");
            chats.CreateNewChat("B", "architect");

            var counterFile = Path.Combine(root, ".spla", RoleInstanceCounters.FileName);
            File.WriteAllText(counterFile, "{ this is not json");

            // Zero would re-issue architect_1, which is the one thing this must never do; the sessions
            // on disk are the floor. (They cannot see deleted chats — hence the file in the first place.)
            Assert.Equal(3, Manager(root).CreateNewChat("C", "architect").AsInstance);
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
