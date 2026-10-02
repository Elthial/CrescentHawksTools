using RevengeTools.Binary;
using RevengeTools.Formats.Units;
using System.Text.Json.Serialization;

namespace RevengeTools.Formats.Saves;

public sealed class RevengeSaveSlot
{
    internal RevengeSaveSlot(int index, int descriptorOffset, byte[] controls, string label,
        int payloadOffset, byte[] payload, IReadOnlyList<UnitRecord> units)
    {
        Index = index;
        DescriptorOffset = descriptorOffset;
        ControlBytes = controls;
        Label = label;
        PayloadOffset = payloadOffset;
        Payload = payload;
        Units = units;
        var reader = new BoundedBinaryReader(payload, $"save slot {index + 1}");
        CampaignStage = reader.ReadUInt16LittleEndian(0x0000);
        ScenarioVariant = reader.ReadUInt16LittleEndian(0x0002);
        CampaignPhase = reader.ReadByte(0x0004);
        TrainingSequenceFlag = reader.ReadByte(0x0005);
        CampaignFlags = reader.ReadUInt16LittleEndian(0x0006);
    }

    public int Index { get; }
    public int SlotNumber => Index + 1;
    public int DescriptorOffset { get; }
    public byte[] ControlBytes { get; }
    public string Label { get; }
    public int PayloadOffset { get; }
    [JsonIgnore]
    public byte[] Payload { get; }
    public ushort CampaignStage { get; }
    public ushort ScenarioVariant { get; }
    public byte CampaignPhase { get; }
    public byte TrainingSequenceFlag { get; }
    public ushort CampaignFlags { get; }
    public IReadOnlyList<UnitRecord> Units { get; }
    public bool AppearsOccupied => ControlBytes.Any(value => value != 0) || Units.Any(unit => unit.IsPopulated);
}

public sealed class RevengeSaveFile
{
    public const int Length = 0x61C0;
    public const int SlotCount = 6;
    public const int DescriptorOffset = 0x12;
    public const int DescriptorLength = 22;
    public const int DescriptorControlLength = 5;
    public const int DescriptorLabelLength = 17;
    public const int PayloadLength = 0x1031;
    public const int CampaignSnapshotLength = 0x221;
    public const int LiveUnitOffset = 0x221;
    public const int LiveUnitCount = 24;
    public static readonly ushort[] ExpectedBoundaries = { 0x0007, 0x0012, 0x009A, 0x10CB, 0x20FC, 0x312D, 0x415E, 0x518F, 0x61C0 };

    private RevengeSaveFile(ushort[] boundaries, IReadOnlyList<RevengeSaveSlot> slots)
    {
        Boundaries = boundaries;
        Slots = slots;
    }

    public IReadOnlyList<ushort> Boundaries { get; }
    public IReadOnlyList<RevengeSaveSlot> Slots { get; }

    public RevengeSaveSlot GetSlot(int slotNumber)
    {
        if (slotNumber is < 1 or > SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slotNumber), "Save slot must be 1 through 6.");
        return Slots[slotNumber - 1];
    }

    public static RevengeSaveFile Parse(byte[] data, string sourceName = "SAVEGAME.DAT")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireExactLength(Length, "Revenge save container");
        ushort[] boundaries = Enumerable.Range(0, ExpectedBoundaries.Length)
            .Select(index => reader.ReadUInt16LittleEndian(index * 2, $"boundary {index}"))
            .ToArray();
        if (!boundaries.SequenceEqual(ExpectedBoundaries))
            throw new InvalidDataException($"{sourceName} does not use the verified Revenge save boundary profile.");

        var slots = new List<RevengeSaveSlot>();
        for (int index = 0; index < SlotCount; index++)
        {
            int descriptor = DescriptorOffset + index * DescriptorLength;
            byte[] controls = reader.ReadBytes(descriptor, DescriptorControlLength, $"slot {index + 1} controls");
            string label = reader.ReadFixedAscii(descriptor + DescriptorControlLength, DescriptorLabelLength,
                $"slot {index + 1} label");
            int payloadOffset = boundaries[index + 2];
            int payloadEnd = boundaries[index + 3];
            if (payloadEnd - payloadOffset != PayloadLength)
                throw new InvalidDataException($"Slot {index + 1} payload length is 0x{payloadEnd - payloadOffset:X}; expected 0x{PayloadLength:X}.");
            byte[] payload = reader.ReadBytes(payloadOffset, PayloadLength, $"slot {index + 1} payload");
            IReadOnlyList<UnitRecord> units = Enumerable.Range(0, LiveUnitCount)
                .Select(unit => UnitRecord.Parse(payload, LiveUnitOffset + unit * UnitRecord.Length, unit,
                    $"{sourceName} slot {index + 1}"))
                .ToArray();
            slots.Add(new RevengeSaveSlot(index, descriptor, controls, label, payloadOffset, payload, units));
        }
        return new RevengeSaveFile(boundaries, slots);
    }
}
