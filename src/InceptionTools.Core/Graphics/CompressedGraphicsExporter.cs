using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using InceptionTools.Installation;

namespace InceptionTools.Graphics;

public sealed class CompressedGraphicsProfile
{
    private readonly byte[] _paletteRgb;

    internal CompressedGraphicsProfile(string fileName, string purpose, int width,
        int height, string paletteName, byte[] paletteRgb)
    {
        FileName = fileName;
        Purpose = purpose;
        Width = width;
        Height = height;
        PaletteName = paletteName;
        _paletteRgb = (byte[])paletteRgb.Clone();
    }

    public string FileName { get; }
    public string Purpose { get; }
    public int Width { get; }
    public int Height { get; }
    public string PaletteName { get; }
    public byte[] PaletteRgb => (byte[])_paletteRgb.Clone();
}

public sealed class CompressedGraphicsInspection
{
    internal CompressedGraphicsInspection(CompressedGraphicsProfile profile, CompressedImage image)
    {
        FileName = profile.FileName;
        Purpose = profile.Purpose;
        Width = profile.Width;
        Height = profile.Height;
        PaletteName = profile.PaletteName;
        StoredLength = image.StoredLength;
        CompressionFormat = image.CompressionFormat;
        ConsumedPayloadBytes = image.ConsumedPayloadBytes;
        RemainingPayloadBytes = image.RemainingPayloadBytes;
    }

    public string FileName { get; }
    public string Purpose { get; }
    public int Width { get; }
    public int Height { get; }
    public string PaletteName { get; }
    public int StoredLength { get; }
    public int CompressionFormat { get; }
    public int ConsumedPayloadBytes { get; }
    public int RemainingPayloadBytes { get; }
}

public sealed class CompressedGraphicsExportResult
{
    internal CompressedGraphicsExportResult(CompressedGraphicsInspection inspection,
        string outputPath, int outputLength)
    {
        Inspection = inspection;
        OutputPath = outputPath;
        OutputLength = outputLength;
    }

    public CompressedGraphicsInspection Inspection { get; }
    public string OutputPath { get; }
    public int OutputLength { get; }
}

public sealed class CompressedGraphicsBatchExportResult
{
    internal CompressedGraphicsBatchExportResult(string outputDirectory,
        List<CompressedGraphicsExportResult> images)
    {
        OutputDirectory = outputDirectory;
        Images = images.AsReadOnly();
    }

    public string OutputDirectory { get; }
    public IReadOnlyList<CompressedGraphicsExportResult> Images { get; }
    public int TotalOutputLength => Images.Sum(image => image.OutputLength);
}

/// <summary>
/// Portable replacement for the legacy System.Drawing CMP/ICN image path.
/// It retains the legacy ICN 16-pixel-wide tile-strip view while using the
/// verified bounded decoder and dependency-free indexed PNG writer.
/// </summary>
public static class CompressedGraphicsExporter
{
    private static readonly byte[] StandardPalette =
    {
        0x00, 0x00, 0x00,  0x00, 0x00, 0xAA,  0x00, 0xAA, 0x00,  0x00, 0xAA, 0xAA,
        0xAA, 0x00, 0x00,  0xAA, 0x00, 0xAA,  0xAA, 0x55, 0x00,  0xAA, 0xAA, 0xAA,
        0x55, 0x55, 0x55,  0x55, 0x55, 0xFF,  0x55, 0xFF, 0x55,  0x55, 0xFF, 0xFF,
        0xFF, 0x55, 0x55,  0xFF, 0x55, 0xFF,  0xFF, 0xFF, 0x55,  0xFF, 0xFF, 0xFF
    };

    private static readonly string[] KnownFiles =
    {
        "ANIMATE.ICN", "BTTLTECH.ICN", "DESTRUCT.ICN", "MAP.ICN", "STARLEAG.ICN",
        "BTBORDER.CMP", "BTSTATS.CMP", "BTTITLE.CMP", "ENDMECH.CMP", "INFOCOM.CMP",
        "MECHSHAP.CMP", "TINYLAND.CMP"
    };

