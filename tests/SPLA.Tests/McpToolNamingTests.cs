using SPLA.Mcp.Client;

namespace SPLA.Tests;

/// <summary>
/// The prefixing rule at the heart of ADR_20260826_service_mcp-client §2: always
/// <c>&lt;server_id&gt;_&lt;tool&gt;</c>, never only on conflict, and no dots or colons — a stranger's
/// suggestion of <c>ghmcp:create_issue</c> is exactly the shape this file exists to reject.
/// </summary>
public sealed class McpToolNamingTests
{
    [Theory]
    [InlineData("ghmcp")]
    [InlineData("a")]
    [InlineData("sql_prod")]
    [InlineData("server123")]
    public void A_well_formed_server_id_is_accepted(string id) =>
        Assert.True(McpToolNaming.IsValidServerId(id));

    [Theory]
    [InlineData("GHMCP")]          // uppercase
    [InlineData("1server")]        // starts with a digit
    [InlineData("gh-mcp")]         // hyphen, not underscore
    [InlineData("gh.mcp")]         // dot
    [InlineData("gh:mcp")]         // colon — the shape a stranger suggested
    [InlineData("")]
    [InlineData(null)]
    [InlineData("this_id_is_seventeen_chars")]   // over the 16-char cap
    public void A_malformed_server_id_is_rejected(string? id) =>
        Assert.False(McpToolNaming.IsValidServerId(id));

    [Fact]
    public void A_tool_is_always_prefixed_not_only_on_conflict()
    {
        var name = McpToolNaming.Prefixed("ghmcp", "create_issue", out var refusal);

        Assert.Equal("ghmcp_create_issue", name);
        Assert.Null(refusal);
    }

    [Fact]
    public void No_dots_or_colons_survive_into_the_prefixed_name()
    {
        var name = McpToolNaming.Prefixed("ghmcp", "create_issue", out _);
        Assert.NotNull(name);
        Assert.DoesNotContain(':', name);
        Assert.DoesNotContain('.', name);
    }

    [Fact]
    public void An_invalid_server_id_refuses_registration_with_a_reason()
    {
        var name = McpToolNaming.Prefixed("GH-MCP", "create_issue", out var refusal);

        Assert.Null(name);
        Assert.NotNull(refusal);
        Assert.Contains("server id", refusal);
    }

    [Fact]
    public void A_name_over_the_length_limit_is_refused_rather_than_truncated()
    {
        // Truncating or hashing would still collide eventually, silently — a missing tool is
        // visible, a quietly renamed one is not. This is the case that must fail loudly.
        var longTool = new string('x', 60);

        var name = McpToolNaming.Prefixed("ghmcp", longTool, out var refusal);

        Assert.Null(name);
        Assert.NotNull(refusal);
        Assert.Contains((McpToolNaming.MaxToolNameLength).ToString(), refusal);
    }

    [Fact]
    public void A_name_exactly_at_the_length_limit_is_accepted()
    {
        // ghmcp_ is 6 chars; 58 more brings the total to exactly 64.
        var tool = new string('x', 58);

        var name = McpToolNaming.Prefixed("ghmcp", tool, out var refusal);

        Assert.NotNull(name);
        Assert.Equal(McpToolNaming.MaxToolNameLength, name!.Length);
        Assert.Null(refusal);
    }

    [Fact]
    public void An_empty_tool_name_is_refused()
    {
        var name = McpToolNaming.Prefixed("ghmcp", "", out var refusal);
        Assert.Null(name);
        Assert.NotNull(refusal);
    }
}
