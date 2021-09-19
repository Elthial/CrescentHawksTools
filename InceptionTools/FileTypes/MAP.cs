using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InceptionTools
{
    class MAP
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MAP(string FilePath, List<byte[]> Tileset)
        {            
            FileLocation = FilePath;
            Name = Path.GetFileNameWithoutExtension(FilePath);
            Extension = Path.GetExtension(FilePath);
            Size = (int)new FileInfo(FilePath).Length;
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
            RAWMap = ReadContents(0x1000); //246C: 101D, 0x1000

            //Keep RawMap arround until we've fully deciphered the MAP format in case we need it for ref
            MapData = Remap(RAWMap);

            MapTileset = Tileset;

            log.Debug("Map Load Successful");

            if (!Extension.Equals(".MTP"))
            {
                throw new ArgumentException("Image file extensions must be .MTP");
            }
        }

        

        public string Name { get; }
        public string FileLocation { get; }
        public string Extension { get; }

        public int Size { get; }
        private byte[] Contents;
        private int ContentsIndex;
        
        public List<byte[]> MapTileset { get; }
       
        public byte Variable1 { get; }
        public byte Variable2 { get; }
        public byte Variable3 { get; }
        public int MapSizeX { get; }
        public int MapSizeY { get; }
        public byte[] Variable6 { get; }
        public byte[] Variable7 { get; }
        public byte[] Variable8 { get; }
        public byte[] Variable9 { get; }
        public byte[] Variable10 { get; }
        public byte[] Variable11 { get; }
        public byte[] Variable12 { get; }
        public byte[] Variable13 { get; }
        public byte[] RAWMap { get; }
        public byte[] MapData { get; }

        private byte[] ReadContents(int BytesToRead)
        {
            var Output = new byte[BytesToRead];


            for (int i = 0; i < BytesToRead; i++)
            {
                Output[i] = (ContentsIndex < Size) ? Contents[ContentsIndex] : (byte)0x00;  
                ContentsIndex++;
            }

            return Output;
        }

        private byte[] Remap(byte[] RAWmap)
        {
            //-------------------------------------------
            // Remap map
            //--------------------------------------------
            // 64 byte object per line
            // 8 x 8 | 8 x 8 | 8 x 8 |
            // Remap into standard linear X * Y map

            log.Debug($"Remapping {Name}");
            log.Debug($"{MapSizeX} * {MapSizeY}");
            log.Debug($"Total size: {RAWmap.Length}");

            var remapped = new byte[RAWmap.Length];

            var MapIndex = 0;
            var BlockSizeX = 8;
            var BlockSizeY = 8;
            var MapBlocksX = MapSizeX / BlockSizeX;
            var MapBlocksY = MapSizeY / BlockSizeY;

            for (int BlockY = 0; BlockY < MapBlocksY; BlockY++)
            {
                for (int BlockX = 0; BlockX < MapBlocksX; BlockX++)
                {
                    for (int TileY = 0; TileY < BlockSizeY; TileY++)
                    {
                        for (int TileX = 0; TileX < BlockSizeX; TileX++)
                        {
                            var TilePerRow = BlockSizeX * MapBlocksX;
                            var OffSetX = (BlockX * BlockSizeX);
                            var OffsetY = ((BlockY * BlockSizeY) + TileY) * TilePerRow;
                            var TotalOffset = OffsetY + OffSetX + TileX;

                            remapped[MapIndex] = RAWmap[TotalOffset];
                            MapIndex++;
                        }
                    }
                }
            }

            log.Debug($"Completed Remapping {Name}");
            return remapped;
        }
    }
}