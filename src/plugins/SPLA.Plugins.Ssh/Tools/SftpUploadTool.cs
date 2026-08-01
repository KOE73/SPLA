using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.Ssh.Transfer;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>sftp_upload</c> — writes one file onto a remote host over SFTP.
/// <para>
/// This is the half that changes a server, so it is refused unless the host is marked
/// <c>allow_write</c>. SFTP never touches the shell, which means <see cref="ReadOnlyGuard"/> — the
/// thing that stops the agent mutating a host through commands — cannot see it at all; the same
/// decision therefore has to be enforced here, or the guard would have an open side door.
/// </para>
/// <para>
/// Content comes either inline or as a <c>blob:</c> handle, so a file read elsewhere can be sent
/// without its bytes passing through the model's context.
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
                "Writes a file onto a remote host over SFTP. Content is given directly, or as a blob:<handle> " +
                "returned by another tool. Only hosts the operator marked as writable accept uploads; on a " +
                "read-only host this is refused and there is no way around it. " +
                "Ownership and permissions on the target are whatever the login user gets — set them afterwards " +
                "with a session command if they matter.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.High,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    remote_path = new { type = "string", description = "Full destination path on the remote host, e.g. '/opt/app/docker-compose.yml'." },
                    content = new { type = "string", description = "The file content, or a blob:<handle> from a previous tool call." },
                    host = new { type = "string", description = "Configured host name. Omit to use the default host." },
                    overwrite = new { type = "boolean", description = "Replace the file if it already exists. Default false." }
                },
                required = new[] { "remote_path", "content" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string remotePath;
        string? content, host;
        bool overwrite;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            remotePath = ToolJson.GetStringTrimmed(doc.RootElement, "remote_path") ?? "";
            content = ToolJson.GetString(doc.RootElement, "content");
            host = ToolJson.GetStringTrimmed(doc.RootElement, "host");
            overwrite = ToolJson.GetBoolean(doc.RootElement, "overwrite", false);
        }
        catch (JsonException)
        {
            return "Error: invalid JSON arguments.";
        }

        if (remotePath.Length == 0) return "Error: 'remote_path' is required.";
        if (content is null) return "Error: 'content' is required.";
        if (!DataChannel.ResolveBytes(content, out var bytes, out var error))
            return $"Error: {error}";

        try
        {
            var (name, cfg) = _transfer.ResolveHost(host);
            var result = await _transfer.UploadAsync(cfg, name, remotePath, bytes, overwrite, cancellationToken);
            return $"Uploaded {result.BytesTransferred} bytes to {name}:{result.Target}.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
