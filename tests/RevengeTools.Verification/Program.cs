using System.Security.Cryptography;
using System.Text;
using RevengeTools.Binary;
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
using RevengeTools.SaveEditing;

namespace RevengeTools.Verification;

public static class Program
{
    private static int _assertions;

    public static int Main(string[] args)
    {
        VerifyBoundedReader();
        VerifyPalette();
        VerifyFont();
        VerifyCps();
        VerifyCmp();
        VerifyIcnAndMap();
        VerifyIcnCatalog();
        VerifyCgaPixelTranslation();
        VerifyScene();
        VerifySceneInstructionPrefix();
        VerifyUnits();
        VerifyHitLocations();
        VerifyWeapons();
        VerifyMusicStreams();
        VerifyDigitalSounds();
        VerifySaveAndEditor();
        if (args.Length > 0)
            VerifyExternalSaves(args[0]);
        if (args.Length > 1)
            VerifyExternalCps(args[1]);
        Console.WriteLine($"RevengeTools verification passed: {_assertions} assertions.");
        return 0;
    }

    private static void VerifyBoundedReader()
    {
        var reader = new BoundedBinaryReader(new byte[] { 0x34, 0x12, 0x78, 0x56 }, "fixture");
        Assert(reader.ReadUInt16LittleEndian(0) == 0x1234, "bounded little-endian word");
        Assert(reader.ReadUInt32LittleEndian(0) == 0x56781234, "bounded little-endian dword");
        AssertThrows<InvalidDataException>(() => reader.ReadByte(4), "bounded read rejects EOF");
        AssertThrows<InvalidDataException>(() => reader.ReadBytes(3, 2), "bounded range rejects overflow");
        var textReader = new BoundedBinaryReader(new byte[] { (byte)'A', 0, (byte)'B', (byte)' ' }, "text fixture");
        Assert(textReader.ReadFixedAscii(0, 4) == "A", "fixed ASCII stops at first NUL");
    }

    private static void VerifyPalette()
    {
        byte[] source = Enumerable.Range(0, 768).Select(index => (byte)(index % 64)).ToArray();
        ColPalette palette = ColPalette.Parse(source, "palette fixture");
        Assert(palette.ColorCount == 256 && palette.MaximumSourceComponent == 63, "COL dimensions and component range");
        byte[] png = palette.RenderSwatches(1);
        Assert(png.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "palette PNG signature");
        source[100] = 64;
        AssertThrows<InvalidDataException>(() => ColPalette.Parse(source), "COL rejects 7-bit component");
    }

    private static void VerifyFont()
    {
        const int height = 1;
        byte[] data = new byte[0x104 + 128 * height];
        WriteUInt16(data, 0, data.Length - 2);
        for (int character = 0; character < 128; character++)
        {
            WriteUInt16(data, 2 + character * 2, 0x104 + character * height);
            data[0x104 + character] = (byte)character;
        }
        data[0x102] = height;
        data[0x103] = 8;
        WestwoodFontV2 font = WestwoodFontV2.Parse(data, "font fixture");
        Assert(font.Width == 8 && font.Height == 1 && font.GlyphCount == 128, "Font v2 dimensions");
        Assert(font.GetGlyphRows(65)[0] == 65, "Font v2 glyph offset");
        Assert(font.RenderContactSheet(1).Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "font PNG signature");
        data[0]--;
        AssertThrows<InvalidDataException>(() => WestwoodFontV2.Parse(data), "Font v2 rejects bad length");
    }

    private static void VerifyUnits()
    {
        byte[] table = new byte[UnitCatalog.Names.Count * UnitRecord.Length];
        for (int index = 0; index < UnitCatalog.Names.Count; index++)
        {
            int offset = index * UnitRecord.Length;
            table[offset + 1] = (byte)index;
            table[offset + 2] = 4;
            table[offset + 3] = 2;
            table[offset + 0x54] = 10;
            table[offset + 0x84] = (byte)(20 + index % 5 * 5);
            table[offset + 0x85] = 1;
            table[offset + 0x86] = 0x80;
            table[offset + 0x87] = 0x12;
            table[offset + 0x88] = 0x50;
            table[offset + 0x89] = 2;
            table[offset + 0x8A] = 4;
            table[offset + 0x8B] = 3;
            table[offset + 0x8C] = 1;
            table[offset + 0x8E] = 1;
            table[offset + 0x8F] = 1;
            table[offset + 0x95] = 6;
        }
        IReadOnlyList<UnitRecord> units = UnitCatalog.ParseTemplates(table, "unit fixture");
        Assert(units.Count == 89, "unit table record count");
        Assert(units[0].UnitName == "Locust" && units[^1].UnitName == "BattleMaster+", "unit name endpoints");
        Assert(units[42].UnitName == "Warhammer", "unit ID alignment");
        Assert(units[0].EngineHits == 1 && units[0].SensorHits == 2, "unit critical counters");
        Assert(units[0].HeatAccumulatorQ8_8 == 0x1280 && units[0].CurrentHeat == 0x12 &&
            units[0].HeatFraction == 0x80, "unit fixed-point heat accumulator");
        Assert(units[0].TacticalSpriteBase == 0x50, "unit tactical sprite base");
        Assert(units[0].GunneryTargetNumber == 4 && units[0].TargetMovementModifier == 3,
            "unit combat modifiers");
        Assert(units[0].MissileUpgradeState == 1 && units[0].HasImprovedMissiles &&
            !units[0].HasInfernoRockets && units[0].DamageModel == 6 &&
            units[0].DamageModelKind == UnitDamageModel.MobileHeadquarters,
            "unit damage-model fields");
        Assert(units[0].CanSpotIndirectFire, "unit indirect-fire spotter flag");
        Assert(units[0].HasAccuracyUpgrade, "unit accuracy-upgrade flag");
        AssertThrows<InvalidDataException>(() => UnitCatalog.ParseTemplates(table[..^1]), "unit table rejects truncation");
        Assert(units[55].UnitKind == UnitKind.Vehicle && units[63].UnitKind == UnitKind.Infantry &&
            units[66].UnitKind == UnitKind.BattleMech, "unit kind ranges");
        Assert(units[0].EquipmentLocations.Count == 8 && units[0].EquipmentLocations[2].Slots.Length == 12,
            "unit equipment location boundaries");
    }

