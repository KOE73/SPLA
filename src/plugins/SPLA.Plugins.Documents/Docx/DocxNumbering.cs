using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SPLA.Plugins.Documents.Docx;

/// <summary>
/// Whether a numbered paragraph is a bullet list or an ordered one.
///
/// <para>OOXML puts a two-hop indirection between the paragraph and the answer: the paragraph names
/// a <c>numId</c>, the numbering instance names an <c>abstractNumId</c>, and only the abstract
/// definition says whether level <i>n</i> is a bullet, a decimal, a letter or a roman numeral. This
/// resolves those hops once per document.</para>
///
/// <para>Anything that is not a bullet counts as ordered — including letters and roman numerals.
/// The distinction that survives into a semantic tree is "these items are in a sequence" versus
/// "these items are a set"; which glyph Word drew the sequence with is decoration.</para>
/// </summary>
internal sealed class DocxNumbering
{
    private readonly Dictionary<(int NumberingId, int Level), bool> _orderedByLevel = [];

    private DocxNumbering() { }

    public static DocxNumbering From(MainDocumentPart mainPart)
    {
        var result = new DocxNumbering();
        var numbering = mainPart.NumberingDefinitionsPart?.Numbering;
        if (numbering is null) return result;

        var abstractDefinitions = numbering.Elements<AbstractNum>()
            .Where(a => a.AbstractNumberId?.Value is not null)
            .ToDictionary(a => a.AbstractNumberId!.Value);

        foreach (var instance in numbering.Elements<NumberingInstance>())
        {
            var numberingId = instance.NumberID?.Value;
            var abstractId = instance.AbstractNumId?.Val?.Value;
            if (numberingId is null || abstractId is null) continue;
            if (!abstractDefinitions.TryGetValue(abstractId.Value, out var definition)) continue;

            foreach (var level in definition.Elements<Level>())
            {
                var index = level.LevelIndex?.Value;
                if (index is null) continue;

                var format = level.NumberingFormat?.Val?.Value;
                result._orderedByLevel[(numberingId.Value, index.Value)] = format != NumberFormatValues.Bullet;
            }
        }

        return result;
    }

    /// <summary>Ordered unless the document says bullet. A document whose numbering part is missing
    /// or unreadable still has lists — rendering them as bullets would claim a fact the file does
    /// not contain, so the default follows the commonest case of an explicit <c>numId</c>.</summary>
    public bool IsOrdered(int numberingId, int level)
        => !_orderedByLevel.TryGetValue((numberingId, level), out var ordered) || ordered;
}
