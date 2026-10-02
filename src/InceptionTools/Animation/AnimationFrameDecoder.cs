using System;
using System.Collections.Generic;
using System.IO;
using InceptionTools.Binary;
using InceptionTools.Records;

namespace InceptionTools.Animation
{
    public sealed class AnimationFrame
    {
        private readonly byte[] _packedBytes;

        internal AnimationFrame(int index, int controlValue, int compressedOffset,
            int compressedLength, int tokenCount, byte[] packedBytes)
        {
            Index = index;
            ControlValue = controlValue;
            CompressedOffset = compressedOffset;
            CompressedLength = compressedLength;
            TokenCount = tokenCount;
            _packedBytes = (byte[])packedBytes.Clone();
        }

        public int Index { get; }
        public int ControlValue { get; }
        public int CompressedOffset { get; }
        public int CompressedLength { get; }
        public int TokenCount { get; }
        public byte[] PackedBytes => (byte[])_packedBytes.Clone();
    }

    public sealed class AnimationSequence
    {
        internal AnimationSequence(List<AnimationFrame> frames, int compressedStreamLength, int consumedBytes)
        {
            Frames = frames.AsReadOnly();
            CompressedStreamLength = compressedStreamLength;
            ConsumedBytes = consumedBytes;
        }

        public IReadOnlyList<AnimationFrame> Frames { get; }
        public int CompressedStreamLength { get; }
        public int ConsumedBytes { get; }
        public int RemainingBytes => CompressedStreamLength - ConsumedBytes;
    }

    public sealed class AnimationFrameDecodeResult
    {
        private readonly byte[] _packedBytes;

        internal AnimationFrameDecodeResult(byte[] packedBytes, int consumedBytes, int tokenCount)
        {
            _packedBytes = (byte[])packedBytes.Clone();
            ConsumedBytes = consumedBytes;
            TokenCount = tokenCount;
        }

        public byte[] PackedBytes => (byte[])_packedBytes.Clone();
        public int ConsumedBytes { get; }
        public int TokenCount { get; }
    }

    public static class AnimationFrameDecoder
    {
        public static AnimationSequence Decode(AnmFileRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (!record.HasPlaybackTerminator)
                throw new InvalidDataException("ANM playback controls do not contain a zero terminator.");

            byte[] stream = record.CompressedFrameStream;
            byte[] controls = record.PlaybackControlBytes;
            byte[] accumulatedFrame = new byte[AnmFileRecord.PackedFrameLength];
            var frames = new List<AnimationFrame>();
            int offset = 0;

            for (int index = 0; index < record.PlaybackControlEntryCount; index++)
            {
                AnimationFrameDecodeResult decoded = DecodeFrame(stream, offset, accumulatedFrame,
                    "ANM frame " + index);
                accumulatedFrame = decoded.PackedBytes;
                frames.Add(new AnimationFrame(index, controls[index], offset,
                    decoded.ConsumedBytes, decoded.TokenCount, accumulatedFrame));
                offset += decoded.ConsumedBytes;
            }

            return new AnimationSequence(frames, stream.Length, offset);
        }

        public static AnimationFrameDecodeResult DecodeFrame(byte[] compressedStream, int startOffset,
            byte[] previousFrame = null, string sourceName = "ANM compressed frame stream")
        {
            var reader = new BoundedBinaryReader(compressedStream, sourceName);
            reader.RequireRange(startOffset, 0, "frame start");

            byte[] frame = previousFrame == null
                ? new byte[AnmFileRecord.PackedFrameLength]
                : CloneFrame(previousFrame);
            int sourceOffset = startOffset;
            int destinationOffset = 0;
            int tokenCount = 0;

            while (destinationOffset < frame.Length)
            {
                int tokenOffset = sourceOffset;
                sbyte control = unchecked((sbyte)reader.ReadByte(sourceOffset++, "RLE control"));
                bool repeat;
                int runLength;

                if (control == 0)
                {
                    int high = reader.ReadByte(sourceOffset++, "extended repeat count high byte");
                    int low = reader.ReadByte(sourceOffset++, "extended repeat count low byte");
                    runLength = (high << 8) | low;
                    repeat = true;
                }
                else if (control < 0)
                {
                    runLength = -control;
                    repeat = true;
                }
                else
                {
                    runLength = control;
                    repeat = false;
                }

                if (runLength == 0)
                    throw new InvalidDataException("Zero-length ANM token at compressed offset 0x" + tokenOffset.ToString("X") + ".");
                if (runLength > frame.Length - destinationOffset)
                    throw new InvalidDataException("ANM token at compressed offset 0x" + tokenOffset.ToString("X") +
                        " outputs " + runLength + " bytes with only " + (frame.Length - destinationOffset) + " remaining in the frame.");

                if (repeat)
                {
                    byte value = reader.ReadByte(sourceOffset++, "repeat value");
                    for (int count = 0; count < runLength; count++)
                        frame[destinationOffset++] ^= value;
                }
                else
                {
                    byte[] literals = reader.ReadBytes(sourceOffset, runLength, "literal bytes");
                    sourceOffset += runLength;
                    for (int count = 0; count < literals.Length; count++)
                        frame[destinationOffset++] ^= literals[count];
                }
                tokenCount++;
            }

            return new AnimationFrameDecodeResult(frame, sourceOffset - startOffset, tokenCount);
        }

        private static byte[] CloneFrame(byte[] previousFrame)
        {
            if (previousFrame.Length != AnmFileRecord.PackedFrameLength)
                throw new ArgumentException("Previous ANM frame must contain exactly 0x" +
                    AnmFileRecord.PackedFrameLength.ToString("X") + " packed bytes.", nameof(previousFrame));
            return (byte[])previousFrame.Clone();
        }
    }
}
