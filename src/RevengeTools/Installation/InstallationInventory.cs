using System.Security.Cryptography;
using RevengeTools.Binary;

namespace RevengeTools.Installation;

public sealed record ExpectedGameFile(string Name, string Category, bool Required = true);

public sealed class InstallationInventoryEntry
{
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required bool Required { get; init; }
    public required bool Present { get; init; }
    public long? Length { get; init; }
    public string? HeaderHex { get; init; }
    public required string Validation { get; init; }
    public string? Sha256 { get; init; }
}

public sealed class InstallationInventoryReport
{
    public required string InstallationPath { get; init; }
    public required string LocatedBy { get; init; }
    public required bool HashesIncluded { get; init; }
    public required string DetectedVersion { get; init; }
    public required IReadOnlyList<InstallationInventoryEntry> Files { get; init; }
    public bool IsValid => Files.All(file => !file.Required || file.Present) &&
        Files.Where(file => file.Present).All(file => file.Validation.StartsWith("ok", StringComparison.Ordinal));
}

public static class InstallationInventory
{
    public static IReadOnlyList<ExpectedGameFile> ExpectedFiles { get; } = BuildExpectedFiles();

    public static InstallationInventoryReport Scan(GameInstallation installation, bool includeHashes)
    {
        Dictionary<string, string> actual = Directory.EnumerateFiles(installation.DirectoryPath)
            .ToDictionary(path => Path.GetFileName(path)!, StringComparer.OrdinalIgnoreCase);
        var expectedNames = new HashSet<string>(ExpectedFiles.Select(file => file.Name), StringComparer.OrdinalIgnoreCase);
        var entries = new List<InstallationInventoryEntry>();

        foreach (ExpectedGameFile expected in ExpectedFiles)
        {
            actual.TryGetValue(expected.Name, out string? path);
            entries.Add(path is null ? Missing(expected) : Inspect(expected.Name, expected.Category, expected.Required, path, includeHashes));
        }

        foreach ((string name, string path) in actual.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (!expectedNames.Contains(name))
                entries.Add(Inspect(name, Categorize(name), false, path, includeHashes));
        }

        return new InstallationInventoryReport
        {
            InstallationPath = installation.DirectoryPath,
            LocatedBy = installation.Source,
            HashesIncluded = includeHashes,
            DetectedVersion = KnownGameVersions.Identify(installation.ResolveFile("REVENGE.EXE")),
            Files = entries
        };
    }

    private static InstallationInventoryEntry Missing(ExpectedGameFile file) => new()
    {
        Name = file.Name,
        Category = file.Category,
        Required = file.Required,
        Present = false,
        Validation = file.Required ? "missing-required" : "missing-optional"
    };

    private static InstallationInventoryEntry Inspect(string name, string category, bool required,
        string path, bool includeHashes)
    {
        GameInstallation.EnsureInputSize(path);
        byte[] data = File.ReadAllBytes(path);
        return new InstallationInventoryEntry
        {
            Name = name,
            Category = category,
            Required = required,
            Present = true,
            Length = data.Length,
            HeaderHex = BitConverter.ToString(data, 0, Math.Min(8, data.Length)).Replace('-', ' '),
            Validation = Validate(name, data),
            Sha256 = includeHashes ? Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant() : null
        };
    }

    private static string Validate(string name, byte[] data)
    {
        string extension = Path.GetExtension(name).ToUpperInvariant();
        var reader = new BoundedBinaryReader(data, name);
        try
        {
            if (name.Equals("REVENGE.EXE", StringComparison.OrdinalIgnoreCase))
                return data.Length >= 2 && data[0] == 'M' && data[1] == 'Z' ? "ok:mz" : "invalid:mz-missing";
            if (name.Equals("MECHTYPE.DAT", StringComparison.OrdinalIgnoreCase))
                return data.Length == 89 * 0x96 ? "ok:89-unit-records" : "invalid:unit-table-length";
            if (name.Equals("SAVEGAME.DAT", StringComparison.OrdinalIgnoreCase))
                return data.Length == 0x61C0 ? "ok:save-container-length" : "invalid:save-container-length";
            if (name.StartsWith("SCENE", StringComparison.OrdinalIgnoreCase) && extension == ".DAT")
                return HasStoredLength(reader) ? "ok:scene-stored-length" : "invalid:scene-stored-length";
            if (extension == ".COL")
                return data.Length == 768 && data.All(value => value <= 63) ? "ok:256-colour-6-bit" : "invalid:palette";
            if (extension is ".CPS")
            {
                if (data.Length < 10 || reader.ReadUInt16LittleEndian(0) != data.Length)
                    return "invalid:cps-stored-length";
                ushort compression = reader.ReadUInt16LittleEndian(2);
                return reader.ReadUInt32LittleEndian(4) == 64000 && reader.ReadUInt16LittleEndian(8) == 0 &&
                    compression is >= 1 and <= 3 ? $"ok:cps-type-{compression}" : "invalid:cps-header";
            }
            if (extension == ".CMP")
                return HasStoredLength(reader) && data.Length >= 3 ? $"ok:westwood-rle-type-{data[2]}" : "invalid:graphics-envelope";
            if (extension == ".ICN")
            {
                if (data.Length < 3) return "invalid:graphics-envelope";
                int declaredEnd = reader.ReadUInt16LittleEndian(0) + 2;
                if (declaredEnd > data.Length) return "invalid:graphics-envelope-overrun";
                int trailing = data.Length - declaredEnd;
                return trailing == 0 ? $"ok:westwood-rle-type-{data[2]}" :
                    $"ok:westwood-rle-type-{data[2]}-trailing-{trailing}-bytes";
            }
            if (extension == ".FNT")
                return data.Length >= 0x104 && reader.ReadUInt16LittleEndian(0) == data.Length - 2 &&
                    data[0x102] > 0 && data[0x103] is > 0 and <= 8 ? "ok:font-v2" : "invalid:font-v2";
            if (extension == ".MAP")
            {
                if (data.Length < 12) return "invalid:map-short";
                int width = reader.ReadUInt16LittleEndian(0);
                int height = reader.ReadUInt16LittleEndian(2);
                return width > 0 && height > 0 && data.Length == 12 + checked(width * height) ? "ok:map-grid" : "invalid:map-grid";
            }
            if (extension == ".MUS")
                return data.Length >= 14 && System.Text.Encoding.ASCII.GetString(data, 0, 4) == "MThd" ? "ok:midi" : "invalid:midi-header";
            if (extension == ".BIN")
                return data.Length > 0 ? "ok:raw-sample-candidate" : "invalid:empty";
            return "ok:unclassified";
        }
        catch (Exception exception) when (exception is InvalidDataException or OverflowException)
        {
            return "invalid:" + exception.Message;
        }
    }

