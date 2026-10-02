using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using InceptionTools.Animation;
using InceptionTools.Installation;
using InceptionTools.Records;

namespace InceptionTools.Inspection
{
    public sealed class AnimationInspection
    {
        public string FileName { get; set; }
        public int FileLength { get; set; }
        public int HeaderLength { get; set; }
        public int PlaybackControlLength { get; set; }
        public int HeaderTrailerLength { get; set; }
        public int CompressedFrameStreamLength { get; set; }
        public bool FileLengthIsMultipleOf128 { get; set; }
        public bool HasPlaybackTerminator { get; set; }
        public int PlaybackControlEntryCount { get; set; }
        public int PostTerminatorNonZeroPlaybackByteCount { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public int PackedFrameLength { get; set; }
        public int[] PlaybackControlBytes { get; set; }
        public int[] HeaderTrailerBytes { get; set; }
        public int[] TimingTableBytes { get; set; }
        public int TimingScaleValue { get; set; }
        public int DecodedFrameCount { get; set; }
        public int ConsumedCompressedBytes { get; set; }
        public int RemainingCompressedBytes { get; set; }
        public AnimationFrameInspection[] Frames { get; set; }
    }

    public sealed class AnimationFrameInspection
    {
        public int Index { get; set; }
        public int ControlValue { get; set; }
        public int CompressedOffset { get; set; }
        public int CompressedLength { get; set; }
        public int TokenCount { get; set; }
        public int NonZeroPackedByteCount { get; set; }
        public int NonZeroPixelCount { get; set; }
        public int[] PaletteIndexCounts { get; set; }
        public int TimingLookupIndex { get; set; }
        public int TimingTableValue { get; set; }
        public int DelayRetraces { get; set; }
    }

