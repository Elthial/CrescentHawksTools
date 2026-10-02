using System;
using InceptionTools.Binary;

namespace InceptionTools.Records
{
    public sealed class CharacterRecord
    {
        public const int Length = 0x11;
        private readonly byte[] _rawBytes;
        private readonly byte[] _skills;

        private CharacterRecord(BoundedBinaryReader reader)
        {
            reader.RequireExactLength(Length, "Character record");
            _rawBytes = reader.ReadBytes(0, Length, "character record");
            NameId = reader.ReadByte(0x00, "name ID");
            Body = reader.ReadByte(0x01, "body");
            Dexterity = reader.ReadByte(0x02, "dexterity");
            Charisma = reader.ReadByte(0x03, "charisma");
            _skills = reader.ReadBytes(0x04, 7, "skills");
            WeaponTableIndex = reader.ReadByte(0x0B, "weapon table index");
            MechAssignment = reader.ReadByte(0x0C, "mech assignment");
            ArmourType = reader.ReadByte(0x0D, "armour type");
            ArmourValue = reader.ReadByte(0x0E, "armour value");
            Health = reader.ReadByte(0x0F, "health");
            TrainingFlags = reader.ReadByte(0x10, "training flags");
        }

        public int NameId { get; }
        public int Body { get; }
        public int Dexterity { get; }
        public int Charisma { get; }
        public byte[] Skills => (byte[])_skills.Clone();
        public int WeaponTableIndex { get; }
        public int MechAssignment { get; }
        public bool IsOnFoot => MechAssignment == 0x08;
        public int ArmourType { get; }
        public int ArmourValue { get; }
        public int Health { get; }
        public int TrainingFlags { get; }
        public bool HasTechTraining => (TrainingFlags & 0x01) != 0;
        public bool HasMedicalTraining => (TrainingFlags & 0x02) != 0;
        public int UnknownTrainingFlags => TrainingFlags & 0xFC;
        public byte[] RawBytes => (byte[])_rawBytes.Clone();

        public static CharacterRecord Parse(byte[] data, string sourceName = "character record")
        {
            return new CharacterRecord(new BoundedBinaryReader(data, sourceName));
        }
    }
}
