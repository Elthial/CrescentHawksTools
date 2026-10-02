using System.Text;
using System.Text.Json;
using CrescentHawksTools.Cli;

namespace RevengeTools.Formats.Music;

public static class MusicStreamReports
{
    public static string Format(MusicStream stream)
    {
        var output = new StringBuilder();
        output.Append("Stream 0x").Append(stream.Index.ToString("X2")).Append(" at ")
            .Append(stream.ImageSegment.ToString("X4")).Append(':')
            .AppendLine(stream.ImageOffset.ToString("X4"));
        output.AppendLine("offset raw      instruction");
        foreach (MusicStreamInstruction instruction in stream.Instructions)
        {
            output.Append("+").Append(instruction.Offset.ToString("X4")).Append(' ')
                .Append(instruction.RawHex.PadRight(8)).Append(' ')
                .Append(instruction.Name);
            if (instruction.Value is int value)
                output.Append(" value=0x").Append(value.ToString("X"));
            if (instruction.DurationUnits is int duration)
                output.Append(" durationUnits=").Append(duration);
            if (instruction.BranchTarget is int target)
                output.Append(" target=+0x").Append(target.ToString("X4"));
            if (instruction.PitDivisor is int divisor)
            {
                output.Append(" pitDivisor=").Append(divisor);
                if (divisor == 0)
                    output.Append(" rest");
                else
                    output.Append(" approxHz=").Append(MusicStreamCatalog.PitInputFrequencyHz / divisor);
            }
            output.AppendLine();
        }
        return output.ToString();
    }

    public static void Export(MusicStreamCatalog catalog, string outputDirectory, bool overwrite)
    {
        foreach (MusicStream stream in catalog.Streams)
        {
            OutputFile.WriteText(Path.Combine(outputDirectory, $"stream-{stream.Index:X2}.txt"),
                Format(stream), overwrite);
            MusicPlaybackTrace playback = MusicPlaybackSimulator.Simulate(stream, catalog.NoteDivisors);
            OutputFile.WriteText(Path.Combine(outputDirectory, $"stream-{stream.Index:X2}-playback.txt"),
                FormatPlayback(stream, playback), overwrite);
        }
        object manifest = catalog.Streams.Select(stream =>
        {
            MusicPlaybackTrace playback = MusicPlaybackSimulator.Simulate(stream, catalog.NoteDivisors);
            return new
            {
                stream.Index,
                address = $"{stream.ImageSegment:X4}:{stream.ImageOffset:X4}",
                length = stream.RawBytes.Length,
                instructionCount = stream.Instructions.Count,
                noteEventCount = stream.Instructions.Count(item => item.Name == "NoteOrRest"),
                expandedPlaybackEventCount = playback.Events.Count,
                expandedPlaybackTicks = playback.TotalTicks,
                commands = stream.Instructions.Where(item => item.Opcode > 0x5F)
                    .GroupBy(item => item.Name).ToDictionary(group => group.Key, group => group.Count())
            };
        });
        OutputFile.WriteText(Path.Combine(outputDirectory, "manifest.json"),
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            overwrite);
    }

    public static string FormatPlayback(MusicStream stream, MusicPlaybackTrace playback)
    {
        var output = new StringBuilder();
        output.Append("Expanded playback for stream 0x").AppendLine(stream.Index.ToString("X2"));
        output.Append("events=").Append(playback.Events.Count)
            .Append(" ticks=").Append(playback.TotalTicks)
            .Append(" executedInstructions=").AppendLine(playback.ExecutedInstructionCount.ToString());
        output.AppendLine("event source startTick ticks scale duration divisor frequency");
        foreach (MusicPlaybackEvent item in playback.Events)
        {
            output.Append(item.EventIndex.ToString("D4")).Append(" +")
                .Append(item.SourceOffset.ToString("X4")).Append(' ')
                .Append(item.StartTick.ToString().PadLeft(9)).Append(' ')
                .Append(item.EffectiveTicks.ToString().PadLeft(5)).Append(' ')
                .Append(item.DurationScale.ToString().PadLeft(5)).Append(' ')
                .Append(item.DurationUnits.ToString().PadLeft(8)).Append(' ')
                .Append(item.PitDivisor.ToString().PadLeft(7)).Append(' ')
                .AppendLine(item.IsRest ? "rest" : $"~{item.ApproximateFrequencyHz}Hz");
        }
        return output.ToString();
    }
}
