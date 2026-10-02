using System.Security.Cryptography;
using System.Text.Json;
using RevengeTools.Cli;
using RevengeTools.Formats.Palettes;
using RevengeTools.Installation;

namespace RevengeTools.Formats.Graphics;

public sealed class CpsExportResult
{
    public required string SourceFile { get; init; }
    public required string PaletteFile { get; init; }
    public required string OutputFile { get; init; }
    public required int CompressionType { get; init; }
    public required string CompressionName { get; init; }
    public required int DecodedBytesWritten { get; init; }
    public required int CompressedBytesConsumed { get; init; }
    public required int CompressedBytesAvailable { get; init; }
    public required string DecodedSha256 { get; init; }
}

public static class CpsExporter
{
    public static CpsExportResult Export(GameInstallation installation, string sourceFile,
        string? paletteFile, string outputFile, bool overwrite)
    {
        string sourcePath = installation.ResolveFile(sourceFile);
        CpsImage image = CpsImage.Decode(File.ReadAllBytes(sourcePath), sourceFile);
        string selectedPalette = paletteFile ?? CpsPaletteProfiles.GetPaletteFileName(sourceFile);
        byte[] paletteRgb;
        if (image.PaletteSize == 768 && paletteFile is null)
        {
            paletteRgb = ColPalette.Parse(image.EmbeddedPaletteBytes, sourceFile + " embedded palette").Rgb;
            selectedPalette = "embedded";
        }
        else
            paletteRgb = ColPalette.Parse(File.ReadAllBytes(installation.ResolveFile(selectedPalette)), selectedPalette).Rgb;
        byte[] pixels = image.Pixels;
        OutputFile.WriteBytes(outputFile, IndexedPngEncoder.Encode(CpsImage.Width, CpsImage.Height, pixels, paletteRgb), overwrite);
        return new CpsExportResult
        {
            SourceFile = Path.GetFileName(sourceFile),
            PaletteFile = selectedPalette,
            OutputFile = Path.GetFullPath(outputFile),
            CompressionType = image.CompressionType,
            CompressionName = image.CompressionName,
            DecodedBytesWritten = image.DecodedBytesWritten,
            CompressedBytesConsumed = image.CompressedBytesConsumed,
            CompressedBytesAvailable = image.CompressedBytesAvailable,
            DecodedSha256 = Convert.ToHexString(SHA256.HashData(pixels)).ToLowerInvariant()
        };
    }

    public static IReadOnlyList<CpsExportResult> ExportAll(GameInstallation installation,
        string outputDirectory, bool overwrite)
    {
        string fullDirectory = Path.GetFullPath(outputDirectory);
        string[] sources = Directory.EnumerateFiles(installation.DirectoryPath, "*.CPS")
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase).ToArray();
        if (sources.Length == 0) throw new InvalidDataException("Installation contains no CPS files.");
        string[] outputs = sources.Select(path => Path.Combine(fullDirectory, Path.GetFileNameWithoutExtension(path) + ".png")).ToArray();
        string manifest = Path.Combine(fullDirectory, "manifest.json");
        if (!overwrite)
        {
            string? collision = outputs.Append(manifest).FirstOrDefault(File.Exists);
            if (collision is not null) throw new IOException("Output already exists: " + collision + ". Pass --force to overwrite it.");
        }

        Directory.CreateDirectory(fullDirectory);
        var results = new List<CpsExportResult>();
        for (int index = 0; index < sources.Length; index++)
            results.Add(Export(installation, Path.GetFileName(sources[index]), null, outputs[index], overwrite));
        string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        OutputFile.WriteText(manifest, json + Environment.NewLine, overwrite);
        return results.AsReadOnly();
    }
}
