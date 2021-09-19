using System.Collections.Generic;
using System.Drawing;

namespace InceptionTools.Graphics
{
    class Palette
    {
        public Palette()
        {
            palette = EGADefault();
        }

        private List<PaletteColour> palette;
        public bool CustomPalette { private set; get; }

        private List<PaletteColour> EGADefault()
        {
            CustomPalette = false;
            return new List<PaletteColour>
            {
                new PaletteColour("Black", 0x00, 0x00, 0x00),
                new PaletteColour("Blue", 0x00, 0x00, 0xAA),
                new PaletteColour("Green", 0x00, 0xAA, 0x00),
                new PaletteColour("Cyan", 0x00, 0xAA, 0xAA),
                new PaletteColour("Red", 0xAA, 0x00, 0x00),
                new PaletteColour("Magenta", 0xAA, 0x00, 0xAA),
                new PaletteColour("Yellow / Brown", 0xAA, 0x55, 0x00),
                new PaletteColour("White / Light Grey", 0xAA, 0xAA, 0xAA),
                new PaletteColour("Dark Gray / Bright Black", 0x55, 0x55, 0x55),
                new PaletteColour("Bright Blue", 0x55, 0x55, 0xFF),
                new PaletteColour("Bright Green", 0x55, 0xFF, 0x55),
                new PaletteColour("Bright Cyan", 0x55, 0xFF, 0xFF),
                new PaletteColour("Bright Red", 0xFF, 0x55, 0x55),
                new PaletteColour("Bright Magenta", 0xFF, 0x55, 0xFF),
                new PaletteColour("Bright Yellow", 0xFF, 0xFF, 0x55),
                new PaletteColour("Bright White", 0xFF, 0xFF, 0xFF)
            };
        }
        public Color GetColour(int PixelValue)
        {
            return palette[PixelValue].Colour;
        }
        public void SwapColour(int Index, PaletteColour SwapColor)
        {
            CustomPalette = true;
            palette[Index] = SwapColor;
        }
        public void ResetToDefaultPalette()
        {
            palette = EGADefault();
        }
    }
}