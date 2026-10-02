using System.Text;

namespace RevengeTools.Binary;

/// <summary>Bounds-checked reads over an immutable byte buffer.</summary>
public sealed class BoundedBinaryReader
{
    private readonly byte[] _data;

    public BoundedBinaryReader(byte[] data, string sourceName = "binary data")
    {
        ArgumentNullException.ThrowIfNull(data);
        _data = (byte[])data.Clone();
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? "binary data" : sourceName;
    }

    public int Length => _data.Length;
    public string SourceName { get; }

    public void RequireExactLength(int expectedLength, string structureName)
    {
        if (Length != expectedLength)
            throw new InvalidDataException($"{structureName} in {SourceName} is 0x{Length:X} bytes; expected 0x{expectedLength:X}.");
    }

    public byte ReadByte(int offset, string? fieldName = null)
    {
        RequireRange(offset, 1, fieldName);
        return _data[offset];
    }

    public ushort ReadUInt16LittleEndian(int offset, string? fieldName = null)
    {
        RequireRange(offset, 2, fieldName);
        return (ushort)(_data[offset] | _data[offset + 1] << 8);
    }

    public uint ReadUInt32LittleEndian(int offset, string? fieldName = null)
    {
        RequireRange(offset, 4, fieldName);
        return (uint)(_data[offset] | _data[offset + 1] << 8 |
            _data[offset + 2] << 16 | _data[offset + 3] << 24);
    }

    public byte[] ReadBytes(int offset, int count, string? fieldName = null)
    {
        RequireRange(offset, count, fieldName);
        byte[] result = new byte[count];
        Buffer.BlockCopy(_data, offset, result, 0, count);
        return result;
    }

    public string ReadFixedAscii(int offset, int count, string? fieldName = null)
    {
        byte[] bytes = ReadBytes(offset, count, fieldName);
        int terminator = Array.IndexOf(bytes, (byte)0);
        int length = terminator >= 0 ? terminator : bytes.Length;
        return Encoding.ASCII.GetString(bytes, 0, length).TrimEnd(' ');
    }

    public void RequireRange(int offset, int count, string? fieldName = null)
    {
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (offset > Length || count > Length - offset)
        {
            string field = string.IsNullOrWhiteSpace(fieldName) ? "Read" : $"Read for {fieldName}";
            throw new InvalidDataException($"{field} at 0x{offset:X} for 0x{count:X} bytes exceeds {SourceName} length 0x{Length:X}.");
        }
    }
}
