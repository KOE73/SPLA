using SPLA.Plugins.Android.Runtime;
using SPLA.Plugins.Android.Scrcpy;
using SPLA.Plugins.Android.Video;

namespace SPLA.Tests;

public sealed class AndroidDecoderTests
{
    public sealed class NativeRuntimeFactAttribute : FactAttribute
    {
        public NativeRuntimeFactAttribute()
        {
            var folder=RuntimePaths.DefaultFolder("scrcpy");
            if (!Directory.Exists(folder) || !Directory.EnumerateFiles(folder,"avcodec-*.dll").Any())
                Skip="Install the scrcpy runtime to exercise the native FFmpeg boundary.";
        }
    }

    [NativeRuntimeFact]
    public void Shipped_ffmpeg_loads_and_allocates_a_real_h264_decoder()
    {
        using var decoder=new H264Decoder(RuntimePaths.DefaultFolder("scrcpy"));
    }

    public sealed class DecoderFixtureFactAttribute : FactAttribute
    {
        public DecoderFixtureFactAttribute()
        {
            if (!File.Exists(Path.Combine(AppContext.BaseDirectory,"Android","Data","tiny.h264")))
                Skip="The plan requires an owner-supplied five-frame 64x64 H.264 fixture (Annex B with AUD). No fixture was supplied.";
            else if (!Directory.Exists(RuntimePaths.DefaultFolder("scrcpy"))) Skip="Install the scrcpy runtime first.";
        }
    }

    [DecoderFixtureFact]
    public void Owner_fixture_decodes_five_frames()
    {
        var bytes=File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"Android","Data","tiny.h264"));
        List<int> boundaries=[];
        for(var offset=0;offset+4<bytes.Length;offset++)
        {
            var prefix=bytes.AsSpan(offset).StartsWith(new byte[]{0,0,0,1})?4:bytes.AsSpan(offset).StartsWith(new byte[]{0,0,1})?3:0;
            if(prefix==0) continue;
            if((bytes[offset+prefix]&31)==9) boundaries.Add(offset);
            offset+=prefix-1;
        }
        Assert.Equal(5,boundaries.Count);
        boundaries[0]=0; boundaries.Add(bytes.Length);
        using var decoder=new H264Decoder(RuntimePaths.DefaultFolder("scrcpy"));
        List<YuvFrame> frames=[];
        for(var index=0;index<5;index++) frames.AddRange(decoder.Decode(new VideoPacket(bytes[boundaries[index]..boundaries[index+1]],index,index==0)));
        frames.AddRange(decoder.Flush());
        Assert.Equal(5,frames.Count); Assert.All(frames,frame=>{Assert.Equal(64,frame.Width);Assert.Equal(64,frame.Height);});
    }
}
