using System.Text;
using System.Text.Json;
using RevengeTools.Cli;
using RevengeTools.Installation;

namespace RevengeTools.Formats.Audio;

public static class DigitalSoundReports
{
    public static string Format(DigitalSoundCatalog catalog)
    {
        var output = new StringBuilder();
        output.AppendLine("index code resource file            source   length packed");
        foreach (DigitalSoundEntry entry in catalog.Entries)
            output.Append(entry.Index.ToString("X2")).Append("    ")
                .Append(entry.SoundCode.ToString("X2")).Append("   ")
                .Append(entry.ResourceIndex.ToString("X2")).Append("       ")
                .Append(entry.ResourceFileName.PadRight(15)).Append(' ')
                .Append("0x").Append(entry.SourceOffset.ToString("X6")).Append(' ')
                .Append("0x").Append(entry.SampleLength.ToString("X4")).Append(' ')
                .Append("0x").AppendLine(entry.PackedBufferOffset.ToString("X4"));
        output.AppendLine();
        output.AppendLine("DS offset kind                    codes           samples bytes duration");
        foreach (DigitalSoundSequenceFragment fragment in catalog.SequenceFragments)
            output.Append("DS:").Append(fragment.DataOffset.ToString("X4")).Append(' ')
                .Append(fragment.Kind.PadRight(23)).Append(' ')
                .Append(fragment.SoundCodes.PadRight(15)).Append(' ')
                .Append(fragment.Entries.Count.ToString().PadLeft(2)).Append("      0x")
                .Append(fragment.TotalSampleBytes.ToString("X5")).Append(' ')
                .Append(fragment.DurationSeconds.ToString("0.000")).AppendLine("s");
        return output.ToString();
    }

    public static void Export(DigitalSoundCatalog catalog, GameInstallation installation,
        string outputDirectory, bool overwrite)
    {
        foreach (DigitalSoundEntry entry in catalog.Entries)
        {
            byte[] sample = catalog.ReadSample(installation, entry);
            string stem = $"sample-{entry.Index:X2}-code-{entry.SoundCode:X2}";
            OutputFile.WriteBytes(Path.Combine(outputDirectory, stem + ".bin"), sample, overwrite);
            OutputFile.WriteBytes(Path.Combine(outputDirectory, stem + ".wav"),
                PcmWaveEncoder.EncodeUnsigned8BitMono(sample, DigitalSoundCatalog.PlaybackRateHz), overwrite);
        }

        var manifest = catalog.Entries.Select(entry => new
        {
            entry.Index,
            soundCode = $"0x{entry.SoundCode:X2}",
            entry.ResourceIndex,
            entry.ResourceFileName,
            entry.SourceOffset,
            entry.SampleLength,
            entry.PackedBufferOffset,
            playbackRateHz = DigitalSoundCatalog.PlaybackRateHz,
            encoding = "unsigned 8-bit PCM, mono",
            rawFile = $"sample-{entry.Index:X2}-code-{entry.SoundCode:X2}.bin",
            waveFile = $"sample-{entry.Index:X2}-code-{entry.SoundCode:X2}.wav"
        });
        OutputFile.WriteText(Path.Combine(outputDirectory, "manifest.json"),
            JsonSerializer.Serialize(new
            {
                playbackRateHz = DigitalSoundCatalog.PlaybackRateHz,
                unityPlaybackScale = $"0x{DigitalSoundCatalog.UnityPlaybackScale:X4}",
                samples = manifest,
                sequenceFragments = catalog.SequenceFragments.Select(fragment => new
                {
                    dataOffset = $"DS:{fragment.DataOffset:X4}",
                    fragment.Kind,
                    fragment.SoundCodes,
                    sampleIndexes = fragment.Entries.Select(entry => entry.Index),
                    sampleCodes = fragment.Entries.Select(entry => $"0x{entry.SoundCode:X2}"),
                    fragment.TotalSampleBytes,
                    fragment.DurationSeconds
                })
            }, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            overwrite);
        OutputFile.WriteText(Path.Combine(outputDirectory, "sequences.txt"), Format(catalog), overwrite);
    }
}
