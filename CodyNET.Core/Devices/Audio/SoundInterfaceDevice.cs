using CodyNET.Core.Cody;
using CodyNET.Core.Devices.Audio;
using CodyNET.Core.Interfaces;

public class SoundInterfaceDevice : IAudioDevice
{
    // Store sound memory locally in the device, for easy access
    private readonly byte[] _soundMemory = new byte[0x100];// TODO: check if length is correct
    private readonly Voice[] _voices;
    private readonly NoiseGenerator _noiseGenerator = new();
    public IReadOnlyList<Voice> Voices => _voices;
    private Mixer _mixer = new();
    private long _lastcycle;
    private readonly long _CPU_HZ = 1_000_000;//1Mhz
    private long _sampleAcc;
    private readonly long _SAMPLERATE = 16_000;//16Khz
    private static readonly IReadOnlyDictionary<int, int> SecondaryVoice = new Dictionary<int, int> { { 0, 2 }, { 1, 0 }, { 2, 1 } };
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
    private const ushort VOLUME_REGISTER = 0x18;
    public const ushort VOICE3_OSC = 0x1B;
    public const ushort VOICE3_ENV = 0x1C;
    private readonly IAudioOutput? _audioOutput;

    private ushort VolumeControl => _soundMemory[VOLUME_REGISTER];

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
        _noiseGenerator.Step();
        StepVoices();
        _soundMemory[VOICE3_OSC] = (byte)(_voices[2].Wave >> 8);
        _soundMemory[VOICE3_ENV] = (byte)(_voices[2].Envelope >> 8);
        short sample = _mixer.Step(_voices[0].Output, _voices[1].Output, _voices[2].Output, VolumeControl);

        _audioOutput?.RenderSample(sample);
    }
    private void StepVoices()
    {
        for (int i = 0; i < 3; i++)
        {
            SyncVoiceAWithVoiceB(_voices, i, SecondaryVoice[i]);
            _voices[i].Step(_noiseGenerator.Value);
            RingModVoiceAWithVoiceB(_voices, i, SecondaryVoice[i]);
        }
    }
    private static void SyncVoiceAWithVoiceB(Voice[] voices, int a, int b)
    {
        if (voices[a].Sync && voices[b].Overflowed)
        {
            voices[a].ResetPhase();
        }
    }
    private static void RingModVoiceAWithVoiceB(Voice[] voices, int a, int b)
    {
        if (voices[a].RingMod && (voices[b].Phase & 0x8000) != 0)
        {
            voices[a].InvertWave();
        }
    }
}