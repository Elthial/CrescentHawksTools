using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace InceptionTools.Installation
{
    public sealed class ExpectedGameFile
    {
        public ExpectedGameFile(string name, string category, bool required, long? expectedLength = null)
        {
            Name = name;
            Category = category;
            Required = required;
            ExpectedLength = expectedLength;
        }

        public string Name { get; }
        public string Category { get; }
        public bool Required { get; }
        public long? ExpectedLength { get; }
    }

    public sealed class InstallationInventoryEntry
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public bool Required { get; set; }
        public bool Present { get; set; }
        public long? Length { get; set; }
        public string HeaderHex { get; set; }
        public string Validation { get; set; }
        public string Sha256 { get; set; }
    }

    public sealed class InstallationInventoryReport
    {
        public string InstallationPath { get; set; }
        public string LocatedBy { get; set; }
        public bool HashesIncluded { get; set; }
        public List<InstallationInventoryEntry> Files { get; set; } = new List<InstallationInventoryEntry>();
    }

    public static class InstallationInventory
    {
        public static IReadOnlyList<ExpectedGameFile> ExpectedFiles { get; } = BuildExpectedFiles();

        public static InstallationInventoryReport Scan(GameInstallation installation, bool includeHashes)
        {
            var actualFiles = Directory.EnumerateFiles(installation.DirectoryPath)
                .ToDictionary(Path.GetFileName, StringComparer.OrdinalIgnoreCase);
            var expectedNames = new HashSet<string>(ExpectedFiles.Select(file => file.Name), StringComparer.OrdinalIgnoreCase);
            var report = new InstallationInventoryReport
            {
                InstallationPath = installation.DirectoryPath,
                LocatedBy = installation.Source,
                HashesIncluded = includeHashes
            };

            foreach (ExpectedGameFile expected in ExpectedFiles)
            {
                actualFiles.TryGetValue(expected.Name, out string path);
                report.Files.Add(InspectExpected(expected, path, includeHashes));
            }

            foreach (KeyValuePair<string, string> actual in actualFiles.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (expectedNames.Contains(actual.Key))
                    continue;

                report.Files.Add(InspectUnexpected(actual.Key, actual.Value, includeHashes));
            }

            return report;
        }

        private static InstallationInventoryEntry InspectExpected(ExpectedGameFile expected, string path, bool includeHashes)
        {
            if (path == null)
            {
                return new InstallationInventoryEntry
                {
                    Name = expected.Name,
                    Category = expected.Category,
                    Required = expected.Required,
                    Present = false,
                    Validation = expected.Required ? "missing-required" : "missing-optional"
                };
            }

            var info = new FileInfo(path);
            string validation = Validate(expected, path, info.Length);
            return new InstallationInventoryEntry
            {
                Name = expected.Name,
                Category = expected.Category,
                Required = expected.Required,
                Present = true,
                Length = info.Length,
                HeaderHex = ReadHeader(path, 8),
                Validation = validation,
                Sha256 = includeHashes ? ComputeSha256(path) : null
            };
        }

        private static InstallationInventoryEntry InspectUnexpected(string name, string path, bool includeHashes)
        {
            var info = new FileInfo(path);
            return new InstallationInventoryEntry
            {
                Name = name,
                Category = "additional",
                Required = false,
                Present = true,
                Length = info.Length,
                HeaderHex = ReadHeader(path, 8),
                Validation = "not-in-expected-set",
                Sha256 = includeHashes ? ComputeSha256(path) : null
            };
        }

        private static string Validate(ExpectedGameFile expected, string path, long length)
        {
            if (expected.ExpectedLength.HasValue && length != expected.ExpectedLength.Value)
                return "length-mismatch:expected-" + expected.ExpectedLength.Value;

            if (expected.Name.EndsWith(".BLD", StringComparison.OrdinalIgnoreCase))
            {
                if (length < 2)
                    return "invalid-bld:shorter-than-length-word";

                using (FileStream stream = File.OpenRead(path))
                {
                    int low = stream.ReadByte();
                    int high = stream.ReadByte();
                    int storedLength = low | (high << 8);
                    return storedLength == length - 2
                        ? "ok:bld-payload-length"
                        : "invalid-bld:payload-length-" + storedLength;
                }
            }

            if (expected.Name.EndsWith(".CMP", StringComparison.OrdinalIgnoreCase) ||
                expected.Name.EndsWith(".ICN", StringComparison.OrdinalIgnoreCase))
            {
                if (length < 3)
                    return "invalid-graphics:short-header";
                using (FileStream stream = File.OpenRead(path))
                {
                    int low = stream.ReadByte();
                    int high = stream.ReadByte();
                    int compression = stream.ReadByte();
                    int storedLength = low | (high << 8);
                    if (storedLength != length - 2)
                        return "invalid-graphics:payload-length-" + storedLength;
                    if (compression != 1 && compression != 2)
                        return "invalid-graphics:compression-" + compression;
                    return "ok:graphics-header";
                }
            }

            if (expected.Name.Equals("BTECH.EXE", StringComparison.OrdinalIgnoreCase))
            {
                using (FileStream stream = File.OpenRead(path))
                    return stream.ReadByte() == 'M' && stream.ReadByte() == 'Z' ? "ok:mz" : "invalid:mz-missing";
            }

            return "ok";
        }

        private static string ReadHeader(string path, int count)
        {
            byte[] buffer = new byte[count];
            int read;
            using (FileStream stream = File.OpenRead(path))
                read = stream.Read(buffer, 0, buffer.Length);
            return BitConverter.ToString(buffer, 0, read).Replace("-", " ");
        }

        private static string ComputeSha256(string path)
        {
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static IReadOnlyList<ExpectedGameFile> BuildExpectedFiles()
        {
            var files = new List<ExpectedGameFile>
            {
                new ExpectedGameFile("BTECH.EXE", "executable", true),
                new ExpectedGameFile("DEMOFILE", "demo-input", true, 0x03FF),
                new ExpectedGameFile("WWOODBT.SIF", "sound", true, 0x1100),
                new ExpectedGameFile("ANIMATE.ICN", "graphics", true),
                new ExpectedGameFile("BTTLTECH.ICN", "graphics", true),
                new ExpectedGameFile("DESTRUCT.ICN", "graphics", true),
                new ExpectedGameFile("MAP.ICN", "graphics", true),
                new ExpectedGameFile("STARLEAG.ICN", "graphics", true),
                new ExpectedGameFile("BTBORDER.CMP", "graphics", true),
                new ExpectedGameFile("BTSTATS.CMP", "graphics", true),
                new ExpectedGameFile("BTTITLE.CMP", "graphics", true),
                new ExpectedGameFile("ENDMECH.CMP", "graphics", true),
                new ExpectedGameFile("INFOCOM.CMP", "graphics", true),
                new ExpectedGameFile("MECHSHAP.CMP", "graphics", true),
                new ExpectedGameFile("TINYLAND.CMP", "graphics", true)
            };

            string[] bldNames =
            {
                "TRAINING", "CITADEL", "COMSTAR", "WEAPON", "ARMOR", "REPAIR", "BARRACKS",
                "LOUNGE", "GARAGE", "HOSPITAL", "ARENA", "PARTY", "CLOTHES", "JAIL", "MAYOR",
                "WEAPON2", "THEATER", "FROB", "VIEWDISK", "BARRACK2", "ENTRANCE", "HUT",
                "ENDMECH", "INSTRUCT", "FINDIT", "WINSCENE"
            };
            files.AddRange(bldNames.Select(name => new ExpectedGameFile(name + ".BLD", "bld", true)));
            files.AddRange(new[] { 1, 2, 11, 14 }.Select(index => new ExpectedGameFile("MAP" + index + ".MTP", "map", true, 0x121D)));
            files.AddRange(Enumerable.Range(3, 8).Select(index => new ExpectedGameFile("MAP" + index + ".MTP", "map", true, 0x061D)));
            files.AddRange(new[] { 12, 13 }.Select(index => new ExpectedGameFile("MAP" + index + ".MTP", "map", true, 0x025D)));
            files.Add(new ExpectedGameFile("MAP15.MTP", "map", true, 0x0300));
            files.AddRange(Enumerable.Range(0, 22).Select(index => new ExpectedGameFile("O" + index + ".ANM", "animation", true)));
            files.AddRange(Enumerable.Range(1, 6).Select(index => new ExpectedGameFile("GAME" + index, "save", false, 0x0F49)));
            return files.OrderBy(file => file.Category).ThenBy(file => file.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
