using System.Text.RegularExpressions;

namespace SPLA.Mcp.Client;

/// <summary>
/// The prefix every tool from a connected server carries, and the two ways registering one is
/// refused.
///
/// <para><b>Always prefixed, never only on conflict.</b> Connecting a second server must not rename
/// the first one's tools — that would silently break stored grants and chat history, which is the
/// entire reason <c>ADR_20260826_service_mcp-client</c> rejected "prefix only when names collide" as
/// an option. So every tool gets <c>&lt;server_id&gt;_&lt;tool&gt;</c> from the moment its server
/// connects, whether or not anything else is using that name.</para>
///
/// <para><b>No dots, no colons.</b> A namespaced name like <c>ghmcp:create_issue</c> — the shape a
/// stranger might reasonably suggest — is not allowed here: <c>agents/plugins.md</c> requires
/// <c>lower_snake_case</c> for every model-facing tool name, and OpenAI-compatible providers reject a
/// function name containing a colon outright. The underscore is the only separator either side
/// accepts.</para>
/// </summary>
public static partial class McpToolNaming
{
    /// <summary>What a connected server's own id must look like — the part before the underscore.
    /// Deliberately short and deliberately excludes digits in the first position and uppercase
    /// everywhere: the id is meant to be typed by a person configuring the server, not generated.</summary>
    [GeneratedRegex("^[a-z][a-z0-9_]{0,15}$")]
    private static partial Regex ServerIdPattern();

    /// <summary>The hard ceiling every provider's function-name field enforces.</summary>
    public const int MaxToolNameLength = 64;

    public static bool IsValidServerId(string? serverId) =>
        !string.IsNullOrEmpty(serverId) && ServerIdPattern().IsMatch(serverId);

    /// <summary>
    /// The name a tool from this server is registered under, or null when it cannot be — the caller
    /// is expected to skip that one tool and log why, and register everything else regardless.
    /// <para>Refuses rather than truncates or hashes. A shortened or hashed name would still collide
    /// with something eventually, silently, and would break exactly the grant stability this scheme
    /// exists to protect — a missing tool is visible; a quietly renamed one is not.</para>
    /// </summary>
    public static string? Prefixed(string serverId, string toolName, out string? refusalReason)
    {
        refusalReason = null;

        if (!IsValidServerId(serverId))
        {
            refusalReason = $"server id '{serverId}' is not a valid MCP server id " +
                "(lowercase letters, digits, underscore; must start with a letter; 16 chars max)";
            return null;
        }

        if (string.IsNullOrWhiteSpace(toolName))
        {
            refusalReason = "tool name is empty";
            return null;
        }

        var prefixed = $"{serverId}_{toolName}";

        if (prefixed.Length > MaxToolNameLength)
        {
            refusalReason = $"prefixed name '{prefixed}' is {prefixed.Length} characters, " +
                $"over the {MaxToolNameLength}-character limit";
            return null;
        }

        return prefixed;
    }
}