    private static void VerifySceneInstructionPrefix()
    {
        byte[] bytes =
        {
            0xFC, 0x02, 0x01,
            0xFA, 0x02, 0x02,
            0xFB, 0x02, 0x18, 0x21, 0x34,
            0xF5, 0x02, 0x03, 0x04,
            0xCF, 0x00,
            0xFF
        };
        SceneInstructionPrefix prefix = SceneInstructionDecoder.DecodeKnownPrefix(bytes);
        Assert(prefix.Instructions.Count == 6 && prefix.ConsumedByteCount == 18 &&
            prefix.Instructions[^2].Name == "SetSceneGlobalFlagBySelector" &&
            prefix.Instructions[^1].Name == "EndSceneScript" &&
            prefix.NextUnknownOpcode is null, "SCENE known-prefix instruction widths");
        Assert(prefix.Instructions[2].Name == "SetUnitAttackTarget" &&
            prefix.Instructions[2].Operands.SequenceEqual(new byte[] { 0x02, 0x18, 0x21, 0x34 }),
            "SCENE ground-target variable-width instruction");

        SceneInstructionPrefix branch = SceneInstructionDecoder.DecodeKnownPrefix(
            new byte[] { 0xF8, 0x03, 0x10, 0x00, 0xFC, 0x00, 0x01 });
        Assert(branch.Instructions.Count == 1 && branch.ConsumedByteCount == 4 &&
            branch.Instructions[0].EndsLinearDecode, "SCENE prefix stops at control flow");

        SceneControlFlowDecode controlFlow = SceneInstructionDecoder.DecodeKnownControlFlow(
            new byte[]
            {
                0xFC, 0x00, 0x01,
                0xF7, 0x08, 0x00,
                0x12, 0x34,
                0xFA, 0x00, 0x02,
                0xFF
            });
        Assert(controlFlow.Instructions.Select(item => item.InstructionOffset)
                .SequenceEqual(new[] { 0x00, 0x03, 0x08, 0x0B }) &&
            controlFlow.UnknownOpcodes.Count == 0,
            "SCENE control-flow decoder follows absolute script target");

        SceneControlFlowDecode remainingHandlers = SceneInstructionDecoder.DecodeKnownControlFlow(
            new byte[]
            {
                0xD0, 0x03, 0x7F,
                0xEF, 0x02, 0x19, 0x0A, 0x00,
                0x44,
                0x00, 0x00,
                0xFF
            });
        Assert(remainingHandlers.Instructions.Any(item =>
                item.Name == "InitializeSceneRuntimeSlot" && item.Length == 3) &&
            remainingHandlers.Instructions.Any(item =>
                item.Name == "JumpIfUnitLacksEquipmentType" &&
                item.BranchTargetInstructionOffset == 0x000A) &&
            remainingHandlers.Instructions.Any(item =>
                item.Name == "FinishInterpreterPassOnOutOfRangeOpcode" && item.Opcode == 0x44) &&
            remainingHandlers.UnknownOpcodes.Count == 0,
            "SCENE D0/EF handlers and generic out-of-range pass finish");

        SceneInstructionPrefix completedDispatch = SceneInstructionDecoder.DecodeKnownPrefix(
            new byte[]
            {
                0xD5, 0x02, 0x03,
                0xE8, 0x04,
                0xE9, 0x05, 0x06, 0x07, 0x08, 0xFF,
                0xEA, 0x09, 0x0A,
                0xF4, 0x0B, 0x0C, 0x0D,
                0xFF
            });
        Assert(completedDispatch.Instructions.Select(item => item.Name).SequenceEqual(new[]
            {
                "GrantUnitOrCampaignUpgrade",
                "AdvanceUnitSceneRoute",
                "SpawnScenarioUnit",
                "AddUnitAmmunitionFamily",
                "RepairUnitArmor",
                "EndSceneScript"
            }) && completedDispatch.ConsumedByteCount == 0x13,
            "SCENE corrected D5/E8/E9/EA/F4 handlers and widths");

        SceneInstructionPrefix completedBranches = SceneInstructionDecoder.DecodeKnownPrefix(
            new byte[] { 0xDA, 0x02, 0x08, 0x00, 0xF0, 0x03, 0x04, 0x0C, 0x00 });
        Assert(completedBranches.Instructions.Count == 1 &&
            completedBranches.Instructions[0].Name == "JumpIfUnitAbsentOrWithdrawn" &&
            completedBranches.Instructions[0].BranchTargetInstructionOffset == 0x0008,
            "SCENE DA conditional branch width");

        SceneControlFlowDecode ammunitionBranch = SceneInstructionDecoder.DecodeKnownControlFlow(
            new byte[] { 0xF0, 0x03, 0x04, 0x05, 0x00, 0xFF });
        Assert(ammunitionBranch.Instructions.Any(item =>
                item.Name == "JumpIfUnitAmmunitionNonzero" &&
                item.BranchTargetInstructionOffset == 0x0005),
            "SCENE F0 conditional branch width");

        SceneInstructionPrefix eventSlots = SceneInstructionDecoder.DecodeKnownPrefix(
            new byte[] { 0xE1, 0x00, 0xE0, 0x00, 0xDC, 0x00, 0xFF });
        Assert(SceneFile.SceneEventMessageBase == 0x42 &&
            SceneFile.SceneEventMessageBase + SceneFile.SceneEventMessageCount == SceneFile.MessageCount &&
            eventSlots.Instructions[0].SceneEventSlot == 0x00 &&
            eventSlots.Instructions[0].SceneEventMessageIndex == 0x42 &&
            eventSlots.Instructions[1].SceneEventSlot == 0x01 &&
            eventSlots.Instructions[1].SceneEventMessageIndex == 0x43 &&
            eventSlots.Instructions[2].SceneEventSlot == 0x05 &&
            eventSlots.Instructions[2].SceneEventMessageIndex == 0x47,
            "SCENE event opcodes map to the six trailing message slots");
    }

