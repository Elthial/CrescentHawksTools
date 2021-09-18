using InceptionTools.Graphics;
using System.Collections.Generic;

namespace InceptionTools
{

    public class AssetExtractor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void ExtractToFileSystem()
        {
            var EGA_Palette = new Palette();
            var BTTITLE_Palette = new Palette();
            var INFOCOM_Palette = new Palette();

            var CHI_Files = new List<InceptionImageFile>()
            {
                new InceptionImageFile(@"G:\btech\BTTLTECH.ICN", EGA_Palette),
                new InceptionImageFile(@"G:\btech\BTBORDER.CMP", EGA_Palette),
                new InceptionImageFile(@"G:\btech\ANIMATE.ICN", EGA_Palette),
                new InceptionImageFile(@"G:\btech\STARLEAG.ICN", EGA_Palette),
                new InceptionImageFile(@"G:\btech\DESTRUCT.ICN", EGA_Palette),
                new InceptionImageFile(@"G:\btech\MAP.ICN", EGA_Palette),
                new InceptionImageFile(@"G:\btech\TINYLAND.CMP", EGA_Palette),
                new InceptionImageFile(@"G:\btech\BTSTATS.CMP", EGA_Palette),
                new InceptionImageFile(@"G:\btech\BTTITLE.CMP",BTTITLE_Palette), 
                new InceptionImageFile(@"G:\btech\INFOCOM.CMP", INFOCOM_Palette),
                new InceptionImageFile(@"G:\btech\MECHSHAP.CMP", EGA_Palette)
            };

            BTTITLE_Palette.SwapColour(1, new PaletteColour("Black / Background", 0x00, 0x00, 0x00));
            INFOCOM_Palette.SwapColour(9, new PaletteColour("Dark Blue / Shadow", 0x00, 0x00, 0xAA));
            INFOCOM_Palette.SwapColour(5, new PaletteColour("Light Blue / Background", 0x55, 0x55, 0xFF));

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

                var EGA = new EGA();
                f.DecompressedContents = EGA.Write2ModeConverter(OutputArray);

                EGA.DrawToFile(f);
            }         
        }
    }
}