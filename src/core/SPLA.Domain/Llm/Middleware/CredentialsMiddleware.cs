using SPLA.Domain.Secrets;

namespace SPLA.Domain.Llm.Middleware;

/// <summary>
/// Turns the credential <i>reference</i> a connection holds into the actual key, immediately before
/// the provider call.
/// <para>
/// Innermost on purpose. Committable config (<c>*.spla</c>) must hold only a pointer —
/// <c>secret:&lt;scope&gt;:&lt;key&gt;</c> or <c>env:VAR</c> — and the plaintext must exist for as
/// short a stretch of the pipeline as possible. Sitting at <see cref="LlmPipelineStage.Transport"/>
/// puts it <b>inside</b> accounting, which is the point: the ledger records a turn without ever
/// having had access to the key material, and a failure to resolve still produces a recorded,
/// classified turn rather than a silent gap.
/// </para>
/// <para>
/// A value that is not a reference passes through untouched — that is the resolver's documented
/// behaviour for literals, and it is what keeps a local <c>api_key: lm-studio</c> working without
/// anyone having to register a secret for a placeholder.
/// </para>
/// </summary>
public sealed class CredentialsMiddleware : ILlmMiddleware
{
    private readonly ISecretResolver _resolver;

    public CredentialsMiddleware(ISecretResolver resolver) => _resolver = resolver;

    public LlmPipelineStage Stage => LlmPipelineStage.Transport;

    public async Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, LlmTurnDelegate next, CancellationToken ct)
    {
        var configured = ctx.Settings.ApiKey;

        // Settings are rebuilt per turn, so replacing the value here cannot leak into another turn or
        // into the stored connection.
        if (ISecretResolver.IsReference(configured))
        {
            string? resolved;
            try
            {
                resolved = await _resolver.ResolveAsync(configured, ct);
            }
            catch (Exception ex)
            {
                // A missing or forbidden secret is an auth failure, not a mysterious 401 later: say so
                // in the one place that knows the reference could not be honoured.
                throw new Interfaces.LlmRequestException(
                    Interfaces.LlmErrorKind.AuthFailed,
                    $"Could not resolve the credential for this connection: {ex.Message}",
                    detail: configured);
            }

            if (string.IsNullOrEmpty(resolved))
                throw new Interfaces.LlmRequestException(
                    Interfaces.LlmErrorKind.AuthFailed,
                    "The connection's credential reference resolved to nothing. Check that the secret " +
                    "exists and that this caller may read it.",
                    detail: configured);

            ctx.Settings.ApiKey = resolved;
        }

        return await next(ctx, ct);
    }
}
