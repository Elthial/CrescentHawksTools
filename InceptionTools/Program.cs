using InceptionTools.Graphics;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace InceptionTools
{

    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            LoadLog4Net();
            log.Info("Log4Net Loaded");

            var Assets = new AssetExtractor();
            Assets.ExtractToFileSystem();
            log.Info("Asset Extract Completed");

            var tileset = Assets.TileSets["BTTLTECH"];
            var map = new MAP(@"G:\btech\MAP1.MTP");
            Console.WriteLine("Review file");


            //-------------------------------------------
            // Temp drawing map
            //--------------------------------------------

            var TileSizeX = 16;
            var TileSizeY = 16;
            var PixelX = map.MapSizeX * TileSizeX;
            var PixelY = map.MapSizeY * TileSizeY;

            log.Info($"Creating {PixelX} x {PixelY} BMP Image.");

            using (var bmp = new Bitmap(PixelX, PixelY, PixelFormat.Format32bppArgb))
            {
                var bytecounter = 0;

                for (int TileY = 0; TileY < map.MapSizeY; TileY++) 
                {
                    for (int TileX = 0; TileX < map.MapSizeX; TileX++) 
                    {
                        DrawTile(bmp, map.MapData[bytecounter], TileX, TileY, tileset);
                        bytecounter++;
                    }
                }

                log.Info(@$"Saving Assets\GameMap.bmp");
                Directory.CreateDirectory("Assets");
                bmp.Save(@$"Assets\GameMap.bmp");
            }


        }

        static void DrawTile(Bitmap bmp, byte TileType, int TileX, int TileY, List<byte[]> TileSet)
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

        static void LoadLog4Net()
        {
            try
            {

                ILoggerRepository repository = log4net.LogManager.GetRepository(Assembly.GetCallingAssembly());

                var fileInfo = new FileInfo(@"log4net.config");

                log4net.Config.XmlConfigurator.Configure(repository, fileInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }

    }

    class MapFile
    {
        public void ReadMapFile()
        {

        }
    }
}