namespace SPLA.Plugins.Android.Scrcpy;

internal static class DeviceMessageDrain
{
    public static async Task RunAsync(Stream control, CancellationToken ct)
    {
        var buffer = new byte[4096];
        while (await control.ReadAsync(buffer, ct) != 0) { }
        throw new EndOfStreamException("scrcpy control connection closed.");
    }
}
