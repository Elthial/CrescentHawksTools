using System;
using System.IO;
using System.Text;
using System.Text.Json;
using InceptionTools.Installation;

namespace InceptionTools.Graphics
{
    public sealed class MechSpriteSheetExportResult
    {
        internal MechSpriteSheetExportResult(string sourceFileName, string outputPath,
            string metadataPath, int width, int height, int sequenceCount, int sourceSpriteCount)
        {
            SourceFileName = sourceFileName;
            OutputPath = outputPath;
            MetadataPath = metadataPath;
            Width = width;
            Height = height;
            SequenceCount = sequenceCount;
            SourceSpriteCount = sourceSpriteCount;
        }

        public string SourceFileName { get; }
        public string OutputPath { get; }
        public string MetadataPath { get; }
        public int Width { get; }
        public int Height { get; }
        public int SequenceCount { get; }
        public int SourceSpriteCount { get; }
    }

    public static class MechSpriteSheetExporter
    {
        public static MechSpriteSheetExportResult Export(GameInstallation installation,
            string outputPath, string metadataPath, bool overwrite = false)
        {
            if (installation == null)
                throw new ArgumentNullException(nameof(installation));
            ValidateOutputPath(outputPath, ".png", "spritesheet PNG");
            ValidateOutputPath(metadataPath, ".json", "spritesheet metadata");

            string fullOutputPath = Path.GetFullPath(outputPath);
            string fullMetadataPath = Path.GetFullPath(metadataPath);
            if (fullOutputPath.Equals(fullMetadataPath, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Spritesheet image and metadata paths must be different.");
            if (!overwrite)
            {
                if (File.Exists(fullOutputPath))
                    throw new IOException("Spritesheet output already exists: " + fullOutputPath +
                        ". Pass --force to overwrite both outputs.");
                if (File.Exists(fullMetadataPath))
                    throw new IOException("Spritesheet metadata already exists: " + fullMetadataPath +
                        ". Pass --force to overwrite both outputs.");
            }

            string sourcePath = installation.ResolveFile("MECHSHAP.CMP");
            string sourceFileName = Path.GetFileName(sourcePath);
            CompressedImage source = CompressedImageDecoder.Decode(
                File.ReadAllBytes(sourcePath), sourceFileName);
            if (source.CompressionFormat != 2)
                throw new InvalidDataException(sourceFileName + " must use verified column-oriented format 2.");
            if (source.RemainingPayloadBytes != 0)
                throw new InvalidDataException(sourceFileName + " has 0x" + source.RemainingPayloadBytes.ToString("X") +
                    " bytes remaining after its complete decoded image.");

            MechSpriteSheet sheet = MechSpriteSheetComposer.Compose(source);
            byte[] png = EgaIndexedPngEncoder.Encode(sheet.Width, sheet.Height,
                sheet.PaletteIndices, MechSpriteSheetComposer.TransparentPaletteIndex);
            string metadata = JsonSerializer.Serialize(new
            {
                SourceFileName = sourceFileName,
                SourceCompressionFormat = source.CompressionFormat,
                SourceWidth = CompressedImage.Width,
                SourceHeight = CompressedImage.Height,
                SourceSpriteCount = MechShapeSpriteCatalog.All.Count,
                Width = sheet.Width,
                Height = sheet.Height,
                CellWidth = sheet.CellWidth,
                CellHeight = sheet.CellHeight,
                TransparentPaletteIndex = MechSpriteSheetComposer.TransparentPaletteIndex,
                Sequences = sheet.Sequences
            }, new JsonSerializerOptions { WriteIndented = true });

            CreateParentDirectory(fullOutputPath);
            CreateParentDirectory(fullMetadataPath);
            WriteBytes(fullOutputPath, png, overwrite);
            WriteText(fullMetadataPath, metadata, overwrite);
            return new MechSpriteSheetExportResult(sourceFileName, fullOutputPath, fullMetadataPath,
                sheet.Width, sheet.Height, sheet.Sequences.Count, MechShapeSpriteCatalog.All.Count);
        }

        private static void ValidateOutputPath(string path, string extension, string description)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("An output path is required for " + description + ".");
            if (!Path.GetExtension(path).Equals(extension, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(description + " output must use a " + extension + " extension.");
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

        private static void WriteText(string path, string value, bool overwrite)
        {
            using (var output = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(output, new UTF8Encoding(false)))
                writer.Write(value);
        }
    }
}
