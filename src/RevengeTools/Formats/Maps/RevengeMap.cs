using RevengeTools.Binary;

namespace RevengeTools.Formats.Maps;

public sealed class RevengeMap
{
    private readonly byte[] _duplicatedTilePrefix;
    private readonly byte[] _tileIds;

    private RevengeMap(int width, int height, byte[] duplicatedTilePrefix, byte[] tileIds)
    {
        Width = width;
        Height = height;
        _duplicatedTilePrefix = duplicatedTilePrefix;
        _tileIds = tileIds;
    }

    public const int HeaderLength = 12;
    public int Width { get; }
    public int Height { get; }
    public byte[] DuplicatedTilePrefix => (byte[])_duplicatedTilePrefix.Clone();
    public byte[] TileIds => (byte[])_tileIds.Clone();

    public static RevengeMap Parse(byte[] data, string sourceName = "MAP file")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireRange(0, HeaderLength, "MAP header");
        int width = reader.ReadUInt16LittleEndian(0, "map width");
        int height = reader.ReadUInt16LittleEndian(2, "map height");
        if (width <= 0 || height <= 0)
            throw new InvalidDataException($"{sourceName} has invalid dimensions {width}x{height}.");
        int tileCount = checked(width * height);
        if (tileCount < 8)
            throw new InvalidDataException($"{sourceName} has only {tileCount} cells; the format requires an eight-byte duplicated tile prefix.");
        if (data.Length != HeaderLength + tileCount)
            throw new InvalidDataException($"{sourceName} is 0x{data.Length:X} bytes; expected 0x{HeaderLength + tileCount:X} for {width}x{height}.");
        byte[] duplicatedTilePrefix = reader.ReadBytes(4, 8, "duplicated tile prefix");
        byte[] tileIds = reader.ReadBytes(HeaderLength, tileCount, "map tile IDs");
        if (!duplicatedTilePrefix.SequenceEqual(tileIds.AsSpan(0, 8).ToArray()))
            throw new InvalidDataException($"{sourceName} header prefix does not duplicate its first eight tile IDs.");
        return new RevengeMap(width, height, duplicatedTilePrefix, tileIds);
    }
}
