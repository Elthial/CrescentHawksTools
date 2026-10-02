using System.Text;

namespace RevengeTools.Formats.Saves;

public sealed record SaveByteDifference(
    int RelativeOffset,
    int FileOffsetA,
    int FileOffsetB,
    string Field,
    byte ValueA,
    byte ValueB);

public sealed record SaveRegionDifference(
    string Region,
    int ChangedByteCount,
    IReadOnlyList<SaveByteDifference> Changes);

public sealed record SaveComparisonReport(
    string SourceA,
    int SlotA,
    string LabelA,
    string SourceB,
    int SlotB,
    string LabelB,
    int ChangedByteCount,
    IReadOnlyList<SaveRegionDifference> Regions);

public static class SaveComparer
{
    public static SaveComparisonReport Compare(byte[] sourceA, int slotA, string nameA,
        byte[] sourceB, int slotB, string nameB)
    {
        RevengeSaveSlot left = RevengeSaveFile.Parse(sourceA, nameA).GetSlot(slotA);
        RevengeSaveSlot right = RevengeSaveFile.Parse(sourceB, nameB).GetSlot(slotB);
        var changes = new List<(string Region, SaveByteDifference Change)>();

        CompareDescriptor(sourceA, left, sourceB, right, changes);
        for (int offset = 0; offset < RevengeSaveFile.PayloadLength; offset++)
        {
            byte valueA = left.Payload[offset];
            byte valueB = right.Payload[offset];
            if (valueA == valueB) continue;
            (string region, string field) = DescribePayloadOffset(offset);
            changes.Add((region, new SaveByteDifference(offset,
                left.PayloadOffset + offset, right.PayloadOffset + offset, field, valueA, valueB)));
        }

        SaveRegionDifference[] regions = changes
            .GroupBy(item => item.Region)
            .Select(group => new SaveRegionDifference(group.Key, group.Count(),
                group.Select(item => item.Change).ToArray()))
            .ToArray();
        return new SaveComparisonReport(nameA, slotA, left.Label, nameB, slotB, right.Label,
            changes.Count, regions);
    }

    private static void CompareDescriptor(byte[] sourceA, RevengeSaveSlot left,
        byte[] sourceB, RevengeSaveSlot right, ICollection<(string Region, SaveByteDifference Change)> changes)
    {
        for (int offset = 0; offset < RevengeSaveFile.DescriptorLength; offset++)
        {
            byte valueA = sourceA[left.DescriptorOffset + offset];
            byte valueB = sourceB[right.DescriptorOffset + offset];
            if (valueA == valueB) continue;
            string field = offset < RevengeSaveFile.DescriptorControlLength
                ? $"control[{offset}]"
                : $"label[{offset - RevengeSaveFile.DescriptorControlLength}]";
            changes.Add(("slotDescriptor", new SaveByteDifference(offset,
                left.DescriptorOffset + offset, right.DescriptorOffset + offset, field, valueA, valueB)));
        }
    }

    private static (string Region, string Field) DescribePayloadOffset(int offset)
    {
        if (offset >= RevengeSaveFile.LiveUnitOffset)
        {
            int relative = offset - RevengeSaveFile.LiveUnitOffset;
            int slot = relative / 0x96;
            int unitOffset = relative % 0x96;
            return ($"liveUnit[{slot:D2}]", $"unit[{slot}].{DescribeUnitOffset(unitOffset)}");
        }

        if (offset is >= 0x0025 and < 0x003D)
            return ("scenarioUnitMapX", $"scenarioUnitMapX[{offset - 0x0025}]");
        if (offset is >= 0x003D and < 0x0055)
            return ("scenarioUnitMapY", $"scenarioUnitMapY[{offset - 0x003D}]");
        if (offset is >= 0x006C and < 0x017A)
        {
            int relative = offset - 0x006C;
            return ("campaignRecords15", $"campaignRecord[{relative / 15}].byte[{relative % 15}]");
        }
        if (offset is >= 0x017A and < 0x01FC)
        {
            int relative = offset - 0x017A;
            int rowOffset = relative % 10;
            return ("lanceAssignments", $"lanceAssignment[{relative / 10}].{DescribeLanceOffset(rowOffset)}");
        }
        if (offset is >= 0x01FC and < 0x0221)
            return ("pilotAvailability", $"pilotAvailability[{offset - 0x01FC}]");

        return offset switch
        {
            0x0000 or 0x0001 => ("campaignHeader", "campaignStage"),
            0x0002 or 0x0003 => ("campaignHeader", "scenarioVariant"),
            0x0004 => ("campaignHeader", "campaignPhase"),
            0x0005 => ("campaignHeader", "trainingSequenceFlag"),
            0x0006 or 0x0007 => ("campaignHeader", "campaignFlags"),
            0x0008 or 0x0009 => ("campaignHeader", "unknown_DS_24E1"),
            0x000A or 0x000B => ("campaignHeader", "unknown_DS_A967"),
            0x000C or 0x000D => ("campaignHeader", "scene3RosterSelector_DS_9991"),
            0x000E or 0x000F => ("campaignHeader", "unknown_DS_88F1"),
            0x0010 or 0x0011 => ("campaignHeader", "unknown_DS_8925"),
            0x0012 or 0x0013 => ("campaignHeader", "unknown_DS_99C5"),
            0x0014 or 0x0015 => ("campaignHeader", "unknown_DS_A965"),
            0x0016 or 0x0017 => ("campaignHeader", "unknown_DS_8594"),
            0x0018 or 0x0019 => ("campaignHeader", "unknown_DS_8108"),
            0x001A or 0x001B => ("campaignHeader", "unknown_DS_810A"),
            >= 0x001C and < 0x001F => ("campaignClock", $"campaignClock[{offset - 0x001C}]"),
            >= 0x001F and < 0x0025 => ("campaignUnknown00A0", $"unknown_DS_00A0[{offset - 0x001F}]"),
            >= 0x0055 and < 0x005C => ("campaignUnknown1B85", $"unknown_DS_1B85[{offset - 0x0055}]"),
            0x005C => ("campaignUnknown1B8E", "unknown_DS_1B8E"),
            >= 0x005D and < 0x006C => ("campaignUnknown1BAD", $"unknown_DS_1BAD[{offset - 0x005D}]"),
            _ => ("campaignUnknown", $"campaignUnknown[0x{offset:X4}]")
        };
    }

