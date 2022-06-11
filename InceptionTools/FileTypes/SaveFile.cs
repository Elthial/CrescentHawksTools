using InceptionTools.Class;
using InceptionTools.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InceptionTools.FileTypes
{
    class SaveFile : AssetFile
    {
        public SaveFile(string FilePath)
        {
            //File Metadata
            FileLocation = FilePath;
            Name = Path.GetFileNameWithoutExtension(FilePath);
            Extension = Path.GetExtension(FilePath);
            Size = (int)new FileInfo(FilePath).Length;

            //Contents
            Contents = File.ReadAllBytes(FileLocation);
            ContentsIndex = 0;

            Header1 = ReadContents(1).First(); 
            CharacterSlot01 = new Infantry(ReadContents(17)); 
            CharacterSlot02 = new Infantry(ReadContents(17));  
            CharacterSlot03 = new Infantry(ReadContents(17));   
            CharacterSlot04 = new Infantry(ReadContents(17));   
            CharacterSlot05 = new Infantry(ReadContents(17));  
            CharacterSlot06 = new Infantry(ReadContents(17));  
            CharacterSlot07 = new Infantry(ReadContents(17)); 
            CharacterSlot08 = new Infantry(ReadContents(17));
            CharacterSlot09 = new Infantry(ReadContents(17));
            CharacterSlot10 = new Infantry(ReadContents(17));
            CharacterSlot11 = new Infantry(ReadContents(17));
            CharacterSlot12 = new Infantry(ReadContents(17));
            CharacterSlot13 = new Infantry(ReadContents(17));
            CharacterSlot14 = new Infantry(ReadContents(17));
            CharacterSlot15 = new Infantry(ReadContents(17));
            CharacterSlot16 = new Infantry(ReadContents(17)); //273

            LanceSlot01 = new Mech(ReadContents(125));
            LanceSlot02 = new Mech(ReadContents(125));
            LanceSlot03 = new Mech(ReadContents(125));
            LanceSlot04 = new Mech(ReadContents(125));

            EnemySlot01 = new Mech(ReadContents(125));
            EnemySlot02 = new Mech(ReadContents(125));
            EnemySlot03 = new Mech(ReadContents(125));
            EnemySlot04 = new Mech(ReadContents(125)); //1273

            MapVisibility = ReadContents(2048);
            CitadelMission = ReadContents(1).First();
            Unknown01 = ReadContents(5); //O8 On foot, 07 Piloting mech?
            CitadelKatrinaVisit = ReadContents(1).First();

            Unknown02 = ReadContents(90);


            C_Bills = ReadContents(4);
            Stock_DasHas = ReadContents(4);
            Stock_Nasdiv = ReadContents(4);
            Stock_BakPhar = ReadContents(4);

            Unknown03 = ReadContents(19);
     
            Array01 = ReadContents(26);
            Array02 = ReadContents(26);
            Array03 = ReadContents(26);
            Array04 = ReadContents(26);
            Array05 = ReadContents(26);
            Array06 = ReadContents(26);
            Array07 = ReadContents(26);
            Array08 = ReadContents(17);

            Block01 = ReadContents(64);
            Block02 = ReadContents(64);
            Block03 = ReadContents(64);
            Block04 = ReadContents(64);

            Position = ReadContents(4);

            if (!string.IsNullOrEmpty(Extension))
            {
                throw new ArgumentException("Save file extensions must be blank");
            }
        }
        public override string Name { get; }
        public override string FileLocation { get; }
        public override string Extension { get; }
        public override int Size { get; }

        protected byte[] Contents;
        protected int ContentsIndex;

        public byte Header1; //Always Zero.

        public Infantry CharacterSlot01 { get; }
        public Infantry CharacterSlot02 { get; }
        public Infantry CharacterSlot03 { get; }
        public Infantry CharacterSlot04 { get; }
        public Infantry CharacterSlot05 { get; }
        public Infantry CharacterSlot06 { get; }
        public Infantry CharacterSlot07 { get; }
        public Infantry CharacterSlot08 { get; }
        public Infantry CharacterSlot09 { get; }
        public Infantry CharacterSlot10 { get; }
        public Infantry CharacterSlot11 { get; }
        public Infantry CharacterSlot12 { get; }
        public Infantry CharacterSlot13 { get; }
        public Infantry CharacterSlot14 { get; }
        public Infantry CharacterSlot15 { get; }
        public Infantry CharacterSlot16 { get; }

        public Mech LanceSlot01 { get; }
        public Mech LanceSlot02 { get; }
        public Mech LanceSlot03 { get; }
        public Mech LanceSlot04 { get; }

        public Mech EnemySlot01 { get; }
        public Mech EnemySlot02 { get; }
        public Mech EnemySlot03 { get; }
        public Mech EnemySlot04 { get; }

        public byte[] MapVisibility { get; }

        //Flags
        public int CitadelMission { get; }

        public int CitadelKatrinaVisit { get; }

        public byte[] Unknown01 { get; }
        public byte[] Unknown02 { get; }


        public byte[] C_Bills { get; }
        public byte[] Stock_DasHas { get; }
        public byte[] Stock_Nasdiv { get; }
        public byte[] Stock_BakPhar { get; }

        public byte[]  Unknown03 { get; }

        public byte[] Array01 { get; }
        public byte[] Array02 { get; }
        public byte[] Array03 { get; }
        public byte[] Array04 { get; }
        public byte[] Array05 { get; }
        public byte[] Array06 { get; }
        public byte[] Array07 { get; }
        public byte[] Array08 { get; }


        public byte[] Block01 { get; }
        public byte[] Block02 { get; }
        public byte[] Block03 { get; }
        public byte[] Block04 { get; }

        //D60-01 is first stock, 4 bytes for each, all 3 in sequence

        //3321 - Mission Flag

        //Find mission flag
        //Find citadel destruction flag
        //Katrina flag
        //Lounge Flag
        //Barracks Flag
        //Clothes Flag
        //Mapper Flag

        //Find out how pilots are assigned
        //Find out how riders are assigned

        //Mech wrecks should be recorded / find location

        //Human 09-16 are enemies?

        public byte[] Position { get; }

        protected byte[] ReadContents(int BytesToRead)
        {
            var Output = new byte[BytesToRead];

            for (int i = 0; i < BytesToRead; i++)
            {
                Output[i] = Contents[ContentsIndex];
                ContentsIndex++;
            }

            return Output;
        }
    }
}
