using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InceptionTools
{
    class MAP : MapFile
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MAP(string FilePath, MapFormat MapEncoding, List<byte[]> Tileset) : base(FilePath, MapEncoding, Tileset)
        { 
            Contents = File.ReadAllBytes(FileLocation);
            ContentsIndex = 0;

            log.Debug($"Loading File: {Path.GetFileName(FilePath)}");
            log.Debug($"FileSize {Size}");
               
            Header1 = ReadContents(1).First(); //3092:5F9C, 0x01 
            Header2 = ReadContents(1).First(); //3092:5F98, 0x01 
            Header3 = ReadContents(1).First(); //3092:5F94, 0x01  
            MapSizeX = ReadContents(1).First(); //3092:5F9E, 0x01
            MapSizeY = ReadContents(1).First(); //3092:5F96, 0x01 //5
            RawNPCNames = System.Text.Encoding.Default.GetString(ReadContents(0x80));   // 246C: A461, 0x80 //133
            RawBuildingNames = ReadContents(0x100); //246C: A561, 0x100 //389
            Variable8 = ReadContents(0x20);    // 3092:4564, 0x20 /421
            Variable9 = ReadContents(0x20);    //3092:4596, 0x20 //453
            Variable10 = ReadContents(0x20);   // 3092:39B4, 0x20 //485
            Variable11 = ReadContents(0x20);   //3092:39D4, 0x20 //517
            Variable12 = ReadContents(0x10);   //0x3092:4602, 0x10 //533
            Variable13 = ReadContents(0x08);   //3092:3768, 0x08 //541
                        
            var Namearr = RawNPCNames.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

            for(int i = 0; i < 8; i++)
            {
                NPCNames.Add(Namearr[i]);
            }

            var RawBuildingNamesArr = System.Text.Encoding.Default.GetString(RawBuildingNames).Split(new char[] { '\0' }, StringSplitOptions.None);

            foreach (string s in RawBuildingNamesArr)
            {
                if (s.StartsWith("MAP"))
                    break;

                BuildingNames.Add(s);
            }

            
            var str3 = System.Text.Encoding.Default.GetString(Variable8);

            


            log.Debug($"FileSize {Size}");
            log.Debug($"FileSize {ContentsIndex}");
            //MAPS 3 - 10 don't have the 0x1000 (4,096 bytes) to read specified by the original code.
            //DOS must have handled it more gracefully or read junk?
            var RemainingFileBytes = Size - ContentsIndex;
            //Keep RawMap arround until we've fully deciphered the MAP format in case we need it for ref
            RAWMap = ReadContents(RemainingFileBytes); //246C: 101D, 0x1000               
            MapData = MapFormat.Equals(MapFormat.BlockFormat) ? RemapBlockFormat(RAWMap) : RemapLinearFormat(RAWMap);    

            log.Debug("Map Load Successful");

            if (!Extension.Equals(".MTP"))
            {
                throw new ArgumentException("Image file extensions must be .MTP");
            }
        }

        List<string> NPCNames = new List<string>();
        List<string> BuildingNames = new List<string>();

        private byte Header1; //Always Zero.
        private byte Header2; //Always Zero.
        private byte Header3; //Always Zero. 
        private string RawNPCNames;
        private byte[] RawBuildingNames;
        public byte[] Variable8 { get; }
        public byte[] Variable9 { get; }
        public byte[] Variable10 { get; }
        public byte[] Variable11 { get; }
        public byte[] Variable12 { get; }
        public byte[] Variable13 { get; }
    }
}