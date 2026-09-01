using SPLA.Domain.Tools;
using System.Collections.Generic;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The forest above a single turn's <see cref="ProgressTree"/> — the piece the background-task plan
/// needs before a task can have a root that outlives the turn that started it.
/// </summary>
public class ProgressHubTests
{
    [Fact]
    public void Register_returns_a_stable_id_and_lists_the_tree()
    {
        var hub = new ProgressHub();
        var tree = new ProgressTree();

        var id = hub.Register(tree);

        Assert.Same(tree, hub.Trees[id]);
    }

    [Fact]
    public void TreeAdded_fires_with_the_same_id_Trees_uses()
    {
        var hub = new ProgressHub();
        var tree = new ProgressTree();
        string? seenId = null;
        ProgressTree? seenTree = null;

        hub.TreeAdded += (id, t) => { seenId = id; seenTree = t; };
        var id = hub.Register(tree);

        Assert.Equal(id, seenId);
        Assert.Same(tree, seenTree);
    }

    [Fact]
    public void NodeChanged_forwards_from_every_registered_tree_with_its_own_id()
    {
        var hub = new ProgressHub();
        var treeA = new ProgressTree();
        var treeB = new ProgressTree();
        var seen = new List<(string TreeId, string NodeId)>();

        hub.NodeChanged += (id, node) => seen.Add((id, node.Id));

        var idA = hub.Register(treeA);
        var idB = hub.Register(treeB);

        using (ProgressScope.BeginTree(treeA)) using (ProgressScope.BeginNode("a")) { }
        using (ProgressScope.BeginTree(treeB)) using (ProgressScope.BeginNode("b")) { }

        // Two events per node (add, then complete) — only the tree id matters here.
        Assert.Contains(seen, e => e.TreeId == idA);
        Assert.Contains(seen, e => e.TreeId == idB);
        Assert.DoesNotContain(seen, e => e.TreeId != idA && e.TreeId != idB);
    }

    [Fact]
    public void A_subscriber_that_attaches_after_registration_still_sees_the_tree_via_Trees()
    {
        // The forest is meant to be joined late — a client that opens a chat mid-background-task must
        // see the running root immediately, not just future events on it.
        var hub = new ProgressHub();
        var tree = new ProgressTree();
        var id = hub.Register(tree);

        var lateSubscriberView = hub.Trees;

        Assert.True(lateSubscriberView.ContainsKey(id));
    }

    [Fact]
    public void Forget_drops_the_tree_and_is_a_no_op_if_repeated()
    {
        var hub = new ProgressHub();
        var id = hub.Register(new ProgressTree());

        hub.Forget(id);
        hub.Forget(id); // must not throw

        Assert.False(hub.Trees.ContainsKey(id));
    }

    [Fact]
    public void Multiple_trees_get_distinct_ids()
    {
        var hub = new ProgressHub();
        var idA = hub.Register(new ProgressTree());
        var idB = hub.Register(new ProgressTree());

        Assert.NotEqual(idA, idB);
        Assert.Equal(2, hub.Trees.Count);
    }
}
