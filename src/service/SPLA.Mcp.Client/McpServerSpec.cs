namespace SPLA.Mcp.Client;

/// <summary>How to reach one foreign MCP server. Everything that is a property of the connection,
/// and nothing that is a property of the project.</summary>
public enum McpTransportKind
{
    /// <summary>A child process speaking JSON-RPC on its stdin/stdout. What almost every published
    /// server is: an <c>npx</c> or <c>uvx</c> command.</summary>
    Stdio,

    /// <summary>An HTTP endpoint speaking MCP's "streamable HTTP" — one POST per request, with the
    /// reply either a plain JSON body or an SSE stream of frames.</summary>
    Http
}

/// <summary>
/// What to connect to, in this project's own words rather than the config file's.
///
/// <para>Deliberately not <c>SplaMcpServerSection</c>. That type is the shape of a YAML section and
/// belongs to whoever reads YAML; this one is the shape of a connection. Keeping them apart is what
/// lets the transport be tested without a settings cascade, and what stops a change to the file
/// format from reaching down here. The runtime maps one onto the other in one place (step 5).</para>
///
/// <para><b>Credentials arrive resolved.</b> <see cref="Env"/> and <see cref="Headers"/> hold actual
/// values, not <c>secret:</c> references — resolution happens above, at the moment of connecting, so
/// this project never touches the secret store and a value never sits in a settings object waiting
/// to be serialised somewhere. See <c>agents/secrets.md</c>.</para>
/// </summary>
/// <param name="Id">The server's id, used as the prefix on every tool name it contributes and as the
/// id of the tool set that holds them. Validated where it is read, not here.</param>
public sealed record McpServerSpec(string Id, McpTransportKind Transport)
{
    /// <summary>Executable to start. <see cref="McpTransportKind.Stdio"/> only.</summary>
    public string? Command { get; init; }

    /// <summary>Arguments, already split — never a single command line for a shell to re-split.
    /// <see cref="McpTransportKind.Stdio"/> only.</summary>
    public IReadOnlyList<string> Args { get; init; } = [];

    /// <summary>Working directory for the child, or null for the host process's own.
    /// <see cref="McpTransportKind.Stdio"/> only.</summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>Environment variables added to the child's environment (the parent's is inherited).
    /// Resolved values. <see cref="McpTransportKind.Stdio"/> only.</summary>
    public IReadOnlyDictionary<string, string> Env { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Endpoint to POST to. <see cref="McpTransportKind.Http"/> only.</summary>
    public string? Url { get; init; }

    /// <summary>Headers sent with every request. Resolved values.
    /// <see cref="McpTransportKind.Http"/> only.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>How long to wait for the handshake and for each call before giving up. Generous by
    /// default: a first <c>npx</c> run downloads a package.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(2);
}
