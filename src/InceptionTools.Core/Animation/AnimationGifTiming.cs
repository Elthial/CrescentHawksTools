using System;

namespace InceptionTools.Animation;

/// <summary>
/// Converts the original hardware vertical-retrace waits to GIF's integer
/// centiseconds using the nominal refresh of EGA 320x200 graphics mode.
/// </summary>
public static class AnimationGifTiming
{
    public const int NominalRefreshRateHz = 60;

    public static int ToCentiseconds(int delayRetraces)
    {
        if (delayRetraces < 0)
            throw new ArgumentOutOfRangeException(nameof(delayRetraces),
                "Animation retrace delays cannot be negative.");

        // Round to nearest centisecond. Integer arithmetic keeps output
        // deterministic and preserves a true zero-retrace delay as zero.
        return checked((delayRetraces * 100 + NominalRefreshRateHz / 2) /
            NominalRefreshRateHz);
    }
}
