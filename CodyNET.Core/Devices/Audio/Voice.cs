using CodyNET.Core.Devices.Audio;
using Serilog.Formatting.Display;
public class Voice
{

    private const ushort registerSize = 0x07;
    private int VOICE_OFFSET;
    private ushort END_ADDRESS;
    public ushort SID_BASE;
    private const ushort FREQ_LO = 0x00;
    private const ushort FREQ_HI = 0x01;
    private const ushort PWM_LO = 0x02;
    private const ushort PWM_HI = 0x03;
    private const ushort CONTROL = 0x04;
    private const ushort ATTACK_DECAY = 0x05;
    private const ushort SUSTAIN_RELEASE = 0x06;
    private readonly byte[] _voiceMemory = new byte[0x07];
    private readonly Oscillator _oscillator = new();
    public ushort Output { get; private set; }

    public Voice(ushort baseAddress, byte[] _memory, ushort voiceOffset)
    {
        SID_BASE = baseAddress;
        END_ADDRESS = (ushort)(baseAddress + registerSize);
        VOICE_OFFSET = voiceOffset;
        _voiceMemory = _memory;
    }
    public ushort Frequency => (ushort)((_voiceMemory[FREQ_HI + VOICE_OFFSET] << 8) | _voiceMemory[FREQ_LO + VOICE_OFFSET]);//not aquivalent to Hz
    public ushort Pwm => (ushort)((_voiceMemory[PWM_HI + VOICE_OFFSET] << 8) | _voiceMemory[PWM_LO + VOICE_OFFSET]);

    // === Control Bits ===

    /// <summary> Bit 0 ("gate") plays/ends the sound. </summary>
    public bool Gate => (_voiceMemory[CONTROL + VOICE_OFFSET] & 0x01) != 0;
    /// <summary> Bit 1 syncs with other voice.
    /// voice 1 -> with voice 3. 
    /// voice 2 -> with voice 1. 
    /// voice 3 -> with voice 2.
    /// </summary>
    public bool Sync => (_voiceMemory[CONTROL + VOICE_OFFSET] & 0x02) != 0;
    /// <summary> Bit 2 enables ring modulation with other voice
    /// voice 1 -> with voice 3. 
    /// voice 2 -> with voice 1. 
    /// voice 3 -> with voice 2. 
    /// </summary>
    public bool RingMod => (_voiceMemory[CONTROL + VOICE_OFFSET] & 0x04) != 0;
    /// <summary> Bit 3 resets the voice internally. </summary>
    public bool Reset => (_voiceMemory[CONTROL + VOICE_OFFSET] & 0x08) != 0;
    /// <summary> Bit 4 selects a triangle wave. </summary>
    public bool Triangle => (_voiceMemory[CONTROL + VOICE_OFFSET] & 0x10) != 0;
    /// <summary> Bit 5 selects a sawtooth wave. </summary>
    public bool Saw => (_voiceMemory[CONTROL + VOICE_OFFSET] & 0x20) != 0;
    /// <summary> Bit 6 selects a pulse wave. </summary>
    public bool Pulse => (_voiceMemory[CONTROL + VOICE_OFFSET] & 0x40) != 0;
    /// <summary> Bit 7 selects a random noise output. </summary>
    public bool Noise => (_voiceMemory[CONTROL + VOICE_OFFSET] & 0x80) != 0;
    public ushort Attack => (ushort)((_voiceMemory[ATTACK_DECAY + VOICE_OFFSET] >> 4) & 0x0F);//high nibble
    public ushort Decay => (ushort)(_voiceMemory[ATTACK_DECAY + VOICE_OFFSET] & 0x0F);//low nibble
    public ushort Sustain => (ushort)(_voiceMemory[SUSTAIN_RELEASE + VOICE_OFFSET] >> 4);//high nibble
    public ushort Release => (ushort)(_voiceMemory[SUSTAIN_RELEASE + VOICE_OFFSET] & 0x0F);//low nibble
    public Waveform WaveformSelected =>
        Triangle ? Waveform.Triangle :
        Saw ? Waveform.Sawtooth :
        Pulse ? Waveform.Pulse :
        Noise ? Waveform.Noise :
        Waveform.None;
    public void Step()
    {
        _oscillator.Step(Frequency, WaveformSelected);
        Output = _oscillator.Wave;
    }
}