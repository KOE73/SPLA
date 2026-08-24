namespace SPLA.Documents;

/// <summary>
/// What a document says, once everything about how it looks has been dropped.
///
/// <para><b>Why this exists instead of "convert to markdown".</b> Markdown is one rendering of this
/// tree, not the tree itself. Making it the internal representation would mean every other output
/// the agent eventually needs — JSON for a typed extraction, plain text for an embedding, one table
/// for a spreadsheet append — is obtained by PARSING markdown, i.e. by parsing something whose
/// structure has already been thrown away. Extraction produces the tree; renderers produce the
/// bytes.</para>
///
/// <para><b>Semantics only.</b> Headings, paragraphs, lists, tables, links, images-as-references.
/// Fonts, colours, margins, cell geometry and tracked changes are not represented and are not meant
/// to be: keeping the document's appearance is the other class of work (Artifact/Layout), with its
/// own API. Mixing the two is what produces an interface where half the arguments are always
/// empty.</para>
/// </summary>
public sealed record ContextDocument(DocumentMetadata Metadata, IReadOnlyList<ContextBlock> Blocks);

/// <summary>
/// What the container knew about itself. Everything except <see cref="SourceName"/> and
/// <see cref="SourceType"/> is optional, because most real documents carry no reliable metadata and
/// an invented value is worse than an absent one.
/// </summary>
/// <param name="SourceName">The file (or entry) name the content came from — the only thing a
/// renderer can honestly title an untitled document with.</param>
/// <param name="SourceType">The MIME type of the ORIGINAL container, not of the rendering.</param>
/// <param name="Extra">Backend-specific leftovers (a docx's revision count, a pdf's producer).
/// Rendered as a metadata block by renderers that show metadata at all; never interpreted here.</param>
public sealed record DocumentMetadata(
    string SourceName,
    string SourceType,
    string? Title = null,
    string? Author = null,
    string? Created = null,
    string? Modified = null,
    IReadOnlyDictionary<string, string>? Extra = null);
