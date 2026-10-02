using System.Security.Cryptography;

namespace InceptionTools.Installation;

public sealed record KnownGameVersion(string Id, string ExecutableName, long Length, string Sha256);

public static class KnownGameVersions
{
    public static IReadOnlyList<KnownGameVersion> All { get; } =
    [
        new("installed-reference", "BTECH.EXE", 152429,
            "F2A9A023D79927B8072DE11DDD6E03DA6D3357D49181D8FBA6D12988BF8CC0EE")
    ];

    public static string Identify(string executablePath)
    {
        var info = new FileInfo(executablePath);
        string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(executablePath)));
        return All.FirstOrDefault(version => version.Length == info.Length && version.Sha256 == hash)?.Id
            ?? "unknown-compatible-executable";
    }
}
