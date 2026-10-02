using System.Text;
using CrescentHawksTools.Cli;
using RevengeTools.Installation;

namespace RevengeTools.SaveEditing;

public static class SaveStateFileService
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

    public static SaveState Export(GameInstallation installation, string fileName, int slot,
        string outputPath, bool overwrite)
    {
        string sourcePath = installation.ResolveFile(fileName);
        SaveState state = SaveStateEditor.FetchState(File.ReadAllBytes(sourcePath), slot, Path.GetFileName(sourcePath));
        OutputFile.WriteText(outputPath, SaveStateTextFormat.Write(state), overwrite);
        return state;
    }

    public static SaveStateUpdateResult Import(GameInstallation installation, string fileName, int slot,
        string statePath, string outputPath, bool dryRun, bool overwrite)
    {
        string sourcePath = installation.ResolveFile(fileName);
        string fullOutput = Path.GetFullPath(outputPath);
        if (fullOutput.Equals(Path.GetFullPath(sourcePath), StringComparison.OrdinalIgnoreCase))
            throw new IOException("Edited save output must not overwrite the source save.");
        byte[] source = File.ReadAllBytes(sourcePath);
        SaveState baseline = SaveStateEditor.FetchState(source, slot, Path.GetFileName(sourcePath));
        string text = File.ReadAllText(Path.GetFullPath(statePath), StrictUtf8);
        SaveState edited = SaveStateTextFormat.Parse(text, baseline);
        if (edited.SlotNumber != slot)
            throw new InvalidDataException($"State text targets slot {edited.SlotNumber}, not requested slot {slot}.");
        SaveStateUpdateResult result = SaveStateEditor.UpdateState(source, edited);
        if (!dryRun) OutputFile.WriteBytes(fullOutput, result.Bytes, overwrite);
        return result;
    }
}
