using System.Collections.Generic;

namespace InceptionTools
{

    public class AssetExtractor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void ExtractToFileSystem()
        {
            var CHI_Files = new List<InceptionImageFile>()
            {
                new InceptionImageFile(@"G:\btech\BTTLTECH.ICN"),
                new InceptionImageFile(@"G:\btech\BTBORDER.CMP"),
                new InceptionImageFile(@"G:\btech\ANIMATE.ICN"),
                new InceptionImageFile(@"G:\btech\STARLEAG.ICN"),
                new InceptionImageFile(@"G:\btech\DESTRUCT.ICN"),
                new InceptionImageFile(@"G:\btech\MAP.ICN"),
                new InceptionImageFile(@"G:\btech\TINYLAND.CMP"),
                new InceptionImageFile(@"G:\btech\BTSTATS.CMP"),
                new InceptionImageFile(@"G:\btech\BTTITLE.CMP"),
                new InceptionImageFile(@"G:\btech\INFOCOM.CMP"),
                new InceptionImageFile(@"G:\btech\MECHSHAP.CMP")
            };

            foreach(var f in CHI_Files)
            {
                log.Info("Crescent Hawks: Inception toolkit");
                log.Info("Current file Header Info");
                log.Info("----------------------------");
                log.Info($"Filename: {f.Name}");
                log.Info($"FileSize (minus two bytes): {f.Size}");
                log.Info($"RLE Format: Format0{f.Format}");
                log.Info("----------------------------");

                var RLE = new RunLengthEncoding();

                byte[] OutputArray;

                if (f.Format.Equals(1))
                {
                    log.Info("Using Decompress_Format01");
                    OutputArray = RLE.Decompress_Format01(f.Contents, f.StartPos);
                }
                else
                {
                    log.Info("Using Decompress_Format02");
                    OutputArray = RLE.Decompress_Format02(f.Contents, f.StartPos);
                }


                var EGA = new EGA();
                var imageData = EGA.Write2ModeConverter(OutputArray);

                EGA.DrawToScreen(imageData, f.Width, f.Name);
            }         
        }
    }
}