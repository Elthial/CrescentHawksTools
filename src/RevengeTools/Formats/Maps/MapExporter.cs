using System.Security.Cryptography;
using CrescentHawksTools.Cli;
using RevengeTools.Formats.Graphics;
using RevengeTools.Formats.Palettes;
using RevengeTools.Installation;

namespace RevengeTools.Formats.Maps;

public sealed class MapExportResult
{
    public required string SourceFile { get; init; }
    public required string IconSetFile { get; init; }
    public required string PaletteFile { get; init; }
    public required string OutputFile { get; init; }
    public required int MapWidth { get; init; }
    public required int MapHeight { get; init; }
    public required int PixelWidth { get; init; }
    public required int PixelHeight { get; init; }
    public required int DistinctTileIds { get; init; }
    public required int MaximumTileId { get; init; }
    public required string PixelSha256 { get; init; }
}

public static class MapExporter
{
    public const string DefaultPaletteFile = "MAP.COL";

    public static MapExportResult Export(GameInstallation installation, string mapFile,
        string iconSetFile, string? paletteFile, string outputFile, bool overwrite)
    {
        RevengeMap map = RevengeMap.Parse(File.ReadAllBytes(installation.ResolveFile(mapFile)), mapFile);
        IcnImage icons = IcnImage.Decode(File.ReadAllBytes(installation.ResolveFile(iconSetFile)), iconSetFile);
        string selectedPalette = paletteFile ?? DefaultPaletteFile;
        byte[] palette = ColPalette.Parse(
            File.ReadAllBytes(installation.ResolveFile(selectedPalette)), selectedPalette).Rgb;
        byte[] tileIds = map.TileIds;
        int maximum = tileIds.Max();
        if (maximum >= IcnImage.TileCount)
            throw new InvalidDataException($"{mapFile} references tile {maximum}, beyond {iconSetFile}'s {IcnImage.TileCount} tiles.");
        int width = checked(map.Width * IcnImage.TileWidth);
        int height = checked(map.Height * IcnImage.TileHeight);
        byte[] output = new byte[checked(width * height)];
        byte[] tilePixels = icons.TilePixels;
        for (int mapY = 0; mapY < map.Height; mapY++)
        {
            for (int mapX = 0; mapX < map.Width; mapX++)
            {
                int tile = tileIds[mapY * map.Width + mapX];
                int sourceOffset = tile * IcnImage.TileWidth * IcnImage.TileHeight;
                int targetX = mapX * IcnImage.TileWidth;
                int targetY = mapY * IcnImage.TileHeight;
                for (int tileY = 0; tileY < IcnImage.TileHeight; tileY++)
                    Buffer.BlockCopy(tilePixels, sourceOffset + tileY * IcnImage.TileWidth,
                        output, (targetY + tileY) * width + targetX, IcnImage.TileWidth);
            }
        }
        OutputFile.WriteBytes(outputFile, IndexedPngEncoder.Encode(width, height, output, palette), overwrite);
        return new MapExportResult
        {
            SourceFile = Path.GetFileName(mapFile),
            IconSetFile = Path.GetFileName(iconSetFile),
            PaletteFile = selectedPalette,
            OutputFile = Path.GetFullPath(outputFile),
            MapWidth = map.Width,
            MapHeight = map.Height,
            PixelWidth = width,
            PixelHeight = height,
            DistinctTileIds = tileIds.Distinct().Count(),
            MaximumTileId = maximum,
            PixelSha256 = Convert.ToHexString(SHA256.HashData(output)).ToLowerInvariant()
        };
    }
}
