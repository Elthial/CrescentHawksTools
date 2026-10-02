using InceptionTools.Class;
using System;
using System.Collections.Generic;
using System.Text;
using InceptionTools.Records;

namespace InceptionTools.Data
{
    public class Mech
    {
        private readonly Dictionary<MechLimb, Actuator> _actuators;

        public Mech(byte[] RawData)
        {
            Record = MechRecord.Parse(RawData, "legacy Mech input");
            Name = Record.Name;
            Tonnage = Record.Tonnage;
            Armour = new Dictionary<ArmourSlot, Armour>()
            {
                { ArmourSlot.R_Arm,     new Armour( Record.CurrentArmour[0], Record.MaximumArmour[0])},
                { ArmourSlot.R_Leg,     new Armour( Record.CurrentArmour[1], Record.MaximumArmour[1])},
                { ArmourSlot.R_Torso,   new Armour( Record.CurrentArmour[2], Record.MaximumArmour[2])},
                { ArmourSlot.Head,      new Armour( Record.CurrentArmour[3], Record.MaximumArmour[3])},
                { ArmourSlot.C_Torso,   new Armour( Record.CurrentArmour[4], Record.MaximumArmour[4])},
                { ArmourSlot.L_Arm,     new Armour( Record.CurrentArmour[5], Record.MaximumArmour[5])},
                { ArmourSlot.L_Leg,     new Armour( Record.CurrentArmour[6], Record.MaximumArmour[6])},
                { ArmourSlot.L_Torso,   new Armour( Record.CurrentArmour[7], Record.MaximumArmour[7])},
                { ArmourSlot.R_Torso_R, new Armour( Record.CurrentArmour[8], Record.MaximumArmour[8])},
                { ArmourSlot.C_Torso_R, new Armour( Record.CurrentArmour[9], Record.MaximumArmour[9])},
                { ArmourSlot.L_Torso_R, new Armour( Record.CurrentArmour[10], Record.MaximumArmour[10])},
            };
            InternalStructure = new Dictionary<CriticalSlot, InternalStructure>()
            {
                { CriticalSlot.R_Arm,     new InternalStructure( Record.CurrentStructure[0], Record.MaximumStructure[0])},
                { CriticalSlot.R_Leg,     new InternalStructure( Record.CurrentStructure[1], Record.MaximumStructure[1])},
                { CriticalSlot.R_Torso,   new InternalStructure( Record.CurrentStructure[2], Record.MaximumStructure[2])},
                { CriticalSlot.Head,      new InternalStructure( Record.CurrentStructure[3], Record.MaximumStructure[3])},
                { CriticalSlot.C_Torso,   new InternalStructure( Record.CurrentStructure[4], Record.MaximumStructure[4])},
                { CriticalSlot.L_Arm,     new InternalStructure( Record.CurrentStructure[5], Record.MaximumStructure[5])},
                { CriticalSlot.L_Leg,     new InternalStructure( Record.CurrentStructure[6], Record.MaximumStructure[6])},
                { CriticalSlot.L_Torso,   new InternalStructure( Record.CurrentStructure[7], Record.MaximumStructure[7])},
            };

            // Historical compatibility only: this dictionary duplicates whole
            // packed bytes and must not be used for limb state. Use the neutral
            // ActuatorByte24/ActuatorByte25 nibble-aware properties instead.
            _actuators = new Dictionary<MechLimb, Actuator>()
            {
                { MechLimb.R_Arm, new Actuator( Record.ActuatorByte24.CurrentRaw, Record.ActuatorByte24.MaximumRaw)},
                { MechLimb.R_Leg, new Actuator( Record.ActuatorByte24.CurrentRaw, Record.ActuatorByte24.MaximumRaw)},
                { MechLimb.L_Arm, new Actuator( Record.ActuatorByte25.CurrentRaw, Record.ActuatorByte25.MaximumRaw)},
                { MechLimb.L_Leg, new Actuator( Record.ActuatorByte25.CurrentRaw, Record.ActuatorByte25.MaximumRaw)},
            };

            EngineHeatSinks = Record.EngineHeatSinks;

            Ammo = new Dictionary<int, Ammo>()
            {
                { 0,  new Ammo( Record.CurrentAmmo[0], Record.MaximumAmmo[0])},
                { 1,  new Ammo( Record.CurrentAmmo[1], Record.MaximumAmmo[1])},
                { 2,  new Ammo( Record.CurrentAmmo[2], Record.MaximumAmmo[2])},
                { 3,  new Ammo( Record.CurrentAmmo[3], Record.MaximumAmmo[3])},
                { 4,  new Ammo( Record.CurrentAmmo[4], Record.MaximumAmmo[4])},
                { 5,  new Ammo( Record.CurrentAmmo[5], Record.MaximumAmmo[5])},
                { 6,  new Ammo( Record.CurrentAmmo[6], Record.MaximumAmmo[6])},
                { 7,  new Ammo( Record.CurrentAmmo[7], Record.MaximumAmmo[7])},
                { 8,  new Ammo( Record.CurrentAmmo[8], Record.MaximumAmmo[8])},
                { 9,  new Ammo( Record.CurrentAmmo[9], Record.MaximumAmmo[9])},
            };

            WalkMove = Record.WalkMove;
            JumpMove = Record.JumpMove;

            byte[] critical = Record.CriticalSlotsRaw;
            CriticalSlots = new Dictionary<CriticalSlot, Critical>()
            {
                { CriticalSlot.L_Arm,    new Critical(7, critical[0..7])},
                { CriticalSlot.L_Torso,  new Critical(7, critical[7..14])},
                { CriticalSlot.R_Arm,    new Critical(7, critical[14..21])},
                { CriticalSlot.R_Torso,  new Critical(7, critical[21..28])},
                { CriticalSlot.L_Leg,    new Critical(2, critical[28..30])},
                { CriticalSlot.R_Leg,    new Critical(2, critical[30..32])},
                { CriticalSlot.C_Torso,  new Critical(2, critical[32..34])},
                { CriticalSlot.Head,     new Critical(1, new byte[] { critical[34] })},
            };
        }

