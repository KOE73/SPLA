using SPLA.Domain.Project;
using SPLA.Registry;
using SPLA.Service.Contracts;

namespace SPLA.CLI.Wire;

/// <summary>
/// Asks an instance what it is doing over the same WebSocket protocol every other client speaks.
///
/// <para>Lives here rather than in the registry project because it is the protocol client that is
/// heavy, not the question: a consumer that only wants to know which projects are held can build a
/// <see cref="FileInstanceRegistry"/> with no probe at all and never link a socket.</para>
/// </summary>
internal sealed class WireInstanceProbe(TimeSpan timeout) : IInstanceProbe
{
    public async Task<(InstanceState State, int Clients)?> ProbeAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);

            await using var client = await CliWireClient.ConnectAsync(endpoint, null, cts.Token);
            var status = await client.RequestInstanceStatusAsync(MessageTypes.InstanceStatus, null, cts.Token);
            if (status is null) return null;

            return InstanceStates.TryParse(status.State, out var state) ? (state, status.Clients) : null;
        }
        catch
        {
            // The lock says somebody holds it, but nobody answered: a stale endpoint over SMB, a
            // firewall, a process wedged before it can accept. The caller keeps "unreachable", which
            // is the honest answer — an observation that failed, not a fact about the instance.
            return null;
        }
    }
}
