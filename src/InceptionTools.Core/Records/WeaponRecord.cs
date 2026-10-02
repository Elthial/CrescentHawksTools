using InceptionTools.Binary;

namespace InceptionTools.Records;

public sealed class WeaponRecord
{
    public const int Length = 0x11;
    private readonly byte[] _rawBytes;

    private WeaponRecord(BoundedBinaryReader reader, int tableIndex)
    {
        reader.RequireExactLength(Length, "Weapon record");
        _rawBytes = reader.ReadBytes(0, Length, "weapon record");
        TableIndex = tableIndex;
        InfantryEquipmentId = tableIndex <= 0x0E ? tableIndex : (int?)null;
        MechComponentId = tableIndex >= 0x0F ? tableIndex + 1 : (int?)null;
        Name = reader.ReadFixedAscii(0, 11, true, "name");
        DamageEncoding = reader.ReadByte(0x0B, "damage encoding");
        AttackCountOrClusterColumnRaw = reader.ReadByte(0x0C, "attack count or cluster column");
        UsesPersonnelDamageEncoding = (AttackCountOrClusterColumnRaw & 0x80) != 0;
        SelectorValue = AttackCountOrClusterColumnRaw & 0x7F;
        PersonnelDiceCount = UsesPersonnelDamageEncoding ? DamageEncoding >> 4 : (int?)null;
        PersonnelFixedDamageBonus = UsesPersonnelDamageEncoding ? DamageEncoding & 0x0F : (int?)null;
        MissileClusterColumn = !UsesPersonnelDamageEncoding && SelectorValue >= 2 ? SelectorValue : (int?)null;
        HeatAndEffectRaw = reader.ReadByte(0x0D, "heat and effect");
        Heat = HeatAndEffectRaw & 0x0F;
        UnknownHeatEffectHighNibble = HeatAndEffectRaw >> 4;
        PackedRangeThresholds = reader.ReadByte(0x0E, "packed range thresholds");
        StoredShortRangeThreshold = PackedRangeThresholds >> 5;
        StoredMediumRangeThreshold = PackedRangeThresholds & 0x1F;
        RangeThresholdScale = !UsesPersonnelDamageEncoding && tableIndex != 0x20 ? 3 : 1;
        EffectiveShortRangeThreshold = StoredShortRangeThreshold * RangeThresholdScale;
        EffectiveMediumRangeThreshold = StoredMediumRangeThreshold * RangeThresholdScale;
        MaximumRange = reader.ReadByte(0x0F, "maximum range");
        SkillIndex = reader.ReadByte(0x10, "skill index");
    }

    public int TableIndex { get; }
    public int? InfantryEquipmentId { get; }
    public int? MechComponentId { get; }
    public string Name { get; }
    public int DamageEncoding { get; }
    public int AttackCountOrClusterColumnRaw { get; }
    public bool UsesPersonnelDamageEncoding { get; }
    public int SelectorValue { get; }
    public int? PersonnelDiceCount { get; }
    public int? PersonnelFixedDamageBonus { get; }
    public int? MissileClusterColumn { get; }
    public int HeatAndEffectRaw { get; }
    public int Heat { get; }
    public int UnknownHeatEffectHighNibble { get; }
    public int PackedRangeThresholds { get; }
    public int StoredShortRangeThreshold { get; }
    public int StoredMediumRangeThreshold { get; }
    public int RangeThresholdScale { get; }
    public int EffectiveShortRangeThreshold { get; }
    public int EffectiveMediumRangeThreshold { get; }
    public int MaximumRange { get; }
    public int SkillIndex { get; }
    public byte[] RawBytes => (byte[])_rawBytes.Clone();

    public static WeaponRecord Parse(byte[] data, int tableIndex, string sourceName = "weapon record")
    {
        if (tableIndex < 0)
            throw new System.ArgumentOutOfRangeException(nameof(tableIndex));
        return new WeaponRecord(new BoundedBinaryReader(data, sourceName), tableIndex);
    }
}
