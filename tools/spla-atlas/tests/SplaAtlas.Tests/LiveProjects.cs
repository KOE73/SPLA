using System.Text;

namespace SplaAtlas.Tests;

/// <summary>
/// Locates the live model projects in the repository and reports byte differences legibly.
/// </summary>
/// <remarks>
/// The suite runs against <c>docs/diagrams/projects/</c> itself rather than against copied fixtures.
/// A copy would be reformatted by whatever copied it and would stop representing the thing under
/// test within a release or two — and the interesting part of these files is precisely their
/// formatting, their key order and the leftovers nobody has cleaned up yet. Nothing here writes into
/// the repository; comparisons are against the bytes just read, and every write goes to a temp file.
/// </remarks>
internal static class LiveProjects
{
    /// <summary>Absolute path of <c>docs/diagrams/projects</c>.</summary>
    public static string Root { get; } = Locate();

    /// <summary>Project directory names, in a stable order.</summary>
    public static IEnumerable<object[]> Names()
    {
        foreach (var directory in Model.DiagramProject.Discover(Root))
        {
            yield return [Path.GetFileName(directory)];
        }
    }

    public static string DirectoryOf(string name) => Path.Combine(Root, name);

    /// <summary>
    /// Asserts two byte sequences are identical, and says where and how they differ when they are not.
    /// </summary>
    /// <remarks>
    /// A bare length-and-equality assertion on a 1 MB file tells you nothing you can act on. The
    /// failures this test is built to catch — a dropped provenance stamp, a reordered key, an eaten
    /// optional field — all look the same from the outside, so the message has to carry the offset
    /// and the surrounding text to tell them apart.
    /// </remarks>
    public static void AssertSameBytes(byte[] expected, byte[] actual, string what)
    {
        if (expected.AsSpan().SequenceEqual(actual))
        {
            return;
        }

        var limit = Math.Min(expected.Length, actual.Length);
        var offset = 0;
        while (offset < limit && expected[offset] == actual[offset])
        {
            offset++;
        }

        Assert.Fail(
            $"{what}: round-trip changed the bytes at offset {offset} " +
            $"(original {expected.Length} bytes, rewritten {actual.Length}).\n" +
            $"  original:  {Excerpt(expected, offset)}\n" +
            $"  rewritten: {Excerpt(actual, offset)}");
    }

    private static string Excerpt(byte[] bytes, int offset)
    {
        var start = Math.Max(0, offset - 60);
        var end = Math.Min(bytes.Length, offset + 60);
        var text = Encoding.UTF8.GetString(bytes, start, end - start);
        return text.Replace("\r", "\\r", StringComparison.Ordinal)
                   .Replace("\n", "\\n", StringComparison.Ordinal);
    }

    private static string Locate()
    {
        var directory = AppContext.BaseDirectory;
        while (directory is not null)
        {
            var candidate = Path.Combine(directory, "docs", "diagrams", "projects");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = Path.GetDirectoryName(directory.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        throw new DirectoryNotFoundException(
            $"docs/diagrams/projects was not found above {AppContext.BaseDirectory}.");
    }
}