    private static void VerifyMusicStreams()
    {
        byte[] stream =
        {
            0xE2, 0x02,
            0xCD, 0x02,
            0x10, 0x03,
            0xCE, 0x03, 0x00,
            0xE6, 0xB8, 0x01, 0x01,
            0xE8, 0x01,
            0xE9, 0x02,
            0xFF
        };
        IReadOnlyList<MusicStreamInstruction> instructions = MusicStreamCatalog.Decode(stream);
        Assert(instructions.Count == 8 && instructions[2].DurationUnits == 4 &&
            instructions[3].BranchTarget == 4, "music stream command widths and loop target");
        Assert(instructions[4].Value == 440 && instructions[4].PitDivisor == 2711 &&
            instructions[4].DurationUnits == 2, "music stream direct-frequency event");
        AssertThrows<InvalidDataException>(() => MusicStreamCatalog.Decode(new byte[] { 0xF0, 0xFF }),
            "music stream rejects unknown opcode");

        ushort[] divisors = new ushort[MusicStreamCatalog.NoteCount];
        divisors[0x10] = 1000;
        byte[] loopedBytes =
        {
            0xE2, 0x02,
            0xCD, 0x02,
            0x10, 0x00,
            0xCE, 0x03, 0x00,
            0xE6, 0xB8, 0x01, 0x01,
            0xFF
        };
        var loopedStream = new MusicStream
        {
            Index = 0,
            ImageSegment = 0,
            ImageOffset = 0,
            RawBytes = loopedBytes,
            Instructions = MusicStreamCatalog.Decode(loopedBytes, 0, divisors)
        };
        MusicPlaybackTrace playback = MusicPlaybackSimulator.Simulate(loopedStream, divisors);
        Assert(playback.Events.Count == 3 && playback.TotalTicks == 8 &&
            playback.Events[0].SourceOffset == 0x04 && playback.Events[1].SourceOffset == 0x04 &&
            playback.Events[2].PitDivisor == 2711 && playback.Events[2].EffectiveTicks == 4,
            "music playback expands loops, duration scale, and PIT divisors");

        byte[] scaleArithmeticBytes =
        {
            0xE2, 0x02,
            0xE8, 0x03,
            0x10, 0x00,
            0xE2, 0x7F,
            0xE9, 0x01,
            0x10, 0x00,
            0xE2, 0xFE,
            0xE9, 0x02,
            0x10, 0x00,
            0xFF
        };
        var scaleArithmeticStream = new MusicStream
        {
            Index = 1,
            ImageSegment = 0,
            ImageOffset = 0,
            RawBytes = scaleArithmeticBytes,
            Instructions = MusicStreamCatalog.Decode(scaleArithmeticBytes, 1, divisors)
        };
        MusicPlaybackTrace scaleArithmetic = MusicPlaybackSimulator.Simulate(
            scaleArithmeticStream, divisors);
        Assert(scaleArithmetic.Events.Select(item => item.DurationScale)
                .SequenceEqual(new[] { 0xFF, 0xFF, 0x00 }) &&
            scaleArithmetic.Events.Select(item => item.EffectiveTicks)
                .SequenceEqual(new[] { 0xFF, 0xFF, 0x01 }),
            "music E8/E9 reproduce driver signed-overflow and wrapping arithmetic");
    }

    private static void VerifyHitLocations()
    {
        const int headerLength = 0x20;
        const int imageLength = HitLocationCatalog.DataImageSegment * 16 + 0x3D20;
        byte[] executable = new byte[headerLength + imageLength];
        executable[0] = (byte)'M';
        executable[1] = (byte)'Z';
        WriteUInt16(executable, 0x08, headerLength / 16);
        int segmentBase = headerLength + HitLocationCatalog.DataImageSegment * 16;

        for (int table = 0; table < HitLocationCatalog.TableCount; table++)
        {
            for (int roll = 0; roll < HitLocationCatalog.ResultCount; roll++)
                WriteUInt16(executable,
                    segmentBase + HitLocationCatalog.TableOffset +
                    table * HitLocationCatalog.TableStride + roll * 2,
                    (table + roll) % 0x0B);
        }
        for (int index = 0; index < HitLocationCatalog.DirectionSelectorCount; index++)
            WriteUInt16(executable,
                segmentBase + HitLocationCatalog.DirectionSelectorOffset + index * 2,
                index % 4);

        HitLocationCatalog catalog = HitLocationCatalog.Parse(executable, "hit-location fixture");
        Assert(catalog.Tables.Count == 5 && catalog.DirectionSelectors.Count == 16,
            "hit-location table and selector counts");
        Assert(catalog.Tables[2].ResultsBy2D6[3] == 5 &&
            HitLocationCatalog.BattleMechLocationName(0x09) == "Center Torso Rear",
            "hit-location roll decoding and BattleMech names");
    }

