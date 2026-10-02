using System.Security.Cryptography;
using RevengeTools.Formats.Saves;

namespace RevengeTools.SaveEditing;

public static class SaveStateEditor
{
    public static SaveState FetchState(byte[] saveBytes, int slotNumber, string sourceName = "SAVEGAME.DAT")
    {
        RevengeSaveSlot slot = RevengeSaveFile.Parse(saveBytes, sourceName).GetSlot(slotNumber);
        var state = new SaveState
        {
            SourceFileName = Path.GetFileName(sourceName),
            SourceSha256 = Convert.ToHexString(SHA256.HashData(saveBytes)).ToLowerInvariant(),
            SlotNumber = slotNumber,
            Label = slot.Label,
            CampaignStage = slot.CampaignStage,
            ScenarioVariant = slot.ScenarioVariant,
            CampaignPhase = slot.CampaignPhase,
            TrainingSequenceFlag = slot.TrainingSequenceFlag,
            CampaignFlags = slot.CampaignFlags
        };
        foreach (var unit in slot.Units)
        {
            state.Units.Add(new SaveUnitState
            {
                Slot = unit.RecordIndex,
                DeploymentState = unit.DeploymentState,
                UnitTypeId = unit.UnitTypeId,
                UnitName = unit.UnitName,
                CurrentInternal = unit.CurrentInternal.Select(value => (int)value).ToArray(),
                CurrentArmor = unit.CurrentArmor.Select(value => (int)value).ToArray(),
                MaximumInternal = unit.MaximumInternal.Select(value => (int)value).ToArray(),
                MaximumArmor = unit.MaximumArmor.Select(value => (int)value).ToArray(),
                CurrentAmmo = unit.CurrentAmmo.Select(value => (int)value).ToArray(),
                Tonnage = unit.Tonnage,
                PilotId = unit.PilotId,
                Experience = unit.PilotExperience,
                Allegiance = unit.Allegiance
            });
        }
        return state;
    }

