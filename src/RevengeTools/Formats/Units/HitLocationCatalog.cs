using RevengeTools.Binary;
using RevengeTools.Formats.Executables;

namespace RevengeTools.Formats.Units;

public sealed record HitLocationTable(int Index, IReadOnlyList<byte> ResultsBy2D6);

public sealed class HitLocationCatalog
{
    public const ushort DataImageSegment = 0x326A;
    public const ushort TableOffset = 0x1038;
    public const ushort DirectionSelectorOffset = 0x3CFE;
    public const int TableCount = 0x05;
    public const int ResultCount = 0x0B;
    public const int TableStride = 0x18;
    public const int DirectionSelectorCount = 0x10;

    private HitLocationCatalog(
        IReadOnlyList<HitLocationTable> tables,
        IReadOnlyList<ushort> directionSelectors)
    {
        Tables = tables;
        DirectionSelectors = directionSelectors;
    }

    public IReadOnlyList<HitLocationTable> Tables { get; }
    public IReadOnlyList<ushort> DirectionSelectors { get; }

    public static HitLocationCatalog Parse(byte[] executable, string sourceName = "REVENGE.EXE")
    {
        MzExecutable mz = MzExecutable.Parse(executable, sourceName);
        var tables = new List<HitLocationTable>(TableCount);
        for (int tableIndex = 0; tableIndex < TableCount; tableIndex++)
        {
            byte[] raw = mz.ReadImageBytes(DataImageSegment,
                checked((ushort)(TableOffset + tableIndex * TableStride)), TableStride,
                $"hit-location table {tableIndex:X2}");
            var reader = new BoundedBinaryReader(raw, $"hit-location table {tableIndex:X2}");
            var results = new byte[ResultCount];
            for (int result = 0; result < ResultCount; result++)
            {
                ushort value = reader.ReadUInt16LittleEndian(result * 2);
                if (value > 0x0A)
                    throw new InvalidDataException(
                        $"Hit-location table {tableIndex:X2}, roll {result + 2} contains invalid location {value:X4}.");
                results[result] = (byte)value;
            }
            if (reader.ReadUInt16LittleEndian(0x16) != 0x0000)
                throw new InvalidDataException($"Hit-location table {tableIndex:X2} has nonzero padding.");
            tables.Add(new HitLocationTable(tableIndex, Array.AsReadOnly(results)));
        }

        byte[] selectorBytes = mz.ReadImageBytes(DataImageSegment,
            DirectionSelectorOffset, DirectionSelectorCount * 2,
            "directional hit-location selectors");
        var selectorReader = new BoundedBinaryReader(selectorBytes,
            "directional hit-location selectors");
        var selectors = new ushort[DirectionSelectorCount];
        for (int index = 0; index < selectors.Length; index++)
        {
            selectors[index] = selectorReader.ReadUInt16LittleEndian(index * 2);
            if (selectors[index] > 0x03)
                throw new InvalidDataException(
                    $"Directional hit-location selector {index:X2} contains invalid table {selectors[index]:X4}.");
        }

        return new HitLocationCatalog(tables.AsReadOnly(), Array.AsReadOnly(selectors));
    }

    public static string BattleMechLocationName(byte code) => code switch
    {
        0x00 => "Head",
        0x01 => "Left Arm",
        0x02 => "Left Torso",
        0x03 => "Center Torso",
        0x04 => "Right Torso",
        0x05 => "Right Arm",
        0x06 => "Left Leg",
        0x07 => "Right Leg",
        0x08 => "Left Torso Rear",
        0x09 => "Center Torso Rear",
        0x0A => "Right Torso Rear",
        _ => $"Unknown {code:X2}"
    };
}
