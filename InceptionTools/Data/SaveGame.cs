using System;
using System.Collections.Generic;
using System.Text;

namespace InceptionTools.Data
{
    class SaveGame
    {
        //1 byte at start

        //8 Characters x 16-bytes
        //1 = Character name from list 00 Jason, 01 = Rex, 02 = Edward, Russ, Rick, Zeke, Possum, Marco, Rusty, Hunter and Hawk
        //B Stat = 0 - 10
        //D stat = 0 - 10
        //C stat = 0 - 10
        //Bows and Blade = 0 - 4
        //Pistol
        //Rifle
        //Gunnery
        //Piloting
        //Tech
        //Medical
        //Weapon owned by character
        // ???? = 07 is invisible
        //Armour Type
        //Armour Value
        //Current health

        //IMHex 000-05 to 0B = 00 to 04 as skills in order

        //IMHex 000-0C is weapon type for Jason
        //01 = Knife
        //02 = Sword
        //VibroBlade
        //shortbow
        //Longbow
        //Crossbow
        //Same weapon set as hacking guide

        //000-0D = 08 as character
        //07 makes invisable and walk through walls? - In mech?

        //IMHex 000-0E is Armour type for Jason
        //01 = Flak Vest [25]
        //02 = Flak Suit [40]
        //Lt Env Suit [30]
        //Hv Env Suit [50]
        //Ablative [50]
        //..... 06 = Completely Unskilled (other system value)

        //IMHex 000-OF is Armour Current value for Jason
        //010-00 is health

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