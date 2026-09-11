using System.Buffers.Binary;
using System.Text;

namespace SPLA.Plugins.Android.Scrcpy;

internal static class ControlMessages
{
    public static byte[] Key(byte action, int keycode, int repeat = 0, int meta = 0)
    {
        var data = new byte[14]; data[1] = action;
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(2), keycode);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(6), repeat);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(10), meta);
        return data;
    }
    public static byte[] Text(string text)
    {
        var utf8 = Encoding.UTF8.GetBytes(text);
        if (utf8.Length > 300) throw new ArgumentException("Text message exceeds 300 UTF-8 bytes.");
        var data = new byte[5 + utf8.Length]; data[0] = 1;
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(1), utf8.Length);
        utf8.CopyTo(data, 5); return data;
    }
    public static byte[] Touch(byte action, long pointerId, int x, int y, int width, int height, double pressure)
    {
        var data = new byte[32]; data[0] = 2; data[1] = action;
        BinaryPrimitives.WriteInt64BigEndian(data.AsSpan(2), pointerId);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(10), x);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(14), y);
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(18), checked((ushort)width));
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(20), checked((ushort)height));
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(22), (ushort)Math.Round(Math.Clamp(pressure, 0, 1) * 65535));
        return data;
    }
    public static byte[] SetClipboard(string text, bool paste)
    {
        var utf8 = Encoding.UTF8.GetBytes(text);
        if (utf8.Length > 256 * 1024 - 14) throw new ArgumentException("Clipboard text exceeds the scrcpy message limit.");
        var data = new byte[14 + utf8.Length]; data[0] = 9; data[9] = paste ? (byte)1 : (byte)0;
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(10), utf8.Length);
        utf8.CopyTo(data, 14); return data;
    }
}
