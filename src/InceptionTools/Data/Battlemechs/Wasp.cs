using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Data.Mechs
{
    public class Wasp
    {
        //WASP
        //TONS: 20

        //Armament LOC
        //Med Laser RA
        //SRMissile2 LL - 50 (Ammo in Hex)

        //Engine: 3
        //Gyro: 2
        //Sensiors: 2

        //HeatSinks: 10

        public byte[] RawData = 
            new byte[] {0x57, 0x41, 0x53, 0x50, 0x20, 0x20, 0x20, 0x20, 0x20, 0x00, //WASP - 0x00 - Null Terminated
                        0x14, //Tonnage - 20                      
                        0x04, //Armor - R. Arm
                        0x06, //Armor - R. Leg
                        0x05, //Armor - R Torso
                        0x04, //Armor - Head
                        0x06, //Armor - C Torso
                        0x04, //Armor - L Arm
                        0x06, //Armor - L Leg
                        0x05, //Armor - L Torso
                        0x02, //Armor - R Torso R
                        0x04, //Armor - C Torso R
                        0x02, //Armor - L Torso R
                        0x03, //IS - R. Arm     
                        0x05, //IS - R. Leg 
                        0x04, //IS - R Torso 
                        0x03, //IS - Head 
                        0x06, //IS - C Torso
                        0x03, //IS - L Arm 
                        0x05, //IS - L Leg 
                        0x04, //IS - L Torso
                        0xFF, 0xFF, //2-byte actuator tags?, $CF - no hand, no LA, $FF all
                        0x04, // ? - # of heat sinks in engine
                        0xFF, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  //Weapon Ammo - 10 Bytes, FF = Ammo Full, 0x32 = 50 SRM
                        0x06, //Walk move
                        0x06, //Jump move
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //LA-7
                        0x22, 0x22, 0x22, 0x00, 0x00, 0x00, 0x00, //LT-7 - $22 Heat Sink x 3
                        0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //RA-7 - $11 Medium Laser
                        0x22, 0x22, 0x22, 0x00, 0x00, 0x00, 0x00, //RT-7 - $22 Heat Sink x 3
                        0x1E, 0x00, //LL-2 - $1E SRM 2
                        0x00, 0x00, //RL-2
                        0x00, 0x00, //CT-2
                        0x00, //H-1
                        0x04, //MA - R. Arm
                        0x06, //MA - R. Leg
                        0x05, //MA - R Torso
                        0x04, //MA - Head
                        0x06, //MA - C Torso
                        0x04, //MA - L Arm
                        0x06, //MA - L Leg
                        0x05, //MA - L Torso
                        0x02, //MA - R Torso R
                        0x04, //MA - C Torso R
                        0x02, //MA - L Torso R
                        0x03, //Max. IS - R. Arm 
                        0x05, //Max. IS - R. Leg 
                        0x04, //Max. IS - R Torso 
                        0x03, //Max. IS - Head 
                        0x06, //Max. IS - C Torso
                        0x03, //Max. IS - L Arm
                        0x05, //Max. IS - L Leg           
                        0x04, //Max. IS - L Torso
                        0xFF, 0xFF, //Maximum Actuators
                        0xFF, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //Max Ammo - 10 Bytes
                        0x00, 0x00, 0x00, 0x01, 0x00, 0xFF, 0x01, 0x00 }; //MechId - WASP 
    }
}
