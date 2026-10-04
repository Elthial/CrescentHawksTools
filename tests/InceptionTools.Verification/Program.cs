using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using CrescentHawksTools.Cli;
using InceptionTools;
using InceptionTools.Animation;
using InceptionTools.Audio;
using InceptionTools.Binary;
using InceptionTools.Graphics;
using InceptionTools.Inspection;
using InceptionTools.Installation;
using InceptionTools.Maps;
using InceptionTools.Records;
using InceptionTools.SaveEditing;

internal static class Program
{
    private static int _assertions;

    private static int Main()
    {
        string fixture = Path.Combine(Path.GetTempPath(), "btchi-inventory-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixture);
        try
        {
            VerifyCommandLineContract();
            byte[] binaryFixture = new byte[] { 0x34, 0x12, 0x78, 0x56, 0x34, 0x12, (byte)'A', 0x00, 0x20 };
            var binaryReader = new BoundedBinaryReader(binaryFixture, "synthetic reader fixture");
            Assert(binaryReader.ReadByte(0) == 0x34, "bounded byte read");
            Assert(binaryReader.ReadUInt16LittleEndian(0) == 0x1234, "bounded little-endian word");
            Assert(binaryReader.ReadUInt32LittleEndian(2) == 0x12345678, "bounded little-endian dword");
            Assert(binaryReader.ReadFixedAscii(6, 3) == "A", "bounded fixed ASCII read");
            byte[] copiedBytes = binaryReader.ReadBytes(0, 2);
            copiedBytes[0] = 0;
            Assert(binaryFixture[0] == 0x34, "bounded reads return copies");
            AssertThrows<InvalidDataException>(() => binaryReader.ReadUInt16LittleEndian(8, "truncated word"), "bounded range diagnostic");
            AssertThrows<InvalidDataException>(() => binaryReader.RequireExactLength(8, "test structure"), "exact structure length diagnostic");

            File.WriteAllBytes(Path.Combine(fixture, "BTECH.EXE"), new byte[] { (byte)'M', (byte)'Z', 0, 0 });
            File.WriteAllBytes(Path.Combine(fixture, "DEMOFILE"), new byte[0x03FF]);
            File.WriteAllBytes(Path.Combine(fixture, "TRAINING.BLD"), new byte[] { 3, 0, 0xEE, 0xC6, 0xEB });
            byte[] decodedScript = new byte[]
            {
                0xFE, 0x06, 0xFD, 0xFA, 0x00, 0xFC, (byte)'H', (byte)'i', 0x00,
                0xF7, 0x27, 0x0D, 0x00, 0xFF, 0xF3, 0x05, 0x04, 0x00, 0x08, 0x00
            };
            File.WriteAllBytes(Path.Combine(fixture, "SCRIPT.BLD"), EncodeBld(decodedScript));
            File.WriteAllBytes(Path.Combine(fixture, "BADTABLE.BLD"),
                EncodeBld(new byte[] { 0xF3, 0x00, 0xFF, 0xFF }));
            File.WriteAllBytes(Path.Combine(fixture, "SOUND.BLD"),
                EncodeBld(new byte[] { 0xE4, 0x0A, 0xFF }));
            File.WriteAllBytes(Path.Combine(fixture, "RECRUIT.BLD"),
                EncodeBld(new byte[] { 0xE9, 0x06, 0xFF }));
            File.WriteAllBytes(Path.Combine(fixture, "REX.BLD"),
                EncodeBld(new byte[] { 0xF5, 0x1E, 0xFF }));
            File.WriteAllBytes(Path.Combine(fixture, "ARENA.BLD"),
                EncodeBld(new byte[] { 0xF5, 0x23, 0xFF }));
            File.WriteAllBytes(Path.Combine(fixture, "JAIL.BLD"),
                EncodeBld(new byte[] { 0xF5, 0x28, 0xFF }));
            File.WriteAllBytes(Path.Combine(fixture, "BTBORDER.CMP"), new byte[] { 3, 0, 1, 0xAA, 0xBB });
            byte[] compressedImageFixture =
                { 8, 0, 2, 2, 0x12, 0x34, 0, 0xFE, 0x7C, 0x56 };
            File.WriteAllBytes(Path.Combine(fixture, "MECHSHAP.CMP"), compressedImageFixture);
            File.WriteAllBytes(Path.Combine(fixture, "ANIMATE.ICN"), compressedImageFixture);
            File.WriteAllBytes(Path.Combine(fixture, "BTTLTECH.ICN"), compressedImageFixture);
            File.WriteAllBytes(Path.Combine(fixture, "TINYLAND.CMP"), compressedImageFixture);
            byte[] mapFixture = new byte[MtpMapRecord.StandardHeaderLength + 64];
            mapFixture[3] = 8;
            mapFixture[4] = 8;
            Array.Copy(System.Text.Encoding.ASCII.GetBytes("JASON\0KATRINA\0"), 0,
                mapFixture, 5, 13);
            mapFixture[MtpMapRecord.StandardHeaderLength] = 0;
            File.WriteAllBytes(Path.Combine(fixture, "MAP1.MTP"), mapFixture);
            File.WriteAllBytes(Path.Combine(fixture, "EXTRA.DAT"), new byte[] { 1, 2, 3 });
            string oversizedPath = Path.Combine(fixture, "OVERSIZED.DAT");
            using (FileStream oversized = File.Create(oversizedPath))
                oversized.SetLength(GameInstallation.MaximumInputFileLength + 1);
            byte[] animation = new byte[0x80];
            animation[0] = 0x41;
            animation[1] = 0x42;
            animation[5] = 0x51;
            animation[AnmFileRecord.PlaybackControlLength] = 1;
            animation[AnmFileRecord.PlaybackControlLength + 1] = 4;
            animation[AnmFileRecord.HeaderLength - 1] = 6;
            byte[] syntheticFrameStream = BuildSyntheticAnimationFrameStream();
            Array.Copy(syntheticFrameStream, 0, animation, AnmFileRecord.HeaderLength, syntheticFrameStream.Length);
            Array.Copy(syntheticFrameStream, 0, animation, AnmFileRecord.HeaderLength + syntheticFrameStream.Length,
                syntheticFrameStream.Length);
            File.WriteAllBytes(Path.Combine(fixture, "O0.ANM"), animation);
            byte[] syntheticSif = { 0x28, 0x80, 0x2B, 0x4C, 0x80, 0x2F, 0x80, 0x41 };
            File.WriteAllBytes(Path.Combine(fixture, "WWOODBT.SIF"), syntheticSif);
            byte[] save = new byte[SaveGameInspector.SaveLength];
            save[0] = 0x0C;
            save[0x0001] = 2;
            save[0x0002] = 9;
            save[0x000D] = 8;
            byte[] mechName = System.Text.Encoding.ASCII.GetBytes("TEST MECH");
            Array.Copy(mechName, 0, save, 0x0111, mechName.Length);
            save[0x0121] = 20;
            save[0x0135] = 0xA5;
            save[0x0136] = 0x3C;
            save[0x017A] = 0xF7;
            save[0x017B] = 0xD2;
            save[0x018D] = 3;
            save[0x0011] = 3;
            WriteUInt32(save, 0x0D5D, 123456);
            WriteUInt16(save, 0x0F45, 0x1234);
            WriteUInt16(save, 0x0F47, 0xABCD);
            File.WriteAllBytes(Path.Combine(fixture, "GAME1"), save);

            GameInstallation located = GameInstallationLocator.Locate(fixture);
            AssertThrows<InvalidDataException>(() => located.ResolveFile("OVERSIZED.DAT"),
                "installation input-size limit rejects oversized files before allocation");
            File.Delete(oversizedPath);
            Assert(located.Source == "--game-dir", "explicit path provenance");
            Assert(located.DirectoryPath == Path.GetFullPath(fixture), "explicit path normalization");

            InstallationInventoryReport report = InstallationInventory.Scan(located, false);
            InstallationInventoryEntry executable = Find(report, "BTECH.EXE");
            InstallationInventoryEntry demo = Find(report, "DEMOFILE");
            InstallationInventoryEntry bld = Find(report, "TRAINING.BLD");
            InstallationInventoryEntry missing = Find(report, "CITADEL.BLD");
            InstallationInventoryEntry extra = Find(report, "EXTRA.DAT");
            InstallationInventoryEntry graphic = Find(report, "BTBORDER.CMP");

            Assert(executable.Validation == "ok:mz", "MZ validation");
            Assert(demo.Validation == "ok", "fixed demo length validation");
            Assert(bld.Validation == "ok:bld-payload-length", "BLD payload-length validation");
            Assert(!missing.Present && missing.Validation == "missing-required", "required-file reporting");
            Assert(extra.Category == "additional", "additional-file reporting");
            Assert(graphic.Validation == "ok:graphics-header", "compressed-graphics header validation");
            Assert(string.IsNullOrEmpty(executable.Sha256), "hashing is opt-in");

            CompressedImage columnImage = CompressedImageDecoder.Decode(compressedImageFixture, "synthetic format 2");
            Assert(columnImage.CompressionFormat == 2 && columnImage.ConsumedPayloadBytes == 7 &&
                columnImage.RemainingPayloadBytes == 0, "bounded format-2 graphics decode");
            Assert(columnImage.PackedBytes[0] == 0x12 && columnImage.PackedBytes[160] == 0x34 &&
                columnImage.PaletteIndices.Take(2).SequenceEqual(new byte[] { 1, 2 }) &&
                columnImage.PaletteIndices.Skip(320).Take(2).SequenceEqual(new byte[] { 3, 4 }),
                "format-2 column traversal and nibble expansion");
            byte[] rowImageFixture = (byte[])compressedImageFixture.Clone();
            rowImageFixture[2] = 1;
            CompressedImage rowImage = CompressedImageDecoder.Decode(rowImageFixture, "synthetic format 1");
            Assert(rowImage.PackedBytes.Take(2).SequenceEqual(new byte[] { 0x12, 0x34 }) &&
                rowImage.PaletteIndices.Take(4).SequenceEqual(new byte[] { 1, 2, 3, 4 }),
                "format-1 row traversal");
            byte[] badImageLength = (byte[])compressedImageFixture.Clone();
            badImageLength[0] = 7;
            AssertThrows<InvalidDataException>(() => CompressedImageDecoder.Decode(badImageLength),
                "compressed-image stored length rejected");
            byte[] transparentPng = EgaIndexedPngEncoder.Encode(2, 1, new byte[] { 0, 15 }, 0);
            Assert(FindBytes(transparentPng, System.Text.Encoding.ASCII.GetBytes("tRNS")) >= 0,
                "portable EGA PNG transparency");
            CompressedGraphicsInspection imageInspection = CompressedGraphicsExporter.Inspect(
                located, "mechshap.cmp");
            Assert(imageInspection.Width == 320 && imageInspection.Height == 200 &&
                imageInspection.Purpose == "sprite source sheet" && imageInspection.RemainingPayloadBytes == 0,
                "compressed graphics inspection profile");
            CompressedGraphicsInspection tileInspection = CompressedGraphicsExporter.Inspect(
                located, "animate.icn");
            Assert(tileInspection.Width == 16 && tileInspection.Height == 4000 &&
                tileInspection.Purpose == "16x16 tile strip",
                "ICN tile-strip compatibility profile");
            string imageExportPath = Path.Combine(fixture, "exports", "MECHSHAP-source.png");
            CompressedGraphicsExportResult imageExport = CompressedGraphicsExporter.ExportPng(
                located, "MECHSHAP.CMP", imageExportPath);
            byte[] exportedImagePng = File.ReadAllBytes(imageExportPath);
            Assert(File.Exists(imageExportPath) && imageExport.OutputLength > 0 &&
                ReadUInt32BigEndian(exportedImagePng, 16) == 320 &&
                ReadUInt32BigEndian(exportedImagePng, 20) == 200,
                "portable compressed graphics PNG export");
            AssertThrows<IOException>(() => CompressedGraphicsExporter.ExportPng(
                located, "MECHSHAP.CMP", imageExportPath), "compressed graphics export refuses overwrite");
            MtpMapRecord mapRecord = MtpMapRecord.ParseStandard(mapFixture, "synthetic MAP1.MTP");
            Assert(mapRecord.Width == 8 && mapRecord.Height == 8 && mapRecord.TileIds.Length == 64 &&
                mapRecord.NpcNameBytes.Take(13).SequenceEqual(
                    System.Text.Encoding.ASCII.GetBytes("JASON\0KATRINA")),
                "bounded standard MTP record");
            byte[] exposedMapTiles = mapRecord.TileIds;
            exposedMapTiles[0] = 0xFF;
            Assert(mapRecord.TileIds[0] == 0, "MTP tile IDs are defensive copies");
            AssertThrows<InvalidDataException>(() => MtpMapRecord.ParseStandard(
                mapFixture.Take(mapFixture.Length - 1).ToArray()), "truncated MTP map rejected");
            byte[] pacifica = ProceduralWorldGenerator.GeneratePacifica();
            byte[] pacificaAgain = ProceduralWorldGenerator.Generate(
                PacificaWorldPreset.CreateSeedTable(), PacificaWorldPreset.WorldVertices);
            byte[] customWorld = ProceduralWorldGenerator.GenerateFromEditorSeed(0x123456);
            Assert(pacifica.Length == 128 * 128 && pacifica.SequenceEqual(pacificaAgain),
                "Pacifica procedural world is complete and deterministic");
            Assert(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pacifica)) ==
                "7EEBD102F87596A53D28F502056BC24A326BBF406343F2E49C4B815089F7CD46",
                "Pacifica procedural world descriptor hash");
            Assert(PacificaWorldPreset.WorldVertices.Length == 274 &&
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                    PacificaWorldPreset.CreateSeedTable())) ==
                    "418F3B67E05FD2B94DCF6ECEFAF8FC5D43E181609B8C368DE3AC7093B12451FD",
                "Pacifica preset retains original vertices and startup RNG construction table");
            AssertThrows<ArgumentOutOfRangeException>(() =>
                PacificaWorldPreset.CreateSeedTable(0x1000000),
                "map seed is restricted to original three-byte RNG state");
            Assert(!pacifica.SequenceEqual(customWorld),
                "custom editor seed changes procedural terrain");
            byte[] overviewTileIds = ProceduralWorldGenerator.BuildOverviewTileIds(pacifica);
            Assert(overviewTileIds.Length == pacifica.Length && overviewTileIds.Max() < 66,
                "Pacifica terrain maps to the complete original TINYLAND overview set");
            Assert(PacificaWorldPreset.FixedLocations.Count == 11 &&
                PacificaWorldPreset.FixedLocations.Count(location => location.IsTown) == 7,
                "Pacifica fixed-map and town marker catalog");
            string worldMapPath = Path.Combine(fixture, "exports", "world-map.html");
            WorldMapExportResult worldMap = WorldMapHtmlExporter.Export(located, worldMapPath);
            string worldHtml = File.ReadAllText(worldMapPath);
            Assert(worldMap.FixedLocationCount == 11 && worldMap.InitialPreset == "Pacifica (Chara III)" &&
                worldHtml.Contains("Pacifica (Chara III) world map", StringComparison.Ordinal) &&
                worldHtml.Contains("Village 1", StringComparison.Ordinal) &&
                worldHtml.Contains("Custom editor seed", StringComparison.Ordinal) &&
                worldHtml.Contains("World vertices", StringComparison.Ordinal) &&
                worldHtml.Contains("function drawVertices()", StringComparison.Ordinal) &&
                worldHtml.Contains("function terrainType(value)", StringComparison.Ordinal) &&
                worldHtml.Contains("id=\"world\" checked", StringComparison.Ordinal) &&
                worldHtml.Contains("const vertices=[0,2,0,3", StringComparison.Ordinal) &&
                worldHtml.Contains("[vertices[r],vertices[r+1],vertices[r+16],vertices[r+17]]",
                    StringComparison.Ordinal) &&
                !worldHtml.Contains("const vertices=\"", StringComparison.Ordinal),
                "self-contained scrollable world-map export");
            AssertThrows<IOException>(() => WorldMapHtmlExporter.Export(located, worldMapPath),
                "world-map export refuses overwrite");
            MtpMapRecord starMapRecord = MtpMapRecord.ParseStarMap(new byte[32 * 24]);
            Assert(!starMapRecord.HasStandardHeader && starMapRecord.Width == 32 &&
                starMapRecord.Height == 24 && starMapRecord.TileIds.Length == 768,
                "raw MAP15 star-map record");
            MtpMapInspection mapInspection = MtpMapExporter.Inspect(located, "map1.mtp");
            Assert(mapInspection.TileSetFileName == "BTTLTECH.ICN" &&
                mapInspection.TileOrder == "RowMajorReadInBlockTraversal",
                "MTP map inspection profile");
            string mapExportPath = Path.Combine(fixture, "exports", "MAP1.png");
            string mapMetadataPath = Path.Combine(fixture, "exports", "MAP1.json");
            MtpMapExportResult mapExport = MtpMapExporter.Export(located, "MAP1.MTP",
                mapExportPath, mapMetadataPath);
            byte[] mapPng = File.ReadAllBytes(mapExportPath);
            Assert(File.Exists(mapMetadataPath) && ReadUInt32BigEndian(mapPng, 16) == 128 &&
                ReadUInt32BigEndian(mapPng, 20) == 128 &&
                File.ReadAllText(mapMetadataPath).Contains("\"SourceTileIds\"", StringComparison.Ordinal),
                "portable MTP map PNG and metadata export");
            AssertThrows<IOException>(() => MtpMapExporter.Export(located, "MAP1.MTP",
                mapExportPath, mapMetadataPath), "MTP map export refuses overwrite");
            Assert(MechShapeSpriteCatalog.All.Count == 0x178 &&
                MechShapeSpriteCatalog.Get(0).Width == 24 &&
                MechShapeSpriteCatalog.Get(0x7C).Width == 16 &&
                MechShapeSpriteCatalog.Get(0x176).Height == 11,
                "assembly-derived MECHSHAP rectangle catalog");
            MechSpriteSheet syntheticSheet = MechSpriteSheetComposer.Compose(columnImage);
            Assert(syntheticSheet.Width == 480 && syntheticSheet.Height == 1848 &&
                syntheticSheet.Sequences.Count == 77, "MECHSHAP sequence sheet geometry");
            Assert(syntheticSheet.Sequences[0].Name == "locust.walk.north" &&
                syntheticSheet.Sequences[0].Frames.Select(frame => frame.SourceSpriteId)
                    .SequenceEqual(new[] { 0, 1, 2, 3 }), "Locust north walk control sequence");
            Assert(syntheticSheet.Sequences[24].Name == "commando.walk.north" &&
                syntheticSheet.Sequences[24].Frames[0].SourceSpriteId == 0x92,
                "Commando sequence block follows Locust block");
            Assert(syntheticSheet.Sequences[53].Name == "teammates.core" &&
                syntheticSheet.Sequences[53].Category == "teammates" &&
                syntheticSheet.Sequences[61].Name == "jason-youngblood.core" &&
                syntheticSheet.Sequences[61].Category == "player" &&
                syntheticSheet.Sequences[69].Name == "enemies-or-civilians.core" &&
                syntheticSheet.Sequences[69].Category == "non-player",
                "personnel palette-role sequence labels");
            Assert(syntheticSheet.Sequences.SelectMany(sequence => sequence.Frames)
                .Select(frame => frame.SourceSpriteId).Distinct().Count() == 0x178,
                "spritesheet includes every extracted MECHSHAP sprite");
            Assert(syntheticSheet.PaletteIndices[0] == 1 && syntheticSheet.PaletteIndices[1] == 2,
                "spritesheet copies source sprite pixels");

            string spriteSheetPath = Path.Combine(fixture, "exports", "MECHSHAP-spritesheet.png");
            string spriteMetadataPath = Path.Combine(fixture, "exports", "MECHSHAP-spritesheet.json");
            MechSpriteSheetExportResult spriteExport = MechSpriteSheetExporter.Export(
                located, spriteSheetPath, spriteMetadataPath);
            Assert(spriteExport.SourceSpriteCount == 0x178 && spriteExport.SequenceCount == 77 &&
                File.Exists(spriteSheetPath) && File.Exists(spriteMetadataPath),
                "MECHSHAP PNG and metadata export");
            Assert(File.ReadAllText(spriteMetadataPath).Contains("\"locust.walk.north\"", StringComparison.Ordinal) &&
                File.ReadAllText(spriteMetadataPath).Contains("\"SourceSpriteId\": 375", StringComparison.Ordinal),
                "MECHSHAP metadata names and complete ID coverage");
            AssertThrows<IOException>(() => MechSpriteSheetExporter.Export(
                located, spriteSheetPath, spriteMetadataPath), "MECHSHAP export refuses overwrite");
            MechSpriteSheetExporter.Export(located, spriteSheetPath, spriteMetadataPath, true);

            string json = InventoryReportWriter.WriteJson(report);
            using (JsonDocument document = JsonDocument.Parse(json))
                Assert(document.RootElement.GetProperty("Files").GetArrayLength() == report.Files.Count, "JSON report");

            File.WriteAllBytes(Path.Combine(fixture, "TRAINING.BLD"), new byte[] { 4, 0, 0xEE, 0xC6, 0xEB });
            report = InstallationInventory.Scan(located, true);
            bld = Find(report, "TRAINING.BLD");
            Assert(bld.Validation.StartsWith("invalid-bld:", StringComparison.Ordinal), "bad BLD length rejected");
            Assert(Find(report, "BTECH.EXE").Sha256?.Length == 64, "optional SHA-256");

            File.WriteAllBytes(Path.Combine(fixture, "TRAINING.BLD"), new byte[] { 3, 0, 0xEE, 0xC6, 0xEB });
            FileInspectionResult decoded = FileInspector.Inspect(located, "training.bld", 0, 3, true);
            Assert(decoded.OffsetDomain.StartsWith("decoded BLD", StringComparison.Ordinal), "decoded offset domain");
            Assert(decoded.Bytes.SequenceEqual(new byte[] { 0xFE, 0x06, 0xFD }), "BLD byte decode");
            Assert(FileInspector.WriteText(decoded).Contains("00000000  FE 06 FD", StringComparison.Ordinal), "hex dump");
            AssertThrows<ArgumentException>(() => FileInspector.Inspect(located, "../BTECH.EXE", 0, 1, false), "path traversal rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => FileInspector.Inspect(located, "BTECH.EXE", 0, 0x1001, false), "oversized read rejected");

            File.WriteAllBytes(Path.Combine(fixture, "BTBORDER.CMP"), new byte[] { 3, 0, 3, 0xAA, 0xBB });
            report = InstallationInventory.Scan(located, false);
            Assert(Find(report, "BTBORDER.CMP").Validation == "invalid-graphics:compression-3", "unknown compression rejected");

            SaveGameDump saveDump = SaveGameInspector.Inspect(located, "game1");
            Assert(saveDump.HeaderMatchesObservedProfile, "save header profile");
            Assert(saveDump.Characters.Count == 16 && saveDump.Mechs.Count == 8, "save record counts");
            Assert(saveDump.Characters[0].NameId == 2 && saveDump.Characters[0].Body == 9, "character offsets");
            Assert(saveDump.Characters[0].MechAssignment == 8, "character mech assignment offset");
            Assert(saveDump.Mechs[0].Name == "TEST MECH" && saveDump.Mechs[0].Tonnage == 20, "mech leading fields");
            Assert(saveDump.Mechs[0].UpgradeLevelFlags == 3, "mech final byte");
            Assert(saveDump.Credits == 123456, "32-bit finance offset");
            Assert(saveDump.PartyMapX == 0x1234 && saveDump.PartyMapY == 0xABCD, "final position words");
            Assert(SaveGameInspector.WriteJson(saveDump).Contains("\"Credits\": 123456", StringComparison.Ordinal), "save JSON");
            CharacterRecordDump selectedCharacter = SaveGameInspector.InspectCharacter(located, "game1", "PLAYER", 0);
            Assert(selectedCharacter.FileOffset == 0x0001 && selectedCharacter.NameId == 2, "targeted player character selection");
            Assert(SaveGameInspector.WriteCharacterText(selectedCharacter).StartsWith("player[0] @0x0001", StringComparison.Ordinal),
                "targeted character text");
            MechRecordDump selectedMech = SaveGameInspector.InspectMech(located, "game1", "player", 0);
            Assert(selectedMech.FileOffset == 0x0111 && selectedMech.Name == "TEST MECH", "targeted player mech selection");
            Assert(SaveGameInspector.WriteMechJson(selectedMech).Contains("\"Name\": \"TEST MECH\"", StringComparison.Ordinal),
                "targeted mech JSON");
            AssertThrows<ArgumentException>(() => SaveGameInspector.InspectCharacter(located, "game1", "party", 0),
                "unknown record group rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => SaveGameInspector.InspectMech(located, "game1", "enemy", 4),
                "out-of-range mech slot rejected");

            SaveGameRecord saveRecord = SaveGameRecord.Parse(save, "synthetic GAME1");
            Assert(saveRecord.PlayerCharacters.Count == 8 && saveRecord.EnemyCharacters.Count == 8 &&
                saveRecord.PlayerMechs.Count == 4 && saveRecord.EnemyMechs.Count == 4, "canonical save record counts");
            Assert(saveRecord.RawBytes.SequenceEqual(save), "canonical save preserves all raw bytes");
            Assert(saveRecord.PlayerCharacters[0].MechAssignment == 8 && saveRecord.PlayerCharacters[0].IsOnFoot, "canonical mech assignment");
            Assert(saveRecord.PlayerCharacters[0].HasTechTraining && saveRecord.PlayerCharacters[0].HasMedicalTraining,
                "canonical character training flags");
            Assert(saveRecord.PlayerMechs[0].ActuatorByte24.CurrentLowNibble == 5 &&
                saveRecord.PlayerMechs[0].ActuatorByte24.CurrentHighNibble == 0x0A, "packed actuator current nibbles");
            Assert(saveRecord.PlayerMechs[0].ActuatorByte24.MaximumLowNibble == 7 &&
                saveRecord.PlayerMechs[0].ActuatorByte24.MaximumHighNibble == 0x0F, "packed actuator maximum nibbles");
            Assert(saveRecord.PlayerMechs[0].UpgradeLevelFlags == 3, "canonical mech tail fields");
            byte[] exposedArmour = saveRecord.PlayerMechs[0].CurrentArmour;
            exposedArmour[0] = 0xFF;
            Assert(saveRecord.PlayerMechs[0].CurrentArmour[0] != 0xFF, "canonical mech arrays are defensive copies");
            byte[] exposedState = saveRecord.SavedStateBytes;
            exposedState[0] = 0xFF;
            Assert(saveRecord.SavedStateBytes[0] != 0xFF, "canonical save state is a defensive copy");
            byte[] fullNameMech = new byte[MechRecord.Length];
            Array.Copy(System.Text.Encoding.ASCII.GetBytes("1234567890ABCDEF"), fullNameMech, 16);
            Assert(MechRecord.Parse(fullNameMech).Name == "1234567890ABCDEF", "complete sixteen-byte mech name");
            byte[] markedNameMech = new byte[MechRecord.Length];
            Array.Copy(System.Text.Encoding.ASCII.GetBytes("LOCUST"), markedNameMech, 6);
            markedNameMech[0] |= 0x80;
            MechRecord markedMech = MechRecord.Parse(markedNameMech);
            Assert(markedMech.Name == "LOCUST" && markedMech.HasProbableNoMechOrDestroyedMarker &&
                markedMech.NameFirstByteRaw == 0xCC, "probable mech name/status high bit exposed separately");
            AssertThrows<InvalidDataException>(() => CharacterRecord.Parse(new byte[16]), "short character record rejected");
            AssertThrows<InvalidDataException>(() => MechRecord.Parse(new byte[124]), "short mech record rejected");

            SaveState editableState = SaveStateEditor.FetchState(save, "GAME1");
            string editableText = SaveStateTextFormat.Write(editableState);
            Assert(editableText.Contains("format=BTCHI_SAVE_STATE_V1", StringComparison.Ordinal) &&
                editableText.Contains("character.player.0.body=9", StringComparison.Ordinal) &&
                editableText.Contains("mech.player.0.name=TEST MECH", StringComparison.Ordinal),
                "editable save text contains typed state fields");
            SaveState unchangedState = SaveStateTextFormat.Parse(editableText,
                SaveStateEditor.FetchState(save, "GAME1"));
            SaveStateUpdateResult unchangedUpdate = SaveStateEditor.UpdateState(save, unchangedState);
            Assert(unchangedUpdate.Changes.Count == 0 && unchangedUpdate.Bytes.SequenceEqual(save),
                "unchanged save text round-trips byte-exactly");

            string changedText = editableText
                .Replace("save.credits=123456", "save.credits=654321", StringComparison.Ordinal)
                .Replace("character.player.0.health=0", "character.player.0.health=77", StringComparison.Ordinal)
                .Replace("mech.player.0.name=TEST MECH", "mech.player.0.name=EDITED MECH", StringComparison.Ordinal);
            SaveState changedState = SaveStateTextFormat.Parse(changedText,
                SaveStateEditor.FetchState(save, "GAME1"));
            SaveStateUpdateResult changedUpdate = SaveStateEditor.UpdateState(save, changedState);
            SaveGameRecord changedRecord = SaveGameRecord.Parse(changedUpdate.Bytes, "edited synthetic GAME1");
            Assert(changedRecord.Credits == 654321 && changedRecord.PlayerCharacters[0].Health == 77 &&
                changedRecord.PlayerMechs[0].Name == "EDITED MECH", "text edits update typed save fields");
            Assert(changedUpdate.Changes.Any(change => change.FileOffset == 0x0D5D) &&
                changedUpdate.Changes.Any(change => change.FileOffset == 0x0010) &&
                changedUpdate.Changes.Any(change => change.FileOffset == 0x0111),
                "save update reports byte offsets");
            Assert(changedUpdate.Bytes[0x0D00] == save[0x0D00], "save update preserves unrelated bytes");
            byte[] exposedUpdate = changedUpdate.Bytes;
            exposedUpdate[0] = 0;
            Assert(changedUpdate.Bytes[0] == 0x0C, "save update bytes are defensive copies");
            AssertThrows<InvalidDataException>(() => SaveStateEditor.UpdateState(new byte[SaveGameRecord.Length], changedState),
                "unrecognized source save profile rejected");
            string wrongHashText = editableText.Replace(editableState.SourceSha256,
                new string('0', 64), StringComparison.Ordinal);
            SaveState wrongHashState = SaveStateTextFormat.Parse(wrongHashText,
                SaveStateEditor.FetchState(save, "GAME1"));
            AssertThrows<InvalidDataException>(() => SaveStateEditor.UpdateState(save, wrongHashState),
                "save text fingerprint mismatch rejected");
            string invalidByteText = editableText.Replace("character.player.0.body=9",
                "character.player.0.body=256", StringComparison.Ordinal);
            SaveState invalidByteState = SaveStateTextFormat.Parse(invalidByteText,
                SaveStateEditor.FetchState(save, "GAME1"));
            AssertThrows<InvalidDataException>(() => SaveStateEditor.UpdateState(save, invalidByteState),
                "out-of-range edited byte rejected");
            AssertThrows<InvalidDataException>(() => SaveStateTextFormat.Parse(
                editableText + "unknown.field=1\n", SaveStateEditor.FetchState(save, "GAME1")),
                "unknown save text key rejected");

            string stateTextPath = Path.Combine(fixture, "exports", "GAME1-state.txt");
            SaveStateTextExportResult stateExport = SaveStateFileService.ExportText(
                located, "GAME1", stateTextPath);
            Assert(File.Exists(stateExport.OutputPath) &&
                File.ReadAllText(stateExport.OutputPath).Contains("source_sha256=", StringComparison.Ordinal),
                "save state command service exports editable text");
            File.WriteAllText(stateTextPath, changedText);
            string editedSavePath = Path.Combine(fixture, "exports", "GAME1.edited");
            SaveStateFileUpdateResult fileUpdate = SaveStateFileService.ImportText(
                located, "GAME1", stateTextPath, editedSavePath);
            Assert(File.Exists(fileUpdate.OutputPath) && fileUpdate.Update.Changes.Count == changedUpdate.Changes.Count &&
                SaveGameRecord.Parse(File.ReadAllBytes(editedSavePath)).Credits == 654321,
                "save state command service imports text to new save");
            AssertThrows<IOException>(() => SaveStateFileService.ImportText(
                located, "GAME1", stateTextPath, Path.Combine(fixture, "GAME1"), true),
                "save import refuses source overwrite");

            byte[] markedEnemySave = (byte[])save.Clone();
            int markedEnemyOffset = 0x0305;
            Array.Clear(markedEnemySave, markedEnemyOffset, 16);
            Array.Copy(System.Text.Encoding.ASCII.GetBytes("LOCUST"), 0,
                markedEnemySave, markedEnemyOffset, 6);
            markedEnemySave[markedEnemyOffset] = 0xFF;
            int emptyEnemyOffset = 0x0305 + 2 * MechRecord.Length;
            markedEnemySave[emptyEnemyOffset] = 0xFF;
            string markedEnemyText = SaveStateTextFormat.Write(
                SaveStateEditor.FetchState(markedEnemySave, "GAME-MARKED"));
            Assert(markedEnemyText.Contains("mech.enemy.0.active=false", StringComparison.Ordinal) &&
                markedEnemyText.Contains("mech.enemy.0.name=LOCUST", StringComparison.Ordinal) &&
                markedEnemyText.Contains("mech.enemy.2.active=false", StringComparison.Ordinal),
                "mech active text replaces probable high-bit marker");
            string activatedEnemyText = markedEnemyText.Replace("mech.enemy.0.active=false",
                "mech.enemy.0.active=true", StringComparison.Ordinal);
            SaveStateUpdateResult activatedEnemy = SaveStateEditor.UpdateState(markedEnemySave,
                SaveStateTextFormat.Parse(activatedEnemyText,
                    SaveStateEditor.FetchState(markedEnemySave, "GAME-MARKED")));
            Assert(activatedEnemy.Bytes[markedEnemyOffset] == (byte)'L' &&
                activatedEnemy.Changes.Count == 1 && activatedEnemy.Changes[0].Field == "mech.enemy.0.active",
                "activating known named mech restores overwritten first letter");
            string invalidEmptyActivation = markedEnemyText.Replace("mech.enemy.2.active=false",
                "mech.enemy.2.active=true", StringComparison.Ordinal);
            AssertThrows<InvalidDataException>(() => SaveStateEditor.UpdateState(markedEnemySave,
                SaveStateTextFormat.Parse(invalidEmptyActivation,
                    SaveStateEditor.FetchState(markedEnemySave, "GAME-MARKED"))),
                "empty 0xFF mech cannot activate without name");
            string populatedEnemyText = invalidEmptyActivation.Replace("mech.enemy.2.name=",
                "mech.enemy.2.name=STINGER", StringComparison.Ordinal);
            SaveStateUpdateResult populatedEnemy = SaveStateEditor.UpdateState(markedEnemySave,
                SaveStateTextFormat.Parse(populatedEnemyText,
                    SaveStateEditor.FetchState(markedEnemySave, "GAME-MARKED")));
            Assert(populatedEnemy.Bytes[emptyEnemyOffset] == (byte)'S' &&
                MechRecord.Parse(populatedEnemy.Bytes.Skip(emptyEnemyOffset).Take(MechRecord.Length).ToArray()).Name == "STINGER",
                "empty mech activates when supplied a name");
            string deactivatedEnemyText = SaveStateTextFormat.Write(
                SaveStateEditor.FetchState(populatedEnemy.Bytes, "GAME-POPULATED")).Replace(
                    "mech.enemy.2.active=true", "mech.enemy.2.active=false", StringComparison.Ordinal);
            SaveStateUpdateResult deactivatedEnemy = SaveStateEditor.UpdateState(populatedEnemy.Bytes,
                SaveStateTextFormat.Parse(deactivatedEnemyText,
                    SaveStateEditor.FetchState(populatedEnemy.Bytes, "GAME-POPULATED")));
            Assert(deactivatedEnemy.Bytes[emptyEnemyOffset] == 0xFF &&
                MechRecord.Parse(deactivatedEnemy.Bytes.Skip(emptyEnemyOffset).Take(MechRecord.Length).ToArray()).Name == "STINGER",
                "deactivating mech writes 0xFF over first name character");

            WeaponTableDump weaponDump = WeaponTableInspector.InspectReferenceTable();
            Assert(weaponDump.Weapons.Count == 33 && weaponDump.RecordLength == 0x11, "weapon table shape");
            Assert(weaponDump.Weapons[0].Name == "Cudgel" && weaponDump.Weapons[3].Name == "VibroBlade", "eleven-byte weapon names");
            Assert(weaponDump.Weapons[9].UsesPersonnelDamageEncoding && weaponDump.Weapons[9].SelectorValue == 4, "personnel repeated attacks");
            Assert(weaponDump.Weapons[15].MechComponentId == 0x10, "weapon/component ID conversion");
            Assert(weaponDump.Weapons[15].Heat == 1 && weaponDump.Weapons[15].EffectiveShortRangeThreshold == 6 &&
                weaponDump.Weapons[15].EffectiveMediumRangeThreshold == 9 && weaponDump.Weapons[15].MaximumRange == 12,
                "mech heat and range decoding");
            Assert(weaponDump.Weapons[25].MissileClusterColumn == 4, "missile cluster selector");
            Assert(WeaponTableInspector.WriteJson(weaponDump).Contains("\"UnknownHeatEffectHighNibble\": 3", StringComparison.Ordinal),
                "weapon JSON preserves unknown high nibble");
            WeaponRecord smallLaser = WeaponRecord.Parse(WeaponReferenceCatalog.Definitions[15].EncodeRecord(), 15);
            Assert(smallLaser.Name == "SmallLaser" && smallLaser.MechComponentId == 0x10 &&
                smallLaser.EffectiveMediumRangeThreshold == 9, "canonical weapon record");

            BldDisassembly bldDump = BldDisassembler.Inspect(located, "script.bld");
            Assert(bldDump.StoredPayloadLength == decodedScript.Length, "BLD disassembly container length");
            Assert(bldDump.DecodedPayloadBytes.SequenceEqual(decodedScript.Select(value => (int)value)), "BLD lossless decoded payload");
            Assert(bldDump.Items.Any(item => item.Mnemonic == "DISPLAY_TEXT" && item.Text == "Hi"), "BLD inline text");
            BldDisassemblyItem branch = bldDump.Items.Single(item => item.Mnemonic == "BRANCH_IF_STATE_NONZERO");
            Assert(branch.BranchTarget == 0x000D && branch.BranchTargetInRange == true, "BLD absolute branch target");
            BldDisassemblyItem stateTable = bldDump.Items.Single(item => item.Mnemonic == "BRANCH_STATE_TABLE");
            Assert(!bldDump.StoppedAtUnresolvedInlineTable &&
                stateTable.InlineBranchTargets.SequenceEqual(new[] { 0x0004, 0x0008 }),
                "BLD in-range-word table boundary inference");
            Assert(bldDump.Items.SelectMany(item => item.DecodedBytes).SequenceEqual(decodedScript.Select(value => (int)value)), "BLD item coverage is lossless");
            Assert(BldDisassembler.WriteText(bldDump).Contains("DISPLAY_TEXT \"Hi\"", StringComparison.Ordinal), "BLD human-readable text output");
            Assert(BldDisassembler.WriteText(bldDump).Contains("targets=0x0004,0x0008, count=2", StringComparison.Ordinal),
                "BLD table targets in human-readable output");
            BldDisassembly badTableDump = BldDisassembler.Inspect(located, "BADTABLE.BLD");
            Assert(badTableDump.StoppedAtUnresolvedInlineTable && badTableDump.Items.Last().Kind == "ambiguous-tail",
                "BLD table fallback preserves an unresolvable tail");
            Assert(BldDisassembler.WriteText(BldDisassembler.Inspect(located, "SOUND.BLD"))
                .Contains("soundId=0x0A (cache-door)", StringComparison.Ordinal), "BLD sound catalog integration");
            Assert(BldDisassembler.WriteText(BldDisassembler.Inspect(located, "RECRUIT.BLD"))
                .Contains("RECRUIT_CRESCENT_HAWK specialtySkill=0x06", StringComparison.Ordinal),
                "BLD recruit-agent opcode semantics");
            Assert(BldDisassembler.WriteText(BldDisassembler.Inspect(located, "REX.BLD"))
                .Contains("RECRUIT_REX_AND_START_KURITA_AMBUSH action=0x1E", StringComparison.Ordinal),
                "BLD Rex/Kurita scripted action semantics");
            Assert(BldDisassembler.WriteText(BldDisassembler.Inspect(located, "ARENA.BLD"))
                .Contains("RUN_ARENA_MECH_COMBAT action=0x23", StringComparison.Ordinal),
                "BLD Arena combat scripted action semantics");
            Assert(BldDisassembler.WriteText(BldDisassembler.Inspect(located, "JAIL.BLD"))
                .Contains("RUN_JAILBREAK_MISSION action=0x28", StringComparison.Ordinal),
                "BLD Jailbreak scripted action semantics");
            AssertThrows<ArgumentOutOfRangeException>(() => BldDisassembler.Inspect(located, "SCRIPT.BLD", decodedScript.Length + 1), "BLD start offset bounds");

            AnimationInspection animationInspection = AnimationInspector.Inspect(located, "o0.anm");
            Assert(animationInspection.FileLength == 0x80 && animationInspection.CompressedFrameStreamLength == 0x4D,
                "ANM container regions");
            Assert(animationInspection.PlaybackControlEntryCount == 2 && animationInspection.HasPlaybackTerminator,
                "ANM first playback terminator");
            Assert(animationInspection.PostTerminatorNonZeroPlaybackByteCount == 1, "ANM preserves post-terminator playback controls");
            Assert(animationInspection.FrameWidth == 88 && animationInspection.FrameHeight == 88 &&
                animationInspection.PackedFrameLength == 0x0F20, "ANM packed frame geometry");
            Assert(animationInspection.DecodedFrameCount == 2 && animationInspection.ConsumedCompressedBytes == 20,
                "ANM sequence frame and cursor totals");
            Assert(animationInspection.Frames[1].NonZeroPackedByteCount == 0,
                "ANM inspection observes accumulated XOR frame");
            AnmFileRecord animationRecord = AnmFileRecord.Parse(animation, "synthetic O0.ANM");
            byte[] exposedAnimationHeader = animationRecord.HeaderBytes;
            exposedAnimationHeader[0] = 0;
            Assert(animationRecord.HeaderBytes[0] == 0x41, "ANM header is a defensive copy");
            Assert(AnimationInspector.WriteText(animationInspection).Contains("Playback entries before first zero: 2", StringComparison.Ordinal),
                "ANM human-readable inspection");
            Assert(AnimationInspector.WriteJson(animationInspection).Contains("\"HeaderTrailerLength\": 19", StringComparison.Ordinal),
                "ANM JSON inspection");
            AssertThrows<InvalidDataException>(() => AnmFileRecord.Parse(new byte[0x32]), "short ANM control header rejected");

            AnimationFrameDecodeResult firstFrame = AnimationFrameDecoder.DecodeFrame(syntheticFrameStream, 0);
            Assert(firstFrame.ConsumedBytes == 10 && firstFrame.TokenCount == 3, "ANM token cursor accounting");
            Assert(firstFrame.PackedBytes[0] == 1 && firstFrame.PackedBytes[1] == 2 && firstFrame.PackedBytes[2] == 3,
                "ANM literal XOR token");
            Assert(firstFrame.PackedBytes[3] == 0xAA && firstFrame.PackedBytes[130] == 0xAA,
                "ANM signed repeat token");
            Assert(firstFrame.PackedBytes[131] == 0x55 && firstFrame.PackedBytes[AnmFileRecord.PackedFrameLength - 1] == 0x55,
                "ANM big-endian extended repeat token");
            AnimationFrameDecodeResult accumulatedFrame = AnimationFrameDecoder.DecodeFrame(syntheticFrameStream, 0, firstFrame.PackedBytes);
            Assert(accumulatedFrame.PackedBytes.All(value => value == 0), "ANM frame XOR accumulation");
            AssertThrows<ArgumentException>(() => AnimationFrameDecoder.DecodeFrame(syntheticFrameStream, 0, new byte[1]),
                "ANM prior frame size rejected");
            AssertThrows<InvalidDataException>(() => AnimationFrameDecoder.DecodeFrame(new byte[] { 0, 0x0F, 0x21, 0 }, 0),
                "ANM frame overflow rejected");

            byte[] packedPixels = new byte[AnmFileRecord.PackedFrameLength];
            packedPixels[0] = 0x12;
            packedPixels[1] = 0xF0;
            AnimationPixelFrame pixelFrame = AnimationPixelDecoder.Decode(packedPixels);
            Assert(pixelFrame.Width == 88 && pixelFrame.Height == 88 &&
                pixelFrame.PaletteIndices.Length == 7744, "ANM pixel frame geometry");
            Assert(pixelFrame.PaletteIndices.Take(4).SequenceEqual(new byte[] { 1, 2, 15, 0 }),
                "ANM high-then-low nibble pixel order");
            byte[] exposedPixels = pixelFrame.PaletteIndices;
            exposedPixels[0] = 0;
            Assert(pixelFrame.PaletteIndices[0] == 1, "ANM pixel indices are a defensive copy");
            AssertThrows<InvalidDataException>(() => AnimationPixelDecoder.Decode(new byte[1]),
                "ANM packed pixel length rejected");
            Assert(animationInspection.Frames[0].PaletteIndexCounts.Sum() == 7744 &&
                animationInspection.Frames[1].NonZeroPixelCount == 0, "ANM inspection pixel statistics");
            Assert(animationInspection.TimingTableBytes.Length == 18 && animationInspection.TimingScaleValue == 6,
                "ANM timing header regions");
            Assert(animationInspection.Frames[0].TimingLookupIndex == 0 &&
                animationInspection.Frames[0].DelayRetraces == 4, "ANM first timing lookup");
            Assert(animationInspection.Frames[1].TimingLookupIndex == 1 &&
                animationInspection.Frames[1].TimingTableValue == 4 &&
                animationInspection.Frames[1].DelayRetraces == 18, "ANM variable timing lookup");
            byte[] invalidTimingAnimation = (byte[])animation.Clone();
            invalidTimingAnimation[0] = 0x40;
            AssertThrows<InvalidDataException>(() =>
                AnimationTimingDecoder.Decode(AnmFileRecord.Parse(invalidTimingAnimation)),
                "ANM out-of-range timing control rejected");

            Assert(AnimationGifTiming.ToCentiseconds(0) == 0 &&
                AnimationGifTiming.ToCentiseconds(4) == 7 &&
                AnimationGifTiming.ToCentiseconds(18) == 30,
                "ANM retraces convert to nearest GIF centisecond at nominal 60 Hz");
            AssertThrows<ArgumentOutOfRangeException>(() => AnimationGifTiming.ToCentiseconds(-1),
                "negative ANM GIF delay rejected");
            byte[] gif = AnimationGifEncoder.Encode(
                new[] { pixelFrame, AnimationPixelDecoder.Decode(new byte[AnmFileRecord.PackedFrameLength]) },
                new[] { 7, 30 });
            Assert(System.Text.Encoding.ASCII.GetString(gif, 0, 6) == "GIF89a" &&
                ReadUInt16(gif, 6) == 88 && ReadUInt16(gif, 8) == 88,
                "ANM GIF signature and geometry");
            Assert(System.Text.Encoding.ASCII.GetString(gif).Contains("NETSCAPE2.0", StringComparison.Ordinal) &&
                gif[gif.Length - 1] == 0x3B, "ANM GIF infinite loop extension and trailer");
            int firstGraphicControl = FindBytes(gif, new byte[] { 0x21, 0xF9, 0x04 });
            int secondGraphicControl = FindBytes(gif, new byte[] { 0x21, 0xF9, 0x04 }, firstGraphicControl + 1);
            Assert(firstGraphicControl >= 0 && ReadUInt16(gif, firstGraphicControl + 4) == 7 &&
                secondGraphicControl >= 0 && ReadUInt16(gif, secondGraphicControl + 4) == 30,
                "ANM GIF per-frame delays");
            AssertThrows<ArgumentException>(() => AnimationGifEncoder.Encode(
                new[] { pixelFrame }, Array.Empty<int>()), "ANM GIF frame/delay mismatch rejected");

            byte[] png = AnimationPngEncoder.Encode(pixelFrame);
            Assert(png.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
                "ANM PNG signature");
            Assert(ReadUInt32BigEndian(png, 16) == 88 && ReadUInt32BigEndian(png, 20) == 88 &&
                png[24] == 8 && png[25] == 3, "ANM indexed PNG header");
            byte[] scanlines = ReadPngScanlines(png);
            Assert(scanlines.Length == 88 * 89 && scanlines[0] == 0 && scanlines[1] == 1 &&
                scanlines[2] == 2 && scanlines[3] == 15 && scanlines[4] == 0, "ANM PNG scanline pixels");

            string exportedFramePath = Path.Combine(fixture, "exports", "frame.png");
            AnimationFrameExportResult export = AnimationFrameExporter.ExportPng(located, "O0.ANM", 0, exportedFramePath);
            Assert(export.FrameIndex == 0 && export.FrameCount == 2 && File.Exists(exportedFramePath),
                "ANM frame PNG export");
            AssertThrows<IOException>(() => AnimationFrameExporter.ExportPng(located, "O0.ANM", 0, exportedFramePath),
                "ANM export refuses overwrite");
            AnimationFrameExporter.ExportPng(located, "O0.ANM", 0, exportedFramePath, true);
            Assert(File.ReadAllBytes(exportedFramePath).Take(8).SequenceEqual(png.Take(8)), "ANM forced export overwrite");
            AssertThrows<ArgumentOutOfRangeException>(() =>
                AnimationFrameExporter.ExportPng(located, "O0.ANM", 2, Path.Combine(fixture, "missing.png")),
                "ANM export frame bounds");

            string sequenceDirectory = Path.Combine(fixture, "sequence");
            AnimationSequenceExportResult sequenceExport = AnimationFrameExporter.ExportPngSequence(
                located, "O0.ANM", sequenceDirectory);
            Assert(sequenceExport.Frames.Count == 2 && Directory.GetFiles(sequenceDirectory, "*.png").Length == 2 &&
                Path.GetFileName(sequenceExport.Frames[0].OutputPath) == "O0-frame-00.png", "ANM numbered PNG sequence export");
            Assert(ReadPngScanlines(File.ReadAllBytes(sequenceExport.Frames[1].OutputPath)).All(value => value == 0),
                "ANM sequence exports accumulated second frame");

            string collisionDirectory = Path.Combine(fixture, "sequence-collision");
            Directory.CreateDirectory(collisionDirectory);
            string collisionPath = Path.Combine(collisionDirectory, "O0-frame-01.png");
            File.WriteAllBytes(collisionPath, new byte[] { 1, 2, 3 });
            AssertThrows<IOException>(() => AnimationFrameExporter.ExportPngSequence(
                located, "O0.ANM", collisionDirectory), "ANM sequence collision rejected");
            Assert(!File.Exists(Path.Combine(collisionDirectory, "O0-frame-00.png")),
                "ANM sequence collisions are preflighted before writes");
            AnimationFrameExporter.ExportPngSequence(located, "O0.ANM", collisionDirectory, true);
            Assert(File.ReadAllBytes(collisionPath).Take(8).SequenceEqual(png.Take(8)),
                "ANM forced sequence overwrite");

            string exportedGifPath = Path.Combine(fixture, "exports", "O0.gif");
            AnimationGifExportResult gifExport = AnimationFrameExporter.ExportGif(
                located, "O0.ANM", exportedGifPath);
            Assert(gifExport.FrameCount == 2 && gifExport.TotalDelayRetraces == 22 &&
                gifExport.TotalDelayCentiseconds == 37 && gifExport.NominalRefreshRateHz == 60,
                "ANM GIF export timing summary");
            Assert(File.ReadAllBytes(exportedGifPath).Take(6).SequenceEqual(
                System.Text.Encoding.ASCII.GetBytes("GIF89a")), "ANM GIF export file");
            AssertThrows<IOException>(() => AnimationFrameExporter.ExportGif(
                located, "O0.ANM", exportedGifPath), "ANM GIF export refuses overwrite");
            AnimationFrameExporter.ExportGif(located, "O0.ANM", exportedGifPath, true);
            AssertThrows<ArgumentException>(() => AnimationFrameExporter.ExportGif(
                located, "O0.ANM", Path.Combine(fixture, "bad.png")), "ANM GIF extension rejected");

            for (int animationIndex = 1; animationIndex < 22; animationIndex++)
                File.WriteAllBytes(Path.Combine(fixture, "O" + animationIndex + ".ANM"), animation);
            string gifBatchDirectory = Path.Combine(fixture, "gif-batch");
            Directory.CreateDirectory(gifBatchDirectory);
            string batchCollision = Path.Combine(gifBatchDirectory, "O21.gif");
            File.WriteAllBytes(batchCollision, new byte[] { 1, 2, 3 });
            AssertThrows<IOException>(() => AnimationFrameExporter.ExportAllGifs(
                located, gifBatchDirectory), "ANM GIF batch collision rejected");
            Assert(!File.Exists(Path.Combine(gifBatchDirectory, "O0.gif")),
                "ANM GIF batch collisions are preflighted before writes");
            AnimationGifBatchExportResult gifBatch = AnimationFrameExporter.ExportAllGifs(
                located, gifBatchDirectory, true);
            Assert(gifBatch.Animations.Count == 22 && gifBatch.TotalFrameCount == 44 &&
                Directory.GetFiles(gifBatchDirectory, "*.gif").Length == 22,
                "complete ANM GIF batch export");
            Assert(File.ReadAllBytes(batchCollision).Take(6).SequenceEqual(
                System.Text.Encoding.ASCII.GetBytes("GIF89a")), "forced ANM GIF batch overwrite");

            SifFileRecord sifRecord = SifFileRecord.Parse(syntheticSif);
            Assert(sifRecord.Frames.Count == 2 && sifRecord.Frames[0].NoteCodes.SequenceEqual(syntheticSif.Take(4)),
                "SIF four-channel frame parsing");
            Assert(PcSpeakerPitch.ToPitDivisor(0x80) == 0 && PcSpeakerPitch.ToFrequencyHz(0x4C) > 1310 &&
                PcSpeakerPitch.ToFrequencyHz(0x4C) < 1330, "SIF rest and executable pitch conversion");
            SifInspection sifInspection = SifInspector.Inspect(located);
            Assert(sifInspection.FrameCount == 2 && sifInspection.RestByteCount == 3 &&
                sifInspection.ActiveNotesPerChannel.SequenceEqual(new[] { 1, 1, 1, 2 }), "SIF inspection counts");
            byte[] pcSifWave = SifRenderer.RenderWave(sifRecord, SifPlaybackMode.PcSpeaker, 8000);
            byte[] tandySifWave = SifRenderer.RenderWave(sifRecord, SifPlaybackMode.Tandy, 8000);
            Assert(System.Text.Encoding.ASCII.GetString(pcSifWave, 0, 4) == "RIFF" &&
                tandySifWave.Length > pcSifWave.Length, "SIF PC-speaker and Tandy WAV rendering");
            AssertThrows<InvalidDataException>(() => SifFileRecord.Parse(new byte[3]), "SIF frame alignment rejected");

            Assert(SoundEffectCatalog.All.Count == 18 && SoundEffectCatalog.Get("0x0A").Name == "cache-door" &&
                SoundEffectCatalog.Get("password-accepted").Id == 0x10, "sound-effect catalog IDs and names");
            Assert(SoundEffectCatalog.Get("mech-energy-weapon").Id == 0x02 &&
                SoundEffectCatalog.Get("personnel-laser").Id == 0x09,
                "confirmed mech and personnel laser sound names");
            AssertThrows<ArgumentOutOfRangeException>(() => SoundEffectCatalog.Get("mech-kick"),
                "incorrect mech-kick sound name is rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => SoundEffectCatalog.Get("laser"),
                "ambiguous laser sound name is rejected");
            Assert(SoundEffectCatalog.ToExecutableWords().Length == 313 &&
                SoundEffectCatalog.ToExecutableWords().Take(10).SequenceEqual(new ushort[]
                    { 1002, 1, 1000, 500, 100, 1, 10, 1, 0, 0 }), "sound-effect table transcription");
            Assert(SoundEffectCatalog.All.Last().ExecutableWordOffset == 304 &&
                SoundEffectCatalog.ToExecutableWords().Skip(304).Take(9).SequenceEqual(new ushort[]
                    { 1001, 5, 4000, 2000, 1, 1, 1, 0, 0 }), "sound-effect table final record");
            Assert(SoundEffectCatalog.CalculateTableSha256() == SoundEffectCatalog.SourceTableSha256,
                "complete sound-effect table transcription hash");
            Assert(SoundEffectCatalog.All.All(effect => SoundEffectRenderer.RenderWave(effect, 8000).Length > 44),
                "all executable sound effects render to WAV");

            VerifyMalformedInputSweep();

            Console.WriteLine("InceptionTools verification passed: " + _assertions + " assertions.");
            return 0;
        }
        finally
        {
            Directory.Delete(fixture, true);
        }
    }

    private static void VerifyCommandLineContract()
    {
        CommandLine parsed = CommandLine.Parse(["--json", "--output=result.json", "input.bin"], "output");
        Assert(parsed.Has("json") && parsed.Get("output") == "result.json" &&
            parsed.Positionals.SequenceEqual(["input.bin"]), "shared CLI flag/value/positional parsing");
        CommandLine terminated = CommandLine.Parse(["--", "--literal"]);
        Assert(terminated.Positionals.SequenceEqual(["--literal"]), "shared CLI option terminator");
        AssertThrows<ArgumentException>(() => CommandLine.Parse(["--output"], "output"),
            "shared CLI rejects missing option value");
        AssertThrows<ArgumentException>(() => CommandLine.Parse(["--json", "--json"]),
            "shared CLI rejects duplicate option");
    }

    private static void VerifyMalformedInputSweep()
    {
        for (int length = 0; length < WeaponRecord.Length; length++)
        {
            byte[] truncated = new byte[length];
            AssertThrows<InvalidDataException>(() => WeaponRecord.Parse(truncated, 0),
                $"weapon parser rejects truncation length {length}");
        }

        for (int length = 0; length < AnmFileRecord.HeaderLength; length += 7)
        {
            byte[] truncated = new byte[length];
            AssertThrows<InvalidDataException>(() => AnmFileRecord.Parse(truncated),
                $"ANM parser rejects truncation length {length}");
        }
    }

    private static InstallationInventoryEntry Find(InstallationInventoryReport report, string name)
    {
        return report.Files.Single(file => file.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static void Assert(bool condition, string description)
    {
        if (!condition)
            throw new InvalidOperationException("Assertion failed: " + description);
        _assertions++;
    }

    private static void AssertThrows<TException>(Action action, string description) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            _assertions++;
            return;
        }
        throw new InvalidOperationException("Assertion failed: " + description);
    }

    private static void WriteUInt16(byte[] data, int offset, int value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
    }

    private static int ReadUInt16(byte[] data, int offset)
    {
        return data[offset] | data[offset + 1] << 8;
    }

    private static int FindBytes(byte[] data, byte[] pattern, int startOffset = 0)
    {
        for (int offset = startOffset; offset <= data.Length - pattern.Length; offset++)
        {
            int index = 0;
            while (index < pattern.Length && data[offset + index] == pattern[index])
                index++;
            if (index == pattern.Length)
                return offset;
        }
        return -1;
    }

    private static void WriteUInt32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);
    }

    private static uint ReadUInt32BigEndian(byte[] data, int offset)
    {
        return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) |
            ((uint)data[offset + 2] << 8) | data[offset + 3];
    }

    private static byte[] ReadPngScanlines(byte[] png)
    {
        using (var compressed = new MemoryStream())
        {
            int offset = 8;
            while (offset < png.Length)
            {
                int length = checked((int)ReadUInt32BigEndian(png, offset));
                string type = System.Text.Encoding.ASCII.GetString(png, offset + 4, 4);
                if (type == "IDAT")
                    compressed.Write(png, offset + 8, length);
                offset += 12 + length;
            }

            compressed.Position = 0;
            using (var decompressed = new MemoryStream())
            using (var zlib = new ZLibStream(compressed, CompressionMode.Decompress))
            {
                zlib.CopyTo(decompressed);
                return decompressed.ToArray();
            }
        }
    }

    private static byte[] EncodeBld(byte[] decoded)
    {
        byte[] file = new byte[decoded.Length + 2];
        WriteUInt16(file, 0, decoded.Length);
        for (int index = 0; index < decoded.Length; index++)
            file[index + 2] = (byte)(((decoded[index] ^ 0xE9) - 0x29) & 0xFF);
        return file;
    }

    private static byte[] BuildSyntheticAnimationFrameStream()
    {
        return new byte[]
        {
            3, 1, 2, 3,
            0x80, 0xAA,
            0, 0x0E, 0x9D, 0x55
        };
    }
}
