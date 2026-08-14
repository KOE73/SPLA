using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Demo.Summarizer;

/// <summary>Where the prompt for one run comes into the conversation.</summary>
public enum PromptPlace
{
    /// <summary>As the project's instructions, i.e. the system prompt. The SPLA-native place.</summary>
    System,
    /// <summary>Inside the user turn, next to the document. Often what a small model actually obeys.</summary>
    User,
    /// <summary>Both — for a model that ignores one of them.</summary>
    Both
}

/// <summary>The worker's own <c>summarize:</c> section of the .spla file (the standard SPLA loader
/// ignores unknown sections — one project file stays the single entry point).</summary>
public sealed class SummarizeConfig
{
    /// <summary>Folder holding the prompt variants, relative to the .spla. Every <c>*.md</c> in it is
    /// one variant, named by its file name without the extension.</summary>
    public string PromptsDir { get; set; } = "ResumePrompts";

    /// <summary>Extensions to try, in preference order, when picking the source document out of a
    /// folder. The first extension that yields exactly one candidate wins.</summary>
    public List<string> SourceExtensions { get; set; } = new() { "md", "txt" };

    /// <summary>A file whose name contains any of these is never a source — that is our own output,
    /// or a summary somebody else already made.</summary>
    public List<string> ExcludeContaining { get; set; } = new() { " - резюме" };

    /// <summary>Word placed in the output name between the source's base name and the stamp.</summary>
    public string OutputMarker { get; set; } = "резюме";

    /// <summary>The user turn. <c>{document}</c> is required; <c>{prompt}</c> is substituted when
    /// <see cref="PromptPlace"/> puts the prompt here.</summary>
    public string UserFrame { get; set; } =
        "Ниже — стенограмма совещания. Составь по ней резюме строго по инструкции.\n\n" +
        "Отвечай только готовым текстом резюме в Markdown: без вступлений, без вопросов, " +
        "без замечаний о том, как ты работал.\n\n" +
        "=== СТЕНОГРАММА ===\n{document}\n=== КОНЕЦ СТЕНОГРАММЫ ===";

    /// <summary>Where the prompt goes: system / user / both.</summary>
    public string PromptPlace { get; set; } = "system";

    /// <summary>Write a YAML front-matter block with the run's metadata into each output file. This is
    /// what makes two attempts comparable after the fact — model, prompt, timings, tokens.</summary>
    public bool FrontMatter { get; set; } = true;

    /// <summary>Echo the model's stream to the console as it runs.</summary>
    public bool Echo { get; set; } = true;

    /// <summary>Print the model's reasoning channel too. Noisy on a thinking model.</summary>
    public bool ShowReasoning { get; set; }

    /// <summary>Model entry ids (from <c>connections:</c>) to run when the command line names none.
    /// Empty = the project's first entry.</summary>
    public List<string> Models { get; set; } = new();

    /// <summary>LM Studio model keys to run when the command line names none. Each is loaded into
    /// LM Studio before its run and the previous one unloaded — that is how one local runtime with a
    /// dozen downloaded models becomes a dozen targets.</summary>
    public List<string> LmstudioModels { get; set; } = new();

    /// <summary>Prompt variant names to run when the command line names none. Empty = every file in
    /// <see cref="PromptsDir"/>.</summary>
    public List<string> Prompts { get; set; } = new();

    /// <summary>Seconds to wait for one run before giving up on it. A 4B model on a 40k-character
    /// transcript can wander for a very long time; the matrix must not hang on it.</summary>
    public int TimeoutSeconds { get; set; } = 900;

    /// <summary>Refuse a run whose document plainly cannot fit the target's context window (rough
    /// 3 characters ≈ 1 token estimate for Russian, plus room for the answer). False = try anyway.</summary>
    public bool SkipIfTooLarge { get; set; } = true;

    public PromptPlace Place => PromptPlace?.Trim().ToLowerInvariant() switch
    {
        "user" => Summarizer.PromptPlace.User,
        "both" => Summarizer.PromptPlace.Both,
        _ => Summarizer.PromptPlace.System
    };

    public static SummarizeConfig Load(string splaFile)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var doc = deserializer.Deserialize<Envelope>(File.ReadAllText(splaFile));
        return doc?.Summarize ?? new SummarizeConfig();
    }

    private sealed class Envelope
    {
        public SummarizeConfig? Summarize { get; set; }
    }
}
