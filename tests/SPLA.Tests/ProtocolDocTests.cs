using System.Reflection;
using System.Runtime.CompilerServices;
using SPLA.Service.Contracts;

namespace SPLA.Tests;

/// <summary>
/// Drift guard for the wire protocol: every <see cref="MessageTypes"/> constant must be documented in
/// <c>agents/protocol.md</c>. Message names are soft strings the TS client matches by value, so an
/// undocumented type is exactly the kind of silent mismatch the registry exists to prevent. Adding a
/// constant without a table row here turns this test red.
/// </summary>
public sealed class ProtocolDocTests
{
    [Fact]
    public void Every_message_type_constant_is_documented()
    {
        var doc = File.ReadAllText(ProtocolDocPath());

        var undocumented = typeof(MessageTypes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true } && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            // The doc lists each wire name as `value` (inline code); require that exact form so a bare
            // substring match can't hide a missing entry.
            .Where(value => !doc.Contains($"`{value}`"))
            .OrderBy(v => v)
            .ToArray();

        Assert.True(undocumented.Length == 0,
            "Undocumented MessageTypes in agents/protocol.md: " + string.Join(", ", undocumented));
    }

    /// <summary>Locates <c>agents/protocol.md</c> from this test's source location (repo root is three
    /// levels up: tests/SPLA.Tests/ → repo). Independent of the build output directory.</summary>
    private static string ProtocolDocPath([CallerFilePath] string thisFile = "")
    {
        var repoRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", ".."));
        return Path.Combine(repoRoot, "agents", "protocol.md");
    }
}
