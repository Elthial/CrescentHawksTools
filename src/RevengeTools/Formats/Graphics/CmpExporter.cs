using System.Security.Cryptography;
using CrescentHawksTools.Cli;
using RevengeTools.Formats.Palettes;
using RevengeTools.Installation;

namespace RevengeTools.Formats.Graphics;

public sealed class CmpExportResult
{
    public required string SourceFile { get; init; }
    public required string PaletteFile { get; init; }
    public required string OutputFile { get; init; }
    public required int CompressionType { get; init; }
    public required string CompressionName { get; init; }
    public required int ConsumedPayloadBytes { get; init; }
    public required int PayloadLength { get; init; }
    public required string DecodedSha256 { get; init; }
}

public static class CmpExporter
{
    public const string DefaultPaletteFile = "MAPS.COL";

    public static CmpExportResult Export(GameInstallation installation, string sourceFile,
        string? paletteFile, string outputFile, bool overwrite)
    {
        string sourcePath = installation.ResolveFile(sourceFile);
        CmpImage image = CmpImage.Decode(File.ReadAllBytes(sourcePath), sourceFile);
        string selectedPalette = paletteFile ?? DefaultPaletteFile;
        byte[] palette = ColPalette.Parse(
            File.ReadAllBytes(installation.ResolveFile(selectedPalette)), selectedPalette).Rgb;
        byte[] pixels = image.Pixels;
        OutputFile.WriteBytes(outputFile,
            IndexedPngEncoder.Encode(CmpImage.Width, CmpImage.Height, pixels, palette), overwrite);
        return new CmpExportResult
        {
            SourceFile = Path.GetFileName(sourceFile),
            PaletteFile = selectedPalette,
            OutputFile = Path.GetFullPath(outputFile),
            CompressionType = image.CompressionType,
            CompressionName = image.CompressionName,
            ConsumedPayloadBytes = image.ConsumedPayloadBytes,
            PayloadLength = image.PayloadLength,
            DecodedSha256 = Convert.ToHexString(SHA256.HashData(pixels)).ToLowerInvariant()
        };
    }
}
