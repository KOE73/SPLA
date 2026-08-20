using System.Net.Http.Headers;
using System.Net.Http.Json;
using SPLA.Domain.Project;

namespace SPLA.Instances;

/// <summary>
/// The registry as seen from outside: one HTTP call to a hub.
///
/// <para>The same interface as <see cref="FileInstanceRegistry"/>, and that is the whole point — a
/// consumer asks "what is running" without knowing whether the answer came off this machine's disk
/// or from a hub two hops away. Which one it gets is a launch-time decision, not a code path every
/// caller has to carry.</para>
///
/// <para>No probing here. The hub already holds live state pushed over each instance's registration
/// channel, so asking every instance again would be slower and no fresher.</para>
/// </summary>
public sealed class RemoteInstanceRegistry : IInstanceRegistry, IDisposable
{
    private readonly HttpClient _http;
    private readonly string _listUrl;
    private readonly bool _ownsClient;

    /// <param name="hubUrl">Base address of the hub, e.g. <c>http://build-server:5060</c>.</param>
    /// <param name="token">Registry token. Absent means the hub is unauthenticated, which is only
    /// reasonable on loopback — see the ADR: an open registration endpoint lets anyone on the network
    /// enumerate and stop somebody's agents.</param>
    public RemoteInstanceRegistry(string hubUrl, string? token = null, HttpClient? http = null)
    {
        _listUrl = hubUrl.TrimEnd('/') + RegistryRoutes.Instances;
        _ownsClient = http is null;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<IReadOnlyList<InstanceRecord>> ListAsync(CancellationToken ct = default)
    {
        var response = await _http.GetFromJsonAsync<RegistryListResponse>(_listUrl, RegistryJson.Options, ct);
        return (response?.Instances ?? []).Select(ToRecord).ToList();
    }

    public async Task<InstanceRecord?> FindAsync(string projectId, CancellationToken ct = default)
    {
        var all = await ListAsync(ct);
        return all.FirstOrDefault(r => string.Equals(r.ProjectId, projectId, StringComparison.OrdinalIgnoreCase));
    }

    private static InstanceRecord ToRecord(RegisteredInstanceDto dto) => dto.ToRecord();

    public void Dispose()
    {
        if (_ownsClient) _http.Dispose();
    }
}

/// <summary>Where the hub answers. One place, because a path typed twice is a path that eventually
/// differs between the two ends.</summary>
public static class RegistryRoutes
{
    /// <summary>WebSocket an instance registers on and then pushes its state over.</summary>
    public const string Channel = "/registry/ws";

    /// <summary>Plain GET returning <see cref="RegistryListResponse"/> — what observers read.</summary>
    public const string Instances = "/registry/instances";

    /// <summary>POST asking the hub to relay a stop to one instance, identified by query
    /// <c>?instance=&lt;id&gt;</c> with an optional <c>&amp;force=true</c>.</summary>
    public const string Stop = "/registry/stop";

    /// <summary>POST asking the hub to relay a stop to <b>everything</b> registered against one
    /// project, identified by query <c>?project=&lt;id&gt;</c> with an optional <c>&amp;force=true</c>.
    /// This is "close the project" — the agent and its windows together, which is the only form of it
    /// that does not leave something behind.</summary>
    public const string StopProject = "/registry/stop-project";

    /// <summary>POST asking the hub to relay a focus request to one participant, identified by query
    /// <c>?instance=&lt;id&gt;</c>. What "Open" uses when a window for the project already exists.</summary>
    public const string Focus = "/registry/focus";

    /// <summary>WebSocket an observer opens to receive the whole listing whenever it changes. The
    /// same JSON as <see cref="Instances"/>, pushed instead of asked for: the state worth watching
    /// is somebody being waited for, and noticing that on a poll interval is not noticing it.</summary>
    public const string Watch = "/registry/watch";
}
