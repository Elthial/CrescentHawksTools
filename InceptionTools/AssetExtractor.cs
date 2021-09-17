using System;
using System.Collections.Generic;
using System.IO;

namespace InceptionTools
{

    class InceptionFile
    {
        public InceptionFile(string ImagePath, int ImageWidth)
        {
            Name = Path.GetFileNameWithoutExtension(ImagePath);
            FilePath = ImagePath;
            Width = ImageWidth;            
            Contents = File.ReadAllBytes(FilePath);            
            Size = BitConverter.ToInt16(Contents, 0);
            Format = Contents[2];
            StartPos = 3;
        }
        public string Name { get; }
        public string FilePath { get; }
        public int Width { get; }
        public int Format { get; }
        public int Size { get; }
        public int StartPos { get; }
        public byte[] Contents { get; }

        //Palette
    }

    public class AssetExtractor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void ExtractToFileSystem()
        {
            var CHI_Files = new List<InceptionFile>()
            {
                new InceptionFile(@"G:\btech\BTTLTECH.ICN", 16),
                new InceptionFile(@"G:\btech\BTBORDER.CMP", 320),
                new InceptionFile(@"G:\btech\ANIMATE.ICN", 16),
                new InceptionFile(@"G:\btech\STARLEAG.ICN", 16),
                new InceptionFile(@"G:\btech\DESTRUCT.ICN", 16),
                new InceptionFile(@"G:\btech\MAP.ICN", 16),
                new InceptionFile(@"G:\btech\TINYLAND.CMP", 320),
                new InceptionFile(@"G:\btech\BTSTATS.CMP", 320),
                new InceptionFile(@"G:\btech\BTTITLE.CMP", 320),
                new InceptionFile(@"G:\btech\INFOCOM.CMP", 320),
                new InceptionFile(@"G:\btech\MECHSHAP.CMP", 320)
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