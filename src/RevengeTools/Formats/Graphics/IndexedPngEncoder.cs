using System.IO.Compression;
using System.Text;

namespace RevengeTools.Formats.Graphics;

/// <summary>Small dependency-free indexed PNG encoder for deterministic exports.</summary>
public static class IndexedPngEncoder
{
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    public static byte[] Encode(int width, int height, byte[] pixels, byte[] paletteRgb)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        ArgumentNullException.ThrowIfNull(pixels);
        ArgumentNullException.ThrowIfNull(paletteRgb);
        if (pixels.Length != checked(width * height))
            throw new ArgumentException("Pixel count does not match image dimensions.", nameof(pixels));
        if (paletteRgb.Length < 3 || paletteRgb.Length > 256 * 3 || paletteRgb.Length % 3 != 0)
            throw new ArgumentException("Palette must contain 1 through 256 RGB triplets.", nameof(paletteRgb));
        int paletteCount = paletteRgb.Length / 3;
        if (pixels.Any(pixel => pixel >= paletteCount))
            throw new InvalidDataException("Image contains a palette index outside the supplied palette.");

        byte[] scanlines = new byte[(width + 1) * height];
        for (int y = 0; y < height; y++)
        {
            int target = y * (width + 1);
            scanlines[target] = 0;
            Array.Copy(pixels, y * width, scanlines, target + 1, width);
        }

        byte[] compressed;
        using (var buffer = new MemoryStream())
        {
            using (var zlib = new ZLibStream(buffer, CompressionLevel.Optimal, true))
                zlib.Write(scanlines);
            compressed = buffer.ToArray();
        }

        using var png = new MemoryStream();
        png.Write(Signature);
        byte[] header = new byte[13];
        WriteBigEndian(header, 0, (uint)width);
        WriteBigEndian(header, 4, (uint)height);
        header[8] = 8;
        header[9] = 3;
        WriteChunk(png, "IHDR", header);
        WriteChunk(png, "PLTE", paletteRgb);
        WriteChunk(png, "IDAT", compressed);
        WriteChunk(png, "IEND", Array.Empty<byte>());
        return png.ToArray();
    }

    private static void WriteChunk(Stream output, string type, byte[] data)
    {
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);
        byte[] length = new byte[4];
        WriteBigEndian(length, 0, (uint)data.Length);
        output.Write(length);
        output.Write(typeBytes);
        output.Write(data);
        uint crc = 0xFFFFFFFF;
        foreach (byte value in typeBytes) crc = UpdateCrc(crc, value);
        foreach (byte value in data) crc = UpdateCrc(crc, value);
        byte[] checksum = new byte[4];
        WriteBigEndian(checksum, 0, crc ^ 0xFFFFFFFF);
        output.Write(checksum);
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (int bit = 0; bit < 8; bit++)
            crc = (crc & 1) != 0 ? 0xEDB88320U ^ crc >> 1 : crc >> 1;
        return crc;
    }

    private static void WriteBigEndian(byte[] destination, int offset, uint value)
    {
        destination[offset] = (byte)(value >> 24);
        destination[offset + 1] = (byte)(value >> 16);
        destination[offset + 2] = (byte)(value >> 8);
        destination[offset + 3] = (byte)value;
    }
}
