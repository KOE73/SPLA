namespace SPLA.Documents;

/// <summary>
/// One semantic unit of a document.
///
/// <para><b>The set is closed on purpose.</b> A block type earns its place only if a renderer would
/// otherwise have to lie about it — a table flattened into paragraphs stops being a table, and a
/// heading flattened into a paragraph loses the document's outline, which is the single most useful
/// thing an agent reads out of a long file. Everything that would only affect appearance
/// (alignment, spacing, colour) has no block and never will: see the Context/Artifact split in
/// docs/adr/ADR_20260824_plugins_document-context.md.</para>
/// </summary>
public abstract record ContextBlock;

/// <param name="Level">1..9, as documents number them. A backend that cannot tell the level says 1
/// rather than 0 — every heading is at least a heading.</param>
public sealed record HeadingBlock(int Level, string Text) : ContextBlock;

public sealed record ParagraphBlock(string Text) : ContextBlock;

/// <param name="Level">Nesting depth from 0. Kept per line rather than as nested lists because that
/// is how every source format actually stores it, and rebuilding a tree only to flatten it again in
/// every renderer buys nothing.</param>
public sealed record ListItemLine(int Level, string Text);

public sealed record ListBlock(bool Ordered, IReadOnlyList<ListItemLine> Items) : ContextBlock;

/// <param name="Header">The first row when the source marked one, else null — an invented header is
/// a data error waiting to happen downstream, where the header names the fields.</param>
public sealed record TableBlock(
    IReadOnlyList<string>? Header,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    string? Caption = null) : ContextBlock;

public sealed record CodeBlock(string? Language, string Text) : ContextBlock;

/// <summary>
/// A picture that WAS there — named, not carried.
///
/// <para>The bytes stay out of the tree deliberately. An extraction is a context-producing
/// operation, and a document with forty screenshots would otherwise turn one call into forty
/// megabytes travelling through a model's window. When an image is actually wanted, it is fetched
/// by address as its own read.</para>
/// </summary>
public sealed record ImageBlock(
    string? Name,
    string? AltText = null,
    string? ContentType = null,
    long? ByteCount = null) : ContextBlock;

/// <summary>A page, sheet or section boundary — the one piece of layout that carries meaning,
/// because "which page was this on" is a question people actually ask of a document.</summary>
public sealed record SectionBreak(string? Label = null) : ContextBlock;
