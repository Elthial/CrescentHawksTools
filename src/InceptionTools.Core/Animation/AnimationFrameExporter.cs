using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InceptionTools.Installation;
using InceptionTools.Records;

namespace InceptionTools.Animation;

public sealed class AnimationFrameExportResult
{
    internal AnimationFrameExportResult(string sourceFileName, int frameIndex, int frameCount,
        string outputPath, int outputLength)
    {
        SourceFileName = sourceFileName;
        FrameIndex = frameIndex;
        FrameCount = frameCount;
        OutputPath = outputPath;
        OutputLength = outputLength;
    }

    public string SourceFileName { get; }
    public int FrameIndex { get; }
    public int FrameCount { get; }
    public string OutputPath { get; }
    public int OutputLength { get; }
}

public sealed class AnimationSequenceExportResult
{
    internal AnimationSequenceExportResult(string sourceFileName, string outputDirectory,
        List<AnimationFrameExportResult> frames)
    {
        SourceFileName = sourceFileName;
        OutputDirectory = outputDirectory;
        Frames = frames.AsReadOnly();
    }

    public string SourceFileName { get; }
    public string OutputDirectory { get; }
    public IReadOnlyList<AnimationFrameExportResult> Frames { get; }
}

public sealed class AnimationGifExportResult
{
    internal AnimationGifExportResult(string sourceFileName, string outputPath, int frameCount,
        int totalDelayRetraces, int totalDelayCentiseconds, int outputLength)
    {
        SourceFileName = sourceFileName;
        OutputPath = outputPath;
        FrameCount = frameCount;
        TotalDelayRetraces = totalDelayRetraces;
        TotalDelayCentiseconds = totalDelayCentiseconds;
        OutputLength = outputLength;
    }

    public string SourceFileName { get; }
    public string OutputPath { get; }
    public int FrameCount { get; }
    public int TotalDelayRetraces { get; }
    public int TotalDelayCentiseconds { get; }
    public int OutputLength { get; }
    public int NominalRefreshRateHz => AnimationGifTiming.NominalRefreshRateHz;
}

public sealed class AnimationGifBatchExportResult
{
    internal AnimationGifBatchExportResult(string outputDirectory, List<AnimationGifExportResult> animations)
    {
        OutputDirectory = outputDirectory;
        Animations = animations.AsReadOnly();
    }

    public string OutputDirectory { get; }
    public IReadOnlyList<AnimationGifExportResult> Animations { get; }
    public int TotalFrameCount => Animations.Sum(animation => animation.FrameCount);
    public int TotalOutputLength => Animations.Sum(animation => animation.OutputLength);
}

