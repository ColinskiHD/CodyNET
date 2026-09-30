using System.Diagnostics.Contracts;
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

    // === Control Registers ===
    //Addresses taken from the CodyComputerBook
    // ' $D400     Voice 1 frequency low byte
    private const ushort VOICE1_FREQ_LO = 0xD400;
    // ' $D401     Voice 1 frequency high byte
    // ' $D402     Voice 1 pulse duty cycle low byte
    // ' $D403     Voice 1 pulse duty cycle high byte (nibble)
    // ' $D404     Voice 1 control register
    // ' $D405     Voice 1 attack (high nibble) and decay (low nibble)
    // ' $D406     Voice 1 sustain (high nibble) and release (low nibble)
    // ' $D407     Voice 2 frequency low byte
    // ' $D408     Voice 2 frequency high byte
    // ' $D409     Voice 2 pulse duty cycle low byte
    // ' $D40A     Voice 2 pulse duty cycle high byte (nibble)
    // ' $D40B     Voice 2 control register
    // ' $D40C     Voice 2 attack (high nibble) and decay (low nibble)
    // ' $D40D     Voice 2 sustain (high nibble) and release (low nibble)
    // ' $D40E     Voice 3 frequency low byte
    // ' $D40F     Voice 3 frequency high byte
    // ' $D410     Voice 3 pulse duty cycle low byte
    // ' $D411     Voice 3 pulse duty cycle high byte (nibble)
    // ' $D412     Voice 3 control register
    // ' $D413     Voice 3 attack (high nibble) and decay (low nibble)
    // ' $D414     Voice 3 sustain (high nibble) and release (low nibble)
    // ' $D415     Reserved (filters?)
    // ' $D416     Reserved (filters?)
    // ' $D417     Reserved (filters?)
    // ' $D418     Volume control (filters?)
    // ' $D419     Reserved
    // ' $D41A     Reserved
    // ' $D41B     Voice 3 oscillator read
    // ' $D41C     Voice 3 envelope read
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
            throw new ArgumentOutOfRangeException(nameof(address), $"Address {address:X4} is out of range for VID device.");
        _soundMemory[address - StartAddress] = value;
        //Dirty = true;
    }
}