using RevengeTools.Binary;

namespace RevengeTools.Formats.Executables;

/// <summary>Bounded access to the unrelocated load image in a DOS MZ executable.</summary>
public sealed class MzExecutable
{
    private readonly byte[] _data;

    private MzExecutable(byte[] data, string sourceName)
    {
        _data = data;
        SourceName = sourceName;
        var reader = new BoundedBinaryReader(data, sourceName);
        if (reader.ReadByte(0) != (byte)'M' || reader.ReadByte(1) != (byte)'Z')
            throw new InvalidDataException(sourceName + " does not have an MZ header.");
        HeaderParagraphs = reader.ReadUInt16LittleEndian(0x08);
        LoadImageOffset = checked(HeaderParagraphs * 16);
        if (LoadImageOffset >= data.Length)
            throw new InvalidDataException(sourceName + " has an invalid MZ header size.");
    }

    public string SourceName { get; }
    public int HeaderParagraphs { get; }
    public int LoadImageOffset { get; }

    public static MzExecutable Parse(byte[] data, string sourceName = "DOS executable") =>
        new(data, sourceName);

    public int ImageAddressToFileOffset(ushort segment, ushort offset)
    {
        int fileOffset = checked(LoadImageOffset + segment * 16 + offset);
        if (fileOffset < LoadImageOffset || fileOffset >= _data.Length)
            throw new InvalidDataException(
                $"Image address {segment:X4}:{offset:X4} lies outside {SourceName}.");
        return fileOffset;
    }

    public byte[] ReadImageBytes(ushort segment, ushort offset, int count, string description)
    {
        int fileOffset = ImageAddressToFileOffset(segment, offset);
        return new BoundedBinaryReader(_data, SourceName).ReadBytes(fileOffset, count, description);
    }

    public string ReadImageAsciiZ(ushort segment, ushort offset, int maximumLength = 80)
    {
        int fileOffset = ImageAddressToFileOffset(segment, offset);
        int available = Math.Min(maximumLength, _data.Length - fileOffset);
        int length = 0;
        while (length < available && _data[fileOffset + length] != 0)
        {
            byte value = _data[fileOffset + length];
            if (value is < 0x20 or > 0x7E)
                throw new InvalidDataException(
                    $"Non-ASCII byte in string at {segment:X4}:{offset:X4} in {SourceName}.");
            length++;
        }
        if (length == available)
            throw new InvalidDataException(
                $"Unterminated string at {segment:X4}:{offset:X4} in {SourceName}.");
        return System.Text.Encoding.ASCII.GetString(_data, fileOffset, length);
    }
}
