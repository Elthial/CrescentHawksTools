using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using InceptionTools.Records;

namespace InceptionTools.Inspection
{
    public sealed class WeaponRecordDump
    {
        public int TableIndex { get; set; }
        public int? InfantryEquipmentId { get; set; }
        public int? MechComponentId { get; set; }
        public string Name { get; set; }
        public int[] RawBytes { get; set; }
        public int DamageEncoding { get; set; }
        public int AttackCountOrClusterColumnRaw { get; set; }
        public bool UsesPersonnelDamageEncoding { get; set; }
        public int SelectorValue { get; set; }
        public int? PersonnelDiceCount { get; set; }
        public int? PersonnelFixedDamageBonus { get; set; }
        public int? MissileClusterColumn { get; set; }
        public int HeatAndEffectRaw { get; set; }
        public int Heat { get; set; }
        public int UnknownHeatEffectHighNibble { get; set; }
        public int PackedRangeThresholds { get; set; }
        public int StoredShortRangeThreshold { get; set; }
        public int StoredMediumRangeThreshold { get; set; }
        public int RangeThresholdScale { get; set; }
        public int EffectiveShortRangeThreshold { get; set; }
        public int EffectiveMediumRangeThreshold { get; set; }
        public int MaximumRange { get; set; }
        public int SkillIndex { get; set; }
    }

    public sealed class WeaponTableDump
    {
        public string Source { get; set; }
        public int RecordLength { get; set; }
        public List<WeaponRecordDump> Weapons { get; set; } = new List<WeaponRecordDump>();
    }

    public static class WeaponTableInspector
    {
        public const int RecordLength = 0x11;
        public const int RecordCount = 33;

        public static WeaponTableDump InspectCapturedTable()
        {
            return Inspect(new GameData().WeaponData);
        }

        public static WeaponTableDump Inspect(IReadOnlyList<byte[]> records)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));
            if (records.Count != RecordCount)
                throw new ArgumentException("Weapon table must contain exactly 33 records.", nameof(records));

            var result = new WeaponTableDump
            {
                Source = "Expanded executable table 3EDB:2ED8..3108 captured in GameData.WeaponData",
                RecordLength = RecordLength
            };

            for (int index = 0; index < records.Count; index++)
            {
                byte[] raw = records[index];
                if (raw == null)
                    throw new ArgumentException("Weapon record " + index + " is null.", nameof(records));
                result.Weapons.Add(ToDump(WeaponRecord.Parse(raw, index, "weapon record " + index)));
            }

            return result;
        }

        public static string WriteJson(WeaponTableDump dump)
        {
            return JsonSerializer.Serialize(dump, new JsonSerializerOptions { WriteIndented = true });
        }

        public static string WriteText(WeaponTableDump dump)
        {
            if (dump == null)
                throw new ArgumentNullException(nameof(dump));

            var output = new StringBuilder();
            output.AppendLine("Weapon table: " + dump.Weapons.Count + " records, 0x" + dump.RecordLength.ToString("X2") + " bytes each");
            output.AppendLine("Source: " + dump.Source);
            foreach (WeaponRecordDump weapon in dump.Weapons)
            {
                output.Append('[').Append(weapon.TableIndex.ToString("X2")).Append("] ")
                    .Append(weapon.Name).Append(" damage=0x").Append(weapon.DamageEncoding.ToString("X2"))
                    .Append(" selector=0x").Append(weapon.AttackCountOrClusterColumnRaw.ToString("X2"))
                    .Append(" heat=").Append(weapon.Heat)
                    .Append(" ranges=").Append(weapon.EffectiveShortRangeThreshold).Append('/')
                    .Append(weapon.EffectiveMediumRangeThreshold).Append('/').Append(weapon.MaximumRange)
                    .Append(" skill=").Append(weapon.SkillIndex);
                if (weapon.MechComponentId.HasValue)
                    output.Append(" component=0x").Append(weapon.MechComponentId.Value.ToString("X2"));
                else
                    output.Append(" equipment=0x").Append(weapon.InfantryEquipmentId.Value.ToString("X2"));
                output.AppendLine();
            }
            return output.ToString();
        }

        private static WeaponRecordDump ToDump(WeaponRecord record)
        {
            return new WeaponRecordDump
            {
                TableIndex = record.TableIndex,
                InfantryEquipmentId = record.InfantryEquipmentId,
                MechComponentId = record.MechComponentId,
                Name = record.Name,
                RawBytes = record.RawBytes.Select(value => (int)value).ToArray(),
                DamageEncoding = record.DamageEncoding,
                AttackCountOrClusterColumnRaw = record.AttackCountOrClusterColumnRaw,
                UsesPersonnelDamageEncoding = record.UsesPersonnelDamageEncoding,
                SelectorValue = record.SelectorValue,
                PersonnelDiceCount = record.PersonnelDiceCount,
                PersonnelFixedDamageBonus = record.PersonnelFixedDamageBonus,
                MissileClusterColumn = record.MissileClusterColumn,
                HeatAndEffectRaw = record.HeatAndEffectRaw,
                Heat = record.Heat,
                UnknownHeatEffectHighNibble = record.UnknownHeatEffectHighNibble,
                PackedRangeThresholds = record.PackedRangeThresholds,
                StoredShortRangeThreshold = record.StoredShortRangeThreshold,
                StoredMediumRangeThreshold = record.StoredMediumRangeThreshold,
                RangeThresholdScale = record.RangeThresholdScale,
                EffectiveShortRangeThreshold = record.EffectiveShortRangeThreshold,
                EffectiveMediumRangeThreshold = record.EffectiveMediumRangeThreshold,
                MaximumRange = record.MaximumRange,
                SkillIndex = record.SkillIndex
            };
        }
    }
}
