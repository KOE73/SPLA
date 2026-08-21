using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools.Resources;

/// <summary>
/// What every resource verb tool shares: the registry, the address parse, and the refusal wording.
///
/// <para><b>Why one tool per verb rather than one tool with a <c>verb</c> argument.</b> The
/// permission pipeline's verdict is a pure function of <see cref="ToolFunctionDefinition"/>'s
/// <c>Scope</c>/<c>Effect</c>/<c>Risk</c> — it is decided before any argument is looked at, and a
/// remembered grant is keyed on (tool, arguments) where <c>*</c> means "always". A single
/// <c>resource</c> tool would therefore have to declare the worst case across all six verbs, so
/// reading a file would ask the person for delete-level trust, and a grant given for a read would
/// stand for a delete. Six definitions cost six small classes; the alternative costs the meaning of
/// every answer the human gives.</para>
///
/// <para><b>Affordances are read, never discovered by throwing.</b> Support for a verb is checked
/// with <see cref="ResourceRegistry.Supports"/> BEFORE the call, and the refusal names the verbs the
/// scheme does serve — the same idiom <see cref="ResourceRegistry.TryResolve"/> uses when it names
/// the schemes that exist. A model that learns "sftp cannot delete" from a stack trace has been
/// taught the same fact at the cost of a turn.</para>
/// </summary>
public abstract class ResourceToolBase : IMcpTool
{
    protected ResourceToolBase(ResourceRegistry resources)
        => Resources = resources ?? throw new ArgumentNullException(nameof(resources));

    /// <summary>The project's address space. Required rather than optional: an optional registry
    /// would grow a fallback path, and a fallback path is how a seam ends up running empty in
    /// production while looking wired in the type system.</summary>
    protected ResourceRegistry Resources { get; }

    public abstract string Name { get; }

    public abstract ToolDefinition GetDefinition();

    public abstract Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default);

    /// <summary>The <c>uri</c> parameter, declared once so six schemas cannot drift apart.</summary>
    protected static object UriParameter => new
    {
        type = "string",
        description = "The resource address: scheme://authority/path — e.g. file:///src/app.cs, " +
                      "sftp://host/etc/nginx.conf. The schemes available to you, and the verbs each " +
                      "one supports, are listed in your system prompt."
    };

    /// <summary>
    /// Parses the arguments, resolves the address and checks the verb, in that order — the order the
    /// failures are cheapest to explain in. On failure the returned result already carries the text
    /// the model should read.
    /// </summary>
    protected bool TryOpen(
        string argumentsJson,
        ResourceVerb verb,
        out JsonDocument document,
        out IResourceProvider provider,
        out ResourceUri uri,
        out ToolResult failure)
    {
        document = null!;
        provider = null!;
        uri = default;
        failure = null!;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
        }
        catch (JsonException)
        {
            failure = ToolResult.Fail("error: invalid_json", "invalid json");
            return false;
        }

        var address = ToolJson.GetStringTrimmed(doc.RootElement, "uri");
        if (address is null)
        {
            doc.Dispose();
            failure = ToolResult.Fail("error: 'uri' is required — an address of the form scheme://path.", "missing uri");
            return false;
        }

        if (!Resources.TryResolve(address, out provider, out uri, out var error))
        {
            doc.Dispose();
            // TryResolve's own text already names the schemes that DO exist; repeating it verbatim is
            // the point — one wording for "there is no such address", wherever it is discovered.
            failure = ToolResult.Fail($"error: {error}", "unresolved address");
            return false;
        }

        if (!ResourceRegistry.Supports(provider, verb))
        {
            var supported = string.Join(", ", ResourceRegistry.VerbsOf(provider).Select(Word));
            doc.Dispose();
            failure = ToolResult.Refuse(
                $"error: scheme '{uri.Scheme}://' does not support '{Word(verb)}'. It supports: {supported}.",
                "verb not supported");
            return false;
        }

        document = doc;
        return true;
    }

    /// <summary>Turns a provider's exception into a failure the model can act on, while letting the
    /// two kinds that must never be flattened travel on: a boundary refusal is a DECISION (told it
    /// was a fault, a model retries or starts repairing something it was never allowed to touch), and
    /// a cancellation is not an outcome at all.</summary>
    protected static ToolResult Failed(string what, Exception ex)
        => ToolResult.Fail($"error: {what} — {ex.Message}", "resource call failed");

    /// <summary>The lower-case wire word for a verb — the same vocabulary
    /// <c>ResourceSchemesContributor</c> renders into the system prompt, so the promise it made in
    /// advance is kept here rather than translated.</summary>
    protected static string Word(ResourceVerb verb) => verb switch
    {
        ResourceVerb.Read => "read",
        ResourceVerb.Exists => "exists",
        ResourceVerb.List => "list",
        ResourceVerb.Write => "write",
        ResourceVerb.Delete => "delete",
        ResourceVerb.MakeDir => "mkdir",
        _ => verb.ToString().ToLowerInvariant()
    };
}
