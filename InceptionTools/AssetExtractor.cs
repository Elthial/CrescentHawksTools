using InceptionTools.Graphics;
using System.Collections.Generic;

namespace InceptionTools
{
    public class AssetExtractor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private EGA EGA;
        private Dictionary<string, List<byte[]>> TileSets = new Dictionary<string, List<byte[]>>();
        public void ExtractToFileSystem()
        {
            EGA = new EGA();

            ExtractImageFiles();
            ExtractMapFiles();
        }

        private void ExtractMapFiles()
        {           
            var CHI_Files = new List<MapFile>()
            {
                new MAP(@"G:\btech\MAP1.MTP", MapFormat.BlockFormat, TileSets["BTTLTECH"]), //Citadel
                new MAP(@"G:\btech\MAP2.MTP", MapFormat.BlockFormat, TileSets["BTTLTECH"]), //Starport
                new MAP(@"G:\btech\MAP3.MTP", MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Prison
                new MAP(@"G:\btech\MAP4.MTP", MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(@"G:\btech\MAP5.MTP", MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(@"G:\btech\MAP6.MTP", MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(@"G:\btech\MAP7.MTP", MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(@"G:\btech\MAP8.MTP", MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(@"G:\btech\MAP9.MTP", MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(@"G:\btech\MAP10.MTP", MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(@"G:\btech\MAP11.MTP",  MapFormat.BlockFormat, TileSets["DESTRUCT"]), //Destroyed Citadel
                new MAP(@"G:\btech\MAP12.MTP",  MapFormat.BlockFormat, TileSets["BTTLTECH"]), //Inventors hut
                new MAP(@"G:\btech\MAP13.MTP",  MapFormat.BlockFormat, TileSets["BTTLTECH"]), //Cache exterior
                new MAP(@"G:\btech\MAP14.MTP",  MapFormat.BlockFormat, TileSets["STARLEAG"]),  //StarLeague Cache
                new STARMAP(@"G:\btech\MAP15.MTP", MapFormat.LinearFormat, TileSets["MAP"]) //Star Map
            };

            //World map is missing
            //No Remaining files, map likely 1024 x 1024 tiles = 1 MB 
            //No file sized to contain it, massively compressed or procedurally generated?
       

            foreach (var f in CHI_Files)
            {
                log.Info("Crescent Hawks: Inception toolkit");
                log.Info("Current file Header Info");
                log.Info("----------------------------");
                log.Info($"Filename: {f.Name}");
                log.Info($"FileSize (minus two bytes): {f.Size}");
                log.Info($"Map Size: {f.MapData.Length}");
                log.Info("----------------------------");

                EGA.DrawMapToFile(f);    
            }
        }

        private void ExtractImageFiles()
        {
            var EGA_Palette = new Palette();
            var BTTITLE_Palette = new Palette();
            var INFOCOM_Palette = new Palette();

            var CHI_Files = new List<InceptionImageFile>()
            {
                new InceptionImageFile(@"G:\btech\BTTLTECH.ICN", ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(@"G:\btech\BTBORDER.CMP", ImagePurpose.TinyTileSet, EGA_Palette),
                new InceptionImageFile(@"G:\btech\ANIMATE.ICN", ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(@"G:\btech\STARLEAG.ICN", ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(@"G:\btech\DESTRUCT.ICN", ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(@"G:\btech\MAP.ICN", ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(@"G:\btech\TINYLAND.CMP", ImagePurpose.TinyTileSet, EGA_Palette),
                new InceptionImageFile(@"G:\btech\BTSTATS.CMP", ImagePurpose.FullScreen, EGA_Palette),
                new InceptionImageFile(@"G:\btech\BTTITLE.CMP", ImagePurpose.FullScreen, BTTITLE_Palette),
                new InceptionImageFile(@"G:\btech\INFOCOM.CMP", ImagePurpose.FullScreen, INFOCOM_Palette),
                new InceptionImageFile(@"G:\btech\MECHSHAP.CMP", ImagePurpose.SpriteSheet, EGA_Palette)
            };

            BTTITLE_Palette.SwapColour(1, new PaletteColour("Black / Background", 0x00, 0x00, 0x00));
            INFOCOM_Palette.SwapColour(9, new PaletteColour("Dark Blue / Shadow", 0x00, 0x00, 0xAA));
            INFOCOM_Palette.SwapColour(5, new PaletteColour("Light Blue / Background", 0x55, 0x55, 0xFF));

            //TODO: Update code with TileSet image which has max number of images in file to remove empty tiles

            foreach (var f in CHI_Files)
            {
                log.Info("Crescent Hawks: Inception toolkit");
                log.Info("Current file Header Info");
                log.Info("----------------------------");
                log.Info($"Filename: {f.Name}");
                log.Info($"FileSize (minus two bytes): {f.Size}");
                log.Info($"RLE Format: Format0{f.CompressionFormat}");
                log.Info("----------------------------");

                var RLE = new RunLengthEncoding();

                byte[] OutputArray;

                if (f.CompressionFormat.Equals(1))
                {
                    log.Info("Using Decompress_Format01");
                    OutputArray = RLE.Decompress_Format01(f.CompressedContents, f.StartPos);
                }
                else
                {
                    log.Info("Using Decompress_Format02");
                    OutputArray = RLE.Decompress_Format02(f.CompressedContents, f.StartPos);
                }
               
                f.DecompressedContents = EGA.Write2ModeConverter(OutputArray);

                EGA.DrawImageToFile(f);

                if (f.Purpose.Equals(ImagePurpose.TileSet))
                {
                    var ts = EGA.DrawToTileSet(f);
                    TileSets.Add(f.Name, ts);
                }
            }
        }
    }
}