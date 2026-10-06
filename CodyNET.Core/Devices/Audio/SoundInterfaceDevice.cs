using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

public class SoundInterfaceDevice : IAudioDevice
{
    // Store sound memory locally in the device, for easy access
    private readonly byte[] _soundMemory = new byte[0x100];// TODO: check if length is correct
    private readonly Voice[] _voices;
    public IReadOnlyList<Voice> Voices => _voices;
    private long _lastcycle;
    private readonly long _CPU_HZ = 1_000_000;//1Mhz
    private long _sampleAcc;
    private readonly long _SAMPLERATE = 16_000;//16Khz
    // === IMemoryMappedDevice ===
    public ushort StartAddress => SID_BASE;
    public ushort EndAddress => 0xD41C;// TODO: Figure out if this number is correct
    public bool SupportsRead => true;
    public bool SupportsWrite => true;
    // === Address Constants ===
    public const ushort SID_BASE = 0xD400;
    public const ushort VOICE1_OFFSET = 0x00;
    public const ushort VOICE2_OFFSET = 0x07;
    public const ushort VOICE3_OFFSET = 0x0E;
    private readonly IAudioOutput? _audioOutput;
    /*
        // === Control Registers ===
        //Addresses taken from cody_audio.spin hardware implementation
        [...]
        // ' $D415     Reserved (filters?)
        private const ushort UNUSED1 = 0xD415;
        // ' $D416     Reserved (filters?)
        private const ushort UNUSED2 = 0xD416;
        // ' $D417     Reserved (filters?)
        private const ushort UNUSED3 = 0xD417;
        // ' $D418     Volume control (filters?)
        private const ushort VOLUME_CONTROL = 0xD418;
        // ' $D419     Reserved
        private const ushort UNUSED4 = 0xD419;
        // ' $D41A     Reserved
        private const ushort UNUSED5 = 0xD41A;
        // ' $D41B     Voice 3 oscillator read
        private const ushort VOICE3_OSC_READ = 0xD41B;
        // ' $D41C     Voice 3 envelope read
        private const ushort VOICE3_ENV_READ = 0xD41C;*/
    public SoundInterfaceDevice(IAudioOutput? audioOutput = null)
    {
        _audioOutput = audioOutput;
        _voices = [new Voice(SID_BASE, _soundMemory, VOICE1_OFFSET),
                   new Voice(SID_BASE, _soundMemory, VOICE2_OFFSET),
                   new Voice(SID_BASE, _soundMemory, VOICE3_OFFSET)];
    }
    public Interrupt Update(long cycle)
    {
        var delta = cycle - _lastcycle;
        _lastcycle = cycle;
        if (delta <= 0)
        {
            return Interrupt.None;
        }
        _sampleAcc += delta * _SAMPLERATE;
        while (_sampleAcc >= _CPU_HZ)
        {
            _sampleAcc -= _CPU_HZ;
            GenerateSample();
        }
        return Interrupt.None;
    }

    public byte Read(ushort address)
    {
        if (address < StartAddress || address > EndAddress)
            throw new ArgumentOutOfRangeException(nameof(address), $"Address {address:X4} is out of range for SID device.");
        return _soundMemory[address - StartAddress]; // TODO: Figure out which registers can be read
    }

    public void Write(ushort address, byte value)
    {
        if (address < StartAddress || address > EndAddress)
            throw new ArgumentOutOfRangeException(nameof(address), $"Address {address:X4} is out of range for SID device.");
        _soundMemory[address - StartAddress] = value;
        //Dirty = true;
    }
    private void GenerateSample()
    {
        _voices[0].Step();//TODO: impelement update for all voices. 
        short sample = (short)(_voices[0].Output - 0x8000);//cast ushort to short 
        _audioOutput?.RenderSample(sample);
    }
}