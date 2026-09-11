using SPLA.Domain.Agent;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using System.Security.Cryptography;
using System.Text;

namespace SPLA.Plugins.Android.Session;

internal sealed class DeviceSessionRegistry
{
    public static DeviceSessionRegistry Instance { get; } = new();
    // A lease stays locked throughout each tool call, including screenshots. Takeover and idle
    // cleanup cannot dispose a backend that is still producing a result for its previous owner.
    private readonly SemaphoreSlim gate = new(1);
    private readonly Dictionary<string, DeviceSession> devices = new(StringComparer.Ordinal);
    private readonly Timer reaper;
    internal DeviceSessionRegistry() => reaper = new(_ => _ = ReapSafelyAsync(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

    public async Task<T> UseAsync<T>(IAgentSession owner, string? serial, bool takeOver,
        Func<CancellationToken, Task<IReadOnlyList<AdbDevice>>> list,
        Func<string, CancellationToken, Task<(IDeviceBackend Backend, string? Fallback)>> create,
        AndroidSettings settings, Func<DeviceSession, Task<T>> action, CancellationToken ct)
    {
        await gate.WaitAsync(ct);
        try
        {
            var existing = devices.Values.FirstOrDefault(s => ReferenceEquals(s.Owner, owner));
            serial ??= existing?.Serial;
            if (serial is null)
            {
                var available = await list(ct);
                var free = available.Where(d => d.State == "device" && !devices.ContainsKey(d.Serial)).ToArray();
                if (free.Length != 1) throw new InvalidOperationException("Choose a serial with android_connect. Devices: " +
                    string.Join(", ", available.Select(d => $"{d.Serial} ({d.State}{(d.State == "unauthorized" ? "; accept USB debugging on the phone" : "")})")));
                serial = free[0].Serial;
            }
            devices.TryGetValue(serial, out var session);
            if (session is not null && !ReferenceEquals(session.Owner, owner))
            {
                if (!takeOver) throw new InvalidOperationException($"Device {serial} is leased by {session.OwnerLabel}. Use take_over only when explicitly requested.");
                await session.Backend.DisposeAsync(); devices.Remove(serial); session = null;
            }
            if (session is null)
            {
                var available = await list(ct);
                if (!available.Any(d => d.Serial == serial && d.State == "device"))
                    throw new InvalidOperationException($"Device {serial} is unavailable or unauthorized. Check android_devices and accept USB debugging on the phone.");
                var lease = AcquireHostLease(serial, owner);
                try
                {
                    var backend = await create(serial, ct);
                    session = new(serial, owner, new LeasedBackend(backend.Backend, lease), backend.Fallback, settings);
                }
                catch { lease.Dispose(); throw; }
                if (existing is not null && existing.Serial != serial)
                { await existing.Backend.DisposeAsync(); devices.Remove(existing.Serial); }
                devices[serial] = session;
            }
            else if (session.Backend.IsBroken)
            {
                var leased = (LeasedBackend)session.Backend;
                await leased.Inner.DisposeAsync();
                try
                {
                    var replacement = await create(serial, ct);
                    leased.Inner = replacement.Backend; session.FallbackReason = replacement.Fallback;
                    session.Refs.Clear();
                }
                catch { devices.Remove(serial); leased.ReleaseLease(); throw; }
            }
            session.LastUsedUtc = DateTimeOffset.UtcNow;
            try { return await action(session); }
            finally { session.LastUsedUtc = DateTimeOffset.UtcNow; }
        }
        finally { gate.Release(); }
    }

    public async Task ReleaseAsync(IAgentSession owner, CancellationToken ct)
    {
        await gate.WaitAsync(ct);
        try
        {
            foreach (var session in devices.Values.Where(s => ReferenceEquals(s.Owner, owner)).ToArray())
            { devices.Remove(session.Serial); await session.Backend.DisposeAsync(); }
        }
        finally { gate.Release(); }
    }

    internal async Task ReapAsync(DateTimeOffset now)
    {
        await gate.WaitAsync();
        try
        {
            foreach (var session in devices.Values.Where(s => now - s.LastUsedUtc > TimeSpan.FromMinutes(s.Settings.IdleDisconnectMinutes)).ToArray())
            { devices.Remove(session.Serial); await session.Backend.DisposeAsync(); }
        }
        finally { gate.Release(); }
    }
    private async Task ReapSafelyAsync() { try { await ReapAsync(DateTimeOffset.UtcNow); } catch (Exception) { /* Retry on the next tick. */ } }

    private static FileStream AcquireHostLease(string serial, IAgentSession owner)
    {
        // Static fields are isolated by PluginLoadContext. An OS-held file also excludes other
        // project load contexts and other SPLA processes, and is released automatically on death.
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SPLA", "runtime", "android", "leases");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(serial))) + ".lease");
        FileStream lease;
        try { lease = new(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read); }
        catch (IOException)
        {
            string label;
            try { using var reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)); label = reader.ReadToEnd(); }
            catch (IOException) { label = "another SPLA project/process"; }
            throw new InvalidOperationException($"Device {serial} is leased by {label}. Disconnect it in that project/process first.");
        }
        try
        {
            lease.SetLength(0);
            lease.Write(Encoding.UTF8.GetBytes($"{owner.ChatId ?? "CLI/worker session"} (process {Environment.ProcessId})"));
            lease.Flush(); return lease;
        }
        catch { lease.Dispose(); throw; }
    }

    private sealed class LeasedBackend(IDeviceBackend inner, FileStream lease) : IDeviceBackend
    {
        public IDeviceBackend Inner { get; set; } = inner;
        public string Kind => Inner.Kind;
        public (int Width, int Height) ScreenSize => Inner.ScreenSize;
        public bool SupportsMultiTouch => Inner.SupportsMultiTouch;
        public bool IsBroken => Inner.IsBroken;
        public Task<Screenshot> CaptureAsync(bool waitStable, CancellationToken ct) => Inner.CaptureAsync(waitStable, ct);
        public Task PlayAsync(Gesture gesture, CancellationToken ct) => Inner.PlayAsync(gesture, ct);
        public Task KeyAsync(int keycode, bool longPress, CancellationToken ct) => Inner.KeyAsync(keycode, longPress, ct);
        public Task<TextEntryResult> TypeTextAsync(string text, CancellationToken ct) => Inner.TypeTextAsync(text, ct);
        public void ReleaseLease() => lease.Dispose();
        public async ValueTask DisposeAsync() { try { await Inner.DisposeAsync(); } finally { lease.Dispose(); } }
    }
}