    private static void VerifyWeapons()
    {
        const int headerLength = 0x20;
        const int imageLength = WeaponCatalog.DataImageSegment * 16 + 0x600;
        byte[] executable = new byte[headerLength + imageLength];
        executable[0] = (byte)'M';
        executable[1] = (byte)'Z';
        WriteUInt16(executable, 0x08, headerLength / 16);
        int segmentBase = headerLength + WeaponCatalog.DataImageSegment * 16;
        for (int index = 0; index < WeaponCatalog.Count; index++)
        {
            int record = segmentBase + WeaponCatalog.TableOffset + index * WeaponDefinition.Length;
            int nameOffset = 0x0200 + index * 0x10;
            WriteUInt16(executable, record, nameOffset);
            WriteUInt16(executable, record + 2, WeaponCatalog.DataImageSegment);
            executable[record + 4] = (byte)(index + 1);
            executable[record + 5] = (byte)(index + 2);
            executable[record + 7] = 3;
            executable[record + 8] = 1;
            executable[record + 0x0B] = (byte)Math.Min(index, WeaponCatalog.AmmoFamilyCount);
            byte[] name = Encoding.ASCII.GetBytes("weapon" + index);
            name.CopyTo(executable, segmentBase + nameOffset);
        }
        for (int index = 0; index < WeaponCatalog.AmmoFamilyCount; index++)
            WriteUInt16(executable, segmentBase + WeaponCatalog.AmmoMultiplierTableOffset + index * 2, index + 10);

        WeaponCatalog catalog = WeaponCatalog.Parse(executable, "weapon fixture");
        Assert(catalog.Weapons.Count == 23 && catalog.Weapons[0].Name == "weapon0",
            "weapon table count and far-name resolution");
        Assert(catalog.Weapons[0].MaximumRangeSquares == 9 && catalog.Weapons[0].WeightTons == 0.5m,
            "weapon range and half-ton conversion");
        Assert(catalog.AmmoCapacityMultipliers[13] == 23,
            "weapon ammunition multiplier table");
    }

    private static void VerifyDigitalSounds()
    {
        const int headerLength = 0x20;
        const int imageLength = DigitalSoundCatalog.DataImageSegment * 16 + 0x6000;
        byte[] executable = new byte[headerLength + imageLength];
        executable[0] = (byte)'M';
        executable[1] = (byte)'Z';
        WriteUInt16(executable, 0x08, headerLength / 16);
        int segmentBase = headerLength + DigitalSoundCatalog.DataImageSegment * 16;
        const ushort resourceIndex = 0x50;
        const ushort resourceNameOffset = 0x2300;
        int resource = segmentBase + DigitalSoundCatalog.ResourceTableOffset +
            resourceIndex * DigitalSoundCatalog.ResourceRecordLength;
        WriteUInt16(executable, resource, resourceNameOffset);
        WriteUInt16(executable, resource + 2, DigitalSoundCatalog.DataImageSegment);
        Encoding.ASCII.GetBytes("INFOCOM.BIN\0").CopyTo(executable, segmentBase + resourceNameOffset);

        for (int index = 0; index < DigitalSoundCatalog.EntryCount; index++)
        {
            int entry = segmentBase + DigitalSoundCatalog.SoundTableOffset + index * DigitalSoundEntry.Length;
            executable[entry] = (byte)(0x41 + index);
            WriteUInt16(executable, entry + 1, resourceIndex);
            WriteUInt32(executable, entry + 3, index * 0x100);
            WriteUInt16(executable, entry + 7, 0x80 + index);
            WriteUInt16(executable, entry + 9, index * 0x80);
        }
        int sequenceTable = segmentBase + DigitalSoundCatalog.LoadBankSequenceTableOffset;
        Encoding.ASCII.GetBytes("ABC\0D\0").CopyTo(executable, sequenceTable);

        DigitalSoundCatalog catalog = DigitalSoundCatalog.Parse(executable, "digital sound fixture");
        Assert(catalog.Entries.Count == 0x16 && catalog.Entries[0].ResourceFileName == "INFOCOM.BIN",
            "digital sound table count and resource-name resolution");
        Assert(catalog.Entries[3].SoundCode == 0x44 && catalog.Entries[3].SourceOffset == 0x300 &&
            catalog.Entries[3].SampleLength == 0x83,
            "digital sound record fields");
        Assert(catalog.SequenceFragments.Count == 2 &&
            catalog.SequenceFragments[0].DataOffset == DigitalSoundCatalog.LoadBankSequenceTableOffset &&
            catalog.SequenceFragments[0].SoundCodes == "ABC" &&
            catalog.SequenceFragments[0].Entries.Select(entry => entry.Index).SequenceEqual(new[] { 0, 1, 2 }) &&
            catalog.SequenceFragments[1].SoundCodes == "D",
            "digital sound sequence fragments resolve every code through the sample table");

        byte[] samples = { 0x00, 0x80, 0xFF };
        byte[] wave = PcmWaveEncoder.EncodeUnsigned8BitMono(samples, DigitalSoundCatalog.PlaybackRateHz);
        Assert(Encoding.ASCII.GetString(wave, 0x00, 0x04) == "RIFF" &&
            Encoding.ASCII.GetString(wave, 0x08, 0x04) == "WAVE" &&
            BitConverter.ToUInt16(wave, 0x14) == PcmWaveEncoder.FormatPcm &&
            BitConverter.ToUInt16(wave, 0x16) == PcmWaveEncoder.ChannelCount &&
            BitConverter.ToUInt32(wave, 0x18) == DigitalSoundCatalog.PlaybackRateHz &&
            BitConverter.ToUInt16(wave, 0x22) == PcmWaveEncoder.BitsPerSample,
            "digital sound WAV format");
        Assert(BitConverter.ToUInt32(wave, 0x28) == samples.Length &&
            wave.AsSpan(0x2C, samples.Length).SequenceEqual(samples) && wave.Length == 0x30,
            "digital sound WAV preserves unsigned PCM bytes and pads odd data");
    }

