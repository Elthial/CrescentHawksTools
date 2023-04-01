using InceptionTools.FileTypes;
using InceptionTools.Graphics;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace InceptionTools
{
    public class AssetExtractor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _SourceFolder;
        private EGA EGA;
        private RunLengthEncoding RLE;
        private Dictionary<string, List<byte[]>> TileSets = new Dictionary<string, List<byte[]>>();
        public void ExtractToFileSystem(string SourceFolder)
        {
            EGA = new EGA();
            RLE = new RunLengthEncoding();
            _SourceFolder = SourceFolder;

            ExtractImageFiles();
            ExtractMapFiles();
            ExtractAnimationFiles();
        }

        private void ExtractMapFiles()
        {           
            var CHI_Files = new List<MapFile>()
            {
                new MAP(Path.Combine(_SourceFolder, "MAP1.MTP"), MapFormat.BlockFormat, TileSets["BTTLTECH"]), //Citadel
                new MAP(Path.Combine(_SourceFolder, "MAP2.MTP"), MapFormat.BlockFormat, TileSets["BTTLTECH"]), //Starport
                new MAP(Path.Combine(_SourceFolder, "MAP3.MTP"), MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Prison
                new MAP(Path.Combine(_SourceFolder, "MAP4.MTP"), MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(Path.Combine(_SourceFolder, "MAP5.MTP"), MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(Path.Combine(_SourceFolder, "MAP6.MTP"), MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(Path.Combine(_SourceFolder, "MAP7.MTP"), MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(Path.Combine(_SourceFolder, "MAP8.MTP"), MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(Path.Combine(_SourceFolder, "MAP9.MTP"), MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(Path.Combine(_SourceFolder, "MAP10.MTP"), MapFormat.LinearFormat, TileSets["BTTLTECH"]), //Village
                new MAP(Path.Combine(_SourceFolder, "MAP11.MTP"),  MapFormat.BlockFormat, TileSets["DESTRUCT"]), //Destroyed Citadel
                new MAP(Path.Combine(_SourceFolder, "MAP12.MTP"),  MapFormat.BlockFormat, TileSets["BTTLTECH"]), //Inventors hut
                new MAP(Path.Combine(_SourceFolder, "MAP13.MTP"),  MapFormat.BlockFormat, TileSets["BTTLTECH"]), //Cache exterior
                new MAP(Path.Combine(_SourceFolder, "MAP14.MTP"),  MapFormat.BlockFormat, TileSets["STARLEAG"]),  //StarLeague Cache
                new STARMAP(Path.Combine(_SourceFolder, "MAP15.MTP"), MapFormat.LinearFormat, TileSets["MAP"]) //Star Map
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

        private void ExtractAnimationFiles()
        {
            //Some of these animations need custom Palettes
            var EGA_Palette = new Palette();
            var CHI_Files = new List<AnimationFile>()
            {
                new AnimationFile(Path.Combine(_SourceFolder, "o0.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o1.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o2.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o3.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o4.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o5.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o6.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o7.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o8.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o9.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o10.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o11.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o12.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o13.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o14.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o15.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o16.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o17.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o18.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o19.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o20.ANM"), EGA_Palette),
                new AnimationFile(Path.Combine(_SourceFolder, "o21.ANM"), EGA_Palette)
            };            

            foreach (var f in CHI_Files)
            {
                log.Info("Crescent Hawks: Inception toolkit");
                log.Info("Current file Header Info");
                log.Info("----------------------------");
                log.Info($"Filename: {f.Name}");
                log.Info($"FileSize: {f.Size}");            
                log.Info("----------------------------");
                                
                byte[] OutputArray = RLE.Decompress_Animation(f.CompressedContents, f.StartPos);

                f.DecompressedContents = EGA.Write2ModeConverter(OutputArray);

                EGA.DrawAnimationToFile(f);
            }
        }

        private void ExtractImageFiles()
        {
            var EGA_Palette = new Palette();
            var BTTITLE_Palette = new Palette();
            var INFOCOM_Palette = new Palette();
            var ENDMECH_Palette = new Palette();

            var CHI_Files = new List<InceptionImageFile>()
            {
                new InceptionImageFile(Path.Combine(_SourceFolder, "BTTLTECH.ICN"), ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "BTBORDER.CMP"), ImagePurpose.TinyTileSet, EGA_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "ANIMATE.ICN"), ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "STARLEAG.ICN"), ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "DESTRUCT.ICN"), ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "MAP.ICN"), ImagePurpose.TileSet, EGA_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "TINYLAND.CMP"), ImagePurpose.TinyTileSet, EGA_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "BTSTATS.CMP"), ImagePurpose.FullScreen, EGA_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "BTTITLE.CMP"), ImagePurpose.FullScreen, BTTITLE_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "INFOCOM.CMP"), ImagePurpose.FullScreen, INFOCOM_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "MECHSHAP.CMP"), ImagePurpose.SpriteSheet, EGA_Palette),
                new InceptionImageFile(Path.Combine(_SourceFolder, "ENDMECH.CMP"), ImagePurpose.FullScreen, ENDMECH_Palette)
            };

            BTTITLE_Palette.SwapColour(1, new PaletteColour("Black / Background", 0x00, 0x00, 0x00));
            INFOCOM_Palette.SwapColour(9, new PaletteColour("Dark Blue / Shadow", 0x00, 0x00, 0xAA));
            INFOCOM_Palette.SwapColour(5, new PaletteColour("Light Blue / Background", 0x55, 0x55, 0xFF));
            ENDMECH_Palette.SwapColour(1, new PaletteColour("Black / Background", 0x00, 0x00, 0x00));         
            ENDMECH_Palette.SwapColour(13, new PaletteColour("Light Blue / Jacket / Reflection", 0x55, 0x55, 0xFF));    
            ENDMECH_Palette.SwapColour(9, new PaletteColour("Dark Blue / Jacket", 0x00, 0x00, 0xAA));

            //TODO: Update code with TileSet image which has max number of images in file to remove empty tiles

            var RLE = new RunLengthEncoding();

            foreach (var f in CHI_Files)
            {
                log.Info("Crescent Hawks: Inception toolkit");
                log.Info("Current file Header Info");
                log.Info("----------------------------");
                log.Info($"Filename: {f.Name}");
                log.Info($"FileSize (minus two bytes): {f.Size}");
                log.Info($"RLE Format: Format0{f.CompressionFormat}");
                log.Info("----------------------------");
                                
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