using System;
using System.Collections.Generic;
using System.IO;
using InceptionTools.Records;

namespace InceptionTools.Animation
{
    public sealed class AnimationFrameTiming
    {
        internal AnimationFrameTiming(int frameIndex, int controlValue, int lookupIndex,
            int tableValue, int scaleValue, int delayRetraces)
        {
            FrameIndex = frameIndex;
            ControlValue = controlValue;
            LookupIndex = lookupIndex;
            TableValue = tableValue;
            ScaleValue = scaleValue;
            DelayRetraces = delayRetraces;
        }

        public int FrameIndex { get; }
        public int ControlValue { get; }
        public int LookupIndex { get; }
        public int TableValue { get; }
        public int ScaleValue { get; }
        public int DelayRetraces { get; }
    }

    public static class AnimationTimingDecoder
    {
        public const int FirstControlValue = 0x41;

        public static IReadOnlyList<AnimationFrameTiming> Decode(AnmFileRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (!record.HasPlaybackTerminator)
                throw new InvalidDataException("ANM playback controls do not contain a zero terminator.");

            byte[] controls = record.PlaybackControlBytes;
            byte[] table = record.TimingTableBytes;
            int scale = record.TimingScaleValue;
            var timings = new List<AnimationFrameTiming>();
            for (int frameIndex = 0; frameIndex < record.PlaybackControlEntryCount; frameIndex++)
            {
                int control = controls[frameIndex];
                int lookupIndex = control - FirstControlValue;
                if (lookupIndex < 0 || lookupIndex >= table.Length)
                    throw new InvalidDataException("ANM frame " + frameIndex + " timing control 0x" +
                        control.ToString("X2") + " indexes outside the 0x" + table.Length.ToString("X2") +
                        "-byte timing table.");

                int tableValue = table[lookupIndex];
                int signedByteProduct = unchecked((sbyte)tableValue) * unchecked((sbyte)scale);
                short scaledLowWord = unchecked((short)(signedByteProduct * 3));
                int delayRetraces = scaledLowWord >> 2;
                timings.Add(new AnimationFrameTiming(frameIndex, control, lookupIndex,
                    tableValue, scale, delayRetraces));
            }

            return timings.AsReadOnly();
        }
    }
}
