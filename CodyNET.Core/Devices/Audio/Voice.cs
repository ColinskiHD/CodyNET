using System.Diagnostics.Contracts;

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

    public Voice(ushort baseAddress, byte[] _memory, ushort voiceOffset)
    {
        SID_BASE = baseAddress;
        END_ADDRESS = (ushort)(baseAddress + registerSize);
        VOICE_OFFSET = voiceOffset;
        _voiceMemory = _memory;
    }
    public ushort Frequency => (ushort)((_voiceMemory[FREQ_HI + VOICE_OFFSET] << 8) | _voiceMemory[FREQ_LO + VOICE_OFFSET]);//not aquivalent to Hz
    public ushort Pwm => (ushort)((_voiceMemory[PWM_HI + VOICE_OFFSET] << 8) | _voiceMemory[PWM_LO + VOICE_OFFSET]);
}