    public static class AnimationInspector
    {
        public static AnimationInspection Inspect(GameInstallation installation, string fileName)
        {
            string path = installation.ResolveFile(fileName);
            if (!Path.GetExtension(path).Equals(".ANM", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Animation inspection requires an .ANM file.", nameof(fileName));

            AnmFileRecord record = AnmFileRecord.Parse(File.ReadAllBytes(path), Path.GetFileName(path));
            AnimationSequence sequence = AnimationFrameDecoder.Decode(record);
            var timings = AnimationTimingDecoder.Decode(record);
            return new AnimationInspection
            {
                FileName = Path.GetFileName(path),
                FileLength = record.FileLength,
                HeaderLength = AnmFileRecord.HeaderLength,
                PlaybackControlLength = AnmFileRecord.PlaybackControlLength,
                HeaderTrailerLength = AnmFileRecord.HeaderTrailerLength,
                CompressedFrameStreamLength = record.CompressedFrameStream.Length,
                FileLengthIsMultipleOf128 = record.FileLengthIsMultipleOf128,
                HasPlaybackTerminator = record.HasPlaybackTerminator,
                PlaybackControlEntryCount = record.PlaybackControlEntryCount,
                PostTerminatorNonZeroPlaybackByteCount = record.PostTerminatorNonZeroPlaybackByteCount,
                FrameWidth = AnmFileRecord.FrameWidth,
                FrameHeight = AnmFileRecord.FrameHeight,
                PackedFrameLength = AnmFileRecord.PackedFrameLength,
                PlaybackControlBytes = record.PlaybackControlBytes.Select(value => (int)value).ToArray(),
                HeaderTrailerBytes = record.HeaderTrailerBytes.Select(value => (int)value).ToArray(),
                TimingTableBytes = record.TimingTableBytes.Select(value => (int)value).ToArray(),
                TimingScaleValue = record.TimingScaleValue,
                DecodedFrameCount = sequence.Frames.Count,
                ConsumedCompressedBytes = sequence.ConsumedBytes,
                RemainingCompressedBytes = sequence.RemainingBytes,
                Frames = sequence.Frames.Select((frame, index) => InspectFrame(frame, timings[index])).ToArray()
            };
        }

        private static AnimationFrameInspection InspectFrame(AnimationFrame frame, AnimationFrameTiming timing)
        {
            byte[] packedBytes = frame.PackedBytes;
            byte[] pixels = AnimationPixelDecoder.Decode(packedBytes).PaletteIndices;
            int[] paletteIndexCounts = new int[16];
            foreach (byte pixel in pixels)
                paletteIndexCounts[pixel]++;

            return new AnimationFrameInspection
            {
                Index = frame.Index,
                ControlValue = frame.ControlValue,
                CompressedOffset = frame.CompressedOffset,
                CompressedLength = frame.CompressedLength,
                TokenCount = frame.TokenCount,
                NonZeroPackedByteCount = packedBytes.Count(value => value != 0),
                NonZeroPixelCount = pixels.Count(value => value != 0),
                PaletteIndexCounts = paletteIndexCounts,
                TimingLookupIndex = timing.LookupIndex,
                TimingTableValue = timing.TableValue,
                DelayRetraces = timing.DelayRetraces
            };
        }

        public static string WriteJson(AnimationInspection inspection)
        {
            return JsonSerializer.Serialize(inspection, new JsonSerializerOptions { WriteIndented = true });
        }

        public static string WriteText(AnimationInspection inspection)
        {
            var output = new StringBuilder();
            output.AppendLine("Animation: " + inspection.FileName + " (0x" + inspection.FileLength.ToString("X") + " bytes)");
            output.AppendLine("Header: 0x" + inspection.HeaderLength.ToString("X2") + " bytes (0x" +
                inspection.PlaybackControlLength.ToString("X2") + " playback controls + 0x" +
                inspection.HeaderTrailerLength.ToString("X2") + " timing table/scale bytes)");
            output.AppendLine("Playback entries before first zero: " + inspection.PlaybackControlEntryCount +
                "; nonzero playback bytes after terminator: " + inspection.PostTerminatorNonZeroPlaybackByteCount);
            output.AppendLine("Compressed frame stream: 0x" + inspection.CompressedFrameStreamLength.ToString("X") + " bytes");
            output.AppendLine("Frame buffer: " + inspection.FrameWidth + "x" + inspection.FrameHeight +
                " pixels, 0x" + inspection.PackedFrameLength.ToString("X") + " packed bytes");
            output.AppendLine("128-byte file blocks: " + (inspection.FileLengthIsMultipleOf128 ? "yes" : "no"));
            output.AppendLine("Decoded frames: " + inspection.DecodedFrameCount + "; compressed bytes consumed: 0x" +
                inspection.ConsumedCompressedBytes.ToString("X") + "; remaining: 0x" + inspection.RemainingCompressedBytes.ToString("X"));
            foreach (AnimationFrameInspection frame in inspection.Frames)
                output.AppendLine("  frame " + frame.Index + " control=0x" + frame.ControlValue.ToString("X2") +
                    " stream=0x" + frame.CompressedOffset.ToString("X") + "+0x" + frame.CompressedLength.ToString("X") +
                    " tokens=" + frame.TokenCount + " nonzeroPacked=" + frame.NonZeroPackedByteCount +
                    " nonzeroPixels=" + frame.NonZeroPixelCount + " palette=" +
                    string.Join(",", frame.PaletteIndexCounts.Select((count, index) => index.ToString("X") + ":" + count)) +
                    " timingIndex=" + frame.TimingLookupIndex + " timingValue=" + frame.TimingTableValue +
                    " delayRetraces=" + frame.DelayRetraces);
            output.AppendLine("Playback controls: " + string.Join(" ", inspection.PlaybackControlBytes.Select(value => value.ToString("X2"))));
            output.AppendLine("Header trailer: " + string.Join(" ", inspection.HeaderTrailerBytes.Select(value => value.ToString("X2"))));
            output.AppendLine("Timing table: " + string.Join(" ", inspection.TimingTableBytes.Select(value => value.ToString("X2"))) +
                "; scale=" + inspection.TimingScaleValue);
            return output.ToString();
        }
    }
}
