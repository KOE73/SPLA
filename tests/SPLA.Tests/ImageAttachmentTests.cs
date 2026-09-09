using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.LLM.OpenAiCompat;

namespace SPLA.Tests;

/// <summary>
/// A picture reaches a model with no name of its own — no file name, no id, and image metadata is
/// not read — so several pictures in one message can only be told apart by the words around them
/// (<see cref="ImageAttachment"/>). Two boundaries decide whether a name actually survives that far:
/// the shape the provider is sent, and the shape the chat file keeps. Both are exercised here,
/// because either one alone can be right while the other silently drops the name — and a dropped
/// name fails invisibly: the model answers about *a* picture, confidently, just not the one asked
/// about.
/// </summary>
public sealed class ImageAttachmentTests
{
    private const string Png = "data:image/png;base64,iVBORw0KGgo=";

    // ── What the provider is sent ─────────────────────────────────────────────

    private static List<(string Type, string Value)> Parts(string? text, params ImageAttachment[] images)
        => OpenAiCompatibleClient.BuildMultimodalContent(text, images.ToList())
            .Cast<Dictionary<string, object?>>()
            .Select(p => ((string)p["type"]!, (string)p["type"]! == "text"
                ? (string)p["text"]!
                : (string)((Dictionary<string, object?>)p["image_url"]!)["url"]!))
            .ToList();

    [Fact]
    public void Each_named_image_is_introduced_by_its_name_and_the_question_comes_last()
    {
        var parts = Parts("How are these different?",
            new ImageAttachment(Png + "1", "seg012.jpg"),
            new ImageAttachment(Png + "2", "seg013.jpg"));

        Assert.Equal(
        [
            ("text", "seg012.jpg:"),
            ("image_url", Png + "1"),
            ("text", "seg013.jpg:"),
            ("image_url", Png + "2"),
            ("text", "How are these different?")
        ], parts);
    }

    [Fact]
    public void An_unnamed_image_adds_nothing_but_itself()
    {
        // The old behaviour, unchanged: nothing named means nothing to say, and position stays the
        // only thing identifying a picture — as it was before names existed.
        Assert.Equal(
        [
            ("image_url", Png),
            ("text", "Describe it.")
        ], Parts("Describe it.", new ImageAttachment(Png)));
    }

    // ── What the chat file keeps ──────────────────────────────────────────────

    private static ChatManager NewChatManager(out string root)
    {
        root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-image-labels-{Guid.NewGuid():N}")).FullName;
        return new ChatManager(new ResolvedSettings
        {
            WorkspacePath = root,
            ProjectFilePath = Path.Combine(root, "project.spla")
        });
    }

    [Fact]
    public void A_name_survives_a_real_save_and_load()
    {
        var chats = NewChatManager(out var root);
        try
        {
            var session = chats.CreateNewChat("named images");
            session.Messages.Add(new ChatSessionMessage
            {
                Role = "user",
                Content = "what is on them",
                Images = [new ChatSessionImage("a1.png", "seg012.jpg"), new ChatSessionImage("b2.png")]
            });
            chats.SaveChat(session);

            var images = chats.LoadChat(session.Id)!.Messages.Single().Images!;

            Assert.Equal("a1.png", images[0].File);
            Assert.Equal("seg012.jpg", images[0].Label);
            Assert.Equal("b2.png", images[1].File);
            Assert.Null(images[1].Label);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_chat_written_before_names_existed_still_reads_and_stays_as_it_was()
    {
        // Every chat on disk today has `images: [file.png]` — bare file names. Those must keep
        // loading, and an image nobody named must be written back the same way rather than rewritten
        // into a wordier shape that says nothing new.
        var chats = NewChatManager(out var root);
        try
        {
            var session = chats.CreateNewChat("old shape");
            var path = Directory.EnumerateFiles(root, $"{session.Id}.yaml", SearchOption.AllDirectories).Single();
            File.WriteAllText(path, File.ReadAllText(path).TrimEnd() + """

                messages:
                - id: aaaaaaaa
                  role: user
                  content: describe them
                  images:
                  - 24a296fb.png
                  - 11cde027.png
                """);

            var loaded = chats.LoadChat(session.Id)!;
            var images = loaded.Messages.Single().Images!;
            Assert.Equal(["24a296fb.png", "11cde027.png"], images.Select(i => i.File));
            Assert.All(images, i => Assert.Null(i.Label));

            chats.SaveChat(loaded);
            Assert.Contains("- 24a296fb.png", File.ReadAllText(path));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
