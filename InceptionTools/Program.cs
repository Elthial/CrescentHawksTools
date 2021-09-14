using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace InceptionTools
{

    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            LoadLog4Net();

            //Format 01 Files
            var BTTLTECH = @"G:\btech\BTTLTECH.ICN"; //16 byte
            var BTBORDER = @"G:\btech\BTBORDER.CMP";  //8 byte
            var ANIMATE = @"G:\btech\ANIMATE.ICN";   //16 byte
            var STARLEAG = @"G:\btech\STARLEAG.ICN"; //16 byte
            var DESTRUCT = @"G:\btech\DESTRUCT.ICN"; //16 byte
            var MAP = @"G:\btech\MAP.ICN";           //16 byte
            var TINYLAND = @"G:\btech\TINYLAND.CMP"; //8 byte

            //Format 02 Files
            var BTSTATS = @"G:\btech\BTSTATS.CMP";
            var BTTITLE = @"G:\btech\BTTITLE.CMP";
            var INFOCOM = @"G:\btech\INFOCOM.CMP";
            var MECHSHAP = @"G:\btech\MECHSHAP.CMP";

            var FilePath = MAP;

            var TestArray = File.ReadAllBytes(FilePath);
            var Filename = Path.GetFileNameWithoutExtension(FilePath);
            var FileSize = BitConverter.ToInt16(TestArray, 0);
            var Format = TestArray[2];

            log.Debug("test");
            Console.WriteLine("Header Info");
            Console.WriteLine("----------------------------");
            Console.WriteLine($"Filename: {Filename}");
            Console.WriteLine($"FileSize (minus two bytes): {FileSize}");
            Console.WriteLine($"RLE Format: Format0{Format}");
            Console.WriteLine("----------------------------");

            byte[] OutputArray;

            var RLE = new RunLengthEncoding();

            if (Format.Equals(1))
            {
                Console.WriteLine("Using Decompress_Format01");
                OutputArray = RLE.Decompress_Format01(TestArray, 3);
            }
            else
            {
                Console.WriteLine("Using Decompress_Format02");
                OutputArray = RLE.Decompress_Format02(TestArray, 3);
            }


            var EGA = new EGA();
            var imageData = EGA.Write2ModeConverter(OutputArray);

            var bmp = EGA.DrawToScreen(imageData, 16);
            Console.WriteLine("Saving BMP");
            bmp.Save($"{Filename}.bmp");

        }

        static void LoadLog4Net()
        {
            try
            {

                ILoggerRepository repository = log4net.LogManager.GetRepository(Assembly.GetCallingAssembly());

                var fileInfo = new FileInfo(@"log4net.config");

                log4net.Config.XmlConfigurator.Configure(repository, fileInfo);

                log.Info("test");

                Console.WriteLine("press any key");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }

    }

    class RunLengthEncoding
    {
        public byte[] Decompress_Format01(byte[] CompressedFile, ushort IndexStart)
        {
            short MaxBufferRemaining = 0x7D00;
            byte[] DecodedBuffer = new byte[MaxBufferRemaining]; //29440

            short DecodedBuffer_Index = 0;
            ushort CompressedFile_Index = IndexStart;

            Console.WriteLine($"CompressedFile Length: {CompressedFile.Length}");

            GetByteToDecompress:

            Console.WriteLine("------------------------------");
            Console.WriteLine($"Current Index: {CompressedFile_Index} of {CompressedFile.Length}");
            Console.WriteLine($"Remaining: {CompressedFile.Length - CompressedFile_Index}");
            Console.WriteLine("------------------------------");
            ushort ByteValue;
            bool IsZeroByte = false;
            sbyte CurrentCompressedByte = (sbyte)CompressedFile[CompressedFile_Index];
            Console.WriteLine($"Byte: {CurrentCompressedByte.ToString("X")}");

            if (CurrentCompressedByte != 0x00)
            {
                ByteValue = (ushort)CurrentCompressedByte;
                if (CurrentCompressedByte >= 0x00)
                    goto SetRepeatValue;

                ByteValue = (ushort)-CurrentCompressedByte; //if byte negative flip to positive. 
                Console.WriteLine($"Negate to: {ByteValue.ToString("X")}");
                //Example: F9 -> 07 as negate: C1, A1 flags
            }
            else
            {
                CompressedFile_Index++;
                //Console.WriteLine($"Current Index: {CompressedFile_Index}");
                ByteValue = BitConverter.ToUInt16(CompressedFile, CompressedFile_Index);
                CompressedFile_Index++;
                //Console.WriteLine($"Current Index: {CompressedFile_Index}");
            }
            //Flag as zero seen
            IsZeroByte = true;
            Console.WriteLine($"Zero Flag");

            SetRepeatValue:
            //Byte value becomes repeat value
            ushort RunLength = ByteValue; //4
            Console.WriteLine($"RunLength: {RunLength.ToString("X")}");

            GetNextByte:
            //Increment to next byte
            ++CompressedFile_Index;
            //Console.WriteLine($"Current Index: {CompressedFile_Index}");
            //Next byte will go to buffer
            byte OutputByte = CompressedFile[CompressedFile_Index];//99           

            do
            {
                Console.WriteLine($"OutputByte: {OutputByte.ToString("X")}");
                DecodedBuffer[DecodedBuffer_Index] = OutputByte;
                ++DecodedBuffer_Index;
                --MaxBufferRemaining;//--dx

                if (MaxBufferRemaining == 0)
                    return DecodedBuffer;


                if (!IsZeroByte)
                {
                    --RunLength;
                    if (RunLength != 0)
                        goto GetNextByte;

                    ++CompressedFile_Index;
                    //Console.WriteLine($"Current Index: {CompressedFile_Index}");
                    Console.WriteLine($"GetByteToDecompress");
                    goto GetByteToDecompress;
                }
                --RunLength;
            } while (RunLength != 0);
            ++CompressedFile_Index;
            //Console.WriteLine($"Current Index: {CompressedFile_Index}");
            Console.WriteLine($"GetByteToDecompress");
            goto GetByteToDecompress;
        }

        public byte[] Decompress_Format02(byte[] CompresssedFile, ushort IndexStart)
        {
            short MaxBufferRemaining = 0x7D00;
            byte[] DecodedBuffer = new byte[MaxBufferRemaining];

            short DecodedBuffer_Index = 0;
            ushort CompressedFile_Index = IndexStart;


            short MaxRunLengthRemaining = 200;

            GetByteToDecompress:
            ushort ByteValue;
            bool IsZeroByte = false;
            sbyte CurrentCompressedByte = (sbyte)CompresssedFile[CompressedFile_Index];

            if (CurrentCompressedByte != 0x00)
            {
                ByteValue = (ushort)CurrentCompressedByte;
                if (CurrentCompressedByte >= 0x00)
                    goto SetRepeatValue;

                ByteValue = (ushort)-CurrentCompressedByte;
            }
            else
            {
                CompressedFile_Index++;
                ByteValue = CompresssedFile[CompressedFile_Index];
                CompressedFile_Index++;
            }
            IsZeroByte = true;

            SetRepeatValue:
            ushort RunLength = ByteValue;

            GetNextByte:
            ++CompressedFile_Index;
            byte OutputByte = CompresssedFile[CompressedFile_Index];

            do
            {
                DecodedBuffer[DecodedBuffer_Index] = OutputByte;
                ++DecodedBuffer_Index;
                --MaxRunLengthRemaining;

                if (MaxRunLengthRemaining == 0)
                {
                    MaxRunLengthRemaining = 200;
                    DecodedBuffer_Index -= 31999; //How to this map outside of pointers sub di,7CFFh
                }
                --MaxBufferRemaining;

                if (MaxBufferRemaining == 0)
                    return DecodedBuffer;

                if (IsZeroByte)
                {
                    --RunLength;
                    if (RunLength != 0x00)
                        goto GetNextByte;

                    ++CompressedFile_Index;
                    goto GetByteToDecompress;
                }
                --RunLength;

            } while (RunLength != 0x00);

            ++CompressedFile_Index;
            goto GetByteToDecompress;
        }
    }

    class EGA
    {
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

            Console.WriteLine($"Screen Resolution is {Width} x {Height}. Expected Pixels {Width * Height}");
            Console.WriteLine($"There are {VGA_Memory.Length - VGA_Offset} missing pixels from the VGA register");

            return VGA_Memory;
        }

        public Bitmap DrawToScreen(byte[] ImageData, int ImageWidth)
        {
            //EGA Screens are default 320 x 200 = 64000 pixels

            int ImageHeight = 64000 / ImageWidth;
            using (var bmp = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb))
            {
                Console.WriteLine($"Creating {ImageWidth} x {ImageHeight} BMP Image.");
                int bytecount = 0;

                for (int y = 0; y <= ImageHeight - 1; y++)
                {
                    for (int x = 0; x <= ImageWidth - 1; x++)
                    {
                        bmp.SetPixel(x, y, GetEGAColour(ImageData[bytecount]));
                        bytecount++;
                    }
                }

                return bmp;
            }
        }

        public Color GetEGAColour(int pixel)
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