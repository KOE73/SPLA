namespace SPLA.Documents;

/// <summary>
/// One backend's ability to turn a container into meaning.
///
/// <para><b>A stream, not a path.</b> The same document arrives from a file, from a resource
/// address, from a blob handle and from an HTTP response; a path-shaped contract would force three
/// of those four to write a temporary file first.</para>
///
/// <para><b>No options bag.</b> <c>IFormatConverter</c> already carries one and it is always null;
/// a second empty extension point would add a parameter every implementation must accept and none
/// would read. When a real knob appears — a PDF page range, a frame interval — it is added then,
/// with a caller that needs it.</para>
/// </summary>
public interface IDocumentExtractor
{
    /// <summary>MIME types this backend accepts, lowercased.</summary>
    IReadOnlyCollection<string> SourceTypes { get; }

    /// <param name="sourceName">File or entry name, used for metadata and for titling an untitled
    /// document. Never used to decide the format — the caller already decided that.</param>
    Task<ContextDocument> ExtractAsync(Stream source, string sourceName, CancellationToken ct = default);
}

/// <summary>The MIME types this corner of the system names often enough to spell wrong once.</summary>
public static class DocumentContentTypes
{
    public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string Pptx = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
    public const string Markdown = "text/markdown";
    public const string PlainText = "text/plain; charset=utf-8";
    public const string Json = "application/json";
    public const string Csv = "text/csv";
}