    public static SaveStateUpdateResult UpdateState(byte[] originalBytes, SaveState edited)
    {
        ArgumentNullException.ThrowIfNull(originalBytes);
        ArgumentNullException.ThrowIfNull(edited);
        string actualHash = Convert.ToHexString(SHA256.HashData(originalBytes)).ToLowerInvariant();
        if (!actualHash.Equals(edited.SourceSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("State fingerprint does not match the selected source save.");

        SaveState original = FetchState(originalBytes, edited.SlotNumber, edited.SourceFileName);
        ValidateShape(edited);
        RequireReadOnlyFieldsUnchanged(original, edited);

        byte[] output = (byte[])originalBytes.Clone();
        var changes = new List<SaveStateChange>();
        RevengeSaveSlot slot = RevengeSaveFile.Parse(originalBytes, edited.SourceFileName).GetSlot(edited.SlotNumber);
        PatchLabel(output, slot.DescriptorOffset + RevengeSaveFile.DescriptorControlLength,
            original.Label, edited.Label, changes);

        for (int index = 0; index < RevengeSaveFile.LiveUnitCount; index++)
        {
            SaveUnitState source = original.Units[index];
            SaveUnitState target = edited.Units[index];
            ValidateCurrentNotAboveMaximum(target, index);
            int record = slot.PayloadOffset + RevengeSaveFile.LiveUnitOffset + index * Formats.Units.UnitRecord.Length;
            PatchByteArray(output, record + 0x04, source.CurrentInternal, target.CurrentInternal,
                $"unit.{index}.current_internal", changes);
            PatchByteArray(output, record + 0x0C, source.CurrentArmor, target.CurrentArmor,
                $"unit.{index}.current_armor", changes);
            PatchWordArray(output, record + 0x38, source.CurrentAmmo, target.CurrentAmmo,
                $"unit.{index}.current_ammo", changes);
        }
        return new SaveStateUpdateResult(output, changes.AsReadOnly());
    }

    private static void ValidateShape(SaveState state)
    {
        if (state.SlotNumber is < 1 or > RevengeSaveFile.SlotCount)
            throw new InvalidDataException("Slot number must be 1 through 6.");
        if (state.Label.Any(character => character > 0x7F) ||
            System.Text.Encoding.ASCII.GetByteCount(state.Label) > RevengeSaveFile.DescriptorLabelLength)
            throw new InvalidDataException("Slot label must contain at most 17 ASCII characters.");
        if (state.Units.Count != RevengeSaveFile.LiveUnitCount)
            throw new InvalidDataException("State must contain exactly 24 unit records.");
        for (int index = 0; index < state.Units.Count; index++)
        {
            SaveUnitState unit = state.Units[index];
            if (unit.Slot != index) throw new InvalidDataException($"Unit record {index} has mismatched slot {unit.Slot}.");
            RequireArray(unit.CurrentInternal, 8, $"unit {index} current internal");
            RequireArray(unit.CurrentArmor, 11, $"unit {index} current armor");
            RequireArray(unit.MaximumInternal, 8, $"unit {index} maximum internal");
            RequireArray(unit.MaximumArmor, 11, $"unit {index} maximum armor");
            RequireArray(unit.CurrentAmmo, 14, $"unit {index} current ammo", ushort.MaxValue);
        }
    }

    private static void RequireArray(int[] values, int length, string field, int maximum = byte.MaxValue)
    {
        if (values.Length != length) throw new InvalidDataException($"{field} must contain {length} values.");
        if (values.Any(value => value < 0 || value > maximum))
            throw new InvalidDataException($"{field} contains a value outside 0..{maximum}.");
    }

    private static void ValidateCurrentNotAboveMaximum(SaveUnitState unit, int index)
    {
        for (int field = 0; field < unit.CurrentInternal.Length; field++)
            if (unit.CurrentInternal[field] > unit.MaximumInternal[field])
                throw new InvalidDataException($"unit.{index}.current_internal[{field}] exceeds its verified maximum.");
        for (int field = 0; field < unit.CurrentArmor.Length; field++)
            if (unit.CurrentArmor[field] > unit.MaximumArmor[field])
                throw new InvalidDataException($"unit.{index}.current_armor[{field}] exceeds its verified maximum.");
    }

    private static void RequireReadOnlyFieldsUnchanged(SaveState source, SaveState target)
    {
        if (source.CampaignStage != target.CampaignStage || source.ScenarioVariant != target.ScenarioVariant ||
            source.CampaignPhase != target.CampaignPhase || source.TrainingSequenceFlag != target.TrainingSequenceFlag ||
            source.CampaignFlags != target.CampaignFlags)
            throw new InvalidDataException("Campaign fields are currently read-only pending controlled game validation.");
        for (int index = 0; index < source.Units.Count; index++)
        {
            SaveUnitState a = source.Units[index];
            SaveUnitState b = target.Units[index];
            if (a.DeploymentState != b.DeploymentState || a.UnitTypeId != b.UnitTypeId || a.UnitName != b.UnitName ||
                !a.MaximumInternal.SequenceEqual(b.MaximumInternal) || !a.MaximumArmor.SequenceEqual(b.MaximumArmor) ||
                a.Tonnage != b.Tonnage || a.PilotId != b.PilotId || a.Experience != b.Experience || a.Allegiance != b.Allegiance)
                throw new InvalidDataException($"Unit {index} contains a change to a currently read-only field.");
        }
    }

    private static void PatchLabel(byte[] output, int offset, string oldLabel, string newLabel,
        List<SaveStateChange> changes)
    {
        // A no-op edit must preserve padding and any historical bytes following
        // the first NUL exactly. Only normalize the fixed field when the user
        // actually changes the visible label.
        if (oldLabel == newLabel) return;
        byte[] newBytes = new byte[RevengeSaveFile.DescriptorLabelLength];
        System.Text.Encoding.ASCII.GetBytes(newLabel, newBytes);
        for (int index = 0; index < newBytes.Length; index++)
            PatchByte(output, offset + index, newBytes[index], $"slot.label[{index}]", changes);
    }

    private static void PatchByteArray(byte[] output, int offset, int[] oldValues, int[] newValues,
        string field, List<SaveStateChange> changes)
    {
        for (int index = 0; index < newValues.Length; index++)
            PatchByte(output, offset + index, (byte)newValues[index], $"{field}[{index}]", changes);
    }

    private static void PatchWordArray(byte[] output, int offset, int[] oldValues, int[] newValues,
        string field, List<SaveStateChange> changes)
    {
        for (int index = 0; index < newValues.Length; index++)
        {
            ushort value = (ushort)newValues[index];
            PatchByte(output, offset + index * 2, (byte)value, $"{field}[{index}].lo", changes);
            PatchByte(output, offset + index * 2 + 1, (byte)(value >> 8), $"{field}[{index}].hi", changes);
        }
    }

    private static void PatchByte(byte[] output, int offset, byte value, string field,
        List<SaveStateChange> changes)
    {
        if (output[offset] == value) return;
        changes.Add(new SaveStateChange(field, offset, output[offset], value));
        output[offset] = value;
    }
}
