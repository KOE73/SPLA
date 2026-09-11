using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace SPLA.Plugins.Android.Video;

internal sealed unsafe class FfmpegNative
{
    private static readonly ConcurrentDictionary<string, Lazy<FfmpegNative>> Libraries = new(StringComparer.OrdinalIgnoreCase);
    public static FfmpegNative Load(string folder) => Libraries.GetOrAdd(Path.GetFullPath(folder),
        path => new Lazy<FfmpegNative>(() => new(path))).Value;
    private readonly List<nint> handles = [];
    public readonly delegate* unmanaged[Cdecl]<uint> CodecVersion;
    public readonly delegate* unmanaged[Cdecl]<int, nint> FindDecoder;
    public readonly delegate* unmanaged[Cdecl]<nint, nint> AllocateContext;
    public readonly delegate* unmanaged[Cdecl]<nint, nint, nint, int> Open;
    public readonly delegate* unmanaged[Cdecl]<nint> AllocatePacket;
    public readonly delegate* unmanaged[Cdecl]<nint, int, int> NewPacket;
    public readonly delegate* unmanaged[Cdecl]<nint, void> UnrefPacket;
    public readonly delegate* unmanaged[Cdecl]<nint*, void> FreePacket;
    public readonly delegate* unmanaged[Cdecl]<nint> AllocateFrame;
    public readonly delegate* unmanaged[Cdecl]<nint, void> UnrefFrame;
    public readonly delegate* unmanaged[Cdecl]<nint*, void> FreeFrame;
    public readonly delegate* unmanaged[Cdecl]<nint, nint, int> SendPacket;
    public readonly delegate* unmanaged[Cdecl]<nint, nint, int> ReceiveFrame;
    public readonly delegate* unmanaged[Cdecl]<nint*, void> FreeContext;

    private FfmpegNative(string folder)
    {
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("Android runtime requires Windows x64.");
        try
        {
            nint LoadFile(string pattern)
            {
                var file = Directory.GetFiles(folder, pattern).Single();
                var handle = NativeLibrary.Load(file); handles.Add(handle); return handle;
            }
            var util = LoadFile("avutil-*.dll");
            // avcodec may import swresample; preload it without changing the process DLL search path.
            if (Directory.GetFiles(folder, "swresample-*.dll").Length != 0) LoadFile("swresample-*.dll");
            var codec = LoadFile("avcodec-*.dll");
            nint C(string name) => NativeLibrary.GetExport(codec, name);
            nint U(string name) => NativeLibrary.GetExport(util, name);
            CodecVersion = (delegate* unmanaged[Cdecl]<uint>)C("avcodec_version");
            if (CodecVersion() >> 16 is < 59 or > 62) throw new NotSupportedException("Unsupported FFmpeg ABI; use the pinned scrcpy runtime.");
            FindDecoder = (delegate* unmanaged[Cdecl]<int, nint>)C("avcodec_find_decoder");
            AllocateContext = (delegate* unmanaged[Cdecl]<nint, nint>)C("avcodec_alloc_context3");
            Open = (delegate* unmanaged[Cdecl]<nint, nint, nint, int>)C("avcodec_open2");
            AllocatePacket = (delegate* unmanaged[Cdecl]<nint>)C("av_packet_alloc");
            NewPacket = (delegate* unmanaged[Cdecl]<nint, int, int>)C("av_new_packet");
            UnrefPacket = (delegate* unmanaged[Cdecl]<nint, void>)C("av_packet_unref");
            FreePacket = (delegate* unmanaged[Cdecl]<nint*, void>)C("av_packet_free");
            AllocateFrame = (delegate* unmanaged[Cdecl]<nint>)U("av_frame_alloc");
            UnrefFrame = (delegate* unmanaged[Cdecl]<nint, void>)U("av_frame_unref");
            FreeFrame = (delegate* unmanaged[Cdecl]<nint*, void>)U("av_frame_free");
            SendPacket = (delegate* unmanaged[Cdecl]<nint, nint, int>)C("avcodec_send_packet");
            ReceiveFrame = (delegate* unmanaged[Cdecl]<nint, nint, int>)C("avcodec_receive_frame");
            FreeContext = (delegate* unmanaged[Cdecl]<nint*, void>)C("avcodec_free_context");
        }
        catch
        {
            foreach (var handle in handles.AsEnumerable().Reverse()) NativeLibrary.Free(handle);
            throw;
        }
    }
}
