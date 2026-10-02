using System;
using System.IO;
using System.Text;
using InceptionTools.Installation;

namespace InceptionTools.SaveEditing;

public sealed class SaveStateTextExportResult
{
    internal SaveStateTextExportResult(string sourceFileName, string outputPath)
    {
        SourceFileName = sourceFileName;
        OutputPath = outputPath;
    }

    public string SourceFileName { get; }
    public string OutputPath { get; }
}

public sealed class SaveStateFileUpdateResult
{
    internal SaveStateFileUpdateResult(string sourceFileName, string stateTextPath,
        string outputPath, SaveStateUpdateResult update)
    {
        SourceFileName = sourceFileName;
        StateTextPath = stateTextPath;
        OutputPath = outputPath;
        Update = update;
    }

    public string SourceFileName { get; }
    public string StateTextPath { get; }
    public string OutputPath { get; }
    public SaveStateUpdateResult Update { get; }
}

/// <summary>
/// Thin file adapter over the reusable SaveStateEditor fetch/update API.
/// </summary>
public static class SaveStateFileService
{
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

    public static SaveState FetchState(GameInstallation installation, string fileName)
    {
        if (installation == null)
            throw new ArgumentNullException(nameof(installation));
        string sourcePath = installation.ResolveFile(fileName);
        return SaveStateEditor.FetchState(File.ReadAllBytes(sourcePath), Path.GetFileName(sourcePath));
    }

    public static SaveStateTextExportResult ExportText(GameInstallation installation,
        string fileName, string outputPath, bool overwrite = false)
    {
        ValidateTextPath(outputPath);
        string fullOutputPath = Path.GetFullPath(outputPath);
        if (!overwrite && File.Exists(fullOutputPath))
            throw new IOException("Save-state text already exists: " + fullOutputPath +
                ". Pass --force to overwrite it.");

        SaveState state = FetchState(installation, fileName);
        CreateParentDirectory(fullOutputPath);
        WriteText(fullOutputPath, SaveStateTextFormat.Write(state), overwrite);
        return new SaveStateTextExportResult(state.SourceFileName, fullOutputPath);
    }

    public static SaveStateFileUpdateResult ImportText(GameInstallation installation,
        string fileName, string stateTextPath, string outputPath, bool overwrite = false)
    {
        if (installation == null)
            throw new ArgumentNullException(nameof(installation));
        ValidateTextPath(stateTextPath);
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("An updated save output path is required.", nameof(outputPath));

        string sourcePath = installation.ResolveFile(fileName);
        string fullStateTextPath = Path.GetFullPath(stateTextPath);
        string fullOutputPath = Path.GetFullPath(outputPath);
        if (!File.Exists(fullStateTextPath))
            throw new FileNotFoundException("Save-state text not found: " + fullStateTextPath);
        if (fullOutputPath.Equals(Path.GetFullPath(sourcePath), StringComparison.OrdinalIgnoreCase))
            throw new IOException("Updated save output must not overwrite the source save. Use a new filename.");
        if (!overwrite && File.Exists(fullOutputPath))
            throw new IOException("Updated save output already exists: " + fullOutputPath +
                ". Pass --force to overwrite that output.");

        byte[] originalBytes = File.ReadAllBytes(sourcePath);
        SaveState baseline = SaveStateEditor.FetchState(originalBytes, Path.GetFileName(sourcePath));
        string text = File.ReadAllText(fullStateTextPath, StrictUtf8);
        SaveState edited = SaveStateTextFormat.Parse(text, baseline);
        SaveStateUpdateResult update = SaveStateEditor.UpdateState(originalBytes, edited);

        CreateParentDirectory(fullOutputPath);
        WriteBytes(fullOutputPath, update.Bytes, overwrite);
        return new SaveStateFileUpdateResult(Path.GetFileName(sourcePath),
            fullStateTextPath, fullOutputPath, update);
    }

    private static void ValidateTextPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("A save-state .txt path is required.");
        if (!Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Save-state text must use a .txt extension.");
    }

    private static void CreateParentDirectory(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    private static void WriteText(string path, string value, bool overwrite)
    {
        using (var output = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write, FileShare.None))
        using (var writer = new StreamWriter(output, new UTF8Encoding(false)))
            writer.Write(value);
    }

    private static void WriteBytes(string path, byte[] bytes, bool overwrite)
    {
        using (var output = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write, FileShare.None))
            output.Write(bytes, 0, bytes.Length);
    }
}
