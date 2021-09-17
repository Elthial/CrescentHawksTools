using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace InceptionTools
{
    class EGA
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public byte[] Write2ModeConverter(Byte[] GraphicsArray)
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
                byte high = (byte)((b >> 4) & 0xF);
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

        public void DrawToScreen(byte[] ImageData, int ImageWidth, string Filename)
        {
            //EGA Screens are default 320 x 200 = 64000 pixels

            int ImageHeight = 64000 / ImageWidth;
            using (var bmp = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb))
            {
                log.Info($"Creating {ImageWidth} x {ImageHeight} BMP Image.");
                int bytecount = 0;

                for (int y = 0; y <= ImageHeight - 1; y++)
                {
                    for (int x = 0; x <= ImageWidth - 1; x++)
                    {
                        bmp.SetPixel(x, y, GetEGADefaultColours(ImageData[bytecount]));
                        bytecount++;
                    }
                }

                log.Info(@$"Saving Assets\{Filename}.bmp");
                Directory.CreateDirectory("Assets");
                bmp.Save(@$"Assets\{Filename}.bmp");
            }
        }

        //Support Palette swapping
        //Change one colour for another
        //Infocom needs 5 (AA00AA) swapped for 41 (5500FF)
        //BTTitle needs 1 (0000AA) swapped for 0 (000000)
        public Color GetEGADefaultColours(int pixel)
        {
            switch (pixel)
            {
                case 0: //Black
                    return Color.FromArgb(0x00, 0x00, 0x00);
                case 1: //Blue
                    return Color.FromArgb(0x00, 0x00, 0xAA);
                case 2: //Green
                    return Color.FromArgb(0x00, 0xAA, 0x00);
                case 3: //Cyan
                    return Color.FromArgb(0x00, 0xAA, 0xAA);
                case 4: //Red
                    return Color.FromArgb(0xAA, 0x00, 0x00);
                case 5: //Magenta
                    return Color.FromArgb(0xAA, 0x00, 0xAA);
                case 6: //Yellow / Brown
                    return Color.FromArgb(0xAA, 0x55, 0x00);
                case 7: //White / Light Grey
                    return Color.FromArgb(0xAA, 0xAA, 0xAA);
                case 8: //Dark Gray / Bright Black
                    return Color.FromArgb(0x55, 0x55, 0x55);
                case 9: //Bright Blue
                    return Color.FromArgb(0x55, 0x55, 0xFF);
                case 10: //Bright Green
                    return Color.FromArgb(0x55, 0xFF, 0x55);
                case 11: //Bright Cyan
                    return Color.FromArgb(0x55, 0xFF, 0xFF);
                case 12: //Bright Red
                    return Color.FromArgb(0xFF, 0x55, 0x55);
                case 13: //Bright Magenta
                    return Color.FromArgb(0xFF, 0x55, 0xFF);
                case 14: //Bright Yellow
                    return Color.FromArgb(0xFF, 0xFF, 0x55);
                case 15: //Bright White
                    return Color.FromArgb(0xFF, 0xFF, 0xFF);
                default:
                    throw new ArgumentOutOfRangeException("Non-EGA colour value detected!");
            }
        }
    }
}