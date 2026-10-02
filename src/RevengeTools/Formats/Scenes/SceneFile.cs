using System.Text;
using RevengeTools.Binary;

namespace RevengeTools.Formats.Scenes;

public sealed class SceneMessage
{
    public required int Index { get; init; }
    public required int Offset { get; init; }
    public required int StoredLength { get; init; }
    public required string Text { get; init; }
}

public sealed class SceneFile
{
    private readonly byte[] _metadata;
    private readonly byte[] _script;
    private readonly ushort[] _offsets;

    private SceneFile(int storedLength, ushort[] offsets, byte[] metadata,
        byte[] script, IReadOnlyList<SceneMessage> messages, int finalPaddingLength,
        byte trailerByte)
    {
        StoredLength = storedLength;
        _offsets = offsets;
        _metadata = metadata;
        _script = script;
        Messages = messages;
        FinalPaddingLength = finalPaddingLength;
        TrailerByte = trailerByte;
    }

    public const int OffsetCount = 73;
    public const int OffsetTableOffset = 2;
    public const int MetadataOffset = 0x94;
    public const int MetadataLength = 0x92;
    public const int DataOffset = 0x126;
    public const int MessageCount = OffsetCount - 1;
    public const int GenericMessageCategoryCount = 0x0B;
    public const int SpeakerGroupCount = 0x06;
    public const int SceneEventMessageBase = GenericMessageCategoryCount * SpeakerGroupCount;
    public const int SceneEventMessageCount = 0x06;
    public const int MapIdMetadataOffset = 0x0F;
    public const int IconSetIdMetadataOffset = 0x10;
    // Verified from the 0x12-byte resource descriptor table at DS:10B0.
    // Entry 0x42 begins at DS:1554 and points to the "SCENEx.DAT" filename.
    public const int ResourceTableIndex = 0x42;

    public int StoredLength { get; }
    public IReadOnlyList<ushort> Offsets => Array.AsReadOnly((ushort[])_offsets.Clone());
    public byte[] Metadata => (byte[])_metadata.Clone();
    public byte[] ScriptBytes => (byte[])_script.Clone();
    public ushort ScriptReservedWord => _script.Length >= 2
        ? (ushort)(_script[0] | _script[1] << 8)
        : throw new InvalidDataException("SCENE instruction block is shorter than its reserved word.");
    public byte[] InstructionBytes => _script.Length >= 2
        ? _script[2..]
        : throw new InvalidDataException("SCENE instruction block is shorter than its reserved word.");
    public IReadOnlyList<SceneMessage> Messages { get; }
    public byte TrailerByte { get; }
    public int FinalPaddingLength { get; }
    public int MapId => _metadata[MapIdMetadataOffset];
    public int IconSetId => _metadata[IconSetIdMetadataOffset];
    public string MapFileName => MapId == 10 ? "MAPA.MAP" : $"MAP{MapId}.MAP";
    public string IconSetFileName => $"ICONSET{IconSetId}.ICN";

    public static SceneFile Parse(byte[] data, string sourceName = "SCENE file")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireRange(0, DataOffset, "SCENE header");
        int storedLength = reader.ReadUInt16LittleEndian(0, "stored length");
        if (storedLength != data.Length - 2)
            throw new InvalidDataException($"{sourceName} stores length 0x{storedLength:X}; expected 0x{data.Length - 2:X}.");

        var offsets = new ushort[OffsetCount];
        for (int index = 0; index < offsets.Length; index++)
            offsets[index] = reader.ReadUInt16LittleEndian(OffsetTableOffset + index * 2,
                $"data offset {index}");
        if (offsets[0] != DataOffset)
            throw new InvalidDataException($"{sourceName} first data offset is 0x{offsets[0]:X}; expected 0x{DataOffset:X}.");
        for (int index = 0; index < offsets.Length; index++)
        {
            if (offsets[index] < DataOffset || offsets[index] >= data.Length)
                throw new InvalidDataException($"{sourceName} data offset {index} (0x{offsets[index]:X}) is outside the data region.");
            if (index > 0 && offsets[index] < offsets[index - 1])
                throw new InvalidDataException($"{sourceName} data offset {index} precedes offset {index - 1}.");
        }

        byte[] metadata = reader.ReadBytes(MetadataOffset, MetadataLength, "scene metadata");
        if (metadata[MapIdMetadataOffset] > 10)
            throw new InvalidDataException($"{sourceName} map selector {metadata[MapIdMetadataOffset]} exceeds MAPA.");
        if (metadata[IconSetIdMetadataOffset] > 7)
            throw new InvalidDataException($"{sourceName} icon-set selector {metadata[IconSetIdMetadataOffset]} exceeds ICONSET7.");
        byte[] script = reader.ReadBytes(offsets[0], offsets[1] - offsets[0], "scene instruction block");
        if (script.Length < 3)
            throw new InvalidDataException($"{sourceName} instruction block is too short.");
        if ((script[0] | script[1]) != 0)
            throw new InvalidDataException($"{sourceName} instruction block has a nonzero reserved word.");
        if (script[2] != 0xFC)
            throw new InvalidDataException($"{sourceName} first instruction is 0x{script[2]:X2}; expected 0xFC.");
        if (script[^1] != 0xFF)
            throw new InvalidDataException($"{sourceName} instruction block is not terminated by 0xFF.");
        var messages = new List<SceneMessage>(MessageCount);
        int finalPaddingLength = 0;
        for (int index = 1; index < offsets.Length; index++)
        {
            int start = offsets[index];
            int end = index + 1 < offsets.Length ? offsets[index + 1] : data.Length - 1;
            byte[] stored = reader.ReadBytes(start, end - start, $"message {index - 1}");
            int terminator = Array.IndexOf(stored, (byte)0);
            if (terminator < 0)
                throw new InvalidDataException($"{sourceName} message {index - 1} is not NUL terminated within its offset span.");
            if (stored[..terminator].Any(value => value is < 0x20 or > 0x7E))
                throw new InvalidDataException($"{sourceName} message {index - 1} contains non-printable bytes.");
            if (stored[(terminator + 1)..].Any(value => value != 0))
                throw new InvalidDataException($"{sourceName} message {index - 1} has nonzero bytes after its NUL terminator.");
            if (index == offsets.Length - 1)
                finalPaddingLength = stored.Length - terminator - 1;
            messages.Add(new SceneMessage
            {
                Index = index - 1,
                Offset = start,
                StoredLength = terminator + 1,
                Text = Encoding.ASCII.GetString(stored, 0, terminator)
            });
        }
        return new SceneFile(storedLength, offsets, metadata, script, messages.AsReadOnly(),
            finalPaddingLength, data[^1]);
    }
}
