using System;
using System.Collections.Generic;
using System.Linq;

namespace InceptionTools.Graphics;

public sealed class MechSpriteSheetFrame
{
    internal MechSpriteSheetFrame(int frameIndex, MechShapeSpriteSource source,
        int x, int y, int width, int height, int contentX, int contentY)
    {
        FrameIndex = frameIndex;
        SourceSpriteId = source.Id;
        SourceX = source.X;
        SourceY = source.Y;
        SourceWidth = source.Width;
        SourceHeight = source.Height;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        ContentX = contentX;
        ContentY = contentY;
    }

    public int FrameIndex { get; }
    public int SourceSpriteId { get; }
    public int SourceX { get; }
    public int SourceY { get; }
    public int SourceWidth { get; }
    public int SourceHeight { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public int ContentX { get; }
    public int ContentY { get; }
}

public sealed class MechSpriteSheetSequence
{
    internal MechSpriteSheetSequence(string name, string category, int row,
        List<MechSpriteSheetFrame> frames)
    {
        Name = name;
        Category = category;
        Row = row;
        Frames = frames.AsReadOnly();
    }

    public string Name { get; }
    public string Category { get; }
    public int Row { get; }
    public IReadOnlyList<MechSpriteSheetFrame> Frames { get; }
}

public sealed class MechSpriteSheet
{
    private readonly byte[] _paletteIndices;

    internal MechSpriteSheet(int width, int height, int cellWidth, int cellHeight,
        List<MechSpriteSheetSequence> sequences, byte[] paletteIndices)
    {
        Width = width;
        Height = height;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        Sequences = sequences.AsReadOnly();
        _paletteIndices = (byte[])paletteIndices.Clone();
    }

    public int Width { get; }
    public int Height { get; }
    public int CellWidth { get; }
    public int CellHeight { get; }
    public IReadOnlyList<MechSpriteSheetSequence> Sequences { get; }
    public byte[] PaletteIndices => (byte[])_paletteIndices.Clone();
}

/// <summary>
/// Rearranges MECHSHAP into uniform 24x24 cells. Mech rows follow the
/// executable's real direction-specific control streams; effects and the
/// personnel sets are retained in their source groups. Their palette roles
/// identify teammates, Jason Youngblood, and the shared enemy/civilian pool.
/// </summary>
public static class MechSpriteSheetComposer
{
    public const int CellWidth = 24;
    public const int CellHeight = 24;
    public const int TransparentPaletteIndex = 0;

    private static readonly string[] Directions =
        { "north", "northeast", "east", "southeast", "south", "southwest", "west", "northwest" };

    public static MechSpriteSheet Compose(CompressedImage source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        IReadOnlyList<SequenceDefinition> definitions = BuildDefinitions();
        int columns = definitions.Max(definition => definition.SpriteIds.Length);
        int width = columns * CellWidth;
        int height = definitions.Count * CellHeight;
        byte[] sourcePixels = source.PaletteIndices;
        byte[] sheetPixels = new byte[width * height];
        var sequences = new List<MechSpriteSheetSequence>();

        for (int row = 0; row < definitions.Count; row++)
        {
            SequenceDefinition definition = definitions[row];
            var frames = new List<MechSpriteSheetFrame>();
            for (int frameIndex = 0; frameIndex < definition.SpriteIds.Length; frameIndex++)
            {
                MechShapeSpriteSource sprite = MechShapeSpriteCatalog.Get(definition.SpriteIds[frameIndex]);
                int cellX = frameIndex * CellWidth;
                int cellY = row * CellHeight;
                int contentX = cellX + (CellWidth - sprite.Width) / 2;
                int contentY = cellY + CellHeight - sprite.Height;
                CopyRectangle(sourcePixels, sheetPixels, width, sprite, contentX, contentY);
                frames.Add(new MechSpriteSheetFrame(frameIndex, sprite, cellX, cellY,
                    CellWidth, CellHeight, contentX, contentY));
            }
            sequences.Add(new MechSpriteSheetSequence(definition.Name, definition.Category, row, frames));
        }

        return new MechSpriteSheet(width, height, CellWidth, CellHeight, sequences, sheetPixels);
    }

    private static void CopyRectangle(byte[] source, byte[] destination, int destinationWidth,
        MechShapeSpriteSource sprite, int destinationX, int destinationY)
    {
        for (int y = 0; y < sprite.Height; y++)
            Array.Copy(source, (sprite.Y + y) * CompressedImage.Width + sprite.X,
                destination, (destinationY + y) * destinationWidth + destinationX, sprite.Width);
    }

