using System.Text;
using SplaAtlas.Model;
using SplaAtlas.Model.Json;

namespace SplaAtlas.Tests;

/// <summary>
/// The rules the model owes the contract, checked on constructed input rather than on live files.
/// </summary>
/// <remarks>
/// The round-trip test proves nothing is lost; these prove the right thing was understood. They use
/// synthetic documents deliberately: several of these cases — a translated text value, a <c>manual</c>
/// origin on an entity, a v2 bare string — do not occur in any live project yet, and a rule that is
/// only tested where it currently fires is a rule that breaks on the first file that uses it.
/// </remarks>
public sealed class ContractTests
{
    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);

    // ---- provenance ----------------------------------------------------------------------------

    [Fact]
    public void TranslatedValueCarriesItsSourceAndHash()
    {
        var catalog = TextCatalog.Parse(Bytes(
            """
            {
              "contractVersion": 3,
              "language": "en",
              "entries": {
                "e_illmgateway": {
                  "description": { "v": "Gateway to the model.", "at": "2026-08-31T14:31:40Z",
                                   "origin": "translated", "from": "ru", "fromHash": "a3f19c" }
                }
              }
            }
            """), "test", "en");

        var value = catalog["e_illmgateway"]!.Description!;

        Assert.Equal("Gateway to the model.", value.Value);
        Assert.Equal(TextOrigin.Translated, value.Origin);
        Assert.Equal("ru", value.From);
        Assert.Equal("a3f19c", value.FromHash);
        Assert.True(value.HasProvenance);
    }

    /// <summary>
    /// A v2 bare string loads with no provenance at all — not with an invented <c>authored</c>.
    /// </summary>
    /// <remarks>
    /// Calling a value authored because nobody recorded otherwise would show a clean ledger where
    /// none was ever kept, and the gap would stop being visible in exactly the report built to find it.
    /// </remarks>
    [Fact]
    public void LegacyBareStringLoadsWithoutInventedProvenance()
    {
        var catalog = TextCatalog.Parse(Bytes(
            """
            { "entries": { "c_llm": { "name": "Провайдеры и шлюз" } } }
            """), "test", "ru");

        var value = catalog["c_llm"]!.Name!;

        Assert.Equal("Провайдеры и шлюз", value.Value);
        Assert.False(value.HasProvenance);
        Assert.Null(value.Origin);
        Assert.Null(value.OriginToken);
        Assert.Null(value.At);
    }

    [Fact]
    public void MissingTextIsAGapAndNotBorrowedFromAnotherKey()
    {
        var catalog = TextCatalog.Parse(Bytes(
            """
            { "entries": { "e_one": { "description": { "v": "x", "origin": "authored" } } } }
            """), "test", "ru");

        Assert.Null(catalog["e_two"]);
        Assert.Null(catalog["e_one"]!.Doc);
    }

    // ---- origin --------------------------------------------------------------------------------

    /// <summary>
    /// The retired <c>manual</c> token reads as authored, and reading does not rewrite it.
    /// </summary>
    [Fact]
    public void LegacyManualOriginReadsAsAuthoredAndSurvivesAWrite()
    {
        const string source =
            """
            {
              "relations": [
                { "id": "r_a_b_call", "from": "e_a", "to": "e_b", "type": "call", "origin": "manual" }
              ]
            }
            """;

        var catalog = RelationCatalog.Parse(Bytes(source), "test");
        var relation = catalog.Relations[0];

        Assert.Equal(Origin.Authored, relation.Origin);
        Assert.True(relation.IsAuthored);
        Assert.Equal("manual", relation.OriginToken);
        Assert.Contains("\"origin\": \"manual\"", Encoding.UTF8.GetString(catalog.Serialize()), StringComparison.Ordinal);
    }

    /// <summary>Rewriting the token happens only when asked for, and then it happens.</summary>
    [Fact]
    public void MigratingLegacyOriginsRewritesTheTokenAndCountsIt()
    {
        var catalog = RelationCatalog.Parse(Bytes(
            """
            {
              "relations": [
                { "id": "r_a", "origin": "manual" },
                { "id": "r_b", "origin": "authored" },
                { "id": "r_c", "origin": "code" }
              ]
            }
            """), "test");

        Assert.Equal(1, catalog.MigrateLegacyOrigins());

        var written = Encoding.UTF8.GetString(catalog.Serialize());
        Assert.DoesNotContain("manual", written, StringComparison.Ordinal);
        Assert.Equal(0, catalog.MigrateLegacyOrigins());
    }

    [Fact]
    public void AnUnknownOriginTokenIsNotGuessedAt()
    {
        var catalog = EntityCatalog.Parse(Bytes(
            """
            { "entities": [ { "id": "e_a", "origin": "imported" } ] }
            """), "test");

        Assert.Null(catalog.Entities[0].Origin);
        Assert.False(catalog.Entities[0].IsAuthored);
        Assert.Equal("imported", catalog.Entities[0].OriginToken);
    }

    // ---- identifiers ---------------------------------------------------------------------------

    /// <summary>A derived relation id is built from entity ids, with the <c>e_</c> prefixes stripped.</summary>
    [Fact]
    public void RelationIdIsDerivedFromEntityIds()
    {
        Assert.Equal(
            "r_repetitionguardmiddleware_illmmiddleware_implements",
            RelationCatalog.DeriveId("e_repetitionguardmiddleware", "e_illmmiddleware", "implements"));

        // Hand-written entities do not always carry the prefix; the id still derives cleanly.
        Assert.Equal("r_a_b_call", RelationCatalog.DeriveId("a", "b", "call"));
    }

    /// <summary>
    /// An entity's id has no setter: it is the handle every other file holds, and a rename edits the
    /// name and leaves it alone.
    /// </summary>
    [Fact]
    public void RenamingATypeChangesTheNameAndNotTheId()
    {
        var catalog = EntityCatalog.Parse(Bytes(
            """
            { "entities": [ { "id": "e_chatmanager", "name": "ChatManager" } ] }
            """), "test");

        catalog.Entities[0].Name = "ConversationManager";

        Assert.Equal("e_chatmanager", catalog.Entities[0].Id);
        Assert.Contains("\"id\": \"e_chatmanager\"", Encoding.UTF8.GetString(catalog.Serialize()), StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateIdsAreReported()
    {
        var catalog = EntityCatalog.Parse(Bytes(
            """
            { "entities": [ { "id": "e_a" }, { "id": "e_b" }, { "id": "e_a" } ] }
            """), "test");

        Assert.Equal(["e_a"], catalog.DuplicateIds());
    }

    // ---- relation types ------------------------------------------------------------------------

    [Fact]
    public void StructuralTypesAreRegisteredWhenTheDictionaryLacksThem()
    {
        var catalog = RelationTypeCatalog.Parse(Bytes(
            """
            { "contractVersion": 3, "relationTypes": [ { "id": "call", "origin": "authored" } ] }
            """), "test");

        var (created, wasCreated) = catalog.EnsureStructuralType("implements");

        Assert.True(wasCreated);
        Assert.Equal("implements", created.Id);
        Assert.Equal(Origin.Code, created.Origin);
        Assert.False(catalog.EnsureStructuralType("implements").Created);
    }

    /// <summary>
    /// A flow type is a person's to define. The utility asking to register one is a bug, so it throws
    /// rather than quietly writing into somebody else's dictionary.
    /// </summary>
    [Fact]
    public void TheUtilityWillNotRegisterAFlowType()
    {
        var catalog = RelationTypeCatalog.CreateEmpty();

        Assert.Throws<ArgumentException>(() => catalog.EnsureStructuralType("data-flow"));
        Assert.Throws<ArgumentException>(() => catalog.EnsureStructuralType("security"));
    }

    /// <summary>An authored row is left exactly as written, style and all.</summary>
    [Fact]
    public void AnAuthoredRelationTypeIsNotRewritten()
    {
        const string source =
            """
            {
              "contractVersion": 3,
              "relationTypes": [
                { "id": "implements", "origin": "authored", "styleId": "edge.custom" }
              ]
            }
            """;

        var catalog = RelationTypeCatalog.Parse(Bytes(source), "test");
        catalog.EnsureStructuralType("implements");

        var written = Encoding.UTF8.GetString(catalog.Serialize());
        Assert.Contains("\"origin\": \"authored\"", written, StringComparison.Ordinal);
        Assert.Contains("\"styleId\": \"edge.custom\"", written, StringComparison.Ordinal);
    }

    [Fact]
    public void RelationTypeTextKeyCarriesThePrefixButTheIdDoesNot()
    {
        var catalog = RelationTypeCatalog.Parse(Bytes(
            """
            { "relationTypes": [ { "id": "data-flow", "origin": "authored" } ] }
            """), "test");

        Assert.Equal("data-flow", catalog.RelationTypes[0].Id);
        Assert.Equal("rt_data-flow", catalog.RelationTypes[0].TextKey);
    }

    // ---- losslessness --------------------------------------------------------------------------

    [Fact]
    public void UnknownFieldsSurviveAWrite()
    {
        const string source =
            """
            {
              "entities": [
                { "id": "e_a", "name": "A", "somethingNobodyModelled": { "deep": [ 1, 2 ] } }
              ]
            }
            """;

        var catalog = EntityCatalog.Parse(Bytes(source), "test");
        catalog.Entities[0].Name = "B";

        var written = Encoding.UTF8.GetString(catalog.Serialize());
        Assert.Contains("somethingNobodyModelled", written, StringComparison.Ordinal);
        Assert.Contains("\"deep\"", written, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reading an absent optional array does not bring it into being.
    /// </summary>
    [Fact]
    public void AskingAnEntityWithoutMembersHowManyItHasDoesNotGiveItSome()
    {
        var catalog = EntityCatalog.Parse(Bytes(
            """
            { "entities": [ { "id": "e_a", "name": "A" } ] }
            """), "test");

        Assert.Empty(catalog.Entities[0].Members);
        Assert.False(catalog.Entities[0].Members.IsMaterialised);
        Assert.DoesNotContain("members", Encoding.UTF8.GetString(catalog.Serialize()), StringComparison.Ordinal);

        catalog.Entities[0].Members.Add().Name = "Stage";
        Assert.Contains("\"members\"", Encoding.UTF8.GetString(catalog.Serialize()), StringComparison.Ordinal);
    }

    [Fact]
    public void KeyOrderIsTheOrderTheFileHad()
    {
        const string source =
            """
            {
              "relations": [
                { "status": "present", "type": "call", "id": "r_a", "to": "e_b", "from": "e_a" }
              ]
            }
            """;

        var catalog = RelationCatalog.Parse(Bytes(source), "test");
        catalog.Relations[0].Type = "event";

        var written = Encoding.UTF8.GetString(catalog.Serialize());
        Assert.True(
            written.IndexOf("\"status\"", StringComparison.Ordinal) <
            written.IndexOf("\"type\"", StringComparison.Ordinal));
        Assert.True(
            written.IndexOf("\"id\"", StringComparison.Ordinal) <
            written.IndexOf("\"from\"", StringComparison.Ordinal));
    }

    /// <summary>An explicit JSON null is not the same statement as an absent key.</summary>
    [Fact]
    public void AnExplicitNullParentIsNotAnAbsentOne()
    {
        var catalog = ContainerCatalog.Parse(Bytes(
            """
            { "containers": [ { "id": "c_a", "parent": null }, { "id": "c_b" } ] }
            """), "test");

        Assert.True(catalog.Containers[0].HasExplicitNullParent);
        Assert.True(catalog.Containers[0].Has("parent"));
        Assert.False(catalog.Containers[1].HasExplicitNullParent);
        Assert.False(catalog.Containers[1].Has("parent"));

        Assert.Contains("\"parent\": null", Encoding.UTF8.GetString(catalog.Serialize()), StringComparison.Ordinal);
    }

    // ---- ownership -----------------------------------------------------------------------------

    /// <summary>
    /// Views and text catalogs are not on the list of files a sync run may write.
    /// </summary>
    [Fact]
    public void OnlyTheThreeRegistriesAreWritableByTheUtility()
    {
        Assert.True(ProjectPaths.IsWritableByUtility(ProjectPaths.Entities));
        Assert.True(ProjectPaths.IsWritableByUtility(ProjectPaths.Relations));
        Assert.True(ProjectPaths.IsWritableByUtility(ProjectPaths.RelationTypes));

        Assert.False(ProjectPaths.IsWritableByUtility(ProjectPaths.Manifest));
        Assert.False(ProjectPaths.IsWritableByUtility(ProjectPaths.Containers));
        Assert.False(ProjectPaths.IsWritableByUtility(ProjectPaths.TextCatalog("ru")));
        Assert.False(ProjectPaths.IsWritableByUtility("v_main.view.json"));
    }
}
