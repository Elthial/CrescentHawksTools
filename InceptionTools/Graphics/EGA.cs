using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace InceptionTools.Graphics
{
    class EGA
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public byte[] Write2ModeConverter(byte[] GraphicsArray)
        {
            int Width = 320;
            int Height = 200;

            // (*
            // 	  Write mode 2
            // 	  The lower 4 bits of the CPU byte are the bits for each plane. If set for a
            // 	  plane, all bits selected with the Bitmask register are set, if clear, they're
            // 	  cleared.
            // 	  So you can do the same as in the example from mode 0, but with fewer
            // 	  instructions:
            // 	  *)	


            //This is a screen draw for 320 x 200
            //320px x 200px / 8bitsperbyte = 8000 Bytes

            //EGA draws 8 pixels at a time, Each pixel is 4 bit (nibble)
            //Write mode two writes to 8 bits across 4 planes
            //Write mode two spreads a byte (high & low nibbles) across two bits
            //Little-endian systems, lower-order bytes precede higher-order bytes

            byte[] VGA_Memory = new byte[Width * Height];
            int VGA_Offset = 0;
            int ByteCount = 0;

            foreach (byte b in GraphicsArray)
            {
                byte high = (byte)(b >> 4 & 0xF);
                byte low = (byte)(b & 0x0F);
                VGA_Memory[VGA_Offset] = high;
                ++VGA_Offset;
                VGA_Memory[VGA_Offset] = low;
                ++VGA_Offset;
                ByteCount++;
            }

            log.Info($"Screen Resolution is {Width} x {Height}. Expected Pixels {Width * Height}");
            log.Info($"There are {VGA_Memory.Length - VGA_Offset} missing pixels from the VGA register");

            return VGA_Memory;
        }

        public List<byte[]> DrawToTileSet(InceptionImageFile f)
        {
            log.Info($"Creating Tileset.");
            //EGA Screens are default 320 x 200 = 64000 pixels

            var TileSet = new List<byte[]>();

            var ImageData = f.DecompressedContents;          
      
            var TileHeight = 16;
            var TileWidth = 16;
            var BytesPerTile = TileWidth * TileHeight;
            var NumOfTiles = f.DecompressedContents.Length / BytesPerTile;
            int bytecount = 0;

            for (int i = 0; i < NumOfTiles; i++)
            {
                var Tile = new byte[BytesPerTile];
                var TileByte = 0;

                for (int y = 0; y < TileHeight; y++)
                {
                    for (int x = 0; x < TileWidth; x++)
                    {
                        Tile[TileByte] = ImageData[bytecount];
                        TileByte++;
                        bytecount++;
                    }
                }

                log.Debug($"TileSet: Adding tile[{i}]");
                TileSet.Add(Tile);
            }

            return TileSet;
        }

        public void DrawToFile(InceptionImageFile f)
        {
            const int ScreenPixels = 64000;
            //EGA Screens are default 320 x 200 = 64000 pixels

            var ImageData = f.DecompressedContents;
            var ImageWidth = f.Width;
            var Filename = f.Name;
            var palette = f.Palette;

            int ImageHeight = ScreenPixels / ImageWidth;
            using (var bmp = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb))
            {
                log.Info($"Creating {ImageWidth} x {ImageHeight} BMP Image.");
                int bytecount = 0;

                for (int y = 0; y < ImageHeight; y++)
                {
                    for (int x = 0; x < ImageWidth; x++)
                    {
                        bmp.SetPixel(x, y, palette.GetColour(ImageData[bytecount]));
                        bytecount++;
                    }
                }

                log.Info(@$"Saving Assets\{Filename}.bmp");
                Directory.CreateDirectory("Assets");
                bmp.Save(@$"Assets\{Filename}.bmp");
            }
        }

        public void DrawMapToFile(MAP map)
        {
            //-------------------------------------------
            // Drawing map for debugging purposes
            //--------------------------------------------

            var TileSizeX = 16;
            var TileSizeY = 16;
            var PixelX = map.MapSizeX * TileSizeX;
            var PixelY = map.MapSizeY * TileSizeY;

            log.Info($"Creating {PixelX} x {PixelY} BMP Image.");

            var mapData = map.MapData;
           
            using (var bmp = new Bitmap(PixelX, PixelY, PixelFormat.Format32bppArgb))
            {
                var bytecounter = 0;

                for (int TileY = 0; TileY < map.MapSizeY; TileY++)
                {
                    for (int TileX = 0; TileX < map.MapSizeX; TileX++)
                    {
                        DrawTile(bmp, mapData[bytecounter], TileX, TileY, map.MapTileset);
                        bytecounter++;
                    }
                }

                log.Info(@$"Saving Assets\Maps\{map.Name}.bmp");
                Directory.CreateDirectory(@"Assets\Maps");
                bmp.Save(@$"Assets\Maps\{map.Name}.bmp");
            }
        }


        private void DrawTile(Bitmap bmp, byte TileType, int TileX, int TileY, List<byte[]> TileSet)
        {
            var bytecount = 0;
            var TileSizeX = 16;
            var TileSizeY = 16;
            var Tile = TileSet[TileType];
            var Palette = new Palette();

            for (int PixelY = 0; PixelY < TileSizeY; PixelY++)
            {
                for (int PixelX = 0; PixelX < TileSizeX; PixelX++)
                {
                    var OffsetX = (TileX * TileSizeX) + PixelX;
                    var OffsetY = (TileY * TileSizeY) + PixelY;
                    int ColourIndex = Tile[bytecount];

                    bmp.SetPixel(OffsetX, OffsetY, Palette.GetColour(ColourIndex));
                    bytecount++;
                }
            }
        }
    }
}