using System.Text.Json.Nodes;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Model;

/// <summary>One entry of an entity's <c>members</c> array.</summary>
/// <remarks>
/// Three kinds occur in live data: <c>property</c> (968), <c>enumValue</c> (186) and <c>method</c>
/// (151). Which fields are filled follows from the kind — a property carries <see cref="Type"/> and
/// sometimes <see cref="TypeRef"/>, an enum value carries <see cref="Value"/>, a method carries
/// <see cref="Signature"/> — but nothing enforces that here. The contract calls <c>memberKind</c> an
/// open string, and a member shape nobody anticipated is worth reporting, not worth losing.
/// </remarks>
public sealed class EntityMember(JsonObject node) : JsonBacked(node)
{
    public string? Name
    {
        get => GetString("name");
        set => SetString("name", value);
    }

    /// <summary><c>property</c> | <c>enumValue</c> | <c>method</c> in current data; open string.</summary>
    public string? MemberKind
    {
        get => GetString("memberKind");
        set => SetString("memberKind", value);
    }

    /// <summary>Declared type name, as written in source.</summary>
    public string? Type
    {
        get => GetString("type");
        set => SetString("type", value);
    }

    /// <summary>
    /// Id of the entity this member's type resolves to, when the type is one of ours. This is what
    /// turns <c>composes</c> from a guess into a fact.
    /// </summary>
    public string? TypeRef
    {
        get => GetString("typeRef");
        set => SetString("typeRef", value);
    }

    /// <summary>Numeric value of an enum member.</summary>
    public int? Value
    {
        get => GetInt("value");
        set => SetInt("value", value);
    }

    /// <summary>Full signature of a method member.</summary>
    public string? Signature
    {
        get => GetString("signature");
        set => SetString("signature", value);
    }
}
