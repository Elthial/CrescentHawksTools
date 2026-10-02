using System.Globalization;
using System.Text;
using System.Text.Json;
using RevengeTools.Cli;
using RevengeTools.Formats.Units;
using RevengeTools.Installation;

namespace RevengeTools.Formats.Weapons;

public sealed class WeaponEvidenceExportResult
{
    public required string OutputDirectory { get; init; }
    public required int WeaponCount { get; init; }
    public required int AmmoFamilyCount { get; init; }
    public required int UnitTemplateCount { get; init; }
    public required int VehicleTemplateCount { get; init; }
}

public static class WeaponReports
{
    public static WeaponEvidenceExportResult ExportEvidence(GameInstallation installation,
        string outputDirectory, bool overwrite)
    {
        byte[] executable = File.ReadAllBytes(installation.ResolveFile("REVENGE.EXE"));
        WeaponCatalog catalog = WeaponCatalog.Parse(executable);
        IReadOnlyList<UnitRecord> units = UnitCatalog.ParseTemplates(
            File.ReadAllBytes(installation.ResolveFile("MECHTYPE.DAT")));

        OutputFile.WriteText(Path.Combine(outputDirectory, "weapons.json"),
            JsonSerializer.Serialize(catalog.Weapons, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            overwrite);
        OutputFile.WriteText(Path.Combine(outputDirectory, "weapons.csv"), FormatCsv(catalog), overwrite);
        OutputFile.WriteText(Path.Combine(outputDirectory, "WEAPON_TABLE.md"), FormatMarkdown(catalog), overwrite);
        OutputFile.WriteText(Path.Combine(outputDirectory, "equipment-frequency.csv"),
            FormatEquipmentFrequency(units, catalog), overwrite);
        OutputFile.WriteText(Path.Combine(outputDirectory, "unit-loadouts.csv"),
            FormatUnitLoadouts(units, catalog), overwrite);
        OutputFile.WriteText(Path.Combine(outputDirectory, "vehicle-records.csv"),
            FormatVehicleRecords(units), overwrite);

        return new WeaponEvidenceExportResult
        {
            OutputDirectory = Path.GetFullPath(outputDirectory),
            WeaponCount = catalog.Weapons.Count,
            AmmoFamilyCount = catalog.AmmoCapacityMultipliers.Count,
            UnitTemplateCount = units.Count,
            VehicleTemplateCount = units.Count(unit => unit.UnitKind == UnitKind.Vehicle)
        };
    }

    private static string FormatCsv(WeaponCatalog catalog)
    {
        var output = new StringBuilder("index,equipment_id,name,name_pointer,heat,damage,min_range_squares,max_range_units,max_range_squares,weight_half_tons,weight_tons,missile_rack_size,ammo_family_id,ammo_family_index,ammo_capacity_multiplier,reserved_09,critical_slot_count,recovery_delay_ticks,weapon_effect_id,raw_hex\n");
        foreach (WeaponDefinition weapon in catalog.Weapons)
        {
            string multiplier = weapon.AmmunitionFamilyIndex is int familyIndex
                ? catalog.AmmoCapacityMultipliers[familyIndex].ToString(CultureInfo.InvariantCulture) : string.Empty;
            output.Append(weapon.Index).Append(',').Append(weapon.EquipmentId).Append(',')
                .Append('"').Append(weapon.Name.Replace("\"", "\"\"")).Append("\",")
                .Append(weapon.NameSegment.ToString("X4")).Append(':').Append(weapon.NameOffset.ToString("X4")).Append(',')
                .Append(weapon.Heat).Append(',').Append(weapon.Damage).Append(',')
                .Append(weapon.MinimumRangeSquares).Append(',').Append(weapon.MaximumRangeUnits).Append(',')
                .Append(weapon.MaximumRangeSquares).Append(',').Append(weapon.WeightHalfTons).Append(',')
                .Append(weapon.WeightTons.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(weapon.MissileRackSize).Append(',').Append(weapon.AmmunitionFamilyId).Append(',')
                .Append(weapon.AmmunitionFamilyIndex?.ToString(CultureInfo.InvariantCulture) ?? string.Empty).Append(',')
                .Append(multiplier).Append(',').Append(weapon.Reserved09).Append(',')
                .Append(weapon.CriticalSlotCount).Append(',').Append(weapon.RecoveryDelayTicks).Append(',')
                .Append(weapon.WeaponEffectId).Append(',').AppendLine(weapon.RawHex);
        }
        return output.ToString();
    }

    private static string FormatMarkdown(WeaponCatalog catalog)
    {
        var output = new StringBuilder();
        output.AppendLine("# Weapon definition table evidence").AppendLine();
        output.AppendLine("Source: `REVENGE.EXE`, unrelocated MZ image `326A:0435` (runtime `DS:0435`).")
            .AppendLine("There are 23 records of `0x0F` bytes. Equipment IDs are one-based: critical-slot values `01h..17h` select these records.")
            .AppendLine("Ammunition family ID `00h` means no ammunition; IDs `01h..0Eh` select the fourteen unit-record ammunition families.").AppendLine();
        output.AppendLine("| ID | Weapon | Heat | Damage | Min | Max units | Max squares | Weight | Rack | Ammo family | Capacity | Slots | Recovery | Effect |")
            .AppendLine("| ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (WeaponDefinition weapon in catalog.Weapons)
        {
            string family = weapon.AmmunitionFamilyId == 0 ? "—" : weapon.AmmunitionFamilyId.ToString(CultureInfo.InvariantCulture);
            string multiplier = weapon.AmmunitionFamilyIndex is int familyIndex
                ? catalog.AmmoCapacityMultipliers[familyIndex].ToString(CultureInfo.InvariantCulture) : "—";
            output.Append("| `").Append(weapon.EquipmentId.ToString("X2")).Append("` | ")
                .Append(weapon.Name).Append(" | ").Append(weapon.Heat).Append(" | ")
                .Append(weapon.Damage).Append(" | ").Append(weapon.MinimumRangeSquares).Append(" | ")
                .Append(weapon.MaximumRangeUnits).Append(" | ").Append(weapon.MaximumRangeSquares).Append(" | ")
                .Append(weapon.WeightTons.ToString("0.0", CultureInfo.InvariantCulture)).Append(" t | ")
                .Append(weapon.MissileRackSize).Append(" | ").Append(family).Append(" | ")
                .Append(multiplier).Append(" | ").Append(weapon.CriticalSlotCount).Append(" | ")
                .Append(weapon.RecoveryDelayTicks).Append(" | ").Append(weapon.WeaponEffectId).AppendLine(" |");
        }
        output.AppendLine().AppendLine("## Confidence").AppendLine();
        output.AppendLine("- **V:** record count/stride, name pointers, weapon ordering, heat, damage, weight, missile rack size, and the ammunition-family bridge are directly corroborated by table values and consumers.")
            .AppendLine("- **P:** stored maximum range is converted to displayed/game squares by multiplying by three; retain both values in analysis.")
            .AppendLine("- **V:** `+0C` matches the number of critical/equipment slots occupied by each weapon; `+0D` is added to the per-unit/per-weapon recovery counter when fired.")
            .AppendLine("- **P:** `+0E` selects the weapon effect dispatcher and is named `weaponEffectId` pending separation of its visual and sound responsibilities.")
            .AppendLine("- **U:** byte `+09` is zero in every definition and remains reserved.");
        return output.ToString();
    }

    private static string FormatEquipmentFrequency(IReadOnlyList<UnitRecord> units, WeaponCatalog catalog)
    {
        var counts = new Dictionary<(int Group, byte Code), int>();
        foreach (UnitRecord unit in units)
        foreach (UnitEquipmentLocation location in unit.EquipmentLocations)
        foreach (byte rawCode in location.Slots)
        {
            byte code = (byte)(rawCode & 0x7F);
            if (code != 0) counts[(location.Index, code)] = counts.GetValueOrDefault((location.Index, code)) + 1;
        }

        var output = new StringBuilder("raw_location_group,battlemech_interpretation,raw_equipment_id,known_weapon,template_occurrences\n");
        foreach (var entry in counts.OrderBy(entry => entry.Key.Group).ThenBy(entry => entry.Key.Code))
        {
            WeaponDefinition? weapon = entry.Key.Code is >= 1 and <= WeaponCatalog.Count
                ? catalog.Weapons[entry.Key.Code - 1] : null;
            output.Append(entry.Key.Group).Append(',').Append(UnitEquipmentLocation.Names[entry.Key.Group]).Append(',')
                .Append("0x").Append(entry.Key.Code.ToString("X2")).Append(',')
                .Append(weapon?.Name ?? string.Empty).Append(',').AppendLine(entry.Value.ToString(CultureInfo.InvariantCulture));
        }
        return output.ToString();
    }

    private static string FormatUnitLoadouts(IReadOnlyList<UnitRecord> units, WeaponCatalog catalog)
    {
        var output = new StringBuilder("unit_id,unit_name,unit_kind,raw_location_group,location_name,slot_index,raw_value,damaged,base_equipment_id,known_weapon\n");
        foreach (UnitRecord unit in units)
        foreach (UnitEquipmentLocation location in unit.EquipmentLocations)
        for (int slotIndex = 0; slotIndex < location.Slots.Length; slotIndex++)
        {
            byte raw = location.Slots[slotIndex];
            byte equipmentId = (byte)(raw & 0x7F);
            if (equipmentId == 0) continue;
            WeaponDefinition? weapon = equipmentId <= WeaponCatalog.Count
                ? catalog.Weapons[equipmentId - 1] : null;
            string locationName = unit.UnitKind == UnitKind.BattleMech
                ? location.BattleMechLocation : $"raw-group-{location.Index}";
            output.Append("0x").Append(unit.UnitTypeId.ToString("X2")).Append(',')
                .Append(unit.UnitName).Append(',').Append(unit.UnitKind).Append(',')
                .Append(location.Index).Append(',').Append(locationName).Append(',')
                .Append(slotIndex).Append(",0x").Append(raw.ToString("X2")).Append(',')
                .Append((raw & 0x80) != 0 ? "true" : "false").Append(",0x")
                .Append(equipmentId.ToString("X2")).Append(',').AppendLine(weapon?.Name ?? string.Empty);
        }
        return output.ToString();
    }

    private static string FormatVehicleRecords(IReadOnlyList<UnitRecord> units)
    {
        var output = new StringBuilder("id,name,tonnage,walk,jump,tactical_sprite_base,internal_0,internal_1,internal_2,internal_3,internal_4,internal_5,internal_6,internal_7,armor_0,armor_1,armor_2,armor_3,armor_4,armor_5,armor_6,armor_7,armor_8,armor_9,armor_10,equipment_hex\n");
        foreach (UnitRecord unit in units.Where(unit => unit.UnitKind == UnitKind.Vehicle))
        {
            output.Append(unit.UnitTypeId).Append(',').Append(unit.UnitName).Append(',').Append(unit.Tonnage)
                .Append(',').Append(unit.WalkMovement).Append(',').Append(unit.JumpMovement)
                .Append(",0x").Append(unit.TacticalSpriteBase.ToString("X2")).Append(',')
                .Append(string.Join(',', unit.MaximumInternal)).Append(',')
                .Append(string.Join(',', unit.MaximumArmor)).Append(',')
                .AppendLine(Convert.ToHexString(unit.CriticalAndEquipmentData));
        }
        return output.ToString();
    }
}
