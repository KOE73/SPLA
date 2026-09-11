using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace SPLA.Plugins.Android.Video;

internal static class PngEncoder
{
    public static byte[] Encode(byte[] rgb, int width, int height)
    {
        if (width <= 0 || height <= 0 || rgb.Length != checked(width * height * 3)) throw new ArgumentException("Invalid RGB frame.");
        using var output = new MemoryStream();
        output.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, width); BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
        header[8] = 8; header[9] = 2;
        Chunk(output, "IHDR", header);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
        {
            var stride = width * 3; var row = new byte[stride + 1]; row[0] = 1;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < stride; x++) row[x + 1] = unchecked((byte)(rgb[y * stride + x] - (x >= 3 ? rgb[y * stride + x - 3] : 0)));
                zlib.Write(row);
            }
        }
        Chunk(output, "IDAT", compressed.ToArray()); Chunk(output, "IEND", []);
        return output.ToArray();
    }

    private static void Chunk(Stream output, string name, byte[] data)
    {
        Span<byte> number = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(number, data.Length); output.Write(number);
        var type = Encoding.ASCII.GetBytes(name); output.Write(type); output.Write(data);
        uint crc = 0xFFFFFFFF;
        foreach (var value in type.Concat(data))
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xEDB88320u : 0);
        }
        BinaryPrimitives.WriteUInt32BigEndian(number, crc ^ 0xFFFFFFFF); output.Write(number);
    }
}
