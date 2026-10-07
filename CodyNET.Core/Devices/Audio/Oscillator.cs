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
    public void Step(ushort frequency, Waveform waveform)
    {
        int freq_coefficient = frequency >> 2;
        int next_phase = Phase + freq_coefficient;
        Overflowed = next_phase > 0xFFFF;
        //TODO: noise logic with next_phase and Phase
        switch (waveform)//TODO: implement all waveforms
        {
            case Waveform.Triangle:
                Wave = Triangle(Phase);
                break;
            case Waveform.Sawtooth:
                Wave = Sawtooth(Phase);
                break;
            default:
                Wave = 0x8000;//TODO: check if this is correct
                break;
        }
        Phase = (ushort)next_phase;
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
}
