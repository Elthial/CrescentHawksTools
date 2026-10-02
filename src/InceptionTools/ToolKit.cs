using InceptionTools.Graphics;
using System;
using System.IO;
using InceptionTools.Installation;
using InceptionTools.Inspection;
using InceptionTools.Animation;
using InceptionTools.SaveEditing;
using InceptionTools.Audio;
using InceptionTools.Maps;

namespace InceptionTools
{
    class ToolKit
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0].Equals("inventory", StringComparison.OrdinalIgnoreCase))
                {
                    RunInventory(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("inspect", StringComparison.OrdinalIgnoreCase))
                {
                    RunInspect(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("dump-save", StringComparison.OrdinalIgnoreCase))
                {
                    RunSaveDump(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("dump-weapons", StringComparison.OrdinalIgnoreCase))
                {
                    RunWeaponDump(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("dump-character", StringComparison.OrdinalIgnoreCase))
                {
                    RunCharacterDump(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("dump-mech", StringComparison.OrdinalIgnoreCase))
                {
                    RunMechDump(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("disassemble-bld", StringComparison.OrdinalIgnoreCase))
                {
                    RunBldDisassembly(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("inspect-animation", StringComparison.OrdinalIgnoreCase))
                {
                    RunAnimationInspection(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-animation-frame", StringComparison.OrdinalIgnoreCase))
                {
                    RunAnimationFrameExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-animation-frames", StringComparison.OrdinalIgnoreCase))
                {
                    RunAnimationSequenceExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-animation-gif", StringComparison.OrdinalIgnoreCase))
                {
                    RunAnimationGifExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-animation-gifs", StringComparison.OrdinalIgnoreCase))
                {
                    RunAllAnimationGifsExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-mech-spritesheet", StringComparison.OrdinalIgnoreCase))
                {
                    RunMechSpriteSheetExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("inspect-image", StringComparison.OrdinalIgnoreCase))
                {
                    RunCompressedGraphicsInspection(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-image", StringComparison.OrdinalIgnoreCase))
                {
                    RunCompressedGraphicsExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-images", StringComparison.OrdinalIgnoreCase))
                {
                    RunAllCompressedGraphicsExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("inspect-map", StringComparison.OrdinalIgnoreCase))
                {
                    RunMapInspection(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-map", StringComparison.OrdinalIgnoreCase))
                {
                    RunMapExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-maps", StringComparison.OrdinalIgnoreCase))
                {
                    RunAllMapsExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-save-state", StringComparison.OrdinalIgnoreCase))
                {
                    RunSaveStateExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("import-save-state", StringComparison.OrdinalIgnoreCase))
                {
                    RunSaveStateImport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("inspect-sif", StringComparison.OrdinalIgnoreCase))
                {
                    RunSifInspection(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-sif-wav", StringComparison.OrdinalIgnoreCase))
                {
                    RunSifWaveExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("play-sif", StringComparison.OrdinalIgnoreCase))
                {
                    RunSifPlayback(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("list-sound-effects", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Write(HasOption(args, "--json") ? SoundEffectWriter.WriteJson() : SoundEffectWriter.WriteText());
                    return;
                }

                if (args.Length > 0 && args[0].Equals("export-sound-effect-wav", StringComparison.OrdinalIgnoreCase))
                {
                    RunSoundEffectWaveExport(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("play-sound-effect", StringComparison.OrdinalIgnoreCase))
                {
                    RunSoundEffectPlayback(args);
                    return;
                }

                if (args.Length > 0 && args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    PrintHelp();
                    return;
                }

                PrintHelp();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                Environment.ExitCode = 1;
            }
        }

        private static void RunInventory(string[] args)
        {
            string gameDirectory = ReadOption(args, "--game-dir");
            string outputPath = ReadOption(args, "--output");
            bool includeHashes = HasOption(args, "--hash");
            bool json = HasOption(args, "--json");

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

        private static void RunInspect(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("inspect requires an installation filename.");

            string gameDirectory = ReadOption(args, "--game-dir");
            long offset = ParseNumber(ReadOption(args, "--offset"), 0);
            int count = checked((int)ParseNumber(ReadOption(args, "--count"), 0x80));
            bool decodeBld = HasOption(args, "--decode-bld");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            FileInspectionResult result = FileInspector.Inspect(installation, args[1], offset, count, decodeBld);
            Console.Write(FileInspector.WriteText(result));
        }

        private static void RunSaveDump(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("dump-save requires a save filename such as GAME1.");

            string gameDirectory = ReadOption(args, "--game-dir");
            bool json = HasOption(args, "--json");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            SaveGameDump dump = SaveGameInspector.Inspect(installation, args[1]);
            Console.Write(json ? SaveGameInspector.WriteJson(dump) : SaveGameInspector.WriteText(dump));
        }

        private static void RunWeaponDump(string[] args)
        {
            bool json = HasOption(args, "--json");
            WeaponTableDump dump = WeaponTableInspector.InspectCapturedTable();
            Console.Write(json ? WeaponTableInspector.WriteJson(dump) : WeaponTableInspector.WriteText(dump));
        }

        private static void RunCharacterDump(string[] args)
        {
            RequireSaveRecordSelection(args, "dump-character");
            string gameDirectory = ReadOption(args, "--game-dir");
            string group = ReadOption(args, "--group") ?? "player";
            int slot = checked((int)ParseNumber(args[2], 0));
            bool json = HasOption(args, "--json");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            CharacterRecordDump character = SaveGameInspector.InspectCharacter(installation, args[1], group, slot);
            Console.Write(json ? SaveGameInspector.WriteCharacterJson(character) : SaveGameInspector.WriteCharacterText(character));
        }

        private static void RunMechDump(string[] args)
        {
            RequireSaveRecordSelection(args, "dump-mech");
            string gameDirectory = ReadOption(args, "--game-dir");
            string group = ReadOption(args, "--group") ?? "player";
            int slot = checked((int)ParseNumber(args[2], 0));
            bool json = HasOption(args, "--json");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            MechRecordDump mech = SaveGameInspector.InspectMech(installation, args[1], group, slot);
            Console.Write(json ? SaveGameInspector.WriteMechJson(mech) : SaveGameInspector.WriteMechText(mech));
        }

        private static void RequireSaveRecordSelection(string[] args, string command)
        {
            if (args.Length < 3 || args[1].StartsWith("--", StringComparison.Ordinal) ||
                args[2].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException(command + " requires a save filename and numeric slot.");
        }

        private static void RunBldDisassembly(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("disassemble-bld requires a BLD filename.");

            string gameDirectory = ReadOption(args, "--game-dir");
            int offset = checked((int)ParseNumber(ReadOption(args, "--offset"), 0));
            bool json = HasOption(args, "--json");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            BldDisassembly result = BldDisassembler.Inspect(installation, args[1], offset);
            Console.Write(json ? BldDisassembler.WriteJson(result) : BldDisassembler.WriteText(result));
        }

        private static void RunAnimationInspection(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("inspect-animation requires an ANM filename.");

            string gameDirectory = ReadOption(args, "--game-dir");
            bool json = HasOption(args, "--json");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            AnimationInspection inspection = AnimationInspector.Inspect(installation, args[1]);
            Console.Write(json ? AnimationInspector.WriteJson(inspection) : AnimationInspector.WriteText(inspection));
        }

        private static void RunAnimationFrameExport(string[] args)
        {
            if (args.Length < 3 || args[1].StartsWith("--", StringComparison.Ordinal) ||
                args[2].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("export-animation-frame requires an ANM filename and numeric frame index.");

            string gameDirectory = ReadOption(args, "--game-dir");
            int frameIndex = checked((int)ParseNumber(args[2], 0));
            string outputPath = ReadOption(args, "--output") ??
                Path.GetFileNameWithoutExtension(args[1]) + "-frame-" + frameIndex.ToString("D2") + ".png";
            bool overwrite = HasOption(args, "--force");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            AnimationFrameExportResult result = AnimationFrameExporter.ExportPng(installation, args[1],
                frameIndex, outputPath, overwrite);
            Console.WriteLine("Exported " + result.SourceFileName + " frame " + result.FrameIndex + "/" +
                (result.FrameCount - 1) + " to " + result.OutputPath + " (" + result.OutputLength + " bytes).");
        }

        private static void RunAnimationSequenceExport(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("export-animation-frames requires an ANM filename.");

            string gameDirectory = ReadOption(args, "--game-dir");
            string outputDirectory = ReadOption(args, "--output-dir") ??
                Path.GetFileNameWithoutExtension(args[1]) + "-frames";
            bool overwrite = HasOption(args, "--force");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            AnimationSequenceExportResult result = AnimationFrameExporter.ExportPngSequence(installation,
                args[1], outputDirectory, overwrite);
            Console.WriteLine("Exported " + result.Frames.Count + " frames from " + result.SourceFileName +
                " to " + result.OutputDirectory + ".");
        }

        private static void RunAnimationGifExport(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("export-animation-gif requires an ANM filename.");

            string gameDirectory = ReadOption(args, "--game-dir");
            string outputPath = ReadOption(args, "--output") ??
                Path.GetFileNameWithoutExtension(args[1]) + ".gif";
            bool overwrite = HasOption(args, "--force");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            AnimationGifExportResult result = AnimationFrameExporter.ExportGif(
                installation, args[1], outputPath, overwrite);
            Console.WriteLine("Exported " + result.FrameCount + " frames from " + result.SourceFileName +
                " to " + result.OutputPath + " (" + result.OutputLength + " bytes, " +
                result.TotalDelayRetraces + " retraces -> " + result.TotalDelayCentiseconds +
                " centiseconds at nominal " + result.NominalRefreshRateHz + " Hz, looping)." );
        }

        private static void RunAllAnimationGifsExport(string[] args)
        {
            string gameDirectory = ReadOption(args, "--game-dir");
            string outputDirectory = ReadOption(args, "--output-dir") ?? "animation-gifs";
            bool overwrite = HasOption(args, "--force");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            AnimationGifBatchExportResult result = AnimationFrameExporter.ExportAllGifs(
                installation, outputDirectory, overwrite);
            Console.WriteLine("Exported " + result.Animations.Count + " looping GIFs containing " +
                result.TotalFrameCount + " frames to " + result.OutputDirectory + " (" +
                result.TotalOutputLength + " bytes total)." );
        }

        private static void RunMechSpriteSheetExport(string[] args)
        {
            string gameDirectory = ReadOption(args, "--game-dir");
            string outputPath = ReadOption(args, "--output") ?? "MECHSHAP-spritesheet.png";
            string metadataPath = ReadOption(args, "--metadata") ??
                Path.ChangeExtension(outputPath, ".json");
            bool overwrite = HasOption(args, "--force");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            MechSpriteSheetExportResult result = MechSpriteSheetExporter.Export(
                installation, outputPath, metadataPath, overwrite);
            Console.WriteLine("Exported " + result.SourceSpriteCount + " source sprites in " +
                result.SequenceCount + " sequence rows to " + result.OutputPath + " (" +
                result.Width + "x" + result.Height + "). Metadata: " + result.MetadataPath + ".");
        }

        private static void RunCompressedGraphicsInspection(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("inspect-image requires a CMP or ICN filename.");
            GameInstallation installation = GameInstallationLocator.Locate(ReadOption(args, "--game-dir"));
            CompressedGraphicsInspection inspection = CompressedGraphicsExporter.Inspect(installation, args[1]);
            Console.Write(HasOption(args, "--json")
                ? CompressedGraphicsExporter.WriteJson(inspection)
                : CompressedGraphicsExporter.WriteText(inspection));
        }

        private static void RunCompressedGraphicsExport(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("export-image requires a CMP or ICN filename.");
            string outputPath = ReadOption(args, "--output") ??
                Path.GetFileNameWithoutExtension(args[1]) + ".png";
            GameInstallation installation = GameInstallationLocator.Locate(ReadOption(args, "--game-dir"));
            CompressedGraphicsExportResult result = CompressedGraphicsExporter.ExportPng(
                installation, args[1], outputPath, HasOption(args, "--force"));
            Console.WriteLine("Exported " + result.Inspection.FileName + " to " + result.OutputPath + " (" +
                result.Inspection.Width + "x" + result.Inspection.Height + ", format " +
                result.Inspection.CompressionFormat + ", " + result.Inspection.PaletteName + ").");
        }

        private static void RunAllCompressedGraphicsExport(string[] args)
        {
            string outputDirectory = ReadOption(args, "--output-dir") ?? "images";
            GameInstallation installation = GameInstallationLocator.Locate(ReadOption(args, "--game-dir"));
            CompressedGraphicsBatchExportResult result = CompressedGraphicsExporter.ExportAllPng(
                installation, outputDirectory, HasOption(args, "--force"));
            Console.WriteLine("Exported " + result.Images.Count + " CMP/ICN images to " +
                result.OutputDirectory + " (" + result.TotalOutputLength + " bytes total).");
        }

        private static void RunMapInspection(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("inspect-map requires a MAP1.MTP through MAP15.MTP filename.");
            GameInstallation installation = GameInstallationLocator.Locate(ReadOption(args, "--game-dir"));
            MtpMapInspection inspection = MtpMapExporter.Inspect(installation, args[1]);
            Console.Write(HasOption(args, "--json")
                ? MtpMapExporter.WriteJson(inspection)
                : MtpMapExporter.WriteText(inspection));
        }

        private static void RunMapExport(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("export-map requires a MAP1.MTP through MAP15.MTP filename.");
            string outputPath = ReadOption(args, "--output") ??
                Path.GetFileNameWithoutExtension(args[1]) + ".png";
            string metadataPath = ReadOption(args, "--metadata") ?? Path.ChangeExtension(outputPath, ".json");
            GameInstallation installation = GameInstallationLocator.Locate(ReadOption(args, "--game-dir"));
            MtpMapExportResult result = MtpMapExporter.Export(installation, args[1], outputPath,
                metadataPath, HasOption(args, "--force"));
            Console.WriteLine("Exported " + result.Inspection.FileName + " using " +
                result.Inspection.TileSetFileName + " to " + result.OutputPath + " (" +
                result.Inspection.Width * 16 + "x" + result.Inspection.Height * 16 +
                "). Metadata: " + result.MetadataPath + ".");
        }

        private static void RunAllMapsExport(string[] args)
        {
            string outputDirectory = ReadOption(args, "--output-dir") ?? "maps";
            GameInstallation installation = GameInstallationLocator.Locate(ReadOption(args, "--game-dir"));
            MtpMapBatchExportResult result = MtpMapExporter.ExportAll(installation,
                outputDirectory, HasOption(args, "--force"));
            Console.WriteLine("Exported " + result.Maps.Count + " MTP maps to " +
                result.OutputDirectory + " (" + result.TotalOutputLength + " PNG bytes total).");
        }

        private static void RunSaveStateExport(string[] args)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("export-save-state requires a save filename such as GAME1.");

            string gameDirectory = ReadOption(args, "--game-dir");
            string outputPath = ReadOption(args, "--output") ?? args[1] + "-state.txt";
            bool overwrite = HasOption(args, "--force");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            SaveStateTextExportResult result = SaveStateFileService.ExportText(
                installation, args[1], outputPath, overwrite);
            Console.WriteLine("Exported editable state for " + result.SourceFileName + " to " +
                result.OutputPath + ".");
        }

        private static void RunSaveStateImport(string[] args)
        {
            if (args.Length < 3 || args[1].StartsWith("--", StringComparison.Ordinal) ||
                args[2].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("import-save-state requires a save filename and state .txt file.");

            string gameDirectory = ReadOption(args, "--game-dir");
            string outputPath = ReadOption(args, "--output") ?? args[1] + ".edited";
            bool overwrite = HasOption(args, "--force");
            GameInstallation installation = GameInstallationLocator.Locate(gameDirectory);
            SaveStateFileUpdateResult result = SaveStateFileService.ImportText(
                installation, args[1], args[2], outputPath, overwrite);
            Console.WriteLine("Imported " + result.StateTextPath + " over " + result.SourceFileName +
                " and wrote " + result.OutputPath + " (" + result.Update.Changes.Count + " changed bytes).");
            foreach (SaveStateChange change in result.Update.Changes)
                Console.WriteLine("  0x" + change.FileOffset.ToString("X4") + " " + change.Field +
                    ": 0x" + change.OldValue.ToString("X2") + " -> 0x" + change.NewValue.ToString("X2"));
        }

        private static void RunSifInspection(string[] args)
        {
            string fileName = ReadOptionalFileArgument(args, SifFileRecord.DefaultFileName);
            GameInstallation installation = GameInstallationLocator.Locate(ReadOption(args, "--game-dir"));
            SifInspection inspection = SifInspector.Inspect(installation, fileName);
            Console.Write(HasOption(args, "--json") ? SifInspector.WriteJson(inspection) : SifInspector.WriteText(inspection));
        }

        private static void RunSifWaveExport(string[] args)
        {
            string fileName = ReadOptionalFileArgument(args, SifFileRecord.DefaultFileName);
            SifPlaybackMode mode = ParseSifMode(ReadOption(args, "--mode"));
            string outputPath = ReadOption(args, "--output") ??
                Path.GetFileNameWithoutExtension(fileName) + "-" + FormatSifMode(mode) + ".wav";
            GameInstallation installation = GameInstallationLocator.Locate(ReadOption(args, "--game-dir"));
            AudioExportResult result = AudioFileService.ExportSifWave(installation, fileName, mode,
                outputPath, HasOption(args, "--force"));
            Console.WriteLine("Exported " + FormatSifMode(mode) + " rendering to " + result.OutputPath +
                " (" + result.DurationSeconds.ToString("F3") + " seconds, " + result.OutputLength + " bytes)." );
        }

        private static void RunSifPlayback(string[] args)
        {
            string fileName = ReadOptionalFileArgument(args, SifFileRecord.DefaultFileName);
            SifPlaybackMode mode = ParseSifMode(ReadOption(args, "--mode"));
            GameInstallation installation = GameInstallationLocator.Locate(ReadOption(args, "--game-dir"));
            byte[] bytes = File.ReadAllBytes(installation.ResolveFile(fileName));
            Console.WriteLine("Playing " + fileName + " using " + FormatSifMode(mode) + " interpretation...");
            WaveAudioPlayer.Play(SifRenderer.RenderWave(SifFileRecord.Parse(bytes), mode));
        }

        private static void RunSoundEffectWaveExport(string[] args)
        {
            SoundEffectDefinition effect = RequireSoundEffect(args, "export-sound-effect-wav");
            string outputPath = ReadOption(args, "--output") ??
                "sound-" + effect.Id.ToString("D2") + "-" + effect.Name + ".wav";
            byte[] wave = SoundEffectRenderer.RenderWave(effect);
            AudioExportResult result = AudioFileService.WriteWave(wave, outputPath, HasOption(args, "--force"));
            Console.WriteLine("Exported sound 0x" + effect.Id.ToString("X2") + " " + effect.Name + " to " +
                result.OutputPath + " (" + result.DurationSeconds.ToString("F3") + " seconds)." );
        }

        private static void RunSoundEffectPlayback(string[] args)
        {
            SoundEffectDefinition effect = RequireSoundEffect(args, "play-sound-effect");
            Console.WriteLine("Playing sound 0x" + effect.Id.ToString("X2") + " " + effect.Name + "...");
            WaveAudioPlayer.Play(SoundEffectRenderer.RenderWave(effect));
        }

        private static SoundEffectDefinition RequireSoundEffect(string[] args, string command)
        {
            if (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException(command + " requires a numeric sound ID or catalog name.");
            return SoundEffectCatalog.Get(args[1]);
        }

        private static string ReadOptionalFileArgument(string[] args, string defaultValue) =>
            args.Length >= 2 && !args[1].StartsWith("--", StringComparison.Ordinal) ? args[1] : defaultValue;

        private static SifPlaybackMode ParseSifMode(string value)
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

        private static string ReadOption(string[] args, string name)
        {
            for (int index = 1; index < args.Length; index++)
            {
                if (!args[index].Equals(name, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (index + 1 >= args.Length)
                    throw new ArgumentException(name + " requires a value.");
                return args[index + 1];
            }
            return null;
        }

        private static bool HasOption(string[] args, string name)
        {
            for (int index = 1; index < args.Length; index++)
                if (args[index].Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static long ParseNumber(string value, long defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return Convert.ToInt64(value.Substring(2), 16);
            return long.Parse(value);
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
}
