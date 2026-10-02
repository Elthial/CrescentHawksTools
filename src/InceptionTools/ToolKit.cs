using System;
using System.IO;
using CrescentHawksTools.Cli;
using InceptionTools.Animation;
using InceptionTools.Audio;
using InceptionTools.Graphics;
using InceptionTools.Inspection;
using InceptionTools.Installation;
using InceptionTools.Maps;
using InceptionTools.SaveEditing;

namespace InceptionTools;

public static class Program
{
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
            CommandLine options = CommandLine.Parse(args.Skip(1),
                "game-dir", "output", "output-dir", "metadata", "offset", "count", "group", "mode");
            ValidateCommand(command, options);

            Action<CommandLine> handler = command switch
            {
                "inventory" => RunInventory,
                "inspect" => RunInspect,
                "dump-save" => RunSaveDump,
                "dump-weapons" => RunWeaponDump,
                "dump-character" => RunCharacterDump,
                "dump-mech" => RunMechDump,
                "disassemble-bld" => RunBldDisassembly,
                "inspect-animation" => RunAnimationInspection,
                "export-animation-frame" => RunAnimationFrameExport,
                "export-animation-frames" => RunAnimationSequenceExport,
                "export-animation-gif" => RunAnimationGifExport,
                "export-animation-gifs" => RunAllAnimationGifsExport,
                "export-mech-spritesheet" => RunMechSpriteSheetExport,
                "inspect-image" => RunCompressedGraphicsInspection,
                "export-image" => RunCompressedGraphicsExport,
                "export-images" => RunAllCompressedGraphicsExport,
                "inspect-map" => RunMapInspection,
                "export-map" => RunMapExport,
                "export-maps" => RunAllMapsExport,
                "export-save-state" => RunSaveStateExport,
                "import-save-state" => RunSaveStateImport,
                "inspect-sif" => RunSifInspection,
                "export-sif-wav" => RunSifWaveExport,
                "play-sif" => RunSifPlayback,
                "list-sound-effects" => RunSoundEffectList,
                "export-sound-effect-wav" => RunSoundEffectWaveExport,
                "play-sound-effect" => RunSoundEffectPlayback,
                _ => throw new ArgumentException($"Unknown command '{args[0]}'. Run 'help' for available commands.")
            };
            handler(options);
            return Environment.ExitCode;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or InvalidDataException or
                                          UnauthorizedAccessException or OverflowException or PlatformNotSupportedException)
        {
            Console.Error.WriteLine("Error: " + exception.Message);
            return 1;
        }
    }

    private static void ValidateCommand(string command, CommandLine options)
    {
        (int minimum, int maximum, string usage, string[] allowed) = command switch
        {
            "inventory" => (0, 0, "inventory [options]", Options("game-dir", "hash", "json", "output")),
            "inspect" => (1, 1, "inspect FILE [options]", Options("game-dir", "offset", "count", "decode-bld")),
            "dump-save" => (1, 1, "dump-save FILE [options]", Options("game-dir", "json")),
            "dump-weapons" => (0, 0, "dump-weapons [--json]", Options("json")),
            "dump-character" => (2, 2, "dump-character FILE SLOT [options]", Options("group", "game-dir", "json")),
            "dump-mech" => (2, 2, "dump-mech FILE SLOT [options]", Options("group", "game-dir", "json")),
            "disassemble-bld" => (1, 1, "disassemble-bld FILE [options]", Options("game-dir", "offset", "json")),
            "inspect-animation" => (1, 1, "inspect-animation FILE [options]", Options("game-dir", "json")),
            "export-animation-frame" => (2, 2, "export-animation-frame FILE FRAME [options]", Options("game-dir", "output", "force")),
            "export-animation-frames" => (1, 1, "export-animation-frames FILE [options]", Options("game-dir", "output-dir", "force")),
            "export-animation-gif" => (1, 1, "export-animation-gif FILE [options]", Options("game-dir", "output", "force")),
            "export-animation-gifs" => (0, 0, "export-animation-gifs [options]", Options("game-dir", "output-dir", "force")),
            "export-mech-spritesheet" => (0, 0, "export-mech-spritesheet [options]", Options("game-dir", "output", "metadata", "force")),
            "inspect-image" => (1, 1, "inspect-image FILE [options]", Options("game-dir", "json")),
            "export-image" => (1, 1, "export-image FILE [options]", Options("game-dir", "output", "force")),
            "export-images" => (0, 0, "export-images [options]", Options("game-dir", "output-dir", "force")),
            "inspect-map" => (1, 1, "inspect-map FILE [options]", Options("game-dir", "json")),
            "export-map" => (1, 1, "export-map FILE [options]", Options("game-dir", "output", "metadata", "force")),
            "export-maps" => (0, 0, "export-maps [options]", Options("game-dir", "output-dir", "force")),
            "export-save-state" => (1, 1, "export-save-state FILE [options]", Options("game-dir", "output", "force")),
            "import-save-state" => (2, 2, "import-save-state FILE STATE.txt [options]", Options("game-dir", "output", "force")),
            "inspect-sif" => (0, 1, "inspect-sif [FILE] [options]", Options("game-dir", "json")),
            "export-sif-wav" => (0, 1, "export-sif-wav [FILE] [options]", Options("game-dir", "mode", "output", "force")),
            "play-sif" => (0, 1, "play-sif [FILE] [options]", Options("game-dir", "mode")),
            "list-sound-effects" => (0, 0, "list-sound-effects [--json]", Options("json")),
            "export-sound-effect-wav" => (1, 1, "export-sound-effect-wav ID|NAME [options]", Options("output", "force")),
            "play-sound-effect" => (1, 1, "play-sound-effect ID|NAME", Options()),
            _ => throw new ArgumentException($"Unknown command '{command}'. Run 'help' for available commands.")
        };
        options.RequireOnly(allowed);
        options.RequirePositionalCount(minimum, maximum, usage);
    }

    private static string[] Options(params string[] names) => names;

    private static void RunInventory(CommandLine options)
    {
        string? gameDirectory = options.Get("game-dir");
        string? outputPath = options.Get("output");
        bool includeHashes = options.Has("hash");
        bool json = options.Has("json");

        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        InstallationInventoryReport report = InstallationInventory.Scan(installation, includeHashes);
        string content = json
            ? InventoryReportWriter.WriteJson(report)
            : InventoryReportWriter.WriteText(report);
        InventoryReportWriter.SaveIfRequested(content, outputPath);

        if (report.Files.Exists(file => file.Required && !file.Present) ||
            report.Files.Exists(file => file.Present &&
                (file.Validation.StartsWith("invalid", StringComparison.Ordinal) ||
                 file.Validation.StartsWith("length-mismatch", StringComparison.Ordinal))))
            Environment.ExitCode = 2;
    }

    private static void RunInspect(CommandLine options)
    {

        string? gameDirectory = options.Get("game-dir");
        long offset = ParseNumber(options.Get("offset"), 0);
        int count = checked((int)ParseNumber(options.Get("count"), 0x80));
        bool decodeBld = options.Has("decode-bld");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        FileInspectionResult result = FileInspector.Inspect(installation, options.Positionals[0], offset, count, decodeBld);
        Console.Write(FileInspector.WriteText(result));
    }

    private static void RunSaveDump(CommandLine options)
    {

        string? gameDirectory = options.Get("game-dir");
        bool json = options.Has("json");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        SaveGameDump dump = SaveGameInspector.Inspect(installation, options.Positionals[0]);
        Console.Write(json ? SaveGameInspector.WriteJson(dump) : SaveGameInspector.WriteText(dump));
    }

    private static void RunWeaponDump(CommandLine options)
    {
        bool json = options.Has("json");
        WeaponTableDump dump = WeaponTableInspector.InspectReferenceTable();
        Console.Write(json ? WeaponTableInspector.WriteJson(dump) : WeaponTableInspector.WriteText(dump));
    }

    private static void RunCharacterDump(CommandLine options)
    {
        string? gameDirectory = options.Get("game-dir");
        string group = options.Get("group") ?? "player";
        int slot = checked((int)ParseNumber(options.Positionals[1], 0));
        bool json = options.Has("json");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        CharacterRecordDump character = SaveGameInspector.InspectCharacter(installation, options.Positionals[0], group, slot);
        Console.Write(json ? SaveGameInspector.WriteCharacterJson(character) : SaveGameInspector.WriteCharacterText(character));
    }

    private static void RunMechDump(CommandLine options)
    {
        string? gameDirectory = options.Get("game-dir");
        string group = options.Get("group") ?? "player";
        int slot = checked((int)ParseNumber(options.Positionals[1], 0));
        bool json = options.Has("json");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        MechRecordDump mech = SaveGameInspector.InspectMech(installation, options.Positionals[0], group, slot);
        Console.Write(json ? SaveGameInspector.WriteMechJson(mech) : SaveGameInspector.WriteMechText(mech));
    }

    private static void RunBldDisassembly(CommandLine options)
    {

        string? gameDirectory = options.Get("game-dir");
        int offset = checked((int)ParseNumber(options.Get("offset"), 0));
        bool json = options.Has("json");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        BldDisassembly result = BldDisassembler.Inspect(installation, options.Positionals[0], offset);
        Console.Write(json ? BldDisassembler.WriteJson(result) : BldDisassembler.WriteText(result));
    }

    private static void RunAnimationInspection(CommandLine options)
    {

        string? gameDirectory = options.Get("game-dir");
        bool json = options.Has("json");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        AnimationInspection inspection = AnimationInspector.Inspect(installation, options.Positionals[0]);
        Console.Write(json ? AnimationInspector.WriteJson(inspection) : AnimationInspector.WriteText(inspection));
    }

    private static void RunAnimationFrameExport(CommandLine options)
    {

        string? gameDirectory = options.Get("game-dir");
        int frameIndex = checked((int)ParseNumber(options.Positionals[1], 0));
        string outputPath = options.Get("output") ??
            Path.GetFileNameWithoutExtension(options.Positionals[0]) + "-frame-" + frameIndex.ToString("D2") + ".png";
        bool overwrite = options.Has("force");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        AnimationFrameExportResult result = AnimationFrameExporter.ExportPng(installation, options.Positionals[0],
            frameIndex, outputPath, overwrite);
        Console.WriteLine("Exported " + result.SourceFileName + " frame " + result.FrameIndex + "/" +
            (result.FrameCount - 1) + " to " + result.OutputPath + " (" + result.OutputLength + " bytes).");
    }

    private static void RunAnimationSequenceExport(CommandLine options)
    {

        string? gameDirectory = options.Get("game-dir");
        string outputDirectory = options.Get("output-dir") ??
            Path.GetFileNameWithoutExtension(options.Positionals[0]) + "-frames";
        bool overwrite = options.Has("force");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        AnimationSequenceExportResult result = AnimationFrameExporter.ExportPngSequence(installation,
            options.Positionals[0], outputDirectory, overwrite);
        Console.WriteLine("Exported " + result.Frames.Count + " frames from " + result.SourceFileName +
            " to " + result.OutputDirectory + ".");
    }

    private static void RunAnimationGifExport(CommandLine options)
    {

        string? gameDirectory = options.Get("game-dir");
        string outputPath = options.Get("output") ??
            Path.GetFileNameWithoutExtension(options.Positionals[0]) + ".gif";
        bool overwrite = options.Has("force");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        AnimationGifExportResult result = AnimationFrameExporter.ExportGif(
            installation, options.Positionals[0], outputPath, overwrite);
        Console.WriteLine("Exported " + result.FrameCount + " frames from " + result.SourceFileName +
            " to " + result.OutputPath + " (" + result.OutputLength + " bytes, " +
            result.TotalDelayRetraces + " retraces -> " + result.TotalDelayCentiseconds +
            " centiseconds at nominal " + result.NominalRefreshRateHz + " Hz, looping).");
    }

    private static void RunAllAnimationGifsExport(CommandLine options)
    {
        string? gameDirectory = options.Get("game-dir");
        string outputDirectory = options.Get("output-dir") ?? "animation-gifs";
        bool overwrite = options.Has("force");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        AnimationGifBatchExportResult result = AnimationFrameExporter.ExportAllGifs(
            installation, outputDirectory, overwrite);
        Console.WriteLine("Exported " + result.Animations.Count + " looping GIFs containing " +
            result.TotalFrameCount + " frames to " + result.OutputDirectory + " (" +
            result.TotalOutputLength + " bytes total).");
    }

    private static void RunMechSpriteSheetExport(CommandLine options)
    {
        string? gameDirectory = options.Get("game-dir");
        string outputPath = options.Get("output") ?? "MECHSHAP-spritesheet.png";
        string metadataPath = options.Get("metadata") ??
            Path.ChangeExtension(outputPath, ".json");
        bool overwrite = options.Has("force");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        MechSpriteSheetExportResult result = MechSpriteSheetExporter.Export(
            installation, outputPath, metadataPath, overwrite);
        Console.WriteLine("Exported " + result.SourceSpriteCount + " source sprites in " +
            result.SequenceCount + " sequence rows to " + result.OutputPath + " (" +
            result.Width + "x" + result.Height + "). Metadata: " + result.MetadataPath + ".");
    }

    private static void RunCompressedGraphicsInspection(CommandLine options)
    {
        GameInstallation installation = GameInstallationLocator.Locate(options.Get("game-dir"));
        CompressedGraphicsInspection inspection = CompressedGraphicsExporter.Inspect(installation, options.Positionals[0]);
        Console.Write(options.Has("json")
            ? CompressedGraphicsExporter.WriteJson(inspection)
            : CompressedGraphicsExporter.WriteText(inspection));
    }

    private static void RunCompressedGraphicsExport(CommandLine options)
    {
        string outputPath = options.Get("output") ??
            Path.GetFileNameWithoutExtension(options.Positionals[0]) + ".png";
        GameInstallation installation = GameInstallationLocator.Locate(options.Get("game-dir"));
        CompressedGraphicsExportResult result = CompressedGraphicsExporter.ExportPng(
            installation, options.Positionals[0], outputPath, options.Has("force"));
        Console.WriteLine("Exported " + result.Inspection.FileName + " to " + result.OutputPath + " (" +
            result.Inspection.Width + "x" + result.Inspection.Height + ", format " +
            result.Inspection.CompressionFormat + ", " + result.Inspection.PaletteName + ").");
    }

    private static void RunAllCompressedGraphicsExport(CommandLine options)
    {
        string outputDirectory = options.Get("output-dir") ?? "images";
        GameInstallation installation = GameInstallationLocator.Locate(options.Get("game-dir"));
        CompressedGraphicsBatchExportResult result = CompressedGraphicsExporter.ExportAllPng(
            installation, outputDirectory, options.Has("force"));
        Console.WriteLine("Exported " + result.Images.Count + " CMP/ICN images to " +
            result.OutputDirectory + " (" + result.TotalOutputLength + " bytes total).");
    }

    private static void RunMapInspection(CommandLine options)
    {
        GameInstallation installation = GameInstallationLocator.Locate(options.Get("game-dir"));
        MtpMapInspection inspection = MtpMapExporter.Inspect(installation, options.Positionals[0]);
        Console.Write(options.Has("json")
            ? MtpMapExporter.WriteJson(inspection)
            : MtpMapExporter.WriteText(inspection));
    }

    private static void RunMapExport(CommandLine options)
    {
        string outputPath = options.Get("output") ??
            Path.GetFileNameWithoutExtension(options.Positionals[0]) + ".png";
        string metadataPath = options.Get("metadata") ?? Path.ChangeExtension(outputPath, ".json");
        GameInstallation installation = GameInstallationLocator.Locate(options.Get("game-dir"));
        MtpMapExportResult result = MtpMapExporter.Export(installation, options.Positionals[0], outputPath,
            metadataPath, options.Has("force"));
        Console.WriteLine("Exported " + result.Inspection.FileName + " using " +
            result.Inspection.TileSetFileName + " to " + result.OutputPath + " (" +
            result.Inspection.Width * 16 + "x" + result.Inspection.Height * 16 +
            "). Metadata: " + result.MetadataPath + ".");
    }

    private static void RunAllMapsExport(CommandLine options)
    {
        string outputDirectory = options.Get("output-dir") ?? "maps";
        GameInstallation installation = GameInstallationLocator.Locate(options.Get("game-dir"));
        MtpMapBatchExportResult result = MtpMapExporter.ExportAll(installation,
            outputDirectory, options.Has("force"));
        Console.WriteLine("Exported " + result.Maps.Count + " MTP maps to " +
            result.OutputDirectory + " (" + result.TotalOutputLength + " PNG bytes total).");
    }

    private static void RunSaveStateExport(CommandLine options)
    {

        string? gameDirectory = options.Get("game-dir");
        string outputPath = options.Get("output") ?? options.Positionals[0] + "-state.txt";
        bool overwrite = options.Has("force");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        SaveStateTextExportResult result = SaveStateFileService.ExportText(
            installation, options.Positionals[0], outputPath, overwrite);
        Console.WriteLine("Exported editable state for " + result.SourceFileName + " to " +
            result.OutputPath + ".");
    }

    private static void RunSaveStateImport(CommandLine options)
    {

        string? gameDirectory = options.Get("game-dir");
        string outputPath = options.Get("output") ?? options.Positionals[0] + ".edited";
        bool overwrite = options.Has("force");
        GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
        SaveStateFileUpdateResult result = SaveStateFileService.ImportText(
            installation, options.Positionals[0], options.Positionals[1], outputPath, overwrite);
        Console.WriteLine("Imported " + result.StateTextPath + " over " + result.SourceFileName +
            " and wrote " + result.OutputPath + " (" + result.Update.Changes.Count + " changed bytes).");
        foreach (SaveStateChange change in result.Update.Changes)
            Console.WriteLine("  0x" + change.FileOffset.ToString("X4") + " " + change.Field +
                ": 0x" + change.OldValue.ToString("X2") + " -> 0x" + change.NewValue.ToString("X2"));
    }

    private static void RunSifInspection(CommandLine options)
    {
        string fileName = ReadOptionalFileArgument(options, SifFileRecord.DefaultFileName);
        GameInstallation installation = GameInstallationLocator.Locate(options.Get("game-dir"));
        SifInspection inspection = SifInspector.Inspect(installation, fileName);
        Console.Write(options.Has("json") ? SifInspector.WriteJson(inspection) : SifInspector.WriteText(inspection));
    }

    private static void RunSifWaveExport(CommandLine options)
    {
        string fileName = ReadOptionalFileArgument(options, SifFileRecord.DefaultFileName);
        SifPlaybackMode mode = ParseSifMode(options.Get("mode"));
        string outputPath = options.Get("output") ??
            Path.GetFileNameWithoutExtension(fileName) + "-" + FormatSifMode(mode) + ".wav";
        GameInstallation installation = GameInstallationLocator.Locate(options.Get("game-dir"));
        AudioExportResult result = AudioFileService.ExportSifWave(installation, fileName, mode,
            outputPath, options.Has("force"));
        Console.WriteLine("Exported " + FormatSifMode(mode) + " rendering to " + result.OutputPath +
            " (" + result.DurationSeconds.ToString("F3") + " seconds, " + result.OutputLength + " bytes).");
    }

    private static void RunSifPlayback(CommandLine options)
    {
        string fileName = ReadOptionalFileArgument(options, SifFileRecord.DefaultFileName);
        SifPlaybackMode mode = ParseSifMode(options.Get("mode"));
        GameInstallation installation = GameInstallationLocator.Locate(options.Get("game-dir"));
        byte[] bytes = File.ReadAllBytes(installation.ResolveFile(fileName));
        Console.WriteLine("Playing " + fileName + " using " + FormatSifMode(mode) + " interpretation...");
        WaveAudioPlayer.Play(SifRenderer.RenderWave(SifFileRecord.Parse(bytes), mode));
    }

    private static void RunSoundEffectWaveExport(CommandLine options)
    {
        SoundEffectDefinition effect = RequireSoundEffect(options, "export-sound-effect-wav");
        string outputPath = options.Get("output") ??
            "sound-" + effect.Id.ToString("D2") + "-" + effect.Name + ".wav";
        byte[] wave = SoundEffectRenderer.RenderWave(effect);
        AudioExportResult result = AudioFileService.WriteWave(wave, outputPath, options.Has("force"));
        Console.WriteLine("Exported sound 0x" + effect.Id.ToString("X2") + " " + effect.Name + " to " +
            result.OutputPath + " (" + result.DurationSeconds.ToString("F3") + " seconds).");
    }

    private static void RunSoundEffectPlayback(CommandLine options)
    {
        SoundEffectDefinition effect = RequireSoundEffect(options, "play-sound-effect");
        Console.WriteLine("Playing sound 0x" + effect.Id.ToString("X2") + " " + effect.Name + "...");
        WaveAudioPlayer.Play(SoundEffectRenderer.RenderWave(effect));
    }

    private static void RunSoundEffectList(CommandLine options) =>
        Console.Write(options.Has("json") ? SoundEffectWriter.WriteJson() : SoundEffectWriter.WriteText());

    private static SoundEffectDefinition RequireSoundEffect(CommandLine options, string command) =>
        SoundEffectCatalog.Get(options.RequirePositional(0,
            command + " requires a numeric sound ID or catalog name."));

    private static string ReadOptionalFileArgument(CommandLine options, string defaultValue) =>
        options.Positionals.Count == 0 ? defaultValue : options.Positionals[0];

    private static SifPlaybackMode ParseSifMode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("pc-speaker", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("pc", StringComparison.OrdinalIgnoreCase))
            return SifPlaybackMode.PcSpeaker;
        if (value.Equals("tandy", StringComparison.OrdinalIgnoreCase))
            return SifPlaybackMode.Tandy;
        throw new ArgumentException("--mode must be pc-speaker or tandy.");
    }

    private static string FormatSifMode(SifPlaybackMode mode) =>
        mode == SifPlaybackMode.Tandy ? "tandy" : "pc-speaker";

    private static long ParseNumber(string? value, long defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return Convert.ToInt64(value[2..], 16);
        return long.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("InceptionTools commands:");
        Console.WriteLine("  inventory [--game-dir PATH] [--hash] [--json] [--output FILE]");
        Console.WriteLine("  inspect FILE [--game-dir PATH] [--offset N] [--count N] [--decode-bld]");
        Console.WriteLine("  dump-save FILE [--game-dir PATH] [--json]");
        Console.WriteLine("  dump-character FILE SLOT [--group player|enemy] [--game-dir PATH] [--json]");
        Console.WriteLine("  dump-mech FILE SLOT [--group player|enemy] [--game-dir PATH] [--json]");
        Console.WriteLine("  dump-weapons [--json]");
        Console.WriteLine("  disassemble-bld FILE [--game-dir PATH] [--offset N] [--json]");
        Console.WriteLine("  inspect-animation FILE [--game-dir PATH] [--json]");
        Console.WriteLine("  export-animation-frame FILE FRAME [--game-dir PATH] [--output FILE.png] [--force]");
        Console.WriteLine("  export-animation-frames FILE [--game-dir PATH] [--output-dir DIR] [--force]");
        Console.WriteLine("  export-animation-gif FILE [--game-dir PATH] [--output FILE.gif] [--force]");
        Console.WriteLine("  export-animation-gifs [--game-dir PATH] [--output-dir DIR] [--force]");
        Console.WriteLine("  export-mech-spritesheet [--game-dir PATH] [--output FILE.png] [--metadata FILE.json] [--force]");
        Console.WriteLine("  inspect-image FILE [--game-dir PATH] [--json]");
        Console.WriteLine("  export-image FILE [--game-dir PATH] [--output FILE.png] [--force]");
        Console.WriteLine("  export-images [--game-dir PATH] [--output-dir DIR] [--force]");
        Console.WriteLine("  inspect-map FILE [--game-dir PATH] [--json]");
        Console.WriteLine("  export-map FILE [--game-dir PATH] [--output FILE.png] [--metadata FILE.json] [--force]");
        Console.WriteLine("  export-maps [--game-dir PATH] [--output-dir DIR] [--force]");
        Console.WriteLine("  export-save-state FILE [--game-dir PATH] [--output FILE.txt] [--force]");
        Console.WriteLine("  import-save-state FILE STATE.txt [--game-dir PATH] [--output FILE] [--force]");
        Console.WriteLine("  inspect-sif [FILE] [--game-dir PATH] [--json]");
        Console.WriteLine("  export-sif-wav [FILE] [--mode pc-speaker|tandy] [--game-dir PATH] [--output FILE.wav] [--force]");
        Console.WriteLine("  play-sif [FILE] [--mode pc-speaker|tandy] [--game-dir PATH]");
        Console.WriteLine("  list-sound-effects [--json]");
        Console.WriteLine("  export-sound-effect-wav ID|NAME [--output FILE.wav] [--force]");
        Console.WriteLine("  play-sound-effect ID|NAME");
        Console.WriteLine();
        Console.WriteLine("Installation lookup order: --game-dir, BTCHI_GAME_DIR, then local Chinception.");
        Console.WriteLine("Running without a command prints this help.");
    }
}
