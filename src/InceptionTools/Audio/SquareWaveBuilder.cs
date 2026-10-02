using System;
using System.Collections.Generic;

namespace InceptionTools.Audio
{
    internal sealed class SquareWaveBuilder
    {
        private readonly List<short> _samples = new List<short>();
        private readonly double[] _phases;

        public SquareWaveBuilder(int sampleRate, int channelCount)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            SampleRate = sampleRate;
            _phases = new double[channelCount];
        }

        public int SampleRate { get; }

        public void AddFrame(double seconds, params double[] frequencies)
        {
            if (seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            int sampleCount = Math.Max(1, (int)Math.Round(seconds * SampleRate));
            for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                double mixed = 0;
                int active = 0;
                for (int channel = 0; channel < frequencies.Length && channel < _phases.Length; channel++)
                {
                    double frequency = frequencies[channel];
                    if (frequency <= 0)
                        continue;
                    mixed += _phases[channel] < 0.5 ? 1 : -1;
                    active++;
                    _phases[channel] += frequency / SampleRate;
                    _phases[channel] -= Math.Floor(_phases[channel]);
                }
                _samples.Add(active == 0 ? (short)0 : (short)(mixed / active * 12000));
            }
        }

        public void AddNoise(double seconds, int seed)
        {
            int sampleCount = Math.Max(1, (int)Math.Round(seconds * SampleRate));
            uint state = (uint)(seed == 0 ? 1 : seed);
            for (int index = 0; index < sampleCount; index++)
            {
                state = (state >> 1) ^ ((uint)-(int)(state & 1) & 0xB400u);
                _samples.Add((state & 1) == 0 ? (short)-9000 : (short)9000);
            }
        }

        public short[] ToArray() => _samples.ToArray();
    }
}
