using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsPatchTool : IMcpTool, IToolHelpProvider
{
    public string Name => "system_patch_file";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Safely replaces a specific block of text in an existing file. The old_text block must exist uniquely in the file.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Absolute or relative path to the file to modify." },
                    old_text = new { type = "string", description = "The exact text block to search for in the file (must match uniquely)." },
                    new_text = new { type = "string", description = "The new text block to replace the old_text block with." }
                },
                required = new[] { "path", "old_text", "new_text" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var path    = ToolJson.GetStringTrimmed(doc.RootElement, "path");
            var oldText = ToolJson.GetString(doc.RootElement, "old_text");
            var newText = ToolJson.GetString(doc.RootElement, "new_text");

            if (path is null || oldText is null || newText is null)
                return "Error: Missing 'path', 'old_text', or 'new_text' parameter.";

            if (!File.Exists(path))
            {
                return $"Error: File not found at {path}";
            }

            var content = await File.ReadAllTextAsync(path, cancellationToken);
            if (content.Length == 0 && oldText.Length > 0)
            {
                return "status: failed\n" +
                       $"file: {path}\n" +
                       "reason: file_is_empty\n" +
                       "hint: file is empty, cannot apply patch.";
            }

            // Normalise line endings for comparison to prevent matches failing due to carriage return differences
            var normalizedContent = content.Replace("\r\n", "\n");
            var normalizedOld = oldText.Replace("\r\n", "\n");
            var normalizedNew = newText.Replace("\r\n", "\n");

            var index = normalizedContent.IndexOf(normalizedOld);
            if (index == -1)
            {
                return "status: failed\n" +
                       $"file: {path}\n" +
                       "reason: old_text_not_found\n" +
                       "hint: The old_text string was not found in the file. Read the file again to obtain the latest exact content.";
            }

            var secondIndex = normalizedContent.IndexOf(normalizedOld, index + normalizedOld.Length);
            if (secondIndex != -1)
            {
                return "status: failed\n" +
                       $"file: {path}\n" +
                       "reason: old_text_not_unique\n" +
                       "hint: The old_text block matches multiple locations in the file. Provide a larger unique old_text context block.";
            }

            // Execute replacement
            var replacedContent = normalizedContent.Substring(0, index) + normalizedNew + normalizedContent.Substring(index + normalizedOld.Length);
            
            // Re-apply correct line endings matching the file's original style or default to current OS line endings
            if (content.Contains("\r\n"))
            {
                replacedContent = replacedContent.Replace("\n", "\r\n");
            }

            await File.WriteAllTextAsync(path, replacedContent, cancellationToken);

            var changedLinesCount = normalizedNew.Split('\n').Length;

            return "status: success\n" +
                   $"file: {path}\n" +
                   $"changedLines: {changedLinesCount}\n" +
                   "summary: Replaced the unique target old_text block successfully.";
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error applying patch: {ex.Message}";
        }
    }

    public string? GetHelpText() =>
        """
        tool: system_patch_file

        summary: Replace one exact text block in an existing file. Use this for narrow edits after reading the current file content.

        arguments:
          path:
            required: true
            formats:
              - absolute_file_path
              - relative_file_path
          old_text:
            required: true
            rules:
              - Must match existing file content exactly after CRLF/LF normalization.
              - Must be unique in the file.
              - Include enough surrounding context to avoid ambiguous matches.
          new_text:
            required: true
            rules:
              - Replacement block.
              - May be empty only when intentionally deleting the matched block.

        behavior:
          old_text_not_found: read the file again and retry with current exact text.
          old_text_not_unique: provide a larger old_text context block.
          line_endings: preserves CRLF when the original file uses CRLF.

        risk:
          writes_file: requires Edit or Agent mode permission.

        examples:
          - request:
              path: SPLA.MCP.Core/McpHost.cs
              old_text: "public void RegisterTool(IMcpTool tool)\n{\n"
              new_text: "public void RegisterTool(IMcpTool tool)\n{\n    ArgumentNullException.ThrowIfNull(tool);\n"
        """;
}
