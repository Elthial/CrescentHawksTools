using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace InceptionTools.SaveEditing;

/// <summary>
/// Diff-friendly key/value representation of SaveState. Lines beginning
/// with # are comments. Numeric input accepts decimal or a 0x hex prefix.
/// </summary>
public static class SaveStateTextFormat
{
    public static string Write(SaveState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        var output = new StringBuilder();
        output.AppendLine("# BattleTech: The Crescent Hawk's Inception editable save state");
        output.AppendLine("# Change values after '='. Arrays are comma-separated bytes.");
        output.AppendLine("# Unknown bytes remain in the original save and are never reconstructed here.");
        output.AppendLine("format=" + SaveState.TextFormatId);
        output.AppendLine("source_file=" + state.SourceFileName);
        output.AppendLine("source_sha256=" + state.SourceSha256);
        output.AppendLine();
        output.AppendLine("save.header=" + HexByte(state.Header));
        output.AppendLine("save.story_state_0cf9=" + HexByte(state.StoryStateAt0CF9));
        output.AppendLine("save.credits=" + state.Credits.ToString(CultureInfo.InvariantCulture));
        output.AppendLine("save.stock_0=" + state.Stock0.ToString(CultureInfo.InvariantCulture));
        output.AppendLine("save.stock_1=" + state.Stock1.ToString(CultureInfo.InvariantCulture));
        output.AppendLine("save.stock_2=" + state.Stock2.ToString(CultureInfo.InvariantCulture));
        output.AppendLine("save.party_map_x=" + state.PartyMapX.ToString(CultureInfo.InvariantCulture));
        output.AppendLine("save.party_map_y=" + state.PartyMapY.ToString(CultureInfo.InvariantCulture));

        foreach (SaveCharacterState character in state.Characters)
        {
            string prefix = "character." + character.Group + "." + character.Slot + ".";
            output.AppendLine();
            output.AppendLine("# " + character.Group + " character " + character.Slot);
            output.AppendLine(prefix + "name_id=" + character.NameId);
            output.AppendLine(prefix + "body=" + character.Body);
            output.AppendLine(prefix + "dexterity=" + character.Dexterity);
            output.AppendLine(prefix + "charisma=" + character.Charisma);
            output.AppendLine(prefix + "skills=" + Join(character.Skills));
            output.AppendLine(prefix + "weapon_table_index=" + character.WeaponTableIndex);
            output.AppendLine(prefix + "mech_assignment=" + character.MechAssignment);
            output.AppendLine(prefix + "armour_type=" + character.ArmourType);
            output.AppendLine(prefix + "armour_value=" + character.ArmourValue);
            output.AppendLine(prefix + "health=" + character.Health);
            output.AppendLine(prefix + "training_flags=" + HexByte(character.TrainingFlags));
        }

        foreach (SaveMechState mech in state.Mechs)
        {
            string prefix = "mech." + mech.Group + "." + mech.Slot + ".";
            output.AppendLine();
            output.AppendLine("# " + mech.Group + " mech " + mech.Slot);
            output.AppendLine("# active is the inverse of the probable no-mech/destroyed high bit");
            output.AppendLine(prefix + "active=" + (mech.Active ? "true" : "false"));
            output.AppendLine(prefix + "name=" + mech.Name);
            output.AppendLine(prefix + "tonnage=" + mech.Tonnage);
            output.AppendLine(prefix + "current_armour=" + Join(mech.CurrentArmour));
            output.AppendLine(prefix + "current_structure=" + Join(mech.CurrentStructure));
            output.AppendLine(prefix + "current_actuator_byte_24=" + HexByte(mech.CurrentActuatorByte24));
            output.AppendLine(prefix + "current_actuator_byte_25=" + HexByte(mech.CurrentActuatorByte25));
            output.AppendLine(prefix + "engine_heat_sinks=" + mech.EngineHeatSinks);
            output.AppendLine(prefix + "current_ammo=" + Join(mech.CurrentAmmo));
            output.AppendLine(prefix + "walk_move=" + mech.WalkMove);
            output.AppendLine(prefix + "jump_move=" + mech.JumpMove);
            output.AppendLine(prefix + "critical_slots_raw=" + Join(mech.CriticalSlotsRaw));
            output.AppendLine(prefix + "maximum_armour=" + Join(mech.MaximumArmour));
            output.AppendLine(prefix + "maximum_structure=" + Join(mech.MaximumStructure));
            output.AppendLine(prefix + "maximum_actuator_byte_69=" + HexByte(mech.MaximumActuatorByte69));
            output.AppendLine(prefix + "maximum_actuator_byte_6a=" + HexByte(mech.MaximumActuatorByte6A));
            output.AppendLine(prefix + "maximum_ammo=" + Join(mech.MaximumAmmo));
            output.AppendLine(prefix + "engine_hits=" + mech.EngineHits);
            output.AppendLine(prefix + "gyro_hits=" + mech.GyroHits);
            output.AppendLine(prefix + "sensor_hits=" + mech.SensorHits);
            output.AppendLine(prefix + "byte_78_probable_life_support_state=" +
                HexByte(mech.Byte78ProbableLifeSupportState));
            output.AppendLine(prefix + "pilot_id=" + mech.PilotId);
            output.AppendLine(prefix + "rider_id=" + mech.RiderId);
            output.AppendLine(prefix + "byte_7b_probable_upgrade_package_base=" +
                HexByte(mech.Byte7BProbableUpgradePackageBase));
            output.AppendLine(prefix + "upgrade_level_flags=" + HexByte(mech.UpgradeLevelFlags));
        }

        return output.ToString();
    }

