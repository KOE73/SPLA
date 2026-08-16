using System;
using System.Threading;

namespace SPLA.Domain.Settings;

/// <summary>
/// Which machine layer the current async flow is on: the home directory that holds
/// <c>defaults.yaml</c>, the machine secrets and the token ledger, and how a secret store for it is
/// built.
///
/// <para><b>Why this exists.</b> Both are otherwise process-wide — <c>SPLA_HOME</c> is an environment
/// variable and <see cref="ConfigLoader.SecretStoreFactory"/> is a static. That is right for the
/// deployments they were written for, where the answer is chosen once at startup and never moves. It
/// is wrong for anything that needs a different answer for the duration of one operation while other
/// work runs beside it: setting a process global for that long is visible to everything, and
/// everything that reads the machine layer — <see cref="ConfigLoader.LoadAndResolve"/>,
/// <c>LocalProjectProvider</c>, <c>LocalProjectBackend</c>, the runtime's token ledger and skill
/// sources — reads it implicitly, so the set of things affected is neither small nor listed
/// anywhere.</para>
///
/// <para><b>Inert unless entered.</b> Nothing here changes how SPLA resolves its home: with no scope
/// open, <c>SPLA_HOME</c> and the static factory answer exactly as before. It adds a narrower answer
/// for a caller that has one, it does not replace the broad one.</para>
///
/// <para>Same <see cref="AsyncLocal{T}"/> shape as <c>AgentSessionScope</c> and <c>ProgressScope</c>,
/// and for the same reason: the override has to follow one flow across async boundaries without
/// being visible to a flow running next to it.</para>
/// </summary>
public static class MachineLayerScope
{
    private static readonly AsyncLocal<MachineLayer?> Ambient = new();

    /// <summary>The override for the current async flow, or <c>null</c> when none is open.</summary>
    public static MachineLayer? Current => Ambient.Value;

    /// <summary>
    /// Routes machine-layer resolution on this flow to the given answers until the returned handle is
    /// disposed. Nesting restores the previous scope.
    /// </summary>
    /// <param name="homeDir">Stands in for <c>SPLA_HOME</c>. Null leaves the home alone.</param>
    /// <param name="secretStoreFactory">Stands in for <see cref="ConfigLoader.SecretStoreFactory"/>.
    /// Null leaves the registered factory alone — use <paramref name="suppressSecretStoreFactory"/> to
    /// mean "no factory at all".</param>
    /// <param name="suppressSecretStoreFactory">Answer "there is no factory" on this flow, so the
    /// plaintext fallback is what gets built. Distinct from a null factory, which means "no opinion".</param>
    public static IDisposable Begin(
        string? homeDir = null,
        Func<string, string?, string, Secrets.ISecretStore?>? secretStoreFactory = null,
        bool suppressSecretStoreFactory = false)
    {
        var previous = Ambient.Value;
        Ambient.Value = new MachineLayer(homeDir, secretStoreFactory, suppressSecretStoreFactory);
        return new Restore(previous);
    }

    private sealed class Restore : IDisposable
    {
        private readonly MachineLayer? _previous;
        public Restore(MachineLayer? previous) => _previous = previous;
        public void Dispose() => Ambient.Value = _previous;
    }
}

/// <summary>One flow's answers about the machine layer. Each part is independently optional: a caller
/// that only wants a different home says only that.</summary>
/// <param name="HomeDir">Where the machine layer lives, or null to keep the ambient answer.</param>
/// <param name="SecretStoreFactory">How to build a non-default secret store, or null for no opinion.</param>
/// <param name="SuppressSecretStoreFactory">Whether to answer "no factory" regardless of what is
/// registered process-wide.</param>
public sealed record MachineLayer(
    string? HomeDir,
    Func<string, string?, string, Secrets.ISecretStore?>? SecretStoreFactory,
    bool SuppressSecretStoreFactory);
