using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Data.Mechs
{
    public class Jenner
    {
        //JENNER
        //TONS: 35

        //Armament LOC
        //Med Laser LA
        //Med Laser LA
        //Med Laser RA
        //Med Laser RA
        //SRMissile4 CT

        //Actuators:
        //Left Leg
        //Right Leg
        //Left Arm
        //Right Arm

        //Engine: 3
        //Gyro: 2
        //Sensor: 2
        //Heat Sinks: 10           

        public byte[] RawData =
        new byte[] {0x4A, 0x45, 0x4E, 0x4E, 0x45, 0x52, 0x20,  0x00, 0x20, 0x20,  0x20,  0x20,  0x20,  0x20, 0x20,  0x00, //JENNER - 0x00 - Null Terminated                       
                        0x23, //Tonage - 35
                        0x04, //Armor - R. Arm
                        0x08, //Armor - R. Leg
                        0x06, //Armor - R Torso
                        0x07, //Armor - Head
                        0x0A, //Armor - C Torso
                        0x04, //Armor - L Arm
                        0x08, //Armor - L Leg
                        0x06, //Armor - L Torso
                        0x04, //Armor - R Torso R
                        0x03, //Armor - C Torso R
                        0x04, //Armor - L Torso R
                        0x06, //IS - R. Arm     
                        0x08, //IS - R. Leg 
                        0x08, //IS - R Torso 
                        0x03, //IS - Head 
                        0x0B, //IS - C Torso
                        0x06, //IS - L Arm 
                        0x08, //IS - L Leg 
                        0x08, //IS - L Torso
                        0xCF, 0xCF, //2-byte actuator tags?, $CF - no hand, no LA, $FF all
                        0x0A, // ? - # of heat sinks in engine
                        0xFF, 0xFF, 0xFF, 0xFF, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, //Weapon Ammo - 10 Bytes
                        0x07, // Walk
                        0x05, // Jump
                        0x11, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, //LA-7 - $11 Medium Laser x 2
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //LT-7
                        0x11, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, //RA-7 - $11 Medium Laser x 2
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //RT-7
                        0x00, 0x00, //LL2
                        0x00, 0x00, //RL2
                        0x1F, 0x00, //CT2 - $1F SRM 4
                        0x00, //H1
                        0x04, //MA - R. Arm
                        0x08, //MA - R. Leg
                        0x06, //MA - R Torso
                        0x07, //MA - Head
                        0x0A, //MA - C Torso
                        0x04, //MA - L Arm
                        0x08, //MA - L Leg
                        0x06, //MA - L Torso
                        0x04, //MA - R Torso R
                        0x03, //MA - C Torso R
                        0x04, //MA - L Torso R
                        0x06, //Max. IS - R. Arm 
                        0x08, //Max. IS - R. Leg 
                        0x08, //Max. IS - R Torso 
                        0x03, //Max. IS - Head 
                        0x0B, //Max. IS - C Torso
                        0x06, //Max. IS - L Arm
                        0x08, //Max. IS - L Leg           
                        0x08, //Max. IS - L Torso
                        0xCF, 0xCF, //Maximum Actuators
                        0xFF, 0xFF, 0xFF, 0xFF, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, //Max Ammo - 10 Bytes
                        0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0x09, 0x00 }; //JENNER
    }
}
