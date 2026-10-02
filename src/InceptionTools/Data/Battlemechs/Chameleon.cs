using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Data.Mechs
{
    public class Chameleon
    {
        //CHAMELEO
        //TONS: 50
        //Pilot: 
        //Rider: 

        //Armament LOC
        //Med Laser LA
        //SmallLaser LT
        //SmallLaser LT
        //LargeLaser RA
        //Med Laser RA
        //SmallLaser RT
        //SmallLaser RT
        //MachineGun CT
        //MachineGun CT

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
        new byte[] {0x43, 0x48, 0x41, 0x4D, 0x45, 0x4C, 0x45, 0x4F, 0x4E, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x00, //CHAMELEON - 0x00 - Null Terminated
                        0x32, //Tonnage - 50
                        0x0A, //Armor - R. Arm
                        0x0A, //Armor - R. Leg
                        0x0C, //Armor - R Torso
                        0x09, //Armor - Head
                        0x10, //Armor - C Torso
                        0x0A, //Armor - L Arm
                        0x0A, //Armor - L Leg
                        0x0C, //Armor - L Torso
                        0x02, //Armor - R Torso R
                        0x03, //Armor - C Torso R
                        0x02, //Armor - L Torso R
                        0x00, //IS - R. Arm     
                        0x00, //IS - R. Leg 
                        0x00, //IS - R Torso 
                        0x01, //IS - Head 
                        0x01, //IS - C Torso
                        0x00, //IS - L Arm 
                        0x00, //IS - L Leg 
                        0x00, //IS - L Torso
                        0xFF, 0xFF, //2-byte actuator tags?, $CF - no hand, no LA, $FF all
                        0x00, // ? - # of heat sinks in engine
                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x64, 0x64, 0x00, //Weapon Ammo - 10 Bytes
                        0x06, // Walk
                        0x06, // jump points
                        0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //LA-7
                        0x10, 0x10, 0x22, 0x22, 0x22, 0x00, 0x00, //LT-7 - $10 Small Laser x2 - $22 Heat Sink x 3
                        0x12, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, //RA-7 - $12 Large Laser - $11 Medium Laser
                        0x10, 0x10, 0x22, 0x22, 0x22, 0x00, 0x00, //RT-7 - $10 Small Laser x 2 -  $22 Heat Sink x 3
                        0x22, 0x22, //LL2 - $22 Heat Sink x 2
                        0x22, 0x22, //RL2 - $22 Heat Sink x 2
                        0x18, 0x18, //CT2 - $18 Machine Gun x 2
                        0x00, //H1
                        0x0A, //MA - R. Arm
                        0x0A, //MA - R. Leg
                        0x0C, //MA - R Torso
                        0x09, //MA - Head
                        0x10, //MA - C Torso
                        0x0A, //MA - L Arm
                        0x0A, //MA - L Leg
                        0x0C, //MA - L Torso
                        0x02, //MA - R Torso R
                        0x03, //MA - C Torso R
                        0x02, //MA - L Torso R
                        0x00, //Max. IS - R. Arm 
                        0x00, //Max. IS - R. Leg 
                        0x00, //Max. IS - R Torso 
                        0x01, //Max. IS - Head 
                        0x01, //Max. IS - C Torso
                        0x00, //Max. IS - L Arm
                        0x00, //Max. IS - L Leg           
                        0x00, //Max. IS - L Torso
                        0xFF, 0xFF, //Maximum Actuators
                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x64, 0x64, 0x00, //Max Ammo - 10 Bytes
                        0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0xC8, 0x00 }; //MechId - CHAMELEON 
    }
}
