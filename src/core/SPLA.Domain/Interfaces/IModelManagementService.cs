using SPLA.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Interfaces;

/// <summary>
/// Server-management side of a provider — listing, loading and unloading models.
/// This is distinct from <see cref="ILLMService"/> (dialogue): LM Studio exposes these via its
/// native <c>/api/v1/*</c> API, which does not support custom tools or assistant messages and so
/// cannot be used for SPLA's chat. Providers that aren't LM Studio simply report
/// <see cref="IsAvailableAsync"/> = false and the management UI is hidden.
/// </summary>
public interface IModelManagementService
{
    /// <summary>True when the native management API is reachable for this endpoint.</summary>
    Task<bool> IsAvailableAsync(string baseUrl, string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists models with full capabilities from the native API, falling back to the bare
    /// OpenAI-compatible id list when the native endpoint is unavailable.
    /// </summary>
    Task<List<ModelInfo>> GetModelDetailsAsync(string baseUrl, string apiKey = "lm-studio", CancellationToken cancellationToken = default);

    /// <summary>Loads a model into memory by its key.</summary>
    Task LoadModelAsync(string baseUrl, string apiKey, string modelKey, CancellationToken cancellationToken = default);

    /// <summary>Unloads a loaded model instance by its instance id.</summary>
    Task UnloadModelAsync(string baseUrl, string apiKey, string instanceId, CancellationToken cancellationToken = default);
}