    private static void VerifyCps()
    {
        byte[] data = new byte[14];
        WriteUInt16(data, 0, data.Length);
        WriteUInt16(data, 2, 3);
        WriteUInt32(data, 4, CpsImage.PixelCount);
        data[10] = 0;
        data[11] = 0;
        data[12] = 4;
        data[13] = 0x2A;
        CpsImage image = CpsImage.Decode(data, "RLE fixture");
        byte[] pixels = image.Pixels;
        Assert(image.CompressionType == 3 && image.DecodedBytesWritten == 4,
            "CPS RLE bounded partial workspace write");
        Assert(pixels.Take(4).All(value => value == 0x2A) && pixels.Skip(4).All(value => value == 0),
            "CPS RLE big-endian extended repeat and zero-filled workspace");

        data[11] = 0xFA;
        data[12] = 1;
        AssertThrows<InvalidDataException>(() => CpsImage.Decode(data, "oversize RLE fixture"),
            "CPS RLE rejects output overrun");
    }

    private static void VerifyCmp()
    {
        byte[] data = new byte[10];
        WriteUInt16(data, 0, data.Length - 2);
        data[2] = 2;
        data[3] = 2;
        data[4] = 0x12;
        data[5] = 0x34;
        data[6] = 0;
        WriteUInt16(data, 7, CmpImage.PackedLength - 2);
        data[9] = 0;
        CmpImage image = CmpImage.Decode(data, "vertical CMP fixture");
        byte[] packed = image.PackedBytes;
        Assert(image.CompressionType == 2 && image.RemainingPayloadBytes == 0,
            "CMP type-2 exact bounded decode");
        Assert(packed[0] == 0x12 && packed[CmpImage.Width / 2] == 0x34 && packed[1] == 0,
            "CMP type-2 column-major traversal");
        Assert(image.Pixels[0] == 1 && image.Pixels[1] == 2,
            "CMP packed nibbles expand to palette indices");

        data[7]++;
        AssertThrows<InvalidDataException>(() => CmpImage.Decode(data, "oversize CMP fixture"),
            "CMP rejects output overrun");
    }

    private static void VerifyIcnAndMap()
    {
        byte[] icnData = new byte[7];
        WriteUInt16(icnData, 0, icnData.Length - 2);
        icnData[2] = 1;
        icnData[3] = 0;
        WriteUInt16(icnData, 4, CmpImage.PackedLength);
        icnData[6] = 0x12;
        IcnImage icons = IcnImage.Decode(icnData, "ICN fixture");
        byte[] sheet = icons.RenderContactSheet(16, out int width, out int height);
        Assert(IcnImage.TileCount == 250 && width == 256 && height == 256,
            "ICN 250-tile contact-sheet dimensions");
        Assert(sheet[0] == 1 && sheet[1] == 2 && sheet[width * 240 + 160] == 0,
            "ICN sequential 16x16 tile placement and unused contact-sheet cells");

        byte[] mapData = new byte[RevengeMap.HeaderLength + 8];
        WriteUInt16(mapData, 0, 8);
        WriteUInt16(mapData, 2, 1);
        for (int index = 0; index < 8; index++)
        {
            mapData[4 + index] = (byte)(0xA0 + index);
            mapData[12 + index] = (byte)(0xA0 + index);
        }
        RevengeMap map = RevengeMap.Parse(mapData, "MAP fixture");
        Assert(map.Width == 8 && map.Height == 1 && map.TileIds[0] == 0xA0 &&
            map.DuplicatedTilePrefix.SequenceEqual(map.TileIds[..8]),
            "MAP dimensions, duplicated first-eight-tile prefix, and tile IDs");
        AssertThrows<InvalidDataException>(() => RevengeMap.Parse(mapData[..^1], "short MAP fixture"),
            "MAP rejects truncated grid");
        mapData[4] ^= 0x01;
        AssertThrows<InvalidDataException>(() => RevengeMap.Parse(mapData, "bad prefix MAP fixture"),
            "MAP rejects a mismatched duplicated tile prefix");
        Assert(MapResourceCatalog.Resolve("map2.map").IconSetFile == "ICONSET0.ICN" &&
            MapResourceCatalog.Resolve("MAPA.MAP").IconSetFile == "ICONSET7.ICN" &&
            MapResourceCatalog.All.Count == 11,
            "SCENE-verified automatic MAP-to-ICONSET catalog");
    }

