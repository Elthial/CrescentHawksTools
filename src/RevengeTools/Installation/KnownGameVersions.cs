using System.Security.Cryptography;

namespace RevengeTools.Installation;

public sealed record KnownGameVersion(string Id, string ExecutableName, long Length, string Sha256);

public static class KnownGameVersions
{
    public static IReadOnlyList<KnownGameVersion> All { get; } =
    [
        new("version-1.00-reference", "REVENGE.EXE", 256411,
            "C4502165E251F3E2A5005B5C0E450E6B335B14C2BE9C035B0FD2D53CBB30667E")
    ];

    public static string Identify(string executablePath)
    {
        var info = new FileInfo(executablePath);
        string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(executablePath)));
        return All.FirstOrDefault(version => version.Length == info.Length && version.Sha256 == hash)?.Id
            ?? "unknown-compatible-executable";
    }
}
