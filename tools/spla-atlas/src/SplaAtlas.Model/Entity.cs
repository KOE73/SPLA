using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>One record of <c>entities.json</c>.</summary>
public sealed class Entity(JsonObject node) : JsonBacked(node)
{
    /// <summary>
    /// Prefixed <c>e_</c>. Issued once and never changed, a rename included.
    /// </summary>
    /// <remarks>
    /// There is no setter, and that is the point. Everything that refers to an entity — texts,
    /// relations, placements on views — refers to this string; changing it is a silent break spread
    /// across four other files. A rename edits <see cref="Name"/> and leaves this alone, after which
    /// id and name legitimately disagree.
    /// </remarks>
    public string? Id => GetString("id");

    /// <summary>The canonical type name. Never translated; no text catalog carries it.</summary>
    public string? Name
    {
        get => GetString("name");
        set => SetString("name", value);
    }

    /// <summary>
    /// Open string. Code yields <c>class</c>, <c>interface</c>, <c>record</c>, <c>struct</c>,
    /// <c>enum</c>; hand-written entities use whatever fits (<c>external</c>, <c>database</c>, …).
    /// </summary>
    /// <remarks>
    /// Live data spells these inconsistently — 1066 records say <c>Class</c> and 52 say <c>class</c>
    /// — so nothing here folds case. Deciding on one spelling is the extractor's business, and it
    /// will show up as drift when it does.
    /// </remarks>
    public string? Kind
    {
        get => GetString("kind");
        set => SetString("kind", value);
    }

    /// <summary>Raw <c>origin</c> token, as spelled on disk.</summary>
    public string? OriginToken
    {
        get => GetString("origin");
        set => SetString("origin", value);
    }

    /// <summary>Parsed origin, or null if the token is one the contract does not define.</summary>
    public Origin? Origin => Model.OriginToken.Parse(OriginToken);

    /// <summary>Whether a person keeps this record. The utility must leave those entirely alone.</summary>
    public bool IsAuthored => Origin == Model.Origin.Authored;

    /// <summary>
    /// <c>present</c> | <c>missing</c> | <c>planned</c> per the contract; five live records say
    /// <c>gone</c>, which the task brief also uses. Left as written until that is settled.
    /// </summary>
    public string? Status
    {
        get => GetString("status");
        set => SetString("status", value);
    }

    public string? Namespace
    {
        get => GetString("namespace");
        set => SetString("namespace", value);
    }

    /// <summary>Repository-relative path to the declaring file.</summary>
    public string? CodeRef
    {
        get => GetString("codeRef");
        set => SetString("codeRef", value);
    }

    /// <summary>Undocumented but present on two live records. Modelled so it survives a rewrite.</summary>
    public string? FirstSeen
    {
        get => GetString("firstSeen");
        set => SetString("firstSeen", value);
    }

    /// <summary>Members of the type, for the detailed render.</summary>
    public JsonBackedList<EntityMember> Members => field ??= List("members", n => new EntityMember(n));
}
