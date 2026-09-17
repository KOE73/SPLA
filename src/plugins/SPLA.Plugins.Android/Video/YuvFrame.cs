namespace SPLA.Plugins.Android.Video;

internal sealed record YuvFrame(int Width, int Height, byte[] Y, byte[] U, byte[] V,
    int StrideY, int StrideUV, bool Nv12, long Sequence, bool FullRange = false);
