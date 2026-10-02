using RevengeTools.Binary;
using RevengeTools.Formats.Graphics;

namespace RevengeTools.Formats.Fonts;

public sealed class WestwoodFontV2
{
    private readonly byte[][] _glyphs;

    private WestwoodFontV2(int declaredSize, int width, int height, ushort[] offsets, byte[][] glyphs)
    {
        DeclaredSize = declaredSize;
        Width = width;
        Height = height;
        Offsets = Array.AsReadOnly(offsets);
        _glyphs = glyphs;
    }

    public int DeclaredSize { get; }
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyList<ushort> Offsets { get; }
    public int GlyphCount => 128;

    public byte[] GetGlyphRows(int character) => (byte[])_glyphs[ValidateCharacter(character)].Clone();

    public static WestwoodFontV2 Parse(byte[] data, string sourceName = "Westwood Font v2")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireRange(0, 0x104, "font header");
        int declared = reader.ReadUInt16LittleEndian(0, "declared file size");
        if (declared != data.Length - 2)
            throw new InvalidDataException($"{sourceName} declares 0x{declared:X} bytes after its size word; expected 0x{data.Length - 2:X}.");
        int height = reader.ReadByte(0x102, "glyph height");
        int width = reader.ReadByte(0x103, "glyph width");
        if (height <= 0 || width is <= 0 or > 8)
            throw new InvalidDataException($"{sourceName} has invalid {width}x{height} glyph dimensions.");

        ushort[] offsets = new ushort[128];
        byte[][] glyphs = new byte[128][];
        for (int index = 0; index < offsets.Length; index++)
        {
            offsets[index] = reader.ReadUInt16LittleEndian(2 + index * 2, $"glyph {index} offset");
            glyphs[index] = reader.ReadBytes(offsets[index], height, $"glyph {index} bitmap");
        }
        return new WestwoodFontV2(declared, width, height, offsets, glyphs);
    }

    public byte[] RenderContactSheet(int scale = 2)
    {
        if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale));
        int cellWidth = (Width + 2) * scale;
        int cellHeight = (Height + 2) * scale;
        int imageWidth = 16 * cellWidth;
        int imageHeight = 8 * cellHeight;
        byte[] pixels = new byte[imageWidth * imageHeight];
        for (int character = 0; character < 128; character++)
        {
            int originX = character % 16 * cellWidth + scale;
            int originY = character / 16 * cellHeight + scale;
            byte[] rows = _glyphs[character];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if ((rows[y] & (0x80 >> x)) == 0) continue;
                    for (int sy = 0; sy < scale; sy++)
                        Array.Fill(pixels, (byte)1, (originY + y * scale + sy) * imageWidth + originX + x * scale, scale);
                }
            }
        }
        return IndexedPngEncoder.Encode(imageWidth, imageHeight, pixels,
            new byte[] { 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF });
    }

    private static int ValidateCharacter(int character)
    {
        if (character is < 0 or >= 128) throw new ArgumentOutOfRangeException(nameof(character));
        return character;
    }
}