        public MechRecord Record { get; }

        public string Name { get; }
        public int Tonnage { get; }

        public Dictionary<ArmourSlot, Armour> Armour { get; }

        public Dictionary<CriticalSlot, InternalStructure> InternalStructure { get; }

        public Dictionary<CriticalSlot, Critical> CriticalSlots { get; }

        [Obsolete("Packed actuator bytes do not map safely to four scalar limbs. Use ActuatorByte24 and ActuatorByte25.")]
        public Dictionary<MechLimb, Actuator> Actuators => _actuators;

        public PackedActuatorByte ActuatorByte24 => Record.ActuatorByte24;
        public PackedActuatorByte ActuatorByte25 => Record.ActuatorByte25;

        public Dictionary<int, Ammo> Ammo { get; }
        
        public int EngineHeatSinks { get; }

        public int WalkMove { get; }
        public int JumpMove { get; }

        public int EngineHits => Record.EngineHits;
        public int GyroHits => Record.GyroHits;
        public int SensorHits => Record.SensorHits;
        public int Byte78ProbableLifeSupportState => Record.Byte78ProbableLifeSupportState;
        public int PilotId => Record.PilotId;
        public int RiderId => Record.RiderId;
        public int Byte7BProbableUpgradePackageBase => Record.Byte7BProbableUpgradePackageBase;
        public int UpgradeLevelFlags => Record.UpgradeLevelFlags;

