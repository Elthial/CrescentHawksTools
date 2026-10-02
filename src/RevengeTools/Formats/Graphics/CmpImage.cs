using RevengeTools.Binary;

namespace RevengeTools.Formats.Graphics;

/// <summary>
/// Older three-byte Westwood CMP container used by BOOMS.CMP. Pixels are
/// stored as two four-bit palette indices per decompressed byte.
/// </summary>
public sealed class CmpImage
{
    private readonly byte[] _packedBytes;
    private readonly byte[] _pixels;

    private CmpImage(int storedLength, int compressionType, int consumedPayloadBytes,
        int payloadLength, int trailingBytes, byte[] packedBytes, byte[] pixels)
    {
        StoredLength = storedLength;
        CompressionType = compressionType;
        ConsumedPayloadBytes = consumedPayloadBytes;
        PayloadLength = payloadLength;
        TrailingBytes = trailingBytes;
        _packedBytes = packedBytes;
        _pixels = pixels;
    }

    public const int Width = 320;
    public const int Height = 200;
    public const int PackedLength = Width * Height / 2;
    public const int HeaderLength = 3;

    public int StoredLength { get; }
    public int CompressionType { get; }
    public string CompressionName => CompressionType switch
    {
        1 => "Westwood horizontal RLE",
        2 => "Westwood vertical RLE",
        _ => "unknown"
    };
    public int ConsumedPayloadBytes { get; }
    public int PayloadLength { get; }
    public int RemainingPayloadBytes => PayloadLength - ConsumedPayloadBytes;
    public int TrailingBytes { get; }
    public byte[] PackedBytes => (byte[])_packedBytes.Clone();
    public byte[] Pixels => (byte[])_pixels.Clone();

    public static CmpImage Decode(byte[] data, string sourceName = "CMP image")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireRange(0, HeaderLength, "CMP header");
        int storedLength = reader.ReadUInt16LittleEndian(0, "stored length");
        int declaredEnd = checked(storedLength + 2);
        if (declaredEnd > data.Length)
            throw new InvalidDataException($"{sourceName} declares end offset 0x{declaredEnd:X} beyond physical size 0x{data.Length:X}.");
        var stream = new BoundedBinaryReader(reader.ReadBytes(0, declaredEnd, "declared CMP/ICN stream"), sourceName);
        int compression = stream.ReadByte(2, "compression type");
        if (compression is not 1 and not 2)
            throw new InvalidDataException($"{sourceName} uses unsupported CMP compression type {compression}.");

        byte[] packed = new byte[PackedLength];
        int input = HeaderLength;
        int logicalOutput = 0;
        while (logicalOutput < packed.Length)
        {
            sbyte command = unchecked((sbyte)stream.ReadByte(input++, "RLE command"));
            if (command > 0)
            {
                int count = command;
                RequireOutput(logicalOutput, count, sourceName);
                stream.RequireRange(input, count, "RLE literals");
                for (int index = 0; index < count; index++)
                    WritePackedByte(packed, compression, logicalOutput++, stream.ReadByte(input++));
            }
            else
            {
                int count;
                if (command < 0)
                    count = -command;
                else
                {
                    count = stream.ReadUInt16LittleEndian(input, "extended RLE repeat count");
                    input += 2;
                    if (count == 0)
                        throw new InvalidDataException($"{sourceName} contains a zero-length extended RLE repeat.");
                }
                RequireOutput(logicalOutput, count, sourceName);
                byte value = stream.ReadByte(input++, "RLE repeat value");
                for (int index = 0; index < count; index++)
                    WritePackedByte(packed, compression, logicalOutput++, value);
            }
        }

        byte[] pixels = new byte[Width * Height];
        int pixel = 0;
        foreach (byte value in packed)
        {
            pixels[pixel++] = (byte)(value >> 4);
            pixels[pixel++] = (byte)(value & 0x0F);
        }
        return new CmpImage(storedLength, compression, input - HeaderLength,
            declaredEnd - HeaderLength, data.Length - declaredEnd, packed, pixels);
    }

    private static void RequireOutput(int offset, int count, string sourceName)
    {
        if (count > PackedLength - offset)
            throw new InvalidDataException($"{sourceName} RLE command at output 0x{offset:X} overruns the 0x{PackedLength:X}-byte packed image.");
    }

    private static void WritePackedByte(byte[] destination, int compression,
        int logicalOffset, byte value)
    {
        int destinationOffset = compression == 1
            ? logicalOffset
            : logicalOffset % Height * (Width / 2) + logicalOffset / Height;
        destination[destinationOffset] = value;
    }
}
