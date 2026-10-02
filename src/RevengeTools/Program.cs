using System.Globalization;
using System.Text;
using System.Text.Json;
using RevengeTools.Cli;
using RevengeTools.Formats.Audio;
using RevengeTools.Formats.Fonts;
using RevengeTools.Formats.Graphics;
using RevengeTools.Formats.Maps;
using RevengeTools.Formats.Music;
using RevengeTools.Formats.Palettes;
using RevengeTools.Formats.Saves;
using RevengeTools.Formats.Scenes;
using RevengeTools.Formats.Units;
using RevengeTools.Formats.Weapons;
using RevengeTools.Installation;
using RevengeTools.SaveEditing;

namespace RevengeTools;

public static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                PrintHelp();
                return 0;
            }

            string command = args[0].ToLowerInvariant();
            CommandLine options = CommandLine.Parse(args.Skip(1));
            return command switch
            {
                "inventory" => RunInventory(options),
                "inspect" => RunInspect(options),
                "inspect-palette" => RunPaletteInspection(options),
                "export-palette" => RunPaletteExport(options),
                "inspect-font" => RunFontInspection(options),
                "export-font" => RunFontExport(options),
                "inspect-image" => RunImageInspection(options),
                "export-image" => RunImageExport(options),
                "export-images" => RunImagesExport(options),
                "inspect-cmp" => RunCmpInspection(options),
                "export-cmp" => RunCmpExport(options),
                "inspect-icn" => RunIcnInspection(options),
                "export-icn" => RunIcnExport(options),
                "export-icns" => RunIcnsExport(options),
                "inspect-unit-sprites" => RunUnitSpriteInspection(options),
                "inspect-cga-translation" => RunCgaTranslationInspection(options),
                "inspect-map" => RunMapInspection(options),
                "export-map" => RunMapExport(options),
                "inspect-scene" => RunSceneInspection(options),
                "export-scenes" => RunScenesExport(options),
                "export-scene-evidence" => RunSceneEvidenceExport(options),
                "export-scene-map" => RunSceneMapExport(options),
                "dump-unit-types" => RunUnitTypes(options),
                "dump-unit-type" => RunUnitType(options),
                "inspect-hit-locations" => RunHitLocationInspection(options),
                "export-weapon-evidence" => RunWeaponEvidenceExport(options),
                "inspect-speaker-effect" => RunMusicStreamInspection(options),
                "export-speaker-effect-evidence" => RunMusicEvidenceExport(options),
                "inspect-music-stream" => RunMusicStreamInspection(options), // legacy alias
                "export-music-evidence" => RunMusicEvidenceExport(options), // legacy alias
                "inspect-digital-sounds" => RunDigitalSoundInspection(options),
                "export-digital-sounds" => RunDigitalSoundExport(options),
                "dump-save" => RunSave(options),
                "dump-save-slot" => RunSaveSlot(options),
                "compare-save" => RunSaveComparison(options),
                "analyze-save-set" => RunSaveSetAnalysis(options),
                "export-save-state" => RunSaveStateExport(options),
                "import-save-state" => RunSaveStateImport(options),
                _ => throw new ArgumentException($"Unknown command '{args[0]}'. Run 'help' for available commands.")
            };
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or InvalidDataException or
                                          UnauthorizedAccessException or JsonException or OverflowException)
        {
            Console.Error.WriteLine("Error: " + exception.Message);
            return 1;
        }
    }

    private static int RunInventory(CommandLine options)
    {
        options.RequireOnly("game-dir", "hash", "json", "output");
        GameInstallation installation = Locate(options);
        InstallationInventoryReport report = InstallationInventory.Scan(installation, options.Has("hash"));
        string text = options.Has("json") ? JsonSerializer.Serialize(report, JsonOptions) : FormatInventory(report);
        WriteOrPrint(text, options.Get("output"), options.Has("force"));
        return report.IsValid ? 0 : 2;
    }

    private static int RunInspect(CommandLine options)
    {
        options.RequireOnly("game-dir", "offset", "count");
        string file = RequirePositional(options, 0, "inspect requires FILE.");
        GameInstallation installation = Locate(options);
        byte[] data = File.ReadAllBytes(installation.ResolveFile(file));
        int offset = options.Get("offset") is string offsetText ? options.ParseNumber(offsetText) : 0;
        int count = options.Get("count") is string countText ? options.ParseNumber(countText) : 0x80;
        if (count is < 0 or > 0x1000) throw new ArgumentOutOfRangeException(nameof(count), "Count must be 0..0x1000.");
        if (offset < 0 || offset > data.Length || count > data.Length - offset)
            throw new InvalidDataException("Requested inspection range exceeds the file.");
        Console.Write(FormatHex(data, offset, count));
        return 0;
    }

    private static int RunPaletteInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string file = RequirePositional(options, 0, "inspect-palette requires FILE.COL.");
        ColPalette palette = ColPalette.Parse(ReadInstallationFile(options, file), file);
        object report = new { file, palette.ColorCount, palette.MaximumSourceComponent, sourceComponentBits = 6, channelOrder = "RGB (working verified render profile)" };
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(report, JsonOptions) :
            $"{file}: {palette.ColorCount} colours; maximum source component {palette.MaximumSourceComponent}; 6-bit RGB expanded to 8-bit.");
        return 0;
    }

    private static int RunPaletteExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output", "force");
        string file = RequirePositional(options, 0, "export-palette requires FILE.COL.");
        ColPalette palette = ColPalette.Parse(ReadInstallationFile(options, file), file);
        string output = options.Get("output") ?? Path.GetFileNameWithoutExtension(file) + "-palette.png";
        OutputFile.WriteBytes(output, palette.RenderSwatches(), options.Has("force"));
        Console.WriteLine(Path.GetFullPath(output));
        return 0;
    }

    private static int RunFontInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string file = RequirePositional(options, 0, "inspect-font requires FILE.FNT.");
        WestwoodFontV2 font = WestwoodFontV2.Parse(ReadInstallationFile(options, file), file);
        object report = new { file, font.DeclaredSize, font.Width, font.Height, font.GlyphCount, firstGlyphOffset = font.Offsets[0], lastGlyphOffset = font.Offsets[^1] };
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(report, JsonOptions) :
            $"{file}: Font v2, {font.GlyphCount} glyphs, {font.Width}x{font.Height}, bitmap range 0x{font.Offsets[0]:X4}..0x{font.Offsets[^1]:X4}.");
        return 0;
    }

    private static int RunFontExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output", "force", "scale");
        string file = RequirePositional(options, 0, "export-font requires FILE.FNT.");
        WestwoodFontV2 font = WestwoodFontV2.Parse(ReadInstallationFile(options, file), file);
        int scale = options.Get("scale") is string scaleText ? options.ParseNumber(scaleText) : 2;
        string output = options.Get("output") ?? Path.GetFileNameWithoutExtension(file) + "-font.png";
        OutputFile.WriteBytes(output, font.RenderContactSheet(scale), options.Has("force"));
        Console.WriteLine(Path.GetFullPath(output));
        return 0;
    }

    private static int RunImageInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string file = RequirePositional(options, 0, "inspect-image requires FILE.CPS.");
        CpsImage image = CpsImage.Decode(ReadInstallationFile(options, file), file);
        byte[] pixels = image.Pixels;
        var report = new
        {
            file,
            image.FileSize,
            image.CompressionType,
            image.CompressionName,
            image.UncompressedSize,
            image.PaletteSize,
            image.DecodedBytesWritten,
            image.CompressedBytesConsumed,
            image.CompressedBytesAvailable,
            image.TrailingCompressedBytes,
            distinctPaletteIndices = pixels.Distinct().Count(),
            minimumPaletteIndex = pixels.Min(),
            maximumPaletteIndex = pixels.Max()
        };
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(report, JsonOptions) :
            $"{file}: {image.CompressionName}, {CpsImage.Width}x{CpsImage.Height}, " +
            $"{image.CompressedBytesConsumed}/{image.CompressedBytesAvailable} compressed bytes consumed, " +
            $"{image.DecodedBytesWritten}/{image.UncompressedSize} output bytes written, " +
            $"{pixels.Distinct().Count()} palette indices ({pixels.Min()}..{pixels.Max()}).");
        return 0;
    }

    private static int RunImageExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "palette", "output", "force", "json");
        string file = RequirePositional(options, 0, "export-image requires FILE.CPS.");
        string output = options.Get("output") ?? Path.GetFileNameWithoutExtension(file) + ".png";
        CpsExportResult result = CpsExporter.Export(Locate(options), file, options.Get("palette"), output, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(result, JsonOptions) :
            $"{result.SourceFile} -> {result.OutputFile} ({result.CompressionName}, palette {result.PaletteFile})");
        return 0;
    }

    private static int RunImagesExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output-dir", "force", "json");
        string outputDirectory = options.Get("output-dir") ?? Path.Combine("RevengeTools.Output", "cps");
        IReadOnlyList<CpsExportResult> results = CpsExporter.ExportAll(Locate(options), outputDirectory, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(results, JsonOptions) :
            $"Exported {results.Count} CPS images to {Path.GetFullPath(outputDirectory)}");
        return 0;
    }

    private static int RunCmpInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string file = RequirePositional(options, 0, "inspect-cmp requires FILE.CMP.");
        CmpImage image = CmpImage.Decode(ReadInstallationFile(options, file), file);
        byte[] pixels = image.Pixels;
        object report = new
        {
            file,
            catalog = IcnCatalog.Classify(file),
            image.StoredLength,
            image.CompressionType,
            image.CompressionName,
            width = CmpImage.Width,
            height = CmpImage.Height,
            image.ConsumedPayloadBytes,
            image.PayloadLength,
            image.RemainingPayloadBytes,
            distinctPaletteIndices = pixels.Distinct().Count(),
            minimumPaletteIndex = pixels.Min(),
            maximumPaletteIndex = pixels.Max()
        };
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(report, JsonOptions) :
            $"{file}: {image.CompressionName}, {CmpImage.Width}x{CmpImage.Height}, " +
            $"{image.ConsumedPayloadBytes}/{image.PayloadLength} payload bytes consumed, " +
            $"{pixels.Distinct().Count()} palette indices ({pixels.Min()}..{pixels.Max()}).");
        return 0;
    }

    private static int RunUnitSpriteInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json", "output", "force");
        GameInstallation installation = Locate(options);
        IReadOnlyList<UnitRecord> units = UnitCatalog.ParseTemplates(
            File.ReadAllBytes(installation.ResolveFile("MECHTYPE.DAT")), "MECHTYPE.DAT");
        IReadOnlyList<UnitSpriteCatalogEntry> catalog = IcnCatalog.BuildUnitSpriteCatalog(units);
        string text = options.Has("json")
            ? JsonSerializer.Serialize(catalog, JsonOptions)
            : string.Join(Environment.NewLine, catalog.Select(entry =>
                $"tile row 0x{entry.LocalTileBase:X2} / sprite 0x{entry.GlobalSpriteBase:X3}: " +
                string.Join(", ", entry.UnitNames)));
        WriteOrPrint(text, options.Get("output"), options.Has("force"));
        return 0;
    }

    private static int RunCgaTranslationInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json", "output", "force");
        GameInstallation installation = Locate(options);
        CgaPixelTranslation tables = CgaPixelTranslationCatalog.Parse(
            File.ReadAllBytes(installation.ResolveFile("REVENGE.EXE")));
        string text = options.Has("json")
            ? JsonSerializer.Serialize(new
            {
                seedTableHex = Convert.ToHexString(tables.SeedTable),
                evenPhaseLookupHex = Convert.ToHexString(tables.EvenPhaseLookup),
                oddPhaseLookupHex = Convert.ToHexString(tables.OddPhaseLookup)
            }, JsonOptions)
            : $"Seed: {Convert.ToHexString(tables.SeedTable)}{Environment.NewLine}" +
              $"Even lookup SHA-256: {Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tables.EvenPhaseLookup)).ToLowerInvariant()}{Environment.NewLine}" +
              $"Odd lookup SHA-256:  {Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(tables.OddPhaseLookup)).ToLowerInvariant()}";
        WriteOrPrint(text, options.Get("output"), options.Has("force"));
        return 0;
    }

    private static int RunCmpExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "palette", "output", "force", "json");
        string file = RequirePositional(options, 0, "export-cmp requires FILE.CMP.");
        string output = options.Get("output") ?? Path.Combine("RevengeTools.Output", "cmp",
            Path.GetFileNameWithoutExtension(file) + ".png");
        CmpExportResult result = CmpExporter.Export(Locate(options), file, options.Get("palette"),
            output, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(result, JsonOptions) :
            $"{result.SourceFile} -> {result.OutputFile} ({result.CompressionName}, palette {result.PaletteFile})");
        return 0;
    }

    private static int RunIcnInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string file = RequirePositional(options, 0, "inspect-icn requires FILE.ICN.");
        IcnImage image = IcnImage.Decode(ReadInstallationFile(options, file), file);
        IcnCatalogProfile catalog = IcnCatalog.Classify(file);
        object report = new
        {
            file,
            catalog,
            image.StoredLength,
            image.CompressionType,
            image.CompressionName,
            tileCount = IcnImage.TileCount,
            tileWidth = IcnImage.TileWidth,
            tileHeight = IcnImage.TileHeight,
            image.ConsumedPayloadBytes,
            image.PayloadLength,
            image.RemainingPayloadBytes,
            image.TrailingBytes
        };
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(report, JsonOptions) :
            $"{file}: {catalog.Kind} catalog, {IcnImage.TileCount} {IcnImage.TileWidth}x{IcnImage.TileHeight} tiles, " +
            $"{image.CompressionName}, {image.ConsumedPayloadBytes}/{image.PayloadLength} payload bytes consumed, " +
            $"{image.TrailingBytes} physical trailing bytes.");
        return 0;
    }

    private static int RunIcnExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "palette", "output", "columns", "force", "json");
        string file = RequirePositional(options, 0, "export-icn requires FILE.ICN.");
        int columns = options.Get("columns") is string columnText
            ? options.ParseNumber(columnText) : IcnExporter.DefaultColumns;
        string output = options.Get("output") ?? Path.Combine("RevengeTools.Output", "icn",
            Path.GetFileNameWithoutExtension(file) + ".png");
        IcnExportResult result = IcnExporter.Export(Locate(options), file, options.Get("palette"),
            output, columns, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(result, JsonOptions) :
            $"{result.SourceFile} -> {result.OutputFile} ({result.TileCount} tiles, palette {result.PaletteFile})");
        return 0;
    }

    private static int RunIcnsExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output-dir", "columns", "force", "json");
        int columns = options.Get("columns") is string columnText
            ? options.ParseNumber(columnText) : IcnExporter.DefaultColumns;
        string outputDirectory = options.Get("output-dir") ?? Path.Combine("RevengeTools.Output", "icn");
        IReadOnlyList<IcnExportResult> results = IcnExporter.ExportAll(Locate(options),
            outputDirectory, columns, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(results, JsonOptions) :
            $"Exported {results.Count} ICN tile sheets to {Path.GetFullPath(outputDirectory)}");
        return 0;
    }

    private static int RunMapInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string file = RequirePositional(options, 0, "inspect-map requires FILE.MAP.");
        RevengeMap map = RevengeMap.Parse(ReadInstallationFile(options, file), file);
        byte[] tileIds = map.TileIds;
        object report = new
        {
            file,
            map.Width,
            map.Height,
            duplicatedTilePrefixHex = Convert.ToHexString(map.DuplicatedTilePrefix),
            tileCount = tileIds.Length,
            distinctTileIds = tileIds.Distinct().Count(),
            minimumTileId = tileIds.Min(),
            maximumTileId = tileIds.Max()
        };
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(report, JsonOptions) :
            $"{file}: {map.Width}x{map.Height}, duplicated tile prefix {Convert.ToHexString(map.DuplicatedTilePrefix)}, " +
            $"{tileIds.Distinct().Count()} tile IDs ({tileIds.Min()}..{tileIds.Max()}).");
        return 0;
    }

    private static int RunMapExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "icons", "palette", "output", "force", "json");
        string file = RequirePositional(options, 0, "export-map requires FILE.MAP.");
        string iconSet = options.Get("icons") ?? MapResourceCatalog.Resolve(file).IconSetFile;
        string output = options.Get("output") ?? Path.Combine("RevengeTools.Output", "maps",
            Path.GetFileNameWithoutExtension(file) + "-" + Path.GetFileNameWithoutExtension(iconSet) + ".png");
        MapExportResult result = MapExporter.Export(Locate(options), file, iconSet,
            options.Get("palette"), output, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(result, JsonOptions) :
            $"{result.SourceFile} + {result.IconSetFile} -> {result.OutputFile} ({result.PixelWidth}x{result.PixelHeight})");
        return 0;
    }

    private static int RunSceneInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string file = RequirePositional(options, 0, "inspect-scene requires FILE.DAT.");
        SceneFile scene = SceneFile.Parse(ReadInstallationFile(options, file), file);
        SceneSummary summary = SceneReports.Summarize(file, scene);
        if (options.Has("json"))
        {
            Console.WriteLine(JsonSerializer.Serialize(new { Summary = summary, scene.Messages }, JsonOptions));
            return 0;
        }
        Console.WriteLine($"{file}: {summary.InstructionLength}-byte instruction stream, " +
            $"{summary.NonEmptyMessageCount}/{summary.MessageCount} messages, " +
            $"{summary.MapFile} + {summary.IconSetFile}");
        foreach (SceneMessage message in scene.Messages.Where(message => message.Text.Length != 0))
            Console.WriteLine($"  [{message.Index:D2}] 0x{message.Offset:X4} {message.Text}");
        return 0;
    }

    private static int RunScenesExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output", "force", "json");
        string output = options.Get("output") ?? Path.Combine("RevengeTools.Output", "scenes", "manifest.json");
        IReadOnlyList<SceneSummary> summaries = SceneReports.ExportAll(Locate(options), output, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(summaries, JsonOptions) :
            $"Exported {summaries.Count} SCENE summaries to {Path.GetFullPath(output)}");
        return 0;
    }

    private static int RunSceneMapExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "palette", "output", "force", "json");
        string file = RequirePositional(options, 0, "export-scene-map requires SCENEx.DAT.");
        string output = options.Get("output") ?? Path.Combine("RevengeTools.Output", "maps",
            Path.GetFileNameWithoutExtension(file) + ".png");
        MapExportResult result = SceneReports.ExportSceneMap(Locate(options), file,
            options.Get("palette"), output, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(result, JsonOptions) :
            $"{file}: {result.SourceFile} + {result.IconSetFile} -> {result.OutputFile}");
        return 0;
    }

    private static int RunSceneEvidenceExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output-dir", "force", "json");
        string outputDirectory = options.Get("output-dir") ??
            Path.Combine("RevengeTools.Output", "scenes", "evidence");
        SceneEvidenceExportResult result = SceneReports.ExportEvidence(Locate(options),
            outputDirectory, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(result, JsonOptions) :
            $"Exported raw evidence for {result.SceneCount} SCENE files to {result.OutputDirectory}");
        return 0;
    }

    private static int RunUnitTypes(CommandLine options)
    {
        options.RequireOnly("game-dir", "json", "csv", "output");
        if (options.Has("json") && options.Has("csv")) throw new ArgumentException("Choose either --json or --csv.");
        IReadOnlyList<UnitRecord> units = LoadTemplates(options);
        string text = options.Has("json") ? JsonSerializer.Serialize(units, JsonOptions) :
            options.Has("csv") ? FormatUnitsCsv(units) : FormatUnits(units);
        WriteOrPrint(text, options.Get("output"), false);
        return 0;
    }

    private static int RunUnitType(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string idText = RequirePositional(options, 0, "dump-unit-type requires ID.");
        int id = options.ParseNumber(idText);
        IReadOnlyList<UnitRecord> units = LoadTemplates(options);
        if (id < 0 || id >= units.Count) throw new ArgumentOutOfRangeException(nameof(id), "Unit ID must be 0..0x58.");
        UnitRecord unit = units[id];
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(unit, JsonOptions) : FormatUnit(unit));
        return 0;
    }

    private static int RunWeaponEvidenceExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output-dir", "force", "json");
        string outputDirectory = options.Get("output-dir") ??
            Path.Combine("RevengeTools.Output", "weapons");
        WeaponEvidenceExportResult result = WeaponReports.ExportEvidence(Locate(options),
            outputDirectory, options.Has("force"));
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(result, JsonOptions) :
            $"Exported {result.WeaponCount} weapon definitions, {result.AmmoFamilyCount} ammunition families, " +
            $"and {result.VehicleTemplateCount} vehicle records to {result.OutputDirectory}");
        return 0;
    }

    private static int RunMusicStreamInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        int index = options.ParseNumber(RequirePositional(options, 0,
            "inspect-speaker-effect requires INDEX."));
        MusicStreamCatalog catalog = LoadMusicStreams(options);
        if (index < 0 || index >= catalog.Streams.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Speaker-effect index must be 0..0x0F.");
        MusicStream stream = catalog.Streams[index];
        Console.WriteLine(options.Has("json")
            ? JsonSerializer.Serialize(stream, JsonOptions)
            : MusicStreamReports.Format(stream));
        return 0;
    }

    private static int RunHitLocationInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        GameInstallation installation = Locate(options);
        string executable = installation.ResolveFile("REVENGE.EXE");
        HitLocationCatalog catalog = HitLocationCatalog.Parse(
            File.ReadAllBytes(executable), "REVENGE.EXE");
        if (options.Has("json"))
        {
            Console.WriteLine(JsonSerializer.Serialize(catalog, JsonOptions));
            return 0;
        }

        Console.WriteLine("Directional selectors: " +
            string.Join(' ', catalog.DirectionSelectors.Select(value => value.ToString("X2"))));
        foreach (HitLocationTable table in catalog.Tables)
        {
            Console.WriteLine($"Table {table.Index:X2}: " + string.Join(", ",
                table.ResultsBy2D6.Select((location, index) =>
                    $"{index + 2}={location:X2} {HitLocationCatalog.BattleMechLocationName(location)}")));
        }
        return 0;
    }

    private static int RunMusicEvidenceExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output-dir", "force", "json");
        string outputDirectory = options.Get("output-dir") ??
            Path.Combine("RevengeTools.Output", "speaker-effects");
        MusicStreamCatalog catalog = LoadMusicStreams(options);
        MusicStreamReports.Export(catalog, outputDirectory, options.Has("force"));
        var report = new
        {
            outputDirectory = Path.GetFullPath(outputDirectory),
            streamCount = catalog.Streams.Count,
            instructionCount = catalog.Streams.Sum(stream => stream.Instructions.Count)
        };
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(report, JsonOptions) :
            $"Exported {report.streamCount} PC-speaker weapon-effect streams and {report.instructionCount} instructions to {report.outputDirectory}");
        return 0;
    }

    private static MusicStreamCatalog LoadMusicStreams(CommandLine options)
    {
        GameInstallation installation = Locate(options);
        string executable = installation.ResolveFile("REVENGE.EXE");
        return MusicStreamCatalog.Parse(File.ReadAllBytes(executable), "REVENGE.EXE");
    }

    private static int RunDigitalSoundInspection(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        GameInstallation installation = Locate(options);
        DigitalSoundCatalog catalog = DigitalSoundCatalog.Parse(
            File.ReadAllBytes(installation.ResolveFile("REVENGE.EXE")), "REVENGE.EXE");
        Console.WriteLine(options.Has("json")
            ? JsonSerializer.Serialize(catalog, JsonOptions)
            : DigitalSoundReports.Format(catalog));
        return 0;
    }

    private static int RunDigitalSoundExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output-dir", "force", "json");
        GameInstallation installation = Locate(options);
        DigitalSoundCatalog catalog = DigitalSoundCatalog.Parse(
            File.ReadAllBytes(installation.ResolveFile("REVENGE.EXE")), "REVENGE.EXE");
        string outputDirectory = options.Get("output-dir") ??
            Path.Combine("RevengeTools.Output", "digital-sounds");
        DigitalSoundReports.Export(catalog, installation, outputDirectory, options.Has("force"));
        var report = new
        {
            outputDirectory = Path.GetFullPath(outputDirectory),
            sampleCount = catalog.Entries.Count,
            totalBytes = catalog.Entries.Sum(entry => entry.SampleLength),
            playbackRateHz = DigitalSoundCatalog.PlaybackRateHz,
            encoding = "unsigned 8-bit PCM, mono"
        };
        Console.WriteLine(options.Has("json") ? JsonSerializer.Serialize(report, JsonOptions) :
            $"Exported {report.sampleCount} digital samples as raw bytes and 8-bit PCM WAV " +
            $"({report.totalBytes} source bytes) to {report.outputDirectory}");
        return 0;
    }

    private static int RunSave(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string file = RequirePositional(options, 0, "dump-save requires FILE.");
        RevengeSaveFile save = RevengeSaveFile.Parse(ReadInstallationFile(options, file), file);
        if (options.Has("json")) Console.WriteLine(JsonSerializer.Serialize(save, JsonOptions));
        else
        {
            Console.WriteLine($"{file}: {RevengeSaveFile.SlotCount} slots, 0x{RevengeSaveFile.PayloadLength:X} bytes each");
            foreach (RevengeSaveSlot slot in save.Slots)
                Console.WriteLine($"  {slot.SlotNumber}: occupied={slot.AppearsOccupied,-5} label='{slot.Label}' stage=0x{slot.CampaignStage:X4} variant=0x{slot.ScenarioVariant:X4} units={slot.Units.Count(unit => unit.IsPopulated)}");
        }
        return 0;
    }

    private static int RunSaveSlot(CommandLine options)
    {
        options.RequireOnly("game-dir", "json");
        string file = RequirePositional(options, 0, "dump-save-slot requires FILE SLOT.");
        int slotNumber = options.ParseNumber(RequirePositional(options, 1, "dump-save-slot requires FILE SLOT."));
        RevengeSaveSlot slot = RevengeSaveFile.Parse(ReadInstallationFile(options, file), file).GetSlot(slotNumber);
        if (options.Has("json")) Console.WriteLine(JsonSerializer.Serialize(slot, JsonOptions));
        else
        {
            Console.WriteLine($"Slot {slot.SlotNumber}: '{slot.Label}', stage=0x{slot.CampaignStage:X4}, variant=0x{slot.ScenarioVariant:X4}, phase=0x{slot.CampaignPhase:X2}, flags=0x{slot.CampaignFlags:X4}");
            foreach (UnitRecord unit in slot.Units.Where(unit => unit.IsPopulated))
                Console.WriteLine($"  {unit.RecordIndex,2}: 0x{unit.UnitTypeId:X2} {unit.UnitName,-14} {unit.Tonnage,3}t pilot=0x{unit.PilotId:X2} exp={unit.PilotExperience} allegiance=0x{unit.Allegiance:X2}");
        }
        return 0;
    }

    private static int RunSaveComparison(CommandLine options)
    {
        options.RequireOnly("json", "output", "force");
        string fileA = RequirePositional(options, 0, "compare-save requires FILE_A SLOT_A FILE_B SLOT_B.");
        int slotA = options.ParseNumber(RequirePositional(options, 1, "compare-save requires FILE_A SLOT_A FILE_B SLOT_B."));
        string fileB = RequirePositional(options, 2, "compare-save requires FILE_A SLOT_A FILE_B SLOT_B.");
        int slotB = options.ParseNumber(RequirePositional(options, 3, "compare-save requires FILE_A SLOT_A FILE_B SLOT_B."));
        if (options.Positionals.Count != 4)
            throw new ArgumentException("compare-save requires exactly FILE_A SLOT_A FILE_B SLOT_B.");

        string fullA = Path.GetFullPath(fileA);
        string fullB = Path.GetFullPath(fileB);
        SaveComparisonReport report = SaveComparer.Compare(File.ReadAllBytes(fullA), slotA, fullA,
            File.ReadAllBytes(fullB), slotB, fullB);
        bool json = options.Has("json");
        string extension = json ? ".json" : ".txt";
        string defaultName = $"{SafeFileStem(fileA)}-slot-{slotA}-vs-{SafeFileStem(fileB)}-slot-{slotB}{extension}";
        string output = options.Get("output") ?? Path.Combine("RevengeTools.Output", "save-diffs", defaultName);
        string text = json ? JsonSerializer.Serialize(report, JsonOptions) : SaveComparisonFormatter.Format(report);
        OutputFile.WriteText(output, text + Environment.NewLine, options.Has("force"));
        Console.WriteLine($"Wrote {Path.GetFullPath(output)} ({report.ChangedByteCount} changed bytes across {report.Regions.Count} regions).");
        return 0;
    }

    private static int RunSaveSetAnalysis(CommandLine options)
    {
        options.RequireOnly("json", "output", "force");
        string directory = RequirePositional(options, 0, "analyze-save-set requires DIRECTORY.");
        if (options.Positionals.Count != 1)
            throw new ArgumentException("analyze-save-set requires exactly one DIRECTORY.");
        SaveSetAnalysisReport report = SaveSetAnalyzer.Analyze(directory);
        bool json = options.Has("json");
        string output = options.Get("output") ?? Path.Combine("RevengeTools.Output", "save-diffs",
            json ? "checkpoint-analysis.json" : "checkpoint-analysis.txt");
        string text = json ? JsonSerializer.Serialize(report, JsonOptions) : SaveSetAnalysisFormatter.Format(report);
        OutputFile.WriteText(output, text + Environment.NewLine, options.Has("force"));
        Console.WriteLine($"Wrote {Path.GetFullPath(output)} ({report.CheckpointCount} occupied checkpoints from {report.FileCount} files).");
        return 0;
    }

    private static int RunSaveStateExport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output", "force");
        string file = RequirePositional(options, 0, "export-save-state requires FILE SLOT.");
        int slot = options.ParseNumber(RequirePositional(options, 1, "export-save-state requires FILE SLOT."));
        string output = options.Get("output") ?? $"{Path.GetFileName(file)}-slot-{slot}-state.txt";
        SaveState state = SaveStateFileService.Export(Locate(options), file, slot, output, options.Has("force"));
        Console.WriteLine($"Exported slot {state.SlotNumber} to {Path.GetFullPath(output)}");
        return 0;
    }

    private static int RunSaveStateImport(CommandLine options)
    {
        options.RequireOnly("game-dir", "output", "dry-run", "force");
        string file = RequirePositional(options, 0, "import-save-state requires FILE SLOT STATE.txt.");
        int slot = options.ParseNumber(RequirePositional(options, 1, "import-save-state requires FILE SLOT STATE.txt."));
        string state = RequirePositional(options, 2, "import-save-state requires FILE SLOT STATE.txt.");
        string output = options.Get("output") ?? Path.GetFileName(file) + ".edited";
        bool dryRun = options.Has("dry-run");
        SaveStateUpdateResult result = SaveStateFileService.Import(Locate(options), file, slot, state, output,
            dryRun, options.Has("force"));
        foreach (SaveStateChange change in result.Changes)
            Console.WriteLine($"0x{change.FileOffset:X4} {change.Field}: 0x{change.OldValue:X2} -> 0x{change.NewValue:X2}");
        Console.WriteLine(dryRun ? $"Dry run: {result.Changes.Count} changed bytes; no output written." :
            $"Wrote {Path.GetFullPath(output)} with {result.Changes.Count} changed bytes.");
        return 0;
    }

    private static GameInstallation Locate(CommandLine options) => GameInstallationLocator.Locate(options.Get("game-dir"));

    private static byte[] ReadInstallationFile(CommandLine options, string file) => File.ReadAllBytes(Locate(options).ResolveFile(file));

    private static IReadOnlyList<UnitRecord> LoadTemplates(CommandLine options) =>
        UnitCatalog.ParseTemplates(ReadInstallationFile(options, "MECHTYPE.DAT"));

    private static string RequirePositional(CommandLine options, int index, string message) =>
        index < options.Positionals.Count ? options.Positionals[index] : throw new ArgumentException(message);

    private static string SafeFileStem(string path)
    {
        string stem = Path.GetFileName(path).Replace('.', '-');
        foreach (char invalid in Path.GetInvalidFileNameChars()) stem = stem.Replace(invalid, '_');
        return stem;
    }

    private static void WriteOrPrint(string text, string? output, bool overwrite)
    {
        if (output is null) Console.WriteLine(text);
        else OutputFile.WriteText(output, text + (text.EndsWith('\n') ? string.Empty : Environment.NewLine), overwrite);
    }

    private static string FormatInventory(InstallationInventoryReport report)
    {
        var output = new StringBuilder();
        output.AppendLine($"Installation: {report.InstallationPath}");
        output.AppendLine($"Located by: {report.LocatedBy}");
        foreach (InstallationInventoryEntry file in report.Files)
            output.AppendLine($"{file.Name,-16} {file.Category,-12} {(file.Present ? file.Length?.ToString(CultureInfo.InvariantCulture) : "missing"),8}  {file.Validation}");
        output.Append($"Result: {(report.IsValid ? "valid" : "invalid")}");
        return output.ToString();
    }

    private static string FormatHex(byte[] data, int offset, int count)
    {
        var output = new StringBuilder();
        for (int row = 0; row < count; row += 16)
        {
            int length = Math.Min(16, count - row);
            output.Append((offset + row).ToString("X8")).Append("  ");
            for (int column = 0; column < 16; column++)
                output.Append(column < length ? data[offset + row + column].ToString("X2") + ' ' : "   ");
            output.Append(' ');
            for (int column = 0; column < length; column++)
            {
                byte value = data[offset + row + column];
                output.Append(value is >= 0x20 and <= 0x7E ? (char)value : '.');
            }
            output.AppendLine();
        }
        return output.ToString();
    }

    private static string FormatUnits(IReadOnlyList<UnitRecord> units) => string.Join(Environment.NewLine,
        units.Select(unit => $"0x{unit.UnitTypeId:X2} {unit.UnitName,-14} {unit.Tonnage,3}t walk={unit.WalkMovement} jump={unit.JumpMovement} heat-sinks={unit.EngineHeatSinkCapacity} sprite=0x{unit.TacticalSpriteBase:X2} damage-model=0x{unit.DamageModel:X2} ({unit.DamageModelKind})"));

    private static string FormatUnitsCsv(IReadOnlyList<UnitRecord> units)
    {
        var output = new StringBuilder("id,name,tonnage,walk,jump,engine_heat_sink_capacity,tactical_sprite_base,damage_model,damage_model_name\n");
        foreach (UnitRecord unit in units)
            output.AppendLine($"{unit.UnitTypeId},\"{unit.UnitName.Replace("\"", "\"\"")}\",{unit.Tonnage},{unit.WalkMovement},{unit.JumpMovement},{unit.EngineHeatSinkCapacity},{unit.TacticalSpriteBase},{unit.DamageModel},\"{unit.DamageModelKind}\"");
        return output.ToString();
    }

    private static string FormatUnit(UnitRecord unit) =>
        $"0x{unit.UnitTypeId:X2} {unit.UnitName}\nTonnage: {unit.Tonnage}\nMovement: walk {unit.WalkMovement}, jump {unit.JumpMovement}\n" +
        $"Current internal: {string.Join(',', unit.CurrentInternal)}\nMaximum internal: {string.Join(',', unit.MaximumInternal)}\n" +
        $"Current armor: {string.Join(',', unit.CurrentArmor)}\nMaximum armor: {string.Join(',', unit.MaximumArmor)}\n" +
        $"Current ammo: {string.Join(',', unit.CurrentAmmo)}\nHeat: {unit.CurrentHeat} + {unit.HeatFraction}/256 (raw 0x{unit.HeatAccumulatorQ8_8:X4})\n" +
        $"Tactical sprite base: 0x{unit.TacticalSpriteBase:X2}\nAccuracy upgrade: {unit.HasAccuracyUpgrade}\nDamage model: 0x{unit.DamageModel:X2} ({unit.DamageModelKind})\n" +
        $"Pilot: 0x{unit.PilotId:X2}; experience: {unit.PilotExperience}; allegiance: 0x{unit.Allegiance:X2}";

    private static void PrintHelp()
    {
        Console.WriteLine("RevengeTools - BattleTech: The Crescent Hawk's Revenge inspection and preservation CLI");
        Console.WriteLine();
        Console.WriteLine("  inventory [--game-dir PATH] [--hash] [--json] [--output FILE]");
        Console.WriteLine("  inspect FILE [--offset N] [--count N] [--game-dir PATH]");
        Console.WriteLine("  inspect-palette FILE.COL [--json] [--game-dir PATH]");
        Console.WriteLine("  export-palette FILE.COL [--output FILE.png] [--force]");
        Console.WriteLine("  inspect-font FILE.FNT [--json] [--game-dir PATH]");
        Console.WriteLine("  export-font FILE.FNT [--scale N] [--output FILE.png] [--force]");
        Console.WriteLine("  inspect-image FILE.CPS [--json] [--game-dir PATH]");
        Console.WriteLine("  export-image FILE.CPS [--palette FILE.COL] [--output FILE.png] [--force]");
        Console.WriteLine("  export-images [--output-dir DIR] [--force] [--json]");
        Console.WriteLine("  inspect-cmp FILE.CMP [--json] [--game-dir PATH]");
        Console.WriteLine("  export-cmp FILE.CMP [--palette FILE.COL] [--output FILE.png] [--force]");
        Console.WriteLine("  inspect-icn FILE.ICN [--json] [--game-dir PATH]");
        Console.WriteLine("  export-icn FILE.ICN [--palette FILE.COL] [--columns N] [--output FILE.png] [--force]");
        Console.WriteLine("  inspect-unit-sprites [--json] [--output FILE] [--force]");
        Console.WriteLine("  inspect-cga-translation [--json] [--output FILE] [--force]");
        Console.WriteLine("  export-icns [--columns N] [--output-dir DIR] [--force] [--json]");
        Console.WriteLine("  inspect-map FILE.MAP [--json] [--game-dir PATH]");
        Console.WriteLine("  export-map FILE.MAP [--icons FILE.ICN] [--palette FILE.COL] [--output FILE.png] [--force]");
        Console.WriteLine("  inspect-scene SCENEx.DAT [--json] [--game-dir PATH]");
        Console.WriteLine("  export-scenes [--output FILE.json] [--force] [--json]");
        Console.WriteLine("  export-scene-evidence [--output-dir DIR] [--force] [--json]");
        Console.WriteLine("  export-scene-map SCENEx.DAT [--palette FILE.COL] [--output FILE.png] [--force]");
        Console.WriteLine("  dump-unit-types [--json|--csv] [--output FILE]");
        Console.WriteLine("  dump-unit-type ID [--json]");
        Console.WriteLine("  inspect-hit-locations [--json]");
        Console.WriteLine("  export-weapon-evidence [--output-dir DIR] [--force] [--json]");
        Console.WriteLine("  inspect-speaker-effect INDEX [--json]");
        Console.WriteLine("  export-speaker-effect-evidence [--output-dir DIR] [--force] [--json]");
        Console.WriteLine("  inspect-digital-sounds [--json]");
        Console.WriteLine("  export-digital-sounds [--output-dir DIR] [--force] [--json]");
        Console.WriteLine("  dump-save FILE [--json]");
        Console.WriteLine("  dump-save-slot FILE SLOT [--json]");
        Console.WriteLine("  compare-save FILE_A SLOT_A FILE_B SLOT_B [--json] [--output FILE] [--force]");
        Console.WriteLine("  analyze-save-set DIRECTORY [--json] [--output FILE] [--force]");
        Console.WriteLine("  export-save-state FILE SLOT [--output FILE.txt] [--force]");
        Console.WriteLine("  import-save-state FILE SLOT STATE.txt [--output FILE] [--dry-run] [--force]");
        Console.WriteLine();
        Console.WriteLine("Game lookup: --game-dir, BTCHR_GAME_DIR, or an ignored Chrevenge directory.");
    }
}
