using System.Text;
using System.Text.Json;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.Ssh.Transfer;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// Tools for working with a container's contents. They exist because a container is deliberately NOT
/// wired into the project's file surface: the ordinary <c>system_*</c> file tools do not look inside
/// one, so that a few hundred foreign config files cannot turn up in every later search of the
/// project. The price is these three verbs.
/// </summary>
internal static class TarToolSupport
{
    public static TarContainer Open(string workspaceRoot, string localPath)
    {
        if (!TarContainer.IsContainerPath(localPath))
            throw new InvalidOperationException($"'{localPath}' is not a container — the path must end in .tar");
        return new TarContainer(LocalTarget.Resolve(workspaceRoot, localPath));
    }
}

/// <summary><c>tar_list</c> — what is inside a container.</summary>
public sealed class TarListTool : IMcpTool
{
    private readonly string _workspaceRoot;

    public TarListTool(string workspaceRoot) => _workspaceRoot = workspaceRoot;

    public string Name => "tar_list";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Lists the files inside a container (.tar) in the project. Shows each entry's path, size, " +
                "Unix permissions, and where symbolic links pointed on the source host.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "The container inside the project, e.g. 'staging/prod.tar'." },
                    pattern = new { type = "string", description = "Optional filter on the entry path, e.g. '*.conf'." }
                },
                required = new[] { "path" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string path;
        string? pattern;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            path = ToolJson.GetStringTrimmed(doc.RootElement, "path") ?? "";
            pattern = ToolJson.GetStringTrimmed(doc.RootElement, "pattern");
        }
        catch (JsonException)
        {
            return Task.FromResult(ToolResult.Fail("Error: invalid JSON arguments.", "invalid json"));
        }

        if (path.Length == 0) return Task.FromResult(ToolResult.Fail("Error: 'path' is required.", "missing path"));

        try
        {
            var container = TarToolSupport.Open(_workspaceRoot, path);
            if (!container.Exists) return Task.FromResult(ToolResult.Fail($"Error: no container at {path}.", "no container"));

            var entries = container.List();
            if (pattern is not null)
            {
                var needle = pattern.Trim('*');
                entries = entries.Where(e => e.Path.Contains(needle, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (entries.Count == 0) return Task.FromResult(ToolResult.Text($"{path} has no matching entries."));

            var text = new StringBuilder($"{entries.Count} entr(ies) in {path}:\n");
            foreach (var entry in entries)
            {
                text.Append($"  {entry.Type.ToString().ToLowerInvariant(),-13} {entry.Path}");
                if (entry.Type == TransferEntryType.File) text.Append($"  {entry.Size} bytes");
                text.Append($"  mode {ToOctal(entry.Mode)}");
                if (entry.LinkTarget is not null) text.Append($"  -> {entry.LinkTarget}");
                text.Append('\n');
            }
            return Task.FromResult(ToolResult.Text(text.ToString()));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Fail($"Error: {ex.Message}", "tar list failed"));
        }
    }

    private static string ToOctal(UnixFileMode mode) => Convert.ToString((int)mode & 0x1FF, 8).PadLeft(3, '0');
}

/// <summary><c>tar_read</c> — one file out of a container.</summary>
public sealed class TarReadTool : IMcpTool
{
    private readonly string _workspaceRoot;

    public TarReadTool(string workspaceRoot) => _workspaceRoot = workspaceRoot;

    public string Name => "tar_read";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Reads one file out of a container (.tar) in the project. Use it to inspect a configuration " +
                "taken off a server before deciding what belongs on another one.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "The container inside the project, e.g. 'staging/prod.tar'." },
                    entry = new { type = "string", description = "Path of the file inside the container, e.g. 'etc/nginx/nginx.conf' (as shown by tar_list)." },
                    output = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "path", "entry" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string path, entry;
        string? output, outputName;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            path = ToolJson.GetStringTrimmed(doc.RootElement, "path") ?? "";
            entry = ToolJson.GetStringTrimmed(doc.RootElement, "entry") ?? "";
            output = ToolJson.GetStringTrimmed(doc.RootElement, "output");
            outputName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
        }
        catch (JsonException)
        {
            return Task.FromResult(ToolResult.Fail("Error: invalid JSON arguments.", "invalid json"));
        }

        if (path.Length == 0 || entry.Length == 0)
            return Task.FromResult(ToolResult.Fail("Error: 'path' and 'entry' are required.", "missing arguments"));

        try
        {
            var container = TarToolSupport.Open(_workspaceRoot, path);
            var bytes = container.ReadBytes(entry);
            if (bytes is null) return Task.FromResult(ToolResult.Fail($"Error: {path} has no file entry '{entry}'.", "no such entry"));

            // Configuration is text; a binary entry can still be moved on to another host as a blob,
            // it just is not worth putting in front of the model as mojibake.
            var text = Encoding.UTF8.GetString(bytes);
            var isBinary = text.Contains('\0');
            // The entry name is the best type evidence this tool has; recording it here is what keeps a
            // consumer downstream from having to guess what the bytes are.
            var contentType = BlobContentType.Resolve(null, bytes, entry, isText: !isBinary);
            var payload = isBinary
                ? BlobPayload.OfBytes(bytes, contentType)
                : BlobPayload.OfText(text, contentType);

            return Task.FromResult(ToolResult.Text(DataChannel.Route(
                DataChannel.ParseTarget(output), payload,
                $"{entry} from {path} ({bytes.Length} bytes)", outputName)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Fail($"Error: {ex.Message}", "tar read failed"));
        }
    }
}

