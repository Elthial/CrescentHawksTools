using System;
using System.IO;
using InceptionTools.Binary;

namespace InceptionTools.Graphics;

public sealed class CompressedImage
{
    private readonly byte[] _packedBytes;
    private readonly byte[] _paletteIndices;

    internal CompressedImage(int storedLength, int compressionFormat, int consumedPayloadBytes,
        int payloadLength, byte[] packedBytes, byte[] paletteIndices)
    {
        StoredLength = storedLength;
        CompressionFormat = compressionFormat;
        ConsumedPayloadBytes = consumedPayloadBytes;
        PayloadLength = payloadLength;
        _packedBytes = (byte[])packedBytes.Clone();
        _paletteIndices = (byte[])paletteIndices.Clone();
    }

    public const int Width = 320;
    public const int Height = 200;
    public const int PackedLength = Width * Height / 2;

    public int StoredLength { get; }
    public int CompressionFormat { get; }
    public int ConsumedPayloadBytes { get; }
    public int PayloadLength { get; }
    public int RemainingPayloadBytes => PayloadLength - ConsumedPayloadBytes;
    public byte[] PackedBytes => (byte[])_packedBytes.Clone();
    public byte[] PaletteIndices => (byte[])_paletteIndices.Clone();
}

/// <summary>
/// Bounded decoder for the full-screen packed image shared by CMP and ICN
/// files. Format 1 writes row-major bytes; format 2's RLE output traverses
/// 200 rows for each packed-byte column.
/// </summary>
public static class CompressedImageDecoder
{
    public const int HeaderLength = 3;

    public static CompressedImage Decode(byte[] data, string sourceName = "compressed image")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireRange(0, HeaderLength, "graphics header");
        int storedLength = reader.ReadUInt16LittleEndian(0, "stored file length");
        if (storedLength != reader.Length - 2)
            throw new InvalidDataException(sourceName + " stored length is 0x" + storedLength.ToString("X") +
                "; expected file length minus two (0x" + (reader.Length - 2).ToString("X") + ").");

        int format = reader.ReadByte(2, "compression format");
        if (format != 1 && format != 2)
            throw new InvalidDataException(sourceName + " uses unsupported compression format 0x" +
                format.ToString("X2") + ".");

        byte[] packed = new byte[CompressedImage.PackedLength];
        int sourceOffset = HeaderLength;
        int logicalOutputOffset = 0;
        while (logicalOutputOffset < packed.Length)
        {
            sbyte control = unchecked((sbyte)reader.ReadByte(sourceOffset++, "RLE control"));
            if (control > 0)
            {
                int count = control;
                RequireOutputSpace(logicalOutputOffset, count, sourceName);
                for (int index = 0; index < count; index++)
                    WritePackedByte(packed, format, logicalOutputOffset++,
                        reader.ReadByte(sourceOffset++, "RLE literal"));
            }
            else
            {
                int count;
                if (control < 0)
                    count = -control;
                else
                {
                    count = reader.ReadUInt16LittleEndian(sourceOffset, "extended RLE repeat count");
                    sourceOffset += 2;
                    if (count == 0)
                        throw new InvalidDataException(sourceName + " contains a zero-length extended RLE run.");
                }

                RequireOutputSpace(logicalOutputOffset, count, sourceName);
                byte value = reader.ReadByte(sourceOffset++, "RLE repeat value");
                for (int index = 0; index < count; index++)
                    WritePackedByte(packed, format, logicalOutputOffset++, value);
            }
        }

        byte[] pixels = new byte[CompressedImage.Width * CompressedImage.Height];
        int pixelOffset = 0;
        foreach (byte value in packed)
        {
            pixels[pixelOffset++] = (byte)(value >> 4);
            pixels[pixelOffset++] = (byte)(value & 0x0F);
        }

        return new CompressedImage(storedLength, format, sourceOffset - HeaderLength,
            reader.Length - HeaderLength, packed, pixels);
    }

    private static void RequireOutputSpace(int offset, int count, string sourceName)
    {
        if (count > CompressedImage.PackedLength - offset)
            throw new InvalidDataException(sourceName + " RLE run at output offset 0x" + offset.ToString("X") +
                " exceeds the 0x" + CompressedImage.PackedLength.ToString("X") + "-byte image buffer.");
    }

    private static void WritePackedByte(byte[] destination, int format, int logicalOffset, byte value)
    {
        int destinationOffset = format == 1
            ? logicalOffset
            : logicalOffset % CompressedImage.Height * (CompressedImage.Width / 2) +
                logicalOffset / CompressedImage.Height;
        destination[destinationOffset] = value;
    }
}
