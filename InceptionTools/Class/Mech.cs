using InceptionTools.Class;
using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Data
{
    public class Mech
    {
        public Mech(byte[] RawData)
        {
            Name = Encoding.Default.GetString(RawData, 0, 15);
            Tonnage = RawData[16];
            Armour = new Dictionary<ArmourSlot, Armour>()
            {
                { ArmourSlot.R_Arm,     new Armour( RawData[17], RawData[86])},
                { ArmourSlot.R_Leg,     new Armour( RawData[18], RawData[87])},
                { ArmourSlot.R_Torso,   new Armour( RawData[19], RawData[88])},
                { ArmourSlot.Head,      new Armour( RawData[20], RawData[89])},
                { ArmourSlot.C_Torso,   new Armour( RawData[21], RawData[90])},
                { ArmourSlot.L_Arm,     new Armour( RawData[22], RawData[91])},
                { ArmourSlot.L_Leg,     new Armour( RawData[23], RawData[92])},
                { ArmourSlot.L_Torso,   new Armour( RawData[24], RawData[93])},
                { ArmourSlot.R_Torso_R, new Armour( RawData[25], RawData[94])},
                { ArmourSlot.C_Torso_R, new Armour( RawData[26], RawData[95])},
                { ArmourSlot.L_Torso_R, new Armour( RawData[27], RawData[96])},
            };
            InternalStructure = new Dictionary<CriticalSlot, InternalStructure>()
            {
                { CriticalSlot.R_Arm,     new InternalStructure( RawData[28], RawData[97])},
                { CriticalSlot.R_Leg,     new InternalStructure( RawData[29], RawData[98])},
                { CriticalSlot.R_Torso,   new InternalStructure( RawData[30], RawData[99])},
                { CriticalSlot.Head,      new InternalStructure( RawData[31], RawData[100])},
                { CriticalSlot.C_Torso,   new InternalStructure( RawData[32], RawData[101])},
                { CriticalSlot.L_Arm,     new InternalStructure( RawData[33], RawData[102])},
                { CriticalSlot.L_Leg,     new InternalStructure( RawData[34], RawData[103])},
                { CriticalSlot.L_Torso,   new InternalStructure( RawData[35], RawData[104])},
            };

            Actuators = new Dictionary<MechLimb, Actuator>()
            {
                { MechLimb.R_Arm, new Actuator( RawData[36], RawData[105])},
                { MechLimb.R_Leg, new Actuator( RawData[36], RawData[105])},       
                { MechLimb.L_Arm, new Actuator( RawData[37], RawData[106])},
                { MechLimb.L_Leg, new Actuator( RawData[37], RawData[106])},          
            };

            EngineHeatSinks = RawData[38];

            Ammo = new Dictionary<int, Ammo>()
            {
                { 0,  new Ammo( RawData[39], RawData[107])},
                { 1,  new Ammo( RawData[40], RawData[108])},
                { 2,  new Ammo( RawData[41], RawData[109])},
                { 3,  new Ammo( RawData[42], RawData[110])},
                { 4,  new Ammo( RawData[43], RawData[111])},
                { 5,  new Ammo( RawData[44], RawData[112])},
                { 6,  new Ammo( RawData[45], RawData[113])},
                { 7,  new Ammo( RawData[46], RawData[114])},
                { 8,  new Ammo( RawData[47], RawData[115])},
                { 9,  new Ammo( RawData[48], RawData[116])},
            };

            WalkMove = RawData[49];
            JumpMove = RawData[50];

            CriticalSlots = new Dictionary<CriticalSlot, Critical>()
            {
                { CriticalSlot.L_Arm,    new Critical(7, RawData[51..58])},
                { CriticalSlot.L_Torso,  new Critical(7, RawData[58..65])},
                { CriticalSlot.R_Arm,    new Critical(7, RawData[65..72])},
                { CriticalSlot.R_Torso,  new Critical(7, RawData[72..79])},
                { CriticalSlot.L_Leg,    new Critical(2, RawData[79..81])},
                { CriticalSlot.R_Leg,    new Critical(2, RawData[81..83])},
                { CriticalSlot.C_Torso,  new Critical(2, RawData[83..85])},
                { CriticalSlot.Head,     new Critical(1, new byte[] { RawData[85] })},
            };
        }

        public string Name { get; }
        public int Tonnage { get; }

        public Dictionary<ArmourSlot, Armour> Armour { get; }

        public Dictionary<CriticalSlot, InternalStructure> InternalStructure { get; }

        public Dictionary<CriticalSlot, Critical> CriticalSlots { get; }

        public Dictionary<MechLimb, Actuator> Actuators { get; }

        public Dictionary<int, Ammo> Ammo { get; }
        
        public int EngineHeatSinks { get; }

        public int WalkMove { get; }
        public int JumpMove { get; }

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
            sb.AppendLine($"R.Arm: {Actuators[MechLimb.R_Arm]}"); //OK or Gone
            sb.AppendLine($"R.Leg: {Actuators[MechLimb.R_Leg]}");           
            sb.AppendLine($"L Arm: {Actuators[MechLimb.L_Arm]}");
            sb.AppendLine($"L Leg: {Actuators[MechLimb.L_Leg]}");
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