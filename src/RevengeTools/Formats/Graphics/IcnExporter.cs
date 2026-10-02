using System.Security.Cryptography;
using System.Text.Json;
using CrescentHawksTools.Cli;
using RevengeTools.Formats.Palettes;
using RevengeTools.Installation;

namespace RevengeTools.Formats.Graphics;

public sealed class IcnExportResult
{
    public required string SourceFile { get; init; }
    public required string PaletteFile { get; init; }
    public required string OutputFile { get; init; }
    public required int TileCount { get; init; }
    public required int TileWidth { get; init; }
    public required int TileHeight { get; init; }
    public required int Columns { get; init; }
    public required int Rows { get; init; }
    public required int ConsumedPayloadBytes { get; init; }
    public required int PayloadLength { get; init; }
    public required int TrailingBytes { get; init; }
    public required string DecodedSha256 { get; init; }
}

public static class IcnExporter
{
    public const string DefaultPaletteFile = "MAP.COL";
    public const int DefaultColumns = 16;

    public static IcnExportResult Export(GameInstallation installation, string sourceFile,
        string? paletteFile, string outputFile, int columns, bool overwrite)
    {
        IcnImage image = IcnImage.Decode(File.ReadAllBytes(installation.ResolveFile(sourceFile)), sourceFile);
        string selectedPalette = paletteFile ?? DefaultPaletteFile;
        byte[] palette = ColPalette.Parse(
            File.ReadAllBytes(installation.ResolveFile(selectedPalette)), selectedPalette).Rgb;
        byte[] pixels = image.RenderContactSheet(columns, out int width, out int height);
        OutputFile.WriteBytes(outputFile, IndexedPngEncoder.Encode(width, height, pixels, palette), overwrite);
        return new IcnExportResult
        {
            SourceFile = Path.GetFileName(sourceFile),
            PaletteFile = selectedPalette,
            OutputFile = Path.GetFullPath(outputFile),
            TileCount = IcnImage.TileCount,
            TileWidth = IcnImage.TileWidth,
            TileHeight = IcnImage.TileHeight,
            Columns = columns,
            Rows = (IcnImage.TileCount + columns - 1) / columns,
            ConsumedPayloadBytes = image.ConsumedPayloadBytes,
            PayloadLength = image.PayloadLength,
            TrailingBytes = image.TrailingBytes,
            DecodedSha256 = Convert.ToHexString(SHA256.HashData(image.TilePixels)).ToLowerInvariant()
        };
    }

    public static IReadOnlyList<IcnExportResult> ExportAll(GameInstallation installation,
        string outputDirectory, int columns, bool overwrite)
    {
        string fullDirectory = Path.GetFullPath(outputDirectory);
        string[] sources = Directory.EnumerateFiles(installation.DirectoryPath, "*.ICN")
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase).ToArray();
        if (sources.Length == 0) throw new InvalidDataException("Installation contains no ICN files.");
        string[] outputs = sources.Select(path => Path.Combine(fullDirectory,
            Path.GetFileNameWithoutExtension(path) + ".png")).ToArray();
        string manifest = Path.Combine(fullDirectory, "manifest.json");
        if (!overwrite)
        {
            string? collision = outputs.Append(manifest).FirstOrDefault(File.Exists);
            if (collision is not null)
                throw new IOException("Output already exists: " + collision + ". Pass --force to overwrite it.");
        }
        Directory.CreateDirectory(fullDirectory);
        var results = new List<IcnExportResult>();
        for (int index = 0; index < sources.Length; index++)
            results.Add(Export(installation, Path.GetFileName(sources[index]), null,
                outputs[index], columns, overwrite));
        OutputFile.WriteText(manifest,
            JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            overwrite);
        return results.AsReadOnly();
    }
}
