using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

internal static class NetworkScanHelpers
{
    public static readonly int[] CommonPorts =
    {
        20, 21, 22, 23, 25, 53, 67, 68, 80, 110, 123, 135, 137, 138, 139,
        143, 161, 389, 443, 445, 465, 500, 587, 631, 993, 995, 1433, 1521,
        1883, 2049, 2375, 2376, 3000, 3306, 3389, 5000, 5432, 5672, 5900,
        6379, 8000, 8080, 8443, 8888, 9000, 9200, 9300, 11211, 27017
    };

    public static IReadOnlyList<int> ParsePorts(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("common", StringComparison.OrdinalIgnoreCase))
        {
            return CommonPorts;
        }

        if (value.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return Enumerable.Range(1, 65535).ToArray();
        }

        var ports = new SortedSet<int>();
        foreach (var part in value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var dash = part.IndexOf('-', StringComparison.Ordinal);
            if (dash > 0)
            {
                var start = ParsePort(part[..dash]);
                var end = ParsePort(part[(dash + 1)..]);
                if (start > end) (start, end) = (end, start);
                for (var port = start; port <= end; port++)
                {
                    ports.Add(port);
                }
            }
            else
            {
                ports.Add(ParsePort(part));
            }
        }

        return ports.ToArray();
    }

    public static IReadOnlyList<IPAddress> ParseHostRange(string? cidr, string? start, string? end, int maxHosts)
    {
        if (!string.IsNullOrWhiteSpace(cidr))
        {
            return ParseCidr(cidr, maxHosts);
        }

        if (!string.IsNullOrWhiteSpace(start) && !string.IsNullOrWhiteSpace(end))
        {
            var startValue = ToUInt32(IPAddress.Parse(start));
            var endValue = ToUInt32(IPAddress.Parse(end));
            if (startValue > endValue) (startValue, endValue) = (endValue, startValue);
            return BuildRange(startValue, endValue, maxHosts);
        }

        throw new ArgumentException("Specify either 'cidr' (for example 192.168.1.0/24) or both 'start' and 'end'.");
    }

    public static async Task<bool> PingAsync(IPAddress address, int timeout, CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(address, timeout);
            cancellationToken.ThrowIfCancellationRequested();
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> CheckPortAsync(string host, int port, int timeout, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port, cancellationToken).AsTask();
            var delayTask = Task.Delay(timeout, cancellationToken);
            var completed = await Task.WhenAny(connectTask, delayTask);
            if (completed != connectTask) return false;

            await connectTask;
            return client.Connected;
        }
        catch
        {
            return false;
        }
    }

    public static string FormatPorts(IEnumerable<int> ports)
    {
        return string.Join(", ", ports.OrderBy(x => x));
    }

    private static IReadOnlyList<IPAddress> ParseCidr(string cidr, int maxHosts)
    {
        var parts = cidr.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException("CIDR must look like 192.168.1.0/24.");
        }

        var baseAddress = IPAddress.Parse(parts[0]);
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var prefix) || prefix < 0 || prefix > 32)
        {
            throw new ArgumentException("CIDR prefix must be between 0 and 32.");
        }

        var baseValue = ToUInt32(baseAddress);
        var mask = prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
        var network = baseValue & mask;
        var broadcast = network | ~mask;

        var first = prefix >= 31 ? network : network + 1;
        var last = prefix >= 31 ? broadcast : broadcast - 1;
        return BuildRange(first, last, maxHosts);
    }

    private static IReadOnlyList<IPAddress> BuildRange(uint first, uint last, int maxHosts)
    {
        var count = last >= first ? (ulong)last - first + 1 : 0;
        if (count == 0) return Array.Empty<IPAddress>();
        if (count > (ulong)maxHosts)
        {
            throw new ArgumentException($"Range contains {count} hosts. Limit it to {maxHosts} or fewer hosts.");
        }

        var addresses = new List<IPAddress>((int)count);
        for (var value = first; value <= last; value++)
        {
            addresses.Add(FromUInt32(value));
            if (value == uint.MaxValue) break;
        }
        return addresses;
    }

    private static int ParsePort(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) || port < 1 || port > 65535)
        {
            throw new ArgumentException($"Invalid TCP port '{value}'. Port must be 1..65535.");
        }

        return port;
    }

    private static uint ToUInt32(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new ArgumentException("Only IPv4 addresses are supported for range scans.");
        }

        var bytes = address.GetAddressBytes();
        return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
    }

    private static IPAddress FromUInt32(uint value)
    {
        return new IPAddress(new[]
        {
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value
        });
    }
}
