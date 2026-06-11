using System.Xml.Linq;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Indexing;

/// <summary>
/// Parses 1C metadata XML / .mdo files to extract:
///   • The object itself (kind, name, synonym/summary)
///   • Child objects owned by the object (forms, table parts, attributes with complex types)
///
/// 1C metadata XML follows one of two schemas depending on platform version:
///   • Pre-8.3.10 "thick" format  — root element name matches the kind, e.g. &lt;Document&gt;
///   • 8.3.10+ EDT format         — root element is &lt;mdclass:Document xmlns:mdclass="..."&gt;
///
/// Both formats store the synonym in //Properties/Synonym/v8:item/content or
/// //Properties/Synonym/content and child forms under //ChildObjects/Forms.
/// 
/// Because real dumps differ, the parser uses liberal XPath-style searches
/// (Descendants) rather than rigid paths so it degrades gracefully.
/// </summary>
public class OneCMetadataParser
{
    // Namespace used by EDT-format files
    private static readonly XNamespace NsMd = "http://v8.1c.ru/8.1/data/core";
    private static readonly XNamespace NsV8 = "http://v8.1c.ru/8.1/data/core";

    public ParsedMetadata? Parse(string filePath, string objectKind, string objectName)
    {
        XDocument doc;
        try { doc = XDocument.Load(filePath); }
        catch { return null; }

        var result = new ParsedMetadata
        {
            Kind     = objectKind,
            Name     = objectName,
            FullName = $"{objectKind}.{objectName}",
            FilePath = filePath,
        };

        var root = doc.Root;
        if (root is null) return result;

        // ── Synonym → Summary ──────────────────────────────────────────────
        result.Summary = ExtractSynonym(root);

        // ── Child objects → owns relations ─────────────────────────────────
        result.OwnedChildren.AddRange(ExtractChildren(root, objectKind, objectName));

        return result;
    }

    // ── Synonym extraction ────────────────────────────────────────────────────

    private static string? ExtractSynonym(XElement root)
    {
        // Try several known synonym element paths
        var candidates = root.Descendants()
            .Where(e => e.Name.LocalName.Equals("Synonym", StringComparison.OrdinalIgnoreCase));

        foreach (var syn in candidates)
        {
            // Pattern 1: <Synonym><v8:item><content>...</content></v8:item></Synonym>
            var content = syn.Descendants()
                .FirstOrDefault(e => e.Name.LocalName.Equals("content", StringComparison.OrdinalIgnoreCase));
            if (content is not null && !string.IsNullOrWhiteSpace(content.Value))
                return content.Value.Trim();

            // Pattern 2: <Synonym>text</Synonym>
            if (!string.IsNullOrWhiteSpace(syn.Value) && !syn.HasElements)
                return syn.Value.Trim();
        }

        return null;
    }

    // ── Child object extraction ───────────────────────────────────────────────

    private static IEnumerable<OwnedChild> ExtractChildren(
        XElement root, string parentKind, string parentName)
    {
        // Forms
        foreach (var child in ChildrenOfType(root, "Forms", "Form"))
            yield return new OwnedChild("Form", child, parentKind, parentName);

        // Table parts (ТабличныеЧасти)
        foreach (var child in ChildrenOfType(root, "TabularSections", "TabularSection"))
            yield return new OwnedChild("TabularSection", child, parentKind, parentName);

        // Commands
        foreach (var child in ChildrenOfType(root, "Commands", "Command"))
            yield return new OwnedChild("Command", child, parentKind, parentName);
    }

    private static IEnumerable<string> ChildrenOfType(XElement root, string collectionTag, string itemTag)
    {
        var collections = root.Descendants()
            .Where(e => e.Name.LocalName.Equals(collectionTag, StringComparison.OrdinalIgnoreCase));

        foreach (var col in collections)
        {
            foreach (var item in col.Elements())
            {
                // The child may be <Form>Name</Form> or <Form><Name>...</Name>...</Form>
                if (item.Name.LocalName.Equals(itemTag, StringComparison.OrdinalIgnoreCase))
                {
                    var nameEl = item.Elements()
                        .FirstOrDefault(e => e.Name.LocalName.Equals("Name", StringComparison.OrdinalIgnoreCase));

                    var childName = nameEl is not null
                        ? nameEl.Value.Trim()
                        : item.Value.Trim();

                    if (!string.IsNullOrWhiteSpace(childName))
                        yield return childName;
                }
                else
                {
                    // plain text child: <Forms><Form>ФормаДокумента</Form></Forms>
                    var v = item.Value.Trim();
                    if (!string.IsNullOrWhiteSpace(v))
                        yield return v;
                }
            }
        }
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public class ParsedMetadata
{
    public string   Kind          { get; set; } = string.Empty;
    public string   Name          { get; set; } = string.Empty;
    public string   FullName      { get; set; } = string.Empty;
    public string?  Summary       { get; set; }
    public string   FilePath      { get; set; } = string.Empty;
    public List<OwnedChild> OwnedChildren { get; } = new();
}

public class OwnedChild
{
    public string ChildKind       { get; }
    public string ChildName       { get; }
    public string ParentKind      { get; }
    public string ParentName      { get; }
    public string ChildFullName   => $"{ParentKind}.{ParentName}.{ChildKind}.{ChildName}";

    public OwnedChild(string childKind, string childName, string parentKind, string parentName)
    {
        ChildKind  = childKind;
        ChildName  = childName;
        ParentKind = parentKind;
        ParentName = parentName;
    }
}

