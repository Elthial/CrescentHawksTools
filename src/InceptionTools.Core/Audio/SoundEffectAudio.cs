using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InceptionTools.Audio;

public sealed class SoundEffectCommand
{
    public ushort Algorithm { get; set; }
    public ushort[] Parameters { get; set; } = [];
}

public sealed class SoundEffectDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public int ExecutableWordOffset { get; set; }
    public IReadOnlyList<SoundEffectCommand> Commands { get; set; } = [];
}

public static class SoundEffectCatalog
{
    public const string SourceTableSha256 = "065BFB0DBA2A02575BB77BD00C7CDD010931E2A957AB4DD7337563AF860FCC79";
    // Structured transcription of the ushort table at 3EDB:5008..5279.
    // Each effect ends with the executable's three-word [1,0,0] delimiter.
    public static IReadOnlyList<SoundEffectDefinition> All { get; } = new[]
    {
        Effect(0x01, "missile", "Missile launch or flight", "Probable", 0,
            C(1002, 1, 1000, 500, 100, 1, 10)),
        Effect(0x02, "mech-kick", "Mech kick", "Probable", 10,
            C(1002, 1, 6000, 7000, 10, 1, 10)),
        Effect(0x03, "repeating-projectile", "Repeating projectile weapon", "Probable", 20,
            C(1002, 5, 10, 800, 20, 3, 50)),
        Effect(0x04, "infantry-impact", "Infantry combat impact/noise", "Probable", 30,
            C(1004, 1, 50, 1000, 5, 5, 175)),
        Effect(0x05, "vibroblade", "Vibroblade", "Probable", 40,
            C(1004, 1, 10, 50, 5, 5, 1)),
        Effect(0x06, "single-shot-projectile", "Single-shot projectile weapon", "Probable", 50,
            C(1, 2000, 2), C(2, 3000, 2), C(2, 4000, 2), C(1, 5000, 2), C(1, 7000, 2), C(1, 10000, 2)),
        Effect(0x07, "arena-destruction", "Arena destruction", "Probable", 71,
            C(1004, 1, 50, 1000, 5, 5, 40), C(1004, 1, 10, 2000, 10, 5, 50)),
        Effect(0x08, "terrain-damage", "Terrain damage", "Probable", 88,
            C(1004, 1, 500, 1000, 2, 2, 10), C(1004, 1, 500, 1500, 2, 2, 20), C(1004, 1, 500, 2000, 2, 2, 40)),
        Effect(0x09, "laser", "Laser weapon", "Probable", 112,
            C(1003, 4, 500, 400, 300, 1, 1)),
        Effect(0x0A, "cache-door", "Star League cache grinding door", "Probable", 122,
            C(1002, 20, 100, 50, 1, 100, 40)),
        Effect(0x0B, "bow-string", "Bow string", "Probable", 132,
            C(1002, 10, 10, 10, 10, 10, 10), C(1002, 10, 30, 10, 10, 10, 10), C(1002, 10, 50, 10, 10, 10, 10)),
        Effect(0x0C, "mech-startup", "Successful mech startup", "Probable", 156,
            C(1004, 1, 1400, 1800, 1, 5, 150), C(1003, 1, 1200, 500, 1, 5, 20)),
        Effect(0x0D, "blade-impact", "Blade impact", "Probable", 173,
            C(1001, 10, 200, 100, 1, 10), C(1001, 10, 300, 100, 1, 10),
            C(1001, 10, 400, 100, 1, 10), C(1001, 10, 500, 100, 1, 10)),
        Effect(0x0E, "mech-startup-failed", "Failed mech startup", "Probable", 200,
            C(1003, 1, 900, 875, 1, 8, 40), C(1004, 1, 850, 900, 1, 8, 40),
            C(1003, 1, 900, 875, 1, 8, 40), C(1004, 1, 850, 900, 1, 8, 40),
            C(1003, 1, 900, 875, 1, 8, 40), C(1004, 1, 850, 900, 1, 8, 40),
            C(1004, 1, 900, 1300, 1, 8, 80)),
        Effect(0x0F, "map-interaction", "Map interaction", "Probable", 252,
            C(20, 830, 1), C(20, 680, 1), C(20, 600, 1), C(20, 830, 1), C(20, 680, 1), C(20, 600, 1)),
        Effect(0x10, "password-accepted", "Password accepted", "Probable", 273,
            C(30, 550, 1), C(30, 450, 1), C(30, 390, 1), C(30, 500, 1), C(30, 400, 1), C(30, 340, 1)),
        Effect(0x11, "password-incorrect", "Password incorrect", "Probable", 294,
            C(1003, 40, 150, 20, 10, 2, 10)),
        Effect(0x12, "squished-by-mech", "Infantry squished by a mech", "Probable", 304,
            C(1001, 5, 4000, 2000, 1, 1))
    };