    public static IReadOnlyList<string> SupportedFileNames => Array.AsReadOnly(KnownFiles);

    public static CompressedGraphicsInspection Inspect(GameInstallation installation, string fileName)
    {
        Load(installation, fileName, out CompressedGraphicsProfile profile, out CompressedImage image);
        return new CompressedGraphicsInspection(profile, image);
    }

    public static string WriteText(CompressedGraphicsInspection inspection)
    {
        if (inspection == null)
            throw new ArgumentNullException(nameof(inspection));
        return inspection.FileName + Environment.NewLine +
            "  purpose: " + inspection.Purpose + Environment.NewLine +
            "  dimensions: " + inspection.Width + "x" + inspection.Height + Environment.NewLine +
            "  palette: " + inspection.PaletteName + Environment.NewLine +
            "  compression format: " + inspection.CompressionFormat + Environment.NewLine +
            "  stored length: 0x" + inspection.StoredLength.ToString("X") + Environment.NewLine +
            "  payload consumed: 0x" + inspection.ConsumedPayloadBytes.ToString("X") + Environment.NewLine +
            "  payload remaining: 0x" + inspection.RemainingPayloadBytes.ToString("X") + Environment.NewLine;
    }

    public static string WriteJson(CompressedGraphicsInspection inspection)
    {
        if (inspection == null)
            throw new ArgumentNullException(nameof(inspection));
        return JsonSerializer.Serialize(inspection, new JsonSerializerOptions { WriteIndented = true }) +
            Environment.NewLine;
    }

    public static CompressedGraphicsExportResult ExportPng(GameInstallation installation,
        string fileName, string outputPath, bool overwrite = false)
    {
        Load(installation, fileName, out CompressedGraphicsProfile profile, out CompressedImage image);
        PreparedImage prepared = Prepare(profile, image, outputPath);
        Write(prepared, overwrite);
        return prepared.ToResult();
    }

    public static CompressedGraphicsBatchExportResult ExportAllPng(GameInstallation installation,
        string outputDirectory, bool overwrite = false)
    {
        if (installation == null)
            throw new ArgumentNullException(nameof(installation));
        if (string.IsNullOrWhiteSpace(outputDirectory))
            throw new ArgumentException("A graphics output directory is required.", nameof(outputDirectory));

        string fullOutputDirectory = Path.GetFullPath(outputDirectory);
        var prepared = new List<PreparedImage>();
        foreach (string fileName in KnownFiles)
        {
            Load(installation, fileName, out CompressedGraphicsProfile profile, out CompressedImage image);
            prepared.Add(Prepare(profile, image, Path.Combine(fullOutputDirectory,
                Path.GetFileNameWithoutExtension(profile.FileName) + ".png")));
        }

        if (!overwrite)
            foreach (PreparedImage image in prepared)
                if (File.Exists(image.OutputPath))
                    throw new IOException("Graphics output already exists: " + image.OutputPath +
                        ". Pass --force to overwrite the complete batch.");

        Directory.CreateDirectory(fullOutputDirectory);
        foreach (PreparedImage image in prepared)
            Write(image, overwrite);
        return new CompressedGraphicsBatchExportResult(fullOutputDirectory,
            prepared.Select(image => image.ToResult()).ToList());
    }

