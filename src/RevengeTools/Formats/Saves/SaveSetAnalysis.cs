using System.Text;

namespace RevengeTools.Formats.Saves;

public sealed record SaveCheckpointSummary(
    string File,
    int Slot,
    string Label,
    ushort CampaignStage,
    ushort ScenarioVariant,
    byte CampaignPhase,
    byte TrainingSequenceFlag,
    ushort CampaignFlags,
    int PopulatedUnits,
    string PopulatedUnitSlots,
    string ScenarioUnitMapX,
    string ScenarioUnitMapY,
    string LanceAssignments,
    string PilotAvailability);

public sealed record SaveSetByteProfile(
    string Region,
    int Offset,
    int DistinctValueCount,
    string ValuesHex);

public sealed record SaveSetAnalysisReport(
    string Directory,
    int FileCount,
    int CheckpointCount,
    IReadOnlyList<SaveCheckpointSummary> Checkpoints,
    IReadOnlyList<SaveSetByteProfile> ByteProfiles);

public static class SaveSetAnalyzer
{
    public static SaveSetAnalysisReport Analyze(string directory)
    {
        string fullDirectory = Path.GetFullPath(directory);
        if (!Directory.Exists(fullDirectory))
            throw new DirectoryNotFoundException("Save-set directory does not exist: " + fullDirectory);
        string[] files = Directory.GetFiles(fullDirectory, "SAVEGAME.S??")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        if (files.Length == 0)
            throw new InvalidDataException("No SAVEGAME.S?? files found in " + fullDirectory);

        var checkpoints = new List<SaveCheckpointSummary>();
        var payloads = new List<byte[]>();
        foreach (string file in files)
        {
            RevengeSaveFile save = RevengeSaveFile.Parse(File.ReadAllBytes(file), Path.GetFileName(file));
            foreach (RevengeSaveSlot slot in save.Slots.Where(slot => slot.AppearsOccupied))
            {
                payloads.Add(slot.Payload);
                checkpoints.Add(new SaveCheckpointSummary(Path.GetFileName(file), slot.SlotNumber, slot.Label,
                    slot.CampaignStage, slot.ScenarioVariant, slot.CampaignPhase, slot.TrainingSequenceFlag,
                    slot.CampaignFlags, slot.Units.Count(unit => unit.IsPopulated),
                    string.Join(',', slot.Units.Where(unit => unit.IsPopulated).Select(unit => unit.RecordIndex)),
                    Convert.ToHexString(slot.Payload.AsSpan(0x0025, 24)),
                    Convert.ToHexString(slot.Payload.AsSpan(0x003D, 24)),
                    Convert.ToHexString(slot.Payload.AsSpan(0x017A, 130)),
                    Convert.ToHexString(slot.Payload.AsSpan(0x01FC, 37))));
            }
        }

        var profiles = new List<SaveSetByteProfile>();
        AddProfiles(profiles, payloads, "scenarioUnitMapX", 0x0025, 24);
        AddProfiles(profiles, payloads, "scenarioUnitMapY", 0x003D, 24);
        AddProfiles(profiles, payloads, "pilotAvailability", 0x01FC, 37);
        return new SaveSetAnalysisReport(fullDirectory, files.Length, checkpoints.Count, checkpoints, profiles);
    }

    private static void AddProfiles(ICollection<SaveSetByteProfile> profiles, IReadOnlyList<byte[]> payloads,
        string region, int start, int length)
    {
        for (int offset = 0; offset < length; offset++)
        {
            byte[] values = payloads.Select(payload => payload[start + offset]).Distinct().Order().ToArray();
            profiles.Add(new SaveSetByteProfile(region, offset, values.Length,
                string.Join(',', values.Select(value => value.ToString("X2")))));
        }
    }
}

public static class SaveSetAnalysisFormatter
{
    public static string Format(SaveSetAnalysisReport report)
    {
        var output = new StringBuilder();
        output.AppendLine($"Save set: {report.Directory}");
        output.AppendLine($"Files: {report.FileCount}; occupied checkpoints: {report.CheckpointCount}");
        output.AppendLine();
        output.AppendLine("file slot stage variant phase train flags units slots label");
        foreach (SaveCheckpointSummary checkpoint in report.Checkpoints)
            output.AppendLine($"{checkpoint.File,-12} {checkpoint.Slot,2} 0x{checkpoint.CampaignStage:X4} " +
                $"0x{checkpoint.ScenarioVariant:X4} 0x{checkpoint.CampaignPhase:X2} 0x{checkpoint.TrainingSequenceFlag:X2} " +
                $"0x{checkpoint.CampaignFlags:X4} {checkpoint.PopulatedUnits,2} [{checkpoint.PopulatedUnitSlots}] {checkpoint.Label}");

        output.AppendLine();
        output.AppendLine("Scenario-state byte profiles across occupied checkpoints:");
        foreach (SaveSetByteProfile profile in report.ByteProfiles)
            output.AppendLine($"  {profile.Region}[{profile.Offset:D2}] distinct={profile.DistinctValueCount,2} values={profile.ValuesHex}");
        return output.ToString().TrimEnd();
    }
}