    public static SoundEffectDefinition Get(string idOrName)
    {
        if (string.IsNullOrWhiteSpace(idOrName))
            throw new ArgumentException("A sound-effect ID or name is required.", nameof(idOrName));
        SoundEffectDefinition? result;
        if (TryParseId(idOrName, out int id))
            result = All.SingleOrDefault(effect => effect.Id == id);
        else
            result = All.SingleOrDefault(effect => effect.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase));
        if (result == null)
            throw new ArgumentOutOfRangeException(nameof(idOrName), "Unknown sound-effect ID or name: " + idOrName + ".");
        return result;
    }

    public static bool TryGet(int id, out SoundEffectDefinition effect)
    {
        SoundEffectDefinition? match = All.SingleOrDefault(candidate => candidate.Id == id);
        effect = match!;
        return match is not null;
    }

    public static ushort[] ToExecutableWords()
    {
        var words = new List<ushort>();
        foreach (SoundEffectDefinition effect in All)
        {
            foreach (SoundEffectCommand command in effect.Commands)
            {
                words.Add(command.Algorithm);
                words.AddRange(command.Parameters);
            }
            words.AddRange(new ushort[] { 1, 0, 0 });
        }
        return words.ToArray();
    }

    public static byte[] ToExecutableBytes()
    {
        ushort[] words = ToExecutableWords();
        var bytes = new byte[words.Length * 2];
        for (int index = 0; index < words.Length; index++)
        {
            bytes[index * 2] = (byte)words[index];
            bytes[index * 2 + 1] = (byte)(words[index] >> 8);
        }
        return bytes;
    }

    public static string CalculateTableSha256() =>
        Convert.ToHexString(SHA256.HashData(ToExecutableBytes()));

    private static bool TryParseId(string value, out int id)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.TryParse(value.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out id);
        return int.TryParse(value, out id);
    }

    private static SoundEffectDefinition Effect(int id, string name, string description, string confidence,
        int wordOffset, params SoundEffectCommand[] commands) => new SoundEffectDefinition
        {
            Id = id,
            Name = name,
            Description = description,
            Confidence = confidence,
            ExecutableWordOffset = wordOffset,
            Commands = commands
        };

    private static SoundEffectCommand C(ushort algorithm, params ushort[] parameters) =>
        new SoundEffectCommand { Algorithm = algorithm, Parameters = parameters };
}

public static class SoundEffectRenderer
{
    public const int DefaultSampleRate = 22050;
    private const double DelayUnitSeconds = 0.0015;
    private const double SweepToneUnitSeconds = 0.00005;

    // This follows the table's verified command topology and PIT divisors.
    // Busy-loop timing and gate-toggle noise are rendered deterministically,
    // but remain an approximation until CPU-cycle timing is emulated.
    public static short[] Render(SoundEffectDefinition effect, int sampleRate = DefaultSampleRate)
    {
        if (effect == null)
            throw new ArgumentNullException(nameof(effect));
        var builder = new SquareWaveBuilder(sampleRate, 1);
        foreach (SoundEffectCommand command in effect.Commands)
            RenderCommand(builder, command);
        return builder.ToArray();
    }

