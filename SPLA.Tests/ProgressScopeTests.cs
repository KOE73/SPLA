using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;

namespace SPLA.Tests;

public class ProgressScopeTests
{
    [Fact]
    public void Report_without_open_tree_is_a_noop()
    {
        // Must not throw when no tree/node is active.
        ProgressScope.Report(1, 10);
        ProgressScope.Report(new ToolProgress { Message = "x" });
    }

    [Fact]
    public void BeginNode_without_tree_returns_noop_handle()
    {
        using var node = ProgressScope.BeginNode("network_ping_host");
        node.Fail();      // must be safe
        ProgressScope.Report(1, 1); // still a noop, no tree
    }

    [Fact]
    public void Open_node_receives_ticks_and_completes_on_dispose()
    {
        var tree = new ProgressTree();
        var changes = new List<ProgressNode>();
        tree.NodeChanged += changes.Add;

        using (ProgressScope.BeginTree(tree))
        {
            using (ProgressScope.BeginNode("network_lan_scan"))
            {
                ProgressScope.Report(1, 4);
                ProgressScope.Report(2, 4);
            }
            ProgressScope.Report(3, 4); // node disposed — lands nowhere
        }

        var node = Assert.Single(tree.Nodes);
        Assert.Equal("network_lan_scan", node.Label);
        Assert.Equal(ProgressState.Completed, node.State);
        Assert.Equal(2, node.Latest!.Current);
        Assert.Equal(0.5, node.Latest.Fraction);
        Assert.NotNull(node.FinishedAt);
        // add + 2 reports + complete = 4 change events
        Assert.Equal(4, changes.Count);
    }

    [Fact]
    public void Failed_node_is_marked_failed()
    {
        var tree = new ProgressTree();
        using (ProgressScope.BeginTree(tree))
        using (var node = ProgressScope.BeginNode("onec_build_index"))
        {
            node.Fail();
        }

        Assert.Equal(ProgressState.Failed, Assert.Single(tree.Nodes).State);
    }

    [Fact]
    public async Task Parallel_child_tools_attach_under_their_parent_node()
    {
        // The script scenario: one parent tool fans out to parallel child tool calls. Each child must
        // land as a child node of the parent — built purely from AsyncLocal forking, no wiring.
        var tree = new ProgressTree();

        using (ProgressScope.BeginTree(tree))
        using (ProgressScope.BeginNode("roslyn_script_run"))
        {
            await Parallel.ForEachAsync(Enumerable.Range(0, 20), async (i, ct) =>
            {
                using (ProgressScope.BeginNode("network_ping_host"))
                {
                    await Task.Yield();
                    ProgressScope.Report(1, 1, $"ping {i}");
                }
            });
        }

        var nodes = tree.Nodes;
        var root = Assert.Single(nodes, n => n.ParentId == null);
        Assert.Equal("roslyn_script_run", root.Label);

        var children = nodes.Where(n => n.ParentId == root.Id).ToList();
        Assert.Equal(20, children.Count);
        Assert.All(children, c => Assert.Equal("network_ping_host", c.Label));
        Assert.All(children, c => Assert.Equal(ProgressState.Completed, c.State));
    }

    [Fact]
    public void Fraction_is_null_when_total_unknown_or_zero()
    {
        Assert.Null(new ToolProgress { Current = 5 }.Fraction);
        Assert.Null(new ToolProgress { Current = 5, Total = 0 }.Fraction);
        Assert.Equal(0.5, new ToolProgress { Current = 1, Total = 2 }.Fraction);
    }
}
