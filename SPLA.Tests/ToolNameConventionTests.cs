using System.Text.RegularExpressions;
using SPLA.Domain.Settings;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.BasicTools.Network;
using SPLA.MCP.BasicTools.SystemTools;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Network;

namespace SPLA.Tests;

public sealed class ToolNameConventionTests
{
    private static readonly Regex LowerSnakeCaseToolName = new("^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$", RegexOptions.Compiled);
    private static readonly Regex ToolNameDeclaration = new("public\\s+string\\s+Name\\s*=>\\s*\"(?<name>[^\"]+)\"", RegexOptions.Compiled);

    [Fact]
    public void Standard_and_network_tools_use_model_friendly_lower_snake_case_names()
    {
        var tools = new List<IMcpTool>
        {
            new FsListTool(),
            new FsReadTool(),
            new FsSearchTextTool(),
            new FsFindFilesTool(),
            new FsCreateTool(),
            new FsPatchTool(),
            new FsWriteTool(),
            new FsDeleteTool(),
            new RunCommandTool(),
            new DotnetBuildTool(),
            new DotnetTestTool(),
            new GetContextTool(),
            new GetCurrentDateTimeTool(),
            new WebFetchTool(),
            new WebSearchTool(),
        };

        tools.AddRange(new NetworkPlugin().Initialize(new ResolvedSettings()));

        foreach (var tool in tools)
        {
            Assert.True(tool.Name.Length <= 64, $"{tool.Name} exceeds 64 characters.");
            Assert.Matches(LowerSnakeCaseToolName, tool.Name);
        }
    }

    [Fact]
    public void Source_declared_tool_names_use_model_friendly_lower_snake_case_names()
    {
        var repoRoot = FindRepoRoot();
        var sourceFiles = Directory.EnumerateFiles(repoRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));

        var violations = new List<string>();
        foreach (var file in sourceFiles)
        {
            var text = File.ReadAllText(file);
            foreach (Match match in ToolNameDeclaration.Matches(text))
            {
                var toolName = match.Groups["name"].Value;
                if (!LowerSnakeCaseToolName.IsMatch(toolName) || toolName.Length > 64)
                {
                    violations.Add($"{Path.GetRelativePath(repoRoot, file)}: {toolName}");
                }
            }
        }

        Assert.True(violations.Count == 0, "Invalid tool names:\n" + string.Join("\n", violations));
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SPLA.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate SPLA.slnx.");
    }
}
