using System.Collections.Generic;

namespace InceptionTools.SaveEditing;

/// <summary>
/// Editable, typed view of the verified fields in one save. Unknown save
/// bytes deliberately do not appear here; SaveStateEditor.UpdateState
/// patches this view over the original byte-for-byte source.
/// </summary>
public sealed class SaveState
{
    public const string TextFormatId = "BTCHI_SAVE_STATE_V1";

    public string SourceFileName { get; set; } = string.Empty;
    public string SourceSha256 { get; set; } = string.Empty;
    public int Header { get; set; }
    public int StoryStateAt0CF9 { get; set; }
    public uint Credits { get; set; }
    public uint Stock0 { get; set; }
    public uint Stock1 { get; set; }
    public uint Stock2 { get; set; }
    public int PartyMapX { get; set; }
    public int PartyMapY { get; set; }
    public List<SaveCharacterState> Characters { get; } = new List<SaveCharacterState>();
    public List<SaveMechState> Mechs { get; } = new List<SaveMechState>();
}

public sealed class SaveCharacterState
{
    public string Group { get; set; } = string.Empty;
    public int Slot { get; set; }
    public int NameId { get; set; }
    public int Body { get; set; }
    public int Dexterity { get; set; }
    public int Charisma { get; set; }
    public int[] Skills { get; set; } = [];
    public int WeaponTableIndex { get; set; }
    public int MechAssignment { get; set; }
    public int ArmourType { get; set; }
    public int ArmourValue { get; set; }
    public int Health { get; set; }
    public int TrainingFlags { get; set; }
}

public sealed class SaveMechState
{
    public string Group { get; set; } = string.Empty;
    public int Slot { get; set; }
    public string Name { get; set; } = string.Empty;
    public int NameFirstByteRaw { get; internal set; }
    public bool HasProbableNoMechOrDestroyedMarker => (NameFirstByteRaw & 0x80) != 0;
    public bool Active { get; set; }
    public int Tonnage { get; set; }
    public int[] CurrentArmour { get; set; } = [];
    public int[] CurrentStructure { get; set; } = [];
    public int CurrentActuatorByte24 { get; set; }
    public int CurrentActuatorByte25 { get; set; }
    public int EngineHeatSinks { get; set; }
    public int[] CurrentAmmo { get; set; } = [];
    public int WalkMove { get; set; }
    public int JumpMove { get; set; }
    public int[] CriticalSlotsRaw { get; set; } = [];
    public int[] MaximumArmour { get; set; } = [];
    public int[] MaximumStructure { get; set; } = [];
    public int MaximumActuatorByte69 { get; set; }
    public int MaximumActuatorByte6A { get; set; }
    public int[] MaximumAmmo { get; set; } = [];
    public int EngineHits { get; set; }
    public int GyroHits { get; set; }
    public int SensorHits { get; set; }
    public int Byte78ProbableLifeSupportState { get; set; }
    public int PilotId { get; set; }
    public int RiderId { get; set; }
    public int Byte7BProbableUpgradePackageBase { get; set; }
    public int UpgradeLevelFlags { get; set; }
}

public sealed class SaveStateChange
{
    internal SaveStateChange(string field, int fileOffset, byte oldValue, byte newValue)
    {
        Field = field;
        FileOffset = fileOffset;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public string Field { get; }
    public int FileOffset { get; }
    public byte OldValue { get; }
    public byte NewValue { get; }
}

public sealed class SaveStateUpdateResult
{
    private readonly byte[] _bytes;

    internal SaveStateUpdateResult(byte[] bytes, List<SaveStateChange> changes)
    {
        _bytes = (byte[])bytes.Clone();
        Changes = changes.AsReadOnly();
    }

    public byte[] Bytes => (byte[])_bytes.Clone();
    public IReadOnlyList<SaveStateChange> Changes { get; }
}
