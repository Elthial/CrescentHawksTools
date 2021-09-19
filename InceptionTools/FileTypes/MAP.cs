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
               
            Variable1 = ReadContents(1).First(); //3092:5F9C, 0x01
            Variable2 = ReadContents(1).First(); //3092:5F98, 0x01
            Variable3 = ReadContents(1).First(); //3092:5F94, 0x01   
            MapSizeX = ReadContents(1).First(); //3092:5F9E, 0x01
            MapSizeY = ReadContents(1).First(); //3092:5F96, 0x01
            Variable6 = ReadContents(0x80);   // 246C: A461, 0x80
            Variable7 = ReadContents(0x100); //246C: A561, 0x100
            Variable8 = ReadContents(0x20);    // 3092:4564, 0x20
            Variable9 = ReadContents(0x20);    //3092:4596, 0x20
            Variable10 = ReadContents(0x20);   // 3092:39B4, 0x20
            Variable11 = ReadContents(0x20);   //3092:39D4, 0x20
            Variable12 = ReadContents(0x10);   //0x3092:4602, 0x10
            Variable13 = ReadContents(0x08);   //3092:3768, 0x08

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
       
        public byte Variable1 { get; }
        public byte Variable2 { get; }
        public byte Variable3 { get; }  
        public byte[] Variable6 { get; }
        public byte[] Variable7 { get; }
        public byte[] Variable8 { get; }
        public byte[] Variable9 { get; }
        public byte[] Variable10 { get; }
        public byte[] Variable11 { get; }
        public byte[] Variable12 { get; }
        public byte[] Variable13 { get; }
    }
}