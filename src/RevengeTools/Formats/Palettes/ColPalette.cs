using RevengeTools.Binary;
using RevengeTools.Formats.Graphics;

namespace RevengeTools.Formats.Palettes;

public sealed class ColPalette
{
    private readonly byte[] _components6Bit;
    private readonly byte[] _rgb;

    private ColPalette(byte[] components6Bit, byte[] rgb)
    {
        _components6Bit = components6Bit;
        _rgb = rgb;
    }

    public int ColorCount => 256;
    public byte MaximumSourceComponent => _components6Bit.Max();
    public byte[] Components6Bit => (byte[])_components6Bit.Clone();
    public byte[] Rgb => (byte[])_rgb.Clone();

    public static ColPalette Parse(byte[] data, string sourceName = "COL palette")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireExactLength(768, "COL palette");
        byte[] source = reader.ReadBytes(0, 768);
        int invalid = Array.FindIndex(source, value => value > 63);
        if (invalid >= 0)
            throw new InvalidDataException($"{sourceName} component at 0x{invalid:X} is {source[invalid]}; expected 0..63.");
        byte[] rgb = source.Select(value => (byte)((value * 255 + 31) / 63)).ToArray();
        return new ColPalette(source, rgb);
    }

    public byte[] RenderSwatches(int cellSize = 16)
    {
        if (cellSize <= 0) throw new ArgumentOutOfRangeException(nameof(cellSize));
        int width = 16 * cellSize;
        int height = 16 * cellSize;
        byte[] pixels = new byte[width * height];
        for (int color = 0; color < 256; color++)
        {
            int left = color % 16 * cellSize;
            int top = color / 16 * cellSize;
            for (int y = 0; y < cellSize; y++)
                Array.Fill(pixels, (byte)color, (top + y) * width + left, cellSize);
        }
        return IndexedPngEncoder.Encode(width, height, pixels, _rgb);
    }
}
