using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Data.Mechs
{
    public class Commando
    {
        //COMMANDO
        //TONS: 25

        //Armament LOC #2
        //Med Laser LA
        //SRMissile4 RA
        //SRMissile6 RT

        //Armament LOC #2
        //Med Laser LA
        //Med Laser RA
        //Med Laser RT
        //SmallLaser LL
        //SmallLaser LL
        //SmallLaser RL
        //SmallLaser RL
        //SmallLaser CT
        //SmallLaser CT
        //Med Laser H

        //Engine: 3
        //Gyeo: 2
        //Sensiors: 2

        //HeatSinks: 10

        public byte[] RawData =
        new byte[] {0x43, 0x4F, 0x4D, 0x4D, 0x41, 0x4E, 0x44, 0x4F, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x00, //COMMANDO - 0x00 - Null Terminated
                        0x19, //Tonnage - 25
                        0x06, //Armor - R. Arm
                        0x06, //Armor - R. Leg
                        0x08, //Armor - R Torso
                        0x06, //Armor - Head
                        0x08, //Armor - C Torso
                        0x06, //Armor - L Arm
                        0x06, //Armor - L Leg
                        0x08, //Armor - L Torso
                        0x03, //Armor - R Torso R
                        0x04, //Armor - C Torso R
                        0x03, //Armor - L Torso R
                        0x04, //IS - R. Arm     
                        0x06, //IS - R. Leg 
                        0x06, //IS - R Torso 
                        0x03, //IS - Head 
                        0x08, //IS - C Torso
                        0x04, //IS - L Arm 
                        0x06, //IS - L Leg 
                        0x06, //IS - L Torso
                        0xFF, 0xFF, //2-byte actuator tags?, $CF - no hand, no LA, $FF all. Legs and Arms
                        0x06, // ? - # of heat sinks in engine
                        0xFF, 0x19, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //Weapon Ammo - 10 Bytes
                        0x06, //Walk Move
                        0x00, //Jump Move
                        0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //LA7 - $11 Medium Laser
                        0x22, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, //LT7 - $22 Heat Sink x 2
                        0x1F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //RA7 - $1F SRM 4
                        0x22, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, //RT7 - $22 Heat Sink x 2
                        0x00, 0x00, //LL2
                        0x00, 0x00, //RL2 
                        0x20, 0x00, //CT2 - $20 SRM 6
                        0x00, //H1
                        0x06, //MA - R. Arm
                        0x06, //MA - R. Leg
                        0x08, //MA - R Torso
                        0x06, //MA - Head
                        0x08, //MA - C Torso
                        0x06, //MA - L Arm
                        0x06, //MA - L Leg
                        0x08, //MA - L Torso
                        0x03, //MA - R Torso R
                        0x04, //MA - C Torso R
                        0x03, //MA - L Torso R
                        0x04, //Max. IS - R. Arm 
                        0x06, //Max. IS - R. Leg 
                        0x06, //Max. IS - R Torso 
                        0x03, //Max. IS - Head 
                        0x08, //Max. IS - C Torso
                        0x04, //Max. IS - L Arm
                        0x06, //Max. IS - L Leg                       
                        0x06, //Max. IS - L Torso
                        0xFF, 0xFF, //Maximum Actuators
                        0xFF, 0x19, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,//Max Ammo - 10 Bytes
                        0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0x03, 0x00 }; //MechId - COMMANDO
    }
}
