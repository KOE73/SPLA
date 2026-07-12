using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// SSH plugin: read-only remote command execution over SSH. Hosts and their credential references
/// live in the project config (<c>plugins.ssh.settings</c>); the actual passwords live in the global
/// secret store and are resolved only at connect time. Runs in the host (not a sandbox) — an SSH
/// client is a self-contained outbound-TCP tool; isolation buys little here, and the credential must
/// stay host-side regardless.
/// </summary>
public sealed class SshPlugin : ISplaPlugin
{
    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        settings.Plugins.TryGetValue("ssh", out var section);
        var ssh = SshSettings.FromBlob(section?.Settings);

        return new IMcpTool[]
        {
            new SshListHostsTool(ssh),
            new SshRunTool(ssh, settings.SecretResolver),
        };
    }
}
