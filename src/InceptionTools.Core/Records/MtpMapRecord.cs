using System;
using System.IO;
using InceptionTools.Binary;

namespace InceptionTools.Records;

public sealed class MtpMapRecord
{
    private readonly byte[] _headerBytes;
    private readonly byte[] _npcNameBytes;
    private readonly byte[] _buildingNameBytes;
    private readonly byte[] _interactionXBytes;
    private readonly byte[] _interactionYBytes;
    private readonly byte[] _characterXBytes;
    private readonly byte[] _characterYBytes;
    private readonly byte[] _alternateBldBytes;
    private readonly byte[] _mapStateBytes;
    private readonly byte[] _tileIds;

    private MtpMapRecord(string sourceName, bool hasStandardHeader, int width, int height,
        byte[] headerBytes, byte[] npcNameBytes, byte[] buildingNameBytes,
        byte[] interactionXBytes, byte[] interactionYBytes, byte[] characterXBytes,
        byte[] characterYBytes, byte[] alternateBldBytes, byte[] mapStateBytes, byte[] tileIds)
    {
        SourceName = sourceName;
        HasStandardHeader = hasStandardHeader;
        Width = width;
        Height = height;
        _headerBytes = Clone(headerBytes);
        _npcNameBytes = Clone(npcNameBytes);
        _buildingNameBytes = Clone(buildingNameBytes);
        _interactionXBytes = Clone(interactionXBytes);
        _interactionYBytes = Clone(interactionYBytes);
        _characterXBytes = Clone(characterXBytes);
        _characterYBytes = Clone(characterYBytes);
        _alternateBldBytes = Clone(alternateBldBytes);
        _mapStateBytes = Clone(mapStateBytes);
        _tileIds = Clone(tileIds);
    }

    public const int StandardHeaderLength = 0x21D;
    public string SourceName { get; }
    public bool HasStandardHeader { get; }
    public int Width { get; }
    public int Height { get; }
    public byte[] HeaderBytes => Clone(_headerBytes);
    public byte[] NpcNameBytes => Clone(_npcNameBytes);
    public byte[] BuildingNameBytes => Clone(_buildingNameBytes);
    public byte[] InteractionXBytes => Clone(_interactionXBytes);
    public byte[] InteractionYBytes => Clone(_interactionYBytes);
    public byte[] CharacterXBytes => Clone(_characterXBytes);
    public byte[] CharacterYBytes => Clone(_characterYBytes);
    public byte[] AlternateBldBytes => Clone(_alternateBldBytes);
    public byte[] MapStateBytes => Clone(_mapStateBytes);
    public byte[] TileIds => Clone(_tileIds);

    public static MtpMapRecord ParseStandard(byte[] data, string sourceName = "MTP map")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireRange(0, StandardHeaderLength, "standard MTP header");
        int width = reader.ReadByte(3, "map width");
        int height = reader.ReadByte(4, "map height");
        if (width <= 0 || height <= 0)
            throw new InvalidDataException(sourceName + " has zero map dimensions.");
        int tileCount = checked(width * height);
        reader.RequireExactLength(checked(StandardHeaderLength + tileCount), "standard MTP map");

        return new MtpMapRecord(sourceName, true, width, height,
            reader.ReadBytes(0x000, 0x003, "map header bytes"),
            reader.ReadBytes(0x005, 0x080, "probable NPC-name block"),
            reader.ReadBytes(0x085, 0x100, "probable building-name block"),
            reader.ReadBytes(0x185, 0x020, "interaction X block"),
            reader.ReadBytes(0x1A5, 0x020, "interaction Y block"),
            reader.ReadBytes(0x1C5, 0x020, "character X block"),
            reader.ReadBytes(0x1E5, 0x020, "character Y block"),
            reader.ReadBytes(0x205, 0x010, "alternate BLD block"),
            reader.ReadBytes(0x215, 0x008, "map-state block"),
            reader.ReadBytes(StandardHeaderLength, tileCount, "map tile IDs"));
    }

    public static MtpMapRecord ParseStarMap(byte[] data, string sourceName = "MAP15.MTP")
    {
        const int width = 32;
        const int height = 24;
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireExactLength(width * height, "raw star map");
        return new MtpMapRecord(sourceName, false, width, height,
            Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>(),
            Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>(),
            Array.Empty<byte>(), reader.ReadBytes(0, reader.Length, "star-map tile IDs"));
    }

    private static byte[] Clone(byte[] value) => value == null ? Array.Empty<byte>() : (byte[])value.Clone();

}
