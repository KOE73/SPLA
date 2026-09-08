using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// docs/plans/PLAN_20260903_core_readonly-surface.md — the reading path stage 0 made necessary.
/// Stage 0 stopped <see cref="ChatRegistry.GetOrOpen"/> resurrecting an archived chat and left no way
/// to look at one at all; <see cref="ChatRegistry.ReadArchived"/> is that way, and what these guard is
/// the pair of properties that make it safe to call: it reads only archived chats, and reading one
/// wakes nothing.
/// </summary>
public sealed class ReadOnlySurfaceTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-ro-{Guid.NewGuid():N}")).FullName;

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject()
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, """
            version: 1
            name: ReadOnlySurfaceTest
            workspace: .
            agent:
              mode: Edit
            """);
        var settings = ConfigLoader.LoadAndResolve(manifest);
        var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);
        return (runtime, new ChatRegistry(runtime), root);
    }

    [Fact]
    public void ReadArchived_returns_an_archived_chats_history()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            // Written through the manager rather than by running a turn: what is under test is the
            // read path off disk, and a real turn would need a provider to answer it.
            var session = runtime.ChatManager.CreateNewChat("Old business");
            session.Messages.Add(new ChatSessionMessage { Role = "user", Content = "the thing we said" });
            runtime.ChatManager.SaveChat(session);
            var id = session.Id;
            chats.Archive(id);

            var read = chats.ReadArchived(id);

            Assert.NotNull(read);
            Assert.Equal("Old business", read!.Title);
            Assert.Contains(read.Messages, m => m.Content == "the thing we said");
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void ReadArchived_does_not_open_a_runtime()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var id = chats.CreateNew("Old business").ChatId;
            chats.Archive(id);

            chats.ReadArchived(id);

            // The whole point: reading is not a back door into the state GetOrOpen refuses to create.
            Assert.Null(chats.Peek(id));
            Assert.Null(chats.GetOrOpen(id));
            Assert.Equal(ChatLocation.Archived, chats.Locate(id));
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void ReadArchived_refuses_an_active_chat_and_a_missing_one()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var active = chats.CreateNew("Live");

            // An active chat has a runtime and must be reached through it — reading it off disk would
            // hand back whatever was last saved, which is not what anyone looking at it means.
            Assert.Null(chats.ReadArchived(active.ChatId));
            Assert.Null(chats.ReadArchived("no-such-chat"));
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Unarchiving_makes_the_chat_openable_again_and_no_longer_readable()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var id = chats.CreateNew("Back from the dead").ChatId;
            chats.Archive(id);
            Assert.NotNull(chats.ReadArchived(id));

            chats.Unarchive(id);

            // The surface is read-only, the chat is not: the two swap over cleanly, with no state left
            // behind on either side.
            Assert.Null(chats.ReadArchived(id));
            Assert.NotNull(chats.GetOrOpen(id));
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
