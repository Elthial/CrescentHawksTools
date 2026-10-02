namespace InceptionTools.Maps;

/// <summary>Preserves the original 207F procedural block construction at overview resolution.</summary>
public static class ProceduralWorldGenerator
{
    public const int RegionColumns = 16;
    public const int RegionRows = 16;
    public const int BlocksPerRegion = 8;
    public const int OverviewColumns = RegionColumns * BlocksPerRegion;
    public const int OverviewRows = RegionRows * BlocksPerRegion;

    private const int LatticeWidth = 9;
    private const int LatticeLength = LatticeWidth * LatticeWidth;
    private const byte Unfilled = 0xFF;
    private const byte TerrainMask = 0xF0;

    public static byte[] GeneratePacifica() =>
        Generate(PacificaWorldPreset.CreateSeedTable(), PacificaWorldPreset.WorldVertices);

    public static byte[] GenerateFromEditorSeed(uint seed) =>
        Generate(CreateEditorSeedTable(seed), PacificaWorldPreset.WorldVertices);

    public static byte[] CreateEditorSeedTable(uint seed) =>
        PacificaWorldPreset.CreateSeedTable(seed);

    public static byte[] Generate(byte[] constructionSeeds, byte[] worldVertices)
    {
        ArgumentNullException.ThrowIfNull(constructionSeeds);
        ArgumentNullException.ThrowIfNull(worldVertices);
        if (constructionSeeds.Length != PacificaWorldPreset.SeedTableLength)
            throw new ArgumentException("The construction-seed table must contain exactly 256 bytes.",
                nameof(constructionSeeds));
        if (worldVertices.Length != PacificaWorldPreset.WorldVertexCount)
            throw new ArgumentException("The world-vertex table must contain exactly 274 bytes.",
                nameof(worldVertices));

        var world = new byte[OverviewColumns * OverviewRows];
        var lattice = new byte[LatticeLength];
        var block = new byte[BlocksPerRegion * BlocksPerRegion];
        for (int regionY = 0; regionY < RegionRows; regionY++)
            for (int regionX = 0; regionX < RegionColumns; regionX++)
            {
                int region = regionY * RegionColumns + regionX;
                Array.Fill(lattice, Unfilled);
                lattice[0] = worldVertices[region];
                lattice[8] = worldVertices[region + 1];
                // Original 207F:18D8 uses the same 16-byte stride as the packed
                // world-region index. It is not a conventional 17-column vertex grid.
                lattice[72] = worldVertices[region + 16];
                lattice[80] = worldVertices[region + 17];
                BuildBlock(block, lattice, constructionSeeds);
                for (int row = 0; row < BlocksPerRegion; row++)
                    Array.Copy(block, row * BlocksPerRegion, world,
                        (regionY * BlocksPerRegion + row) * OverviewColumns + regionX * BlocksPerRegion,
                        BlocksPerRegion);
            }
        return world;
    }

    public static byte[] BuildOverviewTileIds(byte[] descriptors)
    {
        ArgumentNullException.ThrowIfNull(descriptors);
        if (descriptors.Length != OverviewColumns * OverviewRows)
            throw new ArgumentException("The descriptor map must be 128 by 128.", nameof(descriptors));
        var tiles = new byte[descriptors.Length];
        for (int y = 0; y < OverviewRows; y++)
            for (int x = 0; x < OverviewColumns; x++)
            {
                int index = y * OverviewColumns + x;
                byte terrain = descriptors[index];
                if (terrain == 0x10)
                {
                    tiles[index] = 0x40;
                    continue;
                }
                int flags = 0;
                if (y > 0 && descriptors[index - OverviewColumns] == terrain) flags |= 1;
                if (x < OverviewColumns - 1 && descriptors[index + 1] == terrain) flags |= 2;
                if (y < OverviewRows - 1 && descriptors[index + OverviewColumns] == terrain) flags |= 4;
                if (x > 0 && descriptors[index - 1] == terrain) flags |= 8;
                byte category = terrain >= 0x20 ? (byte)(terrain - 0x10) : terrain;
                tiles[index] = (byte)(category | flags);
            }
        return tiles;
    }

    private static void BuildBlock(byte[] output, byte[] lattice, byte[] seeds)
    {
        byte seedIndex = 0;
        (int First, int Last, bool Horizontal)[] edges =
        {
            (0, 8, true), (72, 80, true), (0, 72, false), (8, 80, false)
        };
        foreach ((int first, int last, bool horizontal) in edges)
        {
            seedIndex = unchecked((byte)(lattice[first] + lattice[last]));
            if (horizontal)
                SubdivideHorizontal(lattice, seeds, ref seedIndex, first, last);
            else
                SubdivideVertical(lattice, seeds, ref seedIndex, first, last);
        }
        SubdivideRectangle(lattice, seeds, ref seedIndex, 0, 8, 72, 80);
        for (int row = 0; row < BlocksPerRegion; row++)
            for (int column = 0; column < BlocksPerRegion; column++)
                output[row * BlocksPerRegion + column] =
                    (byte)(lattice[row * LatticeWidth + column] & TerrainMask);
    }

