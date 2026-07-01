using System;
using System.IO;
using System.Linq;
using SPLA.Domain.Editor;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// File-system operations exposed to web clients for the Workspace-shell surface
/// (project file browser + text/markdown editor). Thin service layer on top of
/// <see cref="FileContentBrowser"/> and <see cref="FileContentSource"/> from SPLA.Domain.Editor.
///
/// Security: every <c>contentRef</c> is resolved to a canonical absolute path and validated
/// to be inside <c>workspaceRoot</c> before any read or write is performed.
/// </summary>
public static class WorkspaceOps
{
    public static FsBrowseResultPayload Browse(string workspaceRoot, string? parentRef)
    {
        var root = Path.GetFullPath(workspaceRoot);

        if (parentRef is not null && !IsUnderRoot(root, parentRef))
            return new FsBrowseResultPayload();

        var browser = new FileContentBrowser(root);
        var nodes = browser.GetChildren(parentRef);

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

    public static FsReadResultPayload Read(string workspaceRoot, string contentRef)
    {
        var root = Path.GetFullPath(workspaceRoot);
        if (!IsUnderRoot(root, contentRef))
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

    public static FsWriteResultPayload Write(string workspaceRoot, string contentRef, string text)
    {
        var root = Path.GetFullPath(workspaceRoot);
        if (!IsUnderRoot(root, contentRef))
            return new FsWriteResultPayload { Ref = contentRef, Ok = false, Error = "Access denied: path is outside workspace." };

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

    /// <summary>Returns true when <paramref name="path"/> resolves to a location inside
    /// <paramref name="normalizedRoot"/> (canonical absolute path).</summary>
    private static bool IsUnderRoot(string normalizedRoot, string path)
    {
        try
        {
            var full = Path.GetFullPath(path);
            var rootWithSep = normalizedRoot.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            return full.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase);
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
