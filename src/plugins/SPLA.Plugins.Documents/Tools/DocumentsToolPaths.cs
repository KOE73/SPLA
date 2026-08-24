using SPLA.Domain.Host;
using SPLA.MCP.Core.Tools;

namespace SPLA.Plugins.Documents.Tools;

/// <summary>
/// How this plugin's tools reach a file: through the workspace, never through
/// <see cref="System.IO"/> on a path the model supplied.
///
/// <para>Two shapes are needed because the two halves of the plugin consume differently. Reading a
/// document needs BYTES, which the workspace gives directly and which also lets a blob handle stand
/// in for a path. Opening a workbook needs a HOST PATH, because ClosedXML reads and rewrites a file
/// in place and cannot be handed a byte array without losing everything in the sheet it did not
/// touch. <see cref="IWorkspace.MapPathToHost"/> is the sanctioned way to get one — the same door
/// the SFTP transport uses — and a workspace that refuses to map is a refusal to answer, not an
/// invitation to try <c>File.Open</c>.</para>
/// </summary>
internal static class DocumentsToolPaths
{
    /// <summary>File bytes from a workspace path or from a <c>blob:</c> handle produced by another
    /// tool. Returns false with a caller-fixable message.</summary>
    public static async Task<(bool Ok, byte[] Bytes, string Name, string? Error)> TryReadAsync(
        string? path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path))
            return (false, [], string.Empty, "Missing 'path'.");

        if (DataChannel.IsHandle(path))
        {
            return DataChannel.ResolveBytes(path, out var stored, out var blobError)
                ? (true, stored, path, null)
                : (false, [], path, blobError);
        }

        var workspace = HostServices.Sandbox.Workspace;
        if (!workspace.FileExists(path))
            return (false, [], path, $"File not found: {path}");

        var bytes = await workspace.ReadAllBytesAsync(path, ct);
        return (true, bytes, Path.GetFileName(path), null);
    }

    /// <summary>A real path on the machine for a workspace path, or a refusal.</summary>
    /// <param name="mustExist">False for an append that may create the file — the path still has to
    /// map, but the file does not have to be there yet.</param>
    public static bool TryHostPath(string? path, bool mustExist, out string hostPath, out string? error)
    {
        hostPath = string.Empty;
        error = null;

        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Missing 'path'.";
            return false;
        }

        var workspace = HostServices.Sandbox.Workspace;
        if (mustExist && !workspace.FileExists(path))
        {
            error = $"File not found: {path}";
            return false;
        }

        var mapped = workspace.MapPathToHost(path);
        if (string.IsNullOrWhiteSpace(mapped))
        {
            error = $"'{path}' is outside this workspace, or the workspace does not expose real paths. " +
                    "Spreadsheet tools need a real file to open.";
            return false;
        }

        hostPath = mapped;
        return true;
    }
}
