using RevengeTools.Binary;
using RevengeTools.Formats.Executables;

namespace RevengeTools.Formats.Music;

public sealed class MusicStreamInstruction
{
    public required int Offset { get; init; }
    public required byte Opcode { get; init; }
    public required string Name { get; init; }
    public required string RawHex { get; init; }
    public int? Value { get; init; }
    public int? DurationUnits { get; init; }
    public int? BranchTarget { get; init; }
    public int? PitDivisor { get; init; }
}

public sealed class MusicStream
{
    public required int Index { get; init; }
    public required ushort ImageSegment { get; init; }
    public required ushort ImageOffset { get; init; }
    public required byte[] RawBytes { get; init; }
    public required IReadOnlyList<MusicStreamInstruction> Instructions { get; init; }
}

public sealed class MusicStreamCatalog
{
    public const ushort DataImageSegment = 0x326A;
    public const ushort PointerTableOffset = 0x58DB;
    public const ushort DriverDataRuntimeSegment = 0x426B;
    public const ushort DriverDataImageSegment = 0x3A6B;
    public const ushort NoteDivisorTableOffset = 0x0020;
    public const int NoteCount = 0x60;
    public const int StreamCount = 0x10;
    public const int PitInputFrequencyHz = 1193180;

    private MusicStreamCatalog(IReadOnlyList<MusicStream> streams, IReadOnlyList<ushort> noteDivisors)
    {
        Streams = streams;
        NoteDivisors = noteDivisors;
    }

    public IReadOnlyList<MusicStream> Streams { get; }
    public IReadOnlyList<ushort> NoteDivisors { get; }

    public static MusicStreamCatalog Parse(byte[] executable, string sourceName = "REVENGE.EXE")
    {
        MzExecutable mz = MzExecutable.Parse(executable, sourceName);
        byte[] pointerBytes = mz.ReadImageBytes(DataImageSegment, PointerTableOffset,
            StreamCount * 4, "music stream pointer table");
        var pointerReader = new BoundedBinaryReader(pointerBytes, "music stream pointer table");
        var pointers = Enumerable.Range(0, StreamCount).Select(index => (
            Offset: pointerReader.ReadUInt16LittleEndian(index * 4),
            Segment: pointerReader.ReadUInt16LittleEndian(index * 4 + 2))).ToArray();
        byte[] divisorBytes = mz.ReadImageBytes(DriverDataImageSegment, NoteDivisorTableOffset,
            NoteCount * 2, "music note divisor table");
        var divisorReader = new BoundedBinaryReader(divisorBytes, "music note divisor table");
        ushort[] noteDivisors = Enumerable.Range(0, NoteCount)
            .Select(index => divisorReader.ReadUInt16LittleEndian(index * 2)).ToArray();

        var streams = new List<MusicStream>(StreamCount);
        for (int index = 0; index < pointers.Length; index++)
        {
            (ushort offset, ushort segment) = pointers[index];
            if (segment != DataImageSegment)
                throw new InvalidDataException($"Music stream {index:X2} points to unexpected image segment {segment:X4}.");
            int endOffset = index + 1 < pointers.Length ? pointers[index + 1].Offset : PointerTableOffset;
            if (endOffset <= offset)
                throw new InvalidDataException($"Music stream {index:X2} has a non-increasing boundary.");
            byte[] raw = mz.ReadImageBytes(segment, offset, endOffset - offset, $"music stream {index:X2}");
            IReadOnlyList<MusicStreamInstruction> instructions = Decode(raw, index, noteDivisors);
            streams.Add(new MusicStream
            {
                Index = index,
                ImageSegment = segment,
                ImageOffset = offset,
                RawBytes = raw,
                Instructions = instructions
            });
        }
        return new MusicStreamCatalog(streams.AsReadOnly(), Array.AsReadOnly(noteDivisors));
    }

    public static IReadOnlyList<MusicStreamInstruction> Decode(byte[] data, int streamIndex = 0,
        IReadOnlyList<ushort>? noteDivisors = null)
    {
        var reader = new BoundedBinaryReader(data, $"music stream {streamIndex:X2}");
        var instructions = new List<MusicStreamInstruction>();
        int cursor = 0;
        bool ended = false;
        while (cursor < data.Length)
        {
            int offset = cursor;
            byte opcode = reader.ReadByte(cursor);
            int width;
            string name;
            int? value = null;
            int? duration = null;
            int? target = null;
            int? divisor = null;

            if (opcode <= 0x5F)
            {
                width = 2;
                name = "NoteOrRest";
                value = opcode;
                duration = reader.ReadByte(cursor + 1) + 1;
                if (noteDivisors is not null)
                {
                    if (noteDivisors.Count != NoteCount)
                        throw new ArgumentException($"Expected {NoteCount} note divisors.", nameof(noteDivisors));
                    divisor = noteDivisors[opcode];
                }
            }
            else
            {
                switch (opcode)
                {
                    case 0xCD:
                        width = 2; name = "Loop1Begin";
                        value = NonZero(reader.ReadByte(cursor + 1));
                        break;
                    case 0xCE:
                        width = 3; name = "Loop1End";
                        value = reader.ReadUInt16LittleEndian(cursor + 1);
                        target = cursor + 1 - value;
                        break;
                    case 0xD1:
                        width = 2; name = "Loop2Begin";
                        value = NonZero(reader.ReadByte(cursor + 1));
                        break;
                    case 0xD2:
                        width = 3; name = "Loop2End";
                        value = reader.ReadUInt16LittleEndian(cursor + 1);
                        target = cursor + 1 - value;
                        break;
                    case 0xE2:
                        width = 2; name = "SetDurationScale";
                        value = NonZero(reader.ReadByte(cursor + 1));
                        break;
                    case 0xE6:
                        width = 4; name = "FrequencyHz";
                        value = reader.ReadUInt16LittleEndian(cursor + 1);
                        if (value == 0)
                            throw new InvalidDataException($"Music stream {streamIndex:X2} has a zero frequency at +0x{cursor:X4}.");
                        divisor = PitInputFrequencyHz / value;
                        duration = reader.ReadByte(cursor + 3) + 1;
                        break;
                    case 0xE8:
                        width = 2; name = "DecreaseDurationScale";
                        value = reader.ReadByte(cursor + 1);
                        break;
                    case 0xE9:
                        width = 2; name = "IncreaseDurationScale";
                        value = reader.ReadByte(cursor + 1);
                        break;
                    case 0xFF:
                        width = 1; name = "End"; ended = true;
                        break;
                    default:
                        throw new InvalidDataException(
                            $"Music stream {streamIndex:X2} has unknown opcode 0x{opcode:X2} at +0x{cursor:X4}.");
                }
            }

            reader.RequireRange(cursor, width, $"instruction at +0x{cursor:X4}");
            instructions.Add(new MusicStreamInstruction
            {
                Offset = offset,
                Opcode = opcode,
                Name = name,
                RawHex = Convert.ToHexString(data, cursor, width),
                Value = value,
                DurationUnits = duration,
                BranchTarget = target,
                PitDivisor = divisor
            });
            cursor += width;
            if (ended)
                break;
        }

        if (!ended)
            throw new InvalidDataException($"Music stream {streamIndex:X2} has no FF terminator.");
        if (cursor != data.Length)
            throw new InvalidDataException(
                $"Music stream {streamIndex:X2} ended at 0x{cursor:X}; expected boundary 0x{data.Length:X}.");
        return instructions.AsReadOnly();
    }

    private static int NonZero(byte value) => value == 0 ? 1 : value;
}