/// <summary>
/// <c>tar_write</c> — add, replace or remove one entry in a container.
/// <para>
/// tar has no in-place update, so every change here rewrites the whole container. That is stated in
/// the tool description rather than hidden: at configuration sizes it does not matter, and pretending
/// the operation is free would be how someone later loops it over a thousand entries.
/// </para>
/// </summary>
public sealed class TarWriteTool : IMcpTool
{
    private readonly string _workspaceRoot;

    public TarWriteTool(string workspaceRoot) => _workspaceRoot = workspaceRoot;

    public string Name => "tar_write";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Adds, replaces or removes one file inside a container (.tar) in the project. Give 'content' " +
                "(directly or as a blob:<handle>) to add or replace an entry; give delete=true to remove one. " +
                "Each call rewrites the whole container, so it is cheap for a few config files and wasteful " +
                "in a long loop — collect the edits and do them one at a time only when they are few.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "The container inside the project, e.g. 'staging/prod.tar'. Created if missing." },
                    entry = new { type = "string", description = "Path of the file inside the container, e.g. 'etc/nginx/nginx.conf'." },
                    content = new { type = "string", description = "New content, or a blob:<handle>. Required unless delete is true." },
                    delete = new { type = "boolean", description = "Remove the entry instead of writing it. Default false." },
                    mode = new { type = "string", description = "Unix permissions to record, e.g. '600'. Default '644'. Informational — nothing is applied locally." }
                },
                required = new[] { "path", "entry" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string path, entry;
        string? content, mode;
        bool delete;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            path = ToolJson.GetStringTrimmed(doc.RootElement, "path") ?? "";
            entry = ToolJson.GetStringTrimmed(doc.RootElement, "entry") ?? "";
            content = ToolJson.GetString(doc.RootElement, "content");
            mode = ToolJson.GetStringTrimmed(doc.RootElement, "mode");
            delete = ToolJson.GetBoolean(doc.RootElement, "delete", false);
        }
        catch (JsonException)
        {
            return Task.FromResult(ToolResult.Fail("Error: invalid JSON arguments.", "invalid json"));
        }

        if (path.Length == 0 || entry.Length == 0)
            return Task.FromResult(ToolResult.Fail("Error: 'path' and 'entry' are required.", "missing arguments"));

        try
        {
            var container = TarToolSupport.Open(_workspaceRoot, path);

            if (delete)
            {
                var removed = container.Delete(new[] { entry });
                return Task.FromResult(removed > 0
                    ? ToolResult.Text($"Removed {entry} from {path}.")
                    : ToolResult.Text($"{path} has no entry '{entry}' — nothing removed."));
            }

            if (content is null) return Task.FromResult(ToolResult.Fail("Error: 'content' is required unless delete is true.", "missing content"));
            if (!DataChannel.ResolveBytes(content, out var bytes, out var error))
                return Task.FromResult(ToolResult.Fail($"Error: {error}", "content unresolved"));

            var meta = new TransferEntry(
                entry, TransferEntryType.File, bytes.LongLength,
                ParseMode(mode), DateTimeOffset.UtcNow);

            container.Write(
                new[] { new PendingEntry(meta, () => new MemoryStream(bytes)) },
                append: container.Exists);

            return Task.FromResult(ToolResult.Text($"Wrote {entry} ({bytes.Length} bytes) into {path}."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Fail($"Error: {ex.Message}", "tar write failed"));
        }
    }

    private static UnixFileMode ParseMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode)) return (UnixFileMode)0b110_100_100; // 644
        try { return (UnixFileMode)Convert.ToInt32(mode.Trim(), 8); }
        catch { return (UnixFileMode)0b110_100_100; }
    }
}
