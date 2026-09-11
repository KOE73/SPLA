using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace SPLA.Plugins.Android.Scrcpy;

internal abstract record DemuxItem;
internal sealed record SessionInfo(int Width, int Height) : DemuxItem;
internal sealed record VideoPacket(byte[] Data, long Pts, bool KeyFrame) : DemuxItem;

internal static class VideoDemuxer
{
    public static async IAsyncEnumerable<DemuxItem> ReadAsync(Stream video, [EnumeratorCancellation] CancellationToken ct)
    {
        var header = new byte[12]; byte[] config = [];
        while (true)
        {
            var first = await video.ReadAsync(header.AsMemory(0, 1), ct);
            if (first == 0) yield break;
            await video.ReadExactlyAsync(header.AsMemory(1), ct);
            if ((header[0] & 0x80) != 0)
            {
                var width = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(4));
                var height = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(8));
                if (width is < 1 or > 16384 || height is < 1 or > 16384 || (long)width * height > 40_000_000)
                    throw new IOException("Invalid scrcpy video dimensions.");
                config = [];
                yield return new SessionInfo(width, height); continue;
            }
            var flags = BinaryPrimitives.ReadUInt64BigEndian(header);
            var length = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(8));
            if (length is < 1 or > 32 * 1024 * 1024) throw new IOException("Invalid scrcpy packet length.");
            var payload = new byte[length]; await video.ReadExactlyAsync(payload, ct);
            if ((flags & (1UL << 62)) != 0)
            {
                if (config.Length + length > 1024 * 1024) throw new IOException("Codec configuration exceeds limit.");
                config = [.. config, .. payload]; continue;
            }
            yield return new VideoPacket(config.Length == 0 ? payload : [.. config, .. payload], (long)(flags & ((1UL << 61) - 1)), (flags & (1UL << 61)) != 0);
            config = [];
        }
    }
}
