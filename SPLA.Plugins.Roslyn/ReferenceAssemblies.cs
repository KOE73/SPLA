using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPLA.Plugins.Roslyn;

/// <summary>
/// The set of metadata references a compilation needs to see the .NET base class library. Built once
/// from the runtime's trusted platform assemblies (the same set the host process runs against), so
/// compiled snippets can use <c>System.*</c> without the caller listing anything. This is the
/// "isolated" reference set — it deliberately does NOT include the user's project or its NuGet
/// packages; resolving those requires an MSBuild workspace (a later step).
/// </summary>
internal static class ReferenceAssemblies
{
    private static readonly Lazy<IReadOnlyList<MetadataReference>> _bcl = new(BuildBcl);

    /// <summary>Cached metadata references for the .NET base class library.</summary>
    public static IReadOnlyList<MetadataReference> Bcl => _bcl.Value;

    private static IReadOnlyList<MetadataReference> BuildBcl()
    {
        var tpa = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string) ?? string.Empty;
        var refs = new List<MetadataReference>();
        foreach (var path in tpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                if (File.Exists(path))
                    refs.Add(MetadataReference.CreateFromFile(path));
            }
            catch
            {
                // Skip anything that can't be loaded as a metadata reference; the rest still compile.
            }
        }
        return refs;
    }
}
