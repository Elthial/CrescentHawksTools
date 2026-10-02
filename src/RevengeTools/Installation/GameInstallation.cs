namespace RevengeTools.Installation;

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
            throw new ArgumentException("File must be one installation filename, not a path.", nameof(fileName));

        string? match = Directory.EnumerateFiles(DirectoryPath)
            .FirstOrDefault(path => Path.GetFileName(path).Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (match is null)
            throw new FileNotFoundException("Installation file not found: " + fileName);
        EnsureInputSize(match);
        return match;
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
    public const string EnvironmentVariable = "BTCHR_GAME_DIR";

    public static GameInstallation Locate(string? explicitPath = null, string? startDirectory = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitPath))
            return Validate(explicitPath, "--game-dir");

        string? environmentPath = Environment.GetEnvironmentVariable(EnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(environmentPath))
            return Validate(environmentPath, EnvironmentVariable);

        var directory = new DirectoryInfo(Path.GetFullPath(startDirectory ?? Environment.CurrentDirectory));
        while (directory is not null)
        {
            if (Directory.EnumerateFiles(directory.FullName)
                .Any(file => Path.GetFileName(file).Equals("REVENGE.EXE", StringComparison.OrdinalIgnoreCase)))
                return Validate(directory.FullName, "current/ancestor directory");

            string local = Path.Combine(directory.FullName, "Chrevenge");
            if (Directory.Exists(local))
                return Validate(local, "local ignored Chrevenge fallback");

            string repository = Path.Combine(directory.FullName, "CrescentHawksRevenge", "Chrevenge");
            if (Directory.Exists(repository))
                return Validate(repository, "repository Revenge installation fallback");

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate a Revenge installation. Pass --game-dir PATH, set {EnvironmentVariable}, or provide an ignored Chrevenge directory.");
    }

    private static GameInstallation Validate(string path, string source)
    {
        string fullPath = Path.GetFullPath(path.Trim().Trim('"'));
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException("Game directory does not exist: " + fullPath);
        string executable = Directory.EnumerateFiles(fullPath)
            .FirstOrDefault(file => Path.GetFileName(file).Equals("REVENGE.EXE", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException("Directory does not contain REVENGE.EXE: " + fullPath);
        using FileStream stream = File.OpenRead(executable);
        if (stream.ReadByte() != 'M' || stream.ReadByte() != 'Z')
            throw new InvalidDataException("REVENGE.EXE does not have an MZ header: " + fullPath);
        return new GameInstallation(fullPath, source);
    }
}
