using System;
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

                for (int y = 0; y <= ImageHeight - 1; y++)
                {
                    for (int x = 0; x <= ImageWidth - 1; x++)
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
    }
}