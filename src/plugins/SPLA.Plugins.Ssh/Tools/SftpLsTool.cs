using System.Text;
using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Ssh.Transfer;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>sftp_ls</c> — lists files on the remote host over SFTP. Replaces parsing <c>ls -la</c> output:
/// the model gets types and sizes as data instead of guessing from columns, and a path that is a
/// file returns a single entry rather than an error.
/// </summary>
public sealed class SftpLsTool : IMcpTool
{
    private readonly SftpTransfer _transfer;

    public SftpLsTool(SftpTransfer transfer) => _transfer = transfer;

    public string Name => "sftp_ls";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Lists files and directories on a remote host over SFTP. Use this to see what exists " +
                "before transferring anything. A path pointing at a file returns just that file, with its size " +
                "and modification time.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    remote_path = new { type = "string", description = "Path on the remote host, e.g. '/opt/app' or '~/config'. No wildcards — use 'pattern' for those." },
                    host = new { type = "string", description = "Configured host name. Omit to use the default host." },
                    recursive = new { type = "boolean", description = "Descend into subdirectories. Default false. Symbolic links are listed but never followed." },
                    pattern = new { type = "string", description = "Optional filter, e.g. '*.conf' or '**/*.yml'. Comma-separated patterns are allowed." },
                    limit = new { type = "integer", description = "Maximum entries to return (default 200)." }
                },
                required = new[] { "remote_path" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string remotePath;
        string? host, pattern;
        bool recursive;
        int limit;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            remotePath = ToolJson.GetStringTrimmed(doc.RootElement, "remote_path") ?? "";
            host = ToolJson.GetStringTrimmed(doc.RootElement, "host");
            pattern = ToolJson.GetStringTrimmed(doc.RootElement, "pattern");
            recursive = ToolJson.GetBoolean(doc.RootElement, "recursive", false);
            limit = ToolJson.GetInt32Clamped(doc.RootElement, "limit", 200, 1, 2000);
        }
        catch (JsonException)
        {
            return ToolResult.Fail("Error: invalid JSON arguments.", "invalid json");
        }

        if (remotePath.Length == 0) return ToolResult.Fail("Error: 'remote_path' is required.", "missing remote_path");

        try
        {
            var (name, cfg) = _transfer.ResolveHost(host);
            var entries = await _transfer.ListAsync(cfg, remotePath, recursive, pattern, limit, cancellationToken);

            if (entries.Count == 0) return ToolResult.Text($"No entries under {remotePath} on {name}.");

            var text = new StringBuilder($"{entries.Count} entr(ies) under {remotePath} on {name}:\n");
            foreach (var entry in entries)
            {
                var kind = entry.Type switch
                {
                    TransferEntryType.Directory => "dir ",
                    TransferEntryType.SymbolicLink => "link",
                    _ => "file"
                };
                text.Append($"  {kind}  {entry.Path}");
                if (entry.Type == TransferEntryType.File) text.Append($"  {entry.Size} bytes");
                text.Append($"  {entry.ModifiedUtc:yyyy-MM-dd HH:mm}Z\n");
            }
            return ToolResult.Text(text.ToString());
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error: {ex.Message}", "sftp list failed");
        }
    }
}
