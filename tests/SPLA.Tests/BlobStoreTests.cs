using System.Text;
using SPLA.Domain.Agent;
using SPLA.MCP.Core.Tools;

namespace SPLA.Tests;

public sealed class BlobStoreTests
{
    private static IDisposable Scope(BlobStore blobs)
        => AgentSessionScope.Begin(new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession(), blobs));

    // ── BlobStore unit tests ─────────────────────────────────────────────────

    [Fact]
    public void Put_and_get_text()
    {
        var store = new BlobStore();
        var handle = store.Put(BlobPayload.OfText("hello world"));
        Assert.StartsWith(BlobStore.HandlePrefix, handle);
        var payload = store.Get(handle);
        Assert.NotNull(payload);
        Assert.Equal(BlobKind.Text, payload!.Kind);
        Assert.Equal("hello world", payload.Text);
    }

    [Fact]
    public void Put_with_name_produces_stable_handle()
    {
        var store = new BlobStore();
        var h1 = store.Put(BlobPayload.OfText("v1"), "ddl");
        var h2 = store.Put(BlobPayload.OfText("v2"), "ddl");
        Assert.Equal(h1, h2);
        Assert.Equal("v2", store.Get(h1)!.Text);
    }

    [Fact]
    public void Get_accepts_handle_without_prefix()
    {
        var store = new BlobStore();
        var handle = store.Put(BlobPayload.OfText("abc"), "x");
        Assert.Equal("abc", store.Get("x")!.Text);
    }

    [Fact]
    public void Delete_removes_blob()
    {
        var store = new BlobStore();
        var h = store.Put(BlobPayload.OfBytes(new byte[] { 1, 2, 3 }));
        Assert.True(store.Delete(h));
        Assert.False(store.Delete(h));
        Assert.Null(store.Get(h));
    }

    [Fact]
    public void List_ordered_by_creation_time()
    {
        var store = new BlobStore();
        var h1 = store.Put(BlobPayload.OfText("a"), "a");
        var h2 = store.Put(BlobPayload.OfText("b"), "b");
        var list = store.List();
        Assert.Equal(2, list.Count);
        Assert.Equal(h1, list[0].Handle);
        Assert.Equal(h2, list[1].Handle);
    }

    [Fact]
    public void Size_reported_correctly_for_text()
    {
        var text = "hello";
        var payload = BlobPayload.OfText(text);
        Assert.Equal(Encoding.UTF8.GetByteCount(text), payload.Size);
    }

    [Fact]
    public void Changed_fires_on_put_and_delete()
    {
        var store = new BlobStore();
        var fires = 0;
        store.Changed += (_, _) => fires++;
        var h = store.Put(BlobPayload.OfText("x"));
        store.Delete(h);
        Assert.Equal(2, fires);
    }

    // ── DataChannel routing tests ────────────────────────────────────────────

    [Fact]
    public void Route_context_returns_payload_text()
    {
        var store = new BlobStore();
        using var _ = Scope(store);
        var payload = BlobPayload.OfText("SELECT * FROM foo");
        var result = DataChannel.Route(OutputTarget.Context, payload, "summary");
        Assert.Equal("SELECT * FROM foo", result);
    }

    [Fact]
    public void Route_blob_returns_summary_and_handle()
    {
        var store = new BlobStore();
        using var _ = Scope(store);
        var payload = BlobPayload.OfText("big data");
        var result = DataChannel.Route(OutputTarget.Blob, payload, "12 rows", "my-data");
        Assert.Contains("blob:my-data", result);
        Assert.Contains("12 rows", result);
        // Data is in store, not in the returned string
        Assert.DoesNotContain("big data", result);
        Assert.Equal("big data", store.Get("blob:my-data")!.Text);
    }

    [Fact]
    public void Route_both_returns_summary_handle_and_inline_text()
    {
        var store = new BlobStore();
        using var _ = Scope(store);
        var payload = BlobPayload.OfText("inline me");
        var result = DataChannel.Route(OutputTarget.Both, payload, "summary", "both-blob");
        Assert.Contains("blob:both-blob", result);
        Assert.Contains("inline me", result);
    }

    // ── Content type: what a blob holds, and who is allowed to look at it ────
    //
    // Regression cover for a binary config file that reached the model as a picture: nothing recorded
    // that the bytes were not an image, and image_view defaulted an unknown type to image/png.

    [Fact]
    public void Route_records_content_type_for_typeless_bytes_and_says_binary()
    {
        var store = new BlobStore();
        using var _ = Scope(store);

        // A 145-byte binary blob with no recognisable signature — the mihomo rule-set case.
        var bytes = new byte[145];
        bytes[0] = 0x4D; bytes[1] = 0x52; bytes[2] = 0x53; bytes[3] = 0x00;
        var result = DataChannel.Route(OutputTarget.Blob, BlobPayload.OfBytes(bytes), "one entry", "rules");

        Assert.Contains("binary application/octet-stream", result);
        Assert.DoesNotContain("image", result);
        Assert.Equal("application/octet-stream", store.Get("blob:rules")!.ContentType);
    }

    [Fact]
    public void Route_sniffs_a_real_image_signature()
    {
        var store = new BlobStore();
        using var _ = Scope(store);

        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 };
        DataChannel.Route(OutputTarget.Blob, BlobPayload.OfBytes(png), "a screenshot", "shot");

        Assert.Equal("image/png", store.Get("blob:shot")!.ContentType);
    }

    [Fact]
    public void Content_type_prefers_declared_then_signature_then_name()
    {
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        Assert.Equal("image/webp", BlobContentType.Resolve("image/webp", jpeg, "a.png", isText: false));
        Assert.Equal("image/jpeg", BlobContentType.Resolve(null, jpeg, "a.png", isText: false));
        Assert.Equal("image/png", BlobContentType.Resolve(null, new byte[] { 1, 2, 3, 4 }, "a.png", isText: false));
        Assert.Equal("application/octet-stream", BlobContentType.Resolve(null, new byte[] { 1, 2, 3, 4 }, "a.mrs", isText: false));
        Assert.Equal("text/plain", BlobContentType.Resolve(null, null, null, isText: true));
    }

    [Fact]
    public async Task Image_view_refuses_a_binary_blob_that_is_not_an_image()
    {
        var store = new BlobStore();
        using var _ = Scope(store);
        var handle = store.Put(BlobPayload.OfBytes(new byte[] { 0x4D, 0x52, 0x53, 0x00 }));

        var result = (await new ImageViewTool().ExecuteAsync($$"""{"handle":"{{handle}}"}""")).TextContent;

        Assert.Contains("not a viewable image", result);
        Assert.Contains("blob_peek", result);
        Assert.Empty(AgentSessionScope.Current!.Images.DrainAll());
    }

    // ── blob_peek ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Blob_peek_hex_dumps_binary()
    {
        var store = new BlobStore();
        using var _ = Scope(store);
        var handle = store.Put(BlobPayload.OfBytes(Encoding.ASCII.GetBytes("MRS\0payload")));

        var result = (await new BlobPeekTool().ExecuteAsync($$"""{"handle":"{{handle}}"}""")).TextContent;

        Assert.Contains("binary", result);
        Assert.Contains("4d 52 53 00", result);   // MRS\0
        Assert.Contains("MRS.payload", result);   // ascii gutter, unprintables as dots
    }

    [Fact]
    public async Task Blob_peek_slices_text_and_reports_the_remainder()
    {
        var store = new BlobStore();
        using var _ = Scope(store);
        var handle = store.Put(BlobPayload.OfText(new string('x', 100) + "TAIL"));

        var result = (await new BlobPeekTool().ExecuteAsync($$"""{"handle":"{{handle}}","offset":0,"length":10}""")).TextContent;

        Assert.Contains("xxxxxxxxxx", result);
        Assert.DoesNotContain("TAIL", result);
        Assert.Contains("94 more characters", result);
    }

    [Fact]
    public async Task Blob_peek_missing_handle_reports_clear_error()
    {
        var store = new BlobStore();
        using var _ = Scope(store);
        var result = (await new BlobPeekTool().ExecuteAsync("""{"handle":"blob:nope"}""")).TextContent;
        Assert.Contains("no blob found", result);
    }

    [Fact]
    public void ResolveText_literal_passthrough()
    {
        Assert.True(DataChannel.ResolveText("hello", out var text, out _));
        Assert.Equal("hello", text);
    }

    [Fact]
    public void ResolveText_resolves_blob_handle()
    {
        var store = new BlobStore();
        using var scope = Scope(store);
        store.Put(BlobPayload.OfText("from blob"), "test-handle");
        Assert.True(DataChannel.ResolveText("blob:test-handle", out var text, out var err));
        Assert.Equal("from blob", text);
    }

    [Fact]
    public void ResolveText_missing_handle_returns_error()
    {
        var store = new BlobStore();
        using var scope = Scope(store);
        Assert.False(DataChannel.ResolveText("blob:nonexistent", out var text2, out var error));
        Assert.Contains("not found", error);
    }

    [Fact]
    public void ParseTarget_defaults_to_context()
    {
        Assert.Equal(OutputTarget.Context, DataChannel.ParseTarget(null));
        Assert.Equal(OutputTarget.Context, DataChannel.ParseTarget(""));
        Assert.Equal(OutputTarget.Context, DataChannel.ParseTarget("context"));
        Assert.Equal(OutputTarget.Blob, DataChannel.ParseTarget("blob"));
        Assert.Equal(OutputTarget.Both, DataChannel.ParseTarget("both"));
        Assert.Equal(OutputTarget.Context, DataChannel.ParseTarget("unknown"));
    }
}
