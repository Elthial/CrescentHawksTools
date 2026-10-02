using System;
using System.IO;
using System.Text;

namespace InceptionTools.Binary
{
    /// <summary>
    /// Provides explicit-offset, bounds-checked reads over an immutable view of
    /// binary data. Multi-byte values are always little-endian.
    /// </summary>
    public sealed class BoundedBinaryReader
    {
        private readonly byte[] _data;

        public BoundedBinaryReader(byte[] data, string sourceName = "binary data")
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            _data = (byte[])data.Clone();
            SourceName = string.IsNullOrWhiteSpace(sourceName) ? "binary data" : sourceName;
        }

        public int Length => _data.Length;
        public string SourceName { get; }

        public void RequireExactLength(int expectedLength, string structureName)
        {
            if (Length != expectedLength)
                throw new InvalidDataException((structureName ?? "Structure") + " in " + SourceName +
                    " is 0x" + Length.ToString("X") + " bytes; expected exactly 0x" + expectedLength.ToString("X") + ".");
        }

        public byte ReadByte(int offset, string fieldName = null)
        {
            RequireRange(offset, 1, fieldName);
            return _data[offset];
        }

        public ushort ReadUInt16LittleEndian(int offset, string fieldName = null)
        {
            RequireRange(offset, 2, fieldName);
            return (ushort)(_data[offset] | (_data[offset + 1] << 8));
        }

        public short ReadInt16LittleEndian(int offset, string fieldName = null)
        {
            return unchecked((short)ReadUInt16LittleEndian(offset, fieldName));
        }

        public uint ReadUInt32LittleEndian(int offset, string fieldName = null)
        {
            RequireRange(offset, 4, fieldName);
            return (uint)(_data[offset] | (_data[offset + 1] << 8) |
                (_data[offset + 2] << 16) | (_data[offset + 3] << 24));
        }

        public byte[] ReadBytes(int offset, int count, string fieldName = null)
        {
            RequireRange(offset, count, fieldName);
            var result = new byte[count];
            Buffer.BlockCopy(_data, offset, result, 0, count);
            return result;
        }

        public string ReadFixedAscii(int offset, int count, bool trimNullAndSpace = true, string fieldName = null)
        {
            string value = Encoding.ASCII.GetString(ReadBytes(offset, count, fieldName));
            return trimNullAndSpace ? value.TrimEnd('\0', ' ') : value;
        }

        public int IndexOf(byte value, int startOffset, string fieldName = null)
        {
            RequireRange(startOffset, 0, fieldName);
            return Array.IndexOf(_data, value, startOffset);
        }

        public void RequireRange(int offset, int count, string fieldName = null)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Byte count cannot be negative.");
            if (offset > Length || count > Length - offset)
            {
                string field = string.IsNullOrWhiteSpace(fieldName) ? "Read" : "Read for " + fieldName;
                throw new InvalidDataException(field + " at 0x" + offset.ToString("X") + " for 0x" + count.ToString("X") +
                    " bytes exceeds " + SourceName + " length 0x" + Length.ToString("X") + ".");
            }
        }
    }
}
