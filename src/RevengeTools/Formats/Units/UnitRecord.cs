using RevengeTools.Binary;
using System.Text.Json.Serialization;

namespace RevengeTools.Formats.Units;

public sealed class UnitRecord
{
    public const int Length = 0x96;
    private readonly byte[] _raw;

    private UnitRecord(byte[] raw, int recordIndex)
    {
        _raw = raw;
        RecordIndex = recordIndex;
        var reader = new BoundedBinaryReader(raw, $"unit record {recordIndex}");
        DeploymentState = reader.ReadByte(0x00);
        UnitTypeId = reader.ReadByte(0x01);
        WalkMovement = reader.ReadByte(0x02);
        JumpMovement = reader.ReadByte(0x03);
        CurrentInternal = reader.ReadBytes(0x04, 8);
        CurrentArmor = reader.ReadBytes(0x0C, 11);
        MaximumInternal = reader.ReadBytes(0x17, 8);
        MaximumArmor = reader.ReadBytes(0x1F, 11);
        WeaponFamilyCounts = reader.ReadBytes(0x2A, 14);
        CurrentAmmo = Enumerable.Range(0, 14).Select(index => reader.ReadUInt16LittleEndian(0x38 + index * 2)).ToArray();
        EngineHeatSinkCapacity = reader.ReadByte(0x54);
        CriticalAndEquipmentData = reader.ReadBytes(0x55, 0x2F);
        Tonnage = reader.ReadByte(0x84);
        RuntimeState = reader.ReadBytes(0x85, 6);
        SpecialEquipmentFlags = reader.ReadBytes(0x8B, 7);
        EngineHits = reader.ReadByte(0x85);
        HeatAccumulatorQ8_8 = reader.ReadUInt16LittleEndian(0x86);
        HeatFraction = reader.ReadByte(0x86);
        CurrentHeat = reader.ReadByte(0x87);
        TacticalSpriteBase = reader.ReadByte(0x88);
        SensorHits = reader.ReadByte(0x89);
        GunneryTargetNumber = reader.ReadByte(0x8A);
        TargetMovementModifier = reader.ReadByte(0x8B);
        MissileUpgradeState = reader.ReadByte(0x8C);
        HasBeagleActiveProbe = reader.ReadByte(0x8D) != 0;
        CanSpotIndirectFire = reader.ReadByte(0x8E) != 0;
        HasAccuracyUpgrade = reader.ReadByte(0x8F) != 0;
        HasDoubleHeatSinks = reader.ReadByte(0x90) != 0;
        HasCase = reader.ReadByte(0x91) != 0;
        PilotId = reader.ReadByte(0x92);
        PilotExperience = reader.ReadByte(0x93);
        Allegiance = reader.ReadByte(0x94);
        DamageModel = reader.ReadByte(0x95);
        DamageModelKind = (UnitDamageModel)DamageModel;
        EquipmentLocations = UnitEquipmentLocation.Parse(CriticalAndEquipmentData);
    }

    public int RecordIndex { get; }
    public byte DeploymentState { get; }
    public byte UnitTypeId { get; }
    public string UnitName => UnitCatalog.NameFor(UnitTypeId);
    public UnitKind UnitKind => UnitCatalog.KindFor(UnitTypeId);
    public byte WalkMovement { get; }
    public byte JumpMovement { get; }
    public byte[] CurrentInternal { get; }
    public byte[] CurrentArmor { get; }
    public byte[] MaximumInternal { get; }
    public byte[] MaximumArmor { get; }
    public byte[] WeaponFamilyCounts { get; }
    public ushort[] CurrentAmmo { get; }
    public byte EngineHeatSinkCapacity { get; }
    public byte[] CriticalAndEquipmentData { get; }
    public byte Tonnage { get; }
    public byte[] RuntimeState { get; }
    public byte[] SpecialEquipmentFlags { get; }
    public byte EngineHits { get; }
    public ushort HeatAccumulatorQ8_8 { get; }
    public byte HeatFraction { get; }
    public byte CurrentHeat { get; }
    public byte TacticalSpriteBase { get; }
    public byte SensorHits { get; }
    public byte GunneryTargetNumber { get; }
    public byte TargetMovementModifier { get; }
    public byte MissileUpgradeState { get; }
    public bool HasImprovedMissiles => MissileUpgradeState != 0 && DamageModelKind != UnitDamageModel.Infantry;
    public bool HasInfernoRockets => MissileUpgradeState != 0 && DamageModelKind == UnitDamageModel.Infantry;
    public bool HasBeagleActiveProbe { get; }
    public bool CanSpotIndirectFire { get; }
    public bool HasAccuracyUpgrade { get; }
    public bool HasDoubleHeatSinks { get; }
    public bool HasCase { get; }
    public byte PilotId { get; }
    public byte PilotExperience { get; }
    public byte Allegiance { get; }
    public byte DamageModel { get; }
    public UnitDamageModel DamageModelKind { get; }
    public IReadOnlyList<UnitEquipmentLocation> EquipmentLocations { get; }
    public bool IsPopulated => Tonnage != 0 && UnitTypeId < UnitCatalog.Names.Count;
    [JsonIgnore]
    public byte[] Raw => (byte[])_raw.Clone();

