using System;
using System.IO;
using InceptionTools.Binary;

namespace InceptionTools.Records
{
    public sealed class AnmFileRecord
    {
        public const int HeaderLength = 0x33;
        public const int PlaybackControlLength = 0x20;
        public const int HeaderTrailerLength = HeaderLength - PlaybackControlLength;
        public const int TimingTableLength = HeaderTrailerLength - 1;
        public const int FrameWidth = 88;
        public const int FrameHeight = 88;
        public const int PackedFrameLength = 0x0F20;

        private readonly byte[] _rawBytes;
        private readonly byte[] _headerBytes;
        private readonly byte[] _playbackControlBytes;
        private readonly byte[] _headerTrailerBytes;
        private readonly byte[] _timingTableBytes;
        private readonly byte[] _compressedFrameStream;

        private AnmFileRecord(BoundedBinaryReader reader)
        {
            if (reader.Length < HeaderLength)
                throw new InvalidDataException("ANM file requires at least a 0x33-byte header; " +
                    reader.SourceName + " contains 0x" + reader.Length.ToString("X") + " bytes.");

            _rawBytes = reader.ReadBytes(0, reader.Length, "ANM file");
            _headerBytes = reader.ReadBytes(0, HeaderLength, "ANM header");
            _playbackControlBytes = reader.ReadBytes(0, PlaybackControlLength, "ANM playback controls");
            _headerTrailerBytes = reader.ReadBytes(PlaybackControlLength, HeaderTrailerLength, "ANM header trailer");
            _timingTableBytes = reader.ReadBytes(PlaybackControlLength, TimingTableLength, "ANM timing table");
            _compressedFrameStream = reader.ReadBytes(HeaderLength,
                reader.Length - HeaderLength, "ANM compressed frame stream");

            int terminator = Array.IndexOf(_playbackControlBytes, (byte)0);
            HasPlaybackTerminator = terminator >= 0;
            PlaybackControlEntryCount = terminator >= 0 ? terminator : PlaybackControlLength;

            int postTerminatorNonZeroCount = 0;
            if (terminator >= 0)
            {
                for (int index = terminator + 1; index < _playbackControlBytes.Length; index++)
                    if (_playbackControlBytes[index] != 0)
                        postTerminatorNonZeroCount++;
            }
            PostTerminatorNonZeroPlaybackByteCount = postTerminatorNonZeroCount;
        }

        public int FileLength => _rawBytes.Length;
        public bool FileLengthIsMultipleOf128 => FileLength % 128 == 0;
        public bool HasPlaybackTerminator { get; }
        public int PlaybackControlEntryCount { get; }
        public int PostTerminatorNonZeroPlaybackByteCount { get; }
        public byte[] HeaderBytes => (byte[])_headerBytes.Clone();
        public byte[] PlaybackControlBytes => (byte[])_playbackControlBytes.Clone();
        public byte[] HeaderTrailerBytes => (byte[])_headerTrailerBytes.Clone();
        public byte[] TimingTableBytes => (byte[])_timingTableBytes.Clone();
        public int TimingScaleValue => _headerTrailerBytes[TimingTableLength];
        public byte[] CompressedFrameStream => (byte[])_compressedFrameStream.Clone();
        public byte[] RawBytes => (byte[])_rawBytes.Clone();

        public static AnmFileRecord Parse(byte[] data, string sourceName = "ANM file")
        {
            return new AnmFileRecord(new BoundedBinaryReader(data, sourceName));
        }
    }
}