    private static IReadOnlyList<SequenceDefinition> BuildDefinitions()
    {
        var definitions = new List<SequenceDefinition>();
        int[][] walk =
        {
            new[] { 0, 1, 2, 3 }, new[] { 4, 5, 6, 7 },
            new[] { 6, 7, 4, 5 }, new[] { 5, 6, 7, 4 },
            new[] { 8, 9, 10, 11 }, new[] { 12, 13, 14, 15 },
            new[] { 15, 12, 13, 14 }, new[] { 14, 15, 12, 13 }
        };
        int[][] locustFire =
        {
            new[] { 0, 2 }, new[] { 0, 0x78, 0x79 }, new[] { 0, 0x78, 0x79 }, new[] { 0, 0x78, 0x79 },
            new[] { 0, 8, 10 }, new[] { 0, 0x7A, 0x7B }, new[] { 0, 0x7A, 0x7B }, new[] { 0, 0x7A, 0x7B }
        };
        int[][] locustKick =
        {
            new[] { 0, 2, 3 }, new[] { 6, 7 }, new[] { 6, 7 }, new[] { 6, 7 },
            new[] { 10, 11 }, new[] { 14, 15 }, new[] { 14, 15 }, new[] { 14, 15 }
        };
        int[][] commandoFireRelative =
        {
            new[] { 17 }, new[] { 6, 18 }, new[] { 6, 18 }, new[] { 6, 18 },
            new[] { 16 }, new[] { 13, 19 }, new[] { 13, 19 }, new[] { 13, 19 }
        };
        int[][] commandoKickRelative =
        {
            new[] { 17, 3 }, new[] { 5, 6 }, new[] { 5, 6 }, new[] { 5, 6 },
            new[] { 16, 9 }, new[] { 13, 14 }, new[] { 13, 14 }, new[] { 13, 14 }
        };

        AddDirectional(definitions, "locust.walk", "locust", walk);
        AddDirectional(definitions, "locust.fire", "locust", locustFire);
        AddDirectional(definitions, "locust.kick", "locust", locustKick);
        AddDirectional(definitions, "commando.walk", "commando", AddBase(walk, 0x92));
        AddDirectional(definitions, "commando.fire", "commando", AddBase(commandoFireRelative, 0x92));
        AddDirectional(definitions, "commando.kick", "commando", AddBase(commandoKickRelative, 0x92));

        definitions.Add(new SequenceDefinition("effects.debris", "effects", Range(0x82, 16)));
        definitions.Add(new SequenceDefinition("effects.fire", "effects", new[] { 0x7C, 0x7D }));
        definitions.Add(new SequenceDefinition("effects.impact", "effects", new[] { 0x7E, 0x7F }));
        definitions.Add(new SequenceDefinition("effects.wreckage", "effects", new[] { 0x80, 0x81 }));
        definitions.Add(new SequenceDefinition("infantry.fallen", "infantry", new[] { 0x176, 0x177 }));

        AddPersonnelSet(definitions, "teammates", "teammates", 0x10, 0x24);
        AddPersonnelSet(definitions, "jason-youngblood", "player", 0xA6, 0xBA);
        AddPersonnelSet(definitions, "enemies-or-civilians", "non-player", 0x10E, 0x122);

        var included = new HashSet<int>(definitions.SelectMany(definition => definition.SpriteIds));
        if (included.Count != MechShapeSpriteCatalog.All.Count)
            throw new InvalidOperationException("Spritesheet definitions must include every MECHSHAP sprite ID.");
        return definitions.AsReadOnly();
    }

    private static void AddDirectional(List<SequenceDefinition> definitions,
        string prefix, string category, int[][] frames)
    {
        for (int index = 0; index < Directions.Length; index++)
            definitions.Add(new SequenceDefinition(prefix + "." + Directions[index], category, frames[index]));
    }

    private static int[][] AddBase(int[][] sequences, int baseId)
    {
        return sequences.Select(sequence => sequence.Select(id => id + baseId).ToArray()).ToArray();
    }

    private static void AddPersonnelSet(List<SequenceDefinition> definitions,
        string prefix, string category, int coreStart, int extendedStart)
    {
        definitions.Add(new SequenceDefinition(prefix + ".core", category, Range(coreStart, 20)));
        for (int row = 0; row < 7; row++)
            definitions.Add(new SequenceDefinition(prefix + ".extended-" + row,
                category, Range(extendedStart + row * 12, 12)));
    }

    private static int[] Range(int start, int count)
    {
        return Enumerable.Range(start, count).ToArray();
    }

    private sealed class SequenceDefinition
    {
        internal SequenceDefinition(string name, string category, int[] spriteIds)
        {
            Name = name;
            Category = category;
            SpriteIds = spriteIds;
        }

        internal string Name { get; }
        internal string Category { get; }
        internal int[] SpriteIds { get; }
    }
}