public static class AnimationFrameExporter
{
    public static AnimationFrameExportResult ExportPng(GameInstallation installation, string fileName,
        int frameIndex, string outputPath, bool overwrite = false)
    {
        if (installation == null)
            throw new ArgumentNullException(nameof(installation));
        if (frameIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(frameIndex), "Animation frame index cannot be negative.");
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("An output PNG path is required.", nameof(outputPath));
        if (!Path.GetExtension(outputPath).Equals(".png", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Animation frame output must use a .png extension.", nameof(outputPath));

        AnimationSequence sequence = LoadSequence(installation, fileName, out string sourceFileName);
        if (frameIndex >= sequence.Frames.Count)
            throw new ArgumentOutOfRangeException(nameof(frameIndex), "Animation " + sourceFileName +
                " contains " + sequence.Frames.Count + " frames; requested index " + frameIndex + ".");

        string fullOutputPath = Path.GetFullPath(outputPath);
        string? directory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        int outputLength = WritePng(sequence.Frames[frameIndex], fullOutputPath, overwrite);

        return new AnimationFrameExportResult(sourceFileName, frameIndex,
            sequence.Frames.Count, fullOutputPath, outputLength);
    }

    public static AnimationSequenceExportResult ExportPngSequence(GameInstallation installation, string fileName,
        string outputDirectory, bool overwrite = false)
    {
        if (installation == null)
            throw new ArgumentNullException(nameof(installation));
        if (string.IsNullOrWhiteSpace(outputDirectory))
            throw new ArgumentException("An animation frame output directory is required.", nameof(outputDirectory));

        AnimationSequence sequence = LoadSequence(installation, fileName, out string sourceFileName);
        string fullOutputDirectory = Path.GetFullPath(outputDirectory);
        string baseName = Path.GetFileNameWithoutExtension(sourceFileName);
        int digits = Math.Max(2, (sequence.Frames.Count - 1).ToString().Length);
        var outputPaths = new List<string>();
        for (int index = 0; index < sequence.Frames.Count; index++)
            outputPaths.Add(Path.Combine(fullOutputDirectory, baseName + "-frame-" +
                index.ToString("D" + digits) + ".png"));

        if (!overwrite)
        {
            foreach (string path in outputPaths)
                if (File.Exists(path))
                    throw new IOException("Animation frame output already exists: " + path +
                        ". Pass --force to overwrite the complete sequence.");
        }

        Directory.CreateDirectory(fullOutputDirectory);
        var results = new List<AnimationFrameExportResult>();
        for (int index = 0; index < sequence.Frames.Count; index++)
        {
            int outputLength = WritePng(sequence.Frames[index], outputPaths[index], overwrite);
            results.Add(new AnimationFrameExportResult(sourceFileName, index,
                sequence.Frames.Count, outputPaths[index], outputLength));
        }

        return new AnimationSequenceExportResult(sourceFileName, fullOutputDirectory, results);
    }

    public static AnimationGifExportResult ExportGif(GameInstallation installation, string fileName,
        string outputPath, bool overwrite = false)
    {
        if (installation == null)
            throw new ArgumentNullException(nameof(installation));
        ValidateGifOutputPath(outputPath);

        string fullOutputPath = Path.GetFullPath(outputPath);
        if (!overwrite && File.Exists(fullOutputPath))
            throw new IOException("Animation GIF output already exists: " + fullOutputPath +
                ". Pass --force to overwrite it.");

        PreparedGif prepared = PrepareGif(installation, fileName, fullOutputPath);
        string? directory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        WriteBytes(prepared.Bytes, fullOutputPath, overwrite);
        return prepared.ToResult();
    }

    public static AnimationGifBatchExportResult ExportAllGifs(GameInstallation installation,
        string outputDirectory, bool overwrite = false)
    {
        if (installation == null)
            throw new ArgumentNullException(nameof(installation));
        if (string.IsNullOrWhiteSpace(outputDirectory))
            throw new ArgumentException("An animation GIF output directory is required.", nameof(outputDirectory));

        string fullOutputDirectory = Path.GetFullPath(outputDirectory);
        var outputPaths = new List<string>();
        for (int index = 0; index < 22; index++)
            outputPaths.Add(Path.Combine(fullOutputDirectory, "O" + index + ".gif"));

        if (!overwrite)
        {
            foreach (string path in outputPaths)
                if (File.Exists(path))
                    throw new IOException("Animation GIF output already exists: " + path +
                        ". Pass --force to overwrite the complete batch.");
        }

        // Decode and encode every source before creating the directory or
        // writing a file. A malformed/missing ANM cannot yield a partial set.
        var prepared = new List<PreparedGif>();
        for (int index = 0; index < 22; index++)
            prepared.Add(PrepareGif(installation, "O" + index + ".ANM", outputPaths[index]));

        Directory.CreateDirectory(fullOutputDirectory);
        foreach (PreparedGif animation in prepared)
            WriteBytes(animation.Bytes, animation.OutputPath, overwrite);

        return new AnimationGifBatchExportResult(fullOutputDirectory,
            prepared.Select(animation => animation.ToResult()).ToList());
    }

    private static AnimationSequence LoadSequence(GameInstallation installation, string fileName,
        out string sourceFileName)
    {
        string sourcePath = installation.ResolveFile(fileName);
        if (!Path.GetExtension(sourcePath).Equals(".ANM", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Animation frame export requires an .ANM file.", nameof(fileName));

        sourceFileName = Path.GetFileName(sourcePath);
        AnmFileRecord record = AnmFileRecord.Parse(File.ReadAllBytes(sourcePath), sourceFileName);
        return AnimationFrameDecoder.Decode(record);
    }

    private static PreparedGif PrepareGif(GameInstallation installation, string fileName, string outputPath)
    {
        string sourcePath = installation.ResolveFile(fileName);
        if (!Path.GetExtension(sourcePath).Equals(".ANM", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Animation GIF export requires an .ANM file.", nameof(fileName));

        string sourceFileName = Path.GetFileName(sourcePath);
        AnmFileRecord record = AnmFileRecord.Parse(File.ReadAllBytes(sourcePath), sourceFileName);
        AnimationSequence sequence = AnimationFrameDecoder.Decode(record);
        IReadOnlyList<AnimationFrameTiming> timings = AnimationTimingDecoder.Decode(record);
        if (sequence.Frames.Count != timings.Count)
            throw new InvalidDataException("Animation frame and timing counts do not match for " + sourceFileName + ".");

        var pixels = new List<AnimationPixelFrame>();
        var delays = new List<int>();
        for (int index = 0; index < sequence.Frames.Count; index++)
        {
            pixels.Add(AnimationPixelDecoder.Decode(sequence.Frames[index]));
            delays.Add(AnimationGifTiming.ToCentiseconds(timings[index].DelayRetraces));
        }

        byte[] bytes = AnimationGifEncoder.Encode(pixels, delays);
        return new PreparedGif(sourceFileName, outputPath, sequence.Frames.Count,
            timings.Sum(timing => timing.DelayRetraces), delays.Sum(), bytes);
    }

    private static void ValidateGifOutputPath(string outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("An output GIF path is required.", nameof(outputPath));
        if (!Path.GetExtension(outputPath).Equals(".gif", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Animation output must use a .gif extension.", nameof(outputPath));
    }

    private static void WriteBytes(byte[] bytes, string outputPath, bool overwrite)
    {
        using (var output = new FileStream(outputPath, overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write, FileShare.None))
            output.Write(bytes, 0, bytes.Length);
    }

    private static int WritePng(AnimationFrame frame, string outputPath, bool overwrite)
    {
        byte[] png = AnimationPngEncoder.Encode(AnimationPixelDecoder.Decode(frame));
        using (var output = new FileStream(outputPath, overwrite ? FileMode.Create : FileMode.CreateNew,
            FileAccess.Write, FileShare.None))
            output.Write(png, 0, png.Length);
        return png.Length;
    }

    private sealed class PreparedGif
    {
        internal PreparedGif(string sourceFileName, string outputPath, int frameCount,
            int totalDelayRetraces, int totalDelayCentiseconds, byte[] bytes)
        {
            SourceFileName = sourceFileName;
            OutputPath = outputPath;
            FrameCount = frameCount;
            TotalDelayRetraces = totalDelayRetraces;
            TotalDelayCentiseconds = totalDelayCentiseconds;
            Bytes = bytes;
        }

        internal string SourceFileName { get; }
        internal string OutputPath { get; }
        internal int FrameCount { get; }
        internal int TotalDelayRetraces { get; }
        internal int TotalDelayCentiseconds { get; }
        internal byte[] Bytes { get; }

        internal AnimationGifExportResult ToResult()
        {
            return new AnimationGifExportResult(SourceFileName, OutputPath, FrameCount,
                TotalDelayRetraces, TotalDelayCentiseconds, Bytes.Length);
        }
    }
}