        public override string ToString() => $"{Name}|{Tonnage} Tons";        
        public string ToStats()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"Tonnage: {Tonnage}");
            sb.AppendLine($"Walk Move: {WalkMove}");
            sb.AppendLine($"Jump Move: {JumpMove}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Armour");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"R.Arm: {Armour[ArmourSlot.R_Arm]}");
            sb.AppendLine($"R.Leg: {Armour[ArmourSlot.R_Leg]}");
            sb.AppendLine($"R Torso: {Armour[ArmourSlot.R_Torso]}");
            sb.AppendLine($"Head: {Armour[ArmourSlot.Head]}");
            sb.AppendLine($"C Torso: {Armour[ArmourSlot.C_Torso]}");
            sb.AppendLine($"L Arm: {Armour[ArmourSlot.L_Arm]}");
            sb.AppendLine($"L Leg: {Armour[ArmourSlot.L_Leg]}");
            sb.AppendLine($"L Torso: {Armour[ArmourSlot.L_Torso]}");
            sb.AppendLine($"R Torso R: {Armour[ArmourSlot.R_Torso_R]}");
            sb.AppendLine($"C Torso R: {Armour[ArmourSlot.C_Torso_R]}");
            sb.AppendLine($"L Torso R: {Armour[ArmourSlot.L_Torso_R]}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Structure");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"R.Arm: {InternalStructure[CriticalSlot.R_Arm]}");
            sb.AppendLine($"R.Leg: {InternalStructure[CriticalSlot.R_Leg]}");
            sb.AppendLine($"R Torso: {InternalStructure[CriticalSlot.R_Torso]}");
            sb.AppendLine($"Head: {InternalStructure[CriticalSlot.Head]}");
            sb.AppendLine($"C Torso: {InternalStructure[CriticalSlot.C_Torso]}");
            sb.AppendLine($"L Arm: {InternalStructure[CriticalSlot.L_Arm]}");
            sb.AppendLine($"L Leg: {InternalStructure[CriticalSlot.L_Leg]}");
            sb.AppendLine($"L Torso: {InternalStructure[CriticalSlot.L_Torso]}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Actuators");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Byte 24 low/high: {ActuatorByte24.CurrentLowNibble:X1}/{ActuatorByte24.CurrentHighNibble:X1} " +
                $"(max {ActuatorByte24.MaximumLowNibble:X1}/{ActuatorByte24.MaximumHighNibble:X1})");
            sb.AppendLine($"Byte 25 low/high: {ActuatorByte25.CurrentLowNibble:X1}/{ActuatorByte25.CurrentHighNibble:X1} " +
                $"(max {ActuatorByte25.MaximumLowNibble:X1}/{ActuatorByte25.MaximumHighNibble:X1})");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Ammo");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Slot 0: {Ammo[0]}");
            sb.AppendLine($"Slot 1: {Ammo[1]}");
            sb.AppendLine($"Slot 2: {Ammo[2]}");
            sb.AppendLine($"Slot 3: {Ammo[3]}");
            sb.AppendLine($"Slot 4: {Ammo[4]}");
            sb.AppendLine($"Slot 5: {Ammo[5]}");
            sb.AppendLine($"Slot 6: {Ammo[6]}");
            sb.AppendLine($"Slot 7: {Ammo[7]}");
            sb.AppendLine($"Slot 8: {Ammo[8]}");
            sb.AppendLine($"Slot 9: {Ammo[9]}");     
            sb.AppendLine("--------------------------------");
            sb.AppendLine("Armament         LOC");

            CriticalComponent_Get(sb, CriticalSlot.L_Arm);
            CriticalComponent_Get(sb, CriticalSlot.L_Torso);
            CriticalComponent_Get(sb, CriticalSlot.R_Arm);
            CriticalComponent_Get(sb, CriticalSlot.R_Torso);
            CriticalComponent_Get(sb, CriticalSlot.L_Leg);
            CriticalComponent_Get(sb, CriticalSlot.R_Leg);
            CriticalComponent_Get(sb, CriticalSlot.C_Torso);
            CriticalComponent_Get(sb, CriticalSlot.Head);            

            return sb.ToString();
        }

        private StringBuilder CriticalComponent_Get(StringBuilder sb, CriticalSlot CS)
        {
            for (int i = 0; i < CriticalSlots[CS].Size; i++)
            {
                if ((MechComponent)CriticalSlots[CS].Slots[i] >= MechComponent.SmallLaser 
                 && (MechComponent)CriticalSlots[CS].Slots[i] != MechComponent.HeatSink)
                {
                    sb.AppendLine($"{(MechComponent)CriticalSlots[CS].Slots[i]}         {CS}");
                }
            }
            return sb;
        }
    }
}
