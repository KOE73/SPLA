namespace SplaAtlas.Model;

/// <summary>
/// The file names a project directory is made of, and who is allowed to write each.
/// </summary>
public static class ProjectPaths
{
    public const string Manifest = "project.json";
    public const string Entities = "entities.json";
    public const string Relations = "relations.json";
    public const string RelationTypes = "relation-types.json";
    public const string Containers = "containers.json";
    public const string ViewsDirectory = "views";

    public static string TextCatalog(string language) => $"text.{language}.json";

    /// <summary>
    /// The only files a sync run may write.
    /// </summary>
    /// <remarks>
    /// <c>relation-types.json</c> is on the list for one narrow reason: registering a structural
    /// type the extractor emits when the dictionary lacks it. Existing rows there belong to a person.
    /// <c>views/</c> and the text catalogs are absent from this list and there is no code in this
    /// assembly that writes them.
    /// </remarks>
    public static IReadOnlyList<string> WritableByUtility { get; } = [Entities, Relations, RelationTypes];

    /// <summary>Whether a sync run is entitled to write this file.</summary>
    public static bool IsWritableByUtility(string fileName) =>
        WritableByUtility.Contains(fileName, StringComparer.OrdinalIgnoreCase);
}
