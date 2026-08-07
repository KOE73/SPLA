using System.Text;
using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Ssh.Transfer;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>sftp_upload</c> — sends a file, a folder or a container from the project to a remote host.
/// <para>
/// The mirror image of <see cref="SftpDownloadTool"/>, on purpose and in every detail: the same
/// local/remote pair, the same include/exclude globs, the same <c>.tar</c>-by-name rule. When the
/// two halves have the same shape a model that managed a download can do an upload without learning
/// anything new — and the previous version, which only accepted bytes, forced it to invent the tree
/// walk itself and get lost partway.
/// </para>
/// <para>
/// This is the half that changes a server, so it is refused unless the host is marked
/// <c>allow_write</c>. SFTP never touches the shell, which means <see cref="ReadOnlyGuard"/> — the
/// thing that stops the agent mutating a host through commands — cannot see it at all; the same
/// decision therefore has to be enforced in the transfer, or the guard would have an open side door.
/// </para>
/// </summary>
public sealed class SftpUploadTool : IMcpTool
{
    private readonly SftpTransfer _transfer;

    public SftpUploadTool(SftpTransfer transfer) => _transfer = transfer;

    public string Name => "sftp_upload";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Copies a file, a whole folder, or a .tar container from the project onto a remote host over SFTP. " +
                "One call does the whole job — do NOT read files and send them one by one. " +
                "local_path is a file, a folder, or a container made by sftp_download; remote_path is where it goes " +
                "(a directory for a folder or container; for a single file, the full destination path, or a directory " +
                "to put it in under its own name). Missing directories are created. A container is unpacked with the " +
                "symlinks and permissions it recorded. Only hosts the operator marked as writable accept uploads; " +
                "on a read-only host this is refused and there is no way around it. " +
                "To write content you generated yourself, with no file in the project, use sftp_write_file instead.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.High,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    local_path = new { type = "string", description = "What to send, relative to the project: a file ('conf/app.yml'), a folder ('conf'), or a container ('staging/prod.tar')." },
                    remote_path = new { type = "string", description = "Destination on the remote host, e.g. '/opt/app' for a folder or '/opt/app/docker-compose.yml' for one file." },
                    host = new { type = "string", description = "Configured host name. Omit to use the default host." },
                    recursive = new { type = "boolean", description = "Descend into subfolders when local_path is a folder. Default true." },
                    include = new { type = "string", description = "Only send matching files, e.g. '*.conf,*.yml,**/.env'. Omit to send everything." },
                    exclude = new { type = "string", description = "Skip matching files, e.g. '**/node_modules/**,**/*.log'." },
                    on_conflict = new
                    {
                        type = "string",
                        @enum = new[] { "overwrite", "skip", "newer", "abort" },
                        description =
                            "What to do about files that already exist on the host. " +
                            "'overwrite' (default) replaces them; " +
                            "'skip' leaves them as they are and reports how many were left alone; " +
                            "'newer' replaces only the ones older than your local copy; " +
                            "'abort' sends NOTHING AT ALL if even one of them exists — use it when the target must be untouched."
                    }
                },
                required = new[] { "local_path", "remote_path" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string localPath, remotePath;
        string? host, include, exclude, conflictName;
        bool recursive;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            localPath = ToolJson.GetStringTrimmed(doc.RootElement, "local_path") ?? "";
            remotePath = ToolJson.GetStringTrimmed(doc.RootElement, "remote_path") ?? "";
            host = ToolJson.GetStringTrimmed(doc.RootElement, "host");
            include = ToolJson.GetStringTrimmed(doc.RootElement, "include");
            exclude = ToolJson.GetStringTrimmed(doc.RootElement, "exclude");
            recursive = ToolJson.GetBoolean(doc.RootElement, "recursive", true);
            conflictName = ToolJson.GetStringTrimmed(doc.RootElement, "on_conflict");
        }
        catch (JsonException)
        {
            return ToolResult.Fail("Error: invalid JSON arguments.", "invalid json");
        }

        if (localPath.Length == 0) return ToolResult.Fail("Error: 'local_path' is required.", "missing local_path");
        if (remotePath.Length == 0) return ToolResult.Fail("Error: 'remote_path' is required.", "missing remote_path");

        var conflict = ParseConflict(conflictName);
        if (conflict is null)
            return ToolResult.Fail(
                $"Error: unknown on_conflict '{conflictName}' — use overwrite, skip, newer or abort.",
                "bad on_conflict");

        try
        {
            var (name, cfg) = _transfer.ResolveHost(host);
            var result = await _transfer.UploadAsync(
                cfg, name, localPath, remotePath, recursive, include, exclude, conflict.Value, cancellationToken);

            var text = new StringBuilder(
                $"Uploaded {result.Transferred} file(s), {result.BytesTransferred} bytes, from {localPath}\n" +
                $"to {name}:{result.Target}");
            if (result.Skipped > 0) text.Append($"\nleft as they were: {result.Skipped}");
            if (result.Problems.Count > 0)
            {
                text.Append($"\nnot sent: {result.Problems.Count}");
                foreach (var problem in result.Problems.Take(10))
                    text.Append($"\n  {problem.Path}: {problem.Reason}");
            }
            return ToolResult.Text(text.ToString());
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error: {ex.Message}", "upload failed");
        }
    }

    /// <summary>Absent means overwrite — the same default the download side has, so the two halves
    /// behave alike when the model says nothing.</summary>
    private static UploadConflict? ParseConflict(string? name) => name?.ToLowerInvariant() switch
    {
        null or "" or "overwrite" => UploadConflict.Overwrite,
        "skip" => UploadConflict.Skip,
        "newer" => UploadConflict.Newer,
        "abort" => UploadConflict.Abort,
        _ => null
    };
}
