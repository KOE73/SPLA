namespace SPLA.Plugins.Android.Video;

internal static class YuvToRgb
{
    public static byte[] ToRgb24(YuvFrame frame)
    {
        var rgb = new byte[checked(frame.Width * frame.Height * 3)];
        for (var y = 0; y < frame.Height; y++)
        for (var x = 0; x < frame.Width; x++)
        {
            var chroma = (y / 2) * frame.StrideUV + (x / 2) * (frame.Nv12 ? 2 : 1);
            var d = frame.U[chroma] - 128; var e = (frame.Nv12 ? frame.U[chroma + 1] : frame.V[chroma]) - 128;
            var luma = frame.Y[y * frame.StrideY + x];
            var c = frame.FullRange ? 256 * luma : 298 * (luma - 16);
            var offset = (y * frame.Width + x) * 3;
            rgb[offset] = (byte)Math.Clamp((c + (frame.FullRange ? 359 : 409) * e + 128) >> 8, 0, 255);
            rgb[offset + 1] = (byte)Math.Clamp((c - (frame.FullRange ? 88 : 100) * d - (frame.FullRange ? 183 : 208) * e + 128) >> 8, 0, 255);
            rgb[offset + 2] = (byte)Math.Clamp((c + (frame.FullRange ? 454 : 516) * d + 128) >> 8, 0, 255);
        }
        return rgb;
    }
}
