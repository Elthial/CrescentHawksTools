using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using InceptionTools.Graphics;
using InceptionTools.Installation;
using InceptionTools.Records;

namespace InceptionTools.Maps
{
    public enum MtpLegacyTileOrder
    {
        RowMajorReadInBlockTraversal,
        BlockTraversalReadIntoRowMajor
    }

    public sealed class MtpMapProfile
    {
        internal MtpMapProfile(int mapNumber, string tileSetFileName, MtpLegacyTileOrder tileOrder,
            bool hasStandardHeader)
        {
            MapNumber = mapNumber;
            FileName = "MAP" + mapNumber + ".MTP";
            TileSetFileName = tileSetFileName;
            TileOrder = tileOrder;
            HasStandardHeader = hasStandardHeader;
        }

        public int MapNumber { get; }
        public string FileName { get; }
        public string TileSetFileName { get; }
        public MtpLegacyTileOrder TileOrder { get; }
        public bool HasStandardHeader { get; }
    }

    public sealed class MtpMapInspection
    {
        internal MtpMapInspection(MtpMapProfile profile, MtpMapRecord record)
        {
            FileName = profile.FileName;
            MapNumber = profile.MapNumber;
            Width = record.Width;
            Height = record.Height;
            HasStandardHeader = record.HasStandardHeader;
            TileSetFileName = profile.TileSetFileName;
            TileOrder = profile.TileOrder.ToString();
            MaximumTileId = record.TileIds.Max(value => (int)value);
        }

        public string FileName { get; }
        public int MapNumber { get; }
        public int Width { get; }
        public int Height { get; }
        public bool HasStandardHeader { get; }
        public string TileSetFileName { get; }
        public string TileOrder { get; }
        public int MaximumTileId { get; }
    }

    public sealed class MtpMapExportResult
    {
        internal MtpMapExportResult(MtpMapInspection inspection, string outputPath,
            string metadataPath, int outputLength)
        {
            Inspection = inspection;
            OutputPath = outputPath;
            MetadataPath = metadataPath;
            OutputLength = outputLength;
        }

        public MtpMapInspection Inspection { get; }
        public string OutputPath { get; }
        public string MetadataPath { get; }
        public int OutputLength { get; }
    }

    public sealed class MtpMapBatchExportResult
    {
        internal MtpMapBatchExportResult(string outputDirectory, List<MtpMapExportResult> maps)
        {
            OutputDirectory = outputDirectory;
            Maps = maps.AsReadOnly();
        }

        public string OutputDirectory { get; }
        public IReadOnlyList<MtpMapExportResult> Maps { get; }
        public int TotalOutputLength => Maps.Sum(map => map.OutputLength);
    }

    public static class MtpMapExporter
    {
        private const int TileWidth = 16;
        private const int TileHeight = 16;
        private const int TilePixelCount = TileWidth * TileHeight;
        private static readonly IReadOnlyList<MtpMapProfile> Profiles = CreateProfiles();

        public static IReadOnlyList<MtpMapProfile> SupportedProfiles => Profiles;

        public static MtpMapInspection Inspect(GameInstallation installation, string fileName)
        {
            Load(installation, fileName, out MtpMapProfile profile, out MtpMapRecord record);
            return new MtpMapInspection(profile, record);
        }

        public static string WriteText(MtpMapInspection inspection)
        {
            if (inspection == null)
                throw new ArgumentNullException(nameof(inspection));
            var text = new StringBuilder();
            text.AppendLine(inspection.FileName);
            text.AppendLine("  dimensions: " + inspection.Width + "x" + inspection.Height + " tiles (" +
                inspection.Width * TileWidth + "x" + inspection.Height * TileHeight + " pixels)");
            text.AppendLine("  standard header: " + inspection.HasStandardHeader.ToString().ToLowerInvariant());
            text.AppendLine("  tile set: " + inspection.TileSetFileName);
            text.AppendLine("  probable tile order: " + inspection.TileOrder);
            text.AppendLine("  maximum tile ID: " + inspection.MaximumTileId);
            text.AppendLine("  metadata blocks: preserved raw in export JSON; internal string indexing unresolved");
            return text.ToString();
        }

        public static string WriteJson(MtpMapInspection inspection)
        {
            if (inspection == null)
                throw new ArgumentNullException(nameof(inspection));
            return JsonSerializer.Serialize(inspection, new JsonSerializerOptions { WriteIndented = true }) +
                Environment.NewLine;
        }

        public static MtpMapExportResult Export(GameInstallation installation, string fileName,
            string outputPath, string metadataPath, bool overwrite = false)
        {
            PreparedMap prepared = Prepare(installation, fileName, outputPath, metadataPath);
            Preflight(new[] { prepared }, overwrite);
            Write(prepared, overwrite);
            return prepared.ToResult();
        }

