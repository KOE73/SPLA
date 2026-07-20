using System.Text.Json;
using Microsoft.Data.Sqlite;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;
using SPLA.Plugins.OneC.Web;

namespace SPLA.Tests;

public sealed class OneCWebActionsTests : IDisposable
{
    private readonly string _testDirectory = Path.Combine(
        Path.GetTempPath(),
        $"spla-onec-web-{Guid.NewGuid():N}");

    [Fact]
    public async Task Overview_search_object_and_graph_use_the_typed_index_layer()
    {
        Directory.CreateDirectory(_testDirectory);
        var databasePath = Path.Combine(_testDirectory, "onec.sqlite");
        using (var database = new OneCIndexDatabase(databasePath))
        {
            database.EnsureCreated();
            var documentId = database.UpsertObject(new()
            {
                Kind = "Document",
                Name = "SalesInvoice",
                FullName = "Document.SalesInvoice",
                Path = "Documents/SalesInvoice",
                Summary = "Sales invoice"
            });
            var registerId = database.UpsertObject(new()
            {
                Kind = "AccumulationRegister",
                Name = "Stock",
                FullName = "AccumulationRegister.Stock"
            });
            database.UpsertRelation(new()
            {
                FromObjectId = documentId,
                ToObjectId = registerId,
                Type = RelationType.Writes,
                Confidence = RelationConfidence.Exact
            });
        }

        var actions = new OneCWebActions(databasePath, _testDirectory);

        var overview = await InvokeAsync(actions, "overview");
        Assert.Equal(2, overview.GetProperty("objectCount").GetInt32());
        Assert.Equal(1, overview.GetProperty("relationCount").GetInt32());
        Assert.Equal(2, overview.GetProperty("sectionCount").GetInt32());

        var search = await InvokeAsync(actions, "search", new { query = "invoice" });
        Assert.Equal("Document.SalesInvoice", search.GetProperty("results")[0].GetProperty("fullName").GetString());

        var item = await InvokeAsync(actions, "object", new { fullName = "Document.SalesInvoice" });
        Assert.Equal("Sales invoice", item.GetProperty("summary").GetString());

        var graph = await InvokeAsync(actions, "graph", new
        {
            fullName = "Document.SalesInvoice",
            mode = "dataflow",
            depth = 2,
            limit = 100
        });
        Assert.Equal(2, graph.GetProperty("summary").GetProperty("nodeCount").GetInt32());
        Assert.Equal(1, graph.GetProperty("summary").GetProperty("edgeCount").GetInt32());
        Assert.Equal("writes", graph.GetProperty("edges")[0].GetProperty("type").GetString());
    }

    [Fact]
    public async Task Rebuild_rejects_a_directory_outside_the_project_workspace()
    {
        Directory.CreateDirectory(_testDirectory);
        var actions = new OneCWebActions(Path.Combine(_testDirectory, "onec.sqlite"), _testDirectory);
        using var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { path = Path.GetTempPath() }));

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => actions.InvokeAsync("rebuild", payload.RootElement));

        Assert.Contains("inside the project workspace", error.Message);
    }

    private static async Task<JsonElement> InvokeAsync(
        OneCWebActions actions,
        string action,
        object? payload = null)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(payload ?? new { }));
        var result = await actions.InvokeAsync(action, document.RootElement);
        return JsonSerializer.SerializeToElement(result);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory, recursive: true);
    }
}
