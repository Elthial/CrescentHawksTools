using System;
using System.IO;
using InceptionTools.Records;

namespace InceptionTools.Animation;

public sealed class AnimationPixelFrame
{
    private readonly byte[] _paletteIndices;

    internal AnimationPixelFrame(byte[] paletteIndices)
    {
        _paletteIndices = (byte[])paletteIndices.Clone();
    }

    public int Width => AnmFileRecord.FrameWidth;
    public int Height => AnmFileRecord.FrameHeight;
    public byte[] PaletteIndices => (byte[])_paletteIndices.Clone();
}

/// <summary>
/// Expands an accumulated ANM frame from two four-bit pixels per byte to
/// one EGA palette index per byte. This is the state before the original
/// executable transposes the pixels into hardware plane bytes.
/// </summary>
public static class AnimationPixelDecoder
{
    public const int PixelCount = AnmFileRecord.FrameWidth * AnmFileRecord.FrameHeight;

    public static AnimationPixelFrame Decode(AnimationFrame frame)
    {
        if (frame == null)
            throw new ArgumentNullException(nameof(frame));
        return Decode(frame.PackedBytes);
    }

    public static AnimationPixelFrame Decode(byte[] packedBytes)
    {
        if (packedBytes == null)
            throw new ArgumentNullException(nameof(packedBytes));
        if (packedBytes.Length != AnmFileRecord.PackedFrameLength)
            throw new InvalidDataException("ANM pixel conversion requires exactly 0x" +
                AnmFileRecord.PackedFrameLength.ToString("X") + " packed bytes; received 0x" +
                packedBytes.Length.ToString("X") + ".");

        byte[] paletteIndices = new byte[PixelCount];
        int destination = 0;
        foreach (byte packed in packedBytes)
        {
            paletteIndices[destination++] = (byte)(packed >> 4);
            paletteIndices[destination++] = (byte)(packed & 0x0F);
        }

        return new AnimationPixelFrame(paletteIndices);
    }
}
