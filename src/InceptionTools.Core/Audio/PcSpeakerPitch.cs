using System;

namespace InceptionTools.Audio;

public static class PcSpeakerPitch
{
    public const double PitClockHz = 1193182.0;
    public const int MusicTimerDivisor = 0x0FFF;
    public const byte RestCode = 0x80;

    // 204B:0121. The executable divides the PIT clock by one of these
    // values, then shifts the result according to noteCode / 12.
    private static readonly ushort[] SemitoneFrequencyTenths =
    {
        0x105A, 0x1153, 0x125B, 0x1372, 0x149A, 0x15D4,
        0x1720, 0x1880, 0x19F5, 0x1B80, 0x1D23, 0x1EDE
    };

    public static double MusicInterruptRateHz => PitClockHz / MusicTimerDivisor;

    public static ushort ToPitDivisor(byte noteCode)
    {
        if (noteCode == RestCode)
            return 0;

        int octave = noteCode / 12;
        int shift = 8 - octave;
        if (shift < 0 || shift > 15)
            throw new ArgumentOutOfRangeException(nameof(noteCode), "Note code produces an unsupported 16-bit PIT shift.");

        int baseDivisor = (int)PitClockHz / SemitoneFrequencyTenths[noteCode % 12];
        int divisor = baseDivisor << shift;
        if (divisor <= 0 || divisor > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(noteCode), "Note code produces an invalid 16-bit PIT divisor.");
        return (ushort)divisor;
    }

    public static double ToFrequencyHz(byte noteCode)
    {
        ushort divisor = ToPitDivisor(noteCode);
        return divisor == 0 ? 0 : PitClockHz / divisor;
    }
}