    private static void VerifyIcnCatalog()
    {
        Assert(IcnCatalog.Classify("ICONSET7.ICN").Kind == IcnCatalogKind.Terrain &&
            IcnCatalog.Classify("DESTROY3.ICN").Kind == IcnCatalogKind.Destruction &&
            IcnCatalog.Classify("MECHSET2.ICN").Kind == IcnCatalogKind.TacticalSprites,
            "ICN filename roles");

        byte[] table = new byte[UnitCatalog.Names.Count * UnitRecord.Length];
        for (int index = 0; index < UnitCatalog.Names.Count; index++)
        {
            int offset = index * UnitRecord.Length;
            table[offset + 1] = (byte)index;
            table[offset + 0x84] = 20;
            table[offset + 0x88] = (byte)(index < 2 ? 0x10 : 0x20);
        }
        IReadOnlyList<UnitSpriteCatalogEntry> catalog = IcnCatalog.BuildUnitSpriteCatalog(
            UnitCatalog.ParseTemplates(table, "sprite catalog fixture"));
        Assert(catalog.Count == 2 && catalog[0].GlobalSpriteBase == 0x110 &&
            catalog[0].FriendlyFacingTiles.SequenceEqual(new[] { 0x10, 0x12, 0x14, 0x16 }) &&
            catalog[0].OpposingFacingTiles.SequenceEqual(new[] { 0x18, 0x1A, 0x1C, 0x1E }) &&
            !catalog[0].IsInfantry,
            "MECHSET tactical sprite row addressing");

        table[2 * UnitRecord.Length + 0x88] = 0xD6;
        table[2 * UnitRecord.Length + 0x95] = (byte)UnitDamageModel.Infantry;
        catalog = IcnCatalog.BuildUnitSpriteCatalog(
            UnitCatalog.ParseTemplates(table, "infantry sprite catalog fixture"));
        UnitSpriteCatalogEntry infantry = catalog.Single(entry => entry.LocalTileBase == 0xD6);
        Assert(infantry.IsInfantry && infantry.FriendlyFacingTiles.SequenceEqual(new[] { 0xD6 }) &&
            infantry.OpposingFacingTiles.SequenceEqual(new[] { 0xD8 }),
            "infantry MECHSET sprite pair addressing");
    }

    private static void VerifyCgaPixelTranslation()
    {
        byte[] seed = Enumerable.Range(0, 0x20).Select(value => (byte)value).ToArray();
        CgaPixelTranslation tables = CgaPixelTranslationCatalog.Build(seed);
        Assert(tables.EvenPhaseLookup[0xA5] == (byte)((0x0A << 2) | 0x15) &&
            tables.OddPhaseLookup[0xA5] == (byte)((0x1A << 2) | 0x05),
            "CGA even/odd packed-pixel lookup expansion");
        Assert(CgaPixelTranslationCatalog.BuildTransparencyMask(0x10, 0x01) == 0x3C &&
            CgaPixelTranslationCatalog.BuildTransparencyMask(0x00, 0x00) == 0xFF,
            "CGA four-pixel transparency-mask expansion");
        AssertThrows<InvalidDataException>(() => CgaPixelTranslationCatalog.Build(seed[..^1]),
            "CGA translation rejects incomplete seed tables");
    }

    private static void VerifyScene()
    {
        byte[] data = new byte[SceneFile.DataOffset + 4 + SceneFile.MessageCount + 1];
        WriteUInt16(data, 0, data.Length - 2);
        WriteUInt16(data, 2, SceneFile.DataOffset);
        for (int index = 1; index < SceneFile.OffsetCount; index++)
            WriteUInt16(data, 2 + index * 2, SceneFile.DataOffset + 4 + index - 1);
        data[SceneFile.MetadataOffset + SceneFile.MapIdMetadataOffset] = 9;
        data[SceneFile.MetadataOffset + SceneFile.IconSetIdMetadataOffset] = 7;
        data[SceneFile.DataOffset] = 0x00;
        data[SceneFile.DataOffset + 1] = 0x00;
        data[SceneFile.DataOffset + 2] = 0xFC;
        data[SceneFile.DataOffset + 3] = 0xFF;
        data[^1] = 0x5A;
        SceneFile scene = SceneFile.Parse(data, "SCENE fixture");
        Assert(scene.ScriptReservedWord == 0 &&
            scene.InstructionBytes.SequenceEqual(new byte[] { 0xFC, 0xFF }) &&
            scene.Messages.Count == 72 && scene.Messages.All(message => message.Text.Length == 0),
            "SCENE offset table, instruction envelope, and message table");
        Assert(scene.MapFileName == "MAP9.MAP" && scene.IconSetFileName == "ICONSET7.ICN" &&
            scene.TrailerByte == 0x5A, "SCENE verified resource selectors and raw trailer");

        WriteUInt16(data, 2 + 2 * 2, SceneFile.DataOffset);
        AssertThrows<InvalidDataException>(() => SceneFile.Parse(data, "overlapping SCENE fixture"),
            "SCENE rejects overlapping message spans");
    }

