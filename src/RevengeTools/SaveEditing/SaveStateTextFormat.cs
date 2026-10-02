using System.Globalization;
using System.Text;

namespace RevengeTools.SaveEditing;

public static class SaveStateTextFormat
{
    public static string Write(SaveState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var output = new StringBuilder();
        output.AppendLine("# BattleTech: The Crescent Hawk's Revenge editable save state");
        output.AppendLine("# Editable now: slot.label, current_internal, current_armor, current_ammo.");
        output.AppendLine("# All other fields are evidence-bearing read-only context.");
        output.AppendLine("format=" + SaveState.TextFormatId);
        output.AppendLine("source_file=" + state.SourceFileName);
        output.AppendLine("source_sha256=" + state.SourceSha256);
        output.AppendLine("slot.number=" + state.SlotNumber);
        output.AppendLine("slot.label=" + state.Label);
        output.AppendLine("campaign.stage=" + HexWord(state.CampaignStage));
        output.AppendLine("campaign.scenario_variant=" + HexWord(state.ScenarioVariant));
        output.AppendLine("campaign.phase=" + HexByte(state.CampaignPhase));
        output.AppendLine("campaign.training_sequence_flag=" + HexByte(state.TrainingSequenceFlag));
        output.AppendLine("campaign.flags=" + HexWord(state.CampaignFlags));

        foreach (SaveUnitState unit in state.Units)
        {
            string prefix = $"unit.{unit.Slot}.";
            output.AppendLine();
            output.AppendLine($"# Unit {unit.Slot}: {unit.UnitName}");
            output.AppendLine(prefix + "deployment_state=" + HexByte(unit.DeploymentState));
            output.AppendLine(prefix + "type_id=" + HexByte(unit.UnitTypeId));
            output.AppendLine(prefix + "name=" + unit.UnitName);
            output.AppendLine(prefix + "current_internal=" + Join(unit.CurrentInternal));
            output.AppendLine(prefix + "current_armor=" + Join(unit.CurrentArmor));
            output.AppendLine(prefix + "maximum_internal=" + Join(unit.MaximumInternal));
            output.AppendLine(prefix + "maximum_armor=" + Join(unit.MaximumArmor));
            output.AppendLine(prefix + "current_ammo=" + Join(unit.CurrentAmmo));
            output.AppendLine(prefix + "tonnage=" + unit.Tonnage);
            output.AppendLine(prefix + "pilot_id=" + HexByte(unit.PilotId));
            output.AppendLine(prefix + "experience=" + unit.Experience);
            output.AppendLine(prefix + "allegiance=" + HexByte(unit.Allegiance));
        }
        return output.ToString();
    }

    public static SaveState Parse(string text, SaveState baseline)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(baseline);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool sawFormat = false;
        bool sawFingerprint = false;
        bool sawSlot = false;
        using var input = new StringReader(text);
        string? line;
        int lineNumber = 0;
        while ((line = input.ReadLine()) is not null)
        {
            lineNumber++;
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
            int equals = line.IndexOf('=');
            if (equals <= 0) throw Error(lineNumber, "expected key=value");
            string key = line[..equals].Trim();
            string value = line[(equals + 1)..].Trim();
            if (!seen.Add(key)) throw Error(lineNumber, $"duplicate key '{key}'");
            try
            {
                if (key.Equals("format", StringComparison.OrdinalIgnoreCase))
                {
                    if (value != SaveState.TextFormatId) throw new InvalidDataException($"unsupported format '{value}'");
                    sawFormat = true;
                }
                else if (key.Equals("source_file", StringComparison.OrdinalIgnoreCase)) baseline.SourceFileName = value;
                else if (key.Equals("source_sha256", StringComparison.OrdinalIgnoreCase))
                {
                    if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
                        throw new InvalidDataException("source_sha256 must contain 64 hexadecimal digits");
                    baseline.SourceSha256 = value;
                    sawFingerprint = true;
                }
                else if (key.Equals("slot.number", StringComparison.OrdinalIgnoreCase))
                {
                    baseline.SlotNumber = ParseInt(value);
                    sawSlot = true;
                }
                else if (key.Equals("slot.label", StringComparison.OrdinalIgnoreCase)) baseline.Label = value;
                else if (key.StartsWith("campaign.", StringComparison.OrdinalIgnoreCase)) ApplyCampaign(baseline, key[9..], value);
                else if (key.StartsWith("unit.", StringComparison.OrdinalIgnoreCase)) ApplyUnit(baseline, key, value);
                else throw new InvalidDataException($"unknown key '{key}'");
            }
            catch (Exception exception) when (exception is FormatException or OverflowException or InvalidDataException or InvalidOperationException)
            {
                throw Error(lineNumber, exception.Message);
            }
        }
        if (!sawFormat) throw new InvalidDataException("State text is missing the format key.");
        if (!sawFingerprint) throw new InvalidDataException("State text is missing source_sha256.");
        if (!sawSlot) throw new InvalidDataException("State text is missing slot.number.");
        return baseline;
    }

    private static void ApplyCampaign(SaveState state, string field, string value)
    {
        switch (field.ToLowerInvariant())
        {
            case "stage": state.CampaignStage = ParseInt(value); break;
            case "scenario_variant": state.ScenarioVariant = ParseInt(value); break;
            case "phase": state.CampaignPhase = ParseInt(value); break;
            case "training_sequence_flag": state.TrainingSequenceFlag = ParseInt(value); break;
            case "flags": state.CampaignFlags = ParseInt(value); break;
            default: throw new InvalidDataException($"unknown campaign field '{field}'");
        }
    }

    private static void ApplyUnit(SaveState state, string key, string value)
    {
        string[] parts = key.Split('.');
        if (parts.Length != 3) throw new InvalidDataException("unit key must be unit.SLOT.FIELD");
        int slot = ParseInt(parts[1]);
        SaveUnitState unit = state.Units.Single(item => item.Slot == slot);
        switch (parts[2].ToLowerInvariant())
        {
            case "deployment_state": unit.DeploymentState = ParseInt(value); break;
            case "type_id": unit.UnitTypeId = ParseInt(value); break;
            case "name": unit.UnitName = value; break;
            case "current_internal": unit.CurrentInternal = ParseArray(value); break;
            case "current_armor": unit.CurrentArmor = ParseArray(value); break;
            case "maximum_internal": unit.MaximumInternal = ParseArray(value); break;
            case "maximum_armor": unit.MaximumArmor = ParseArray(value); break;
            case "current_ammo": unit.CurrentAmmo = ParseArray(value); break;
            case "tonnage": unit.Tonnage = ParseInt(value); break;
            case "pilot_id": unit.PilotId = ParseInt(value); break;
            case "experience": unit.Experience = ParseInt(value); break;
            case "allegiance": unit.Allegiance = ParseInt(value); break;
            default: throw new InvalidDataException($"unknown unit field '{parts[2]}'");
        }
    }

    private static int ParseInt(string value)
    {
        return value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? int.Parse(value[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)
            : int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    private static int[] ParseArray(string value) => value.Split(',', StringSplitOptions.TrimEntries)
        .Select(ParseInt).ToArray();

    private static string Join(IEnumerable<int> values) => string.Join(',', values);
    private static string HexByte(int value) => $"0x{value:X2}";
    private static string HexWord(int value) => $"0x{value:X4}";
    private static InvalidDataException Error(int line, string message) => new($"State text line {line}: {message}");
}
