using System;
using System.Collections.Generic;
using System.IO;

namespace InceptionTools.Installation;

public sealed class GameInstallation
{
    public const long MaximumInputFileLength = 128L * 1024 * 1024;

    public GameInstallation(string directoryPath, string source)
    {
        DirectoryPath = directoryPath;
        Source = source;
    }

    public string DirectoryPath { get; }
    public string Source { get; }

    public string ResolveFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
            throw new ArgumentException("File must be a single installation filename, not a path.");

        foreach (string candidate in Directory.EnumerateFiles(DirectoryPath))
        {
            if (Path.GetFileName(candidate).Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                EnsureInputSize(candidate);
                return candidate;
            }
        }

        throw new FileNotFoundException("Installation file not found: " + fileName);
    }

    public static void EnsureInputSize(string path)
    {
        long length = new FileInfo(path).Length;
        if (length > MaximumInputFileLength)
            throw new InvalidDataException($"Input file is {length} bytes; limit is {MaximumInputFileLength} bytes: {path}");
    }
}

public static class GameInstallationLocator
{
    public const string EnvironmentVariable = "BTCHI_GAME_DIR";

    public static GameInstallation Locate(string? explicitPath = null, string? startDirectory = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitPath))
            return Validate(explicitPath, "--game-dir");

        string? environmentPath = Environment.GetEnvironmentVariable(EnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(environmentPath))
            return Validate(environmentPath, EnvironmentVariable);

        string start = Path.GetFullPath(startDirectory ?? Environment.CurrentDirectory);
        var directory = new DirectoryInfo(start);
        while (directory != null)
        {
            if (ContainsFile(directory.FullName, "BTECH.EXE"))
                return Validate(directory.FullName, "current/ancestor directory");

            string localReference = Path.Combine(directory.FullName, "Chinception");
            if (Directory.Exists(localReference))
                return Validate(localReference, "local ignored Chinception fallback");

            string repositoryReference = Path.Combine(
                directory.FullName, "CrescentHawksInception", "Chinception");
            if (Directory.Exists(repositoryReference))
                return Validate(repositoryReference, "repository Inception installation fallback");

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate a BattleTech installation. Pass --game-dir PATH, " +
            "set " + EnvironmentVariable + ", or provide an ignored Chinception directory.");
    }

    private static GameInstallation Validate(string path, string source)
    {
        string fullPath = Path.GetFullPath(path.Trim().Trim('"'));
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException("Game directory does not exist: " + fullPath);

        string executable = Directory.EnumerateFiles(fullPath)
            .FirstOrDefault(file => Path.GetFileName(file).Equals("BTECH.EXE", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException("Directory does not contain BTECH.EXE: " + fullPath);
        using FileStream stream = File.OpenRead(executable);
        if (stream.ReadByte() != 'M' || stream.ReadByte() != 'Z')
            throw new InvalidDataException("BTECH.EXE does not have an MZ header: " + fullPath);

        return new GameInstallation(fullPath, source);
    }

    private static bool ContainsFile(string directory, string fileName) =>
        Directory.EnumerateFiles(directory)
            .Any(file => Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase));
}
