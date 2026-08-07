using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.Ssh.Transfer;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>sftp_write_file</c> — writes given content to one path on a remote host.
/// <para>
/// This exists so that <see cref="SftpUploadTool"/> can be about PATHS and nothing else. The two
/// jobs — "send what is in the project" and "write what I just composed" — used to share one tool
/// with two mutually exclusive parameters, which is exactly the shape a small model gets wrong: it
/// fills in both, or neither, and the failure looks like a transfer problem rather than a mistake in
/// the call. One purpose per tool costs an entry in the tool list and buys an argument set that
/// cannot be misread.
/// </para>
/// <para>
/// Content comes either inline or as a <c>blob:</c> handle, so a file produced elsewhere can be sent
/// without its bytes passing through the model's context.
/// </para>
/// </summary>
public sealed class SftpWriteFileTool : IMcpTool
{
    private readonly SftpTransfer _transfer;

    public SftpWriteFileTool(SftpTransfer transfer) => _transfer = transfer;

    public string Name => "sftp_write_file";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Writes content you provide into ONE file on a remote host over SFTP — for a config you composed " +
                "yourself, or for a blob:<handle> returned by another tool. To send a file, folder or container that " +
                "already exists in the project, use sftp_upload instead. Missing parent directories are created. " +
                "Only hosts the operator marked as writable accept writes; on a read-only host this is refused and " +
                "there is no way around it. " +
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

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
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
            return ToolResult.Fail("Error: invalid JSON arguments.", "invalid json");
        }

        if (remotePath.Length == 0) return ToolResult.Fail("Error: 'remote_path' is required.", "missing remote_path");
        if (content is null) return ToolResult.Fail("Error: 'content' is required.", "missing content");
        if (!DataChannel.ResolveBytes(content, out var bytes, out var error))
            return ToolResult.Fail($"Error: {error}", "content unresolved");

        try
        {
            var (name, cfg) = _transfer.ResolveHost(host);
            var result = await _transfer.WriteFileAsync(cfg, name, remotePath, bytes, overwrite, cancellationToken);
            return ToolResult.Text($"Wrote {result.BytesTransferred} bytes to {name}:{result.Target}.");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error: {ex.Message}", "write failed");
        }
    }
}
