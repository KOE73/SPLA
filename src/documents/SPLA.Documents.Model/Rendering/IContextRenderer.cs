namespace SPLA.Documents.Rendering;

/// <summary>
/// One projection of the semantic tree onto bytes a consumer understands.
///
/// <para>Renderers live with the MODEL, not with the backends. Two backends reading the same docx
/// must produce the same markdown, or "swap the provider" stops being a free operation and becomes
/// a change in what the agent reads. The tree is the contract between them; the rendering is
/// shared.</para>
/// </summary>
public interface IContextRenderer
{
    /// <summary>The MIME type of what <see cref="Render"/> produces.</summary>
    string TargetType { get; }

    string Render(ContextDocument document);
}
