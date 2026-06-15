using System.Net;
using System.Reflection;

namespace SPLA.Tests;

public class NetworkScanHelpersTests
{
    [Fact]
    public void ParseHostRange_Cidr24_ReturnsEveryAddressIncludingNetworkAndBroadcast()
    {
        var helperType = typeof(SPLA.Plugins.Network.LanScanTool).Assembly.GetType("SPLA.Plugins.Network.NetworkScanHelpers")
            ?? throw new InvalidOperationException("NetworkScanHelpers type was not found.");

        var parseHostRange = helperType.GetMethod("ParseHostRange", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("ParseHostRange method was not found.");

        var addresses = Assert.IsAssignableFrom<IReadOnlyList<IPAddress>>(
            parseHostRange.Invoke(null, ["192.168.10.0/24", null, null, 256]));

        Assert.Equal(256, addresses.Count);
        Assert.Equal(IPAddress.Parse("192.168.10.0"), addresses[0]);
        Assert.Equal(IPAddress.Parse("192.168.10.255"), addresses[^1]);
    }
}
