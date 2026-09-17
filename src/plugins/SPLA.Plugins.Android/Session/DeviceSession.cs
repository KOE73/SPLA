using SPLA.Domain.Agent;
using SPLA.Plugins.Android.Ui;

namespace SPLA.Plugins.Android.Session;

internal sealed class DeviceSession(string serial, IAgentSession owner, IDeviceBackend backend, string? fallback, AndroidSettings settings)
{
    public string Serial { get; } = serial;
    public IAgentSession Owner { get; } = owner;
    public string OwnerLabel => Owner.ChatId ?? "CLI/worker session";
    public IDeviceBackend Backend { get; set; } = backend;
    public string? FallbackReason { get; set; } = fallback;
    public AndroidSettings Settings { get; } = settings;
    public DateTimeOffset LastUsedUtc { get; set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, UiElement> Refs { get; set; } = [];
    public (int Width, int Height) RefSize { get; set; }
    public (int Width, int Height)? LastShownSize { get; set; }
}
