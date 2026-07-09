using System.Text.Json;
using System.Threading.Tasks;
using SPLA.Plugins.Roslyn;

namespace SPLA.Tests;

public class CompileCheckToolTests
{
    private static Task<string> Check(string code, string? kind = null)
    {
        var tool = new CompileCheckTool();
        var args = kind is null
            ? JsonSerializer.Serialize(new { code })
            : JsonSerializer.Serialize(new { code, kind });
        return tool.ExecuteAsync(args);
    }

    [Fact]
    public async Task Valid_library_class_reports_ok()
    {
        var result = await Check("public class C { public int Add(int a, int b) => a + b; }");
        Assert.StartsWith("ok: true", result);
        Assert.Contains("diagnostics: none", result);
    }

    [Fact]
    public async Task Syntax_error_is_reported_as_not_ok_with_cs_code()
    {
        var result = await Check("public class C { public int Bad( => ; }");
        Assert.StartsWith("ok: false", result);
        Assert.Contains("CS", result);
    }

    [Fact]
    public async Task Unknown_type_is_a_semantic_error()
    {
        var result = await Check("public class C { public Nonexistent F() => null; }");
        Assert.StartsWith("ok: false", result);
        // CS0246: type or namespace 'Nonexistent' could not be found.
        Assert.Contains("CS0246", result);
    }

    [Fact]
    public async Task Top_level_statements_compile_under_program_kind()
    {
        var result = await Check("System.Console.WriteLine(\"hi\");", kind: "program");
        Assert.StartsWith("ok: true", result);
    }

    [Fact]
    public async Task Uses_bcl_references_without_being_listed()
    {
        var code = "using System.Linq; using System.Collections.Generic; " +
                   "public class C { public int Sum(IEnumerable<int> xs) => xs.Sum(); }";
        var result = await Check(code);
        Assert.StartsWith("ok: true", result);
    }

    [Fact]
    public async Task Missing_code_returns_error()
    {
        var tool = new CompileCheckTool();
        var result = await tool.ExecuteAsync("{}");
        Assert.StartsWith("Error:", result);
    }
}
