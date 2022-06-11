using System;
using System.Collections.Generic;
using System.Text;

// Many thanks to Jonathan Schattke who provided a lot of this information at http://lethe97.fortunecity.ws
namespace InceptionTools.Data
{
    class SaveGame
    {
        //IMHex D50-0D is 4-bytes = C-bill value
        //Last 4-bytes are character position


        //500 to CF0 is explored map areas //2042 bytes

        //E30 - 0D = First Aid
        //E30 - 0E = Field Medical Kit
    }
}
//246C:0B80 -< little map?

//O0 - Mech start up button
//O1 - Manpad vs Mech
//O2 - Neuro helmet talk
//03 - getting shit in cockpit
//04 - Locust fire
//07 - Mech leveling gun and firing
//O6 = Crescent hawks secret card
//O8 - Siren anim
//O9 - Mech step on
//O10
//O11
//O12
//O13
//O14
//015 - Steiner doors opening
//O16 = Arm blown off Wasp
//017 - Citadel secutary and banner
//O18 - MechTechs shop
//O19 - Draconis Hall
//O20 - Arena Poster
//O21 - Weaponshop owner

//3092:C164 is EXE version of SAVEGAME data

/* 
 * 
 * struct Finance {
	u32 CBills, Defhas, Nasdiv, Bakphar;
	};
	
struct MechData {
		char Name[16];
		u8 Tonnage;
		u8 CurrentArmour[11];
		u8 CurrentStructure[8];
		u8 CurrentActuators[4];
		u8 EngineHeatsinks;
		u8 CurrentAmmo[10];
		u8 WalkMove;
		u8 JumpMove;
		u8 Critical_L_Arm[7];
		u8 Critical_L_Torso[7];
		u8 Critical_R_Arm[7];
		u8 Critical_R_Torso[7];
		u8 Critical_L_Leg[2];
		u8 Critical_R_Leg[2];
		u8 Critical_C_Torso[2];
		u8 Critical_Head;
		u8 Maxrmour[11];
		u8 MaxStructure[8];
		u8 MaxActuators[4];
		u8 MaxAmmo[10];
		u8 Unknown[4];
	};
	
struct Skills {
	u8 BowsAndBlade;
	u8 Pistol;
	u8 Rifle;
	u8 Gunnery;
	u8 Piloting;
	u8 Tech;
	u8 Medical;	
};
	
struct Infantry {
		u8 Character;
		u8 Body;
		u8 Dexterity;
		u8 Charisma;
		Skills skill;
		u8 Weapon;
		u8 Unknown;
		u8 ArmourType;
		u8 ArmourValue;
		u8 Health;
		u8 Unknown2;
};

Infantry Party01 @ 0x0001;
Infantry Party02 @ 0x0012;
Infantry Party03 @ 0x0023;
Infantry Party04 @ 0x0034;
Infantry Party05 @ 0x0045;
Infantry Party06 @ 0x0056;
Infantry Party07 @ 0x0067;
Infantry Party08 @ 0x0078;

Infantry Enemy01 @ 0x0089;
Infantry Enemy02 @ 0x009A;
Infantry Enemy03 @ 0x00AB;
Infantry Enemy04 @ 0x00BC;
Infantry Enemy05 @ 0x00CD;
Infantry Enemy06 @ 0x00DE;
Infantry Enemy07 @ 0x00EF;
Infantry Enemy08 @ 0x0100;
	
	
u8 Header @0x000;	
	
MechData Lance01 @ 0x0111;
MechData Lance02 @ 0x018E;
MechData Lance03 @ 0x020B;
MechData Lance04 @ 0x0288;
	

	MechData EnemyMech01 @ 0x0305;
	MechData EnemyMech02 @ 0x0382;
	MechData EnemyMech03 @ 0x03FF;
	MechData EnemyMech04 @ 0x047C;

	u8 MapVisibility[2048] @ 0x04F9;	

Finance money @ 0x0D5D;


//Flags
u8 CitadelMissionFlag @ 0x0CF9;


u16 PartyMapPositionX @0x0F45;
u16 PartyMapPositionY @0x0F47;


*/