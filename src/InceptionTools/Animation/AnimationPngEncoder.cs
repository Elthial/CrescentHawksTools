using System;
using InceptionTools.Graphics;

namespace InceptionTools.Animation
{
    /// <summary>
    /// Writes an 88x88 ANM pixel frame as a portable indexed-colour PNG.
    /// ANM files contain palette indices, not palette RGB data, so this encoder
    /// uses the standard 16-colour EGA mapping used elsewhere in InceptionTools.
    /// </summary>
    public static class AnimationPngEncoder
    {
        public static byte[] Encode(AnimationPixelFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            return EgaIndexedPngEncoder.Encode(frame.Width, frame.Height, frame.PaletteIndices);
        }
    }
}
