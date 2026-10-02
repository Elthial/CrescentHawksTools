using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace InceptionTools.Graphics
{
    /// <summary>Portable indexed PNG writer using the toolkit's standard EGA palette.</summary>
    public static class EgaIndexedPngEncoder
    {
        private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        private static readonly byte[] EgaPaletteRgb =
        {
            0x00, 0x00, 0x00,  0x00, 0x00, 0xAA,  0x00, 0xAA, 0x00,  0x00, 0xAA, 0xAA,
            0xAA, 0x00, 0x00,  0xAA, 0x00, 0xAA,  0xAA, 0x55, 0x00,  0xAA, 0xAA, 0xAA,
            0x55, 0x55, 0x55,  0x55, 0x55, 0xFF,  0x55, 0xFF, 0x55,  0x55, 0xFF, 0xFF,
            0xFF, 0x55, 0x55,  0xFF, 0x55, 0xFF,  0xFF, 0xFF, 0x55,  0xFF, 0xFF, 0xFF
        };

        public static byte[] Encode(int width, int height, byte[] paletteIndices,
            int? transparentPaletteIndex = null)
        {
            return Encode(width, height, paletteIndices, EgaPaletteRgb, transparentPaletteIndex);
        }

        public static byte[] Encode(int width, int height, byte[] paletteIndices,
            byte[] paletteRgb, int? transparentPaletteIndex = null)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (paletteIndices == null)
                throw new ArgumentNullException(nameof(paletteIndices));
            if (paletteRgb == null)
                throw new ArgumentNullException(nameof(paletteRgb));
            if (paletteRgb.Length != 16 * 3)
                throw new ArgumentException("An EGA palette must contain exactly 16 RGB triplets.",
                    nameof(paletteRgb));
            if (paletteIndices.Length != checked(width * height))
                throw new ArgumentException("Indexed pixel count does not match the image dimensions.",
                    nameof(paletteIndices));
            if (transparentPaletteIndex.HasValue &&
                (transparentPaletteIndex.Value < 0 || transparentPaletteIndex.Value >= 16))
                throw new ArgumentOutOfRangeException(nameof(transparentPaletteIndex));
            foreach (byte pixel in paletteIndices)
                if (pixel >= 16)
                    throw new InvalidDataException("EGA image contains palette index " + pixel +
                        "; expected a value in the range 0..15.");

            byte[] scanlines = new byte[(width + 1) * height];
            for (int y = 0; y < height; y++)
            {
                int scanlineOffset = y * (width + 1);
                scanlines[scanlineOffset] = 0;
                Array.Copy(paletteIndices, y * width, scanlines, scanlineOffset + 1, width);
            }

            byte[] imageData;
            using (var compressed = new MemoryStream())
            {
                using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, true))
                    zlib.Write(scanlines, 0, scanlines.Length);
                imageData = compressed.ToArray();
            }

            using (var png = new MemoryStream())
            {
                png.Write(Signature, 0, Signature.Length);
                byte[] header = new byte[13];
                WriteUInt32BigEndian(header, 0, (uint)width);
                WriteUInt32BigEndian(header, 4, (uint)height);
                header[8] = 8;
                header[9] = 3;
                WriteChunk(png, "IHDR", header);
                WriteChunk(png, "PLTE", paletteRgb);
                if (transparentPaletteIndex.HasValue)
                {
                    byte[] alpha = new byte[transparentPaletteIndex.Value + 1];
                    for (int index = 0; index < alpha.Length; index++)
                        alpha[index] = 0xFF;
                    alpha[transparentPaletteIndex.Value] = 0;
                    WriteChunk(png, "tRNS", alpha);
                }
                WriteChunk(png, "IDAT", imageData);
                WriteChunk(png, "IEND", Array.Empty<byte>());
                return png.ToArray();
            }
        }

        private static void WriteChunk(Stream output, string type, byte[] data)
        {
            byte[] typeBytes = Encoding.ASCII.GetBytes(type);
            byte[] length = new byte[4];
            WriteUInt32BigEndian(length, 0, (uint)data.Length);
            output.Write(length, 0, length.Length);
            output.Write(typeBytes, 0, typeBytes.Length);
            output.Write(data, 0, data.Length);

            uint crc = 0xFFFFFFFF;
            foreach (byte value in typeBytes)
                crc = UpdateCrc(crc, value);
            foreach (byte value in data)
                crc = UpdateCrc(crc, value);
            byte[] checksum = new byte[4];
            WriteUInt32BigEndian(checksum, 0, crc ^ 0xFFFFFFFF);
            output.Write(checksum, 0, checksum.Length);
        }

        private static uint UpdateCrc(uint crc, byte value)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
                crc = (crc & 1) != 0 ? 0xEDB88320U ^ (crc >> 1) : crc >> 1;
            return crc;
        }

        private static void WriteUInt32BigEndian(byte[] destination, int offset, uint value)
        {
            destination[offset] = (byte)(value >> 24);
            destination[offset + 1] = (byte)(value >> 16);
            destination[offset + 2] = (byte)(value >> 8);
            destination[offset + 3] = (byte)value;
        }
    }
}
