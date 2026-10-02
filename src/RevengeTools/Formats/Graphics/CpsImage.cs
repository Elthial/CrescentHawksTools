using RevengeTools.Binary;

namespace RevengeTools.Formats.Graphics;

public sealed class CpsImage
{
    private readonly byte[] _paletteBytes;
    private readonly byte[] _pixels;

    private CpsImage(int fileSize, int compressionType, int uncompressedSize,
        byte[] paletteBytes, byte[] pixels, int decodedBytesWritten,
        int compressedBytesConsumed, int compressedBytesAvailable)
    {
        FileSize = fileSize;
        CompressionType = compressionType;
        UncompressedSize = uncompressedSize;
        _paletteBytes = paletteBytes;
        _pixels = pixels;
        DecodedBytesWritten = decodedBytesWritten;
        CompressedBytesConsumed = compressedBytesConsumed;
        CompressedBytesAvailable = compressedBytesAvailable;
    }

    public const int Width = 320;
    public const int Height = 200;
    public const int PixelCount = Width * Height;
    public int FileSize { get; }
    public int CompressionType { get; }
    public string CompressionName => CompressionType switch
    {
        1 => "Westwood LZW-12",
        2 => "Westwood LZW-14",
        3 => "Westwood RLE",
        _ => "unknown"
    };
    public int UncompressedSize { get; }
    public int PaletteSize => _paletteBytes.Length;
    public int DecodedBytesWritten { get; }
    public int CompressedBytesConsumed { get; }
    public int CompressedBytesAvailable { get; }
    public int TrailingCompressedBytes => CompressedBytesAvailable - CompressedBytesConsumed;
    public byte[] EmbeddedPaletteBytes => (byte[])_paletteBytes.Clone();
    public byte[] Pixels => (byte[])_pixels.Clone();

    public static CpsImage Decode(byte[] data, string sourceName = "CPS image")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireRange(0, 10, "CPS header");
        int fileSize = reader.ReadUInt16LittleEndian(0, "file size");
        if (fileSize != data.Length)
            throw new InvalidDataException($"{sourceName} declares file size 0x{fileSize:X}; physical size is 0x{data.Length:X}.");
        int compression = reader.ReadUInt16LittleEndian(2, "compression type");
        int expanded = checked((int)reader.ReadUInt32LittleEndian(4, "uncompressed size"));
        if (expanded != PixelCount)
            throw new InvalidDataException($"{sourceName} declares 0x{expanded:X} output bytes; expected 0x{PixelCount:X} for a PC CPS screen.");
        int paletteSize = reader.ReadUInt16LittleEndian(8, "palette size");
        if (paletteSize is not 0 and not 768)
            throw new InvalidDataException($"{sourceName} has unsupported palette size {paletteSize}.");
        byte[] palette = reader.ReadBytes(10, paletteSize, "embedded palette");
        int payloadOffset = 10 + paletteSize;
        byte[] payload = reader.ReadBytes(payloadOffset, data.Length - payloadOffset, "compressed payload");

        (byte[] pixels, int consumed, int written) = compression switch
        {
            1 => AddWrittenLength(WestwoodLzwDecoder.Decode(payload, 12, 0x0FFF, expanded, sourceName), expanded),
            2 => AddWrittenLength(WestwoodLzwDecoder.Decode(payload, 14, 0x3FFF, expanded, sourceName), expanded),
            3 => WestwoodRleDecoder.Decode(payload, expanded, sourceName),
            _ => throw new InvalidDataException($"{sourceName} uses unsupported CPS compression type {compression}.")
        };
        return new CpsImage(fileSize, compression, expanded, palette, pixels, written, consumed, payload.Length);
    }

    private static (byte[] Bytes, int ConsumedBytes, int WrittenBytes) AddWrittenLength(
        (byte[] Bytes, int ConsumedBytes) decoded, int writtenBytes) =>
        (decoded.Bytes, decoded.ConsumedBytes, writtenBytes);
}

internal static class WestwoodLzwDecoder
{
    public static (byte[] Bytes, int ConsumedBytes) Decode(byte[] source, int codeWidth,
        int endMarker, int outputLength, string sourceName)
    {
        var bits = new MostSignificantBitReader(source, sourceName);
        var codes = new List<int>();
        bool ended = false;
        while (bits.RemainingBits >= codeWidth)
        {
            int code = bits.Read(codeWidth);
            if (code == endMarker)
            {
                ended = true;
                break;
            }
            if (code >= 0x100 && code - 0x100 >= codes.Count)
                throw new InvalidDataException($"{sourceName} LZW code 0x{code:X} at group {codes.Count} points outside prior groups.");
            codes.Add(code);
        }
        if (!ended)
            throw new InvalidDataException($"{sourceName} does not contain the expected 0x{endMarker:X} LZW end marker.");

        byte[] output = new byte[outputLength];
        int outputOffset = 0;
        for (int group = 0; group < codes.Count; group++)
            ExpandGroup(codes, group, output, ref outputOffset, sourceName);
        if (outputOffset != outputLength)
            throw new InvalidDataException($"{sourceName} LZW stream produced 0x{outputOffset:X} bytes; expected 0x{outputLength:X}.");
        return (output, (bits.BitPosition + 7) / 8);
    }

