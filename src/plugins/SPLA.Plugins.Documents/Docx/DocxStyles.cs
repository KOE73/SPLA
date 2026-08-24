using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SPLA.Plugins.Documents.Docx;

/// <summary>
/// Answers the one question the OOXML model refuses to answer directly: <b>is this paragraph a
/// heading, and of what level?</b>
///
/// <para>Word stores no such flag. It stores a style id, and the meaning of that id lives in
/// <c>styles.xml</c> — which is why this is a resolver built per document rather than a lookup
/// table. Three sources are consulted, in falling order of trust: the style's outline level (what
/// Word's own navigation pane uses), the style's NAME, and finally the style id itself.</para>
///
/// <para><b>Names are matched in Russian as well as English.</b> Documents written in a localised
/// Word carry style names like "Заголовок 2" with a style id to match, and a resolver that only
/// knows "Heading 2" turns every heading in such a document into a plain paragraph — losing exactly
/// the structure this extraction exists to recover. This is a data-recognition rule, not
/// prompt text, so the language list is a property of the corpus, not of the system prompt.</para>
/// </summary>
internal sealed partial class DocxStyles
{
    private readonly Dictionary<string, int> _headingLevelByStyleId = new(StringComparer.OrdinalIgnoreCase);

    private DocxStyles() { }

    public static DocxStyles From(MainDocumentPart mainPart)
    {
        var styles = new DocxStyles();

        foreach (var style in mainPart.StyleDefinitionsPart?.Styles?.Elements<Style>() ?? [])
        {
            var id = style.StyleId?.Value;
            if (string.IsNullOrWhiteSpace(id)) continue;

            var level = LevelFromOutline(style.StyleParagraphProperties?.OutlineLevel?.Val?.Value)
                        ?? LevelFromName(style.StyleName?.Val?.Value)
                        ?? LevelFromName(id);

            if (level is not null) styles._headingLevelByStyleId[id] = level.Value;
        }

        return styles;
    }

    /// <summary>Heading level for a paragraph, or null when it is ordinary text. The paragraph's own
    /// outline level wins over its style's — a paragraph that was promoted by hand is still a
    /// heading in Word's own outline.</summary>
    public int? HeadingLevel(string? styleId, int? paragraphOutlineLevel)
    {
        if (LevelFromOutline(paragraphOutlineLevel) is { } direct) return direct;
        if (string.IsNullOrWhiteSpace(styleId)) return null;
        if (_headingLevelByStyleId.TryGetValue(styleId, out var known)) return known;

        // No styles part, or a style defined nowhere: the id is all there is, and "Heading2" says
        // enough on its own.
        return LevelFromName(styleId);
    }

    /// <summary>Outline level 0..8 is heading 1..9. The value 9 is Word's "body text" and is not a
    /// heading — treating it as one would make every ordinary paragraph a level-10 heading.</summary>
    private static int? LevelFromOutline(int? outlineLevel)
        => outlineLevel is >= 0 and <= 8 ? outlineLevel.Value + 1 : null;

    private static int? LevelFromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var match = HeadingNamePattern().Match(name);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var level) && level is >= 1 and <= 9)
            return level;

        return TitleNamePattern().IsMatch(name) ? 1 : null;
    }

    [GeneratedRegex(@"^\s*(?:heading|заголовок)[\s\-_]*([1-9])\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex HeadingNamePattern();

    [GeneratedRegex(@"^\s*(?:title|название|заглавие)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex TitleNamePattern();
}
