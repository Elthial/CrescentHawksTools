using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Data.Mechs
{
    public class Locust
    {
        //LOCUST
        //Tons: 20

        //Armament LOC
        //MachineGun LA
        //MachineGun RA
        //Med Laser CT

        //Engine: 3
        //Gyro: 2
        //Sensors: 2

        //HeatSinks: 10

        public byte[] RawData =
            new byte[] {0x4C, 0x4F, 0x43, 0x55, 0x53, 0x54, 0x20, 0x20, 0x20, 0x20, 0x20, 0x00, //LOCUST 16 bytes - 0x00 - Null Terminated
                        0x14, //Tonnage - 20                            
                        0x04, //Armor - R. Arm
                        0x08, //Armor - R. Leg
                        0x08, //Armor - R Torso
                        0x08, //Armor - Head
                        0x0A, //Armor - C Torso
                        0x04, //Armor - L Arm
                        0x08, //Armor - L Leg
                        0x08, //Armor - L Torso
                        0x02, //Armor - R Torso R
                        0x02, //Armor - C Torso R
                        0x02, //Armor - L Torso R
                        0x03, //IS - R. Arm     
                        0x05, //IS - R. Leg 
                        0x04, //IS - R Torso 
                        0x03, //IS - Head 
                        0x06, //IS - C Torso
                        0x03, //IS - L Arm 
                        0x05, //IS - L Leg 
                        0x04, //IS - L Torso
                        0xCF, 0xCF, //2-byte actuator tags?, $CF - no hand, no LA, $FF all
                        0x06, // ? - # of heat sinks in engine
                        0x64, 0x64, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //Weapon Ammo - 10 Bytes
                        0x08, //Walk move
                        0x00, //Jump move - N/A
                        0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //LA-7 - $18 Machine Gun
                        0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //LT-7 - $22 Heatsink
                        0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //RA-7 - $18 Machine Gun
                        0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //RT-7 - $22 Heatsink
                        0x22, 0x00, //LL-2 - $22 Heatsink
                        0x22, 0x00, //RL-2 - $22 Heatsink
                        0x11, 0x00, //CT-2 - $11 Medium Laser
                        0x00, //H-1
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
                        0x64, 0x64, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,//Max Ammo - 10 Bytes
                        0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0x00, 0x00 };//MechId - LOCUST 
        
    }
}
