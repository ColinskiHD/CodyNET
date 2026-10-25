/*this class implements parts of the following cody_audio.spin logic:
            make_wave
                ' Combine frequency into 16 bit number
                ' Shift by 2 because frequency * 4000 / 16 KHz sample rate
                mov     freq_coefficient, voice_freq_h
                shl     freq_coefficient, #8
                or      freq_coefficient, voice_freq_l
                shr     freq_coefficient, #2
                
                 ' Calculate next phase
                mov     temp_phase, phase
                add     temp_phase, freq_coefficient
                
                ' If we overflowed, set our internal sync bit to apply later
                testn   temp_phase, MASK_16                 wz
                muxnz   sync, #$02
*/
using System.Runtime.CompilerServices;
using CodyNET.Core.Devices.Audio;

public class Oscillator
{
    public ushort Phase { get; private set; }
    public bool Overflowed { get; private set; }
    public ushort Wave { get; private set; }
    private const ushort MASK_16 = 0xFFFF;
    private const ushort PHASEBIT_NOISE = 0x4000;// Phase accumulator bit to check when generating noise values
    public void Step(ushort frequency, Waveform waveform, ushort noise, ushort pwm = 0)
    {
        int freq_coefficient = frequency >> 2;
        int temp_phase = Phase + freq_coefficient;
        Overflowed = temp_phase > 0xFFFF;
        switch (waveform)
        {
            case Waveform.Triangle:
                Wave = Triangle(Phase);
                break;
            case Waveform.Sawtooth:
                Wave = Sawtooth(Phase);
                break;
            case Waveform.Pulse:
                Wave = Pulse(Phase, pwm);
                break;
            case Waveform.Noise:
                Wave = Noise(Phase, noise, temp_phase, Wave);
                break;
            default:
                break;
        }
        Phase = (ushort)temp_phase;
    }

    private static ushort Sawtooth(ushort Phase)
    {
        return Phase;
    }
    private static ushort Triangle(ushort Phase)
    {
        bool bit_15 = (Phase & 0x8000) != 0;
        ushort wave = (ushort)(Phase << 1);
        if (bit_15)
        {
            wave = (ushort)(MASK_16 ^ wave);
        }
        return wave;
    }

    private static ushort Pulse(ushort Phase, ushort pwm)
    {
        pwm = (ushort)(pwm << 4);
        bool high = Phase < pwm;
        ushort wave = 0x0000;
        if (high)
        {
            wave = MASK_16;
        }
        return wave;
    }

    private static ushort Noise(ushort Phase, ushort noise, int temp_phase, ushort lastWave)
    {
        ushort wave = lastWave;
        ushort temp = Phase;
        temp = (ushort)(temp ^ temp_phase);
        if ((temp & PHASEBIT_NOISE) != 0)
        {
            wave = noise;
        }
        return wave;
    }
}
