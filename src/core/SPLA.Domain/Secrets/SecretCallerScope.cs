using SPLA.Domain.Identity;

namespace SPLA.Domain.Secrets;

/// <summary>
/// Who the current call is on behalf of, carried ambiently so a resolve deep inside a plugin still
/// knows whose rights to check without threading an identity through every signature.
/// <para>
/// Same pattern as the agent's existing session/permission scopes — deliberately, so there is one
/// way to answer "who is doing this", not two. Defaults to <see cref="LocalIdentity.Single"/>, which
/// is what a CLI, a desktop run and a worker are: one person, no arbitration.
/// </para>
/// </summary>
public static class SecretCallerScope
{
    private static readonly AsyncLocal<IIdentity?> Current = new();

    /// <summary>The identity in effect, or the local single user when nothing was set.</summary>
    public static IIdentity Identity => Current.Value ?? LocalIdentity.Single;

    /// <summary>Runs the enclosed work as <paramref name="identity"/>; restores on dispose.</summary>
    public static IDisposable Begin(IIdentity identity)
    {
        var previous = Current.Value;
        Current.Value = identity;
        return new Restore(previous);
    }

    private sealed class Restore : IDisposable
    {
        private readonly IIdentity? _previous;
        private bool _done;

        public Restore(IIdentity? previous) => _previous = previous;

        public void Dispose()
        {
            if (_done) return;
            _done = true;
            Current.Value = _previous;
        }
    }
}

/// <summary>Raised when an identity is refused access to an entry. Carries the scope and key — never
/// any part of the value — so it is safe to log and to show.</summary>
public sealed class SecretAccessDeniedException : Exception
{
    public SecretScope Scope { get; }
    public string Key { get; }

    public SecretAccessDeniedException(SecretScope scope, string key, string who, SecretRight right)
        : base($"'{who}' is not allowed to {right.ToString().ToLowerInvariant()} secret " +
               $"'{SecretRef.Name(scope)}{SecretRef.ScopeSeparator}{key}'.")
    {
        Scope = scope;
        Key = key;
    }
}
