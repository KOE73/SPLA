using SPLA.Domain.Llm;
using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Assembles what the "i" popup shows for one model entry.
/// <para>
/// Three sources, deliberately kept as separate sections rather than one merged list. A balance
/// belongs to the credential, a price belongs to the model, and a rate-limit budget is the last thing
/// observed rather than a current reading — flattened together they would read as equally live claims
/// about the same thing.
/// </para>
/// <para>
/// Nothing here returns credential material. It returns what a provider says <i>about</i> a
/// credential, which is a different thing and is safe to show.
/// </para>
/// </summary>
public static class ProviderInfoOps
{
    public static async Task<ProviderInfoResult> GetAsync(
        AgentRuntime runtime, string? modelId, CancellationToken ct = default)
    {
        var entry = runtime.Settings.FindModel(modelId);
        if (entry == null)
            return new ProviderInfoResult { ModelId = modelId ?? "", Error = "Unknown model entry." };

        var result = new ProviderInfoResult
        {
            ModelId = entry.Id,
            ConnectionId = entry.Connection.Id,
            ConnectionName = entry.Connection.DisplayName,
            Provider = entry.Provider
        };

        // 1. What the provider will tell us about this key. Only providers that implement the
        //    capability; everyone else simply contributes nothing rather than an empty stub.
        if (runtime.Providers.Contains(entry.Provider) &&
            runtime.Providers.Resolve(entry.Provider) is IProviderAccountInfo account)
        {
            var apiKey = await ResolveAsync(runtime, entry.Connection.ApiKey, ct);
            var adminKey = await ResolveAsync(runtime, entry.Connection.AdminKey, ct);

            try
            {
                foreach (var section in await account.GetAccountInfoAsync(
                             entry.Endpoint ?? "", apiKey, adminKey, ct))
                    result.Sections.Add(ToDto(section));
            }
            catch (OperationCanceledException) { throw; }
            catch
            {
                // Informational panel: a provider that is down must not break the screen where its
                // settings get fixed.
            }
        }

        // 2. What we last saw on the wire. Free — these headers arrived with turns already paid for —
        //    and labelled by observation time, because they are history, not a live reading.
        var observed = runtime.ProviderState.Get(entry.Connection.Id);
        if (observed.Count > 0)
            result.Sections.Add(ToDto(new ProviderFactSection
            {
                Title = "Last seen on a request", Facts = observed
            }));

        // 3. The model half. Deliberately last and separately titled: everything above describes the
        //    key, which several models may share.
        result.Sections.Add(ToDto(new ProviderFactSection
        {
            Title = $"Model · {entry.Entry.DisplayName}",
            Facts = ModelFacts(entry)
        }));

        return result;
    }

    private static List<ProviderFact> ModelFacts(ResolvedModelEntry entry)
    {
        var now = DateTimeOffset.UtcNow;
        var facts = new List<ProviderFact>
        {
            new() { Key = "model.wire", Label = "Model", Value = entry.Model is { Length: > 0 } m ? m : "(provider default)", ObservedAt = now }
        };

        if (entry.ContextLength is > 0)
            facts.Add(new ProviderFact
            {
                Key = "model.context", Label = "Context", Value = entry.ContextLength!.Value.ToString(),
                Unit = "tokens", Kind = ProviderFactKind.Counter, ObservedAt = now
            });

        if (!string.IsNullOrWhiteSpace(entry.Endpoint))
            facts.Add(new ProviderFact { Key = "model.endpoint", Label = "Endpoint", Value = entry.Endpoint!, ObservedAt = now });

        return facts;
    }

    /// <summary>Materializes a credential reference for a read-only probe. A literal passes through,
    /// and a reference that cannot be resolved yields null rather than throwing — the panel then shows
    /// the sections it could build instead of nothing at all.</summary>
    private static async Task<string?> ResolveAsync(AgentRuntime runtime, string? value, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try { return await runtime.Settings.SecretResolver.ResolveAsync(value, ct); }
        catch { return null; }
    }

    private static ProviderFactSectionDto ToDto(ProviderFactSection s) => new()
    {
        Title = s.Title,
        DeepLink = s.DeepLink,
        Facts = s.Facts.Select(f => new ProviderFactDto
        {
            Key = f.Key,
            Label = f.Label,
            Value = f.Value,
            Unit = f.Unit,
            Kind = f.Kind.ToString().ToLowerInvariant(),
            Severity = f.Severity.ToString().ToLowerInvariant(),
            ObservedAt = f.ObservedAt,
            ResetsAt = f.ResetsAt
        }).ToList()
    };
}
