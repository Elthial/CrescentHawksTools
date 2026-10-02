using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Data.Mechs
{
    public class Spectator
    {
        public byte[] RawData = 
        new byte[] {0x73, 0x70, 0x65, 0x63, 0x74, 0x61, 0x74, 0x6F, 0x72, 0x00, 0x20, 0x20, 0x20, 0x20, 0x20, 0x00, //Spectator - 0x00 - Null Terminated
                        0x14, //Tonnage - 20
                        0x00, //Armor - R. Arm
                        0x00, //Armor - R. Leg
                        0x00, //Armor - R Torso
                        0x01, //Armor - Head
                        0x01, //Armor - C Torso
                        0x00, //Armor - L Arm
                        0x00, //Armor - L Leg
                        0x00, //Armor - L Torso
                        0x01, //Armor - R Torso R
                        0x01, //Armor - C Torso R
                        0x01, //Armor - L Torso R
                        0x00, //IS - R. Arm     
                        0x01, //IS - R. Leg 
                        0x00, //IS - R Torso 
                        0x01, //IS - Head 
                        0x01, //IS - C Torso
                        0x00, //IS - L Arm 
                        0x01, //IS - L Leg 
                        0x00, //IS - L Torso              
                        0xCF, 0xCF, //2-byte actuator tags?, $CF - no hand, no LA, $FF all
                        0x06, // ? - # of heat sinks in engine
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //Weapon Ammo - 10 Bytes
                        0x00, //Walk
                        0x00, //Jump
                        0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //LA-7 - $18 Machine Gun
                        0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //LT-7 - $22 Heat Sink
                        0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //RA-7 - $18 Machine Gun
                        0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //RT-7 - $22 Heat Sink
                        0x22, 0x00, //LL2 - $22 Heat Sink
                        0x22, 0x00, //RL2 - $22 Heat Sink
                        0x11, 0x00, //CT2 - $11 Medium Laser
                        0x00, //H1
                        0x04, //MA - R. Arm
                        0x08, //MA - R. Leg
                        0x08, //MA - R Torso
                        0x08, //MA - Head
                        0x0A, //MA - C Torso
                        0x04, //MA - L Arm
                        0x08, //MA - L Leg
                        0x08, //MA - L Torso
                        0x02, //MA - R Torso R
                        0x02, //MA - C Torso R
                        0x02, //MA - L Torso R
                        0x03, //Max. IS - R. Arm 
                        0x05, //Max. IS - R. Leg 
                        0x04, //Max. IS - R Torso 
                        0x03, //Max. IS - Head 
                        0x06, //Max. IS - C Torso
                        0x03, //Max. IS - L Arm
                        0x05, //Max. IS - L Leg           
                        0x04, //Max. IS - L Torso
                        0xCF, 0xCF, //Maximum Actuators
                        0x64, 0x64, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00,  0x00, 0x00, //Max Ammo - 10 Bytes
                        0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0x00, 0x00 }; //MechId - Spectator

    }
}