    private static void VerifySaveAndEditor()
    {
        byte[] source = BuildSaveFixture();
        RevengeSaveFile save = RevengeSaveFile.Parse(source, "save fixture");
        RevengeSaveSlot slot = save.GetSlot(1);
        Assert(slot.Label == "fixture" && slot.CampaignStage == 3, "save slot descriptor and campaign stage");
        Assert(slot.Units.Count == 24 && slot.Units[0].UnitName == "Locust" && slot.Units[0].Tonnage == 20,
            "save live unit records");

        SaveState state = SaveStateEditor.FetchState(source, 1, "SAVEGAME.DAT");
        string text = SaveStateTextFormat.Write(state);
        SaveState parsed = SaveStateTextFormat.Parse(text, SaveStateEditor.FetchState(source, 1, "SAVEGAME.DAT"));
        SaveStateUpdateResult unchanged = SaveStateEditor.UpdateState(source, parsed);
        Assert(unchanged.Changes.Count == 0 && unchanged.Bytes.SequenceEqual(source), "save no-op round trip exact");

        parsed.Label = "edited";
        parsed.Units[0].CurrentInternal[0] = 4;
        parsed.Units[0].CurrentAmmo[0] = 9;
        SaveStateUpdateResult changed = SaveStateEditor.UpdateState(source, parsed);
        Assert(changed.Changes.Count > 0 && RevengeSaveFile.Parse(changed.Bytes).GetSlot(1).Label == "edited",
            "save editor patches label");
        Assert(RevengeSaveFile.Parse(changed.Bytes).GetSlot(1).Units[0].CurrentInternal[0] == 4 &&
            RevengeSaveFile.Parse(changed.Bytes).GetSlot(1).Units[0].CurrentAmmo[0] == 9,
            "save editor patches verified current state");
        Assert(changed.Bytes[0x97] == source[0x97], "save editor preserves unknown header bytes");

        byte[] comparisonBytes = (byte[])source.Clone();
        int comparisonPayload = RevengeSaveFile.ExpectedBoundaries[2];
        comparisonBytes[comparisonPayload + 0x0025 + 3] = 0x44;
        comparisonBytes[comparisonPayload + 0x017A + 7] = 0x05;
        comparisonBytes[comparisonPayload + RevengeSaveFile.LiveUnitOffset + 0x85] = 0x80;
        SaveComparisonReport comparison = SaveComparer.Compare(source, 1, "before", comparisonBytes, 1, "after");
        Assert(comparison.ChangedByteCount == 3 && comparison.Regions.Count == 3,
            "save comparison groups changed regions");
        IReadOnlyList<SaveByteDifference> comparisonChanges = comparison.Regions
            .SelectMany(region => region.Changes).ToArray();
        Assert(comparisonChanges.Any(change => change.Field == "scenarioUnitMapX[3]") &&
            comparisonChanges.Any(change => change.Field == "lanceAssignment[0].persistentRosterSlot") &&
            comparisonChanges.Any(change => change.Field == "unit[0].engineHits"),
            "save comparison assigns semantic field names");

        string analysisDirectory = Path.Combine(Path.GetTempPath(), "RevengeTools-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(analysisDirectory);
            File.WriteAllBytes(Path.Combine(analysisDirectory, "SAVEGAME.S01"), source);
            SaveSetAnalysisReport analysis = SaveSetAnalyzer.Analyze(analysisDirectory);
            Assert(analysis.FileCount == 1 && analysis.CheckpointCount == 6 &&
                analysis.ByteProfiles.Count == 85, "save-set analysis profiles occupied checkpoints");
        }
        finally
        {
            if (Directory.Exists(analysisDirectory)) Directory.Delete(analysisDirectory, true);
        }

        SaveState badFingerprint = SaveStateEditor.FetchState(source, 1);
        badFingerprint.SourceSha256 = new string('0', 64);
        AssertThrows<InvalidDataException>(() => SaveStateEditor.UpdateState(source, badFingerprint),
            "save editor enforces fingerprint");
        SaveState readOnly = SaveStateEditor.FetchState(source, 1);
        readOnly.CampaignStage++;
        AssertThrows<InvalidDataException>(() => SaveStateEditor.UpdateState(source, readOnly),
            "save editor rejects unproved campaign edit");
    }

    private static byte[] BuildSaveFixture()
    {
        byte[] data = new byte[RevengeSaveFile.Length];
        for (int index = 0; index < RevengeSaveFile.ExpectedBoundaries.Length; index++)
            WriteUInt16(data, index * 2, RevengeSaveFile.ExpectedBoundaries[index]);
        Encoding.ASCII.GetBytes("fixture", data.AsSpan(RevengeSaveFile.DescriptorOffset + 5));
        data[RevengeSaveFile.DescriptorOffset + 1] = 1;
        for (int slot = 0; slot < RevengeSaveFile.SlotCount; slot++)
        {
            int payload = RevengeSaveFile.ExpectedBoundaries[slot + 2];
            WriteUInt16(data, payload, slot + 3);
            int unit = payload + RevengeSaveFile.LiveUnitOffset;
            data[unit] = 1;
            data[unit + 1] = 0;
            for (int index = 0; index < 8; index++)
            {
                data[unit + 0x04 + index] = 5;
                data[unit + 0x17 + index] = 5;
            }
            for (int index = 0; index < 11; index++)
            {
                data[unit + 0x0C + index] = 6;
                data[unit + 0x1F + index] = 6;
            }
            WriteUInt16(data, unit + 0x38, 10);
            data[unit + 0x84] = 20;
        }
        data[0x97] = 0xA5;
        return data;
    }

    private static void VerifyExternalSaves(string directory)
    {
        string fullDirectory = Path.GetFullPath(directory);
        string[] files = Directory.GetFiles(fullDirectory, "SAVEGAME.S??").OrderBy(path => path).ToArray();
        if (files.Length == 0)
            throw new InvalidOperationException("No SAVEGAME.S?? files found in " + fullDirectory);
        foreach (string file in files)
        {
            byte[] source = File.ReadAllBytes(file);
            RevengeSaveFile parsedFile = RevengeSaveFile.Parse(source, Path.GetFileName(file));
            Assert(parsedFile.Slots.Count == RevengeSaveFile.SlotCount, Path.GetFileName(file) + " slot count");
            for (int slot = 1; slot <= RevengeSaveFile.SlotCount; slot++)
            {
                SaveState baseline = SaveStateEditor.FetchState(source, slot, Path.GetFileName(file));
                string text = SaveStateTextFormat.Write(baseline);
                SaveState reparsed = SaveStateTextFormat.Parse(text,
                    SaveStateEditor.FetchState(source, slot, Path.GetFileName(file)));
                SaveStateUpdateResult result = SaveStateEditor.UpdateState(source, reparsed);
                Assert(result.Changes.Count == 0 && result.Bytes.SequenceEqual(source),
                    $"{Path.GetFileName(file)} slot {slot} no-op round trip");
            }
        }
    }