    public static UnitRecord Parse(byte[] data, int offset = 0, int recordIndex = 0, string sourceName = "unit data")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        return new UnitRecord(reader.ReadBytes(offset, Length, $"unit record {recordIndex}"), recordIndex);
    }
}

public enum UnitKind
{
    BattleMech,
    Vehicle,
    Infantry
}

public enum UnitDamageModel : byte
{
    BattleMech = 0x00,
    HoverCombatVehicle = 0x01,
    ConventionalCombatVehicle = 0x02,
    Infantry = 0x03,
    ArmoredPersonnelCarrier = 0x04,
    AmmunitionCarrier = 0x05,
    MobileHeadquarters = 0x06
}

public sealed class UnitEquipmentLocation
{
    private static readonly int[] Offsets = { 0x00, 0x01, 0x09, 0x15, 0x17, 0x23, 0x2B, 0x2D };
    private static readonly int[] Lengths = { 1, 8, 12, 2, 12, 8, 2, 2 };

    public static IReadOnlyList<string> Names { get; } = Array.AsReadOnly(new[]
    {
        "Head", "Left Arm", "Left Torso", "Center Torso",
        "Right Torso", "Right Arm", "Left Leg", "Right Leg"
    });

    private UnitEquipmentLocation(int index, byte[] slots)
    {
        Index = index;
        BattleMechLocation = Names[index];
        Slots = slots;
    }

    public int Index { get; }
    public string BattleMechLocation { get; }
    public byte[] Slots { get; }

    internal static IReadOnlyList<UnitEquipmentLocation> Parse(byte[] data) =>
        Enumerable.Range(0, Offsets.Length)
            .Select(index => new UnitEquipmentLocation(index, data[Offsets[index]..(Offsets[index] + Lengths[index])]))
            .ToArray();
}

public static class UnitCatalog
{
    public static IReadOnlyList<string> Names { get; } = Array.AsReadOnly(new[]
    {
        "Locust", "Wasp", "Stinger", "Commando", "Javelin", "Spider", "UrbanMech", "Valkyrie",
        "Firestarter", "Jenner", "Ostscout", "Panther", "Assassin", "Cicada", "Clint", "Hermes II",
        "Vulcan", "Whitworth", "Blackjack", "Hatchetman", "Phoenix Hawk", "Vindicator", "Centurion",
        "Enforcer", "Hunchback", "Trebuchet", "Dervish", "Griffin", "Shadow Hawk", "Scorpion",
        "Wolverine", "Dragon", "Ostroc", "Ostsol", "Quickdraw", "Rifleman", "Catapult", "Crusader",
        "JagerMech", "Thunderbolt", "Archer", "Grasshopper", "Warhammer", "Marauder", "Orion", "Awesome",
        "Charger", "Goliath", "Victor", "Zeus", "BattleMaster", "Stalker", "Cyclops", "Banshee", "Atlas",
        "Rommel Tank", "Pegasus", "Drillson", "Mobile HQ", "APC", "Ammo Carrier", "Galleon", "Skulker",
        "Infantry", "Jump Infantry", "Elementals", "Puma", "Black Hawk", "Mad Cat", "Locust+", "Locust*",
        "Wasp+", "Wasp*", "Commando+", "Commando*", "Blackjack+", "Phoenix Hawk+", "Phoenix Hawk*",
        "Griffin+", "Griffin*", "Rifleman+", "Rifleman*", "Warhammer+", "Warhammer*", "Marauder+",
        "Jenner+", "Dragon+", "Shadow Hawk+", "BattleMaster+"
    });

    public static string NameFor(int typeId) => typeId >= 0 && typeId < Names.Count ? Names[typeId] : $"unknown-{typeId:X2}";

    public static UnitKind KindFor(int typeId) => typeId switch
    {
        >= 0x37 and <= 0x3E => UnitKind.Vehicle,
        >= 0x3F and <= 0x41 => UnitKind.Infantry,
        _ => UnitKind.BattleMech
    };

    public static IReadOnlyList<UnitRecord> ParseTemplates(byte[] data, string sourceName = "MECHTYPE.DAT")
    {
        var reader = new BoundedBinaryReader(data, sourceName);
        reader.RequireExactLength(Names.Count * UnitRecord.Length, "unit template table");
        return Enumerable.Range(0, Names.Count)
            .Select(index => UnitRecord.Parse(data, index * UnitRecord.Length, index, sourceName)).ToArray();
    }
}
