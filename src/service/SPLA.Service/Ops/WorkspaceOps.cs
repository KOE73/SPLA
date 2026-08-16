using System;
using System.IO;
using System.Linq;
using SPLA.Domain.Editor;
using SPLA.Domain.Host;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// File-system operations exposed to web clients for the Workspace-shell surface
/// (project file browser + text/markdown editor). Thin service layer on top of
/// <see cref="FileContentBrowser"/> and <see cref="FileContentSource"/> from SPLA.Domain.Editor.
///
/// Security: every <c>contentRef</c> is resolved through the PROJECT's boundary — the same object
/// the file tools and SFTP work behind — before any read or write is performed. It used to build its
/// own boundary from a root string, which meant this surface and the agent's disagreed about what
/// "inside" covered; the <c>.spla/</c> cutout is the case where they did.
/// </summary>
public static class WorkspaceOps
{
    public static FsBrowseResultPayload Browse(PathBoundary boundary, string? parentRef)
    {
        // An unbounded boundary has no root to browse from. Callers hand in a bounded one (see
        // WorkspaceHandlers.BoundaryOf); returning nothing beats dereferencing null if one ever does not.
        if (boundary.Root is not { } root) return new FsBrowseResultPayload();

        if (parentRef is not null && !IsUnderRoot(boundary, parentRef))
            return new FsBrowseResultPayload();

        var browser = new FileContentBrowser(root);

        // Filtered, not merely refused on entry: a folder listed but dead on click is worse than one
        // that is not listed, and the cutout is the whole reason there is anything to hide.
        var nodes = browser.GetChildren(parentRef).Where(n => IsUnderRoot(boundary, n.Ref));

        return new FsBrowseResultPayload
        {
            Nodes = nodes.Select(n => new FsNodeDto
            {
                Ref         = n.Ref,
                Label       = n.Label,
                Kind        = n.Kind == ContentNodeKind.Folder ? "folder" : "leaf",
                ContentType = n.ContentType,
                SizeBytes   = n.SizeBytes,
                Modified    = n.Modified?.ToString("o")
            }).ToList()
        };
    }

    public static FsReadResultPayload Read(PathBoundary boundary, string contentRef)
    {
        if (!IsUnderRoot(boundary, contentRef))
            return new FsReadResultPayload { Ref = contentRef, Error = "Access denied: path is outside workspace." };

        var source = new FileContentSource();
        if (!source.CanResolve(contentRef))
            return new FsReadResultPayload { Ref = contentRef, Error = "Invalid file ref." };

        try
        {
            var text = source.ReadText(contentRef);
            var contentType = ExtensionToContentType(Path.GetExtension(contentRef));
            return new FsReadResultPayload { Ref = contentRef, Text = text, ContentType = contentType };
        }
        catch (Exception ex)
        {
            return new FsReadResultPayload { Ref = contentRef, Error = ex.Message };
        }
    }

    public static FsWriteResultPayload Write(PathBoundary boundary, string contentRef, string text)
    {
        if (!IsUnderRoot(boundary, contentRef))
            return new FsWriteResultPayload { Ref = contentRef, Ok = false, Error = "Access denied: path is outside workspace." };

        // The mount's floor, enforced again here. This surface writes through FileContentSource and
        // never passes LocalWorkspace.Guard, so the check that holds for every tool would not hold for
        // the editor — and a mount declared read-only means the operator called that copy canonical.
        if (boundary.Resolve(contentRef).Mount is { Access: MountAccess.Read } readOnly)
            return new FsWriteResultPayload
            {
                Ref = contentRef,
                Ok = false,
                Error = $"Access denied: mount '{readOnly.Name}' is declared read-only."
            };

        var source = new FileContentSource();
        if (!source.CanResolve(contentRef))
            return new FsWriteResultPayload { Ref = contentRef, Ok = false, Error = "Invalid file ref." };

        try
        {
            source.WriteText(contentRef, text);
            return new FsWriteResultPayload { Ref = contentRef, Ok = true };
        }
        catch (Exception ex)
        {
            return new FsWriteResultPayload { Ref = contentRef, Ok = false, Error = ex.Message };
        }
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Whether a client-supplied ref lands inside the project. Strictly inside: the root itself is
    /// not a file anyone reads or writes, so browsing starts one level in and this keeps saying no to
    /// a ref that is merely the root spelled out.
    /// </summary>
    private static bool IsUnderRoot(PathBoundary boundary, string path)
    {
        try
        {
            if (!boundary.TryResolve(path, out var full, out _)) return false;

            return !string.Equals(
                full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                boundary.Root,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string ExtensionToContentType(string ext) => ext.ToLowerInvariant() switch
    {
        ".md"            => "md",
        ".jsonl"         => "jsonl",
        ".json"          => "json",
        ".yaml" or ".yml" => "yaml",
        ".sql"           => "sql",
        ".cs"            => "cs",
        ".c" or ".h"     => "c",
        ".cpp"           => "cpp",
        ".java"          => "java",
        _                => "txt"
    };
}