    private static void VerifyExternalCps(string directory)
    {
        string fullDirectory = Path.GetFullPath(directory);
        WeaponCatalog weapons = WeaponCatalog.Parse(
            File.ReadAllBytes(Path.Combine(fullDirectory, "REVENGE.EXE")), "REVENGE.EXE");
        Assert(weapons.Weapons.Count == 23 && weapons.Weapons[0].Name == "Small Laser" &&
            weapons.Weapons[^1].Name == "SRM-6", "local executable weapon table endpoints");
        Assert(weapons.Weapons[9].AmmunitionFamilyId == 1 &&
            weapons.Weapons[^1].AmmunitionFamilyId == 14,
            "local executable weapon-to-ammunition-family bridge");
        Assert(weapons.Weapons[7].CriticalSlotCount == 3 &&
            weapons.Weapons.All(weapon => weapon.RecoveryDelayTicks == 8),
            "local executable critical-slot and weapon-recovery fields");
        string[] files = Directory.GetFiles(fullDirectory, "*.CPS").OrderBy(path => path).ToArray();
        Assert(files.Length == 39, "local CPS file count");
        var typeCounts = new Dictionary<int, int>();
        foreach (string file in files)
        {
            CpsImage image = CpsImage.Decode(File.ReadAllBytes(file), Path.GetFileName(file));
            typeCounts[image.CompressionType] = typeCounts.GetValueOrDefault(image.CompressionType) + 1;
            Assert(image.Pixels.Length == CpsImage.PixelCount &&
                image.CompressedBytesConsumed <= image.CompressedBytesAvailable,
                Path.GetFileName(file) + " bounded CPS decode");
        }
        Assert(typeCounts.GetValueOrDefault(1) == 10 && typeCounts.GetValueOrDefault(2) == 28 &&
            typeCounts.GetValueOrDefault(3) == 1, "local CPS compression distribution");
        CpsImage border = CpsImage.Decode(File.ReadAllBytes(Path.Combine(fullDirectory, "BTBORDER.CPS")), "BTBORDER.CPS");
        Assert(border.DecodedBytesWritten == CpsImage.PixelCount,
            "BTBORDER expands to the declared full-screen workspace");
        CmpImage booms = CmpImage.Decode(File.ReadAllBytes(Path.Combine(fullDirectory, "BOOMS.CMP")), "BOOMS.CMP");
        Assert(booms.CompressionType == 2 && booms.RemainingPayloadBytes == 0 &&
            booms.Pixels.Distinct().Count() == 12, "BOOMS exact vertical-RLE sprite-sheet decode");

        string[] iconFiles = Directory.GetFiles(fullDirectory, "*.ICN").OrderBy(path => path).ToArray();
        Assert(iconFiles.Length == 18, "local ICN file count");
        foreach (string file in iconFiles)
        {
            IcnImage image = IcnImage.Decode(File.ReadAllBytes(file), Path.GetFileName(file));
            int expectedTrailing = Path.GetFileName(file).Equals("ICONSET3.ICN", StringComparison.OrdinalIgnoreCase)
                ? 2941 : 0;
            Assert(image.RemainingPayloadBytes == 0 && image.TrailingBytes == expectedTrailing,
                Path.GetFileName(file) + " exact declared ICN stream");
        }

        string[] mapFiles = Directory.GetFiles(fullDirectory, "*.MAP").OrderBy(path => path).ToArray();
        Assert(mapFiles.Length == 11, "local MAP file count");
        foreach (string file in mapFiles)
        {
            RevengeMap map = RevengeMap.Parse(File.ReadAllBytes(file), Path.GetFileName(file));
            Assert(map.TileIds.Max() < IcnImage.TileCount,
                Path.GetFileName(file) + " tile IDs resolve in a 250-tile ICN set");
        }

        string[] sceneFiles = Directory.GetFiles(fullDirectory, "SCENE*.DAT").OrderBy(path => path).ToArray();
        Assert(sceneFiles.Length == 30, "local SCENE file count");
        var relationships = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string file in sceneFiles)
        {
            SceneFile scene = SceneFile.Parse(File.ReadAllBytes(file), Path.GetFileName(file));
            relationships.Add(scene.MapFileName + ":" + scene.IconSetFileName);
            Assert(scene.Messages.Count == 72 && scene.ScriptBytes.Length > 0,
                Path.GetFileName(file) + " bounded script and messages");
        }
        Assert(relationships.Count == 11, "SCENE files select 11 unique map/icon-set pairs");
        SceneFile firstScene = SceneFile.Parse(File.ReadAllBytes(Path.Combine(fullDirectory, "SCENE1.DAT")), "SCENE1.DAT");
        SceneFile lastMapScene = SceneFile.Parse(File.ReadAllBytes(Path.Combine(fullDirectory, "SCENER.DAT")), "SCENER.DAT");
        Assert(firstScene.MapId == 0 && firstScene.IconSetId == 0 &&
            lastMapScene.MapId == 10 && lastMapScene.IconSetId == 7,
            "SCENE resource selector endpoints");
    }

    private static void WriteUInt16(byte[] data, int offset, int value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
    }

    private static void WriteUInt32(byte[] data, int offset, int value)
    {
        WriteUInt16(data, offset, value);
        WriteUInt16(data, offset + 2, value >> 16);
    }

    private static void Assert(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException("Assertion failed: " + description);
        _assertions++;
    }

    private static void AssertThrows<TException>(Action action, string description) where TException : Exception
    {
        try { action(); }
        catch (TException) { _assertions++; return; }
        throw new InvalidOperationException("Assertion failed: " + description);
    }
}
