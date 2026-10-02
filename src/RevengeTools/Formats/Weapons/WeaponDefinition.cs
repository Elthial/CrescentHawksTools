using RevengeTools.Binary;
using RevengeTools.Formats.Executables;

namespace RevengeTools.Formats.Weapons;

public sealed class WeaponDefinition
{
    public const int Length = 0x0F;

    private WeaponDefinition(byte[] raw, int index, string name)
    {
        var reader = new BoundedBinaryReader(raw, $"weapon definition {index}");
        Index = index;
        NameOffset = reader.ReadUInt16LittleEndian(0x00);
        NameSegment = reader.ReadUInt16LittleEndian(0x02);
        Name = name;
        Heat = reader.ReadByte(0x04);
        Damage = reader.ReadByte(0x05);
        MinimumRangeSquares = reader.ReadByte(0x06);
        MaximumRangeUnits = reader.ReadByte(0x07);
        WeightHalfTons = reader.ReadByte(0x08);
        Reserved09 = reader.ReadByte(0x09);
        MissileRackSize = reader.ReadByte(0x0A);
        AmmunitionFamilyId = reader.ReadByte(0x0B);
        CriticalSlotCount = reader.ReadByte(0x0C);
        RecoveryDelayTicks = reader.ReadByte(0x0D);
        WeaponEffectId = reader.ReadByte(0x0E);
        RawHex = Convert.ToHexString(raw);
    }

    public int Index { get; }
    public int EquipmentId => Index + 1;
    public ushort NameOffset { get; }
    public ushort NameSegment { get; }
    public string Name { get; }
    public byte Heat { get; }
    public byte Damage { get; }
    public byte MinimumRangeSquares { get; }
    public byte MaximumRangeUnits { get; }
    public int MaximumRangeSquares => MaximumRangeUnits * 3;
    public byte WeightHalfTons { get; }
    public decimal WeightTons => WeightHalfTons / 2m;
    public byte Reserved09 { get; }
    public byte MissileRackSize { get; }
    public byte AmmunitionFamilyId { get; }
    public int? AmmunitionFamilyIndex => AmmunitionFamilyId == 0 ? null : AmmunitionFamilyId - 1;
    public byte CriticalSlotCount { get; }
    public byte RecoveryDelayTicks { get; }
    public byte WeaponEffectId { get; }
    public string RawHex { get; }

    internal static WeaponDefinition Parse(byte[] raw, int index, string name) => new(raw, index, name);
}

public sealed class WeaponCatalog
{
    public const ushort DataImageSegment = 0x326A;
    public const ushort TableOffset = 0x0435;
    public const int Count = 23;
    public const ushort AmmoMultiplierTableOffset = 0x058E;
    public const int AmmoFamilyCount = 14;

    private WeaponCatalog(IReadOnlyList<WeaponDefinition> weapons, IReadOnlyList<ushort> ammoCapacityMultipliers)
    {
        Weapons = weapons;
        AmmoCapacityMultipliers = ammoCapacityMultipliers;
    }

    public IReadOnlyList<WeaponDefinition> Weapons { get; }
    public IReadOnlyList<ushort> AmmoCapacityMultipliers { get; }

    public static WeaponCatalog Parse(byte[] executable, string sourceName = "REVENGE.EXE")
    {
        MzExecutable mz = MzExecutable.Parse(executable, sourceName);
        var weapons = new List<WeaponDefinition>(Count);
        for (int index = 0; index < Count; index++)
        {
            ushort recordOffset = checked((ushort)(TableOffset + index * WeaponDefinition.Length));
            byte[] raw = mz.ReadImageBytes(DataImageSegment, recordOffset,
                WeaponDefinition.Length, $"weapon definition {index}");
            var reader = new BoundedBinaryReader(raw, $"weapon definition {index}");
            ushort nameOffset = reader.ReadUInt16LittleEndian(0);
            ushort nameSegment = reader.ReadUInt16LittleEndian(2);
            string name = mz.ReadImageAsciiZ(nameSegment, nameOffset);
            weapons.Add(WeaponDefinition.Parse(raw, index, name));
        }

        byte[] multipliers = mz.ReadImageBytes(DataImageSegment, AmmoMultiplierTableOffset,
            AmmoFamilyCount * 2, "ammunition capacity multiplier table");
        var multiplierReader = new BoundedBinaryReader(multipliers, "ammunition capacity multiplier table");
        ushort[] values = Enumerable.Range(0, AmmoFamilyCount)
            .Select(index => multiplierReader.ReadUInt16LittleEndian(index * 2)).ToArray();
        return new WeaponCatalog(weapons.AsReadOnly(), Array.AsReadOnly(values));
    }
}
