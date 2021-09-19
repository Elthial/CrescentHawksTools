using System.Collections.Generic;
using System.IO;

namespace InceptionTools
{
    class MapFile : AssetFile
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MapFile(string FilePath, MapFormat MapEncoding, List<byte[]> Tileset)
        {            
            FileLocation = FilePath;
            Name = Path.GetFileNameWithoutExtension(FilePath);
            Extension = Path.GetExtension(FilePath);
            Size = (int)new FileInfo(FilePath).Length;
            Contents = File.ReadAllBytes(FileLocation);
            ContentsIndex = 0;
            MapTileset = Tileset;
            MapFormat = MapEncoding;
        }

        public override string Name { get; }
        public override string FileLocation { get; }
        public override string Extension { get; }
        public override int Size { get; }
        protected byte[] Contents;
        protected int ContentsIndex;
        public int MapSizeX { get; protected set; }
        public int MapSizeY { get; protected set; }
        public List<byte[]> MapTileset { get; }
        public MapFormat MapFormat { get; }           
        public byte[] RAWMap { get; protected set; }
        public byte[] MapData { get; protected set; }

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

        protected byte[] RemapBlockFormat(byte[] RAWmap)
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

        protected byte[] RemapLinearFormat(byte[] RAWmap)
        {
            //-------------------------------------------
            // Remap map
            //--------------------------------------------
            // 8 Block linear map
            // Remap into standard linear X * Y map

            log.Debug($"Remapping {Name}");
            log.Debug($"{MapSizeX} * {MapSizeY}");
            log.Debug($"Total size: {RAWmap.Length}");

            var remapped = new byte[4096];

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

                            remapped[TotalOffset] = RAWmap[MapIndex];
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