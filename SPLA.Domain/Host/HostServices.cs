using SPLA.Domain.Agent;

namespace SPLA.Domain.Host;

/// <summary>
/// Ambient access point tools use to reach the host boundary. Resolves the sandbox of the chat
/// whose loop is currently running via <see cref="AgentSessionScope"/>, falling back to
/// <see cref="PassthroughSandbox.Default"/> when no scope is open (direct CLI use, tests) — so the
/// fallback preserves today's behaviour.
/// </summary>
public static class HostServices
{
    public static ISandbox Sandbox => AgentSessionScope.Current?.Sandbox ?? PassthroughSandbox.Default;
}
