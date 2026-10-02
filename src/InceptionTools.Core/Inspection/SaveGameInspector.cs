using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using InceptionTools.Installation;
using InceptionTools.Records;

namespace InceptionTools.Inspection;

public sealed class CharacterRecordDump
{
    public string Group { get; set; } = string.Empty;
    public int Slot { get; set; }
    public int FileOffset { get; set; }
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

public sealed class MechRecordDump
{
    public string Group { get; set; } = string.Empty;
    public int Slot { get; set; }
    public int FileOffset { get; set; }
    public string Name { get; set; } = string.Empty;
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

public sealed class SaveGameDump
{
    public string FileName { get; set; } = string.Empty;
    public int FileLength { get; set; }
    public int Header { get; set; }
    public bool HeaderMatchesObservedProfile { get; set; }
    public List<CharacterRecordDump> Characters { get; set; } = new List<CharacterRecordDump>();
    public List<MechRecordDump> Mechs { get; set; } = new List<MechRecordDump>();
    public int ProbableWorldMapVisibilityBitCount { get; set; }
    public int StoryStateAt0CF9 { get; set; }
    public uint Credits { get; set; }
    public uint Stock0 { get; set; }
    public uint Stock1 { get; set; }
    public uint Stock2 { get; set; }
    public int PartyMapX { get; set; }
    public int PartyMapY { get; set; }
}

public static class SaveGameInspector
{
    public const int SaveLength = SaveGameRecord.Length;
    public const int CharacterRecordLength = CharacterRecord.Length;
    public const int MechRecordLength = MechRecord.Length;

    public static SaveGameDump Inspect(GameInstallation installation, string fileName)
    {
        string path = installation.ResolveFile(fileName);
        byte[] data = File.ReadAllBytes(path);
        SaveGameRecord save = SaveGameRecord.Parse(data, Path.GetFileName(path));

        var result = new SaveGameDump
        {
            FileName = Path.GetFileName(path),
            FileLength = SaveGameRecord.Length,
            Header = save.Header,
            HeaderMatchesObservedProfile = save.HeaderMatchesObservedProfile,
            ProbableWorldMapVisibilityBitCount = CountSetBits(save.ProbableWorldMapVisibility),
            StoryStateAt0CF9 = save.StoryStateAt0CF9,
            Credits = save.Credits,
            Stock0 = save.Stock0,
            Stock1 = save.Stock1,
            Stock2 = save.Stock2,
            PartyMapX = save.PartyMapX,
            PartyMapY = save.PartyMapY
        };

        for (int slot = 0; slot < 8; slot++)
            result.Characters.Add(ReadCharacter(save.PlayerCharacters[slot], "player", slot, 0x0001 + slot * CharacterRecordLength));
        for (int slot = 0; slot < 8; slot++)
            result.Characters.Add(ReadCharacter(save.EnemyCharacters[slot], "enemy", slot, 0x0089 + slot * CharacterRecordLength));
        for (int slot = 0; slot < 4; slot++)
            result.Mechs.Add(ReadMech(save.PlayerMechs[slot], "player", slot, 0x0111 + slot * MechRecordLength));
        for (int slot = 0; slot < 4; slot++)
            result.Mechs.Add(ReadMech(save.EnemyMechs[slot], "enemy", slot, 0x0305 + slot * MechRecordLength));

        return result;
    }

    public static string WriteJson(SaveGameDump dump)
    {
        return JsonSerializer.Serialize(dump, new JsonSerializerOptions { WriteIndented = true });
    }

    public static CharacterRecordDump InspectCharacter(GameInstallation installation, string fileName, string group, int slot)
    {
        ValidateSelection(group, slot, 8, "character");
        SaveGameDump dump = Inspect(installation, fileName);
        return dump.Characters.Single(character =>
            character.Group.Equals(group, StringComparison.OrdinalIgnoreCase) && character.Slot == slot);
    }

    public static MechRecordDump InspectMech(GameInstallation installation, string fileName, string group, int slot)
    {
        ValidateSelection(group, slot, 4, "mech");
        SaveGameDump dump = Inspect(installation, fileName);
        return dump.Mechs.Single(mech =>
            mech.Group.Equals(group, StringComparison.OrdinalIgnoreCase) && mech.Slot == slot);
    }

    public static string WriteCharacterJson(CharacterRecordDump character)
    {
        return JsonSerializer.Serialize(character, new JsonSerializerOptions { WriteIndented = true });
    }

    public static string WriteMechJson(MechRecordDump mech)
    {
        return JsonSerializer.Serialize(mech, new JsonSerializerOptions { WriteIndented = true });
    }

    public static string WriteCharacterText(CharacterRecordDump character)
    {
        var output = new StringBuilder();
        AppendCharacter(output, character);
        return output.ToString();
    }

    public static string WriteMechText(MechRecordDump mech)
    {
        var output = new StringBuilder();
        AppendMech(output, mech);
        return output.ToString();
    }

