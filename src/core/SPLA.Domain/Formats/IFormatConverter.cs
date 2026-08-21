using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Resources;

namespace SPLA.Domain.Formats;

/// <summary>
/// One registered projection from one content type to another: docx → markdown, a pdf page → png,
/// a video → frames.
///
/// <para><b>Projection, not equality.</b> A conversion is one-way and lossy by assumption. Markdown
/// out of a docx has lost the tracked changes, the styles and half the tables' geometry; a frame out
/// of a video has lost the other 24 in that second. Nothing here promises a round trip, and no caller
/// may treat <c>Convert(Convert(x))</c> as <c>x</c>. Naming the operation a projection is the whole
/// point: an "equality" would invite writing back through it, which is where such registries acquire
/// their first data-loss bug.</para>
///
/// <para><b>One hop only.</b> A converter declares exactly one (source, target) pair, and the registry
/// resolves exactly one registered pair — it never searches for a path such as
/// <c>docx → html → md</c>. Composed conversion graphs are what rotted ImageMagick's delegates:
/// once the machine may compose, the answer to "what will this do" stops being readable off the
/// registration list and becomes a function of what happens to be installed. If a chain is ever
/// genuinely wanted, it is written as its own converter and registered explicitly as its own pair,
/// where it can be read, named and summarised like every other member.</para>
/// </summary>
public interface IFormatConverter
{
    /// <summary>The MIME type this accepts, lowercased. Compared case-insensitively and without
    /// parameters, so <c>text/plain</c> also answers for <c>text/plain; charset=utf-8</c>. May be a
    /// wildcard of the form <c>image/*</c>.</summary>
    string SourceType { get; }

    /// <summary>The MIME type this produces, lowercased. Same matching rules as
    /// <see cref="SourceType"/>.</summary>
    string TargetType { get; }

    /// <summary>One line for the system prompt: what this projection does and what it costs — the same
    /// role <see cref="IResourceProvider.Summary"/> plays for a scheme. Written as an example, not as
    /// prose: "Word document to Markdown — text, headings and lists; styles and tracked changes are
    /// dropped".</summary>
    string Summary { get; }

    /// <summary>
    /// Projects <paramref name="source"/> onto <see cref="TargetType"/>.
    ///
    /// <para><b>Why async with a <see cref="CancellationToken"/> when today's converters are trivial.</b>
    /// The first members of this set (identity, text re-labelling) are plain synchronous byte work and
    /// would be honest as a synchronous method. The ones this contract exists for are not: docx and pdf
    /// unpacking, and anything that shells out to FFmpeg, are seconds-long and must be abortable.
    /// Widening the signature afterwards would rewrite every implementation that had been written to
    /// the narrow one, so the wide signature goes in first and the trivial converters pay a
    /// <c>Task.FromResult</c>.</para>
    /// </summary>
    /// <param name="options">
    /// Converter-specific knobs. <b>Always null today</b> — no converter has options yet, and none
    /// should gain them casually. The slot exists because ADDING a parameter to a published interface
    /// later is a breaking change for every implementor, while an unused parameter costs nothing but
    /// the word. When a converter does need options (a pdf page number, a frame interval) it declares
    /// its own schema for them, the way a plugin declares its own settings editor; the registry stays
    /// ignorant of the contents and never inspects, validates or forwards them on anyone's behalf.
    /// </param>
    Task<ResourceContent> ConvertAsync(
        ResourceContent source,
        IReadOnlyDictionary<string, object?>? options,
        CancellationToken ct = default);
}