    private static void Load(GameInstallation installation, string fileName,
        out CompressedGraphicsProfile profile, out CompressedImage image)
    {
        if (installation == null)
            throw new ArgumentNullException(nameof(installation));
        string sourcePath = installation.ResolveFile(fileName);
        string extension = Path.GetExtension(sourcePath);
        if (!extension.Equals(".CMP", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".ICN", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Compressed graphics inspection requires a .CMP or .ICN file.",
                nameof(fileName));

        string sourceFileName = Path.GetFileName(sourcePath);
        profile = CreateProfile(sourceFileName);
        image = CompressedImageDecoder.Decode(File.ReadAllBytes(sourcePath), sourceFileName);
        if (image.RemainingPayloadBytes != 0)
            throw new InvalidDataException(sourceFileName + " has 0x" +
                image.RemainingPayloadBytes.ToString("X") +
                " bytes remaining after its complete decoded image.");
    }

    private static CompressedGraphicsProfile CreateProfile(string fileName)
    {
        bool tileStrip = Path.GetExtension(fileName).Equals(".ICN", StringComparison.OrdinalIgnoreCase);
        int width = tileStrip ? 16 : CompressedImage.Width;
        int height = CompressedImage.Width * CompressedImage.Height / width;
        string purpose = tileStrip ? "16x16 tile strip" : GetCmpPurpose(fileName);
        byte[] palette = (byte[])StandardPalette.Clone();
        string paletteName = "standard EGA";

        if (fileName.Equals("BTTITLE.CMP", StringComparison.OrdinalIgnoreCase))
        {
            SetColour(palette, 1, 0x00, 0x00, 0x00);
            paletteName = "BTTITLE legacy override";
        }
        else if (fileName.Equals("INFOCOM.CMP", StringComparison.OrdinalIgnoreCase))
        {
            SetColour(palette, 9, 0x00, 0x00, 0xAA);
            SetColour(palette, 5, 0x55, 0x55, 0xFF);
            paletteName = "INFOCOM legacy override";
        }
        else if (fileName.Equals("ENDMECH.CMP", StringComparison.OrdinalIgnoreCase))
        {
            SetColour(palette, 1, 0x00, 0x00, 0x00);
            SetColour(palette, 13, 0x55, 0x55, 0xFF);
            SetColour(palette, 9, 0x00, 0x00, 0xAA);
            paletteName = "ENDMECH legacy override";
        }

        return new CompressedGraphicsProfile(fileName, purpose, width, height,
            paletteName, palette);
    }

    private static string GetCmpPurpose(string fileName)
    {
        if (fileName.Equals("MECHSHAP.CMP", StringComparison.OrdinalIgnoreCase))
            return "sprite source sheet";
        if (fileName.Equals("BTBORDER.CMP", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("TINYLAND.CMP", StringComparison.OrdinalIgnoreCase))
            return "packed screen/tiny-tile source";
        return "320x200 screen";
    }

    private static void SetColour(byte[] palette, int index, byte red, byte green, byte blue)
    {
        int offset = index * 3;
        palette[offset] = red;
        palette[offset + 1] = green;
        palette[offset + 2] = blue;
    }

    private static PreparedImage Prepare(CompressedGraphicsProfile profile,
        CompressedImage image, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("A graphics output PNG path is required.", nameof(outputPath));
        if (!Path.GetExtension(outputPath).Equals(".png", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Graphics output must use a .png extension.", nameof(outputPath));
        byte[] png = EgaIndexedPngEncoder.Encode(profile.Width, profile.Height,
            image.PaletteIndices, profile.PaletteRgb);
        return new PreparedImage(new CompressedGraphicsInspection(profile, image),
            Path.GetFullPath(outputPath), png);
    }

    private static void Write(PreparedImage prepared, bool overwrite)
    {
        string? directory = Path.GetDirectoryName(prepared.OutputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        using (var output = new FileStream(prepared.OutputPath,
            overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None))
            output.Write(prepared.Bytes, 0, prepared.Bytes.Length);
    }

    private sealed class PreparedImage
    {
        internal PreparedImage(CompressedGraphicsInspection inspection, string outputPath, byte[] bytes)
        {
            Inspection = inspection;
            OutputPath = outputPath;
            Bytes = bytes;
        }

        internal CompressedGraphicsInspection Inspection { get; }
        internal string OutputPath { get; }
        internal byte[] Bytes { get; }

        internal CompressedGraphicsExportResult ToResult() =>
            new CompressedGraphicsExportResult(Inspection, OutputPath, Bytes.Length);
    }
}