    public static string WriteText(SaveGameDump dump)
    {
        var output = new StringBuilder();
        output.AppendLine("Save: " + dump.FileName + " (0x" + dump.FileLength.ToString("X") + " bytes)");
        output.AppendLine("Header: 0x" + dump.Header.ToString("X2") +
            (dump.HeaderMatchesObservedProfile ? " (observed profile)" : " (UNRECOGNIZED)"));
        output.AppendLine("Position: X=0x" + dump.PartyMapX.ToString("X4") + " Y=0x" + dump.PartyMapY.ToString("X4"));
        output.AppendLine("Credits: " + dump.Credits + "; stocks: " + dump.Stock0 + ", " + dump.Stock1 + ", " + dump.Stock2);
        output.AppendLine("State[file+0x0CF9]: 0x" + dump.StoryStateAt0CF9.ToString("X2"));
        output.AppendLine("Probable world-map visibility bits set: " + dump.ProbableWorldMapVisibilityBitCount);
        output.AppendLine();
        output.AppendLine("CHARACTERS");
        foreach (CharacterRecordDump character in dump.Characters)
            AppendCharacter(output, character);
        output.AppendLine();
        output.AppendLine("MECHS");
        foreach (MechRecordDump mech in dump.Mechs)
            AppendMech(output, mech);
        return output.ToString();
    }

    private static void AppendCharacter(StringBuilder output, CharacterRecordDump character)
    {
        output.Append(character.Group).Append('[').Append(character.Slot).Append("] @0x")
            .Append(character.FileOffset.ToString("X4")).Append(" nameId=0x")
            .Append(character.NameId.ToString("X2")).Append(" body=").Append(character.Body)
            .Append(" dex=").Append(character.Dexterity).Append(" cha=").Append(character.Charisma)
            .Append(" skills=").Append(string.Join(",", character.Skills))
            .Append(" weapon=").Append(character.WeaponTableIndex)
            .Append(" mech=").Append(character.MechAssignment)
            .Append(" armour=").Append(character.ArmourType).Append('/').Append(character.ArmourValue)
            .Append(" health=").Append(character.Health)
            .Append(" training=0x").Append(character.TrainingFlags.ToString("X2")).AppendLine();
    }

    private static void AppendMech(StringBuilder output, MechRecordDump mech)
    {
        output.Append(mech.Group).Append('[').Append(mech.Slot).Append("] @0x")
            .Append(mech.FileOffset.ToString("X4")).Append(" name=\"").Append(mech.Name)
            .Append("\" tons=").Append(mech.Tonnage).Append(" walk=").Append(mech.WalkMove)
            .Append(" jump=").Append(mech.JumpMove).Append(" pilot=").Append(mech.PilotId)
            .Append(" rider=").Append(mech.RiderId).Append(" hits(E/G/S)=")
            .Append(mech.EngineHits).Append('/').Append(mech.GyroHits).Append('/').Append(mech.SensorHits)
            .Append(" upgrade=0x").Append(mech.UpgradeLevelFlags.ToString("X2")).AppendLine();
    }

    private static void ValidateSelection(string group, int slot, int slotCount, string recordType)
    {
        if (!string.Equals(group, "player", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(group, "enemy", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Group must be 'player' or 'enemy'.", nameof(group));
        if (slot < 0 || slot >= slotCount)
            throw new ArgumentOutOfRangeException(nameof(slot),
                recordType + " slot must be between 0 and " + (slotCount - 1) + ".");
    }

    private static CharacterRecordDump ReadCharacter(CharacterRecord record, string group, int slot, int offset)
    {
        return new CharacterRecordDump
        {
            Group = group,
            Slot = slot,
            FileOffset = offset,
            NameId = record.NameId,
            Body = record.Body,
            Dexterity = record.Dexterity,
            Charisma = record.Charisma,
            Skills = record.Skills.Select(value => (int)value).ToArray(),
            WeaponTableIndex = record.WeaponTableIndex,
            MechAssignment = record.MechAssignment,
            ArmourType = record.ArmourType,
            ArmourValue = record.ArmourValue,
            Health = record.Health,
            TrainingFlags = record.TrainingFlags
        };
    }

    private static MechRecordDump ReadMech(MechRecord record, string group, int slot, int offset)
    {
        return new MechRecordDump
        {
            Group = group,
            Slot = slot,
            FileOffset = offset,
            Name = record.Name,
            Tonnage = record.Tonnage,
            CurrentArmour = ToIntArray(record.CurrentArmour),
            CurrentStructure = ToIntArray(record.CurrentStructure),
            CurrentActuatorByte24 = record.ActuatorByte24.CurrentRaw,
            CurrentActuatorByte25 = record.ActuatorByte25.CurrentRaw,
            EngineHeatSinks = record.EngineHeatSinks,
            CurrentAmmo = ToIntArray(record.CurrentAmmo),
            WalkMove = record.WalkMove,
            JumpMove = record.JumpMove,
            CriticalSlotsRaw = ToIntArray(record.CriticalSlotsRaw),
            MaximumArmour = ToIntArray(record.MaximumArmour),
            MaximumStructure = ToIntArray(record.MaximumStructure),
            MaximumActuatorByte69 = record.ActuatorByte24.MaximumRaw,
            MaximumActuatorByte6A = record.ActuatorByte25.MaximumRaw,
            MaximumAmmo = ToIntArray(record.MaximumAmmo),
            EngineHits = record.EngineHits,
            GyroHits = record.GyroHits,
            SensorHits = record.SensorHits,
            Byte78ProbableLifeSupportState = record.Byte78ProbableLifeSupportState,
            PilotId = record.PilotId,
            RiderId = record.RiderId,
            Byte7BProbableUpgradePackageBase = record.Byte7BProbableUpgradePackageBase,
            UpgradeLevelFlags = record.UpgradeLevelFlags
        };
    }

    private static int[] ToIntArray(byte[] values)
    {
        return values.Select(value => (int)value).ToArray();
    }

    private static int CountSetBits(byte[] values)
    {
        int total = 0;
        foreach (byte sourceValue in values)
        {
            int value = sourceValue;
            while (value != 0)
            {
                total += value & 1;
                value >>= 1;
            }
        }
        return total;
    }
}
