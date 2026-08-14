namespace SPLA.CLI.Batch;

internal static class OutputNaming
{
    /// <summary>Makes a value safe for a file name — same rule as the Summarizer demo's
    /// <c>Discovery.Sanitize</c>, kept in step so output from either tool reads the same way.</summary>
    public static string Sanitize(string value)
    {
        var chars = value.Replace('/', '-').Replace('\\', '-').Replace(':', '-').Replace('@', '-');
        foreach (var bad in Path.GetInvalidFileNameChars())
            chars = chars.Replace(bad, '-');
        while (chars.Contains("--")) chars = chars.Replace("--", "-");
        return chars.Trim(' ', '-', '.');
    }

    /// <summary>Expands <c>{timestamp}</c>/<c>{prompt}</c>/<c>{model}</c>/<c>{label}</c> in a template,
    /// sanitizes the result, and de-duplicates against whatever is already in <paramref name="dir"/>
    /// so two cells landing in the same second never overwrite each other.</summary>
    public static string BuildPath(string dir, string template, DateTimeOffset stamp, PromptItem prompt, string modelId)
    {
        var label = $"{prompt.Name} {modelId}";
        var name = template
            .Replace("{timestamp}", stamp.ToString("yyyyMMdd-HHmmss"))
            .Replace("{prompt}", prompt.Name)
            .Replace("{model}", modelId)
            .Replace("{label}", label);

        var path = Path.Combine(dir, Sanitize(name) + ".md");
        var n = 2;
        while (File.Exists(path))
            path = Path.Combine(dir, Sanitize(name) + $" ({n++})") + ".md";
        return path;
    }
}