    private static string DescribeLanceOffset(int offset) => offset switch
    {
        0 or 1 => "uiLeft",
        2 or 3 => "uiTop",
        4 or 5 => "uiWidth",
        6 => "activeStyle",
        7 => "persistentRosterSlot",
        8 => "liveUnitSlot",
        9 => "pilotId",
        _ => $"byte[{offset}]"
    };

    private static string DescribeUnitOffset(int offset) => offset switch
    {
        0x00 => "deploymentState",
        0x01 => "unitTypeId",
        0x02 => "walkMovement",
        0x03 => "jumpMovement",
        >= 0x04 and <= 0x0B => $"currentInternal[{offset - 0x04}]",
        >= 0x0C and <= 0x16 => $"currentArmor[{offset - 0x0C}]",
        >= 0x17 and <= 0x1E => $"maximumInternal[{offset - 0x17}]",
        >= 0x1F and <= 0x29 => $"maximumArmor[{offset - 0x1F}]",
        >= 0x2A and <= 0x37 => $"weaponFamilyCount[{offset - 0x2A}]",
        >= 0x38 and <= 0x53 => $"currentAmmo[{(offset - 0x38) / 2}].{(((offset - 0x38) & 1) == 0 ? "lo" : "hi")}",
        0x54 => "engineHeatSinkCapacity",
        >= 0x55 and <= 0x83 => $"criticalAndEquipment[0x{offset - 0x55:X2}]",
        0x84 => "tonnage",
        0x85 => "engineHits",
        0x86 => "heatAccumulator.fraction",
        0x87 => "currentHeat",
        0x88 => "tacticalSpriteBase",
        0x89 => "sensorHits",
        0x8A => "gunneryTargetNumber",
        0x8B => "targetMovementModifier",
        0x8C => "missileUpgradeState",
        0x8D => "hasBeagleActiveProbe",
        0x8E => "canSpotIndirectFire",
        0x8F => "hasAccuracyUpgrade",
        0x90 => "hasDoubleHeatSinks",
        0x91 => "hasCASE",
        0x92 => "pilotId",
        0x93 => "pilotExperience",
        0x94 => "allegiance",
        0x95 => "damageModel",
        _ => $"byte[0x{offset:X2}]"
    };
}

public static class SaveComparisonFormatter
{
    public static string Format(SaveComparisonReport report)
    {
        var output = new StringBuilder();
        output.AppendLine($"A: {report.SourceA} slot {report.SlotA} '{report.LabelA}'");
        output.AppendLine($"B: {report.SourceB} slot {report.SlotB} '{report.LabelB}'");
        output.AppendLine($"Changed bytes: {report.ChangedByteCount}");
        foreach (SaveRegionDifference region in report.Regions)
        {
            output.AppendLine();
            output.AppendLine($"[{region.Region}] {region.ChangedByteCount} changed byte(s)");
            foreach (SaveByteDifference change in region.Changes)
                output.AppendLine($"  {(region.Region == "slotDescriptor" ? "descriptor" : "payload")}+0x{change.RelativeOffset:X4} " +
                    $"file 0x{change.FileOffsetA:X4}/0x{change.FileOffsetB:X4} " +
                    $"{change.Field}: 0x{change.ValueA:X2} -> 0x{change.ValueB:X2}");
        }
        return output.ToString().TrimEnd();
    }
}