    private static bool HasStoredLength(BoundedBinaryReader reader) =>
        reader.Length >= 2 && reader.ReadUInt16LittleEndian(0) == reader.Length - 2;

    private static string Categorize(string name)
    {
        if (name.Equals("REVENGE.EXE", StringComparison.OrdinalIgnoreCase)) return "executable";
        if (name.Equals("MECHTYPE.DAT", StringComparison.OrdinalIgnoreCase)) return "unit-table";
        if (name.Equals("SAVEGAME.DAT", StringComparison.OrdinalIgnoreCase)) return "save";
        if (name.StartsWith("SCENE", StringComparison.OrdinalIgnoreCase)) return "scene";
        return Path.GetExtension(name).TrimStart('.').ToLowerInvariant();
    }

    private static IReadOnlyList<ExpectedGameFile> BuildExpectedFiles()
    {
        var files = new List<ExpectedGameFile>
        {
            new("REVENGE.EXE", "executable"),
            new("MECHTYPE.DAT", "unit-table"),
            new("SAVEGAME.DAT", "save", false),
            new("FONT6.FNT", "font"),
            new("FONT8.FNT", "font"),
            new("BOOMS.CMP", "graphics")
        };
        string[] cps = { "BASEEXT", "BLAZACE", "BRUSHES", "BTBORDER", "CHIUN", "CHIUN'50", "COMM", "DSATTACK", "DSLYONS", "ENDGAME", "FINAL", "GREASE", "HALFDS", "HALFDSSH", "HAWKLOGO", "ISMAP1", "ISMAP2", "ISMAPO", "JEN", "JUMPSHIP", "KELL", "KHBASE", "KHHANGER", "KURT", "MAP1", "MAP2", "MECHBAY", "MECHS1", "MECHS2", "ORDERS", "PILOTS1", "PILOTS2", "QUADPICS", "REX", "RIP", "SHAW", "SWAMP", "TANK", "TITLE" };
        files.AddRange(cps.Select(name => new ExpectedGameFile(name + ".CPS", "cps")));
        string[] palettes = { "BASEEXT", "BLAZACE", "DSATTACK", "DSLYONS", "ENDGAME", "FACES", "FINAL", "HALFDS", "HAWKLOGO", "ISMAPS", "JUMPSHIP", "KHBASE", "KHHANGER", "MAP", "MAPS", "MECHBAY", "QUADPICS", "RIP", "SELCTION", "SWAMP", "TANK", "TITLE" };
        files.AddRange(palettes.Select(name => new ExpectedGameFile(name + ".COL", "palette")));
        files.AddRange(Enumerable.Range(0, 8).Select(index => new ExpectedGameFile($"ICONSET{index}.ICN", "icon-set")));
        files.AddRange(Enumerable.Range(0, 8).Select(index => new ExpectedGameFile($"DESTROY{index}.ICN", "destroy-set")));
        files.Add(new("MECHSET1.ICN", "mech-set"));
        files.Add(new("MECHSET2.ICN", "mech-set"));
        files.AddRange(Enumerable.Range(0, 10).Select(index => new ExpectedGameFile($"MAP{index}.MAP", "map")));
        files.Add(new("MAPA.MAP", "map"));
        files.AddRange(Enumerable.Range(1, 9).Select(index => new ExpectedGameFile($"SCENE{index}.DAT", "scene")));
        files.AddRange(Enumerable.Range('A', 21).Select(value => new ExpectedGameFile($"SCENE{(char)value}.DAT", "scene")));
        string[] music = { "DETHMEC1", "DETHMEC2", "MARCH1", "MARCH2", "SUBTITLE", "SUSP1", "SUSP2", "TITLE", "VICTORY", "WINGAME" };
        files.AddRange(music.Select(name => new ExpectedGameFile(name + ".MUS", "music")));
        string[] samples = { "DPHRASES", "INCOMING", "INFOCOM", "LARGEGUN", "LASER3", "LGBOOM1", "SMGUN1", "WARN1" };
        files.AddRange(samples.Select(name => new ExpectedGameFile(name + ".BIN", "sample")));
        return files;
    }
}