        public static MtpMapBatchExportResult ExportAll(GameInstallation installation,
            string outputDirectory, bool overwrite = false)
        {
            if (installation == null)
                throw new ArgumentNullException(nameof(installation));
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentException("A map output directory is required.", nameof(outputDirectory));
            string fullOutputDirectory = Path.GetFullPath(outputDirectory);
            var prepared = new List<PreparedMap>();
            foreach (MtpMapProfile profile in Profiles)
            {
                string basePath = Path.Combine(fullOutputDirectory, "MAP" + profile.MapNumber);
                prepared.Add(Prepare(installation, profile.FileName, basePath + ".png", basePath + ".json"));
            }
            Preflight(prepared, overwrite);
            Directory.CreateDirectory(fullOutputDirectory);
            foreach (PreparedMap map in prepared)
                Write(map, overwrite);
            return new MtpMapBatchExportResult(fullOutputDirectory,
                prepared.Select(map => map.ToResult()).ToList());
        }

        private static PreparedMap Prepare(GameInstallation installation, string fileName,
            string outputPath, string metadataPath)
        {
            ValidateOutputPath(outputPath, ".png", "map PNG");
            ValidateOutputPath(metadataPath, ".json", "map metadata");
            string fullOutputPath = Path.GetFullPath(outputPath);
            string fullMetadataPath = Path.GetFullPath(metadataPath);
            if (fullOutputPath.Equals(fullMetadataPath, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Map image and metadata paths must be different.");

            Load(installation, fileName, out MtpMapProfile profile, out MtpMapRecord record);
            byte[] normalizedTiles = NormalizeTiles(record.TileIds, record.Width, record.Height,
                profile.TileOrder);
            byte[] pixels = RenderTiles(installation, profile.TileSetFileName,
                normalizedTiles, record.Width, record.Height);
            byte[] png = EgaIndexedPngEncoder.Encode(record.Width * TileWidth,
                record.Height * TileHeight, pixels);
            var inspection = new MtpMapInspection(profile, record);
            string metadata = JsonSerializer.Serialize(new
            {
                Inspection = inspection,
                HeaderBytes = Convert.ToHexString(record.HeaderBytes),
                NpcNameBytes = Convert.ToHexString(record.NpcNameBytes),
                BuildingNameBytes = Convert.ToHexString(record.BuildingNameBytes),
                InteractionXBytes = Convert.ToHexString(record.InteractionXBytes),
                InteractionYBytes = Convert.ToHexString(record.InteractionYBytes),
                CharacterXBytes = Convert.ToHexString(record.CharacterXBytes),
                CharacterYBytes = Convert.ToHexString(record.CharacterYBytes),
                AlternateBldBytes = Convert.ToHexString(record.AlternateBldBytes),
                MapStateBytes = Convert.ToHexString(record.MapStateBytes),
                SourceTileIds = Array.ConvertAll(record.TileIds, value => (int)value),
                RenderedTileIds = Array.ConvertAll(normalizedTiles, value => (int)value)
            }, new JsonSerializerOptions { WriteIndented = true });
            return new PreparedMap(inspection, fullOutputPath, fullMetadataPath,
                png, new UTF8Encoding(false).GetBytes(metadata));
        }

        private static void Load(GameInstallation installation, string fileName,
            out MtpMapProfile profile, out MtpMapRecord record)
        {
            if (installation == null)
                throw new ArgumentNullException(nameof(installation));
            string sourcePath = installation.ResolveFile(fileName);
            if (!Path.GetExtension(sourcePath).Equals(".MTP", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Map inspection requires an .MTP file.", nameof(fileName));
            string sourceName = Path.GetFileName(sourcePath);
            profile = Profiles.SingleOrDefault(candidate =>
                candidate.FileName.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
                throw new ArgumentException("No map profile is defined for " + sourceName + ".", nameof(fileName));
            byte[] data = File.ReadAllBytes(sourcePath);
            record = profile.HasStandardHeader
                ? MtpMapRecord.ParseStandard(data, sourceName)
                : MtpMapRecord.ParseStarMap(data, sourceName);
        }

        private static byte[] NormalizeTiles(byte[] source, int width, int height,
            MtpLegacyTileOrder tileOrder)
        {
            if (width % 8 != 0 || height % 8 != 0)
                throw new InvalidDataException("Legacy MTP tile ordering requires dimensions divisible by eight.");
            var output = new byte[source.Length];
            int traversalIndex = 0;
            for (int blockY = 0; blockY < height / 8; blockY++)
                for (int blockX = 0; blockX < width / 8; blockX++)
                    for (int tileY = 0; tileY < 8; tileY++)
                        for (int tileX = 0; tileX < 8; tileX++)
                        {
                            int rowMajorIndex = (blockY * 8 + tileY) * width + blockX * 8 + tileX;
                            if (tileOrder == MtpLegacyTileOrder.BlockTraversalReadIntoRowMajor)
                                output[rowMajorIndex] = source[traversalIndex];
                            else
                                output[traversalIndex] = source[rowMajorIndex];
                            traversalIndex++;
                        }
            return output;
        }

        private static byte[] RenderTiles(GameInstallation installation, string tileSetFileName,
            byte[] tileIds, int mapWidth, int mapHeight)
        {
            string tileSetPath = installation.ResolveFile(tileSetFileName);
            CompressedImage tileSet = CompressedImageDecoder.Decode(
                File.ReadAllBytes(tileSetPath), tileSetFileName);
            if (tileSet.RemainingPayloadBytes != 0)
                throw new InvalidDataException(tileSetFileName + " contains trailing compressed payload bytes.");
            int tileCount = tileSet.PaletteIndices.Length / TilePixelCount;
            byte[] tilePixels = tileSet.PaletteIndices;
            var output = new byte[checked(mapWidth * TileWidth * mapHeight * TileHeight)];
            for (int mapY = 0; mapY < mapHeight; mapY++)
                for (int mapX = 0; mapX < mapWidth; mapX++)
                {
                    int tileId = tileIds[mapY * mapWidth + mapX];
                    if (tileId >= tileCount)
                        throw new InvalidDataException("Map tile ID " + tileId + " exceeds " +
                            tileSetFileName + " tile count " + tileCount + ".");
                    int tileOffset = tileId * TilePixelCount;
                    for (int pixelY = 0; pixelY < TileHeight; pixelY++)
                        Array.Copy(tilePixels, tileOffset + pixelY * TileWidth, output,
                            (mapY * TileHeight + pixelY) * mapWidth * TileWidth + mapX * TileWidth,
                            TileWidth);
                }
            return output;
        }

        private static void ValidateOutputPath(string path, string extension, string description)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("An output path is required for " + description + ".");
            if (!Path.GetExtension(path).Equals(extension, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(description + " output must use a " + extension + " extension.");
        }

        private static void Preflight(IEnumerable<PreparedMap> maps, bool overwrite)
        {
            if (overwrite)
                return;
            foreach (PreparedMap map in maps)
            {
                if (File.Exists(map.OutputPath))
                    throw new IOException("Map output already exists: " + map.OutputPath +
                        ". Pass --force to overwrite the complete export.");
                if (File.Exists(map.MetadataPath))
                    throw new IOException("Map metadata already exists: " + map.MetadataPath +
                        ". Pass --force to overwrite the complete export.");
            }
        }

        private static void Write(PreparedMap map, bool overwrite)
        {
            CreateParentDirectory(map.OutputPath);
            CreateParentDirectory(map.MetadataPath);
            WriteBytes(map.OutputPath, map.PngBytes, overwrite);
            WriteBytes(map.MetadataPath, map.MetadataBytes, overwrite);
        }

        private static void CreateParentDirectory(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
        }

        private static void WriteBytes(string path, byte[] bytes, bool overwrite)
        {
            using (var output = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write, FileShare.None))
                output.Write(bytes, 0, bytes.Length);
        }

        private static IReadOnlyList<MtpMapProfile> CreateProfiles()
        {
            var profiles = new List<MtpMapProfile>();
            for (int map = 1; map <= 14; map++)
            {
                string tileSet = map == 11 ? "DESTRUCT.ICN" : map == 14 ? "STARLEAG.ICN" : "BTTLTECH.ICN";
                bool oldBlockFormat = map == 1 || map == 2 || map >= 11;
                profiles.Add(new MtpMapProfile(map, tileSet, oldBlockFormat
                    ? MtpLegacyTileOrder.RowMajorReadInBlockTraversal
                    : MtpLegacyTileOrder.BlockTraversalReadIntoRowMajor, true));
            }
            profiles.Add(new MtpMapProfile(15, "MAP.ICN",
                MtpLegacyTileOrder.BlockTraversalReadIntoRowMajor, false));
            return profiles.AsReadOnly();
        }

        private sealed class PreparedMap
        {
            internal PreparedMap(MtpMapInspection inspection, string outputPath, string metadataPath,
                byte[] pngBytes, byte[] metadataBytes)
            {
                Inspection = inspection;
                OutputPath = outputPath;
                MetadataPath = metadataPath;
                PngBytes = pngBytes;
                MetadataBytes = metadataBytes;
            }

            internal MtpMapInspection Inspection { get; }
            internal string OutputPath { get; }
            internal string MetadataPath { get; }
            internal byte[] PngBytes { get; }
            internal byte[] MetadataBytes { get; }
            internal MtpMapExportResult ToResult() =>
                new MtpMapExportResult(Inspection, OutputPath, MetadataPath, PngBytes.Length);
        }
    }
}