    private static void ExpandGroup(IReadOnlyList<int> codes, int group, byte[] output,
        ref int outputOffset, string sourceName)
    {
        int offset = group;
        int depth = 0;
        while (codes[offset] >= 0x100)
        {
            offset = codes[offset] - 0x100;
            if (offset >= group)
                throw new InvalidDataException($"{sourceName} LZW group {group} has a forward or cyclic reference.");
            if (++depth > codes.Count)
                throw new InvalidDataException($"{sourceName} LZW group {group} exceeds the reference-depth limit.");
        }
        Write(output, ref outputOffset, codes[offset], sourceName);
        if (depth == 0) return;

        int remaining = depth;
        while (remaining != 0)
        {
            offset++;
            int upperBound = offset;
            while (codes[offset] >= 0x100)
            {
                offset = codes[offset] - 0x100;
                if (offset >= upperBound)
                    throw new InvalidDataException($"{sourceName} LZW group {group} has an invalid phrase reference.");
            }
            Write(output, ref outputOffset, codes[offset], sourceName);
            remaining--;
            offset = group;
            for (int index = 0; index < remaining; index++)
            {
                offset = codes[offset] - 0x100;
                if (offset >= group)
                    throw new InvalidDataException($"{sourceName} LZW group {group} has an invalid parent reference.");
            }
        }
    }

    private static void Write(byte[] output, ref int offset, int value, string sourceName)
    {
        if (value is < 0 or > 0xFF)
            throw new InvalidDataException($"{sourceName} LZW literal is outside one byte.");
        if (offset >= output.Length)
            throw new InvalidDataException($"{sourceName} LZW output exceeds 0x{output.Length:X} bytes.");
        output[offset++] = (byte)value;
    }
}

internal static class WestwoodRleDecoder
{
    public static (byte[] Bytes, int ConsumedBytes, int WrittenBytes) Decode(
        byte[] source, int outputLength, string sourceName)
    {
        byte[] output = new byte[outputLength];
        int input = 0;
        int target = 0;
        // Revenge's 0800:0CDE loads the 16-bit extended count and XCHGs CH,CL,
        // so its PC CPS variant stores that count most-significant byte first.
        // Walking to the end of the compressed stream also permits deliberately
        // partial streams while retaining the header-sized, zero-filled workspace.
        while (input < source.Length)
        {
            RequireInput(source, input, 1, sourceName);
            sbyte command = unchecked((sbyte)source[input++]);
            if (command > 0)
            {
                int count = command;
                RequireInput(source, input, count, sourceName);
                RequireOutput(output, target, count, sourceName);
                Buffer.BlockCopy(source, input, output, target, count);
                input += count;
                target += count;
            }
            else
            {
                int count;
                if (command < 0)
                    count = -command;
                else
                {
                    RequireInput(source, input, 2, sourceName);
                    count = source[input] << 8 | source[input + 1];
                    input += 2;
                    if (count == 0)
                        throw new InvalidDataException($"{sourceName} RLE stream contains a zero-length extended repeat.");
                }
                RequireInput(source, input, 1, sourceName);
                RequireOutput(output, target, count, sourceName);
                Array.Fill(output, source[input++], target, count);
                target += count;
            }
        }
        return (output, input, target);
    }

    private static void RequireInput(byte[] source, int offset, int count, string sourceName)
    {
        if (offset < 0 || count < 0 || offset > source.Length || count > source.Length - offset)
            throw new InvalidDataException($"{sourceName} RLE command overruns its compressed payload at 0x{offset:X}.");
    }

    private static void RequireOutput(byte[] output, int offset, int count, string sourceName)
    {
        if (count > output.Length - offset)
            throw new InvalidDataException($"{sourceName} RLE command overruns the 0x{output.Length:X}-byte output at 0x{offset:X}.");
    }
}

internal sealed class MostSignificantBitReader
{
    private readonly byte[] _source;
    private readonly string _sourceName;

    public MostSignificantBitReader(byte[] source, string sourceName)
    {
        _source = source;
        _sourceName = sourceName;
    }

    public int BitPosition { get; private set; }
    public int RemainingBits => _source.Length * 8 - BitPosition;

    public int Read(int count)
    {
        if (count <= 0 || count > 24) throw new ArgumentOutOfRangeException(nameof(count));
        if (RemainingBits < count)
            throw new InvalidDataException($"{_sourceName} compressed bitstream ends at bit {BitPosition}.");
        int value = 0;
        for (int index = 0; index < count; index++)
        {
            int byteOffset = BitPosition >> 3;
            int bitOffset = 7 - (BitPosition & 7);
            value = value << 1 | (_source[byteOffset] >> bitOffset & 1);
            BitPosition++;
        }
        return value;
    }
}
