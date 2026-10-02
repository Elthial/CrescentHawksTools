using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using InceptionTools.Records;

namespace InceptionTools.SaveEditing;

/// <summary>
/// Reusable fetch/update API. UpdateState starts with a clone of the exact
/// original save and changes only typed fields whose values differ.
/// </summary>
public static class SaveStateEditor
{
    private const int PlayerCharacterOffset = 0x0001;
    private const int EnemyCharacterOffset = 0x0089;
    private const int PlayerMechOffset = 0x0111;
    private const int EnemyMechOffset = 0x0305;

    public static SaveState FetchState(byte[] saveBytes, string sourceName = "save file")
    {
        if (saveBytes == null)
            throw new ArgumentNullException(nameof(saveBytes));

        SaveGameRecord save = SaveGameRecord.Parse(saveBytes, sourceName);
        var state = new SaveState
        {
            SourceFileName = Path.GetFileName(sourceName),
            SourceSha256 = ComputeSha256(saveBytes),
            Header = save.Header,
            StoryStateAt0CF9 = save.StoryStateAt0CF9,
            Credits = save.Credits,
            Stock0 = save.Stock0,
            Stock1 = save.Stock1,
            Stock2 = save.Stock2,
            PartyMapX = save.PartyMapX,
            PartyMapY = save.PartyMapY
        };

        AddCharacters(state.Characters, save.PlayerCharacters, "player");
        AddCharacters(state.Characters, save.EnemyCharacters, "enemy");
        AddMechs(state.Mechs, save.PlayerMechs, "player");
        AddMechs(state.Mechs, save.EnemyMechs, "enemy");
        return state;
    }

