using RevengeTools.Binary;
using RevengeTools.Formats.Executables;
using RevengeTools.Installation;

namespace RevengeTools.Formats.Audio;

public sealed class DigitalSoundEntry
{
    public const int Length = 0x0B;

    internal DigitalSoundEntry(int index, byte[] raw)
    {
        var reader = new BoundedBinaryReader(raw, $"digital sound entry {index:X2}");
        Index = index;
        SoundCode = reader.ReadByte(0x00);
        ResourceIndex = reader.ReadUInt16LittleEndian(0x01);
        SourceOffset = reader.ReadUInt32LittleEndian(0x03);
        SampleLength = reader.ReadUInt16LittleEndian(0x07);
        PackedBufferOffset = reader.ReadUInt16LittleEndian(0x09);
        RawHex = Convert.ToHexString(raw);
    }

    public int Index { get; }
    public byte SoundCode { get; }
    public ushort ResourceIndex { get; }
    public string ResourceFileName { get; internal set; } = string.Empty;
    public uint SourceOffset { get; }
    public ushort SampleLength { get; }
    public ushort PackedBufferOffset { get; }
    public string RawHex { get; }
}

public sealed class DigitalSoundSequenceFragment
{
    internal DigitalSoundSequenceFragment(ushort dataOffset, string soundCodes,
        IReadOnlyList<DigitalSoundEntry> entries)
    {
        DataOffset = dataOffset;
        SoundCodes = soundCodes;
        Entries = entries;
    }

    public ushort DataOffset { get; }
    public string Kind => DataOffset < DigitalSoundCatalog.PlaybackSequenceTableOffset
        ? "load-bank code list" : "playback fragment";
    public string SoundCodes { get; }
    public IReadOnlyList<DigitalSoundEntry> Entries { get; }
    public int TotalSampleBytes => Entries.Sum(entry => entry.SampleLength);
    public decimal DurationSeconds => TotalSampleBytes / (decimal)DigitalSoundCatalog.PlaybackRateHz;
}

public sealed class DigitalSoundCatalog
{
    public const ushort DataImageSegment = 0x326A;
    public const ushort SoundTableOffset = 0x5CFC;
    public const int EntryCount = 0x16;
    public const ushort ResourceTableOffset = 0x10B0;
    public const int ResourceRecordLength = 0x12;
    public const int PlaybackRateHz = 0x1F40;
    public const ushort LoadBankSequenceTableOffset = 0x5DEE;
    public const ushort PlaybackSequenceTableOffset = 0x5E6E;
    public const ushort SequenceTableEndOffset = 0x5F6B;
    public const ushort UnityPlaybackScale = 0x6633;

    private DigitalSoundCatalog(IReadOnlyList<DigitalSoundEntry> entries,
        IReadOnlyList<DigitalSoundSequenceFragment> sequenceFragments)
    {
        Entries = entries;
        SequenceFragments = sequenceFragments;
    }

    public IReadOnlyList<DigitalSoundEntry> Entries { get; }
    public IReadOnlyList<DigitalSoundSequenceFragment> SequenceFragments { get; }

    public static DigitalSoundCatalog Parse(byte[] executable, string sourceName = "REVENGE.EXE")
    {
        MzExecutable mz = MzExecutable.Parse(executable, sourceName);
        var entries = new List<DigitalSoundEntry>(EntryCount);
        for (int index = 0; index < EntryCount; index++)
        {
            ushort offset = checked((ushort)(SoundTableOffset + index * DigitalSoundEntry.Length));
            byte[] raw = mz.ReadImageBytes(DataImageSegment, offset, DigitalSoundEntry.Length,
                $"digital sound entry {index:X2}");
            var entry = new DigitalSoundEntry(index, raw);
            ushort resourceOffset = checked((ushort)(ResourceTableOffset + entry.ResourceIndex * ResourceRecordLength));
            byte[] resource = mz.ReadImageBytes(DataImageSegment, resourceOffset, 4,
                $"resource descriptor {entry.ResourceIndex:X2}");
            var resourceReader = new BoundedBinaryReader(resource, $"resource descriptor {entry.ResourceIndex:X2}");
            ushort nameOffset = resourceReader.ReadUInt16LittleEndian(0x00);
            ushort nameSegment = resourceReader.ReadUInt16LittleEndian(0x02);
            entry.ResourceFileName = mz.ReadImageAsciiZ(nameSegment, nameOffset);
            entries.Add(entry);
        }

        byte[] sequenceBytes = mz.ReadImageBytes(DataImageSegment, LoadBankSequenceTableOffset,
            SequenceTableEndOffset - LoadBankSequenceTableOffset, "digital sound sequence table");
        var byCode = entries.ToDictionary(entry => entry.SoundCode);
        var fragments = new List<DigitalSoundSequenceFragment>();
        int position = 0;
        while (position < sequenceBytes.Length)
        {
            int start = position;
            while (position < sequenceBytes.Length && sequenceBytes[position] != 0x00) position++;
            if (position == sequenceBytes.Length)
                throw new InvalidDataException("Digital sound sequence table has an unterminated fragment.");
            string codes = System.Text.Encoding.ASCII.GetString(sequenceBytes, start, position - start);
            if (codes.Length != 0)
            {
                var resolved = new List<DigitalSoundEntry>(codes.Length);
                foreach (char code in codes)
                {
                    if (!byCode.TryGetValue((byte)code, out DigitalSoundEntry? entry))
                        throw new InvalidDataException(
                            $"Digital sound sequence at DS:{LoadBankSequenceTableOffset + start:X4} uses unknown code 0x{(byte)code:X2}.");
                    resolved.Add(entry);
                }
                fragments.Add(new DigitalSoundSequenceFragment(
                    checked((ushort)(LoadBankSequenceTableOffset + start)), codes, resolved.AsReadOnly()));
            }
            position++;
        }

        return new DigitalSoundCatalog(entries.AsReadOnly(), fragments.AsReadOnly());
    }

    public byte[] ReadSample(GameInstallation installation, DigitalSoundEntry entry)
    {
        byte[] resource = File.ReadAllBytes(installation.ResolveFile(entry.ResourceFileName));
        if (entry.SourceOffset > resource.Length || entry.SampleLength > resource.Length - entry.SourceOffset)
            throw new InvalidDataException(
                $"Digital sound entry {entry.Index:X2} exceeds {entry.ResourceFileName}: " +
                $"offset 0x{entry.SourceOffset:X}, length 0x{entry.SampleLength:X}.");
        return resource.AsSpan(checked((int)entry.SourceOffset), entry.SampleLength).ToArray();
    }
}
