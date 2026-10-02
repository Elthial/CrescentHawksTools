using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Data.Mechs
{
    public class UrbanMech
    {
        public byte[] RawData =
        new byte[] {0x55, 0x52, 0x42, 0x41, 0x4E, 0x4D, 0x45, 0x43, 0x48, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x00, //URBANMECH - 0x00 - Null Terminated
                        0x1E, //Tonnage - 30
                        0x0A, //Armor - R. Arm
                        0x08, //Armor - R. Leg
                        0x0C, //Armor - R Torso
                        0x09, //Armor - Head
                        0x0B, //Armor - C Torso
                        0x0A, //Armor - L Arm
                        0x08, //Armor - L Leg
                        0x0C, //Armor - L Torso
                        0x04, //Armor - R Torso R
                        0x08, //Armor - C Torso R
                        0x04, //Armor - L Torso R
                        0x05, //IS - R. Arm     
                        0x07, //IS - R. Leg 
                        0x07, //IS - R Torso 
                        0x03, //IS - Head 
                        0x0A, //IS - C Torso
                        0x05, //IS - L Arm 
                        0x07, //IS - L Leg 
                        0x07, //IS - L Torso
                        0xCF, 0xCF, //2-byte actuator tags?, $CF - no hand, no LA, $FF all
                        0x02, // ? - # of heat sinks in engine
                        0xFF, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //Weapon Ammo - 10 Bytes
                        0x02, //Walk
                        0x02, //Jump
                        0x10, 0x22, 0x22, 0x00, 0x00, 0x00, 0x00, //LA-7 - $10 Small Laser -  $22 Heat Sink x 2
                        0x22, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, //LT-7 - $22 Heat Sink x 2
                        0x16, 0x22, 0x22, 0x00, 0x00, 0x00, 0x00, //RA-7 - $16 AC/10 - $22 Heat Sink x 2
                        0x22, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, //RT-7 - $22 Heat Sink x 2
                        0x00, 0x00, //LL2
                        0x00, 0x00, //RL2
                        0x00, 0x00, //CT2
                        0x00, //H1
                        0x0A, //MA - R. Arm
                        0x08, //MA - R. Leg
                        0x0C, //MA - R Torso
                        0x09, //MA - Head
                        0x0B, //MA - C Torso
                        0x0A, //MA - L Arm
                        0x08, //MA - L Leg
                        0x0C, //MA - L Torso
                        0x04, //MA - R Torso R
                        0x08, //MA - C Torso R
                        0x04, //MA - L Torso R
                        0x05, //Max. IS - R. Arm 
                        0x07, //Max. IS - R. Leg 
                        0x07, //Max. IS - R Torso 
                        0x03, //Max. IS - Head 
                        0x0A, //Max. IS - C Torso
                        0x05, //Max. IS - L Arm
                        0x07, //Max. IS - L Leg           
                        0x07, //Max. IS - L Torso
                        0xCF, 0xCF, //Maximum Actuators
                        0xFF, 0x14, 0xFF, 0xFF, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00,//Max Ammo - 10 Bytes
                        0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0x06, 0x00}; //MechId - URBANMECH

}
}
