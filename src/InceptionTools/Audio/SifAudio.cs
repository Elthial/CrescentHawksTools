using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using InceptionTools.Installation;

namespace InceptionTools.Audio
{
    public enum SifPlaybackMode
    {
        PcSpeaker,
        Tandy
    }

    public sealed class SifFrame
    {
        public int Index { get; set; }
        public byte[] NoteCodes { get; set; }
    }

    public sealed class SifFileRecord
    {
        public const string DefaultFileName = "WWOODBT.SIF";
        public byte[] Bytes { get; private set; }
        public IReadOnlyList<SifFrame> Frames { get; private set; }

        public static SifFileRecord Parse(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0 || bytes.Length % 4 != 0)
                throw new InvalidDataException("SIF data must be a nonempty sequence of four-byte channel frames.");

            byte[] copy = (byte[])bytes.Clone();
            var frames = new List<SifFrame>(copy.Length / 4);
            for (int offset = 0; offset < copy.Length; offset += 4)
                frames.Add(new SifFrame
                {
                    Index = offset / 4,
                    NoteCodes = copy.Skip(offset).Take(4).ToArray()
                });
            return new SifFileRecord { Bytes = copy, Frames = frames };
        }
    }

    public sealed class SifInspection
    {
        public string FileName { get; set; }
        public int FileLength { get; set; }
        public int FrameCount { get; set; }
        public int RestByteCount { get; set; }
        public int[] ActiveNotesPerChannel { get; set; }
        public int[] UniqueNoteCodes { get; set; }
        public double PcSpeakerDurationSeconds { get; set; }
        public double TandyDurationSeconds { get; set; }
    }

    public static class SifInspector
    {
        public static SifInspection Inspect(GameInstallation installation, string fileName = SifFileRecord.DefaultFileName)
        {
            string path = installation.ResolveFile(fileName);
            SifFileRecord record = SifFileRecord.Parse(File.ReadAllBytes(path));
            return new SifInspection
            {
                FileName = Path.GetFileName(path),
                FileLength = record.Bytes.Length,
                FrameCount = record.Frames.Count,
                RestByteCount = record.Bytes.Count(value => value == PcSpeakerPitch.RestCode),
                ActiveNotesPerChannel = Enumerable.Range(0, 4)
                    .Select(channel => record.Frames.Count(frame => frame.NoteCodes[channel] != PcSpeakerPitch.RestCode)).ToArray(),
                UniqueNoteCodes = record.Bytes.Where(value => value != PcSpeakerPitch.RestCode)
                    .Distinct().OrderBy(value => value).Select(value => (int)value).ToArray(),
                PcSpeakerDurationSeconds = record.Bytes.Length * 4 / PcSpeakerPitch.MusicInterruptRateHz,
                TandyDurationSeconds = record.Frames.Count * 20 / PcSpeakerPitch.MusicInterruptRateHz
            };
        }

        public static string WriteJson(SifInspection inspection) =>
            JsonSerializer.Serialize(inspection, new JsonSerializerOptions { WriteIndented = true });

        public static string WriteText(SifInspection inspection)
        {
            var output = new StringBuilder();
            output.AppendLine("SIF: " + inspection.FileName);
            output.AppendLine("Bytes: " + inspection.FileLength + " (headerless note stream)");
            output.AppendLine("Four-channel frames: " + inspection.FrameCount);
            output.AppendLine("Rest bytes (0x80): " + inspection.RestByteCount);
            output.AppendLine("Active notes by channel: " + string.Join(", ", inspection.ActiveNotesPerChannel));
            output.AppendLine("Unique note codes: " + string.Join(" ", inspection.UniqueNoteCodes.Select(value => "0x" + value.ToString("X2"))));
            output.AppendLine("PC-speaker duration: " + inspection.PcSpeakerDurationSeconds.ToString("F3") + " seconds");
            output.AppendLine("Tandy duration: " + inspection.TandyDurationSeconds.ToString("F3") + " seconds");
            return output.ToString();
        }
    }

    public static class SifRenderer
    {
        public const int DefaultSampleRate = 22050;

        public static short[] Render(SifFileRecord record, SifPlaybackMode mode, int sampleRate = DefaultSampleRate)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            var builder = new SquareWaveBuilder(sampleRate, mode == SifPlaybackMode.PcSpeaker ? 1 : 4);
            if (mode == SifPlaybackMode.PcSpeaker)
            {
                double duration = 4 / PcSpeakerPitch.MusicInterruptRateHz;
                foreach (byte note in record.Bytes)
                    builder.AddFrame(duration, PcSpeakerPitch.ToFrequencyHz(note));
            }
            else
            {
                double duration = 20 / PcSpeakerPitch.MusicInterruptRateHz;
                foreach (SifFrame frame in record.Frames)
                    builder.AddFrame(duration, frame.NoteCodes.Select(PcSpeakerPitch.ToFrequencyHz).ToArray());
            }
            return builder.ToArray();
        }

        public static byte[] RenderWave(SifFileRecord record, SifPlaybackMode mode, int sampleRate = DefaultSampleRate) =>
            PcmWaveEncoder.EncodeMono16(Render(record, mode, sampleRate), sampleRate);
    }

    public sealed class AudioExportResult
    {
        public string OutputPath { get; set; }
        public int OutputLength { get; set; }
        public double DurationSeconds { get; set; }
    }

    public static class AudioFileService
    {
        public static AudioExportResult ExportSifWave(GameInstallation installation, string fileName,
            SifPlaybackMode mode, string outputPath, bool overwrite = false)
        {
            string sourcePath = installation.ResolveFile(fileName);
            byte[] wave = SifRenderer.RenderWave(SifFileRecord.Parse(File.ReadAllBytes(sourcePath)), mode);
            return WriteWave(wave, outputPath, overwrite);
        }

        public static AudioExportResult WriteWave(byte[] wave, string outputPath, bool overwrite)
        {
            string fullPath = Path.GetFullPath(outputPath);
            if (File.Exists(fullPath) && !overwrite)
                throw new IOException("Output file already exists: " + fullPath + ". Use --force to overwrite it.");
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllBytes(fullPath, wave);
            int sampleRate = BitConverter.ToInt32(wave, 24);
            int dataLength = BitConverter.ToInt32(wave, 40);
            return new AudioExportResult
            {
                OutputPath = fullPath,
                OutputLength = wave.Length,
                DurationSeconds = dataLength / 2.0 / sampleRate
            };
        }
    }
}
