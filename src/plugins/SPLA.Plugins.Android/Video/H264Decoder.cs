using System.Runtime.InteropServices;
using SPLA.Plugins.Android.Scrcpy;

namespace SPLA.Plugins.Android.Video;

internal sealed unsafe class H264Decoder : IDisposable
{
    private readonly FfmpegNative native;
    private nint context, packet, frame;
    private long sequence;
    public H264Decoder(string folder)
    {
        native = FfmpegNative.Load(folder);
        try
        {
            var codec = native.FindDecoder(27);
            if (codec == 0) throw new IOException("FFmpeg H.264 decoder is unavailable.");
            context = native.AllocateContext(codec);
            packet = native.AllocatePacket(); frame = native.AllocateFrame();
            if (context == 0 || packet == 0 || frame == 0) throw new OutOfMemoryException();
            Check(native.Open(context, codec, 0));
        }
        catch { Dispose(); throw; }
    }

    public IReadOnlyList<YuvFrame> Decode(VideoPacket data)
    {
        ObjectDisposedException.ThrowIf(context == 0, this);
        List<YuvFrame> output = [];
        Check(native.NewPacket(packet, data.Data.Length));
        try
        {
            Marshal.Copy(data.Data, 0, *(nint*)((byte*)packet + 24), data.Data.Length);
            *(int*)((byte*)packet + 40) = data.KeyFrame ? 1 : 0;
            var result = native.SendPacket(context, packet);
            if (result == -11) { Receive(output); result = native.SendPacket(context, packet); }
            Check(result); Receive(output);
        }
        finally { native.UnrefPacket(packet); }
        return output;
    }

    private void Receive(List<YuvFrame> output)
    {
        while (true)
        {
            var result = native.ReceiveFrame(context, frame);
            if (result is -11 or -0x20464F45) return;
            Check(result);
            try
            {
                var width = *(int*)((byte*)frame + 104); var height = *(int*)((byte*)frame + 108);
                var format = *(int*)((byte*)frame + 116);
                if (width is < 1 or > 16384 || height is < 1 or > 16384 || (long)width * height > 40_000_000)
                    throw new IOException("Invalid decoded frame dimensions.");
                if (format is not (0 or 12 or 23)) throw new IOException($"Unsupported pixel format {format}.");
                var chromaWidth = (width + 1) / 2; var chromaHeight = (height + 1) / 2;
                var nv12 = format == 23;
                byte[] CopyPlane(int index, int rowSize, int rows)
                {
                    var source = *(nint*)((byte*)frame + index * 8);
                    var stride = *(int*)((byte*)frame + 64 + index * 4);
                    if (source == 0 || Math.Abs((long)stride) < rowSize) throw new IOException("Invalid decoded plane.");
                    var bytes = new byte[checked(rowSize * rows)];
                    for (var row = 0; row < rows; row++) Marshal.Copy(source + row * stride, bytes, row * rowSize, rowSize);
                    return bytes;
                }
                output.Add(new(width, height, CopyPlane(0, width, height), CopyPlane(1, nv12 ? chromaWidth * 2 : chromaWidth, chromaHeight),
                    nv12 ? [] : CopyPlane(2, chromaWidth, chromaHeight), width, nv12 ? chromaWidth * 2 : chromaWidth, nv12, ++sequence, format == 12));
            }
            finally { native.UnrefFrame(frame); }
        }
    }

    internal IReadOnlyList<YuvFrame> Flush()
    {
        ObjectDisposedException.ThrowIf(context == 0, this);
        List<YuvFrame> output = [];
        Check(native.SendPacket(context, 0)); Receive(output); return output;
    }

    private static void Check(int result) { if (result < 0) throw new IOException($"FFmpeg error {result}."); }
    public void Dispose()
    {
        var oldPacket = packet; packet = 0; if (oldPacket != 0) native.FreePacket(&oldPacket);
        var oldFrame = frame; frame = 0; if (oldFrame != 0) native.FreeFrame(&oldFrame);
        var oldContext = context; context = 0; if (oldContext != 0) native.FreeContext(&oldContext);
    }
}
