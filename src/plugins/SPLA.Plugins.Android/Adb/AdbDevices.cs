namespace SPLA.Plugins.Android.Adb;

internal sealed record AdbDevice(string Serial, string State, string? Model, string? Product, string? TransportId);

internal static class AdbDevices
{
    public static IReadOnlyList<AdbDevice> Parse(string output)
    {
        List<AdbDevice> devices = [];
        foreach (var line in output.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("List of devices") || line.StartsWith('*') || line.StartsWith("adb server")) continue;
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            string? Value(string key) => parts.FirstOrDefault(p => p.StartsWith(key + ":", StringComparison.Ordinal))?[(key.Length + 1)..];
            devices.Add(new(parts[0], parts[1] == "no" && parts.ElementAtOrDefault(2) == "permissions" ? "no permissions" : parts[1],
                Value("model"), Value("product"), Value("transport_id")));
        }
        return devices;
    }
}
