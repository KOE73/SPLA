using System.Text;
using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Ssh.Transfer;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>sftp_download</c> — brings a remote file or directory to the project over SFTP.
/// <para>
/// One tool for both a file and a tree, and one tool for both destinations: the shape is decided by
/// the destination NAME (<c>.tar</c> = container, anything else = folder) rather than by a separate
/// parameter or a separate tool. That keeps the decision where the model already has to think —
/// what to call the thing it is creating — instead of adding a mode it can get wrong.
/// </para>
/// </summary>
public sealed class SftpDownloadTool : IMcpTool
{
    private readonly SftpTransfer _transfer;

    public SftpDownloadTool(SftpTransfer transfer) => _transfer = transfer;

    public string Name => "sftp_download";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Copies a file or a whole directory from a remote host into the project over SFTP. " +
                "If local_path ends in '.tar' the files go into a container (one file holding a Linux tree, " +
                "keeping names, symlinks and permissions as they were); otherwise they are written as ordinary " +
                "files and folders. Fails before transferring anything if the selection is too large, does not fit, " +
                "or contains names a Windows folder cannot hold.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    remote_path = new { type = "string", description = "File or directory on the remote host, e.g. '/etc/nginx'." },
                    local_path = new { type = "string", description = "Destination inside the project, e.g. 'staging/prod.tar' (container) or 'staging/nginx' (plain folder). Always relative to the project." },
                    host = new { type = "string", description = "Configured host name. Omit to use the default host." },
                    recursive = new { type = "boolean", description = "Descend into subdirectories when remote_path is a directory. Default true." },
                    include = new { type = "string", description = "Only take matching files, e.g. '*.conf,*.yml,**/.env'. Omit to take everything." },
                    exclude = new { type = "string", description = "Skip matching files, e.g. '**/cache/**,**/*.log'." },
                    overwrite = new { type = "boolean", description = "Folder destination: replace files that already exist. Default true." },
                    append = new { type = "boolean", description = "Container destination: add to the existing container instead of replacing it (an entry with the same path is replaced). Default false." }
                },
                required = new[] { "remote_path", "local_path" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string remotePath, localPath;
        string? host, include, exclude;
        bool recursive, overwrite, append;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            remotePath = ToolJson.GetStringTrimmed(doc.RootElement, "remote_path") ?? "";
            localPath = ToolJson.GetStringTrimmed(doc.RootElement, "local_path") ?? "";
            host = ToolJson.GetStringTrimmed(doc.RootElement, "host");
            include = ToolJson.GetStringTrimmed(doc.RootElement, "include");
            exclude = ToolJson.GetStringTrimmed(doc.RootElement, "exclude");
            recursive = ToolJson.GetBoolean(doc.RootElement, "recursive", true);
            overwrite = ToolJson.GetBoolean(doc.RootElement, "overwrite", true);
            append = ToolJson.GetBoolean(doc.RootElement, "append", false);
        }
        catch (JsonException)
        {
            return "Error: invalid JSON arguments.";
        }

        if (remotePath.Length == 0) return "Error: 'remote_path' is required.";
        if (localPath.Length == 0) return "Error: 'local_path' is required.";

        try
        {
            var (name, cfg) = _transfer.ResolveHost(host);
            var result = await _transfer.DownloadAsync(
                cfg, remotePath, localPath, recursive, include, exclude, overwrite, append, cancellationToken);

            var text = new StringBuilder(
                $"Downloaded {result.Transferred} file(s), {result.BytesTransferred} bytes, from {name}:{remotePath}\n" +
                $"into {result.Target}");
            if (result.Skipped > 0) text.Append($"\nskipped: {result.Skipped}");
            if (result.Problems.Count > 0)
            {
                text.Append($"\nfailed: {result.Problems.Count}");
                foreach (var problem in result.Problems.Take(10))
                    text.Append($"\n  {problem.Path}: {problem.Reason}");
            }
            return text.ToString();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
