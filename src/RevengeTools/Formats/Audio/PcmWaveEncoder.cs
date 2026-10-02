using System.Buffers.Binary;

namespace RevengeTools.Formats.Audio;

/// <summary>Builds a canonical mono, unsigned 8-bit PCM RIFF/WAVE file.</summary>
public static class PcmWaveEncoder
{
    public const ushort FormatPcm = 0x0001;
    public const ushort ChannelCount = 0x0001;
    public const ushort BitsPerSample = 0x0008;
    public const ushort BlockAlignment = 0x0001;

    public static byte[] EncodeUnsigned8BitMono(ReadOnlySpan<byte> samples, int sampleRateHz)
    {
        if (sampleRateHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRateHz));

        int paddingLength = samples.Length & 0x01;
        byte[] wave = new byte[checked(0x2C + samples.Length + paddingLength)];
        "RIFF"u8.CopyTo(wave.AsSpan(0x00));
        BinaryPrimitives.WriteUInt32LittleEndian(wave.AsSpan(0x04), checked((uint)(wave.Length - 0x08)));
        "WAVE"u8.CopyTo(wave.AsSpan(0x08));
        "fmt "u8.CopyTo(wave.AsSpan(0x0C));
        BinaryPrimitives.WriteUInt32LittleEndian(wave.AsSpan(0x10), 0x10);
        BinaryPrimitives.WriteUInt16LittleEndian(wave.AsSpan(0x14), FormatPcm);
        BinaryPrimitives.WriteUInt16LittleEndian(wave.AsSpan(0x16), ChannelCount);
        BinaryPrimitives.WriteUInt32LittleEndian(wave.AsSpan(0x18), checked((uint)sampleRateHz));
        BinaryPrimitives.WriteUInt32LittleEndian(wave.AsSpan(0x1C), checked((uint)(sampleRateHz * BlockAlignment)));
        BinaryPrimitives.WriteUInt16LittleEndian(wave.AsSpan(0x20), BlockAlignment);
        BinaryPrimitives.WriteUInt16LittleEndian(wave.AsSpan(0x22), BitsPerSample);
        "data"u8.CopyTo(wave.AsSpan(0x24));
        BinaryPrimitives.WriteUInt32LittleEndian(wave.AsSpan(0x28), checked((uint)samples.Length));
        samples.CopyTo(wave.AsSpan(0x2C));
        return wave;
    }
}