    public static SaveStateUpdateResult UpdateState(byte[] originalBytes, SaveState editedState,
        bool requireSourceFingerprint = true)
    {
        if (originalBytes == null)
            throw new ArgumentNullException(nameof(originalBytes));
        if (editedState == null)
            throw new ArgumentNullException(nameof(editedState));

        SaveState originalState = FetchState(originalBytes, editedState.SourceFileName ?? "save file");
        if (!originalState.Header.Equals(0x0C))
            throw new InvalidDataException("Save header is not the observed 0x0C profile; update refused.");
        if (requireSourceFingerprint && !string.Equals(editedState.SourceSha256,
            originalState.SourceSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Text state fingerprint does not match the source save.");

        ValidateShape(editedState);
        byte[] output = (byte[])originalBytes.Clone();
        var changes = new List<SaveStateChange>();

        PatchByte(output, 0x0000, editedState.Header, originalState.Header, "save.header", changes);
        PatchByte(output, 0x0CF9, editedState.StoryStateAt0CF9, originalState.StoryStateAt0CF9,
            "save.story_state_0cf9", changes);
        PatchUInt32(output, 0x0D5D, editedState.Credits, originalState.Credits, "save.credits", changes);
        PatchUInt32(output, 0x0D61, editedState.Stock0, originalState.Stock0, "save.stock_0", changes);
        PatchUInt32(output, 0x0D65, editedState.Stock1, originalState.Stock1, "save.stock_1", changes);
        PatchUInt32(output, 0x0D69, editedState.Stock2, originalState.Stock2, "save.stock_2", changes);
        PatchUInt16(output, 0x0F45, editedState.PartyMapX, originalState.PartyMapX, "save.party_map_x", changes);
        PatchUInt16(output, 0x0F47, editedState.PartyMapY, originalState.PartyMapY, "save.party_map_y", changes);

        for (int index = 0; index < editedState.Characters.Count; index++)
        {
            SaveCharacterState edited = editedState.Characters[index];
            SaveCharacterState original = FindCharacter(originalState, edited.Group, edited.Slot);
            int offset = CharacterOffset(edited.Group, edited.Slot);
            string prefix = "character." + edited.Group + "." + edited.Slot + ".";
            PatchCharacter(output, offset, prefix, original, edited, changes);
        }

        for (int index = 0; index < editedState.Mechs.Count; index++)
        {
            SaveMechState edited = editedState.Mechs[index];
            SaveMechState original = FindMech(originalState, edited.Group, edited.Slot);
            int offset = MechOffset(edited.Group, edited.Slot);
            string prefix = "mech." + edited.Group + "." + edited.Slot + ".";
            PatchMech(output, offset, prefix, original, edited, changes);
        }

        SaveGameRecord.Parse(output, "updated save");
        return new SaveStateUpdateResult(output, changes);
    }

    public static string ComputeSha256(byte[] bytes)
    {
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private static void AddCharacters(List<SaveCharacterState> destination,
        IReadOnlyList<CharacterRecord> records, string group)
    {
        for (int slot = 0; slot < records.Count; slot++)
        {
            CharacterRecord record = records[slot];
            destination.Add(new SaveCharacterState
            {
                Group = group,
                Slot = slot,
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
            });
        }
    }

    private static void AddMechs(List<SaveMechState> destination,
        IReadOnlyList<MechRecord> records, string group)
    {
        for (int slot = 0; slot < records.Count; slot++)
        {
            MechRecord record = records[slot];
            destination.Add(new SaveMechState
            {
                Group = group,
                Slot = slot,
                Name = record.Name,
                NameFirstByteRaw = record.NameFirstByteRaw,
                Active = !record.HasProbableNoMechOrDestroyedMarker,
                Tonnage = record.Tonnage,
                CurrentArmour = ToInts(record.CurrentArmour),
                CurrentStructure = ToInts(record.CurrentStructure),
                CurrentActuatorByte24 = record.ActuatorByte24.CurrentRaw,
                CurrentActuatorByte25 = record.ActuatorByte25.CurrentRaw,
                EngineHeatSinks = record.EngineHeatSinks,
                CurrentAmmo = ToInts(record.CurrentAmmo),
                WalkMove = record.WalkMove,
                JumpMove = record.JumpMove,
                CriticalSlotsRaw = ToInts(record.CriticalSlotsRaw),
                MaximumArmour = ToInts(record.MaximumArmour),
                MaximumStructure = ToInts(record.MaximumStructure),
                MaximumActuatorByte69 = record.ActuatorByte24.MaximumRaw,
                MaximumActuatorByte6A = record.ActuatorByte25.MaximumRaw,
                MaximumAmmo = ToInts(record.MaximumAmmo),
                EngineHits = record.EngineHits,
                GyroHits = record.GyroHits,
                SensorHits = record.SensorHits,
                Byte78ProbableLifeSupportState = record.Byte78ProbableLifeSupportState,
                PilotId = record.PilotId,
                RiderId = record.RiderId,
                Byte7BProbableUpgradePackageBase = record.Byte7BProbableUpgradePackageBase,
                UpgradeLevelFlags = record.UpgradeLevelFlags
            });
        }
    }

    private static void ValidateShape(SaveState state)
    {
        if (state.Characters.Count != 16)
            throw new InvalidDataException("State must contain exactly 16 character records.");
        if (state.Mechs.Count != 8)
            throw new InvalidDataException("State must contain exactly 8 mech records.");
        ValidateUniqueKeys(state.Characters.Select(item => item.Group + ":" + item.Slot), 16, "character");
        ValidateUniqueKeys(state.Mechs.Select(item => item.Group + ":" + item.Slot), 8, "mech");
    }

    private static void ValidateUniqueKeys(IEnumerable<string> keys, int expectedCount, string label)
    {
        if (keys.Distinct(StringComparer.OrdinalIgnoreCase).Count() != expectedCount)
            throw new InvalidDataException("State contains duplicate " + label + " group/slot entries.");
    }

    private static SaveCharacterState FindCharacter(SaveState state, string group, int slot)
    {
        return state.Characters.Single(item =>
            item.Group.Equals(group, StringComparison.OrdinalIgnoreCase) && item.Slot == slot);
    }

    private static SaveMechState FindMech(SaveState state, string group, int slot)
    {
        return state.Mechs.Single(item =>
            item.Group.Equals(group, StringComparison.OrdinalIgnoreCase) && item.Slot == slot);
    }

    private static int CharacterOffset(string group, int slot)
    {
        ValidateGroupAndSlot(group, slot, 8, "character");
        return (group.Equals("player", StringComparison.OrdinalIgnoreCase)
            ? PlayerCharacterOffset : EnemyCharacterOffset) + slot * CharacterRecord.Length;
    }

    private static int MechOffset(string group, int slot)
    {
        ValidateGroupAndSlot(group, slot, 4, "mech");
        return (group.Equals("player", StringComparison.OrdinalIgnoreCase)
            ? PlayerMechOffset : EnemyMechOffset) + slot * MechRecord.Length;
    }

    private static void ValidateGroupAndSlot(string group, int slot, int count, string label)
    {
        if (!string.Equals(group, "player", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(group, "enemy", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(label + " group must be player or enemy.");
        if (slot < 0 || slot >= count)
            throw new InvalidDataException(label + " slot is outside 0.." + (count - 1) + ".");
    }

    private static void PatchCharacter(byte[] bytes, int offset, string prefix,
        SaveCharacterState original, SaveCharacterState edited, List<SaveStateChange> changes)
    {
        PatchByte(bytes, offset + 0x00, edited.NameId, original.NameId, prefix + "name_id", changes);
        PatchByte(bytes, offset + 0x01, edited.Body, original.Body, prefix + "body", changes);
        PatchByte(bytes, offset + 0x02, edited.Dexterity, original.Dexterity, prefix + "dexterity", changes);
        PatchByte(bytes, offset + 0x03, edited.Charisma, original.Charisma, prefix + "charisma", changes);
        PatchArray(bytes, offset + 0x04, edited.Skills, original.Skills, 7, prefix + "skills", changes);
        PatchByte(bytes, offset + 0x0B, edited.WeaponTableIndex, original.WeaponTableIndex,
            prefix + "weapon_table_index", changes);
        PatchByte(bytes, offset + 0x0C, edited.MechAssignment, original.MechAssignment,
            prefix + "mech_assignment", changes);
        PatchByte(bytes, offset + 0x0D, edited.ArmourType, original.ArmourType, prefix + "armour_type", changes);
        PatchByte(bytes, offset + 0x0E, edited.ArmourValue, original.ArmourValue, prefix + "armour_value", changes);
        PatchByte(bytes, offset + 0x0F, edited.Health, original.Health, prefix + "health", changes);
        PatchByte(bytes, offset + 0x10, edited.TrainingFlags, original.TrainingFlags,
            prefix + "training_flags", changes);
    }

    private static void PatchMech(byte[] bytes, int offset, string prefix,
        SaveMechState original, SaveMechState edited, List<SaveStateChange> changes)
    {
        ValidateName(edited.Name, prefix + "name");
        bool nameChanged = !string.Equals(edited.Name, original.Name, StringComparison.Ordinal);
        bool activeChanged = edited.Active != original.Active;
        if ((nameChanged || activeChanged) && edited.Active && edited.Name.Length == 0)
            throw new InvalidDataException(prefix + "active cannot be true without a mech name.");
        if (nameChanged)
        {
            byte[] encoded = new byte[16];
            for (int index = 0; index < 15; index++)
                encoded[index] = (byte)' ';
            Encoding.ASCII.GetBytes(edited.Name, 0, edited.Name.Length, encoded, 0);
            if (!edited.Active)
                encoded[0] = 0xFF;
            PatchRawArray(bytes, offset, encoded, prefix + "name", changes);
        }
        else if (activeChanged)
        {
            int firstByte = bytes[offset];
            byte changedFirstByte;
            if (!edited.Active)
                changedFirstByte = 0xFF;
            else if (firstByte != 0xFF)
                changedFirstByte = (byte)(firstByte & 0x7F);
            else if (NameSuffixMatches(bytes, offset, edited.Name))
                changedFirstByte = (byte)edited.Name[0];
            else
                throw new InvalidDataException(prefix +
                    "active cannot restore an 0xFF first byte unless the complete name matches its stored suffix.");
            PatchRawByte(bytes, offset, changedFirstByte, prefix + "active", changes);
        }
        PatchByte(bytes, offset + 0x10, edited.Tonnage, original.Tonnage, prefix + "tonnage", changes);
        PatchArray(bytes, offset + 0x11, edited.CurrentArmour, original.CurrentArmour, 11,
            prefix + "current_armour", changes);
        PatchArray(bytes, offset + 0x1C, edited.CurrentStructure, original.CurrentStructure, 8,
            prefix + "current_structure", changes);
        PatchByte(bytes, offset + 0x24, edited.CurrentActuatorByte24, original.CurrentActuatorByte24,
            prefix + "current_actuator_byte_24", changes);
        PatchByte(bytes, offset + 0x25, edited.CurrentActuatorByte25, original.CurrentActuatorByte25,
            prefix + "current_actuator_byte_25", changes);
        PatchByte(bytes, offset + 0x26, edited.EngineHeatSinks, original.EngineHeatSinks,
            prefix + "engine_heat_sinks", changes);
        PatchArray(bytes, offset + 0x27, edited.CurrentAmmo, original.CurrentAmmo, 10,
            prefix + "current_ammo", changes);
        PatchByte(bytes, offset + 0x31, edited.WalkMove, original.WalkMove, prefix + "walk_move", changes);
        PatchByte(bytes, offset + 0x32, edited.JumpMove, original.JumpMove, prefix + "jump_move", changes);
        PatchArray(bytes, offset + 0x33, edited.CriticalSlotsRaw, original.CriticalSlotsRaw, 0x23,
            prefix + "critical_slots_raw", changes);
        PatchArray(bytes, offset + 0x56, edited.MaximumArmour, original.MaximumArmour, 11,
            prefix + "maximum_armour", changes);
        PatchArray(bytes, offset + 0x61, edited.MaximumStructure, original.MaximumStructure, 8,
            prefix + "maximum_structure", changes);
        PatchByte(bytes, offset + 0x69, edited.MaximumActuatorByte69, original.MaximumActuatorByte69,
            prefix + "maximum_actuator_byte_69", changes);
        PatchByte(bytes, offset + 0x6A, edited.MaximumActuatorByte6A, original.MaximumActuatorByte6A,
            prefix + "maximum_actuator_byte_6a", changes);
        PatchArray(bytes, offset + 0x6B, edited.MaximumAmmo, original.MaximumAmmo, 10,
            prefix + "maximum_ammo", changes);
        PatchByte(bytes, offset + 0x75, edited.EngineHits, original.EngineHits, prefix + "engine_hits", changes);
        PatchByte(bytes, offset + 0x76, edited.GyroHits, original.GyroHits, prefix + "gyro_hits", changes);
        PatchByte(bytes, offset + 0x77, edited.SensorHits, original.SensorHits, prefix + "sensor_hits", changes);
        PatchByte(bytes, offset + 0x78, edited.Byte78ProbableLifeSupportState,
            original.Byte78ProbableLifeSupportState, prefix + "byte_78_probable_life_support_state", changes);
        PatchByte(bytes, offset + 0x79, edited.PilotId, original.PilotId, prefix + "pilot_id", changes);
        PatchByte(bytes, offset + 0x7A, edited.RiderId, original.RiderId, prefix + "rider_id", changes);
        PatchByte(bytes, offset + 0x7B, edited.Byte7BProbableUpgradePackageBase,
            original.Byte7BProbableUpgradePackageBase, prefix + "byte_7b_probable_upgrade_package_base", changes);
        PatchByte(bytes, offset + 0x7C, edited.UpgradeLevelFlags, original.UpgradeLevelFlags,
            prefix + "upgrade_level_flags", changes);
    }

    private static void ValidateName(string value, string field)
    {
        if (value == null)
            throw new InvalidDataException(field + " cannot be null.");
        if (value.Length > 16 || value.Any(character => character < 0x20 || character > 0x7E))
            throw new InvalidDataException(field + " must be at most 16 printable ASCII characters.");
    }

    private static bool NameSuffixMatches(byte[] bytes, int offset, string completeName)
    {
        if (completeName.Length < 2)
            return false;
        int suffixLength = 0;
        while (suffixLength < 15 && bytes[offset + 1 + suffixLength] != 0 &&
            bytes[offset + 1 + suffixLength] != (byte)' ')
            suffixLength++;
        string storedSuffix = Encoding.ASCII.GetString(bytes, offset + 1, suffixLength);
        return completeName.Substring(1).Equals(storedSuffix, StringComparison.Ordinal);
    }

    private static void PatchByte(byte[] bytes, int offset, int edited, int original,
        string field, List<SaveStateChange> changes)
    {
        if (edited < byte.MinValue || edited > byte.MaxValue)
            throw new InvalidDataException(field + " must be between 0 and 255.");
        if (edited != original)
            PatchRawByte(bytes, offset, (byte)edited, field, changes);
    }

    private static void PatchUInt16(byte[] bytes, int offset, int edited, int original,
        string field, List<SaveStateChange> changes)
    {
        if (edited < ushort.MinValue || edited > ushort.MaxValue)
            throw new InvalidDataException(field + " must be between 0 and 65535.");
        if (edited == original)
            return;
        PatchRawByte(bytes, offset, (byte)edited, field, changes);
        PatchRawByte(bytes, offset + 1, (byte)(edited >> 8), field, changes);
    }

    private static void PatchUInt32(byte[] bytes, int offset, uint edited, uint original,
        string field, List<SaveStateChange> changes)
    {
        if (edited == original)
            return;
        for (int index = 0; index < 4; index++)
            PatchRawByte(bytes, offset + index, (byte)(edited >> (index * 8)), field, changes);
    }

    private static void PatchArray(byte[] bytes, int offset, int[] edited, int[] original,
        int expectedLength, string field, List<SaveStateChange> changes)
    {
        if (edited == null || edited.Length != expectedLength)
            throw new InvalidDataException(field + " must contain exactly " + expectedLength + " values.");
        for (int index = 0; index < edited.Length; index++)
        {
            int oldValue = original[index];
            PatchByte(bytes, offset + index, edited[index], oldValue, field + "[" + index + "]", changes);
        }
    }

    private static void PatchRawArray(byte[] bytes, int offset, byte[] values,
        string field, List<SaveStateChange> changes)
    {
        for (int index = 0; index < values.Length; index++)
            PatchRawByte(bytes, offset + index, values[index], field + "[" + index + "]", changes);
    }

    private static void PatchRawByte(byte[] bytes, int offset, byte value,
        string field, List<SaveStateChange> changes)
    {
        if (bytes[offset] == value)
            return;
        changes.Add(new SaveStateChange(field, offset, bytes[offset], value));
        bytes[offset] = value;
    }

    private static int[] ToInts(byte[] values)
    {
        return values.Select(value => (int)value).ToArray();
    }
}
