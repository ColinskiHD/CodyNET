using System.Diagnostics.Contracts;// TODO: do i need this import?
using CodyNET.Core.Cody;
using CodyNET.Core.Interfaces;

public class SoundInterfaceDevice : IAudioDevice
{
    // Store sound memory locally in the device, for easy access
    private readonly byte[] _soundMemory = new byte[0x100];// TODO: check if length is correct
    // === IMemoryMappedDevice ===
    public ushort StartAddress => SID_BASE;
    public ushort EndAddress => 0xD41C;// TODO: Figure out if this number is correct
    public bool SupportsRead => true;
    public bool SupportsWrite => true;

    // === Address Constants ===
    public const ushort SID_BASE = 0xD400;
    public const ushort VOICE1_BASE = 0xD400;
    public const ushort VOICE2_BASE = 0xD407;
    public const ushort VOICE3_BASE = 0xD40E;
    /*
        // === Control Registers ===
        //Addresses taken from cody_audio.spin hardware implementation
        // ' $D400     Voice 1 frequency low byte
        private const ushort VOICE1_FREQ_LO = 0xD400;
        // ' $D401     Voice 1 frequency high byte
        private const ushort VOICE1_FREQ_HI = 0xD401;
        // ' $D402     Voice 1 pulse duty cycle low byte
        private const ushort VOICE1_PWM_DUTY_LO = 0xD402;
        // ' $D403     Voice 1 pulse duty cycle high byte (nibble)
        private const ushort VOICE1_PWM_DUTY_HI = 0xD403;
        // ' $D404     Voice 1 control register
        private const ushort VOICE1_CONTROL = 0xD404;
        // ' $D405     Voice 1 attack (high nibble) and decay (low nibble)
        private const ushort VOICE1_ATTACK_DECAY = 0xD405;
        // ' $D406     Voice 1 sustain (high nibble) and release (low nibble)
        private const ushort VOICE1_SUSTAIN_RELEASE = 0xD406;
        // ' $D407     Voice 2 frequency low byte
        private const ushort VOICE2_FREQ_LO = 0xD407;
        // ' $D408     Voice 2 frequency high byte
        private const ushort VOICE2_FREQ_HI = 0xD408;
        // ' $D409     Voice 2 pulse duty cycle low byte
        private const ushort VOICE2_PWM_DUTY_LO = 0xD409;
        // ' $D40A     Voice 2 pulse duty cycle high byte (nibble)
        private const ushort VOICE2_PWM_DUTY_HI = 0xD40A;
        // ' $D40B     Voice 2 control register
        private const ushort VOICE2_CONTROL = 0xD40B;
        // ' $D40C     Voice 2 attack (high nibble) and decay (low nibble)
        private const ushort VOICE2_ATTACK_DECAY = 0xD40C;
        // ' $D40D     Voice 2 sustain (high nibble) and release (low nibble)
        private const ushort VOICE2_SUSTAIN_RELEASE = 0xD40D;
        // ' $D40E     Voice 3 frequency low byte
        private const ushort VOICE3_FREQ_LO = 0xD40E;
        // ' $D40F     Voice 3 frequency high byte
        private const ushort VOICE3_FREQ_HI = 0xD40F;
        // ' $D410     Voice 3 pulse duty cycle low byte
        private const ushort VOICE3_PWM_DUTY_LO = 0xD410;
        // ' $D411     Voice 3 pulse duty cycle high byte (nibble)
        private const ushort VOICE3_PWM_DUTY_HI = 0xD411;
        // ' $D412     Voice 3 control register
        private const ushort VOICE3_CONTROL = 0xD412;
        // ' $D413     Voice 3 attack (high nibble) and decay (low nibble)
        private const ushort VOICE3_ATTACK_DECAY = 0xD413;
        // ' $D414     Voice 3 sustain (high nibble) and release (low nibble)
        private const ushort VOICE3_SUSTAIN_RELEASE = 0xD414;
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
    public Interrupt Update(long cycle)
    {
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
}