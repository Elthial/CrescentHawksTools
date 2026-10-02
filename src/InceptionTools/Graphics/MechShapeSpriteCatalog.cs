using System;
using System.Collections.Generic;
using System.Linq;

namespace InceptionTools.Graphics
{
    public sealed class MechShapeSpriteSource
    {
        internal MechShapeSpriteSource(int id, int x, int y, int width, int height)
        {
            Id = id;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int Id { get; }
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
    }

    /// <summary>
    /// Exact 0D27:0410..082C rectangle map passed to 1F3D:070A while the
    /// original executable constructs its 376-entry MECHSHAP sprite table.
    /// </summary>
    public static class MechShapeSpriteCatalog
    {
        private static readonly IReadOnlyList<MechShapeSpriteSource> Entries = Build();

        public static IReadOnlyList<MechShapeSpriteSource> All => Entries;

        public static MechShapeSpriteSource Get(int id)
        {
            if (id < 0 || id >= Entries.Count)
                throw new ArgumentOutOfRangeException(nameof(id),
                    "MECHSHAP sprite ID must be in the range 0.." + (Entries.Count - 1) + ".");
            return Entries[id];
        }

        private static IReadOnlyList<MechShapeSpriteSource> Build()
        {
            var entries = new Dictionary<int, MechShapeSpriteSource>();
            for (int id = 0; id < 12; id++)
                Add(entries, id, id * 24, 0, 24, 24);
            for (int id = 12; id < 16; id++)
                Add(entries, id, (id - 12) * 24, 24, 24, 24);
            for (int id = 16; id < 36; id++)
                Add(entries, id, (id - 4) * 8, 24, 8, 8);

            for (int row = 9; row < 16; row++)
                for (int column = 12; column < 24; column++)
                    Add(entries, row * 12 + column - 0x54, column * 8, row * 8, 8, 8);

            Add(entries, 0x78, 0, 0x60, 24, 24);
            Add(entries, 0x79, 24, 0x60, 24, 24);
            Add(entries, 0x7A, 72, 0x78, 24, 24);
            Add(entries, 0x7B, 48, 0x78, 24, 24);
            Add(entries, 0x7C, 64, 0xA0, 16, 14);
            Add(entries, 0x7D, 80, 0xA0, 16, 14);
            Add(entries, 0x7E, 0, 0xA0, 8, 8);
            Add(entries, 0x7F, 8, 0xA0, 8, 8);
            Add(entries, 0x80, 64, 0x90, 16, 16);
            Add(entries, 0x81, 80, 0x90, 16, 16);

            for (int row = 0x12; row < 0x16; row++)
                for (int column = 4; column < 8; column++)
                    Add(entries, row * 4 + column + 0x36, column * 8, row * 8, 8, 8);

            for (int index = 0; index < 12; index++)
                Add(entries, 0x92 + index, index * 24, 0x30, 24, 24);
            for (int index = 0; index < 4; index++)
                Add(entries, 0x9E + index, index * 24, 0x48, 24, 24);

            Add(entries, 0xA2, 48, 0x60, 24, 24);
            Add(entries, 0xA3, 72, 0x60, 24, 24);
            Add(entries, 0xA4, 0, 0x78, 24, 24);
            Add(entries, 0xA5, 24, 0x78, 24, 24);

            for (int id = 0xA6; id < 0xBA; id++)
                Add(entries, id, (id - 0x9A) * 8, 0x20, 8, 8);

            for (int row = 9; row < 16; row++)
                for (int column = 0x18; column < 0x24; column++)
                    Add(entries, row * 12 + column + 0x36, column * 8, row * 8, 8, 8);

            for (int id = 0x10A; id < 0x11E; id++)
                Add(entries, id + 4, (id - 0xFE) * 8, 0x28, 8, 8);

            for (int row = 0x10; row < 0x17; row++)
                for (int column = 0x0C; column < 0x18; column++)
                    Add(entries, row * 12 + column + 0x56, column * 8, row * 8, 8, 8);

            Add(entries, 0x176, 0, 0x90, 16, 11);
            Add(entries, 0x177, 16, 0x90, 16, 11);

            if (entries.Count != 0x178 || entries.Keys.Min() != 0 || entries.Keys.Max() != 0x177)
                throw new InvalidOperationException("MECHSHAP catalog must cover every sprite ID 0x000..0x177.");
            return entries.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList().AsReadOnly();
        }

        private static void Add(Dictionary<int, MechShapeSpriteSource> entries,
            int id, int x, int y, int width, int height)
        {
            if (entries.ContainsKey(id))
                throw new InvalidOperationException("Duplicate MECHSHAP sprite ID 0x" + id.ToString("X") + ".");
            if (x < 0 || y < 0 || width <= 0 || height <= 0 ||
                x + width > CompressedImage.Width || y + height > CompressedImage.Height)
                throw new InvalidOperationException("MECHSHAP sprite ID 0x" + id.ToString("X") +
                    " has an invalid source rectangle.");
            entries.Add(id, new MechShapeSpriteSource(id, x, y, width, height));
        }
    }
}
