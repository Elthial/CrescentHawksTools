using System.Drawing;

namespace InceptionTools.Graphics
{
    class PaletteColour
    {
        public PaletteColour(string Name, int Red, int Green, int Blue)
        {
            name = Name;
            Colour = Color.FromArgb(Red, Green, Blue);
        }
        readonly string name;
        public readonly Color Colour;
    }
}