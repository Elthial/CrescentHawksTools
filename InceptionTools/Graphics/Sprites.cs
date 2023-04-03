using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InceptionTools.Graphics
{
    internal class Sprites
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _SourceFolder;

        public Sprites(string SourceFolder) 
        {
            _SourceFolder = SourceFolder;
        }


        // 1F3D:070A: Register void Extract_Sprites_From_MECHSHAP(Stack int16 Index, Stack int16 Ptr06, Stack int16 Ptr08, Stack Eq_10931 VerticalOffset, Stack Eq_10932 HorizontalOffset)
        // Called from:
        //      Setup_Game
        internal void Extract_Sprites_From_MECHSHAP(int SpriteId, int x0, int y0, int SizeX, int SizeY)
        {
            x0 *= 8;
            SizeX *= 8;

            var x1 = x0 + SizeX;
            var y1 = y0 + SizeY;
            Console.WriteLine($"Sprite: {SpriteId}, x0: {x0}, y0: {y0}, x1: {SizeX}, y1: {SizeY}");


            var EGA_Palette = new Palette();
            var MechSnap = new InceptionImageFile(Path.Combine(_SourceFolder, "MECHSHAP.CMP"), ImagePurpose.SpriteSheet, EGA_Palette);

            var CHI_Files = new List<InceptionImageFile>()
            {
                new InceptionImageFile(Path.Combine(_SourceFolder, "MECHSHAP.CMP"), ImagePurpose.SpriteSheet, EGA_Palette),
            };
            var EGA = new EGA();
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

                EGA.DrawSpriteToFile(f, SpriteId, x0, y0, SizeX, SizeY);
            }
        }
    }
}
