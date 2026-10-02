using System;
using System.Collections.Generic;
using System.Text;
using InceptionTools.Records;

namespace InceptionTools.Class
{
    public class Infantry
    {
        public Infantry(byte[] RawData)
        {
            Record = CharacterRecord.Parse(RawData, "legacy Infantry input");
            Name = (Character)Record.NameId;
            Body = Record.Body;
            Dexterity = Record.Dexterity;
            Charisma = Record.Charisma;
            Skills = new Dictionary<Skill, int>
            {
                { Skill.BowsAndBlade, Record.Skills[0] },
                { Skill.Pistol, Record.Skills[1] },
                { Skill.Rifle, Record.Skills[2] },
                { Skill.Gunnery, Record.Skills[3] },
                { Skill.Piloting, Record.Skills[4] },
                { Skill.Tech, Record.Skills[5] },
                { Skill.Medical, Record.Skills[6] }
            };
            Weapon = (InfantryWeapon)Record.WeaponTableIndex;
            MechAssignment = Record.MechAssignment;
            ArmourType = (InfantryArmour)Record.ArmourType;
            ArmourValue = Record.ArmourValue;
            Health = Record.Health;
            TrainingFlags = Record.TrainingFlags;
        }

        public CharacterRecord Record { get; }

        public Character Name { get; }

        public int Body { get; }

        public int Dexterity { get; }

        public int Charisma { get; }

        public Dictionary<Skill, int> Skills { get; }

        public InfantryWeapon Weapon { get; }

        public int MechAssignment { get; }
        public bool IsOnFoot => Record.IsOnFoot;

        [Obsolete("Use MechAssignment.")]
        public int UnknownValue => MechAssignment;

        public InfantryArmour ArmourType { get; }

        public int ArmourValue { get; }

        public int Health { get; }

        public int TrainingFlags { get; }
        public bool HasTechTraining => Record.HasTechTraining;
        public bool HasMedicalTraining => Record.HasMedicalTraining;
        public int UnknownTrainingFlags => Record.UnknownTrainingFlags;

        [Obsolete("Use TrainingFlags.")]
        public int UnknownValue2 => TrainingFlags;

        public override string ToString() => $"{Name}|Health:{Health}|{Weapon}|{ArmourType}";

    }
}
