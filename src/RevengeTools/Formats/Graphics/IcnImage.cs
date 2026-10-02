namespace RevengeTools.Formats.Graphics;

/// <summary>Revenge ICN files are 250 sequential 16x16 four-bit tiles in the older Westwood envelope.</summary>
public sealed class IcnImage
{
    private readonly CmpImage _container;

    private IcnImage(CmpImage container) => _container = container;

    public const int TileWidth = 16;
    public const int TileHeight = 16;
    public const int TileCount = CmpImage.Width * CmpImage.Height / (TileWidth * TileHeight);
    public int StoredLength => _container.StoredLength;
    public int CompressionType => _container.CompressionType;
    public string CompressionName => _container.CompressionName;
    public int ConsumedPayloadBytes => _container.ConsumedPayloadBytes;
    public int PayloadLength => _container.PayloadLength;
    public int RemainingPayloadBytes => _container.RemainingPayloadBytes;
    public int TrailingBytes => _container.TrailingBytes;
    public byte[] TilePixels => _container.Pixels;

    public static IcnImage Decode(byte[] data, string sourceName = "ICN image")
    {
        CmpImage decoded = CmpImage.Decode(data, sourceName);
        if (decoded.CompressionType != 1)
            throw new InvalidDataException($"{sourceName} uses ICN compression type {decoded.CompressionType}; Revenge's local tile sets use type 1.");
        if (decoded.RemainingPayloadBytes != 0)
            throw new InvalidDataException($"{sourceName} leaves 0x{decoded.RemainingPayloadBytes:X} bytes inside its declared compressed stream.");
        return new IcnImage(decoded);
    }

    public byte[] RenderContactSheet(int columns, out int width, out int height)
    {
        if (columns is < 1 or > TileCount) throw new ArgumentOutOfRangeException(nameof(columns));
        int rows = (TileCount + columns - 1) / columns;
        width = checked(columns * TileWidth);
        height = checked(rows * TileHeight);
        byte[] output = new byte[checked(width * height)];
        byte[] source = TilePixels;
        for (int tile = 0; tile < TileCount; tile++)
        {
            int targetX = tile % columns * TileWidth;
            int targetY = tile / columns * TileHeight;
            int sourceOffset = tile * TileWidth * TileHeight;
            for (int y = 0; y < TileHeight; y++)
                Buffer.BlockCopy(source, sourceOffset + y * TileWidth,
                    output, (targetY + y) * width + targetX, TileWidth);
        }
        return output;
    }
}