    public static byte[] RenderWave(SoundEffectDefinition effect, int sampleRate = DefaultSampleRate) =>
        PcmWaveEncoder.EncodeMono16(Render(effect, sampleRate), sampleRate);

    private static void RenderCommand(SquareWaveBuilder builder, SoundEffectCommand command)
    {
        ushort[] p = command.Parameters;
        if (command.Algorithm <= 1000)
        {
            AddTone(builder, p[0], command.Algorithm * p[1] * DelayUnitSeconds);
            return;
        }

        switch (command.Algorithm)
        {
            case 1001:
                for (int repeat = 0; repeat < p[0]; repeat++)
                    builder.AddNoise(Math.Max(0.001, p[3] * DelayUnitSeconds), p[4] + repeat);
                break;
            case 1002:
                for (int repeat = 0; repeat < p[0]; repeat++)
                    for (int cycle = 0; cycle < p[4]; cycle++)
                        for (int delta = 0; delta < p[2] * 2; delta += Math.Max(1, (int)p[5]))
                        {
                            // The original 16-bit SUB/ADD sequence wraps. Several real
                            // effects deliberately begin below zero in mathematical terms.
                            int divisor = unchecked((ushort)(p[1] - p[2] + delta));
                            AddTone(builder, divisor, p[3] * SweepToneUnitSeconds);
                        }
                break;
            case 1003:
            case 1004:
                for (int repeat = 0; repeat < p[0]; repeat++)
                {
                    int step = Math.Max(1, (int)p[5]);
                    if (command.Algorithm == 1003)
                        for (int divisor = p[1]; divisor > p[2]; divisor -= step)
                            AddTone(builder, divisor, p[4] * DelayUnitSeconds);
                    else
                        for (int divisor = p[1]; divisor < p[2]; divisor += step)
                            AddTone(builder, divisor, p[4] * DelayUnitSeconds);
                }
                break;
            default:
                throw new InvalidDataException("Unsupported executable sound algorithm " + command.Algorithm + ".");
        }
    }

    private static void AddTone(SquareWaveBuilder builder, int divisor, double seconds) =>
        builder.AddFrame(seconds, PcSpeakerPitch.PitClockHz / (divisor == 0 ? 65536 : divisor));
}

public static class SoundEffectWriter
{
    public static string WriteJson() => JsonSerializer.Serialize(SoundEffectCatalog.All,
        new JsonSerializerOptions { WriteIndented = true });

    public static string WriteText()
    {
        var output = new StringBuilder();
        output.AppendLine("Executable sound effects (3EDB:5008):");
        foreach (SoundEffectDefinition effect in SoundEffectCatalog.All)
        {
            output.Append("0x").Append(effect.Id.ToString("X2")).Append("  ")
                .Append(effect.Name.PadRight(24)).Append(" ").Append(effect.Description)
                .Append(" [").Append(effect.Confidence).Append("] commands=")
                .Append(effect.Commands.Count).Append(" table=3EDB:")
                .Append((0x5008 + effect.ExecutableWordOffset * 2).ToString("X4")).AppendLine();
        }
        return output.ToString();
    }
}

public static class WaveAudioPlayer
{
    private const uint SoundMemory = 0x0004;
    private const uint SoundSync = 0x0000;

    public static void Play(byte[] wave)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException("Direct WAV playback currently uses the Windows winmm API; export the WAV on other platforms.");
        if (!PlaySound(wave, IntPtr.Zero, SoundMemory | SoundSync))
            throw new InvalidOperationException("Windows could not play the generated WAV data.");
    }

    [DllImport("winmm.dll", SetLastError = true)]
    private static extern bool PlaySound(byte[] pszSound, IntPtr hmod, uint fdwSound);
}
