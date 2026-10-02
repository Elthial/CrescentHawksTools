namespace RevengeTools.Cli;

public static class OutputFile
{
    public static void WriteBytes(string path, byte[] data, bool overwrite)
    {
        string fullPath = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        using var output = new FileStream(fullPath, overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write, FileShare.None);
        output.Write(data);
    }

    public static void WriteText(string path, string text, bool overwrite)
    {
        string fullPath = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        using var output = new FileStream(fullPath, overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(output, new System.Text.UTF8Encoding(false));
        writer.Write(text);
    }
}