    private static void SubdivideHorizontal(byte[] lattice, byte[] seeds, ref byte seedIndex,
        int first, int last)
    {
        var stack = new Stack<(int First, int Last)>();
        stack.Push((first, last));
        while (stack.Count != 0)
        {
            (first, last) = stack.Pop();
            int span = last - first;
            if (span == 1) continue;
            int halfSpan = span >> 1;
            int midpoint = first + halfSpan;
            stack.Push((first, midpoint));
            stack.Push((midpoint, last));
            if (lattice[midpoint] == Unfilled)
            {
                int mean = (lattice[first] + lattice[last]) >> 1;
                int amplitude = unchecked((byte)(halfSpan << 1));
                int mask = unchecked((byte)((amplitude << 1) - 1));
                byte noise = unchecked((byte)((seeds[seedIndex] & mask) - amplitude));
                byte value = unchecked((byte)(mean + noise));
                lattice[midpoint] = value >= 0x80 ? (byte)0 : value;
                seedIndex++;
            }
        }
    }

    private static void SubdivideVertical(byte[] lattice, byte[] seeds, ref byte seedIndex,
        int first, int last)
    {
        var stack = new Stack<(int First, int Last)>();
        stack.Push((first, last));
        while (stack.Count != 0)
        {
            (first, last) = stack.Pop();
            int span = last - first;
            if (span == LatticeWidth) continue;
            int halfSpan = span >> 1;
            int midpoint = first + halfSpan;
            stack.Push((first, midpoint));
            stack.Push((midpoint, last));
            if (lattice[midpoint] == Unfilled)
            {
                int mean = (lattice[first] + lattice[last]) >> 1;
                int amplitude = halfSpan >> 3;
                if (amplitude == 9) amplitude--;
                amplitude = unchecked((byte)(amplitude << 1));
                int mask = unchecked((byte)((amplitude << 1) - 1));
                byte noise = unchecked((byte)((seeds[seedIndex] & mask) - amplitude));
                byte value = unchecked((byte)(mean + noise));
                lattice[midpoint] = value >= 0x80 ? (byte)0 : value;
                seedIndex++;
            }
        }
    }

    private static void SubdivideRectangle(byte[] lattice, byte[] seeds, ref byte seedIndex,
        int topLeft, int topRight, int bottomLeft, int bottomRight)
    {
        var stack = new Stack<(int Tl, int Tr, int Bl, int Br)>();
        stack.Push((topLeft, topRight, bottomLeft, bottomRight));
        while (stack.Count != 0)
        {
            (int tl, int tr, int bl, int br) = stack.Pop();
            if (tr - tl == 1) continue;
            int centre = tl + ((br - tl) >> 1);
            if (lattice[centre] == Unfilled)
                lattice[centre] = (byte)((lattice[tl] + lattice[tr] + lattice[bl] + lattice[br]) >> 2);
            int halfWidth = (tr - tl) >> 1;
            int halfHeight = (bl - tl) >> 1;
            int top = tl + halfWidth;
            int left = tl + halfHeight;
            int bottom = bl + halfWidth;
            int right = tr + halfHeight;
            ushort edgeMean = (ushort)((lattice[tl] + lattice[tr]) >> 1);
            FillRectangleEdge(lattice, seeds, ref seedIndex, top, edgeMean,
                halfWidth << 1, out _, out _);

            edgeMean = (ushort)((lattice[tl] + lattice[bl]) >> 1);
            if (FillRectangleEdge(lattice, seeds, ref seedIndex, left, edgeMean,
                VerticalAmplitude(halfHeight), out byte leftNoise, out byte leftValue))
                edgeMean = (ushort)((leftNoise << 8) | leftValue); // Native retained DH.

            edgeMean = (ushort)((((edgeMean & 0xFF00) | lattice[bl]) + lattice[br]) >> 1);
            if (FillRectangleEdge(lattice, seeds, ref seedIndex, bottom, edgeMean,
                halfWidth << 1, out _, out byte bottomValue))
                edgeMean = (ushort)((edgeMean & 0xFF00) | bottomValue);

            edgeMean = (ushort)((((edgeMean & 0xFF00) | lattice[tr]) + lattice[br]) >> 1);
            // The original right edge deliberately uses halfWidth for amplitude.
            FillRectangleEdge(lattice, seeds, ref seedIndex, right, edgeMean,
                VerticalAmplitude(halfWidth), out _, out _);

            // Original child order is TL, TR, BL, BR and the native LIFO stack visits BR first.
            stack.Push((tl, top, left, centre));
            stack.Push((top, tr, centre, right));
            stack.Push((left, centre, bl, bottom));
            stack.Push((centre, right, bottom, br));
        }
    }

    private static int VerticalAmplitude(int halfSpan)
    {
        int amplitude = halfSpan >> 3;
        if (amplitude == 9) amplitude--;
        return amplitude << 1;
    }

    private static bool FillRectangleEdge(byte[] lattice, byte[] seeds, ref byte seedIndex,
        int index, int mean, int amplitude, out byte noise, out byte value)
    {
        noise = 0;
        value = lattice[index];
        if (lattice[index] != Unfilled) return false;
        int byteAmplitude = unchecked((byte)amplitude);
        int mask = unchecked((byte)((byteAmplitude << 1) - 1));
        noise = unchecked((byte)((seeds[seedIndex] & mask) - byteAmplitude));
        value = unchecked((byte)(mean + noise));
        lattice[index] = value >= 0x80 ? (byte)0 : value;
        value = lattice[index];
        seedIndex++;
        return true;
    }
}
