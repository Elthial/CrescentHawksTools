using System;
using System.IO;
using System.Text;

namespace InceptionTools.Audio
{
    public static class PcmWaveEncoder
    {
        public static byte[] EncodeMono16(short[] samples, int sampleRate)
        {
            if (samples == null)
                throw new ArgumentNullException(nameof(samples));
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate));

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                int dataLength = checked(samples.Length * 2);
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(checked(36 + dataLength));
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)1);
                writer.Write(sampleRate);
                writer.Write(checked(sampleRate * 2));
                writer.Write((ushort)2);
                writer.Write((ushort)16);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataLength);
                foreach (short sample in samples)
                    writer.Write(sample);
                writer.Flush();
                return stream.ToArray();
            }
        }
    }
}
