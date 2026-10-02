using RevengeTools.Binary;

namespace RevengeTools.Formats.Music;

public sealed class MusicPlaybackEvent
{
    public required int EventIndex { get; init; }
    public required int SourceOffset { get; init; }
    public required byte Opcode { get; init; }
    public required bool IsRest { get; init; }
    public required int PitDivisor { get; init; }
    public required int DurationScale { get; init; }
    public required int DurationUnits { get; init; }
    public required int EffectiveTicks { get; init; }
    public required long StartTick { get; init; }
    public int? ApproximateFrequencyHz => PitDivisor == 0
        ? null
        : MusicStreamCatalog.PitInputFrequencyHz / PitDivisor;
}

public sealed class MusicPlaybackTrace
{
    public required IReadOnlyList<MusicPlaybackEvent> Events { get; init; }
    public required long TotalTicks { get; init; }
    public required int ExecutedInstructionCount { get; init; }
}

public static class MusicPlaybackSimulator
{
    public static MusicPlaybackTrace Simulate(MusicStream stream, IReadOnlyList<ushort> noteDivisors,
        int maximumInstructions = 100_000)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(noteDivisors);
        if (noteDivisors.Count != MusicStreamCatalog.NoteCount)
            throw new ArgumentException($"Expected {MusicStreamCatalog.NoteCount} note divisors.", nameof(noteDivisors));
        if (maximumInstructions <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumInstructions));

        byte[] data = stream.RawBytes;
        var reader = new BoundedBinaryReader(data, $"music stream {stream.Index:X2} playback");
        var events = new List<MusicPlaybackEvent>();
        int cursor = 0;
        int durationScale = 0;
        byte loop1Remaining = 0;
        byte loop2Remaining = 0;
        int executed = 0;
        long tick = 0;

        while (true)
        {
            if (++executed > maximumInstructions)
                throw new InvalidDataException($"Music stream {stream.Index:X2} exceeded the playback instruction limit.");
            byte opcode = reader.ReadByte(cursor);

            if (opcode <= 0x5F)
            {
                int durationUnits = reader.ReadByte(cursor + 1) + 1;
                int divisor = noteDivisors[opcode];
                AddEvent(opcode, divisor, durationUnits);
                cursor += 2;
                continue;
            }

            switch (opcode)
            {
                case 0xCD:
                    loop1Remaining = (byte)NonZero(reader.ReadByte(cursor + 1));
                    cursor += 2;
                    break;
                case 0xCE:
                    loop1Remaining--;
                    cursor = loop1Remaining != 0
                        ? cursor + 1 - reader.ReadUInt16LittleEndian(cursor + 1)
                        : cursor + 3;
                    break;
                case 0xD1:
                    loop2Remaining = (byte)NonZero(reader.ReadByte(cursor + 1));
                    cursor += 2;
                    break;
                case 0xD2:
                    loop2Remaining--;
                    cursor = loop2Remaining != 0
                        ? cursor + 1 - reader.ReadUInt16LittleEndian(cursor + 1)
                        : cursor + 3;
                    break;
                case 0xE2:
                    durationScale = NonZero(reader.ReadByte(cursor + 1));
                    cursor += 2;
                    break;
                case 0xE6:
                    int frequencyHz = reader.ReadUInt16LittleEndian(cursor + 1);
                    if (frequencyHz == 0)
                        throw new InvalidDataException($"Music stream {stream.Index:X2} has a zero frequency at +0x{cursor:X4}.");
                    int directDurationUnits = reader.ReadByte(cursor + 3) + 1;
                    AddEvent(opcode, MusicStreamCatalog.PitInputFrequencyHz / frequencyHz, directDurationUnits);
                    cursor += 4;
                    break;
                case 0xE8:
                    durationScale = SubtractDurationScaleLikeDriver(
                        (byte)durationScale, reader.ReadByte(cursor + 1));
                    cursor += 2;
                    break;
                case 0xE9:
                    durationScale = AddDurationScaleLikeDriver(
                        (byte)durationScale, reader.ReadByte(cursor + 1));
                    cursor += 2;
                    break;
                case 0xFF:
                    return new MusicPlaybackTrace
                    {
                        Events = events.AsReadOnly(),
                        TotalTicks = tick,
                        ExecutedInstructionCount = executed
                    };
                default:
                    throw new InvalidDataException(
                        $"Music stream {stream.Index:X2} has unknown opcode 0x{opcode:X2} at +0x{cursor:X4}.");
            }
        }

        void AddEvent(byte opcode, int divisor, int durationUnits)
        {
            // The driver stores scale*units-1 in a word. A zero product wraps
            // to FFFFh and is retired by the next signed countdown check, so
            // it still occupies one service interval rather than being invalid.
            int effectiveTicks = Math.Max(1, checked(durationScale * durationUnits));
            events.Add(new MusicPlaybackEvent
            {
                EventIndex = events.Count,
                SourceOffset = cursor,
                Opcode = opcode,
                IsRest = divisor == 0,
                PitDivisor = divisor,
                DurationScale = durationScale,
                DurationUnits = durationUnits,
                EffectiveTicks = effectiveTicks,
                StartTick = tick
            });
            tick = checked(tick + effectiveTicks);
        }
    }

    private static int NonZero(byte value) => value == 0 ? 1 : value;

    private static byte SubtractDurationScaleLikeDriver(byte value, byte decrease)
    {
        byte result = unchecked((byte)(value - decrease));
        bool signedOverflow = ((value ^ decrease) & (value ^ result) & 0x80) != 0;
        // This reproduces JNO/JNZ in the original handler. The second test is
        // effectively unreachable for SUB, but retaining it documents the
        // executable rather than replacing it with intended saturation.
        return signedOverflow && result == 0 ? (byte)1 : result;
    }

    private static byte AddDurationScaleLikeDriver(byte value, byte increase)
    {
        byte result = unchecked((byte)(value + increase));
        bool signedOverflow = ((~(value ^ increase) & (value ^ result)) & 0x80) != 0;
        return signedOverflow ? (byte)0xFF : result;
    }
}
