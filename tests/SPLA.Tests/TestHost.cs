using System.Runtime.CompilerServices;
using SPLA.Secrets.Dpapi;

namespace SPLA.Tests;

internal static class TestHost
{
    // Tests embedding ConfigLoader need the same backend registration as the application hosts.
    // Individual selection tests override or suppress it through MachineLayerScope.
    [ModuleInitializer]
    internal static void Initialize() => DpapiSecrets.Register();
}