    public static SaveState Parse(string text, SaveState baseline)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));
        if (baseline == null)
            throw new ArgumentNullException(nameof(baseline));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool sawFormat = false;
        bool sawFingerprint = false;
        using (var input = new StringReader(text))
        {
            string? line;
            int lineNumber = 0;
            while ((line = input.ReadLine()) != null)
            {
                lineNumber++;
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
                    continue;
                int equals = line.IndexOf('=');
                if (equals <= 0)
                    throw Error(lineNumber, "expected key=value");
                string key = line.Substring(0, equals).Trim();
                string value = line.Substring(equals + 1).Trim();
                if (!seen.Add(key))
                    throw Error(lineNumber, "duplicate key '" + key + "'");

                try
                {
                    if (key.Equals("format", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!value.Equals(SaveState.TextFormatId, StringComparison.Ordinal))
                            throw new InvalidDataException("unsupported format '" + value + "'");
                        sawFormat = true;
                    }
                    else if (key.Equals("source_file", StringComparison.OrdinalIgnoreCase))
                        baseline.SourceFileName = value;
                    else if (key.Equals("source_sha256", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
                            throw new InvalidDataException("source_sha256 must contain 64 hex digits");
                        baseline.SourceSha256 = value;
                        sawFingerprint = true;
                    }
                    else if (key.StartsWith("save.", StringComparison.OrdinalIgnoreCase))
                        ApplySaveValue(baseline, key.Substring(5), value);
                    else if (key.StartsWith("character.", StringComparison.OrdinalIgnoreCase))
                        ApplyCharacterValue(baseline, key, value);
                    else if (key.StartsWith("mech.", StringComparison.OrdinalIgnoreCase))
                        ApplyMechValue(baseline, key, value);
                    else
                        throw new InvalidDataException("unknown key '" + key + "'");
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException ||
                                           ex is InvalidDataException || ex is InvalidOperationException)
                {
                    throw Error(lineNumber, ex.Message);
                }
            }
        }

        if (!sawFormat)
            throw new InvalidDataException("State text is missing the format key.");
        if (!sawFingerprint)
            throw new InvalidDataException("State text is missing the source_sha256 key.");
        return baseline;
    }

    private static void ApplySaveValue(SaveState state, string field, string value)
    {
        switch (field.ToLowerInvariant())
        {
            case "header": state.Header = ParseInt(value); break;
            case "story_state_0cf9": state.StoryStateAt0CF9 = ParseInt(value); break;
            case "credits": state.Credits = ParseUInt(value); break;
            case "stock_0": state.Stock0 = ParseUInt(value); break;
            case "stock_1": state.Stock1 = ParseUInt(value); break;
            case "stock_2": state.Stock2 = ParseUInt(value); break;
            case "party_map_x": state.PartyMapX = ParseInt(value); break;
            case "party_map_y": state.PartyMapY = ParseInt(value); break;
            default: throw new InvalidDataException("unknown save field '" + field + "'");
        }
    }

    private static void ApplyCharacterValue(SaveState state, string key, string value)
    {
        string[] parts = key.Split('.');
        if (parts.Length != 4)
            throw new InvalidDataException("character key must be character.GROUP.SLOT.FIELD");
        int slot = ParseInt(parts[2]);
        SaveCharacterState character = state.Characters.Single(item =>
            item.Group.Equals(parts[1], StringComparison.OrdinalIgnoreCase) && item.Slot == slot);
        switch (parts[3].ToLowerInvariant())
        {
            case "name_id": character.NameId = ParseInt(value); break;
            case "body": character.Body = ParseInt(value); break;
            case "dexterity": character.Dexterity = ParseInt(value); break;
            case "charisma": character.Charisma = ParseInt(value); break;
            case "skills": character.Skills = ParseArray(value); break;
            case "weapon_table_index": character.WeaponTableIndex = ParseInt(value); break;
            case "mech_assignment": character.MechAssignment = ParseInt(value); break;
            case "armour_type": character.ArmourType = ParseInt(value); break;
            case "armour_value": character.ArmourValue = ParseInt(value); break;
            case "health": character.Health = ParseInt(value); break;
            case "training_flags": character.TrainingFlags = ParseInt(value); break;
            default: throw new InvalidDataException("unknown character field '" + parts[3] + "'");
        }
    }

    private static void ApplyMechValue(SaveState state, string key, string value)
    {
        string[] parts = key.Split('.');
        if (parts.Length != 4)
            throw new InvalidDataException("mech key must be mech.GROUP.SLOT.FIELD");
        int slot = ParseInt(parts[2]);
        SaveMechState mech = state.Mechs.Single(item =>
            item.Group.Equals(parts[1], StringComparison.OrdinalIgnoreCase) && item.Slot == slot);
        switch (parts[3].ToLowerInvariant())
        {
            case "active": mech.Active = ParseBool(value); break;
            case "name": mech.Name = value; break;
            case "tonnage": mech.Tonnage = ParseInt(value); break;
            case "current_armour": mech.CurrentArmour = ParseArray(value); break;
            case "current_structure": mech.CurrentStructure = ParseArray(value); break;
            case "current_actuator_byte_24": mech.CurrentActuatorByte24 = ParseInt(value); break;
            case "current_actuator_byte_25": mech.CurrentActuatorByte25 = ParseInt(value); break;
            case "engine_heat_sinks": mech.EngineHeatSinks = ParseInt(value); break;
            case "current_ammo": mech.CurrentAmmo = ParseArray(value); break;
            case "walk_move": mech.WalkMove = ParseInt(value); break;
            case "jump_move": mech.JumpMove = ParseInt(value); break;
            case "critical_slots_raw": mech.CriticalSlotsRaw = ParseArray(value); break;
            case "maximum_armour": mech.MaximumArmour = ParseArray(value); break;
            case "maximum_structure": mech.MaximumStructure = ParseArray(value); break;
            case "maximum_actuator_byte_69": mech.MaximumActuatorByte69 = ParseInt(value); break;
            case "maximum_actuator_byte_6a": mech.MaximumActuatorByte6A = ParseInt(value); break;
            case "maximum_ammo": mech.MaximumAmmo = ParseArray(value); break;
            case "engine_hits": mech.EngineHits = ParseInt(value); break;
            case "gyro_hits": mech.GyroHits = ParseInt(value); break;
            case "sensor_hits": mech.SensorHits = ParseInt(value); break;
            case "byte_78_probable_life_support_state":
                mech.Byte78ProbableLifeSupportState = ParseInt(value); break;
            case "pilot_id": mech.PilotId = ParseInt(value); break;
            case "rider_id": mech.RiderId = ParseInt(value); break;
            case "byte_7b_probable_upgrade_package_base":
                mech.Byte7BProbableUpgradePackageBase = ParseInt(value); break;
            case "upgrade_level_flags": mech.UpgradeLevelFlags = ParseInt(value); break;
            default: throw new InvalidDataException("unknown mech field '" + parts[3] + "'");
        }
    }

    private static int ParseInt(string value)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.Parse(value.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    private static uint ParseUInt(string value)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return uint.Parse(value.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        return uint.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    private static bool ParseBool(string value)
    {
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
            return true;
        if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
            return false;
        throw new InvalidDataException("boolean value must be true or false");
    }

    private static int[] ParseArray(string value)
    {
        if (value.Length == 0)
            return Array.Empty<int>();
        return value.Split(',').Select(item => ParseInt(item.Trim())).ToArray();
    }

    private static string Join(int[] values)
    {
        return string.Join(",", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
    }

    private static string HexByte(int value)
    {
        return "0x" + value.ToString("X2", CultureInfo.InvariantCulture);
    }

    private static InvalidDataException Error(int line, string message)
    {
        return new InvalidDataException("State text line " + line + ": " + message);
    }
}
