using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InceptionTools.Animation;

/// <summary>
/// Dependency-free GIF89a writer for complete 16-colour ANM frames.
/// Each image is a full replacement frame and the NETSCAPE extension makes
/// the result loop indefinitely.
/// </summary>
public static class AnimationGifEncoder
{
    private const int PaletteSize = 16;
    private const int LzwMinimumCodeSize = 4;
    private const int ClearCode = 16;
    private const int EndCode = 17;
    private const int FixedCodeSize = 5;
    private const int LiteralsPerClear = 14;

    private static readonly byte[] EgaPaletteRgb =
    {
        0x00, 0x00, 0x00,  0x00, 0x00, 0xAA,  0x00, 0xAA, 0x00,  0x00, 0xAA, 0xAA,
        0xAA, 0x00, 0x00,  0xAA, 0x00, 0xAA,  0xAA, 0x55, 0x00,  0xAA, 0xAA, 0xAA,
        0x55, 0x55, 0x55,  0x55, 0x55, 0xFF,  0x55, 0xFF, 0x55,  0x55, 0xFF, 0xFF,
        0xFF, 0x55, 0x55,  0xFF, 0x55, 0xFF,  0xFF, 0xFF, 0x55,  0xFF, 0xFF, 0xFF
    };

    public static byte[] Encode(IReadOnlyList<AnimationPixelFrame> frames,
        IReadOnlyList<int> delayCentiseconds)
    {
        if (frames == null)
            throw new ArgumentNullException(nameof(frames));
        if (delayCentiseconds == null)
            throw new ArgumentNullException(nameof(delayCentiseconds));
        if (frames.Count == 0)
            throw new ArgumentException("At least one animation frame is required.", nameof(frames));
        if (frames.Count != delayCentiseconds.Count)
            throw new ArgumentException("The frame and delay counts must match.", nameof(delayCentiseconds));

        using (var gif = new MemoryStream())
        {
            WriteAscii(gif, "GIF89a");
            WriteUInt16(gif, frames[0].Width);
            WriteUInt16(gif, frames[0].Height);
            gif.WriteByte(0xB3); // global 16-entry palette, four-bit colour resolution
            gif.WriteByte(0);
            gif.WriteByte(0);
            gif.Write(EgaPaletteRgb, 0, EgaPaletteRgb.Length);
            WriteLoopExtension(gif);

            for (int index = 0; index < frames.Count; index++)
                WriteFrame(gif, frames[index], delayCentiseconds[index],
                    frames[0].Width, frames[0].Height);

            gif.WriteByte(0x3B);
            return gif.ToArray();
        }
    }

    private static void WriteLoopExtension(Stream output)
    {
        output.WriteByte(0x21);
        output.WriteByte(0xFF);
        output.WriteByte(11);
        WriteAscii(output, "NETSCAPE2.0");
        output.WriteByte(3);
        output.WriteByte(1);
        WriteUInt16(output, 0); // zero means loop forever
        output.WriteByte(0);
    }

    private static void WriteFrame(Stream output, AnimationPixelFrame frame, int delayCentiseconds,
        int expectedWidth, int expectedHeight)
    {
        if (frame == null)
            throw new ArgumentException("Animation frames cannot contain null entries.", nameof(frame));
        if (frame.Width != expectedWidth || frame.Height != expectedHeight)
            throw new ArgumentException("Every animation frame must have identical dimensions.", nameof(frame));
        if (delayCentiseconds < 0 || delayCentiseconds > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(delayCentiseconds),
                "GIF frame delays must fit an unsigned 16-bit centisecond value.");

        byte[] pixels = frame.PaletteIndices;
        foreach (byte pixel in pixels)
            if (pixel >= PaletteSize)
                throw new InvalidDataException("GIF frame contains palette index " + pixel +
                    "; ANM pixels must be in the range 0..15.");

        output.WriteByte(0x21);
        output.WriteByte(0xF9);
        output.WriteByte(4);
        output.WriteByte(0x04); // retain the complete frame until its successor
        WriteUInt16(output, delayCentiseconds);
        output.WriteByte(0);
        output.WriteByte(0);

        output.WriteByte(0x2C);
        WriteUInt16(output, 0);
        WriteUInt16(output, 0);
        WriteUInt16(output, frame.Width);
        WriteUInt16(output, frame.Height);
        output.WriteByte(0);
        output.WriteByte(LzwMinimumCodeSize);
        WriteSubBlocks(output, EncodeLiteralLzw(pixels));
    }

    private static byte[] EncodeLiteralLzw(byte[] pixels)
    {
        // Frequent clear codes deliberately keep every code five bits wide.
        // This is larger than dictionary compression but tiny for 88x88
        // frames, simple to audit, and accepted by ordinary GIF decoders.
        using (var packed = new MemoryStream())
        {
            int bitBuffer = 0;
            int bitCount = 0;
            int offset = 0;
            while (offset < pixels.Length)
            {
                WriteCode(packed, ClearCode, ref bitBuffer, ref bitCount);
                int count = Math.Min(LiteralsPerClear, pixels.Length - offset);
                for (int index = 0; index < count; index++)
                    WriteCode(packed, pixels[offset++], ref bitBuffer, ref bitCount);
            }
            WriteCode(packed, EndCode, ref bitBuffer, ref bitCount);
            if (bitCount > 0)
                packed.WriteByte((byte)bitBuffer);
            return packed.ToArray();
        }
    }

    private static void WriteCode(Stream output, int code, ref int bitBuffer, ref int bitCount)
    {
        bitBuffer |= code << bitCount;
        bitCount += FixedCodeSize;
        while (bitCount >= 8)
        {
            output.WriteByte((byte)bitBuffer);
            bitBuffer >>= 8;
            bitCount -= 8;
        }
    }

    private static void WriteSubBlocks(Stream output, byte[] data)
    {
        int offset = 0;
        while (offset < data.Length)
        {
            int count = Math.Min(255, data.Length - offset);
            output.WriteByte((byte)count);
            output.Write(data, offset, count);
            offset += count;
        }
        output.WriteByte(0);
    }

    private static void WriteAscii(Stream output, string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        output.Write(bytes, 0, bytes.Length);
    }

    private static void WriteUInt16(Stream output, int value)
    {
        output.WriteByte((byte)value);
        output.WriteByte((byte)(value >> 8));
    }
}
