using RevengeTools.Formats.Executables;

namespace RevengeTools.Formats.Graphics;

public sealed record CgaPixelTranslation(
    byte[] SeedTable,
    byte[] EvenPhaseLookup,
    byte[] OddPhaseLookup);

public static class CgaPixelTranslationCatalog
{
    public const ushort DataImageSegment = 0x326A;
    public const ushort SeedTableOffset = 0x03BC;
    public const int SeedTableLength = 0x20;

    public static CgaPixelTranslation Parse(byte[] executable,
        string sourceName = "REVENGE.EXE")
    {
        byte[] seed = MzExecutable.Parse(executable, sourceName).ReadImageBytes(
            DataImageSegment, SeedTableOffset, SeedTableLength,
            "CGA packed-pixel translation seed table");
        return Build(seed);
    }

    public static CgaPixelTranslation Build(ReadOnlySpan<byte> seedTable)
    {
        if (seedTable.Length != SeedTableLength)
            throw new InvalidDataException(
                $"CGA translation seed must contain 0x{SeedTableLength:X} bytes.");
        byte[] seed = seedTable.ToArray();
        byte[] even = new byte[0x100];
        byte[] odd = new byte[0x100];
        for (int packedPixels = 0; packedPixels <= 0xFF; ++packedPixels)
        {
            int highPixel = packedPixels >> 4;
            int lowPixel = packedPixels & 0x0F;
            even[packedPixels] = (byte)((seed[highPixel] << 2) | seed[0x10 + lowPixel]);
            odd[packedPixels] = (byte)((seed[0x10 + highPixel] << 2) | seed[lowPixel]);
        }
        return new(seed, even, odd);
    }

    public static byte BuildTransparencyMask(byte firstPackedPixels,
        byte secondPackedPixels)
    {
        int occupied = 0;
        if ((firstPackedPixels & 0xF0) != 0) occupied |= 0xC0;
        if ((firstPackedPixels & 0x0F) != 0) occupied |= 0x30;
        if ((secondPackedPixels & 0xF0) != 0) occupied |= 0x0C;
        if ((secondPackedPixels & 0x0F) != 0) occupied |= 0x03;
        return (byte)~occupied;
    }
}
