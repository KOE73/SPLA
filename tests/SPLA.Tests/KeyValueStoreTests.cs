using System.Collections.Generic;
using System.Linq;
using SPLA.Domain.Agent;

namespace SPLA.Tests;

public class KeyValueStoreTests
{
    [Fact]
    public void Set_then_get_returns_value()
    {
        var kv = new KeyValueStore("session");
        kv.Set("context:plan", "1) scan 2) report");
        Assert.Equal("1) scan 2) report", kv.Get("context:plan"));
    }

    [Fact]
    public void Set_overwrites_existing_key()
    {
        var kv = new KeyValueStore("session");
        kv.Set("k", "a");
        kv.Set("k", "b");
        Assert.Equal("b", kv.Get("k"));
        Assert.Single(kv.List());
    }

    [Fact]
    public void Get_missing_key_returns_null()
    {
        var kv = new KeyValueStore("session");
        Assert.Null(kv.Get("nope"));
    }

    [Fact]
    public void Delete_removes_and_reports()
    {
        var kv = new KeyValueStore("session");
        kv.Set("k", "v");
        Assert.True(kv.Delete("k"));
        Assert.False(kv.Delete("k"));
        Assert.Null(kv.Get("k"));
    }

    [Fact]
    public void List_is_ordered_by_key()
    {
        var kv = new KeyValueStore("session");
        kv.Set("b", "2");
        kv.Set("a", "1");
        kv.Set("c", "3");
        Assert.Equal(new[] { "a", "b", "c" }, kv.List().Select(x => x.Key));
    }

    [Fact]
    public void Set_empty_key_throws()
    {
        var kv = new KeyValueStore("session");
        Assert.Throws<System.ArgumentException>(() => kv.Set(" ", "x"));
    }

    [Fact]
    public void LoadFrom_replaces_all_entries()
    {
        var kv = new KeyValueStore("session");
        kv.Set("old", "x");
        kv.LoadFrom(new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" });
        Assert.Null(kv.Get("old"));
        Assert.Equal("1", kv.Get("a"));
        Assert.Equal(2, kv.List().Count);
    }

    [Fact]
    public void Changed_fires_on_mutations()
    {
        var kv = new KeyValueStore("session");
        int count = 0;
        kv.Changed += (_, _) => count++;

        kv.Set("k", "v");      // 1
        kv.Delete("k");        // 2
        kv.Delete("k");        // no-op, no event
        kv.LoadFrom(new Dictionary<string, string> { ["x"] = "1" }); // 3

        Assert.Equal(3, count);
    }

    [Fact]
    public void Snapshot_is_an_independent_copy()
    {
        var kv = new KeyValueStore("session");
        kv.Set("k", "v");
        var snap = kv.Snapshot();
        kv.Set("k", "changed");
        Assert.Equal("v", snap["k"]);
    }
}
