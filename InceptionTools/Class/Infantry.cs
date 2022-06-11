using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Class
{
    public class Infantry
    {
        public Infantry(byte[] RawData)
        {
            Name = (Character)RawData[0];
            Body = RawData[1];
            Dexterity = RawData[2];
            Charisma = RawData[3];
            Skills = new Dictionary<Skill, int>
            {
                { Skill.BowsAndBlade, RawData[4] },
                { Skill.Pistol, RawData[5] },
                { Skill.Rifle, RawData[6] },
                { Skill.Gunnery, RawData[7] },
                { Skill.Piloting, RawData[8] },
                { Skill.Tech, RawData[9] },
                { Skill.Medical, RawData[10] }
            };
            Weapon = (InfantryWeapon)RawData[11];
            UnknownValue = RawData[12];
            ArmourType = (InfantryArmour)RawData[13];
            ArmourValue = RawData[14];
            Health = RawData[15];
            UnknownValue2 = RawData[16];
        }

        public Character Name { get; }

        public int Body { get; }

        public int Dexterity { get; }
        
        public int Charisma { get; }

        public Dictionary<Skill, int> Skills { get; }

        public InfantryWeapon Weapon { get; }

        public int UnknownValue { get; }

        public InfantryArmour ArmourType { get; }

        public int ArmourValue { get; }

        public int Health { get; }

        public int UnknownValue2 { get; }

        public override string ToString() => $"{Name}|Health:{Health}|{Weapon}|{ArmourType}";
        
    }
}